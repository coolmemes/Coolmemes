using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions
{
    class FurniHasFurni : IWiredCondition
    {
        private RoomItem item;
        private List<RoomItem> items;
        private int onlyOneFurniOn;
        private bool isDisposed;

        public FurniHasFurni(RoomItem item, List<RoomItem> items, int onlyonefurnion)
        {
            this.item = item;
            this.items = items;
            this.onlyOneFurniOn = onlyonefurnion;
            this.isDisposed = false;
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public int OnlyOneFurniOn
        {
            get
            {
                return onlyOneFurniOn;
            }
        }

        public bool AllowsExecution(RoomUser user)
        {
            if (items.Count <= 0)
                return true;

            if(OnlyOneFurniOn == 0)
                return items.Any(item => item.GetRoom().GetGameMap().GetRoomItemForMinZ(item.GetX, item.GetY, item.TotalHeight).Count > 0);
            else
                return items.All(item => item.GetRoom().GetGameMap().GetRoomItemForMinZ(item.GetX, item.GetY, item.TotalHeight).Count > 0);
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = OnlyOneFurniOn.ToString() + ";;false";
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