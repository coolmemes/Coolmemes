using System;
using System.Linq;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.Quests;
using Butterfly.HabboHotel.Users.Badges;
using ButterStorm;
using HabboEvents;
using Butterfly.Core;
using Butterfly.HabboHotel.Users.Relationships;
using ButterStorm.HabboHotel.Users.Inventory;
using Butterfly.HabboHotel.Achievements.Composer;
using Butterfly.HabboHotel.Users;
using System.Collections.Generic;
using Butterfly.HabboHotel.Users.HabboQuiz;
using Butterfly.HabboHotel.Alfas.Manager;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Group;
using Butterfly.HabboHotel.Filter;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Users.Nux;
using Butterfly.HabboHotel.Navigators.Bonus;

namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        internal void GetBalance()
        {
            Session.GetHabbo().UpdateCreditsBalance();
            Session.GetHabbo().UpdateExtraMoneyBalance();
        }

        internal void GetBadges()
        {
            Session.SendMessage(Session.GetHabbo().GetBadgeComponent().Serialize());
        }

        internal void UpdateBadges()
        {
            try
            {
                Session.GetHabbo().GetBadgeComponent().ResetSlots();

                var TimesSlot = new int[6];
                for (var i = 0; i < 5; i++)
                {
                    var Slot = Request.PopWiredInt32();
                    var Badge = Request.PopFixedString();

                    TimesSlot[Slot]++;
                    if (TimesSlot[Slot] > 1)
                        return;

                    if (Badge.Length == 0)
                        continue;

                    if (!Session.GetHabbo().GetBadgeComponent().HasBadge(Badge) || Slot < 1 || Slot > 5)
                        return;

                    Session.GetHabbo().GetBadgeComponent().GetBadge(Badge).Slot = Slot;
                    Session.GetHabbo().GetBadgeComponent().GetBadge(Badge).needInsert = true;
                }

                OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_BADGE);

                #region Packet
                var Message = new ServerMessage(Outgoing.UpdateBadges);
                Message.AppendUInt(Session.GetHabbo().Id);
                Message.AppendInt32(Session.GetHabbo().GetBadgeComponent().EquippedCount);

                foreach (Badge Badge in Session.GetHabbo().GetBadgeComponent().BadgeList.Values)
                {
                    if (Badge.Slot <= 0)
                    {
                        continue;
                    }

                    Message.AppendInt32(Badge.Slot);
                    Message.AppendString(Badge.Code + Badge.Level);
                }

                if (Session.GetHabbo().InRoom && Session.GetHabbo().CurrentRoom != null)
                {
                    Session.GetHabbo().CurrentRoom.SendMessage(Message);
                }
                else
                {
                    Session.SendMessage(Message);
                }
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal void GetAchievements()
        {
            OtanixEnvironment.GetGame().GetAchievementManager().GetList(Session);
        }
        public void attTags()
        {
            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);

            if (Room == null)
            {
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Request.PopWiredUInt());

            if (User == null || User.IsBot)
            {
                return;
            }
            ServerMessage message = new ServerMessage(Outgoing.PopularTags);
            message.AppendInt32((int)Session.GetHabbo().Id);
            message.AppendInt32(1);
            //message.AppendInt32(User.GetClient().GetHabbo().Tags.Count);
            //foreach (string Tag in User.GetClient().GetHabbo().Tags)
            //{
                message.AppendString("TesteTag");
            //}

            Session.SendMessage(message);

            return;
        }
        public void SendCampaingData()
        {
            var logText = Request.PopFixedString();
            var secretValue = "";
            if (logText == OtanixEnvironment.GetGame().GetLandingTopUsersManager().EventMessage) // landing.view.dynamic.slot.6.conf
            {
                Session.SendMessage(OtanixEnvironment.GetGame().GetLandingTopUsersManager().GetMessage);
                Session.SendMessage(BonusManager.GenerateMessage(Session));
                return;
            }

            var arrayText = logText.Split(',');
            if (arrayText.Count() == 2)
                secretValue = arrayText[1];

            GetResponse().Init(Outgoing.ParseCampaingData);
            GetResponse().AppendString(logText);
            GetResponse().AppendString(secretValue);
            SendResponse();
        }

        internal void LoadProfile()
        {
            int userID = Request.PopWiredInt32();
            var IsMe = Request.PopWiredBoolean();
            if (userID < 0)
                return;
            Habbo Data = null;
            if(userID == Session.GetHabbo().Id)
                Data = Session.GetHabbo();
            else
                Data = UsersCache.getHabboCache(Convert.ToUInt32(userID));
 
            if (Data == null)
                return;

            int friendCount = 0;
            bool MyFriend = false;
            bool FriendPetition = false;
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT COUNT(*) FROM messenger_friendships WHERE sender = " + userID + " OR receiver = " + userID);
                friendCount = dbClient.getInteger();

                if (userID != Session.GetHabbo().Id)
                {
                    dbClient.setQuery("SELECT NULL FROM messenger_friendships WHERE (sender = " + userID + " AND receiver = " + Session.GetHabbo().Id + ") OR (sender = " + Session.GetHabbo().Id + " AND receiver = " + userID + ")");
                    MyFriend = dbClient.findsResult();

                    if (MyFriend == false)
                    {
                        dbClient.setQuery("SELECT NULL FROM messenger_requests WHERE sender = " + Session.GetHabbo().Id + " AND receiver = " + userID);
                        FriendPetition = dbClient.findsResult();
                    }
                }
            }

            Response.Init(Outgoing.ProfileInformation);
            Response.AppendUInt(Data.Id);
            Response.AppendString(Data.Username);
            Response.AppendString(Data.Look);
            Response.AppendString(Data.Motto);
            Response.AppendString(Data.Created.ToShortDateString().Replace("/", "-"));
            Response.AppendUInt(Data.AchievementPoints); // Achievement Points
            Response.AppendInt32(friendCount); // friends
            Response.AppendBoolean(MyFriend);
            Response.AppendBoolean(FriendPetition);
            Response.AppendBoolean((OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Data.Id) != null));
            Response.AppendInt32(Data.MyGroups.Count); // group count

            foreach (uint groupId in Data.MyGroups)
            {
                GroupItem group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(groupId);
                if (group == null)
                    return;

                Response.AppendUInt(group.Id);
                Response.AppendString(group.Name);
                Response.AppendString(group.GroupImage);
                Response.AppendString(group.HtmlColor1);
                Response.AppendString(group.HtmlColor2);
                Response.AppendBoolean(Data.FavoriteGroup == group.Id);
                Response.AppendUInt(group.OwnerId);
                Response.AppendBoolean(false); // ??
            }
            Response.AppendInt32((Int32)(OtanixEnvironment.GetUnixTimestamp() - Data.LastOnline));
            Response.AppendBoolean(true); // show it
            SendResponse();
        }

        internal void ChangeLook()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            if(OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(Session.GetHabbo().Id))
            {
                Session.SendNotif(LanguageLocale.GetValue("prisao.roupa"));
                return;
            }

            if (!Session.GetHabbo().passouPin)
            {
                Session.SendNotif("Você precisa digitar o pin staff");
                return;
            }

            if ((OtanixEnvironment.GetUnixTimestamp() - Session.GetHabbo().LastChangeLookTime) < 5)
            {
                Session.SendNotif(LanguageLocale.GetValue("change.look.alert.time"));
                return;
            }

            var Gender = Request.PopFixedString().ToUpper();
            var Look = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());

            if (!AntiMutant.ValidateLook(Look, Gender, Session.GetHabbo()))
            {
                return;
            }

            Session.GetHabbo().Look = OtanixEnvironment.FilterFigure(Look);
            Session.GetHabbo().Gender = Gender.ToLower();
            Session.GetHabbo().LastChangeLookTime = OtanixEnvironment.GetUnixTimestamp();

            if (Session.GetHabbo().GetMessenger() != null)
            {
                OtanixEnvironment.GetGame().GetClientManager().QueueConsoleUpdate(Session);
            }

            OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_CHANGE_LOOK);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_AvatarLooks", 1);

            if (Session.GetHabbo().CitizenshipLevel == 1)
                OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "citizenship");

            Session.GetMessageHandler().GetResponse().Init(Outgoing.ChangeMiniLook);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToUpper());
            Session.GetMessageHandler().SendResponse();

            Session.GetMessageHandler().GetResponse().Init(Outgoing.UpdateUserInformation);
            Session.GetMessageHandler().GetResponse().AppendInt32(-1);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToLower());
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Motto);
            Session.GetMessageHandler().GetResponse().AppendUInt(Session.GetHabbo().AchievementPoints);
            Session.GetMessageHandler().SendResponse();

            if (Session.GetHabbo().InRoom)
            {
                var Room = Session.GetHabbo().CurrentRoom;

                if (Room == null)
                {
                    return;
                }

                var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                if (User == null)
                {
                    return;
                }

                var RoomUpdate = new ServerMessage(Outgoing.UpdateUserInformation);
                RoomUpdate.AppendInt32(User.VirtualId);
                RoomUpdate.AppendString(Session.GetHabbo().Look);
                RoomUpdate.AppendString(Session.GetHabbo().Gender.ToLower());
                RoomUpdate.AppendString(Session.GetHabbo().Motto);
                RoomUpdate.AppendUInt(Session.GetHabbo().AchievementPoints);
                Room.SendMessage(RoomUpdate);
            }
        }

        internal void ChangeMotto()
        {

            if (Session != null && Session.GetHabbo() != null && OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(Session.GetHabbo().Id))
            {
                Session.SendNotif(LanguageLocale.GetValue("prisao.missao"));
                return;
            }

            if (Session != null && Session.GetHabbo() != null && !Session.GetHabbo().passouPin)
            {
                Session.SendNotif("Você precisa digitar o pin staff");
                return;
            }

            if ((OtanixEnvironment.GetUnixTimestamp() - Session.GetHabbo().LastChangeLookTime) < 5)
            {
                Session.SendNotif(LanguageLocale.GetValue("change.look.alert.time"));
                return;
            }

            string Motto = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());

            if (BlackWordsManager.Check(Motto, BlackWordType.Hotel, Session, "<Mision>"))
                return;

            if (Motto == Session.GetHabbo().Motto) // Prevents spam?
            {
                return;
            }

            Session.GetHabbo().Motto = Motto;
            Session.GetHabbo().LastChangeLookTime = OtanixEnvironment.GetUnixTimestamp();

            OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_CHANGE_MOTTO);

            if (Session.GetHabbo().InRoom)
            {
                var Room = Session.GetHabbo().CurrentRoom;

                if (Room == null)
                {
                    return;
                }

                var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                if (User == null)
                {
                    return;
                }

                var RoomUpdate = new ServerMessage(Outgoing.UpdateUserInformation);
                RoomUpdate.AppendInt32(User.VirtualId);
                RoomUpdate.AppendString(Session.GetHabbo().Look);
                RoomUpdate.AppendString(Session.GetHabbo().Gender.ToLower());
                RoomUpdate.AppendString(Session.GetHabbo().Motto);
                RoomUpdate.AppendUInt(Session.GetHabbo().AchievementPoints);
                Room.SendMessage(RoomUpdate);
            }

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_Motto", 1);
        }

        internal void GetWardrobe()
        {
            if (Session.GetHabbo().WardrobeLoaded == false)
                Session.GetHabbo()._LoadWardrobe();

            GetResponse().Init(Outgoing.WardrobeData);
            GetResponse().AppendInt32(1);
            GetResponse().AppendInt32(Session.GetHabbo().wardrobes.Count);
            foreach (var wardrob in Session.GetHabbo().wardrobes.Values)
            {
                GetResponse().AppendInt32(wardrob.slotId);
                GetResponse().AppendString(wardrob.look);
                GetResponse().AppendString(wardrob.gender);
            }
            SendResponse();
        }
    
        internal void SaveWardrobe()
        {
            var SlotId = Request.PopWiredInt32();
            var Look = Request.PopFixedString();
            var Gender = Request.PopFixedString().ToUpper();

            if (!AntiMutant.ValidateLook(Look, Gender, Session.GetHabbo()))
            {
                return;
            }

            if (Session.GetHabbo().wardrobes.ContainsKey(SlotId))
            {
                Session.GetHabbo().wardrobes[SlotId].look = Look;
                Session.GetHabbo().wardrobes[SlotId].gender = Gender;
                Session.GetHabbo().wardrobes[SlotId].needInsert = true;
            }
            else
            {
                var wardrobe = new Wardrobe(SlotId, Look, Gender) {needInsert = true};

                Session.GetHabbo().wardrobes.Add(SlotId, wardrobe);
            }
        }

        internal void GetPetsInventory()
        {
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
        }

        internal void GetBotsInventory()
        {
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
        }

        internal void CheckNameChange()
        {
                if (Session.GetHabbo() == null || Session == null)
                    return;

                var HabboName = Request.PopFixedString();

                GetResponse().Init(Outgoing.CheckName);
                switch (nameAvailable(HabboName))
                {
                    case -1:
                        GetResponse().AppendInt32(4);
                        GetResponse().AppendString(HabboName);
                        GetResponse().AppendInt32(0);
                        break;

                    case -2:
                        GetResponse().AppendInt32(4);
                        GetResponse().AppendString(HabboName);
                        GetResponse().AppendInt32(0);
                        break;

                    case 0:
                        GetResponse().AppendInt32(5);
                        GetResponse().AppendString(HabboName);
                        GetResponse().AppendInt32(3); // To check
                        GetResponse().AppendString("Jefa" + HabboName);
                        GetResponse().AppendString("Con" + HabboName);
                        GetResponse().AppendString(HabboName + "Risa");
                        break;

                    default:
                        GetResponse().AppendInt32(0);
                        GetResponse().AppendString(HabboName);
                        GetResponse().AppendInt32(0);
                        break;
                }
                SendResponse();
        }

        private static int nameAvailable(string username)
        {
            username = username.ToLower();
            if (username.Length > 10)
            {
                return -2;
            }

            if (username.StartsWith("mod-") || username.StartsWith("adm-") || username.StartsWith("bot-") || username.Contains("admin") || !(OtanixEnvironment.IsValidAlphaNumeric(username)))
            {
                return -1;
            }

            return UsersCache.getIdByUsername(username) != 0 ? 0 : 1;
        }

        internal void SaveNameChange()
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;
            var HabboName = Request.PopFixedString();

            if (Session.GetHabbo().NameChanges >= EmuSettings.MAX_NAME_CHANGES)
            {
                Session.SendNotif("Has superado el máximo de cambios de nombre.");
                return;
            }
            if (HabboName == Session.GetHabbo().Username)
            {
                Session.GetHabbo().NameChanges += 1;
                //using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().getQueryreactor())
                //{
                //    dbClient.runFastQuery("UPDATE users SET namechanges = " + Session.GetHabbo().NameChanges + " WHERE id = " + Session.GetHabbo().Id);
                //}
                var MyNameUnchanged = new ServerMessage(Outgoing.GetName);
                MyNameUnchanged.AppendInt32(0);
                MyNameUnchanged.AppendString(HabboName);
                MyNameUnchanged.AppendInt32(0);
                Session.SendMessage(MyNameUnchanged);
                return;
            }

            if (nameAvailable(HabboName)!=1) // Algún tipo de error.
            {
                Session.SendNotif("Ocurrió algún error al procesar la petición.");
                return;
            }

            Session.GetHabbo().NameChanges += 1;
            //using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().getQueryreactor())
            //{
            //    dbClient.runFastQuery("UPDATE users SET namechanges = " + Session.GetHabbo().NameChanges + " WHERE id = " + Session.GetHabbo().Id);
            //}

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("UPDATE rooms SET owner = @newname WHERE owner = @oldname");
                dbClient.addParameter("newname", HabboName);
                dbClient.addParameter("oldname", Session.GetHabbo().Username);
                dbClient.runQuery();

                dbClient.setQuery("UPDATE users SET username = @newname WHERE id = @userid");
                dbClient.addParameter("newname", HabboName);
                dbClient.addParameter("userid", Session.GetHabbo().Id);
                dbClient.runQuery();
            }

            OtanixEnvironment.GetGame().GetClientManager().ChangeUsernameInUsernameRegisterUserName(Session.GetHabbo().Username, HabboName);
            Session.GetHabbo().Username = HabboName;

            foreach (uint roomId in Session.GetHabbo().UsersRooms)
            {
                RoomData data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
                if(data != null)
                    data.Owner = HabboName;
            }

            var MyName = new ServerMessage(Outgoing.GetName);
            MyName.AppendInt32(0);
            MyName.AppendString(HabboName);
            MyName.AppendInt32(0);
            Session.SendMessage(MyName);

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            var MyAllName = new ServerMessage(Outgoing.ChangeUserListName);
            MyAllName.AppendInt32((int)Room.Id);
            MyAllName.AppendInt32((int)User.VirtualId);
            MyAllName.AppendString(HabboName);
            Room.SendMessage(MyAllName);


            /* var MyAllName = new ServerMessage(Outgoing.ChangeUserListName);
             MyAllName.AppendInt32((int)Session.GetHabbo().Id);
             MyAllName.AppendUInt(Session.GetHabbo().CurrentRoomId);
             MyAllName.AppendString(HabboName);
             Room.SendMessage(MyAllName);
             
            Essa versão comentada era a do CarlosD (tava errado as infos q ele estava passando)
             */

            if (Session.GetHabbo().Id == Room.RoomData.OwnerId)
            {
                var Update = new ServerMessage(Outgoing.UpdateRoom);
                Update.AppendInt32((int)Room.RoomId);
                Room.SendMessage(Update);
            }

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_Name", 1);
        }

        internal void AddRelation()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetRelationshipComposer() == null)
                return;

            var UserId = Request.PopWiredUInt();
            var RelationState = Request.PopWiredInt32();

            // Si intentan inyectar al mismo usuario.
            if (Session.GetHabbo().Id == UserId)
                return;

            if (Session.GetHabbo().GetRelationshipComposer().LoveRelation.ContainsKey(UserId))
                Session.GetHabbo().GetRelationshipComposer().LoveRelation.Remove(UserId);

            if (Session.GetHabbo().GetRelationshipComposer().FriendRelation.ContainsKey(UserId))
                Session.GetHabbo().GetRelationshipComposer().FriendRelation.Remove(UserId);

            if (Session.GetHabbo().GetRelationshipComposer().DieRelation.ContainsKey(UserId))
                Session.GetHabbo().GetRelationshipComposer().DieRelation.Remove(UserId);

            if (RelationState == 0)
            {
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("DELETE FROM user_relationships WHERE user_id = @userId AND member_id = @memberId");
                    dbClient.addParameter("userId", Session.GetHabbo().Id);
                    dbClient.addParameter("memberId", UserId);
                    dbClient.runQuery();
                }
            }
            else
            {
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("REPLACE INTO user_relationships VALUES (@userId, @memberId, @relationType)");
                    dbClient.addParameter("userId", Session.GetHabbo().Id);
                    dbClient.addParameter("memberId", UserId);
                    dbClient.addParameter("relationType", RelationState);
                    dbClient.runQuery();
                }

                var rel = new Relationship(UserId, RelationState);

                if (RelationState == 1)
                    Session.GetHabbo().GetRelationshipComposer().LoveRelation.Add(UserId, rel);
                else if (RelationState == 2)
                    Session.GetHabbo().GetRelationshipComposer().FriendRelation.Add(UserId, rel);
                else if (RelationState == 3)
                    Session.GetHabbo().GetRelationshipComposer().DieRelation.Add(UserId, rel);
            }

            var zUser = UsersCache.getHabboCache(UserId);
            if (zUser == null)
                return;

            ServerMessage Message = new ServerMessage(Outgoing.UpdateRelations);
            Message.AppendInt32(0); // Array con contenido (int|string)
            Message.AppendInt32(1); // Array de la acción.
            Message.AppendInt32(0); // ??
            Message.AppendUInt(zUser.Id);
            Message.AppendString(zUser.Username);
            Message.AppendInt32(1); // ??
            Message.AppendBoolean(OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId) != null);
            Message.AppendBoolean(zUser.CurrentRoom != null);
            Message.AppendString(zUser.Look);
            Message.AppendInt32(0);
            Message.AppendString(zUser.Motto);
            Message.AppendString("");
            Message.AppendString(zUser.RealName);
            Message.AppendBoolean(true);
            Message.AppendBoolean(true);
            Message.AppendBoolean(false); // pocket
            Message.AppendShort(RelationState);
            Session.SendMessage(Message);
        }

        internal void SerializeRelation()
        {
            var UserId = Request.PopWiredUInt();
            var zUser = UsersCache.getHabboCache(UserId);
            if (zUser == null || zUser.GetRelationshipComposer() == null)
                return;

            if (OtanixEnvironment.GetGame().GetPromotionalBadges().promotional_badges.ContainsKey(zUser.Username))
            {
                var placaAEntregar = "";
                OtanixEnvironment.GetGame().GetPromotionalBadges().promotional_badges.TryGetValue(zUser.Username, out placaAEntregar);
                if (placaAEntregar != "" && Session.GetHabbo().GetBadgeComponent() != null && (Session.GetHabbo().CurrentRoomId == zUser.CurrentRoomId) && Session.GetHabbo().CurrentRoomId > 0)
                {
                    if (Session.GetHabbo().GetBadgeComponent().HasBadge(placaAEntregar) == false)
                    {
                        Session.GetHabbo().GiveUserDiamonds(4);
                        Session.GetHabbo().GetBadgeComponent().GiveBadge(placaAEntregar);
                        Session.SendNotif("¡¡Uffa!! Encotrou o(a) " + zUser.Username + ", que sorte!! Receba este emblema e 4 diamantes para comemorar.");
                    }
                }
            }

            var Message = new ServerMessage(Outgoing.SerializeRelations);
            Message.AppendUInt(UserId);
            Message.AppendInt32(((zUser.GetRelationshipComposer().LoveRelation.Count > 0) ? 1 : 0) + ((zUser.GetRelationshipComposer().FriendRelation.Count > 0) ? 1 : 0) + ((zUser.GetRelationshipComposer().DieRelation.Count > 0) ? 1 : 0));
            if (zUser.GetRelationshipComposer().LoveRelation.Count > 0)
            {
                Relationship member = zUser.GetRelationshipComposer().LoveRelation.ElementAt(new Random().Next(0, zUser.GetRelationshipComposer().LoveRelation.Count - 1)).Value;
                Habbo _User = UsersCache.getHabboCache(member.MemberId);
                if (_User == null)
                    return;

                Message.AppendInt32(1);
                Message.AppendInt32(zUser.GetRelationshipComposer().LoveRelation.Count);
                Message.AppendUInt(_User.Id);
                Message.AppendString(_User.Username);
                Message.AppendString(_User.Look);
            }

            if (zUser.GetRelationshipComposer().FriendRelation.Count > 0)
            {
                Relationship member = zUser.GetRelationshipComposer().FriendRelation.ElementAt(new Random().Next(0, zUser.GetRelationshipComposer().FriendRelation.Count - 1)).Value;
                Habbo _User = UsersCache.getHabboCache(member.MemberId);
                if (_User == null)
                    return;

                Message.AppendInt32(2);
                Message.AppendInt32(zUser.GetRelationshipComposer().FriendRelation.Count);
                Message.AppendUInt(_User.Id);
                Message.AppendString(_User.Username);
                Message.AppendString(_User.Look);
            }

            if (zUser.GetRelationshipComposer().DieRelation.Count > 0)
            {
                Relationship member = zUser.GetRelationshipComposer().DieRelation.ElementAt(new Random().Next(0, zUser.GetRelationshipComposer().DieRelation.Count - 1)).Value;
                Habbo _User = UsersCache.getHabboCache(member.MemberId);
                if (_User == null)
                    return;

                Message.AppendInt32(3);
                Message.AppendInt32(zUser.GetRelationshipComposer().DieRelation.Count);
                Message.AppendUInt(_User.Id);
                Message.AppendString(_User.Username);
                Message.AppendString(_User.Look);
            }

            Session.SendMessage(Message);
        }

        internal void SaveVolumen()
        {
            var SystemVolumen = Request.PopWiredInt32();
            var FurniVolumen = Request.PopWiredInt32();
            var TraxVolumen = Request.PopWiredInt32();

            if (SystemVolumen < 0 || SystemVolumen > 100 || FurniVolumen < 0 || FurniVolumen > 100 || TraxVolumen < 0 || TraxVolumen > 100)
                return;

            Session.GetHabbo().volumenSystem = SystemVolumen + ";" + FurniVolumen + ";" + TraxVolumen;
        }

        internal void PreferOldChat()
        {
            Session.GetHabbo().preferOldChat = Request.PopWiredBoolean();
        }

        internal void IgnoreRoomInvitation()
        {
            Session.GetHabbo().IgnoreRoomInvitations = Request.PopWiredBoolean();
        }

        internal void StartHabboQuiz()
        {
            string HabboType = Request.PopFixedString();
            if (HabboType == "HabboWay1")
            {
                Session.GetHabbo().HabboQuizQuestions = new List<int>(5);

                Session.GetMessageHandler().GetResponse().Init(Outgoing.InitHabboQuiz);
                Session.GetMessageHandler().GetResponse().AppendString(HabboType);
                Session.GetMessageHandler().GetResponse().AppendInt32(5); // longitud.
                for (int i = 0; i < 5; i++)
                {
                    int rndNumber = new Random().Next(10);
                    if (Session.GetHabbo().HabboQuizQuestions.Contains(rndNumber))
                    {
                        for (int ii = 0; ii < 10; ii++)
                        {
                            if (!Session.GetHabbo().HabboQuizQuestions.Contains(ii))
                            {
                                rndNumber = ii;
                                break;
                            }
                        }
                    }
                    Session.GetHabbo().HabboQuizQuestions.Add(rndNumber);
                    Session.GetMessageHandler().GetResponse().AppendInt32(rndNumber);
                }
                Session.GetMessageHandler().SendResponse();
            }
            else if (HabboType == "SafetyQuiz1")
            {
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SafetyQuizGraduate", 1);

                if (Session.GetHabbo().CitizenshipLevel == 0)
                    OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "citizenship");
            }
        }

        internal void FinishHabboQuiz()
        {
            string HabboType = Request.PopFixedString();
            if (HabboType != "HabboWay1")
                return;

            int HabboQuestions = Request.PopWiredInt32();
            List<int> errors = new List<int>(5);

            Session.GetMessageHandler().GetResponse().Init(Outgoing.FinishHabboQuiz);
            Session.GetMessageHandler().GetResponse().AppendString(HabboType);
            for (int i = 0; i < HabboQuestions; i++)
            {
                int QuestionId = Session.GetHabbo().HabboQuizQuestions[i];
                int respuesta = Request.PopWiredInt32();
                if (!Quiz.CorrectAnswer(QuestionId, respuesta))
                {
                    errors.Add(QuestionId);
                }
            }
            Session.GetMessageHandler().GetResponse().AppendInt32(errors.Count);
            foreach (int error in errors)
            {
                Session.GetMessageHandler().GetResponse().AppendInt32(error);
            }
            Session.GetMessageHandler().SendResponse();

            if (errors.Count == 0)
            {
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_HabboWayGraduate", 1);

                if (Session.GetHabbo().CitizenshipLevel == 3)
                    OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "citizenship");
                else if (Session.GetHabbo().HelperLevel == 3)
                    OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "helper");
            }
        }

        internal void OpenTalents()
        {
            string TalentType = Request.PopFixedString();
            if (TalentType != "citizenship" && TalentType  != "helper")
                return;

            Session.SendMessage(OtanixEnvironment.GetGame().GetTalentManager().Serialize(Session, TalentType));
        }

        internal void OpenHabboAlfa()
        {
            if (!Session.GetHabbo().HasFuse("fuse_alfa_tool"))
                return;

            bool OnService = Request.PopWiredBoolean();
            bool GuideEnabled = Request.PopWiredBoolean();
            bool HelperEnabled = Request.PopWiredBoolean();
            bool GuardianEnabled = Request.PopWiredBoolean();

            TourManager TourManager = OtanixEnvironment.GetGame().GetAlfaManager().GetTourManager();
            HelpManager HelpManager = OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager();
            BullyManager BullyManager = OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager();

            if (OnService)
            {
                if (GuideEnabled)
                {
                    TourManager.AddAlfa(Session.GetHabbo().Id);
                }
                if (HelperEnabled)
                {
                    HelpManager.AddAlfa(Session.GetHabbo().Id);
                }
                if (GuardianEnabled)
                {
                    BullyManager.AddGuardian(Session.GetHabbo().Id);
                }
            }
            else
            {
                TourManager.RemoveAlfa(Session.GetHabbo().Id);
                HelpManager.RemoveAlfa(Session.GetHabbo().Id);
                BullyManager.RemoveGuardian(Session.GetHabbo().Id);

                GuideEnabled = false;
                HelperEnabled = false;
                GuardianEnabled = false;
            }

            Session.GetHabbo().AlfaGuideEnabled = GuideEnabled;
            Session.GetHabbo().AlfaHelperEnabled = HelperEnabled;
            Session.GetHabbo().AlfaGuardianEnabled = GuardianEnabled;

            Session.GetMessageHandler().GetResponse().Init(Outgoing.OpenHabboAlfa);
            Session.GetMessageHandler().GetResponse().AppendBoolean(OnService);
            Session.GetMessageHandler().GetResponse().AppendInt32(OtanixEnvironment.GetGame().GetAlfaManager().GetTourManager().AlfasCount());
            Session.GetMessageHandler().GetResponse().AppendInt32(OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().AlfasCount());
            Session.GetMessageHandler().GetResponse().AppendInt32(OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().GuardianCount());
            Session.GetMessageHandler().SendResponse();
        }

        internal void AcceptOrNotAlfaHelp()
        {
            if (Session.GetHabbo().AlfaHelpEnabled)
                return;

            bool accept = Request.PopWiredBoolean();

            Bully bully = null;
            if(OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().Bullies.ContainsKey(Session.GetHabbo().AlfaServiceId))
                bully = OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().Bullies[Session.GetHabbo().AlfaServiceId];
            
            if (bully != null && bully.bullyState == BullyState.SEARCHING_USER)
            {
                if (accept)
                {
                    Session.GetHabbo().AlfaHelpEnabled = true;
                    Session.GetHabbo().LastAlfaSend = OtanixEnvironment.GetUnixTimestamp();

                    bully.bullyState = BullyState.WAITING_RESPONSE;
                    bully.NeedUpdate = false;
                    bully.customTimer = DateTime.Now;
                    bully.SerializeAlfaChat();

                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_GuideChatReviewer", 1);
                    if (Session.GetHabbo().HelperLevel == 6 || Session.GetHabbo().HelperLevel == 7)
                        OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "helper");
                }
                else
                {
                    bully.NeedUpdate = true;
                }
            }
        }

        internal void AlfaOpinion()
        {
            if (!Session.GetHabbo().AlfaHelpEnabled)
                return;

            int VoteValue = Request.PopWiredInt32();
            if (VoteValue < 0 && VoteValue > 2)
                return;

            Bully bully = null;
            if (OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().Bullies.ContainsKey(Session.GetHabbo().AlfaServiceId))
                bully = OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().Bullies[Session.GetHabbo().AlfaServiceId];

            if (bully != null && bully.bullyState == BullyState.WAITING_RESPONSE)
            {
                if (VoteValue == 0)
                    bully.bullySolution = BullySolution.ACCEPTABLE;
                else if (VoteValue == 1)
                    bully.bullySolution = BullySolution.BULLY;
                else if (VoteValue == 2)
                    bully.bullySolution = BullySolution.HORROR;

                bully.bullyState = BullyState.FINISHED;
            }
        }

        internal void ExitAlfaHelpVotation()
        {
            /*if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().AlfaHelpingIn == null)
                return;

            Session.GetHabbo().AlfaHelpingIn = null;
            Session.GetHabbo().AlfaVotationType = -1;
            Session.GetHabbo().AlfaVotationNeedUpdate = false;
            Session.GetHabbo().AlfaException.Clear();
            Session.GetHabbo().AlfaImHelping = false;*/
        }

        internal void CallForAlfaHelp()
        {
            if (OtanixEnvironment.GetUnixTimestamp() - Session.GetHabbo().LastAlfaSend < 1200)
            {
                Session.GetMessageHandler().GetResponse().Init(Outgoing.onGuideSessionError);
                Session.GetMessageHandler().GetResponse().AppendInt32(0);
                Session.GetMessageHandler().SendResponse();

                return;
            }

            int helpType = Request.PopWiredInt32();
            string Message = Request.PopFixedString();

            if (helpType == 1)
            {
                if (OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().UserStartedHelp(Session.GetHabbo().Id))
                {
                    Session.GetMessageHandler().GetResponse().Init(Outgoing.ReportAcoso);
                    Session.GetMessageHandler().GetResponse().AppendInt32(1); // length
                    Session.GetMessageHandler().GetResponse().AppendInt32(3); // type: Bully
                    Session.GetMessageHandler().GetResponse().AppendInt32(0); // timer sec
                    Session.GetMessageHandler().GetResponse().AppendBoolean(true); // false = usuario, true = null
                    Session.GetMessageHandler().SendResponse();

                    return;
                }

                Help help = new Help(Session.GetHabbo().Id, Message);
                if (!help.SearchAlfa())
                {
                    help.SerializeNoAlfas();
                    return;
                }

                OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().AddHelp(help);
            }

            Session.GetMessageHandler().GetResponse().Init(Outgoing.onGuideSessionAttached);
            Session.GetMessageHandler().GetResponse().AppendBoolean(false); // true = tour - false = help
            Session.GetMessageHandler().GetResponse().AppendInt32(helpType);
            Session.GetMessageHandler().GetResponse().AppendString(Message);
            Session.GetMessageHandler().GetResponse().AppendInt32(15); // seconds
            Session.GetMessageHandler().SendResponse();

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_GuideRequester", 1);
            if (Session.GetHabbo().HelperLevel == 2)
                OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "helper");
        }

        internal void ResponseAlfaHelp()
        {
            bool accept = Request.PopWiredBoolean();

            Help help = null;
            if (OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps.ContainsKey(Session.GetHabbo().AlfaServiceId))
                help = OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps[Session.GetHabbo().AlfaServiceId];

            if (help != null && help.helpState == HelpState.SEARCHING_USER)
            {
                if (accept)
                {
                    Session.GetHabbo().AlfaHelpEnabled = true;
                    Session.GetHabbo().LastAlfaSend = OtanixEnvironment.GetUnixTimestamp();

                    help.helpState = HelpState.TALKING;
                    help.NeedUpdate = false;
                    help.customTimer = DateTime.Now;
                    help.SerializeAlfaChat();

                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_GuideRequestHandler", 1);
                    if (Session.GetHabbo().HelperLevel == 3 || Session.GetHabbo().HelperLevel == 4 || Session.GetHabbo().HelperLevel == 5 || Session.GetHabbo().HelperLevel == 6 || Session.GetHabbo().HelperLevel == 7)
                        OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "helper");
                }
                else
                {
                    help.NeedUpdate = true;
                }
            }
        }

        internal void AlfaHelpChat()
        {
            string chatMessage = Request.PopFixedString();
            uint helpId = (Session.GetHabbo().AlfaServiceId > 0) ? Session.GetHabbo().AlfaServiceId : Session.GetHabbo().Id;

            Help help = null;
            if (OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps.ContainsKey(helpId))
                help = OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps[helpId];

            if (help != null && help.helpState == HelpState.TALKING)
            {
                help.AddChatMessage(new HelpChatMessage(true, Session.GetHabbo().Id, chatMessage, 0));
            }
        }

        internal void IsWrittingAlfa()
        {
            bool writting = Request.PopWiredBoolean();
            uint helpId = (Session.GetHabbo().AlfaServiceId > 0) ? Session.GetHabbo().AlfaServiceId : Session.GetHabbo().Id;

            Help help = null;
            if (OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps.ContainsKey(helpId))
                help = OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps[helpId];

            if (help != null && help.helpState == HelpState.TALKING)
            {
                ServerMessage onGuideSessionPartnerIsTyping = new ServerMessage(Outgoing.onGuideSessionPartnerIsTyping);
                onGuideSessionPartnerIsTyping.AppendBoolean(writting);
                if (help.ReporterId == Session.GetHabbo().Id && help.Alfa != null)
                    help.Alfa.SendMessage(onGuideSessionPartnerIsTyping);
                else if(help.HelperId == Session.GetHabbo().Id && help.Reporter != null)
                    help.Reporter.SendMessage(onGuideSessionPartnerIsTyping);
            }
        }

        internal void CloseAlfaLink()
        {
            uint helpId = (Session.GetHabbo().AlfaServiceId > 0) ? Session.GetHabbo().AlfaServiceId : Session.GetHabbo().Id;

            Help help = null;
            if (OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps.ContainsKey(helpId))
                help = OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps[helpId];

            if (help != null && help.helpState == HelpState.TALKING)
            {
                help.helpState = HelpState.FINISHED;
                if (help.Reporter != null)
                    help.Reporter.GetHabbo().HabboAlfaUserId = help.HelperId;

                ServerMessage onGuideSessionEnded = new ServerMessage(Outgoing.onGuideSessionEnded);
                onGuideSessionEnded.AppendInt32(1);
                help.sendMessage(onGuideSessionEnded);
            }
        }

        internal void RecomendHelpers()
        {
            if (Session.GetHabbo().HabboAlfaUserId == 0)
            {
                Session.GetMessageHandler().GetResponse().Init(Outgoing.onGuideSessionDetached);
                Session.GetMessageHandler().SendResponse();

                return;
            }

            bool recomend = Request.PopWiredBoolean();

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                if (recomend)
                    dbClient.runFastQuery("INSERT INTO user_alfas_opinion (user_id, likes) VALUES (" + Session.GetHabbo().HabboAlfaUserId + ", 1) ON DUPLICATE KEY UPDATE likes = likes + 1");
                else
                    dbClient.runFastQuery("INSERT INTO user_alfas_opinion (user_id, unlikes) VALUES (" + Session.GetHabbo().HabboAlfaUserId + ", 1) ON DUPLICATE KEY UPDATE unlikes = unlikes + 1");
            }

            Session.GetMessageHandler().GetResponse().Init(Outgoing.onGuideSessionDetached);
            Session.GetMessageHandler().SendResponse();

            GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Session.GetHabbo().HabboAlfaUserId);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_GuideFeedbackGiver", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().HabboAlfaUserId, "ACH_GuideRecommendation", 1);

            if (Session.GetHabbo().HelperLevel == 2)
                OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "helper");

            if (client != null)
                if (client.GetHabbo().HelperLevel == 4 || client.GetHabbo().HelperLevel == 5 || client.GetHabbo().HelperLevel == 6 || client.GetHabbo().HelperLevel == 7)
                    OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(client, "helper");

            Session.GetHabbo().HabboAlfaUserId = 0;
        }

        internal void CancelAlfaHelp()
        {
            Help help = null;
            if (OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps.ContainsKey(Session.GetHabbo().Id))
                help = OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps[Session.GetHabbo().Id];

            if (help != null)
            {
                help.helpState = HelpState.FINISHED;

                ServerMessage message = new ServerMessage(Outgoing.onGuideSessionDetached);
                help.sendMessage(message);
            }
        }

        internal void AlfaChatVisit()
        {
            Help help = null;
            if (OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps.ContainsKey(Session.GetHabbo().AlfaServiceId))
                help = OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps[Session.GetHabbo().AlfaServiceId];

            if(help == null || help.Reporter == null || help.Reporter.GetHabbo() == null)
                return;

            var Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(help.Reporter.GetHabbo().CurrentRoomId);
            if (Room == null)
                return;

            Session.GetMessageHandler().enterOnRoom3(Room);
        }

        internal void AlfaChatInvite()
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || Room.RoomData == null)
                return;

            Help help = null;
            if (OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps.ContainsKey(Session.GetHabbo().AlfaServiceId))
                help = OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps[Session.GetHabbo().AlfaServiceId];

            if (help != null && help.helpState == HelpState.TALKING)
            {
                help.AddChatMessage(new HelpChatMessage(false, Session.GetHabbo().Id, Room.RoomData.Name, Room.Id));
            }
        }

        internal void EnableFocusUser()
        {
            Session.GetHabbo().DontFocusUser = Request.PopWiredBoolean();
        }

        internal void AlertaEmbajador()
        {
            uint HabboId = Request.PopWiredUInt();

            GameClient User = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(HabboId);
            if (User == null || User.GetHabbo() == null)
                return;

            if (User.GetHabbo().Rank >= Session.GetHabbo().Rank)
                return;

            /*if((OtanixEnvironment.GetUnixTimestamp() - User.GetHabbo().LastAmbassadorNotif) < 60)
            {
                Session.SendNotif("Este usuario ya ha sido avisado por un embajador en el último minuto.");
                return;
            }*/

            ServerMessage serverAlert = new ServerMessage(Outgoing.GeneratingNotification);
            serverAlert.AppendString("info." + EmuSettings.HOTEL_LINK);
            serverAlert.AppendInt32(5);
            serverAlert.AppendString("image");
            serverAlert.AppendString(LanguageLocale.GetValue("ambassador.image"));
            serverAlert.AppendString("title");
            serverAlert.AppendString("${notification.ambassador.alert.warning.title}");
            serverAlert.AppendString("message");
            serverAlert.AppendString("${notification.ambassador.alert.warning.message}");
            serverAlert.AppendString("linkTitle");
            serverAlert.AppendString("¡Entendido!");
            serverAlert.AppendString("linkUrl");
            serverAlert.AppendString("event:");
            User.SendMessage(serverAlert);
        }
 
        internal void ShowNewUserInformation()
        {
            if (Session.GetHabbo().NewIdentity == 0)
                return;

            Session.SendMessage(NuxUserInformation.ShowInformation(Session.GetHabbo().NewUserInformationStep));
            Session.GetHabbo().NewUserInformationStep++;

            if (Session.GetHabbo().NewUserInformationStep == NuxUserInformation.NewUserInformation.Length)
            {
                Session.SendMessage(OtanixEnvironment.GetGame().GetGiftManager().Serialize());
            }
        }
        internal void NuxMsgseiLaOq()
        {
            uint HabboId = Request.PopWiredUInt();

            Session.SendNotif(HabboId.ToString());
            return;
        }
        internal void GetNuxPresentEvent()
        {
            int idk1 = Request.PopWiredInt32(); // n sei oq é
            int idk2 = Request.PopWiredInt32(); // n sei oq é
            int idk3 = Request.PopWiredInt32(); // n sei oq é
            uint itemSelecionado = Request.PopWiredUInt(); // identificador do item selecionado ( 0, 1 ou 2)

            OtanixEnvironment.GetGame().GetGiftManager().deliverItem(itemSelecionado, Session);
            return;
        }
        internal void SMSVerificar()
        {
            string senhaDigitada = Request.PopFixedString();

            if (Session.GetHabbo().tentativasLogin++ > 3)
            {
                Session.SendNotif("Você errou a senha muitas vezes");
                Session.Disconnect();
            }           

            if (senhaDigitada.ToLower() != EmuSettings.PIN_CLIENTE.ToLower())
            {

                Session.SendWindowManagerAlert("Você digitou a senha incorretamente.");

                ServerMessage passouSucesso = new ServerMessage(Outgoing.MobilePhoneNumero);
                passouSucesso.AppendInt32(1);
                passouSucesso.AppendInt32(1);
                Session.SendMessage(passouSucesso);
                Session.GetHabbo().tentativasLogin++;
            }
            else
            {
                ServerMessage error = new ServerMessage(Outgoing.SMSerroRetorno);
                error.AppendInt32(2);
                error.AppendInt32(2);
                Session.SendMessage(error);

                ServerMessage passouSucesso = new ServerMessage(Outgoing.MobilePhoneNumero);
                passouSucesso.AppendInt32(-1);
                passouSucesso.AppendInt32(-1);
                Session.SendMessage(passouSucesso);

                if (Session.GetHabbo().HasFuse("fuse_mod"))
                {
                    Session.SendMessage(OtanixEnvironment.GetGame().GetModerationTool().SerializeTool(Session.GetHabbo()));
                }

                Session.SendWindowManagerAlert("Digitou a senha corretamente!");
                Session.GetHabbo().passouPin = true;
             }

            return;
        }
    }
}
