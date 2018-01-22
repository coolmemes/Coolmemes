using Butterfly.HabboHotel.Items;
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
    class NotActorInGroup : IWiredCondition
    {
        private UInt32 groupId;
        private RoomItem item;
        private bool isDisposed;

        public NotActorInGroup(UInt32 groupId, RoomItem item)
        {
            this.groupId = groupId;
            this.isDisposed = false;
            this.item = item;
        }

        public bool AllowsExecution(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return false;

            if (user.GetClient().GetHabbo().MyGroups.Contains(groupId))
                return false;

            return true;
        }


        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            //WiredUtillity.SaveTriggerItem(dbClient, (int)item.Id, timeout.ToString(), string.Empty, false);
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
