using System;
using System.Drawing;

namespace Butterfly.HabboHotel.Pathfinding
{
    internal struct ThreeDCoord : IEquatable<ThreeDCoord>
    {
        internal int X;
        internal int Y;
        internal int Z;

        internal ThreeDCoord(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public bool Equals(ThreeDCoord comparedCoord)
        {
            return (X == comparedCoord.X && Y == comparedCoord.Y && Z == comparedCoord.Z);
        }

        public bool Equals(Point comparedCoord)
        {
            return (X == comparedCoord.X && Y == comparedCoord.Y);
        }

        public static bool operator ==(ThreeDCoord a, ThreeDCoord b)
        {
            return (a.X == b.X && a.Y == b.Y && a.Z == b.Z);
        }

        public static bool operator !=(ThreeDCoord a, ThreeDCoord b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return X ^ Y ^ Z;
        }

        internal bool Any(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else
                return base.GetHashCode().Equals(obj.GetHashCode());
        }

        public Point SquareBehind(int Rot)
        {
            if (Rot == 0)
                return new Point(X, Y + 1);

            else if (Rot == 2)
                return new Point(X - 1, Y);

            else if (Rot == 4)
                return new Point (X, Y - 1);

            else if (Rot == 6)
                return new Point(X + 1, Y);

            return new Point(X, Y);
        }

        public Point LeftSide(int Rot)
        {
            if (Rot == 0)
                return new Point(X - 1, Y);
            else if (Rot == 2)
                return new Point(X, Y - 1);
            else if (Rot == 4)
                return new Point(X + 1, Y);
            else if (Rot == 6)
                return new Point(X, Y + 1);

            return new Point(X, Y);
        }

        public Point RightSide(int Rot)
        {
            if (Rot == 0)
                return new Point(X + 1, Y);
            else if (Rot == 2)
                return new Point(X, Y + 1);
            else if (Rot == 4)
                return new Point(X - 1, Y);
            else if (Rot == 6)
                return new Point(X, Y - 1);

            return new Point(X, Y);
        }

        public Point UpLeftSide(int Rot)
        {
            if (Rot == 0)
                return new Point(X - 1, Y + 1);
            else if (Rot == 2)
                return new Point(X - 1, Y + 1);
            else if (Rot == 4)
                return new Point(X - 1, Y - 1);
            else if (Rot == 6)
                return new Point(X + 1, Y + 1);

            return new Point(X, Y);
        }

        public Point UpRightSide(int Rot)
        {
            if (Rot == 0)
                return new Point(X + 1, Y + 1);
            else if (Rot == 2)
                return new Point(X - 1, Y - 1);
            else if (Rot == 4)
                return new Point(X + 1, Y - 1);
            else if (Rot == 6)
                return new Point(X + 1, Y - 1);

            return new Point(X, Y);
        }

        // Two tiles

        public Point SquareBehindTwoTiles(int Rot)
        {
            if (Rot == 0)
                return new Point(X, Y + 2);

            else if (Rot == 2)
                return new Point(X - 2, Y);

            else if (Rot == 4)
                return new Point(X, Y - 2);

            else if (Rot == 6)
                return new Point(X + 2, Y);

            return new Point(X, Y);
        }

        public Point LeftSideTwoTiles(int Rot)
        {
            if (Rot == 0)
                return new Point(X - 2, Y);
            else if (Rot == 2)
                return new Point(X, Y - 2);
            else if (Rot == 4)
                return new Point(X + 2, Y);
            else if (Rot == 6)
                return new Point(X, Y + 2);

            return new Point(X, Y);
        }

        public Point RightSideTwoTiles(int Rot)
        {
            if (Rot == 0)
                return new Point(X + 2, Y);
            else if (Rot == 2)
                return new Point(X, Y + 2);
            else if (Rot == 4)
                return new Point(X - 2, Y);
            else if (Rot == 6)
                return new Point(X, Y - 2);

            return new Point(X, Y);
        }

        public Point UpLeftSideTwoTiles(int Rot)
        {
            if (Rot == 0)
                return new Point(X - 2, Y + 2);
            else if (Rot == 2)
                return new Point(X - 2, Y + 2);
            else if (Rot == 4)
                return new Point(X - 2, Y - 2);
            else if (Rot == 6)
                return new Point(X + 2, Y + 2);

            return new Point(X, Y);
        }

        public Point UpRightSideTwoTiles(int Rot)
        {
            if (Rot == 0)
                return new Point(X + 2, Y + 2);
            else if (Rot == 2)
                return new Point(X - 2, Y - 2);
            else if (Rot == 4)
                return new Point(X + 2, Y - 2);
            else if (Rot == 6)
                return new Point(X + 2, Y - 2);

            return new Point(X, Y);
        }
    }
}
