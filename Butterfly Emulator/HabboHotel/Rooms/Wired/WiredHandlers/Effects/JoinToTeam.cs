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
    class JoinToTeam : IWiredTrigger, IWiredEffect
    {
        private readonly WiredHandler handler;
        private RoomItem itemID;
        private Team staticTeam;

        public JoinToTeam(WiredHandler handler, RoomItem itemID, Team team)
        {
            this.itemID = itemID;
            this.handler = handler;
            this.staticTeam = team;
        }

        public Team Team
        {
            get
            {
                return staticTeam;
            }
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            //InteractorGenericSwitch.DoAnimation(itemID);

            if (user != null && !user.IsBot && user.GetClient() != null)
            {
                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect((int)staticTeam + 39);
                user.team = staticTeam;
            }
        }

        public void Dispose()
        {

        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = ((int)staticTeam).ToString() + ";;false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID.Id + "', @data" + itemID.Id + ", @to_item" + itemID.Id + ", @original_location" + itemID.Id + ")");
            wiredInserts.AddParameter("data" + itemID.Id, wired_data);
            wiredInserts.AddParameter("to_item" + itemID.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID.Id, wired_original_location);
        }
    }
}
