using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions
{
    class DateRangeActive: IWiredCondition
    {
        private Int32 startDate;
        private Int32 endDate;
        private RoomItem item;
        private bool isDisposed;

        public Int32 StartDate
        {
            get
            {
                return startDate;
            }
        }

        public Int32 EndDate
        {
            get
            {
                return endDate;
            }
        }

        public DateRangeActive(Int32 startDate, Int32 endDate, RoomItem item)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.isDisposed = false;
            this.item = item;
        }

        public bool AllowsExecution(RoomUser user)
        {
            Int32 actualUnix = OtanixEnvironment.GetUnixTimestamp();
            if(actualUnix >= startDate && actualUnix <= endDate)
                return true;

            return false;
        }


        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = this.startDate + ";" + this.endDate + ";False";
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
