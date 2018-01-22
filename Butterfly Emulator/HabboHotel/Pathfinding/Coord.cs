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
    }
}
