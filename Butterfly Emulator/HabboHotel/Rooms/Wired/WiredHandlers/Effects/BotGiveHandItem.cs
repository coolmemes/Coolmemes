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
    class BotGiveHandItem : IWiredTrigger, IWiredEffect, IWiredCycleable
    {
        private UInt32 itemID;
        private Room room;
        private WiredHandler handler;
        private String botname;
        private Int32 handitem;
        private UInt32 time;
        private UInt32 cycles;
        private Boolean disposed;
        private readonly Queue delayedUsers;

        public String BotName
        {
            get
            {
                return botname;
            }
        }

        public Int32 HandItem
        {
            get
            {
                return handitem;
            }
        }

        public UInt32 Time
        {
            get
            {
                return time;
            }
        }

        public BotGiveHandItem(UInt32 itemID, Room room, WiredHandler handler, String botname, Int32 handitem, UInt32 time)
        {
            this.itemID = itemID;
            this.room = room;
            this.handler = handler;
            this.botname = botname;
            this.handitem = handitem;
            this.time = time;
            this.disposed = false;
            this.cycles = 0;
            this.delayedUsers = new Queue();
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            if (disposed)
                return;

            cycles = 0;
            if (time == 0 && user != null)
            {
                DoAction(user);
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

        public bool OnCycle()
        {
            if (disposed)
                return false;

            cycles++;
            if (cycles > time)
            {
                if (delayedUsers.Count > 0)
                {
                    lock (delayedUsers.SyncRoot)
                    {
                        while (delayedUsers.Count > 0)
                        {
                            var user = (RoomUser)delayedUsers.Dequeue();
                            DoAction(user);
                        }
                    }
                }
                return false;
            }

            return true;
        }

        private void DoAction(RoomUser user)
        {
            if (!string.IsNullOrEmpty(botname))
            {
                List<RoomUser> botList = room.GetRoomUserManager().GetBots;
                foreach (RoomUser bot in botList)
                {
                    if (bot.GetUsername().ToLower().Equals(botname.ToLower()))
                    {
                        if (user != null)
                        {
                            bot.MoveTo(user.SquareInFront);
                            user.CarryItem(handitem);
                        }
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            disposed = true;
            room = null;
            handler = null;
        }

        public bool Disposed()
        {
            return disposed;
        }

        public void ResetTimer()
        {

        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = botname + ";" + handitem + ";" + time;
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID + "', @data" + itemID + ", @to_item" + itemID + ", @original_location" + itemID + ")");
            wiredInserts.AddParameter("data" + itemID, wired_data);
            wiredInserts.AddParameter("to_item" + itemID, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID, wired_original_location);
        }
    }
}