using Butterfly.Core;
using Butterfly.HabboHotel.Filter;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Groups;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;

namespace Butterfly.HabboHotel.Group
{
    class GroupManager
    {
        #region Variables
        internal Dictionary<uint, GroupItem> loadedGroups;

        private readonly Queue groupsToAddQueue;
        private readonly Queue groupsToRemoveQueue;
        #endregion

        #region Return Values
        internal int ClearGroupsCache()
        {
            int groupsCount = loadedGroups.Count;
            loadedGroups.Clear();
            return groupsCount;
        }
        #endregion

        #region Cycling
        internal void OnCycle()
        {
            WorkGroupsToAddQueue();
            WorkGroupsToRemoveQueue();
        }

        internal uint GetTotalPosts(uint userId)
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT COUNT(*) FROM groups_forums_boards WHERE owner_id = '" + userId + "'");
                return Convert.ToUInt32(dbClient.getInteger());
            }
        }

        internal GroupItem LoadGroup(uint Id)
        {
            try
            {
                if (loadedGroups.ContainsKey(Id))
                    return loadedGroups[Id];

                DataRow dRow;
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT * FROM groups WHERE id = '" + Id + "'");
                    dRow = dbClient.getRow();
                }

                if (dRow == null)
                    return null;

                var Group = new GroupItem(dRow);

                lock (groupsToAddQueue.SyncRoot)
                {
                    groupsToAddQueue.Enqueue(Group);
                }

                return Group;
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
                return null;
            }
        }

        internal GroupItem LoadGroup(DataRow dRow)
        {
            try
            {
                if (loadedGroups.ContainsKey(Convert.ToUInt32(dRow["id"])))
                    return loadedGroups[Convert.ToUInt32(dRow["id"])];

                var Group = new GroupItem(dRow);

                lock (groupsToAddQueue.SyncRoot)
                {
                    groupsToAddQueue.Enqueue(Group);
                }

                return Group;
            }
            catch
            {
                return null;
            }
        }

        private void WorkGroupsToAddQueue()
        {
            if (groupsToAddQueue.Count > 0)
            {
                lock (groupsToAddQueue.SyncRoot)
                {
                    while (groupsToAddQueue.Count > 0)
                    {
                        var group = (GroupItem)groupsToAddQueue.Dequeue();
                        if (!loadedGroups.ContainsKey(group.Id))
                            loadedGroups.Add(group.Id, group);
                    }
                }
            }
        }

        private void WorkGroupsToRemoveQueue()
        {
            if (groupsToRemoveQueue.Count > 0)
            {
                lock (groupsToRemoveQueue.SyncRoot)
                {
                    while (groupsToRemoveQueue.Count > 0)
                    {
                        var groupID = (uint)groupsToRemoveQueue.Dequeue();
                        loadedGroups.Remove(groupID);
                    }
                }
            }
        }

        internal void RemoveGroupQueue(uint GroupId)
        {
            lock (groupsToRemoveQueue.SyncRoot)
            {
                groupsToRemoveQueue.Enqueue(GroupId);
            }
        }
        #endregion

        #region Methods
        internal GroupManager()
        {
            loadedGroups = new Dictionary<uint, GroupItem>();
            groupsToAddQueue = new Queue();
            groupsToRemoveQueue = new Queue();
        }

        internal void HandleGroup(ClientMessage Message, GameClient Session)
        {
            var Group = new GroupItem
            {   
                Name = OtanixEnvironment.FilterInjectionChars(Message.PopFixedString()),
                Description = OtanixEnvironment.FilterInjectionChars(Message.PopFixedString()),
                RoomId = Message.PopWiredUInt(),
                RightsType = 1
            };

            if (BlackWordsManager.Check(Group.Name, BlackWordType.Insult, Session, "<Nombre de Grupo>"))
                Group.Name = "Mensaje bloqueado por el filtro bobba.";

            if (BlackWordsManager.Check(Group.Description, BlackWordType.Insult, Session, "<Descripción de Grupo>"))
                Group.Description = "Mensaje bloqueado por el filtro bobba.";

            var rData = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData((uint)Group.RoomId);
            if (rData.GroupId != 0)
            {
                Session.SendNotif("Esta sala ya tiene creado un grupo");
                return;
            }
            if(EmuSettings.HOTEL_LUCRATIVO && (Session.GetHabbo().Moedas < 10))
            {
                Session.SendNotif("Moedas insuficientes");
                return;
            }

            if(rData.OwnerId != Session.GetHabbo().Id)
            {
                Session.SendNotif("¡Oops, ha sucedido un error has intentado crear un grupo en una sala que no te pertenece!");
                return;
            }

            Session.GetHabbo().Moedas -= 10;

            Group.CustomColor1 = Message.PopWiredInt32();
            Group.CustomColor2 = Message.PopWiredInt32();
            var ArrayItem = (Message.PopWiredInt32() - 3) / 3;
            Group.GroupBase = Message.PopWiredInt32();
            Group.GroupBaseColor = Message.PopWiredInt32();
            Group.GroupBasePosition = Message.PopWiredInt32();
            for (var k = 0; k < ArrayItem; k++)
            {
                if (k == 0)
                    Group.GroupItem1 = new int[] { Message.PopWiredInt32(), Message.PopWiredInt32(), Message.PopWiredInt32() };
                else if (k == 1)
                    Group.GroupItem2 = new int[] { Message.PopWiredInt32(), Message.PopWiredInt32(), Message.PopWiredInt32() };
                else if (k == 2)
                    Group.GroupItem3 = new int[] { Message.PopWiredInt32(), Message.PopWiredInt32(), Message.PopWiredInt32() };
                else if (k == 3)
                    Group.GroupItem4 = new int[] { Message.PopWiredInt32(), Message.PopWiredInt32(), Message.PopWiredInt32() };
            }

            Group.GroupImage = GenerateGuildImage(Group);
            Group.HtmlColor1 = GetHtmlColor(Group.CustomColor1, 1);
            Group.HtmlColor2 = GetHtmlColor(Group.CustomColor2, 2);
            Group.DateCreated = DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
            Group.OwnerId = Session.GetHabbo().Id;
            Group.OwnerName = Session.GetHabbo().Username;

            var ThisMonth = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[DateTime.Now.Month - 1].Substring(0, 3);
            ThisMonth = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(ThisMonth);
            var DateJoined = ThisMonth + " " + DateTime.Now.Day + ", " + DateTime.Now.Year;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO groups (name, description, roomid, customcolor1, customcolor2, groupbase, groupbasecolor, groupbaseposition, groupitem1, groupitem2, groupitem3, groupitem4, groupimage, htmlcolor1, htmlcolor2, datecreated, ownerid, rightsType) VALUES (@name, @description, '" + Group.RoomId + "', '" + Group.CustomColor1 + "','" + Group.CustomColor2 + "','" + Group.GroupBase + "','" + Group.GroupBaseColor + "','" + Group.GroupBasePosition + "','" + GeneratePartData(Group.GroupItem1) + "', '" + GeneratePartData(Group.GroupItem2) + "', '" + GeneratePartData(Group.GroupItem3) + "', '" + GeneratePartData(Group.GroupItem4) + "', '" + Group.GroupImage + "','" + Group.HtmlColor1 + "','" + Group.HtmlColor2 + "','" + Group.DateCreated + "','" + Group.OwnerId + "','1')");
                dbClient.addParameter("name", Group.Name);
                dbClient.addParameter("description", Group.Description);
                Group.Id = (uint)dbClient.insertQuery();

                Session.GetHabbo().MyGroups.Add(Group.Id);

                if (Session.GetHabbo().FavoriteGroup == 0)
                {
                    Session.GetHabbo().FavoriteGroup = Group.Id;
                }

                dbClient.runFastQuery("UPDATE rooms SET groupId = '" + Group.Id + "' WHERE id = '" + Group.RoomId + "'");

                dbClient.setQuery("INSERT INTO groups_users VALUES (@groupid, @userid, '1', '0', @datejoined)");
                dbClient.addParameter("groupid", Group.Id);
                dbClient.addParameter("userid", Session.GetHabbo().Id);
                dbClient.addParameter("datejoined", DateJoined);
                dbClient.runQuery();
            }

            rData.GroupId = Group.Id;

            var SendItem = new ServerMessage(Outgoing.PurchaseOKMessageOfferData);
            SendItem.AppendInt32(6165);
            SendItem.AppendString("CREATE_GUILD");
            SendItem.AppendBoolean(false);
            SendItem.AppendInt32(10);
            SendItem.AppendInt32(0);
            SendItem.AppendInt32(0);
            SendItem.AppendBoolean(true);
            SendItem.AppendInt32(0);
            SendItem.AppendInt32(2);
            SendItem.AppendBoolean(false);
            Session.SendMessage(SendItem);

            var SendOwnerId = new ServerMessage(Outgoing.SendOwnerId);
            SendOwnerId.AppendUInt(Session.GetHabbo().Id);
            Session.SendMessage(SendOwnerId);

            var GroupAndRoom = new ServerMessage(Outgoing.SendRoomAndGroup);
            GroupAndRoom.AppendUInt(Group.RoomId);
            GroupAndRoom.AppendUInt(Group.Id);
            Session.SendMessage(GroupAndRoom);

            var Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom((uint)Group.RoomId);
            if (Room != null)
            {
                var Update = new ServerMessage(Outgoing.UpdateRoom);
                Update.AppendUInt(Group.RoomId);
                Room.SendMessage(Update);

                var AddGuild = new ServerMessage(Outgoing.SendMyGroups);
                AddGuild.AppendInt32(Session.GetHabbo().MyGroups.Count); // Count of guilds
                foreach (var xGroupId in Session.GetHabbo().MyGroups)
                {
                    var xGroup = OtanixEnvironment.GetGame().GetGroup().LoadGroup(xGroupId);
                    if (xGroup == null)
                    {
                        Session.GetHabbo().MyGroups.Remove(xGroupId);
                        return;
                    }

                    AddGuild.AppendUInt(xGroup.Id);
                    AddGuild.AppendString(xGroup.Name);
                    AddGuild.AppendString(xGroup.GroupImage);
                    AddGuild.AppendString(xGroup.HtmlColor1);
                    AddGuild.AppendString(xGroup.HtmlColor2);
                    AddGuild.AppendBoolean((xGroup.Id == Session.GetHabbo().FavoriteGroup) ? true : false);
                    AddGuild.AppendUInt(xGroup.OwnerId);
                    AddGuild.AppendBoolean(false); // not used.
                }
                Session.SendMessage(AddGuild);

                if (Session.GetHabbo().FavoriteGroup == Group.Id)
                {
                    var UpdateUserGroup = new ServerMessage(Outgoing.SendGroup);
                    UpdateUserGroup.AppendInt32(1);
                    UpdateUserGroup.AppendUInt(Group.Id);
                    UpdateUserGroup.AppendString(Group.GroupImage);
                    Room.SendMessage(UpdateUserGroup);

                    if (Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id) != null)
                    {
                        var UpdateUserGroup2 = new ServerMessage(Outgoing.UpdateUserGroupRemoving);
                        UpdateUserGroup2.AppendInt32(Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id).VirtualId);
                        UpdateUserGroup2.AppendUInt(Group.Id);
                        UpdateUserGroup2.AppendInt32(3); // state
                        UpdateUserGroup2.AppendString(Group.Name);
                        Room.SendMessage(UpdateUserGroup2);
                    }
                }
            }
        }

        internal string GeneratePartData(int[] data)
        {
            try
            {
                return (data != null) ? (data[0] + ";" + data[1] + ";" + data[2]) : "";
            }
            catch { return ""; }
        }

        internal string GetHtmlColor(int color, int htmlType)
        {
            try
            {
                if (htmlType == 1 && GuildsPartsManager.htmlBadges1.ContainsKey(color))
                    return GuildsPartsManager.htmlBadges1[color] as string;
                else if (htmlType == 2 && GuildsPartsManager.htmlBadges2.ContainsKey(color))
                    return GuildsPartsManager.htmlBadges2[color] as string;
                else
                    return "ffffff";
            }
            catch
            {
                return "ffffff";
            }
        }

        internal string GenerateGuildImage(GroupItem Group)
        {
            var Image = "";
            Image += "b" + ((Group.GroupBase < 10) ? "0" : "") + Group.GroupBase + ((Group.GroupBaseColor < 10) ? "0" : "") + Group.GroupBaseColor;

            if (Group.GroupItem1 != null)
            {
                Image += "s" + ((Group.GroupItem1[0] < 10) ? "0" : "") + Group.GroupItem1[0]
       + ((Group.GroupItem1[1] < 10) ? "0" : "") + Group.GroupItem1[1]
       + ((Group.GroupItem1[2] < 9) ? Group.GroupItem1[2].ToString() : "0");
            }
            if (Group.GroupItem2 != null)
            {
                Image += "s" + ((Group.GroupItem2[0] < 10) ? "0" : "") + Group.GroupItem2[0]
       + ((Group.GroupItem2[1] < 10) ? "0" : "") + Group.GroupItem2[1]
       + ((Group.GroupItem2[2] < 9) ? Group.GroupItem2[2].ToString() : "0");
            }
            if (Group.GroupItem3 != null)
            {
                Image += "s" + ((Group.GroupItem3[0] < 10) ? "0" : "") + Group.GroupItem3[0]
       + ((Group.GroupItem3[1] < 10) ? "0" : "") + Group.GroupItem3[1]
       + ((Group.GroupItem3[2] < 9) ? Group.GroupItem3[2].ToString() : "0");
            }
            if (Group.GroupItem4 != null)
            {
                Image += "s" + ((Group.GroupItem4[0] < 10) ? "0" : "") + Group.GroupItem4[0]
       + ((Group.GroupItem4[1] < 10) ? "0" : "") + Group.GroupItem4[1]
       + ((Group.GroupItem4[2] < 9) ? Group.GroupItem4[2].ToString() : "0");
            }
            return Image;
        }
        #endregion

        #region Voids
        //private bool ForumsMoreActive = false;
        //internal List<GroupItem> GroupsMoreActiveForums;

        internal List<GroupItem> LoadForumsMoreActive()
        {
            //if (ForumsMoreActive == false)
            //{
            List<GroupItem> GroupsMoreActiveForums = new List<GroupItem>();
            //    ForumsMoreActive = true;
            DataTable dTable = null;
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT groupid FROM groups_forums ORDER BY totalPosts DESC LIMIT 20");
                dTable = dbClient.getTable();
            }

            if (dTable != null)
            {
                foreach (DataRow dRow in dTable.Rows)
                {
                    GroupItem g = LoadGroup(Convert.ToUInt32(dRow["groupid"]));
                    if (g == null)
                        continue;

                    GroupsMoreActiveForums.Add(g);
                }
            }
            return GroupsMoreActiveForums;
            //}
        }        
        #endregion
    }
}
