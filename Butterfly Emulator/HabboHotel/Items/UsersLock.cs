using Butterfly.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Items
{
    class UsersLock
    {
        internal RoomItem Item;
        internal RoomUser roomUserOne; // owner Item
        internal RoomUser roomUserTwo; // the Friend
        internal Int32 roomUserOneResponse;
        internal Int32 roomUserTwoResponse;

        internal UsersLock(RoomItem item)
        {
            this.Item = item;
            this.ClearLock();
        }

        internal void ClearLock()
        {
            this.roomUserOne = null;
            this.roomUserTwo = null;
            this.roomUserOneResponse = 0;
            this.roomUserTwoResponse = 0;
        }
    }
}
