using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class Collision : IWiredTrigger, IWiredCycleable
    {
        private RoomItem item;
        private WiredHandler handler;
        private bool disposed;

        public Collision(RoomItem item, WiredHandler handler, RoomUserManager roomUserManager)
        {
            this.item = item;
            this.handler = handler;
            
            handler.RequestCycle(this);
            this.disposed = false;
        }

        public bool OnCycle()
        {
            if (item.wiredEventUser.Count > 0)
            {
                for(int i = 0; i < item.wiredEventUser.Count; i++)
                {
                    handler.RequestStackHandle(item, null, item.wiredEventUser[i], Team.none);
                }

                item.wiredEventUser.Clear();
            }

            return true;
        }

        public void ResetTimer()
        {

        }

        public void Dispose()
        {
            handler = null;
            disposed = true;
            item = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {

        }

        public bool Disposed()
        {
            return disposed;
        }
    }
}