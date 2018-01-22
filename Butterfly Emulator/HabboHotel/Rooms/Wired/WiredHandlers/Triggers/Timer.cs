using System;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class Timer : IWiredTrigger, IWiredCycleable
    {
        private RoomItem item;
        private WiredHandler handler;
        private int requiredCycles;
        private int currentCycle;
        private bool disposed;

        public Timer(RoomItem item, WiredHandler handler, int cycleCount, GameManager gameManager)
        {
            this.item = item;
            this.handler = handler;
            requiredCycles = cycleCount;
            currentCycle = 0;
            handler.RequestCycle(this);
            disposed = false;
        }

        public int IntTimer
        {
            get
            {
                return requiredCycles;
            }
        }

        public bool OnCycle()
        {
            if (currentCycle == -1)
                return true;

            if (requiredCycles <= currentCycle)
            {
                currentCycle = -1;
                handler.RequestStackHandle(item, null, null, Team.none);
            }
            else
            {
                currentCycle++;
            }
            return true;
        }

        public void Dispose()
        {
            disposed = true;
            item = null;
            handler = null;
        }

        public void ResetTimer()
        {
            currentCycle = 0;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = requiredCycles.ToString() + ";;false";
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
