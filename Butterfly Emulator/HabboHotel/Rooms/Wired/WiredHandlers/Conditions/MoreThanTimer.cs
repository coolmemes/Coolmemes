using System;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Database_Manager.Database;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Items;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions
{
    class MoreThanTimer : IWiredCondition
    {
        private int timeout;
        private Room room;
        private RoomItem item;
        private bool isDisposed = false;

        public MoreThanTimer(int timeout, Room room, RoomItem item)
        {
            this.timeout = timeout;
            this.room = room;
            this.isDisposed = false;
            this.item = item;
        }

        public int Time
        {
            get
            {
                return timeout;
            }
        }

        public bool AllowsExecution(RoomUser user)
        {
            if (room == null || room.lastTimerReset == null)
                return false;

            return ((DateTime.Now - room.lastTimerReset).TotalSeconds > (timeout / 2));
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = timeout.ToString() + ";;false";
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
            room = null;
        }

        public bool Disposed()
        {
            return isDisposed;
        }
    }
}
