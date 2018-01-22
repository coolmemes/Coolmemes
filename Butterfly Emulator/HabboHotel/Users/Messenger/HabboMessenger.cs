using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database;
using HabboEvents;
using Butterfly.Core;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.ChatMessageStorage;
using Butterfly.HabboHotel.Group;

namespace Butterfly.HabboHotel.Users.Messenger
{
    class HabboMessenger
    {
        private readonly uint UserId;
        public Dictionary<uint, MessengerRequest> requests;
        private Dictionary<uint, MessengerBuddy> friends;
        internal bool AppearOffline;

        internal int myFriends
        {
            get
            {
                return friends.Count;
            }
        }

        internal HabboMessenger(uint UserId, Dictionary<uint, MessengerBuddy> friends, Dictionary<uint, MessengerRequest> requests)
        {
            this.requests = requests;
            this.friends = friends;
            this.UserId = UserId;
        }

        internal void ClearRequests()
        {
            requests.Clear();
        }

        internal MessengerRequest GetRequest(uint senderID)
        {
            if (requests.ContainsKey(senderID))
                return requests[senderID];

            return null;
        }

        internal void Destroy()
        {
            try
            {
                var onlineUsers = OtanixEnvironment.GetGame().GetClientManager().GetClientsById(friends.Keys);

                foreach (var client in onlineUsers.Where(client => client != null && client.GetHabbo() != null && client.GetHabbo().GetMessenger() != null))
                {
                    client.GetHabbo().GetMessenger().UpdateFriend(UserId, GetClient());
                }
            }
            catch (Exception e)
            {
                Logging.LogException("Messenger destroy --> " + e.ToString());
            }

            friends.Clear();
            requests.Clear();
        }

        internal void OnStatusChanged()
        {
            IEnumerable<GameClient> onlineUsers = OtanixEnvironment.GetGame().GetClientManager().GetClientsById(friends.Keys);

            foreach (var client in onlineUsers)
            {
                if (client == null || client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
                    continue;

                client.GetHabbo().GetMessenger().UpdateFriend(UserId, client);
            }
        }

        internal void UpdateFriend(uint userid, GameClient client)
        {
            if (friends.ContainsKey(userid))
            {
                var Userclient = GetClient();
                if (Userclient != null && friends[userid] != null && client != null && client.GetHabbo() != null)
                {
                    Userclient.SendMessage(SerializeUpdate(friends[userid], client.GetHabbo()));
                }
            }
        }

        internal void HandleAllRequests()
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM messenger_requests WHERE sender = " + UserId + " OR receiver = " + UserId);
            }

            ClearRequests();
        }

        internal void HandleRequest(uint sender)
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM messenger_requests WHERE (sender = " + UserId + " AND receiver = " + sender + ") OR (receiver = " + UserId + " AND sender = " + sender + ")");
            }

            requests.Remove(sender);
        }

        internal void CreateFriendship(uint friendID)
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("REPLACE INTO messenger_friendships (sender,receiver) VALUES (" + UserId + "," + friendID + ")");
            }

            OnNewFriendship(friendID);

            var User = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(UserId, "ACH_FriendListSize", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(friendID, "ACH_FriendListSize", 1);

            if (GetClient().GetHabbo().CitizenshipLevel == 3)
                OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(GetClient(), "citizenship");

            if (User != null && User.GetHabbo().GetMessenger() != null)
            {
                User.GetHabbo().GetMessenger().OnNewFriendship(UserId);
            }
        }

        internal void DestroyFriendship(uint friendID)
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM messenger_friendships WHERE (sender = " + UserId + " AND receiver = " + friendID + ") OR (receiver = " + UserId + " AND sender = " + friendID + ")");
                dbClient.runFastQuery("DELETE FROM user_relationships WHERE user_id = " + UserId + " AND member_id = " + friendID);
                dbClient.runFastQuery("DELETE FROM user_relationships WHERE user_id = " + friendID + " AND member_id = " + UserId);
            }

            OnDestroyFriendship(friendID);

            GameClient User = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(UserId, "ACH_FriendListSize", -1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(friendID, "ACH_FriendListSize", -1);

            if (User != null && User.GetHabbo().GetMessenger() != null)
            {
                User.GetHabbo().GetMessenger().OnDestroyFriendship(UserId);
            }
        }

        internal void OnNewFriendship(uint friendID)
        {
            if ((requests.Count + friends.Count) > EmuSettings.FRIENDS_LIMIT)
            {
                GetClient().SendNotif("Você possui muitos amigos, exclua alguns para poder adicionar mais!");
                return;
            }

            var friend = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);
            MessengerBuddy newFriend;

            if (friend == null || friend.GetHabbo() == null)
            {
                DataRow dRow;
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT username,motto,look,last_online FROM users WHERE id = " + friendID);
                    dRow = dbClient.getRow();
                }

                newFriend = new MessengerBuddy(friendID, (string)dRow["username"], (string)dRow["look"], (string)dRow["motto"]);
            }
            else
            {
                var user = friend.GetHabbo();
                newFriend = new MessengerBuddy(friendID, user.Username, user.Look, user.Motto);
            }

            if (!friends.ContainsKey(friendID))
                friends.Add(friendID, newFriend);

            GetClient().SendMessage(SerializeUpdate(newFriend, GetClient().GetHabbo()));
        }

        internal bool RequestExists(uint requestID)
        {
            if (requests.ContainsKey(requestID))
                return true;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT sender FROM messenger_friendships WHERE sender = @myID AND receiver = @friendID");
                dbClient.addParameter("myID", (int)UserId);
                dbClient.addParameter("friendID", (int)requestID);
                return dbClient.findsResult();
            }
        }

        internal bool FriendshipExists(uint friendID)
        {
            return friends.ContainsKey(friendID);
        }

        internal void OnDestroyFriendship(uint Friend)
        {
            friends.Remove(Friend);
            if (GetClient().GetHabbo().GetRelationshipComposer().LoveRelation.ContainsKey(Friend))
                GetClient().GetHabbo().GetRelationshipComposer().LoveRelation.Remove(Friend);

            if (GetClient().GetHabbo().GetRelationshipComposer().FriendRelation.ContainsKey(Friend))
                GetClient().GetHabbo().GetRelationshipComposer().FriendRelation.Remove(Friend);

            if (GetClient().GetHabbo().GetRelationshipComposer().DieRelation.ContainsKey(Friend))
                GetClient().GetHabbo().GetRelationshipComposer().DieRelation.Remove(Friend);

            GetClient().GetMessageHandler().GetResponse().Init(Outgoing.FriendUpdate);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(0);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(1);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(-1);
            GetClient().GetMessageHandler().GetResponse().AppendUInt(Friend);
            GetClient().GetMessageHandler().SendResponse();
        }

        internal bool RequestBuddy(string UserQuery)
        {
            if ((requests.Count + friends.Count) > EmuSettings.FRIENDS_LIMIT)
            {
                GetClient().GetMessageHandler().GetResponse().Init(Outgoing.CannotAddFriends);
                GetClient().GetMessageHandler().GetResponse().AppendInt32(39);
                GetClient().GetMessageHandler().GetResponse().AppendInt32(1); // 1 -> nuestra lista esta llena
                GetClient().GetMessageHandler().SendResponse();
                return true;
            }

            uint userID;
            bool hasFQDisabled;

            var client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(UserQuery);

            if (client == null)
            {
                DataRow Row = null;
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT id,block_newfriends FROM users WHERE username = @query");
                    dbClient.addParameter("query", UserQuery.ToLower());
                    Row = dbClient.getRow();
                }

                if (Row == null)
                    return false;

                userID = Convert.ToUInt32(Row["id"]);
                hasFQDisabled = OtanixEnvironment.EnumToBool(Row["block_newfriends"].ToString());
            }
            else
            {
                userID = client.GetHabbo().Id;
                hasFQDisabled = client.GetHabbo().HasFriendRequestsDisabled;
            }

            if (hasFQDisabled)
            {
                GetClient().GetMessageHandler().GetResponse().Init(Outgoing.CannotAddFriends);
                GetClient().GetMessageHandler().GetResponse().AppendInt32(39);
                GetClient().GetMessageHandler().GetResponse().AppendInt32(3); // 3 -> Amigo tiene peticiones desactivadas
                GetClient().GetMessageHandler().SendResponse();
                return true;
            }

            var ToId = userID;
            if (RequestExists(ToId))
            {
                return true;
            }

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("REPLACE INTO messenger_requests (sender,receiver) VALUES (" + UserId + "," + ToId + ")");
            }

            var ToUser = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(ToId);

            if (ToUser == null || ToUser.GetHabbo() == null || ToUser.GetHabbo().GetMessenger() == null)
            {
                return true;
            }

            var Request = new MessengerRequest(ToId, UserId, OtanixEnvironment.GetGame().GetClientManager().GetNameById(UserId));

            ToUser.GetHabbo().GetMessenger().OnNewRequest(UserId);

            var NewFriendNotif = new ServerMessage(Outgoing.SendFriendRequest);
            Request.Serialize(NewFriendNotif);
            ToUser.SendMessage(NewFriendNotif);
            if(!requests.ContainsKey(ToId))
                requests.Add(ToId, Request);

            return true;
        }

        internal void OnNewRequest(uint friendID)
        {
            if (!requests.ContainsKey(friendID))
                requests.Add(friendID, new MessengerRequest(UserId, friendID, OtanixEnvironment.GetGame().GetClientManager().GetNameById(friendID)));
        }

        internal void SendInstantMessageGroup(Int32 GroupId, string Message)
        {
            var realGroupID = Math.Abs(GroupId);
            GroupItem theGroup = OtanixEnvironment.GetGame().GetGroup().LoadGroup(Convert.ToUInt32(realGroupID));
            if (!theGroup.IsMember(UserId))
                return;

            //Console.WriteLine(Message);
            ServerMessage InstantMessage = new ServerMessage(Outgoing.InstantChat);
            InstantMessage.AppendInt32(GroupId);
            InstantMessage.AppendString(Message);
            InstantMessage.AppendInt32(0);
            InstantMessage.AppendString(GetClient().GetHabbo().Username + "/" + GetClient().GetHabbo().Look + "/" + UserId);
            OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(InstantMessage, "ingroup_" + realGroupID, UserId);
        }


        internal void SendInstantMessage(uint ToId, string Message)
        {

            if (GetClient() != null && GetClient().GetHabbo() != null && OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(GetClient().GetHabbo().Id))
            {
                GetClient().SendNotif(LanguageLocale.GetValue("prisao.chatamigos"));
                return;
            }

            if (GetClient() != null && GetClient().GetHabbo() != null && !GetClient().GetHabbo().passouPin)
            {
                GetClient().SendNotif("Você precisa digitar o pin staff");
                return;
            }

            if (ToId == EmuSettings.CHAT_USER_ID)
            {
                if (!GetClient().GetHabbo().HasFuse("fuse_chat_staff")) // no inyections
                    return;

                ServerMessage InstantMessage = new ServerMessage(Outgoing.InstantChat);
                InstantMessage.AppendUInt(EmuSettings.CHAT_USER_ID);
                InstantMessage.AppendString(GetClient().GetHabbo().Username + ": " + Message);
                InstantMessage.AppendInt32(0);
                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(InstantMessage, "fuse_chat_staff", UserId);

                return;
            }

            if (!FriendshipExists(ToId))
            {
                DeliverInstantMessageError(6, ToId);
                return;
            }

            var Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(ToId);

            if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().GetMessenger() == null)
            {
                // DeliverInstantMessageError(5, ToId);
                MessengerChat.AddMessageToId(UserId, ToId, Message);
                return;
            }

            if (OtanixEnvironment.GetGame().GetMuteManager().HasMuteExpired(GetClient().GetHabbo().Id) > 0)
            {
                DeliverInstantMessageError(4, ToId);
                return;
            }

            if (OtanixEnvironment.GetGame().GetMuteManager().HasMuteExpired(Client.GetHabbo().Id) > 0)
            {
                DeliverInstantMessageError(3, ToId); // No return, as this is just a warning.
            }

            if (Message == "")
                return;

            SpyChatMessage.SaveUserLog(UserId, 0, ToId, Message);
            Client.GetHabbo().GetMessenger().DeliverInstantMessage(Message, UserId);
        }

        internal void SendInstantMessageOffline(UInt32 FromUserId, string Message, int timeSended)
        {

            if (GetClient() != null && GetClient().GetHabbo() != null && OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(GetClient().GetHabbo().Id))
            {
                GetClient().SendNotif(LanguageLocale.GetValue("prisao.chatamigos"));
                return;
            }

            if (GetClient() != null && GetClient().GetHabbo() != null && !GetClient().GetHabbo().passouPin)
            {
                GetClient().SendNotif("Você precisa digitar o pin staff");
                return;
            }

            if (!FriendshipExists(FromUserId))
            {
                return;
            }

            DeliverInstantMessage(Message, FromUserId, timeSended);
        }

        internal void DeliverInstantMessage(string message, uint convoID, int timeSended = 0)
        {
            var InstantMessage = new ServerMessage(Outgoing.InstantChat);
            InstantMessage.AppendUInt(convoID);
            InstantMessage.AppendString(message);
            InstantMessage.AppendInt32(timeSended == 0 ? 0 : OtanixEnvironment.GetUnixTimestamp() - timeSended);
            if (GetClient() != null)
                GetClient().SendMessage(InstantMessage);
        }

        internal void DeliverInstantMessageError(int ErrorId, UInt32 ConversationId)
        {
            /*
            3                =     Your friend is muted and cannot reply.
            4                =     Your message was not sent because you are muted.
            5                =     Your friend is not online.
            6                =     Receiver is not your friend anymore.
            7                =     Your friend is busy.
            8                =     Your friend is wanking*/
            ServerMessage reply = new ServerMessage(Outgoing.InstantChatError);
            reply.AppendInt32(ErrorId);
            reply.AppendUInt(ConversationId);
            reply.AppendString("");
            if(GetClient() != null)
                GetClient().SendMessage(reply);
        }

        internal ServerMessage SerializeFriendsCategories(Habbo Habbo)
        {
            // MessengerInitComposer (aqui vai gerar os usuarios e os grupos)
            var reply = new ServerMessage(Outgoing.InitFriendsCategories);
            reply.AppendInt32(1100); // limite de amigos
            reply.AppendInt32(300); // idk
            reply.AppendInt32(1100); // limite de amigos HC
            reply.AppendInt32(1); // categorys (foreach) int = id da categoria, string = Nome da categoria
            reply.AppendInt32(1); // id da categoria
            reply.AppendString("Chats de Grupos"); // nome da categoria
            reply.AppendInt32(100);
            reply.AppendInt32(0);









           /* var reply = new ServerMessage(Outgoing.InitFriendsCategories);
            reply.AppendInt32(1100); // limite de amigos
            reply.AppendInt32(300); // idk
            reply.AppendInt32(1100); // limite de amigos HC
            reply.AppendInt32(0); // categorys (foreach) int = id da categoria, string = Nome da categoria
            reply.AppendInt32(100);
            reply.AppendInt32(0);*/
            return reply;
        }

        internal ServerMessage SerializeFriends(Habbo Habbo)
        {
            var reply = new ServerMessage(Outgoing.InitFriends);
            reply.AppendInt32(1); // return on swf, loool
            reply.AppendInt32(0); // page index
            reply.AppendInt32(friends.Count + GetClient().GetHabbo().MyGroups.Count);

            foreach (var xGroupId in Habbo.MyGroups)
            {
                GroupItem theGroup = OtanixEnvironment.GetGame().GetGroup().LoadGroup(xGroupId);
                if (theGroup.temChat)
                {
                    var friendGroupID = -Convert.ToInt32(theGroup.Id);
                    reply.AppendInt32(friendGroupID);
                    reply.AppendString(theGroup.Name);
                    reply.AppendInt32(0);
                    reply.AppendBoolean(true);
                    reply.AppendBoolean(false);
                    reply.AppendString(theGroup.GroupImage);
                    reply.AppendInt32(1);
                    reply.AppendString(theGroup.Description);
                    reply.AppendString(string.Empty);
                    reply.AppendString(string.Empty);
                    reply.AppendBoolean(false);
                    reply.AppendBoolean(false);
                    reply.AppendBoolean(false);
                    reply.AppendShort(0);
                }
            }
            foreach (var friend in friends.Values)
            {
                friend.UpdateUserSettings();

                if (friend.IsOnline)
                {
                    friend.FriendConnectAlert(Habbo.Username);
                }

                friend.Serialize(reply, Habbo);
            }
            return reply;
        }

        internal static ServerMessage SerializeUpdate(MessengerBuddy friend, Habbo Habbo)
        {
            friend.UpdateUserSettings();

            var reply = new ServerMessage(Outgoing.FriendUpdate);
            reply.AppendInt32(0); // category
            reply.AppendInt32(friend.IsOnline ? 2 : 1); // number of updates
            if (friend.IsOnline)
            {
                reply.AppendInt32(1);
                friend.Serialize(reply, Habbo);
            }
            reply.AppendInt32(0); // don't know
            friend.Serialize(reply, Habbo);
            reply.AppendBoolean(false);

            return reply;
        }

        internal ServerMessage SerializeRequests(GameClient Session)
        {
            if (requests == null)
                return null;

            try
            {
                ServerMessage Message = new ServerMessage(Outgoing.InitRequests);
                Message.AppendInt32((requests.Count > EmuSettings.FRIENDS_REQUEST_LIMIT) ? (int)EmuSettings.FRIENDS_REQUEST_LIMIT : requests.Count);
                Message.AppendInt32((requests.Count > EmuSettings.FRIENDS_REQUEST_LIMIT) ? (int)EmuSettings.FRIENDS_REQUEST_LIMIT : requests.Count);

                var i = 0;
                foreach (var request in requests.Values)
                {
                    i++;
                    if (i > EmuSettings.FRIENDS_REQUEST_LIMIT)
                    {
                        Session.SendNotif("Tienes más de " + EmuSettings.FRIENDS_REQUEST_LIMIT + " peticiones de amigos, por lo que solo hemos cargado " + EmuSettings.FRIENDS_REQUEST_LIMIT + " y las próximas serán cargadas una vez aceptes estas y reinicies sesión!");
                        break;
                    }

                    request.Serialize(Message);
                }

                return Message;
            }
            catch
            {
                return null;
            }
        }

        internal void SerializeOfflineMessages(GameClient Session)
        {
            List<MessengerChatInfo> mci = MessengerChat.GetMessagesByUserId(Session.GetHabbo().Id);

            if (mci == null)
                return;

            foreach (MessengerChatInfo m in mci)
            {
                Session.GetHabbo().GetMessenger().SendInstantMessageOffline(m.UserID, m.Message, m.timeSended);
            }

            MessengerChat.ClearMessageToId(Session.GetHabbo().Id);
        }

        internal ServerMessage PerformSearch(string query)
        {
            var results = SearchResultFactory.GetSearchResult(query);

            var existingFriends = new List<SearchResult>();
            var othersUsers = new List<SearchResult>();

            foreach (var result in results)
            {
                if (result.userID == GetClient().GetHabbo().Id)
                    continue;
                if (FriendshipExists(result.userID))
                    existingFriends.Add(result);
                else
                    othersUsers.Add(result);
            }

            var reply = new ServerMessage(Outgoing.SearchFriend);
            reply.AppendInt32(existingFriends.Count);
            foreach (var result in existingFriends)
            {
                result.Searialize(reply);
            }

            reply.AppendInt32(othersUsers.Count);
            foreach (var result in othersUsers)
            {
                result.Searialize(reply);
            }

            return reply;
        }

        private GameClient GetClient()
        {
            return OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
        }

        internal List<RoomData> GetActiveFriendsRooms()
        {
            List<RoomData> rooms = new List<RoomData>();

            foreach(var buddy in friends.Values.Where(p => p.InRoom))
            {
                RoomData d = buddy.currentRoom.RoomData;
                if (!rooms.Contains(d))
                    rooms.Add(d);

                if (rooms.Count >= 40)
                    break;
            }
            return rooms;
        }

        internal void ClearConsole()
        {
            List<uint> UsersIds = new List<uint>(friends.Keys);

            foreach (uint UserId in UsersIds)
            {
                DestroyFriendship(UserId);
            }

            UsersIds.Clear();
            UsersIds = null;
        }
    }
}