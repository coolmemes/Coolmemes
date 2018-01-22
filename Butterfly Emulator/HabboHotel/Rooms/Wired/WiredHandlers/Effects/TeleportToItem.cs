using System;
using System.Collections.Generic;
using System.Linq;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using System.Collections;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;


namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class TeleportToItem : IWiredTrigger, IWiredCycleable, IWiredEffect
    {
        private Gamemap gamemap;
        private WiredHandler handler;

        private List<RoomItem> items;
        private int delay;
        private int cycles;
        private readonly Queue delayedUsers;
        private readonly Random rnd;
        private RoomItem itemID;
        private bool disposed;

        public TeleportToItem(Gamemap gamemap, WiredHandler handler, List<RoomItem> items, int delay, RoomItem itemID)
        {
            this.gamemap = gamemap;
            this.handler = handler;
            this.items = items;
            this.delay = delay+1;
            this.itemID = itemID;
            cycles = 0;
            delayedUsers = new Queue();
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

        public bool OnCycle()
        {
            cycles++;
            if (cycles > delay)
            {
                if (delayedUsers.Count > 0)
                {
                    lock (delayedUsers.SyncRoot)
                    {
                        while (delayedUsers.Count > 0)
                        {
                            var user = (RoomUser)delayedUsers.Dequeue();
                            TeleportUser(user);
                            //InteractorGenericSwitch.DoAnimation(itemID);
                        }
                    }
                }
                return false;
            }

            return true;
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            try
            {
                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(4);
                cycles = 0;
                if (delay == 0 && user != null)
                {
                    TeleportUser(user);
                }
                else
                {
                    lock (delayedUsers.SyncRoot)
                    {
                        delayedUsers.Enqueue(user);
                    }
                    handler.RequestCycle(this);
                }
            }
            catch { }
        }

        private void TeleportUser(RoomUser user)
        {

            if (items.Count == 0 || user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null
                || user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() == null || disposed
                || user.GetClient().GetHabbo().IsTeleporting) 
                return;

            if (items.Count > 1)
            {
                var test = items.Count;
                RoomItem item = items[rnd.Next(0, items.Count - 1)];
                if (item == null || item.GetRoom().GetRoomItemHandler().GetItem(item.Id) == null)
                {
                    this.items.Remove(item);

                   user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().BackupEffect);
                   user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().BackupEffect = 0;

                    return;
                }

                gamemap.TeleportToItem(user, item);
            }
            else if (items.Count == 1)
            {
                RoomItem item = items.First();
                if (item == null || item.GetRoom().GetRoomItemHandler().GetItem(item.Id) == null)
                {
                    this.items.Remove(item);

                   user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().BackupEffect);
                   user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().BackupEffect = 0;

                    return;
                }

                gamemap.TeleportToItem(user, item);
            }

           user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().BackupEffect);
           user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().BackupEffect = 0;
        }

        public void Dispose()
        {
            disposed = true;
            gamemap = null;
            handler = null;
            if (items != null)
                items.Clear();
            items = null;
            if (delayedUsers != null)
                delayedUsers.Clear();
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
