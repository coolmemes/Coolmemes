using System.Drawing;

namespace WiredSolver
{
    public class WireTransfer
    {
        public Point location { get; private set; }
        private WireCurrentTransfer currentTransfer;
        public CurrentType Current { get; private set; }

        public WireTransfer(int x, int y, WireCurrentTransfer currentTransfer)
        {
            location = new Point(x, y);
            this.currentTransfer = currentTransfer;
            Current = CurrentType.OFF;
        }

        public WireCurrentTransfer getTransfer()
        {
            return currentTransfer;
        }

        internal bool transfersCurrentTo(WireCurrentTransfer current)
        {
            return ((current & currentTransfer) == current);
        }

        public bool isPowered()
        {
            return Current == CurrentType.ON || Current == CurrentType.SENDER;
        }

        internal bool acceptsCurrentFrom(WireCurrentTransfer wireCurrentTransfer)
        {
            if (currentTransfer == WireCurrentTransfer.NONE)
                return true;

            else if (wireCurrentTransfer == WireCurrentTransfer.DOWN && transfersCurrentTo(WireCurrentTransfer.DOWN))
                return true;
            else if (wireCurrentTransfer == WireCurrentTransfer.UP && transfersCurrentTo(WireCurrentTransfer.UP))
                return true;
            else if (wireCurrentTransfer == WireCurrentTransfer.LEFT && transfersCurrentTo(WireCurrentTransfer.LEFT))
                return true;
            else if (wireCurrentTransfer == WireCurrentTransfer.RIGHT && transfersCurrentTo(WireCurrentTransfer.RIGHT))
                return true;
            else 
                return false;
        }

        internal void setCurrentTransfer(WireCurrentTransfer wireCurrentTransfer)
        {
            currentTransfer = wireCurrentTransfer;
        }

        internal void setCurrent(CurrentType currentType)
        {
            Current = currentType;
        }

        internal bool setPower(bool powerIsOn)
        {
            if (powerIsOn && Current == CurrentType.OFF)
            {
                Current = CurrentType.ON;
                return true;
            }
            else if(!powerIsOn && Current == CurrentType.ON)
            {
                    Current = CurrentType.OFF;
                    return true;
            }
            return false;
        }
        public override string ToString()
        {
            return string.Format("Location x:{0} y:{1}", location.X, location.Y);
        }
    }
}
