using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Astar.Algorithm;
using enclosuretest.Algorithm;

namespace enclosuretest
{
    public class GameField : IPathNode
    {

        private byte[,] currentField;
        private readonly AStarSolver<GameField> astarSolver;
        private readonly Queue<GametileUpdate> newEntries = new Queue<GametileUpdate>();
        private readonly bool diagonal;
        private GametileUpdate currentlyChecking;

        public GameField(byte[,] theArray, bool diagonalAllowed)
        {
            currentField = theArray;
            diagonal = diagonalAllowed;
            astarSolver = new AStarSolver<GameField>(diagonalAllowed, AStarHeuristicType.EXPERIMENTAL_SEARCH, this, theArray.GetUpperBound(1) + 1, theArray.GetUpperBound(0) + 1);
        }

        public void updateLocation(int x, int y, byte value)
        {
            newEntries.Enqueue(new GametileUpdate(x,y,value));
        }

        public List<PointField> doUpdate(bool oneloop = false)
        {
            var returnList = new List<PointField>();
            while (newEntries.Count > 0)
            {
                currentlyChecking = newEntries.Dequeue();

                var pointList = getConnectedItems(currentlyChecking);
                if (pointList.Count > 1)
                {
                    var RouteList = handleListOfConnectedPoints(pointList);

                    returnList.AddRange(from nodeList in RouteList where nodeList.Count >= 4 select findClosed(nodeList) into field where field != null select field);
                }
                currentField[currentlyChecking.y, currentlyChecking.x] = currentlyChecking.value;
            }
            return returnList;
        }

        private PointField findClosed(LinkedList<AStarSolver<GameField>.PathNode> nodeList)
        {
            var returnList = new PointField(currentlyChecking.value);

            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;

            foreach (var node in nodeList)
            {
                if (node.X < minX)
                    minX = node.X;

                if(node.X > maxX)
                    maxX = node.X;

                if (node.Y < minY)
                    minY = node.Y;

                if (node.Y > maxY)
                    maxY = node.Y;

            }

            var middleX = (int)Math.Ceiling(((maxX - minX) / 2f)) + minX;
            var middleY = (int)Math.Ceiling(((maxY - minY) / 2f)) + minY;
            //Console.WriteLine("Middle: x:[{0}]  y:[{1}]", middleX, middleY);

            Point current;
            var toFill = new List<Point>();
            var checkedItems = new List<Point> {new Point(currentlyChecking.x, currentlyChecking.y)};
            Point toAdd;
            toFill.Add(new Point(middleX,middleY));
            int x;
            int y;
            while(toFill.Count > 0)
            {
                current = toFill[0];
                x = current.X;
                y = current.Y;
               
                if (x < minX)
                    return null;//OOB
                if (x > maxX)
                    return null;//OOB
                if (y < minY)
                    return null;//OOB
                if (y > maxY)
                    return null; //OOB
                
                if (this[y - 1, x] && currentField[y - 1, x] == 0)
                {
                    toAdd = new Point(x, y - 1);
                    if (!toFill.Contains(toAdd) && !checkedItems.Contains(toAdd))
                        toFill.Add(toAdd);
                }
                if (this[y + 1, x] && currentField[y + 1, x] == 0 )
                {
                    toAdd = new Point(x, y + 1);
                    if (!toFill.Contains(toAdd) && !checkedItems.Contains(toAdd))
                        toFill.Add(toAdd);
                }
                if (this[y, x - 1] && currentField[y, x - 1] == 0)
                {
                    toAdd = new Point(x - 1, y);
                    if (!toFill.Contains(toAdd) && !checkedItems.Contains(toAdd))
                        toFill.Add(toAdd);
                }
                if (this[y, x + 1] && currentField[y, x + 1] == 0)
                {
                    toAdd = new Point(x + 1, y);
                    if (!toFill.Contains(toAdd) && !checkedItems.Contains(toAdd))
                        toFill.Add(toAdd);
                }
                if (getValue(current) == 0)
                    returnList.add(current);
                checkedItems.Add(current);
                toFill.RemoveAt(0);
                

            }

            return returnList;
        }

        private List<LinkedList<AStarSolver<GameField>.PathNode>> handleListOfConnectedPoints(List<Point> pointList)
        {
            var returnList = new List<LinkedList<AStarSolver<GameField>.PathNode>>();
            var amount = 0;
            foreach (var begin in pointList)
            {
                amount++;
                if (amount == pointList.Count / 2 + 1)
                    return returnList;
                returnList.AddRange(pointList.Where(end => begin != end).Select(end => astarSolver.Search(end, begin)).Where(list => list != null));
            }
            return returnList;
        }

        private List<Point> getConnectedItems(GametileUpdate update)
        {
            var ConnectedItems = new List<Point>();
            var x = update.x;
            var y = update.y;
            if (diagonal)
            {
                if (this[y - 1, x - 1] && currentField[y - 1, x - 1] == update.value)
                {
                    ConnectedItems.Add(new Point(x - 1, y - 1));
                }
                if (this[y - 1, x + 1] && currentField[y - 1, x + 1] == update.value)
                {
                    ConnectedItems.Add(new Point(x + 1, y - 1));
                }
                if (this[y + 1, x - 1] && currentField[y + 1, x - 1] == update.value)
                {
                    ConnectedItems.Add(new Point(x - 1, y + 1));
                }
                if (this[y + 1, x + 1] && currentField[y + 1, x + 1] == update.value)
                {
                    ConnectedItems.Add(new Point(x + 1, y + 1));
                }
            }


            if (this[y - 1, x] && currentField[y - 1, x] == update.value)
            {
                ConnectedItems.Add(new Point(x, y - 1));
            }
            if (this[y + 1, x] && currentField[y + 1, x] == update.value)
            {
                ConnectedItems.Add(new Point(x, y + 1));
            }
            if (this[y, x - 1] && currentField[y, x - 1] == update.value)
            {
                ConnectedItems.Add(new Point(x - 1, y));
            }
            if (this[y, x + 1] && currentField[y, x + 1] == update.value)
            {
                ConnectedItems.Add(new Point(x + 1, y));
            }

            return ConnectedItems;
        }

        public bool this[int y, int x]
        {
            get
            {
                if (y < 0 || x < 0)
                    return false;
                else if (y > currentField.GetUpperBound(0) || x > currentField.GetUpperBound(1))
                    return false;
                return true;
            }

        }

        private void setValue(int x, int y, byte value)
        {
            if (this[y, x])
            {
                currentField[y, x] = value;
            }
        }

        public byte getValue(int x, int y)
        {
            if (this[y, x])
            {
                return currentField[y, x];
            }
            return 0;
        }

        public byte getValue(Point p)
        {
            if (this[p.Y, p.X])
            {
                return currentField[p.Y, p.X];
            }
            return 0;
        }

        public bool IsBlocked(int x, int y, bool lastTile)
        {
            if (currentlyChecking.x == x && currentlyChecking.y == y)
                return true;
            return getValue(x, y) != currentlyChecking.value;
        }

        public void destroy()
        {
            currentField = null;
        }
    }
}