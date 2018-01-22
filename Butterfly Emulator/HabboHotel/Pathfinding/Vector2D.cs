using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Pathfinding
{
    class Vector2D
    {
        private int x;
        private int y;

        public int X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }

        public int Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }

        public Vector2D() { }

        public Vector2D(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int GetDistanceSquared(Vector2D Point)
        {
            int dx = this.X - Point.X;
            int dy = this.Y - Point.Y;
            return (dx * dx) + (dy * dy);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2D)
            {
                Vector2D v2d = (Vector2D)obj;
                return v2d.X == this.X && v2d.Y == this.Y;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (X + " " + Y).GetHashCode();
        }

        public override string ToString()
        {
            return X + ", " + Y;
        }

        public static Vector2D operator +(Vector2D One, Vector2D Two)
        {
            return new Vector2D(One.X + Two.X, One.Y + Two.Y);
        }

        public static Vector2D operator -(Vector2D One, Vector2D Two)
        {
            return new Vector2D(One.X - Two.X, One.Y - Two.Y);
        }

        public static Vector2D Zero = new Vector2D(0, 0);
    }
}
