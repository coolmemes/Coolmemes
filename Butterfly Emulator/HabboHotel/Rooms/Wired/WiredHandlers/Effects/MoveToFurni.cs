using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class MoveToFurni : IWiredTrigger, IWiredCycleable, IWiredEffect
    {
        private WiredHandler handler;
        private List<RoomItem> items;
        private int length;
        private int direction;
        private int delay;
        private int cycles;
        private readonly Random rnd;
        private RoomItem itemID;
        private bool disposed;

        public MoveToFurni(WiredHandler handler, List<RoomItem> items, int length, int direction, int delay, RoomItem itemID)
        {
            this.handler = handler;
            this.items = items;
            this.length = length;
            this.direction = direction;
            this.delay = delay;
            this.itemID = itemID;
            cycles = 0;
            rnd = new Random();
            disposed = false;
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public int Length
        {
            get
            {
                return length;
            }
        }

        public int Direction
        {
            get
            {
                return direction;
            }
        }

        public int Time
        {
            get
            {
                return delay;
            }
        }

        public bool OnCycle()
        {
            cycles++;
            if (cycles > delay)
            {
                MoveToItem();
            }

            return true;
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            try
            {
                cycles = 0;
                if (delay == 0 && user != null)
                {
                    MoveToItem();
                }
                else
                {
                    handler.RequestCycle(this);
                }
            }
            catch { }
        }

        private void MoveToItem()
        {
            if (items.Count < 2)
                return;

            #region Choosing Items
            RoomItem mainItem = items[rnd.Next(0, items.Count - 1)];

            List<RoomItem> backItems = new List<RoomItem>();
            backItems.AddRange(items);
            backItems.Remove(mainItem);

            RoomItem selectedItem = backItems[rnd.Next(0, backItems.Count - 1)];

            backItems.Clear();
            backItems = null;
            #endregion

            if (mainItem == null || selectedItem == null)
                return;

            Point nPoint = new Point(selectedItem.GetX, selectedItem.GetY);

            switch (direction)
            {
                case 0:
                    {
                        //up
                        nPoint.Y = nPoint.Y - length;
                        break;
                    }

                case 2:
                    {
                        //right
                        nPoint.X = nPoint.X + length;
                        break;
                    }

                case 4:
                    {
                        //down
                        nPoint.Y = nPoint.Y + length;
                        break;
                    }

                case 6:
                    {
                        //left
                        nPoint.X = nPoint.X - length;
                        break;
                    }
            }

            mainItem.GetRoom().GetRoomItemHandler().SetFloorItem(null, mainItem, nPoint.X, nPoint.Y, mainItem.Rot, false, false, true, true);
        }

        public void Dispose()
        {
            disposed = true;
            handler = null;
            if (items != null)
                items.Clear();
            items = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = length + ";" + direction + ";" + delay;
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

            wiredInserts.AddQuery("('" + itemID.Id + "', @data" + itemID.Id + ", @to_item" + itemID.Id + ", @original_location" + itemID.Id + ")");
            wiredInserts.AddParameter("data" + itemID.Id, wired_data);
            wiredInserts.AddParameter("to_item" + itemID.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID.Id, wired_original_location);
        }

        public bool Disposed()
        {
            return disposed;
        }

        public void ResetTimer()
        {

        }
    }
}
