using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class CallStacks : MovementManagement, IWiredEffect, IWiredTrigger
    {
        private Room room;
        private WiredHandler handler;
        private RoomItem item;
        private List<RoomItem> items;

        public CallStacks(List<RoomItem> items, Room room, WiredHandler handler, RoomItem itemID)
        {
            this.items = items;
            this.room = room;
            this.handler = handler;
            this.item = itemID;
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public void Handle(RoomUser user, Team team, RoomItem iItem)
        {
            //InteractorGenericSwitch.DoAnimation(item);
            Queue<RoomItem> toRemove = new Queue<RoomItem>();

            foreach (var _item in items)
            {
                if (room.GetRoomItemHandler().GetItem(_item.Id) == null)
                {
                    toRemove.Enqueue(_item);
                    continue;
                }

                /*if(_item.Coordinate == item.Coordinate)
                {
                    continue;
                }*/

                handler.RequestStackHandleEffects(_item.Coordinate, null, user, Team.none);
            }

            while (toRemove.Count > 0)
            {
                RoomItem itemToRemove = toRemove.Dequeue();
                if (items.Contains(itemToRemove))
                    items.Remove(itemToRemove);
            }
        }

        public void Dispose()
        {
            room = null;
            handler = null;
            if (items != null)
                items.Clear();
            items = null;
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
    }
}
