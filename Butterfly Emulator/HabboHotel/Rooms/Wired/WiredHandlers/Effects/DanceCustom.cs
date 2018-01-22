using Butterfly.Core;
using Butterfly.HabboHotel.Filter;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Messages;
using Butterfly.Util;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class DanceCustom : IWiredTrigger, IWiredEffect
    {
        private readonly WiredHandler handler;
        private RoomItem itemID;

        private string message;

        public DanceCustom(string message, WiredHandler handler, RoomItem itemID)
        {
            this.itemID = itemID;
            this.handler = handler;
            this.message = message;
        }

        public string Message
        {
            get
            {
                return message;
            }
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            if (user != null && !user.IsBot && user.GetClient() != null && message.Length > 0)
            {
                int DanceId = Convert.ToInt32(message);

                if (DanceId < 1 || DanceId > 4 && DanceId != -1)
                {
                    user.WhisperComposer("WIRED de Dança configurado errado, valores permitidos: 1-4");
                    return;
                }                   

                user.DanceId = DanceId;

                // envia o packet da dança.
                ServerMessage DanceMessage = new ServerMessage(Outgoing.Dance);
                DanceMessage.AppendInt32(user.VirtualId);
                DanceMessage.AppendInt32(DanceId);
                user.GetClient().SendMessage(DanceMessage);
            }
        }

        public void Dispose()
        {
            message = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = message.ToString() + ";;false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID.Id + "', @data" + itemID.Id + ", @to_item" + itemID.Id + ", @original_location" + itemID.Id + ")");
            wiredInserts.AddParameter("data" + itemID.Id, wired_data);
            wiredInserts.AddParameter("to_item" + itemID.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID.Id, wired_original_location);
        }
    }
}
