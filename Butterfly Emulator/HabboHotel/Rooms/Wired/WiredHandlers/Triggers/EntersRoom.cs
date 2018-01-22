using System;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class EntersRoom : IWiredTrigger
    {
        private RoomItem item;
        private WiredHandler handler;
        private bool isOneUser;
        private string userName;
        private readonly RoomEventDelegate delegateFunction;

        public EntersRoom(RoomItem item, WiredHandler handler, RoomUserManager roomUserManager, bool isOneUser, string userName)
        {
            this.item = item;
            this.handler = handler;
            this.isOneUser = isOneUser;
            this.userName = userName;
            delegateFunction = roomUserManager_OnUserEnter;

            roomUserManager.OnUserEnter += delegateFunction;
        }

        public string Username
        {
            get
            {
                return userName;
            }
        }

        private void roomUserManager_OnUserEnter(object sender, EventArgs e)
        {
            var user = (RoomUser)sender;

            if ((!user.IsBot && isOneUser && !string.IsNullOrEmpty(userName) && user.GetUsername() == userName) || !isOneUser)
            {
                //InteractorGenericSwitch.DoAnimation(item);
                handler.RequestStackHandle(item, null, user, user.team);
            }
        }

        public void Dispose()
        {
            handler = null;
            userName = null;
            if (item != null && item.GetRoom() != null)
                item.GetRoom().GetRoomUserManager().OnUserEnter -= delegateFunction;
            item = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = userName.ToString() + ";;" + isOneUser;
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }
    }
}
