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
    class HandleItemUser : IWiredCondition
    {
        private Int32 handleId;
        private RoomItem item;
        private bool isDisposed;

        public Int32 HandleId
        {
            get
            {
                return handleId;
            }
        }

        public HandleItemUser(Int32 handleId, RoomItem item)
        {
            this.handleId = handleId;
            this.isDisposed = false;
            this.item = item;
        }

        public bool AllowsExecution(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return false;

            if (user.CarryItemID == handleId)
                return true;

            return false;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = handleId + ";;false";
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
