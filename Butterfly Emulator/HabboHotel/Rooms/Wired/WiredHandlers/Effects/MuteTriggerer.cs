using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class MuteTriggerer : IWiredTrigger, IWiredCycleable, IWiredEffect
    {
        private WiredHandler handler;
        private string message;
        private uint mutetime;
        private uint delay;
        private int cycles;
        private RoomItem itemID;
        private bool disposed;
        private readonly Queue delayedUsers;

        public string Message
        {
            get
            {
                return message;
            }
        }

        public uint MuteTime
        {
            get
            {
                return mutetime;
            }
        }

        public uint Time
        {
            get
            {
                return delay;
            }
        }

        public MuteTriggerer(WiredHandler handler, string message, uint mutetime, uint delay, RoomItem itemID)
        {
            this.handler = handler;
            this.message = message;
            this.mutetime = mutetime;
            this.delay = delay;
            this.itemID = itemID;
            delayedUsers = new Queue();
            cycles = 0;
            disposed = false;
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
                            user.WhisperComposer(message);
                            itemID.GetRoom().AddMute(user.HabboId, (int)mutetime);
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
                cycles = 0;
                if (delay == 0 && user != null)
                {
                    user.WhisperComposer(message);
                    itemID.GetRoom().AddMute(user.HabboId, (int)mutetime);
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

        public void Dispose()
        {
            disposed = true;
            handler = null;
            if (delayedUsers != null)
                delayedUsers.Clear();
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = message + ";" + mutetime + ";"+ delay;
            string wired_to_item = "";
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
