using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Navigators;
using Butterfly.HabboHotel.Navigators.Bonus;
using Butterfly.HabboHotel.Navigators.RoomQueue;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Users.Navigator;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections.Generic;

namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        internal void AddFavorite()
        {
            var Id = Request.PopWiredUInt();

            var Data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(Id);

            if (Data == null || Session.GetHabbo().FavoriteRooms.Count >= 30 || Session.GetHabbo().FavoriteRooms.Contains(Id))
            {
                return;
            }

            GetResponse().Init(Outgoing.FavsUpdate);
            GetResponse().AppendUInt(Id);
            GetResponse().AppendBoolean(true);
            SendResponse();

            Session.GetHabbo().FavoriteRooms.Add(Id);

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("INSERT INTO user_favorites (user_id,room_id) VALUES (" + Session.GetHabbo().Id + "," + Id + ")");
            }
        }

        internal void RemoveFavorite()
        {
            uint Id = Request.PopWiredUInt();

            if (!Session.GetHabbo().FavoriteRooms.Contains(Id))
                return;

            Session.GetHabbo().FavoriteRooms.Remove(Id);

            GetResponse().Init(Outgoing.FavsUpdate);
            GetResponse().AppendUInt(Id);
            GetResponse().AppendBoolean(false);
            SendResponse();

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM user_favorites WHERE user_id = " + Session.GetHabbo().Id + " AND room_id = " + Id + "");
            }
        }

        internal void AddRoomToSelectionStaff()
        {
            if (!Session.GetHabbo().HasFuse("fuse_add_room_staff"))
                return;

            //if (!OtanixEnvironment.GetGame().GetNewNavigatorManager().CategoryExists(EmuSettings.NAVIGATOR_STAFF_SELECTION))
            //{
            //    Session.SendNotif(LanguageLocale.GetValue("navigator.contains.selection.staff").Replace("{0}", EmuSettings.NAVIGATOR_STAFF_SELECTION));
            //    return;
            //}

            uint RoomId = Request.PopWiredUInt();
            RoomData Data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            if (Data == null)
                return;

            bool RemoveRoom = Request.PopWiredBoolean();

            if(RemoveRoom == false)
            {
                if(OtanixEnvironment.GetGame().GetNewNavigatorManager().GetRoomsInCategory(EmuSettings.NAVIGATOR_STAFF_SELECTION).Contains(RoomId))
                {
                    Session.SendNotif(LanguageLocale.GetValue("navigator.contains.room.selection.staff"));
                    return;
                }

                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("INSERT INTO navigator_new_selections VALUES (@id, '" + RoomId + "')");
                    dbClient.addParameter("id", EmuSettings.NAVIGATOR_STAFF_SELECTION);
                    dbClient.runQuery();
                }

                OtanixEnvironment.GetGame().GetNewNavigatorManager().AddRoomToCategory(EmuSettings.NAVIGATOR_STAFF_SELECTION, RoomId);

                Data.Type = "public";
                Data.roomNeedSqlUpdate = true;

                GameClient User = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID((UInt32)Data.OwnerId);
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement((UInt32)Data.OwnerId, "ACH_Spr", 1);

                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "-Selection Staff-", "Added this room:" + RoomId);
            }
            else
            {
                if (!OtanixEnvironment.GetGame().GetNewNavigatorManager().GetRoomsInCategory(EmuSettings.NAVIGATOR_STAFF_SELECTION).Contains(RoomId))
                {
                    Session.SendNotif(LanguageLocale.GetValue("navigator.no.contains.room.selection.staff"));
                    return;
                }

                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("DELETE FROM navigator_new_selections WHERE room_id = '" + RoomId + "'");
                }

                OtanixEnvironment.GetGame().GetNewNavigatorManager().RemoveRoomToCategory(EmuSettings.NAVIGATOR_STAFF_SELECTION, RoomId);

                Data.Type = "private";
                Data.roomNeedSqlUpdate = true;

                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "-Selection Staff-", "Removed this room:" + RoomId);
            }

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            if (Room != null)
            {
                ServerMessage Update = new ServerMessage(Outgoing.UpdateRoom);
                Update.AppendUInt(Room.Id);
                Room.SendMessage(Update);
            }
        }

        internal void GoToHotelView()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            if (Session.GetHabbo().roomIdQueue > 0)
                {
                    RoomQueue rQueue = OtanixEnvironment.GetGame().GetRoomQueueManager().GetRoomQueue(Session.GetHabbo().roomIdQueue);
                    if (rQueue != null)
                    {
                        rQueue.RemoveUserToQueue(Session.GetHabbo().Id);
                    }

                    Session.GetHabbo().roomIdQueue = 0;

                    Session.GetMessageHandler().GetResponse().Init(Outgoing.OutOfRoom);
                    Session.GetMessageHandler().SendResponse();

                    return;
                }

                if (Session.GetHabbo().InRoom)
                {
                    var currentRoom = Session.GetHabbo().CurrentRoom;
                    if (currentRoom != null && currentRoom.GetRoomUserManager() != null)
                        currentRoom.GetRoomUserManager().RemoveUserFromRoom(Session, true, false);
                }
                else
                {
                    Session.GetMessageHandler().GetResponse().Init(Outgoing.OutOfRoom);
                    Session.GetMessageHandler().SendResponse();
                }
            Session.SendMessage(BonusManager.GenerateMessage(Session));
        }

        internal void LoadNews()
        {
            if (Session == null)
                return;

            Session.SendMessage(OtanixEnvironment.GetGame().GetNewsManager().getCachedMessage());
        }

        internal void GetFlatCats()
        {
            if (Session == null)
                return;

            Session.SendMessage(OtanixEnvironment.GetGame().GetNavigator().SerializeFlatCategories(Session));
            Session.SendMessage(OtanixEnvironment.GetGame().GetNavigator().SerializePromotionsCategories());
        }

        internal void EnableNewNavigator()
        {
            if (Session == null)
                return;

            OtanixEnvironment.GetGame().GetNewNavigatorManager().SerializeNewNavigator(Session);
        }

        internal void NewNavigatorPacket()
        {
            if (Session == null)
                return;

            string name = Request.PopFixedString();
            string textbox = Request.PopFixedString();
            Session.SendMessage(OtanixEnvironment.GetGame().GetNewNavigatorManager().SerlializeNewNavigator(name, textbox, Session));
        }

        internal void SaveNavigatorSearch()
        {
            if (Session.GetHabbo().navigatorLogs.Count > 50)
            {
                Session.SendNotif("Has llegado al límite permitido de guardados.");
                return;
            }

            string value1 = Request.PopFixedString();
            string value2 = Request.PopFixedString();

            NaviLogs naviLogs = new NaviLogs(Session.GetHabbo().navigatorLogs.Count, value1, value2);

            if (!Session.GetHabbo().navigatorLogs.ContainsKey(naviLogs.Id))
                Session.GetHabbo().navigatorLogs.Add(naviLogs.Id, naviLogs);

            ServerMessage Message = new ServerMessage(Outgoing.NavigatorSavedSearchesParser);
            Message.AppendInt32(Session.GetHabbo().navigatorLogs.Count);
            foreach (NaviLogs navi in Session.GetHabbo().navigatorLogs.Values)
            {
                Message.AppendInt32(navi.Id);
                Message.AppendString(navi.Value1); // searchCode
                Message.AppendString(navi.Value2); // filter
                Message.AppendString(""); // localization
            }
            Session.SendMessage(Message);
        }

        internal void DeleteNavigatorSearch()
        {
            int searchId = Request.PopWiredInt32();

            if (Session.GetHabbo().navigatorLogs.ContainsKey(searchId))
            {
                Session.GetHabbo().navigatorLogs.Remove(searchId);

                ServerMessage Message = new ServerMessage(Outgoing.NavigatorSavedSearchesParser);
                Message.AppendInt32(Session.GetHabbo().navigatorLogs.Count);
                foreach (NaviLogs navi in Session.GetHabbo().navigatorLogs.Values)
                {
                    Message.AppendInt32(navi.Id);
                    Message.AppendString(navi.Value1); // searchCode
                    Message.AppendString(navi.Value2); // filter
                    Message.AppendString(""); // localization
                }
                Session.SendMessage(Message);
            }
        }

        internal void GoRandomRoom()
        {
            string text = Request.PopFixedString();

            if (text.Equals("random_friending_room") || text.Equals("predefined_noob_lobby"))
            {
                RoomData room = OtanixEnvironment.GetGame().GetRoomManager().GetRandomActivePopularRoom();
                if (room != null)
                {
                    Room roomLoaded = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(room.Id);
                    if (roomLoaded != null)
                    {
                        Session.GetMessageHandler().enterOnRoom3(roomLoaded);
                    }
                }
            }
        }
    }
}