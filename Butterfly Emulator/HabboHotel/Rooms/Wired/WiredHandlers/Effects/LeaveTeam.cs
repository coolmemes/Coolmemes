using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class LeaveTeam : IWiredTrigger, IWiredEffect
    {
        private readonly WiredHandler handler;
        private RoomItem itemID;

        public LeaveTeam(WiredHandler handler, RoomItem itemID)
        {
            this.itemID = itemID;
            this.handler = handler;
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            //InteractorGenericSwitch.DoAnimation(itemID);

            if (user != null && !user.IsBot && user.GetClient() != null)
            {
                if (user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect == 40 || user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect == 41 || user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect == 42 || user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect == 43)
                    user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(0);
                user.team = Team.none;
            }
        }

        public void Dispose()
        {

        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            //WiredUtillity.SaveTriggerItem(dbClient, (int)itemID.Id, ((int)staticTeam).ToString(), string.Empty, false);
        }
    }
}
