using System.Collections.Generic;
using Butterfly.HabboHotel.Quests;
using ButterStorm;
using HabboEvents;
using Butterfly.Core;
using System;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Users.Messenger;
using Butterfly.HabboHotel.Filter;
namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        internal void RemoveBuddy()
        {
            if (Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            int Requests = Request.PopWiredInt32();

            for (int i = 0; i < Requests; i++)
            {
                int FriendId2 = Request.PopWiredInt32();

                if (FriendId2 < 0)
                    continue;

                uint FriendId = Convert.ToUInt32(FriendId2);


                if (!Session.GetHabbo().GetMessenger().FriendshipExists(FriendId))
                    continue;

                Session.GetHabbo().GetMessenger().DestroyFriendship(FriendId);
            }
        }

        internal void SearchHabbo()
        {
            if (Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            Session.SendMessage(Session.GetHabbo().GetMessenger().PerformSearch(Request.PopFixedString()));
        }

        internal void AcceptRequest()
        {
            if (Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            int Amount = Request.PopWiredInt32();

            for (int i = 0; i < Amount; i++)
            {
                uint RequestId = Request.PopWiredUInt();

                MessengerRequest massRequest = Session.GetHabbo().GetMessenger().GetRequest(RequestId);

                if (massRequest == null)
                {
                    continue;
                }

                if (massRequest.To != Session.GetHabbo().Id)
                {
                    // not this user's request. filthy haxxor!
                    return;
                }

                if (!Session.GetHabbo().GetMessenger().FriendshipExists(massRequest.To))
                {
                    Session.GetHabbo().GetMessenger().CreateFriendship(massRequest.From);
                }

                Session.GetHabbo().GetMessenger().HandleRequest(RequestId);
            }
        }

        internal void DeclineRequest()
        {
            if (Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            bool AllUsers = Request.PopWiredBoolean();
            int Amount = Request.PopWiredInt32();

            if (AllUsers == false && Amount == 1)
            {
                Session.GetHabbo().GetMessenger().HandleRequest(Request.PopWiredUInt());
            }
            else
            {
                Session.GetHabbo().GetMessenger().HandleAllRequests();
            }
        }

        internal void RequestBuddy()
        {
            if (Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            if (Session.GetHabbo().GetMessenger().RequestBuddy(Request.PopFixedString()))
            {
                OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_FRIEND);
            }
        }

        internal void SendInstantMessenger()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
            {
                return;
            }

            var userId = Request.PopWiredInt32();
            var message = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());

            #region Mute
            if (Session.GetHabbo().Rank < 4) // Si no es un staff comprobamos si está muteado.
            {
                int timeToEndGlobalMute = OtanixEnvironment.GetGame().GetMuteManager().HasMuteExpired(Session.GetHabbo().Id);
                if (timeToEndGlobalMute > 0)
                    return;
            }
            #endregion
            #region Flood
            if (!Session.GetHabbo().HasFuse("ignore_flood_filter"))
            {
                TimeSpan SinceLastMessage = DateTime.Now - Session.GetHabbo().spamFloodTime;
                if (SinceLastMessage.Seconds > 3)
                {
                    FloodCount = 0;
                }
                else if (FloodCount > 5)
                {
                    OtanixEnvironment.GetGame().GetMuteManager().AddUserMute(Session.GetHabbo().Id, 0.5);
                    return;
                }
                Session.GetHabbo().spamFloodTime = DateTime.Now;
                FloodCount++;
            }
            #endregion
            #region Filter
            if (!Session.GetHabbo().HasFuse("ignore_spam_filter"))
            {
                if (BlackWordsManager.Check(message, BlackWordType.Hotel, Session, "<Consola Privado>"))
                    return;
            }
            #endregion

            bool isGroup = userId < 0;

            if (isGroup)
                Session.GetHabbo().GetMessenger().SendInstantMessageGroup(userId, message);
            else
                Session.GetHabbo().GetMessenger().SendInstantMessage(Convert.ToUInt32(userId), message);
        }

        internal void FollowBuddy()
        {

            var BuddyId2 = Request.PopWiredInt32();

            if (BuddyId2 < 0)
                return;

            var BuddyId = Convert.ToUInt32(BuddyId2);

            var Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(BuddyId);

            if (Client == null || Client.GetHabbo() == null || !Client.GetHabbo().InRoom)
            {
                return;
            }

            if (!Session.GetHabbo().GetMessenger().FriendshipExists(BuddyId))
            {
                return;
            }

            if (Client.GetHabbo().CurrentRoomId == Session.GetHabbo().CurrentRoomId)
            {
                Session.SendNotif("Você está no mesmo quarto que ele.");
                return;
            }

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Client.GetHabbo().CurrentRoomId);
           
            if (Room == null)
            {
                return;
            }

            GetResponse().Init(Outgoing.FollowBuddy);
            GetResponse().AppendUInt(Client.GetHabbo().CurrentRoomId);
            SendResponse();

            GetResponse().Init(Outgoing.RoomData);
            Response.AppendBoolean(true); // some info about if this packet show roomdata etc (someshit like validRoomEntering or something...)
            Room.RoomData.Serialize(Response);
            Response.AppendBoolean(true); // is Waiting to the validRoomEntering (now understand someshits...)
            Response.AppendBoolean((Room.RoomData.Type.ToLower() == "public") ? true : false); // navigator.staffpicks.unpick(true) / navigator.staffpicks.pick(false) (So check if this room is already picked by staff!)
            Response.AppendBoolean(false); // ignore (validRoomEntering) or something...
            Response.AppendBoolean(Room.RoomMuted); // navigator.muteall_on/off (they're muted or not)
            Response.AppendInt32(Room.RoomData.MuteFuse); // 0 = moderation_mute_none, 1 = moderation_mute_rights
            Response.AppendInt32(Room.RoomData.KickFuse); // 0 = moderation_kick_none, 1 = moderation_kick_rights, 2 = moderation_kick_all
            Response.AppendInt32(Room.RoomData.BanFuse); // 0 = moderation_ban_none, 1 = moderation_ban_rigths
            Response.AppendBoolean(Room.CheckRights(Session, true)); //mute visible
            Response.AppendInt32(Room.RoomData.BubbleMode); // 0 = Free Flow Mode (bubbles can pass) / 1 = Line-by-Line-Mode (old)
            Response.AppendInt32(Room.RoomData.BubbleType); // 0 = Wide bubbles / 1 = Normal bubbles / 2 = Thin bubbles
            Response.AppendInt32(Room.RoomData.BubbleScroll); // 0 = Fast scrolling up / 1 = Normal scrolling up / 2 = Slow scrolling up
            Response.AppendInt32(14); // Distancia chat
            Response.AppendInt32(1);
            SendResponse();
        }

        internal void SendInstantInvite()
        {
            var count = Request.PopWiredInt32();

            var UserIds = new List<uint>();

            for (var i = 0; i < count; i++)
            {
                UserIds.Add(Request.PopWiredUInt());
            }

            var message = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString(), true);

            #region Mute
            if (Session.GetHabbo().Rank < 4) // Si no es un staff comprobamos si está muteado.
            {
                int timeToEndGlobalMute = OtanixEnvironment.GetGame().GetMuteManager().HasMuteExpired(Session.GetHabbo().Id);
                if (timeToEndGlobalMute > 0)
                    return;
            }
            #endregion
            #region Flood
            if (!Session.GetHabbo().HasFuse("ignore_flood_filter"))
            {
                TimeSpan SinceLastMessage = DateTime.Now - Session.GetHabbo().spamFloodTime;
                if (SinceLastMessage.Seconds > 3)
                {
                    FloodCount = 0;
                }
                else if (FloodCount > 5)
                {
                    OtanixEnvironment.GetGame().GetMuteManager().AddUserMute(Session.GetHabbo().Id, 0.5);
                    return;
                }
                Session.GetHabbo().spamFloodTime = DateTime.Now;
                FloodCount++;
            }
            #endregion
            #region Filter
            if (!Session.GetHabbo().HasFuse("ignore_spam_filter"))
            {
                if (BlackWordsManager.Check(message, BlackWordType.Hotel, Session, "<Consola Spam>"))
                    return;
            }
            #endregion

            ServerMessage Message = new ServerMessage(Outgoing.InstantInvite);
            Message.AppendUInt(Session.GetHabbo().Id);
            Message.AppendString(message);

            foreach (var Id in UserIds)
            {
                if (!Session.GetHabbo().GetMessenger().FriendshipExists(Id))
                    continue;

                var Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Id);

                if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().IgnoreRoomInvitations)
                    return;

                Client.SendMessage(Message);
            }
        }
    }
}
