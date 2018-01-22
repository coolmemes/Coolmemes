using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms
{
    class RoomVisits
    {
        internal UInt32 RoomId;
        internal String RoomName;
        internal Boolean IsPublic;
        internal Int32 Hour;
        internal Int32 Minute;

        internal RoomVisits(UInt32 RoomId, String RoomName, Boolean IsPublic, Int32 Hour, Int32 Minute)
        {
            this.RoomId = RoomId;
            this.RoomName = RoomName;
            this.IsPublic = IsPublic;
            this.Hour = Hour;
            this.Minute = Minute; 
        }
    }
}
