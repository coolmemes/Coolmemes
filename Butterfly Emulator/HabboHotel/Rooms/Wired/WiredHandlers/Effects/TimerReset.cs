using System;
using System.Collections.Generic;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using System.Data;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class TimerReset : IWiredEffect, IWiredTrigger, IWiredCycleable
    {
        private Room room;
        private RoomItem itemID;
        private WiredHandler handler;

        private int delay;
        private int cycles;

        private bool disposed;

        public TimerReset(Room room, WiredHandler handler, int delay, RoomItem itemID)
        {
            this.itemID = itemID;
            this.room = room;
            this.handler = handler;
            this.delay = delay;
            this.cycles = 0;
            this.disposed = false;
        }

        public int Time
        {
            get
            {
                return delay;
            }
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            if (delay > 0)
            {
                cycles = 0;
                handler.RequestCycle(this);
            }
            else
            {
                ResetTimers();
            }
        }

        public bool OnCycle()
        {
            if (cycles >= delay)
            {
                ResetTimers();
                return false;
            }
            else
            {
                cycles++;
            }
            return true;
        }

        public void Dispose()
        {
            disposed = true;
            room = null;
            handler = null;
        }

        private void ResetTimers()
        {
            handler.needTimersReset = 1;
            room.lastTimerReset = DateTime.Now;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = delay.ToString() + ";;false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID.Id + "', @data" + itemID.Id + ", @to_item" + itemID.Id + ", @original_location" + itemID.Id + ")");
            wiredInserts.AddParameter("data" + itemID.Id, wired_data);
            wiredInserts.AddParameter("to_item" + itemID.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID.Id, wired_original_location);
        }

        public bool Disposed()
        {
            return disposed;
        }

        public void ResetTimer()
        {

        }
    }
}
