using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions
{
    class UserNotWearingEffect : IWiredCondition
    {
        private UInt32 effect;
        private RoomItem item;
        private bool isDisposed;

        public UInt32 Effect
        {
            get
            {
                return effect;
            }
        }

        public UserNotWearingEffect(UInt32 effect, RoomItem item)
        {
            this.effect = effect;
            this.isDisposed = false;
            this.item = item;
        }

        public bool AllowsExecution(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return false;

            if (user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect != this.effect)
                return true;

            return false;
        }


        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = this.effect + ";;False";
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
