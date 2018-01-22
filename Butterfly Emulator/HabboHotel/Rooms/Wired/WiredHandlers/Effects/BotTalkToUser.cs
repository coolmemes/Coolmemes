using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Messages;
using Butterfly.Util;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class BotTalkToUser : IWiredTrigger, IWiredEffect
    {
        private UInt32 itemID;
        private Room room;
        private WiredHandler handler;
        private String message;
        private Boolean talkorwhisper; // true = susurrar | false = hablar

        public String Message
        {
            get
            {
                return message;
            }
        }

        public Int32 TalkOrWhisper
        {
            get
            {
                return talkorwhisper ? 1 : 0;
            }
        }

        public BotTalkToUser(UInt32 itemID, Room room, WiredHandler handler, String message, Boolean talkorwhisper)
        {
            this.itemID = itemID;
            this.room = room;
            this.handler = handler;
            this.message = message;
            this.talkorwhisper = talkorwhisper;
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            if (user == null || user.GetClient() == null)
                return;

            if (!string.IsNullOrEmpty(message))
            {
                List<RoomUser> botList = room.GetRoomUserManager().GetBots;
                foreach (RoomUser bot in botList)
                {
                    if (bot.GetUsername().ToLower().Equals(message.Split((char)9)[0].ToLower()))
                    {
                        ServerMessage ChatMessage = new ServerMessage((talkorwhisper) ? Outgoing.Whisp : Outgoing.Talk);
                        ChatMessage.AppendInt32(bot.VirtualId);
                        ChatMessage.AppendString(message.Split((char)9)[1]);
                        ChatMessage.AppendInt32(0);
                        ChatMessage.AppendInt32(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR);
                        ChatMessage.AppendInt32(0);
                        ChatMessage.AppendInt32(-1);
                        user.GetClient().SendMessage(ChatMessage);

                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            room = null;
            handler = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = message + ";;" + talkorwhisper.ToString();
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID + "', @data" + itemID + ", @to_item" + itemID + ", @original_location" + itemID + ")");
            wiredInserts.AddParameter("data" + itemID, wired_data);
            wiredInserts.AddParameter("to_item" + itemID, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID, wired_original_location);
        }
    }
}
