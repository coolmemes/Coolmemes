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

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class LongRepeater : IWiredTrigger, IWiredCycleable
    {
        private int cyclesRequired;
        private int cycleCount;
        private WiredHandler handler;
        private RoomItem item;
        private bool disposed;

        public LongRepeater(WiredHandler handler, RoomItem item, int cyclesRequired)
        {
            this.handler = handler;
            this.cyclesRequired = cyclesRequired;
            this.item = item;

            handler.RequestCycle(this);
            disposed = false;
        }

        public int Time
        {
            get
            {
                return cyclesRequired;
            }
        }

        public bool OnCycle()
        {
            cycleCount++;

            if (cycleCount > ((cyclesRequired * 10) - 0.5))
            {
                handler.RequestStackHandle(item, null, null, Team.none);
                //InteractorGenericSwitch.DoAnimation(item);
                cycleCount = 0;
            }
            return true;
        }

        public void Dispose()
        {
            disposed = true;
            handler = null;
            item = null;
        }

        public void ResetTimer()
        {
            cycleCount = 0;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = cyclesRequired.ToString() + ";;false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }

        public bool Disposed()
        {
            return disposed;
        }
    }
}
