using System.Collections.Generic;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using System.Collections;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;
using System;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class StateChanged : IWiredTrigger, IWiredCycleable
    {
        private WiredHandler handler;
        private List<RoomItem> items;
        private readonly RoomItem item;
        private readonly OnItemTrigger delegateFunction;
        private readonly Queue triggeringQueue;
        //private int delay;
        private int cycleCount;
        private bool disposed;

        public StateChanged(WiredHandler handler, RoomItem item, List<RoomItem> items)
        {
            this.handler = handler;
            this.items = items;
            this.item = item;
            //this.delay = delay;
            delegateFunction = itemTriggered;
            cycleCount = 0;
            triggeringQueue = new Queue();

            foreach (var _item in items)
            {
                _item.itemTriggerEventHandler += delegateFunction;
            }
            disposed = true;
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public bool OnCycle()
        {
            if (cycleCount > 0)
            {
                if (triggeringQueue.Count > 0)
                {
                    lock (triggeringQueue.SyncRoot)
                    {
                        while (triggeringQueue.Count > 0)
                        {
                            var e = (ItemTriggeredArgs)triggeringQueue.Dequeue();
                            onTrigger(e);
                        }
                    }
                }
                return false;
            }
            else
            {
                cycleCount++;
                return true;
            }
        }

        private void itemTriggered(object sender, ItemTriggeredArgs e)
        {
            /*if (delay > 0)
            {
                triggeringQueue.Enqueue(e);
                handler.RequestCycle(this);
            }
            else
            {*/
                onTrigger(e);
            //}
        }

        private void onTrigger(ItemTriggeredArgs e)
        {
            handler.RequestStackHandle(item, e.TriggeringItem, e.TriggeringUser, Team.none);
            //InteractorGenericSwitch.DoAnimation(item);
        }

        public void Dispose()
        {
            disposed = true;
            handler = null;

            if (items != null)
            {
                foreach (var _item in items)
                {
                    _item.itemTriggerEventHandler -= delegateFunction;
                }

                items.Clear();
            }
            items = null;
        }

        public void ResetTimer()
        {
            cycleCount = 0;
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

        public bool Disposed()
        {
            return disposed;
        }
    }
}
