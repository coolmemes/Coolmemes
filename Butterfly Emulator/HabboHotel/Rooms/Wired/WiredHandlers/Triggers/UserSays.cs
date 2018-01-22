using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Messages;
using Butterfly.Util;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class UserSays : IWiredTrigger
    {
        private RoomItem item;
        private WiredHandler handler;
        private bool isOwnerOnly;
        private string triggerMessage;
        private readonly RoomUserSaysDelegate delegateFunction;

        public UserSays(RoomItem item, WiredHandler handler, bool isOwnerOnly, string triggerMessage, Room room)
        {
            this.item = item;
            this.handler = handler;
            this.isOwnerOnly = isOwnerOnly;
            this.triggerMessage = triggerMessage;
            delegateFunction = roomUserManager_OnUserSays;

            room.OnUserSays += delegateFunction;
        }

        public String Message
        {
            get
            {
                return triggerMessage;
            }
        }

        public Boolean IsOwnerOnly
        {
            get
            {
                return isOwnerOnly;
            }
        }

        private bool roomUserManager_OnUserSays(object sender, UserSaysArgs e)
        {
            var userSaying = e.user;
            var message = e.message;

            if (userSaying == null || userSaying.GetClient() == null || triggerMessage.Length == 0)
            {
                return false;
            }

            if ((!isOwnerOnly && canBeTriggered(message)) || (isOwnerOnly && userSaying.IsOwner() && canBeTriggered(message)))
            {
                if (handler.RequestStackHandle(item, null, userSaying, e.user.team))
                {
                    userSaying.WhisperComposer(message, 0);
                    return true;
                }                
            }
            return false;
        }

        private bool canBeTriggered(string message)
        {
            return message.ToLower().Contains(triggerMessage.ToLower());
        }

        public void Dispose()
        {
            handler.GetRoom().OnUserSays -= delegateFunction;
            item = null;
            handler = null;
            triggerMessage = null;
        }


        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = triggerMessage.ToString() + ";;" + isOwnerOnly;
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }
    }
}
