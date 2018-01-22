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
    class BotAlcanzaUsuario : IWiredTrigger
    {
        private RoomItem item;
        private WiredHandler handler;
        private String botname;
        private readonly RoomEventDelegate delegateFunction;

        public String Botname
        {
            get
            {
                return botname;
            }
        }

        public BotAlcanzaUsuario(RoomItem item, WiredHandler handler, RoomUserManager roomUserManager, string botname)
        {
            this.item = item;
            this.handler = handler;
            this.botname = botname;

            delegateFunction = roomUserManager_OnBotTakeUser;
            roomUserManager.OnBotTakeUser += delegateFunction;
        }

        private void roomUserManager_OnBotTakeUser(object sender, EventArgs e)
        {
            RoomUser[] userArray = (RoomUser[])sender;
            if(userArray[0] != null && userArray[0].GetUsername().ToLower() == botname.ToLower())
                handler.RequestStackHandle(item, null, userArray[1], userArray[1].team);
        }

        public void Dispose()
        {
            handler = null;
            botname = null;
            if (item != null && item.GetRoom() != null)
                item.GetRoom().GetRoomUserManager().OnBotTakeUser -= delegateFunction;
            item = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = botname.ToString() + ";;";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }
    }
}
