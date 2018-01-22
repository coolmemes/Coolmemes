using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otanix.HabboHotel.Rooms.Wired
{
    class OriginalItemLocation
    {
        internal uint itemID { get; private set; }
        internal int X { get; private set; }
        internal int Y { get; private set; }
        internal int Rot { get; private set; }
        internal double Height { get; private set; }
        internal string ExtraData { get; private set; }

        public OriginalItemLocation(uint itemID, int x, int y, double height, int rot, string extradata)
        {
            this.itemID = itemID;
            this.X = x;
            this.Y = y;
            this.Height = height;
            this.Rot = rot;
            if (string.IsNullOrEmpty(extradata))
                this.ExtraData = "0";
            else 
                this.ExtraData = extradata;
        }
    }
}
