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
    class EffectCustom : IWiredTrigger, IWiredEffect
    {
        private readonly WiredHandler handler;
        private RoomItem itemID;

        private string message;

        public EffectCustom(string message, WiredHandler handler, RoomItem itemID)
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
            //InteractorGenericSwitch.DoAnimation(itemID);

            if (user != null && !user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() != null && message.Length > 0)
            {
                int effectid = Convert.ToInt16(message);

                if (effectid < 0 || effectid > 1000)
                    effectid = 0;

                if (effectid == user.CurrentEffect)
                    effectid = 0;

                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(effectid);
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
