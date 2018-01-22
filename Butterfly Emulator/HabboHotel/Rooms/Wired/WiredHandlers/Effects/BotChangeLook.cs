using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Messages;
using Butterfly.Util;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class BotChangeLook : IWiredTrigger, IWiredEffect, IWiredCycleable
    {
        private UInt32 itemID;
        private Room room;
        private WiredHandler handler;
        private String message;
        private UInt32 time;
        private UInt32 cycles;
        private Boolean disposed;

        public String Message
        {
            get
            {
                return message;
            }
        }

        public UInt32 Time
        {
            get
            {
                return time;
            }
        }

        public BotChangeLook(UInt32 itemID, Room room, WiredHandler handler, String message, UInt32 time)
        {
            this.itemID = itemID;
            this.room = room;
            this.handler = handler;
            this.message = message;
            this.time = time;
            this.disposed = false;
            this.cycles = 0;
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            if (disposed)
                return;

            cycles = 0;
            if (time == 0 && user != null)
            {
                DoAction();
            }
            else
            {
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
                DoAction();
                return false;
            }

            return true;
        }

        private void DoAction()
        {
            if (!string.IsNullOrEmpty(message))
            {
                List<RoomUser> botList = room.GetRoomUserManager().GetBots;
                foreach (RoomUser bot in botList)
                {
                    if (bot.GetUsername().ToLower().Equals(message.Split((char)9)[0].ToLower()))
                    {
                        if (!bot.GetLook().Equals(message.Split((char)9)[1].ToLower()))
                        {
                            bot.BotData.Look = message.Split((char)9)[1].ToLower();
                            bot.needSqlUpdate = true;

                            ServerMessage Message = new ServerMessage(Outgoing.UpdateUserInformation);
                            Message.AppendInt32(bot.VirtualId);
                            Message.AppendString(bot.BotData.Look);
                            Message.AppendString(bot.BotData.Gender);
                            Message.AppendString(bot.BotData.Motto);
                            Message.AppendInt32(0);
                            room.SendMessage(Message);
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
            string wired_data = message + ";;" + time;
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID + "', @data" + itemID + ", @to_item" + itemID + ", @original_location" + itemID + ")");
            wiredInserts.AddParameter("data" + itemID, wired_data);
            wiredInserts.AddParameter("to_item" + itemID, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID, wired_original_location);
        }
    }
}
