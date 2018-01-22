using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class BotAlcanzaFurni : IWiredTrigger
    {
        private RoomItem item;
        private WiredHandler handler;
        private List<RoomItem> items;
        private String botname;
        private readonly UserWalksFurniDelegate delegateFunction;

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public String Botname
        {
            get
            {
                return botname;
            }
        }

        public BotAlcanzaFurni(RoomItem item, WiredHandler handler, RoomUserManager roomUserManager, List<RoomItem> items, string botname)
        {
            this.item = item;
            this.handler = handler;
            this.items = items;
            this.botname = botname;

            delegateFunction = roomUserManager_OnBotTakeItem;

            foreach (var targetItem in items)
            {
                targetItem.OnBotWalksOnFurni += delegateFunction;
            }
        }

        private void roomUserManager_OnBotTakeItem(object sender, EventArgs e)
        {
            var user = (RoomUser)sender;
            if (user != null && user.GetUsername().ToLower() == botname.ToLower())
                handler.RequestStackHandle(item, null, null, Team.none);
        }

        public void Dispose()
        {
            handler = null;
            botname = null;
            if (items != null)
            {
                foreach (var targetItem in items)
                {
                    targetItem.OnBotWalksOnFurni -= delegateFunction;
                }
                items.Clear();
            }
            item = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = botname.ToString() + ";;";
            string wired_to_item = "";
            if (items.Count > 0)
            {
                lock (items)
                {
                    foreach (var id in items)
                    {
                        wired_to_item += id.Id + ";";
                    }
                    if (wired_to_item.Length > 0)
                        wired_to_item = wired_to_item.Substring(0, wired_to_item.Length - 1);
                }
            }
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }
    }
}