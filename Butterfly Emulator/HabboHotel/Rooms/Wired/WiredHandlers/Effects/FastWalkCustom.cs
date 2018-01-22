using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class FastWalkCustom : IWiredTrigger, IWiredEffect, IWiredCycleable
    {
        private int cyclesRequired;
        private int cycleCount;
        private WiredHandler handler;
        private RoomItem item;
        private readonly Queue delayedUsers;
        private bool disposed;

        public FastWalkCustom(WiredHandler handler, RoomItem item, int cyclesRequired)
        {
            this.handler = handler;
            this.cyclesRequired = cyclesRequired;
            this.item = item;
            delayedUsers = new Queue();
            disposed = false;
        }

        public int IntTimer
        {
            get
            {
                return cyclesRequired;
            }
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            if (disposed)
                return;
            user.fastWalk = true;
            user.WhisperComposer("Fast walk ativado!");

            lock (delayedUsers.SyncRoot)
            {
                delayedUsers.Enqueue(user);
            }
            handler.RequestCycle(this);
        }

        public bool OnCycle()
        {
            cycleCount++;
            if (cycleCount > (cyclesRequired - 0.5))
            {
                if (delayedUsers.Count > 0)
                {
                    lock (delayedUsers.SyncRoot)
                    {
                        while (delayedUsers.Count > 0)
                        {
                            var user = (RoomUser)delayedUsers.Dequeue();
                            normalWalk(user);
                            cycleCount = 0;
                            //InteractorGenericSwitch.DoAnimation(itemID);
                        }
                    }
                }
                return false;
            }
            return true;
        }

        private void normalWalk(RoomUser mdspqp)
        {
            mdspqp.fastWalk = false;
        }

        public void Dispose()
        {
            disposed = true;
            handler = null;
            item = null;
        }

        public void ResetTimer()
        {
            cycleCount = 0;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = cyclesRequired.ToString() + ";;false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }

        public void LoadFromDatabase(IQueryAdapter dbClient, Room insideRoom)
        {
            dbClient.setQuery("SELECT trigger_data FROM wired_data WHERE trigger_id = @id ");
            dbClient.addParameter("id", (int)item.Id);
            cyclesRequired = dbClient.getInteger();
        }

        public bool Disposed()
        {
            return disposed;
        }
    }
}
