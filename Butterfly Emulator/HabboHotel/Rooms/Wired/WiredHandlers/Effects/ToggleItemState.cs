using System.Collections;
using System.Collections.Generic;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;
using System;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class ToggleItemState : IWiredTrigger, IWiredCycleable, IWiredEffect
    {
        private readonly RoomItem item;
        private WiredHandler handler;

        private readonly List<RoomItem> items;
        private int delay;
        private int cycles;
        private readonly Queue delayedTriggeringUsers;

        private bool disposed;

        public ToggleItemState(WiredHandler handler, List<RoomItem> items, int delay, RoomItem Item)
        {
            this.item = Item;
            this.handler = handler;
            this.items = items;
            this.delay = delay;
            this.cycles = 0;
            this.delayedTriggeringUsers = new Queue();
            this.disposed = false;
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
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
            if (disposed)
                return false;

            cycles++;
            if (cycles > delay)
            {
                if (delayedTriggeringUsers.Count > 0)
                {
                    lock (delayedTriggeringUsers.SyncRoot)
                    {
                        while (delayedTriggeringUsers.Count > 0)
                        {
                            var user = (RoomUser)delayedTriggeringUsers.Dequeue();
                            ToggleItems(user);
                        }
                    }
                }
                return false;
            }

            return true;
        }

        public void Handle(RoomUser user, Team team, RoomItem i)
        {
            if (disposed)
                return;

            cycles = 0;
            if (delay == 0 && user != null)
            {
                ToggleItems(user);
            }
            else
            {
                lock (delayedTriggeringUsers.SyncRoot)
                {
                    delayedTriggeringUsers.Enqueue(user);
                }
                handler.RequestCycle(this);
            }
        }

        private bool ToggleItems(RoomUser user)
        {
            if (disposed)
                return false;

            //InteractorGenericSwitch.DoAnimation(item);
            var itemTriggered = false;

            foreach (var i in items)
            {
                if (i == null)
                    continue;

                if (user != null && user.GetClient() != null)
                    i.Interactor.OnTrigger(user.GetClient(), i, 3, true);
                else
                    i.Interactor.OnTrigger(null, i, 3, true);
                itemTriggered = true;
            }
            return itemTriggered;
        }

        public void Dispose()
        {
            disposed = true;
            handler = null;
            if (items != null)
                items.Clear();
            delayedTriggeringUsers.Clear();
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = delay.ToString() + ";;false";
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

        public void ResetTimer()
        {

        }
    }
}
