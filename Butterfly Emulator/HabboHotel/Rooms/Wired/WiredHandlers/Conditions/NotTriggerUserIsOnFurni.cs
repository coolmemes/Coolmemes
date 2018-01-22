using System;
using System.Collections.Generic;
using System.Linq;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Items;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions
{
    class NotTriggerUserIsOnFurni : IWiredCondition
    {
        private RoomItem item;
        private List<RoomItem> items;
        private bool isDisposed;

        public NotTriggerUserIsOnFurni(RoomItem item, List<RoomItem> items)
        {
            this.item = item;
            this.items = items;
            isDisposed = false;
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public bool AllowsExecution(RoomUser user)
        {
            return items.Any(item => item.GetRoom().GetGameMap().GetUsersOnItem(item).Count > 0 && item.GetRoom().GetGameMap().GetUsersOnItem(item).Any(usuario => usuario.GetUsername() != user.GetUsername()));
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = ";;false";
            string wired_to_item = "";
            if (items.Count > 0)
            {
                lock (items)
                {
                    foreach (var id in items)
                    {
                        wired_to_item += id.Id + ";";
                    }
                    if (wired_to_item.Length > 0)
                        wired_to_item = wired_to_item.Substring(0, wired_to_item.Length - 1);
                }
            }
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
            if (items != null)
                items.Clear();
            items = null;
        }

        public bool Disposed()
        {
            return isDisposed;
        }
    }
}
