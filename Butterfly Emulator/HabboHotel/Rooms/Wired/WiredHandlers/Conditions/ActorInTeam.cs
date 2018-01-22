using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions
{
    class ActorInTeam : IWiredCondition
    {
        private Team team;
        private RoomItem item;
        private bool isDisposed;

        public ActorInTeam(Team team, RoomItem item)
        {
            this.team = team;
            this.isDisposed = false;
            this.item = item;
        }

        internal Team Team
        {
            get
            {
                return team;
            }
        }

        public bool AllowsExecution(RoomUser user)
        {
            if (user == null)
                return false;

            if (user.team == team)
                return true;

            return false;
        }


        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = ((int)Team).ToString() + ";;false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }

        public void Dispose()
        {
            isDisposed = true;
            item = null;
        }

        public bool Disposed()
        {
            return isDisposed;
        }
    }
}
