using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions
{
    class UserCountIn : IWiredCondition
    {
        private UInt32 minUsers;
        private UInt32 maxUsers;
        private RoomItem item;
        private bool isDisposed;

        public UserCountIn(UInt32 minUsers, UInt32 maxUsers, RoomItem item)
        {
            this.minUsers = minUsers;
            this.maxUsers = maxUsers;
            this.isDisposed = false;
            this.item = item;
        }

        public UInt32 MinUsers
        {
            get
            {
                return minUsers;
            }
        }

        public UInt32 MaxUsers
        {
            get
            {
                return maxUsers;
            }
        }

        public bool AllowsExecution(RoomUser user)
        {
            try
            {
                if (user == null)
                    return false;

                UInt32 usersNow = (UInt32)OtanixEnvironment.GetGame().GetRoomManager().GetRoom(user.RoomId).UserCount;

                if (usersNow >= minUsers && usersNow <= maxUsers)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }


        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = minUsers.ToString() + ";" + maxUsers.ToString() + ";false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }

        public void Dispose()
        {
            isDisposed = true;
            item = null;
        }

        public bool Disposed()
        {
            return isDisposed;
        }
    }
}
