using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Group;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Navigators
{
    class SearchResultList
    {
        internal static void SerializeNewNavigatorType(string SubName, string SearchQuery, string Title, int ViewMode, bool Collapsed, GameClient Session, ServerMessage Message)
        {
            List<RoomData> provisionalRooms = new List<RoomData>();

            Message.AppendString(SubName); // tittle
            Message.AppendString(Title); // text
            Message.AppendInt32(0); // actionAllowed (0/1/2)
            Message.AppendBoolean(Collapsed); // ??
            Message.AppendInt32(ViewMode); // viewMode

            switch (SubName)
            {
                case "my":
                    {
                        foreach (uint roomId in Session.GetHabbo().UsersRooms)
                        {
                            RoomData data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);

                            if (data != null)
                                provisionalRooms.Add(data);

                            if (provisionalRooms.Count >= EmuSettings.ROOMS_X_PESTAÑA)
                                break;
                        }

                        break;
                    }

                case "favorites":
                    {
                        foreach (int roomId in Session.GetHabbo().FavoriteRooms)
                        {
                            RoomData data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData((uint)roomId);
                            if (data != null)
                                provisionalRooms.Add(data);

                            if (provisionalRooms.Count >= EmuSettings.ROOMS_X_PESTAÑA)
                                break;
                        }

                        break;
                    }

                case "my_groups":
                    {
                        List<uint> GroupsToRemove = new List<uint>();
                        foreach (uint group in Session.GetHabbo().MyGroups)
                        {
                            GroupItem xGroup = OtanixEnvironment.GetGame().GetGroup().LoadGroup(group);
                            if (xGroup == null)
                            {
                                GroupsToRemove.Add(group);
                            }
                            else
                            {
                                RoomData data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(xGroup.RoomId);
                                if (data != null)
                                    provisionalRooms.Add(data);
                            }

                            if (provisionalRooms.Count >= EmuSettings.ROOMS_X_PESTAÑA)
                                break;
                        }

                        if (GroupsToRemove.Count > 0)
                        {
                            foreach (uint groupId in GroupsToRemove)
                            {
                                Session.GetHabbo().MyGroups.Remove(groupId);
                            }
                        }

                        GroupsToRemove.Clear();
                        GroupsToRemove = null;

                        break;
                    }

                case "history":
                    {
                        foreach (RoomVisits visitId in Session.GetHabbo().RoomsVisited)
                        {
                            RoomData data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(visitId.RoomId);
                            if (data != null)
                                provisionalRooms.Add(data);

                            if (provisionalRooms.Count >= EmuSettings.ROOMS_X_PESTAÑA)
                                break;
                        }

                        break;
                    }

                case "friends_rooms":
                    {
                        List<RoomData> roomsFriends = Session.GetHabbo().GetMessenger().GetActiveFriendsRooms().OrderByDescending(p => p.UsersNow).Take(EmuSettings.ROOMS_X_PESTAÑA).ToList();
                        foreach (RoomData data in roomsFriends)
                        {
                            if (data != null)
                                provisionalRooms.Add(data);

                            if (provisionalRooms.Count >= EmuSettings.ROOMS_X_PESTAÑA)
                                break;
                        }

                        roomsFriends.Clear();
                        roomsFriends = null;

                        break;
                    }

                case "popular":
                    {
                        if (!OtanixEnvironment.GetGame().GetRoomManager().GetActiveRooms().Any())
                            break;

                        List<RoomData> rooms = OtanixEnvironment.GetGame().GetRoomManager().GetActiveRooms();

                        foreach (RoomData data in rooms)
                        {
                            if (data != null)
                                provisionalRooms.Add(data);

                            if (provisionalRooms.Count >= EmuSettings.ROOMS_X_PESTAÑA)
                                break;
                        }

                        rooms.Clear();
                        rooms = null;

                        break;
                    }

                case "top_promotions":
                    {
                        List<RoomData> rooms = OtanixEnvironment.GetGame().GetRoomManager().GetEventManager().GetEventRooms();

                        foreach (RoomData data in rooms)
                        {
                            if (data != null)
                                provisionalRooms.Add(data);

                            if (provisionalRooms.Count >= EmuSettings.ROOMS_X_PESTAÑA)
                                break;
                        }

                        rooms.Clear();
                        rooms = null;

                        break;
                    }

                case "groups":
                    {
                        foreach (RoomData RD in OtanixEnvironment.GetGame().GetRoomManager().GetActiveRooms())
                        {
                            if (RD != null && RD.GroupId != 0)
                                provisionalRooms.Add(RD);
                        }
                        provisionalRooms = provisionalRooms.OrderByDescending(p => p.UsersNow).ToList();


                        break;
                    }

                case "query":
                    {
                        bool containsOwner = false;
                        bool containsGroup = false;

                        if (SearchQuery.StartsWith("owner:"))
                        {
                            SearchQuery = SearchQuery.Replace("owner:", string.Empty);
                            containsOwner = true;
                        }
                        else if (SearchQuery.StartsWith("group:"))
                        {
                            SearchQuery = SearchQuery.Replace("group:", string.Empty);
                            containsGroup = true;
                        }

                        if (!containsOwner)
                        {
                            var initForeach = false;

                            try
                            {
                                if (OtanixEnvironment.GetGame().GetRoomManager().GetActiveRooms().Count() > 0)
                                    initForeach = true;
                            }
                            catch { initForeach = false; }

                            if (initForeach)
                            {
                                foreach (var rms in OtanixEnvironment.GetGame().GetRoomManager().GetActiveRooms())
                                {
                                    if (rms.Name.ToLower().Contains(SearchQuery.ToLower()))
                                        provisionalRooms.Add(rms);

                                    if (provisionalRooms.Count >= EmuSettings.ROOMS_X_PESTAÑA)
                                        break;
                                }
                            }
                        }

                        if (provisionalRooms.Count < EmuSettings.ROOMS_X_PESTAÑA || containsOwner || containsGroup)
                        {
                            DataTable dTable = null;
                            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                if (containsOwner)
                                {
                                    dbClient.setQuery("SELECT * FROM rooms WHERE owner = @query LIMIT " + EmuSettings.ROOMS_X_PESTAÑA);
                                    dbClient.addParameter("query", SearchQuery);
                                    dTable = dbClient.getTable();
                                }
                                else if (containsGroup)
                                {
                                    dbClient.setQuery("SELECT * FROM rooms JOIN groups ON rooms.id = groups.roomid WHERE groups.name LIKE @query LIMIT " + EmuSettings.ROOMS_X_PESTAÑA);
                                    dbClient.addParameter("query", "%" + SearchQuery + "%");
                                    dTable = dbClient.getTable();
                                }
                                else
                                {
                  
                                    dbClient.setQuery("SELECT * FROM rooms LEFT OUTER JOIN groups ON rooms.id = groups.roomid WHERE rooms.caption LIKE @query OR rooms.owner LIKE @query OR groups.name LIKE @query LIMIT " + (EmuSettings.ROOMS_X_PESTAÑA - provisionalRooms.Count));
                                    dbClient.addParameter("query", "%" + SearchQuery + "%");
                                    dTable = dbClient.getTable();
                                }
                            }

                            if (dTable != null)
                            {
                                foreach (DataRow Row in dTable.Rows)
                                {
                                    RoomData RData = OtanixEnvironment.GetGame().GetRoomManager().FetchRoomData(Convert.ToUInt32(Row["id"]), Row);
                                    if (!provisionalRooms.Contains(RData))
                                        provisionalRooms.Add(RData);
                                }
                            }
                        }

                        break;
                    }

                default:
                    {
                        if (SubName.StartsWith("category__"))
                        {
                            provisionalRooms = OtanixEnvironment.GetGame().GetNavigator().SerializeNavigatorPopularRoomsNews(ref Message, OtanixEnvironment.GetGame().GetRoomManager().GetActiveRooms(), OtanixEnvironment.GetGame().GetNavigator().GetFlatCatIdByName(SubName.Replace("category__", "")), false);
                        }
                        else
                        {
                            foreach (uint roomId in OtanixEnvironment.GetGame().GetNewNavigatorManager().GetRoomsInCategory(SubName))
                            {
                                RoomData data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
                                if (data != null)
                                    provisionalRooms.Add(data);

                                if (provisionalRooms.Count >= EmuSettings.ROOMS_X_PESTAÑA)
                                    break;
                            }
                        }

                        break;
                    }
            }

            Message.AppendInt32(provisionalRooms.Count);
            foreach (RoomData data in provisionalRooms)
            {
                data.Serialize(Message);
            }

            provisionalRooms.Clear();
            provisionalRooms = null;
        }
    }
}
