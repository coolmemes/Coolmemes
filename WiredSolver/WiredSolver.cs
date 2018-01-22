using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WiredSolver
{
    public class WiredSolverInstance
    {
        private Dictionary<Point,WireTransfer> wiredField;

        public WiredSolverInstance()
        {
            wiredField = new Dictionary<Point,WireTransfer>();
        }

        public List<Point> AddOrUpdateWire(int x, int y, CurrentType newCurrent, WireCurrentTransfer currentTransfer)
        {
            var result = new LinkedList<WireTransfer>();
            var updatedWire = getWireTransfer(new Point(x, y));
            if(newCurrent == CurrentType.ON)
            {
            }
            else if (newCurrent == CurrentType.SENDER)
            {
                currentTransfer = WireCurrentTransfer.DOWN | WireCurrentTransfer.LEFT | WireCurrentTransfer.RIGHT | WireCurrentTransfer.UP;
                updatedWire.setCurrentTransfer(currentTransfer);
                updatedWire.setCurrent(CurrentType.SENDER);
                updatePowerGrid(updatedWire, ref result);
            }
            else if (newCurrent == CurrentType.OFF)
            {
                var neighbours = new WireTransfer[4];
                storeNeighbours(updatedWire, neighbours, true);
                updatedWire.setCurrent(CurrentType.OFF);
                updatedWire.setCurrentTransfer(currentTransfer);
                updatePowerGrid(updatedWire, ref result);
                for (var i = 0; i < 4; i++)
                {
                    if (neighbours[i] == null)
                        continue;
                    updatePowerGrid(neighbours[i], ref result);
                }
            }
            var finalResult = new List<Point>();
            foreach (var t in result)
            {
                if (!finalResult.Contains(t.location))
                    finalResult.Add(t.location);
            }
            return finalResult;
        }

        public List<Point> RemoveWire(int x, int y)
        {
            var result = new LinkedList<WireTransfer>();
            var updatedWire = getWireTransfer(new Point(x, y));
            result.AddFirst(updatedWire);

            if (updatedWire.isPowered() || updatedWire.getTransfer() != WireCurrentTransfer.NONE)
            {
                var neighbours = new WireTransfer[4];
                storeNeighbours(updatedWire, neighbours, true);
                updatedWire.setCurrent(CurrentType.OFF);
                updatedWire.setCurrentTransfer(WireCurrentTransfer.NONE);
                for (var i = 0; i < 4; i++)
                {
                    if (neighbours[i] == null)
                        continue;
                    updatePowerGrid(neighbours[i], ref result);
                }
            }
            var finalResult = new List<Point>();
            foreach (var t in result)
            {
                if (!finalResult.Contains(t.location))
                    finalResult.Add(t.location);
            }
            return finalResult;
        }

        private LinkedList<WireTransfer> updatePowerGrid(WireTransfer wireTransfer, ref LinkedList<WireTransfer> endResult)
        {
            var result = getLinkedWires(wireTransfer);

            var powerIsOn = result.Any(t => t.Current == CurrentType.SENDER);
            if (endResult == null)
            {
                endResult = new LinkedList<WireTransfer>();
            }
            var x = 0;
            var y = 0;
            WireTransfer toTest;
            var testLater = new LinkedList<WireTransfer>();
            foreach (var t in result)
            {
                if (!powerIsOn && t.getTransfer() == WireCurrentTransfer.NONE)
                {
                    testLater.AddFirst(t);
                    
                }
                else if (t.setPower(powerIsOn))
                {
                    endResult.AddFirst(t);
                }
            }
            foreach (var t in testLater)
            {
                x = t.location.X;
                y = t.location.Y;
                toTest = getWireTransfer(new Point(x, y - 1));
                if (toTest.isPowered() && toTest.transfersCurrentTo(WireCurrentTransfer.DOWN))
                {
                    if (t.setPower(true))
                    {
                        endResult.AddFirst(t);
                    }
                    continue;
                }

                toTest = getWireTransfer(new Point(x, y + 1));
                if (toTest.isPowered() && toTest.transfersCurrentTo(WireCurrentTransfer.UP))
                {
                    if (t.setPower(true))
                    {
                        endResult.AddFirst(t);
                    }
                    continue;
                }

                toTest = getWireTransfer(new Point(x - 1, y));
                if (toTest.isPowered() && toTest.transfersCurrentTo(WireCurrentTransfer.RIGHT))
                {
                    if (t.setPower(true))
                    {
                        endResult.AddFirst(t);
                    }
                    continue;
                }

                toTest = getWireTransfer(new Point(x + 1, y));
                if (toTest.isPowered() && toTest.transfersCurrentTo(WireCurrentTransfer.LEFT))
                {
                    if (t.setPower(true))
                    {
                        endResult.AddFirst(t);
                    }
                    continue;
                }
                if (t.setPower(false))
                {
                    endResult.AddFirst(t);
                }
            }

            endResult.AddFirst(wireTransfer);
            return endResult;
        }

        private LinkedList<WireTransfer> getLinkedWires(WireTransfer originalSender)
        {
            var blockedList = new List<Point>(30);

            var openList = new LinkedList<WireTransfer>();
            var endResult = new LinkedList<WireTransfer>();
            WireTransfer toUpdate;
            var neighbours = new WireTransfer[4];


            openList.AddFirst(originalSender);
            endResult.AddFirst(originalSender);
            blockedList.Add(originalSender.location);
            while (openList.Count > 0)
            {
                toUpdate = openList.First.Value;
                openList.RemoveFirst();

                storeNeighbours(toUpdate, neighbours);
                for (var i = 0; i < 4; i++)
                {
                    if (neighbours[i] == null)
                        continue;
                    if (blockedList.Contains(neighbours[i].location))
                        continue;
                         
                    blockedList.Add(neighbours[i].location);
                    openList.AddFirst(neighbours[i]);
                    endResult.AddFirst(neighbours[i]);
                }
            }

            return endResult;
        }

        private void storeNeighbours(WireTransfer toUpdate, WireTransfer[] inNeighbors, bool allneighbours = false)
        {
            var x = toUpdate.location.X;
            var y = toUpdate.location.Y;

            WireTransfer toTest;
            inNeighbors[0] = null;
            inNeighbors[1] = null;
            inNeighbors[2] = null;
            inNeighbors[3] = null;
            if (allneighbours || toUpdate.transfersCurrentTo(WireCurrentTransfer.UP))
            {
                toTest = getWireTransfer(new Point(x, y - 1));
                if (allneighbours || toTest.acceptsCurrentFrom(WireCurrentTransfer.DOWN))
                    inNeighbors[0] = toTest;
            }

            if (allneighbours || toUpdate.transfersCurrentTo(WireCurrentTransfer.LEFT))
            {
                toTest = getWireTransfer(new Point(x - 1, y));
                if (allneighbours || toTest.acceptsCurrentFrom(WireCurrentTransfer.RIGHT))
                    inNeighbors[1] = toTest;
            }

            if (allneighbours || toUpdate.transfersCurrentTo(WireCurrentTransfer.RIGHT))
            {
                toTest = getWireTransfer(new Point(x + 1, y));
                if (allneighbours || toTest.acceptsCurrentFrom(WireCurrentTransfer.LEFT))
                    inNeighbors[2] = toTest;
            }

            if (allneighbours || toUpdate.transfersCurrentTo(WireCurrentTransfer.DOWN))
            {
                toTest = getWireTransfer(new Point(x, y + 1));
                if (allneighbours || toTest.acceptsCurrentFrom(WireCurrentTransfer.UP))
                    inNeighbors[3] = toTest;
            }
        }

        public WireTransfer getWireTransfer(Point location)
        {
            WireTransfer item = null;
          
            if (!wiredField.TryGetValue(location, out item))
            {
                item = new WireTransfer(location.X, location.Y, WireCurrentTransfer.NONE);
                wiredField.Add(item.location, item);
            }
            return item;
        }

        public void Destroy()
        {
            if (wiredField != null)
                wiredField.Clear();
            wiredField = null;
        }
    }
}
