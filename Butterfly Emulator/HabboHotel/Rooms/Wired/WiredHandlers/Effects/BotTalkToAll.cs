using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Messages;
using Butterfly.Util;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class BotTalkToAll : IWiredTrigger, IWiredEffect, IWiredCycleable
    {
        private UInt32 itemID;
        private Room room;
        private WiredHandler handler;
        private String message;
        private Boolean talkorshout; // true = shout | false = hablar
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

        public Int32 TalkOrShout
        {
            get
            {
                return talkorshout ? 1 : 0;
            }
        }

        public UInt32 Time
        {
            get
            {
                return time;
            }
        }

        public BotTalkToAll(UInt32 itemID, Room room, WiredHandler handler, String message, Boolean talkorshout, UInt32 time)
        {
            this.itemID = itemID;
            this.room = room;
            this.handler = handler;
            this.message = message;
            this.talkorshout = talkorshout;
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
                        ServerMessage ChatMessage = new ServerMessage((talkorshout) ? Outgoing.Shout : Outgoing.Talk);
                        ChatMessage.AppendInt32(bot.VirtualId);
                        ChatMessage.AppendString(message.Split((char)9)[1]);
                        ChatMessage.AppendInt32(0);
                        ChatMessage.AppendInt32(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR);
                        ChatMessage.AppendInt32(0);
                        ChatMessage.AppendInt32(-1);
                        room.SendMessage(ChatMessage);

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
            string wired_data = message + ";" + time + ";" + talkorshout.ToString();
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID + "', @data" + itemID + ", @to_item" + itemID + ", @original_location" + itemID + ")");
            wiredInserts.AddParameter("data" + itemID, wired_data);
            wiredInserts.AddParameter("to_item" + itemID, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID, wired_original_location);
        }
    }
}
