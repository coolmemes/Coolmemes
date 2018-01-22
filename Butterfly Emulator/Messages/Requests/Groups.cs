using ButterStorm;
using HabboEvents;
using Butterfly.HabboHotel.Rooms;
using System.Collections.Generic;
using Butterfly.HabboHotel.Group;
using Butterfly.HabboHotel.Groups;
using System.Globalization;
using System;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Users;
using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Filter;
using Butterfly.HabboHotel.Misc;

namespace Butterfly.Messages
{
    internal partial class GameClientMessageHandler
    {
        internal void GenerateBuyGroupPage()
        {
            List<RoomData> RoomsForGroups = new List<RoomData>();
            foreach (uint roomId in Session.GetHabbo().UsersRooms)
            {
                RoomData data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
                if (data != null && data.GroupId != 0)
                    continue;

                RoomsForGroups.Add(data);
            }

            ServerMessage GuildParts = new ServerMessage(Outgoing.SendGuildParts);
            GuildParts.AppendInt32(10); // Buy Price.
            GuildParts.AppendInt32(RoomsForGroups.Count);
            foreach (var room in RoomsForGroups)
            {
                GuildParts.AppendUInt(room.Id);
                GuildParts.AppendString(room.Name);
                GuildParts.AppendBoolean(false); // "${group.edit.error.warning}", "${group.edit.error.controllers}"
            }
            GuildParts.AppendInt32(5); // count (elementos de placa) (placa predeterminada)
            {
                GuildParts.AppendInt32(10);
                GuildParts.AppendInt32(3);
                GuildParts.AppendInt32(4);

                GuildParts.AppendInt32(25);
                GuildParts.AppendInt32(17);
                GuildParts.AppendInt32(5);

                GuildParts.AppendInt32(25);
                GuildParts.AppendInt32(17);
                GuildParts.AppendInt32(3);

                GuildParts.AppendInt32(29);
                GuildParts.AppendInt32(11);
                GuildParts.AppendInt32(4);

                GuildParts.AppendInt32(0);
                GuildParts.AppendInt32(0);
                GuildParts.AppendInt32(0);
            }
            Session.SendMessage(GuildParts);

            Session.SendMessage(OtanixEnvironment.GetGame().GetCatalog().mGroupPage);
        }

        internal void SendGroupColors()
        {
            Session.SendMessage(OtanixEnvironment.GetGame().GetCatalog().mGroupPage);
        }

        internal void CreateGuildMessageComposer()
        {
            OtanixEnvironment.GetGame().GetGroup().HandleGroup(Request, Session);
        }

        internal void ActivateGroupOnRoom()
        {
            uint GroupId = Request.PopWiredUInt();
            bool Type = Request.PopWiredBoolean();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null)
                return;

            RoomData Room = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData((uint)Group.RoomId);
            if (Room == null)
                return;

            SendGroupTypeMessage(Group, Room, Type);
        }

        internal void ClickOnGroupItem()
        {
            uint ItemId = Request.PopWiredUInt();
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null)
                return;

            ServerMessage Message = new ServerMessage(Outgoing.ClickOnGroupItem);
            Message.AppendUInt(ItemId);
            Message.AppendUInt(GroupId);
            Message.AppendString(Group.Name);
            Message.AppendUInt(Group.RoomId);
            Message.AppendBoolean(Session.GetHabbo().MyGroups.Contains(GroupId));
            Message.AppendBoolean(Group.Forum != null);
            Session.SendMessage(Message);
        }

        internal void SendGroupTypeMessage(GroupItem Group, RoomData Room, bool Type)
        {
            ServerMessage InitGroup = new ServerMessage(Outgoing.SendAdvGroupInit);
            InitGroup.AppendUInt(Group.Id); // groupid
            InitGroup.AppendBoolean(true); // cualquiera puede unirse (límite 50000) ?? Sin este true no se muestra en sala.
            InitGroup.AppendInt32(Group.Type); // Type
            InitGroup.AppendString(Group.Name); // Name
            InitGroup.AppendString(Group.Description); // Description
            InitGroup.AppendString(Group.GroupImage); // Image
            InitGroup.AppendUInt(Room.Id); // RoomId
            InitGroup.AppendString(Room.Name); // RoomName
            if (Group.Petitions.Contains(Session.GetHabbo().Id)) // status
                InitGroup.AppendInt32(2);
            else if (Session.GetHabbo().MyGroups.Contains(Group.Id)) // Group.Members.ContainsKey((int)Session.GetHabbo().Id))
                InitGroup.AppendInt32(1);
            else
                InitGroup.AppendInt32(0);
            InitGroup.AppendInt32(Group.MembersCount); // totalMembers
            InitGroup.AppendBoolean(Session.GetHabbo().FavoriteGroup == Group.Id); // Favourite Group
            InitGroup.AppendString(Group.DateCreated); //  fecha de creación
            InitGroup.AppendBoolean((Session.GetHabbo().Id == Group.OwnerId || (!Session.GetHabbo().MyGroups.Contains(Group.Id) && Session.GetHabbo().HasFuse("can_modify_group"))) ? true : false); // administrador (poder gestionar)
            InitGroup.AppendBoolean(Group.Admins.ContainsKey(Session.GetHabbo().Id)); // have pows
            InitGroup.AppendString(Group.OwnerName); // nombre del dueño
            InitGroup.AppendBoolean(Type); // Activar si es al clicar o al entrar en sala
            InitGroup.AppendBoolean(Group.RightsType == 0); // los miembros de este grupo pueden decorar la sala del grupo
            InitGroup.AppendInt32((Session.GetHabbo().MyGroups.Contains(Group.Id)) ? Group.Petitions.Count : 0); // numero de peticiones (mandar si es admin)
            InitGroup.AppendBoolean(Group.Forum == null ? false : true); // show_forum_link
            Session.SendMessage(InitGroup);
        }

        internal void GestionarGrupo()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || (Group.OwnerId != Session.GetHabbo().Id && !(Session.GetHabbo().HasFuse("can_modify_group"))))
                return;

            List<RoomData> RoomsForGroups = new List<RoomData>();
            foreach (uint roomId in Session.GetHabbo().UsersRooms)
            {
                RoomData data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
                if (data != null && data.GroupId != 0)
                    continue;

                RoomsForGroups.Add(data);
            }

            ServerMessage Gestion = new ServerMessage(Outgoing.SendGestionGroup);
            Gestion.AppendInt32(RoomsForGroups.Count);
            foreach (var r in RoomsForGroups)
            {
                Gestion.AppendUInt(r.Id);
                Gestion.AppendString(r.Name);
                Gestion.AppendBoolean(false);
            }
            Gestion.AppendBoolean(Session.GetHabbo().Id == Group.OwnerId);
            Gestion.AppendUInt(Group.Id);
            Gestion.AppendString(Group.Name);
            Gestion.AppendString(Group.Description);
            Gestion.AppendUInt(Group.RoomId);
            Gestion.AppendInt32(Group.CustomColor1);
            Gestion.AppendInt32(Group.CustomColor2);
            Gestion.AppendInt32(Group.Type);
            Gestion.AppendInt32(Group.RightsType);
            Gestion.AppendBoolean(Group.Type == 2); // locked
            Gestion.AppendString(""); // url
            Gestion.AppendInt32(5); // foreach count
            Gestion.AppendInt32(Group.GroupBase);
            Gestion.AppendInt32(Group.GroupBaseColor);
            Gestion.AppendInt32(Group.GroupBasePosition);
            if (Group.GroupItem1 == null)
            {
                Gestion.AppendInt32(0);
                Gestion.AppendInt32(0);
                Gestion.AppendInt32(0);
            }
            else
            {
                Gestion.AppendInt32(Group.GroupItem1[0]);
                Gestion.AppendInt32(Group.GroupItem1[1]);
                Gestion.AppendInt32(Group.GroupItem1[2]);
            }
            if (Group.GroupItem2 == null)
            {
                Gestion.AppendInt32(0);
                Gestion.AppendInt32(0);
                Gestion.AppendInt32(0);
            }
            else
            {
                Gestion.AppendInt32(Group.GroupItem2[0]);
                Gestion.AppendInt32(Group.GroupItem2[1]);
                Gestion.AppendInt32(Group.GroupItem2[2]);
            }
            if (Group.GroupItem3 == null)
            {
                Gestion.AppendInt32(0);
                Gestion.AppendInt32(0);
                Gestion.AppendInt32(0);
            }
            else
            {
                Gestion.AppendInt32(Group.GroupItem3[0]);
                Gestion.AppendInt32(Group.GroupItem3[1]);
                Gestion.AppendInt32(Group.GroupItem3[2]);
            }
            if (Group.GroupItem4 == null)
            {
                Gestion.AppendInt32(0);
                Gestion.AppendInt32(0);
                Gestion.AppendInt32(0);
            }
            else
            {
                Gestion.AppendInt32(Group.GroupItem4[0]);
                Gestion.AppendInt32(Group.GroupItem4[1]);
                Gestion.AppendInt32(Group.GroupItem4[2]);
            }
            Gestion.AppendString(Group.GroupImage);
            Gestion.AppendInt32(Group.MembersCount);
            Session.SendMessage(Gestion);
        }

        internal void SaveGroupIdentity()
        {
            uint GroupId = Request.PopWiredUInt();
            string GroupName = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            string GroupDescription = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || (Group.OwnerId != Session.GetHabbo().Id && !(Session.GetHabbo().HasFuse("can_modify_group"))))
                return;

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Group.RoomId);
            if (Room == null)
                return;

            if (BlackWordsManager.Check(GroupName, BlackWordType.Hotel, Session, "<SaveGroupIdentity>"))
                Group.Name = "Mensaje bloqueado por el filtro bobba.";
            else
                Group.Name = GroupName;

            if (BlackWordsManager.Check(GroupDescription, BlackWordType.Hotel, Session, "<SaveGroupIdentity>"))
                Group.Description = "Mensaje bloqueado por el filtro bobba.";
            else
                Group.Description = GroupDescription;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("UPDATE groups SET name = @Name, description = @Descrip WHERE id = '" + Group.Id + "'");
                dbClient.addParameter("Name", Group.Name);
                dbClient.addParameter("Descrip", Group.Description);
                dbClient.runQuery();
            }

            ServerMessage Update = new ServerMessage(Outgoing.UpdateRoom);
            Update.AppendUInt(Group.RoomId);
            Room.SendMessage(Update);

            ServerMessage Update2 = new ServerMessage(Outgoing.ConfigureWallandFloor);
            Update2.AppendBoolean(Room.RoomData.Hidewall);
            Update2.AppendInt32(Room.RoomData.WallThickness);
            Update2.AppendInt32(Room.RoomData.FloorThickness);
            Room.SendMessage(Update2);
        }

        internal void SaveGroupImage()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || (Group.OwnerId != Session.GetHabbo().Id && !(Session.GetHabbo().HasFuse("can_modify_group"))))
                return;

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Group.RoomId);
            if (Room == null)
                return;

            Group.GroupItem1 = null;
            Group.GroupItem2 = null;
            Group.GroupItem3 = null;
            Group.GroupItem4 = null;

            int ArrayItem = (Request.PopWiredInt32() - 3) / 3;
            Group.GroupBase = Request.PopWiredInt32();
            Group.GroupBaseColor = Request.PopWiredInt32();
            Group.GroupBasePosition = Request.PopWiredInt32();
            for (int k = 0; k < ArrayItem; k++)
            {
                if (k == 0)
                    Group.GroupItem1 = new int[] { Request.PopWiredInt32(), Request.PopWiredInt32(), Request.PopWiredInt32() };
                else if (k == 1)
                    Group.GroupItem2 = new int[] { Request.PopWiredInt32(), Request.PopWiredInt32(), Request.PopWiredInt32() };
                else if (k == 2)
                    Group.GroupItem3 = new int[] { Request.PopWiredInt32(), Request.PopWiredInt32(), Request.PopWiredInt32() };
                else if (k == 3)
                    Group.GroupItem4 = new int[] { Request.PopWiredInt32(), Request.PopWiredInt32(), Request.PopWiredInt32() };
            }

            Group.GroupImage = OtanixEnvironment.GetGame().GetGroup().GenerateGuildImage(Group);

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("UPDATE groups SET groupimage = '" + Group.GroupImage + "', groupbase = '" + Group.GroupBase + "', groupbasecolor = '" + Group.GroupBaseColor + "', groupbaseposition = '" + Group.GroupBasePosition + "', groupitem1 = @groupitem1, groupitem2 = @groupitem2, groupitem3 = @groupitem3, groupitem4 = @groupitem4  WHERE id = '" + Group.Id + "'");
                dbClient.addParameter("groupitem1", (Group.GroupItem1 != null) ? (Group.GroupItem1[0] + ";" + Group.GroupItem1[1] + ";" + Group.GroupItem1[2]) : "");
                dbClient.addParameter("groupitem2", (Group.GroupItem2 != null) ? (Group.GroupItem2[0] + ";" + Group.GroupItem2[1] + ";" + Group.GroupItem2[2]) : "");
                dbClient.addParameter("groupitem3", (Group.GroupItem3 != null) ? (Group.GroupItem3[0] + ";" + Group.GroupItem3[1] + ";" + Group.GroupItem3[2]) : "");
                dbClient.addParameter("groupitem4", (Group.GroupItem4 != null) ? (Group.GroupItem4[0] + ";" + Group.GroupItem4[1] + ";" + Group.GroupItem4[2]) : "");
                dbClient.runQuery();
            }

            ServerMessage Update = new ServerMessage(Outgoing.UpdateRoom);
            Update.AppendUInt(Group.RoomId);
            Room.SendMessage(Update);

            ServerMessage Update2 = new ServerMessage(Outgoing.ConfigureWallandFloor);
            Update2.AppendBoolean(Room.RoomData.Hidewall);
            Update2.AppendInt32(Room.RoomData.WallThickness);
            Update2.AppendInt32(Room.RoomData.FloorThickness);
            Room.SendMessage(Update2);
        }

        internal void SaveGroupColours()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || (Group.OwnerId != Session.GetHabbo().Id && !(Session.GetHabbo().HasFuse("can_modify_group"))))
                return;

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Group.RoomId);
            if (Room == null)
                return;

            Group.CustomColor1 = Request.PopWiredInt32();
            Group.CustomColor2 = Request.PopWiredInt32();
            Group.HtmlColor1 = OtanixEnvironment.GetGame().GetGroup().GetHtmlColor(Group.CustomColor1, 1);
            Group.HtmlColor2 = OtanixEnvironment.GetGame().GetGroup().GetHtmlColor(Group.CustomColor2, 2);

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("UPDATE groups SET customcolor1 = @cc1, customcolor2 = @cc2, htmlcolor1 = @html1, htmlcolor2 = @html2 WHERE id = '" + Group.Id + "'");
                dbClient.addParameter("cc1", Group.CustomColor1);
                dbClient.addParameter("cc2", Group.CustomColor2);
                dbClient.addParameter("html1", Group.HtmlColor1);
                dbClient.addParameter("html2", Group.HtmlColor2);
                dbClient.runQuery();
            }

            ServerMessage Update = new ServerMessage(Outgoing.UpdateRoom);
            Update.AppendUInt(Group.RoomId);
            Room.SendMessage(Update);

            ServerMessage Update2 = new ServerMessage(Outgoing.ConfigureWallandFloor);
            Update2.AppendBoolean(Room.RoomData.Hidewall);
            Update2.AppendInt32(Room.RoomData.WallThickness);
            Update2.AppendInt32(Room.RoomData.FloorThickness);
            Room.SendMessage(Update2);
        }

        internal void SaveGroupSettings()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || (Group.OwnerId != Session.GetHabbo().Id && !(Session.GetHabbo().HasFuse("can_modify_group"))))
                return;

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Group.RoomId);
            if (Room == null)
                return;

            Group.Type = Request.PopWiredInt32();
            Group.RightsType = Request.PopWiredInt32();

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE groups SET type = '" + Group.Type + "', rightsType = '" + Group.RightsType + "' WHERE id = " + Group.Id);
            }

            ServerMessage Update = new ServerMessage(Outgoing.UpdateRoom);
            Update.AppendUInt(Group.RoomId);
            Room.SendMessage(Update);

            ServerMessage Update2 = new ServerMessage(Outgoing.ConfigureWallandFloor);
            Update2.AppendBoolean(Room.RoomData.Hidewall);
            Update2.AppendInt32(Room.RoomData.WallThickness);
            Update2.AppendInt32(Room.RoomData.FloorThickness);
            Room.SendMessage(Update2);

            foreach (RoomUser user in Room.GetRoomUserManager().UserList.Values)
            {
                if (user.IsBot || user.GetClient() == null || !user.GetClient().GetHabbo().MyGroups.Contains(Group.Id))
                    continue;

                if (Group.RightsType == 0 && !Group.Admins.ContainsKey(user.HabboId)) // puedo mover aunque sea newbie!!
                {
                    user.RemoveStatus("flatctrl 0");
                    user.AddStatus("flatctrl 1", "");
                    user.UpdateNeeded = true;

                    Response.Init(Outgoing.RoomRightsLevel);
                    Response.AppendInt32(1);
                    user.GetClient().SendMessage(GetResponse());
                }
                else if (Group.RightsType == 1) // vamos a quitar a los napas
                {
                    if (!Group.Admins.ContainsKey(user.HabboId))
                    {
                        user.RemoveStatus("flatctrl 1");
                        user.AddStatus("flatctrl 0", "");
                        user.UpdateNeeded = true;

                        Response.Init(Outgoing.RoomRightsLevel);
                        Response.AppendInt32(0);
                        user.GetClient().SendMessage(GetResponse());
                    }
                }
            }
        }

        internal void LookGroupMembers()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null)
                return;

            int PageMembers = Request.PopWiredInt32();
            string SearchingBy = Request.PopFixedString();
            int MembersOrPetitions = Request.PopWiredInt32();

            if (MembersOrPetitions == 0) // members
            {
                ServerMessage SendMiembros = new ServerMessage(Outgoing.LookGroupMembers);
                SendMiembros.AppendUInt(Group.Id);
                SendMiembros.AppendString(Group.Name);
                SendMiembros.AppendUInt(Group.RoomId);
                SendMiembros.AppendString(Group.GroupImage);
                SendMiembros.AppendInt32(Group.MembersCount);
                Group.GetGroupUsersPage(PageMembers, SendMiembros);
                SendMiembros.AppendBoolean(Group.Admins.ContainsKey(Session.GetHabbo().Id) || Session.GetHabbo().HasFuse("can_modify_group"));
                SendMiembros.AppendInt32(14);
                SendMiembros.AppendInt32(PageMembers);
                SendMiembros.AppendInt32(MembersOrPetitions);
                SendMiembros.AppendString(SearchingBy);
                Session.SendMessage(SendMiembros);
            }
            else if (MembersOrPetitions == 1) // solo administradores
            {
                List<GroupUser> aOfAdmins = new List<GroupUser>();
                List<GroupUser> OfAdmins = new List<GroupUser>(Group.Admins.Values);
                for (int i = PageMembers * 14; i < (PageMembers * 14) + 14; i++)
                {
                    try { aOfAdmins.Add(OfAdmins[i]); }
                    catch { break; }
                }

                ServerMessage SendAdmins = new ServerMessage(Outgoing.LookGroupMembers);
                SendAdmins.AppendUInt(Group.Id);
                SendAdmins.AppendString(Group.Name);
                SendAdmins.AppendUInt(Group.RoomId);
                SendAdmins.AppendString(Group.GroupImage);
                SendAdmins.AppendInt32(Group.Admins.Count);
                SendAdmins.AppendInt32(aOfAdmins.Count);
                foreach (GroupUser User in aOfAdmins)
                {
                    Habbo zUser = UsersCache.getHabboCache(User.UserId);
                    if (zUser == null)
                        continue;

                    SendAdmins.AppendUInt(User.GroupRank);
                    SendAdmins.AppendUInt(zUser.Id);
                    SendAdmins.AppendString(zUser.Username);
                    SendAdmins.AppendString(zUser.Look);
                    SendAdmins.AppendString(User.DateJoined); // Se ha unido el.
                }
                SendAdmins.AppendBoolean(Group.Admins.ContainsKey(Session.GetHabbo().Id) || Session.GetHabbo().HasFuse("can_modify_group"));
                SendAdmins.AppendInt32(14);
                SendAdmins.AppendInt32(PageMembers);
                SendAdmins.AppendInt32(MembersOrPetitions);
                SendAdmins.AppendString(SearchingBy);
                Session.SendMessage(SendAdmins);
            }
            else if (MembersOrPetitions == 2) // Petitions
            {
                List<uint> OfPetitions = new List<uint>();
                for (var i = PageMembers * 14; i < (PageMembers * 14) + 14; i++)
                {
                    try { OfPetitions.Add(Group.Petitions[i]); }
                    catch { break; }
                }

                ServerMessage SendPetitions = new ServerMessage(Outgoing.LookGroupMembers);
                SendPetitions.AppendUInt(Group.Id);
                SendPetitions.AppendString(Group.Name);
                SendPetitions.AppendUInt(Group.RoomId);
                SendPetitions.AppendString(Group.GroupImage);
                SendPetitions.AppendInt32(Group.Petitions.Count);
                SendPetitions.AppendInt32(OfPetitions.Count);
                foreach (uint UserId in OfPetitions)
                {
                    Habbo zUser = UsersCache.getHabboCache(UserId);
                    if (zUser == null)
                        continue;

                    SendPetitions.AppendInt32(3); // state (if is petition, all times are 3)
                    SendPetitions.AppendUInt(zUser.Id);
                    SendPetitions.AppendString(zUser.Username);
                    SendPetitions.AppendString(zUser.Look);
                    SendPetitions.AppendString(""); // Se ha unido el.
                }
                SendPetitions.AppendBoolean(Group.Admins.ContainsKey(Session.GetHabbo().Id) || Session.GetHabbo().HasFuse("can_modify_group"));
                SendPetitions.AppendInt32(14);
                SendPetitions.AppendInt32(PageMembers);
                SendPetitions.AppendInt32(MembersOrPetitions);
                SendPetitions.AppendString(SearchingBy);
                Session.SendMessage(SendPetitions);
            }
        }

        internal void TryJoinToGroup()
        {
            uint GroupId = Request.PopWiredUInt();

            if (Session.GetHabbo().MyGroups.Contains(GroupId)) // si ya pertenecemos al grupo.
                return;

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null)
                return;

            RoomData Room = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(Group.RoomId);
            if (Room == null)
                return;

            if (Group.Type == 0) // Abierto para todos los públicos
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    if (Session.GetHabbo().FavoriteGroup == 0)
                    {
                        Session.GetHabbo().FavoriteGroup = Group.Id;
                    }

                    string ThisMonth = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[DateTime.Now.Month - 1].Substring(0, 3));
                    string DateJoined = ThisMonth + " " + DateTime.Now.Day + ", " + DateTime.Now.Year;

                    dbClient.runFastQuery("INSERT INTO groups_users VALUES ('" + Group.Id + "','" + Session.GetHabbo().Id + "','1', '2', '" + DateJoined + "')");
                }

                Session.GetHabbo().MyGroups.Add(Group.Id);
                Group.MembersCount++;

                ServerMessage AddGuild = new ServerMessage(Outgoing.SendMyGroups);
                AddGuild.AppendInt32(Session.GetHabbo().MyGroups.Count); // Count of guilds
                foreach (uint xGroupId in Session.GetHabbo().MyGroups)
                {
                    GroupItem xGroup = OtanixEnvironment.GetGame().GetGroup().LoadGroup(xGroupId);
                    if (xGroup == null)
                        return;

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

                SendGroupTypeMessage(Group, Room, false);

                if (Session.GetHabbo().FavoriteGroup == Group.Id)
                {
                    Room rRoom = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Room.Id);
                    if (rRoom != null && rRoom.GetRoomUserManager() != null)
                    {
                        ServerMessage UpdateUserGroup = new ServerMessage(Outgoing.SendGroup);
                        UpdateUserGroup.AppendInt32(1);
                        UpdateUserGroup.AppendUInt(Group.Id);
                        UpdateUserGroup.AppendString(Group.GroupImage);
                        rRoom.SendMessage(UpdateUserGroup);

                        RoomUser roomUser = rRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                        if (roomUser != null)
                        {
                            ServerMessage UpdateUserGroup2 = new ServerMessage(Outgoing.UpdateUserGroupRemoving);
                            UpdateUserGroup2.AppendInt32(roomUser.VirtualId);
                            UpdateUserGroup2.AppendUInt(Group.Id);
                            UpdateUserGroup2.AppendInt32(3); // state
                            UpdateUserGroup2.AppendString(Group.Name);
                            rRoom.SendMessage(UpdateUserGroup2);
                        }
                    }
                }
            }
            else if (Group.Type == 1) // Hace falta mandar petición
            {
                if (Group.Petitions.Contains(Session.GetHabbo().Id))
                    return;

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("INSERT INTO groups_users VALUES ('" + Group.Id + "','" + Session.GetHabbo().Id + "','0', '2', '')");
                }

                Group.Petitions.Add(Session.GetHabbo().Id);
                SendGroupTypeMessage(Group, Room, false);
            }
            else if (Group.Type == 2) // Privado
            {
                Session.SendNotif("Error al unirse, este grupo está cerrado");
                return;
            }
        }

        internal void NotifToLeaveGroup()
        {
            uint GroupId = Request.PopWiredUInt();

            if (!Session.GetHabbo().MyGroups.Contains(GroupId))
                return;

            uint UserId = Request.PopWiredUInt();

            ServerMessage Message = new ServerMessage(Outgoing.NotificationToCancel);
            Message.AppendUInt(UserId);
            Message.AppendInt32(0);
            Session.SendMessage(Message);
        }

        internal void LeaveGroup()
        {
            uint GroupId = Request.PopWiredUInt();

            if (!Session.GetHabbo().MyGroups.Contains(GroupId))
                return;

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null)
                return;

            uint UserId = Request.PopWiredUInt();
            Habbo User = UsersCache.getHabboCache(UserId);
            if (User == null)
                return;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM groups_users WHERE groupid = '" + Group.Id + "' AND userid = '" + User.Id + "' AND acepted = '1'");
            }

            User.MyGroups.Remove(Group.Id);
            Group.MembersCount--;

            ServerMessage Message = new ServerMessage(Outgoing.LeaveGroup);
            Message.AppendUInt(GroupId);
            Message.AppendUInt(UserId);
            Session.SendMessage(Message);

            ServerMessage SendOwnerId = new ServerMessage(Outgoing.SendOwnerId);
            SendOwnerId.AppendUInt(User.Id);
            Session.SendMessage(SendOwnerId);

            ServerMessage ByeGroup = new ServerMessage(Outgoing.ByeGroup);
            ByeGroup.AppendUInt(GroupId);
            Session.SendMessage(ByeGroup);

            if (User.FavoriteGroup == Group.Id)
            {
                User.FavoriteGroup = 0;

                if (User.CurrentRoom != null)
                {
                    ServerMessage UpdateUserGroup = new ServerMessage(Outgoing.UpdateUserGroupRemoving);
                    UpdateUserGroup.AppendInt32(User.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo((uint)UserId).VirtualId);
                    UpdateUserGroup.AppendInt32(-1);
                    UpdateUserGroup.AppendInt32(-1);
                    UpdateUserGroup.AppendString("");
                    User.CurrentRoom.SendMessage(UpdateUserGroup);
                }
            }
        }

        internal void CancelPetition()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null)
                return;

            uint UserId = Request.PopWiredUInt();
            Habbo User = UsersCache.getHabboCache(UserId);
            if (User == null)
                return;

            if (!Group.Petitions.Contains(UserId) || (!Group.Admins.ContainsKey(Session.GetHabbo().Id) && !Session.GetHabbo().HasFuse("can_modify_group")) || User.MyGroups.Contains(GroupId))
                return;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM groups_users WHERE groupid = '" + Group.Id + "' AND userid = '" + UserId + "' AND acepted = '0'");
            }

            Group.Petitions.Remove(UserId);

            ServerMessage Message = new ServerMessage(Outgoing.LeaveGroup);
            Message.AppendUInt(GroupId);
            Message.AppendUInt(UserId);
            Session.SendMessage(Message);
        }

        internal void AcceptMember()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null)
                return;

            uint UserId = Request.PopWiredUInt();
            Habbo User = UsersCache.getHabboCache((uint)UserId);
            if (User == null)
                return;

            if (!Group.Petitions.Contains(UserId) || (!Group.Admins.ContainsKey(Session.GetHabbo().Id) && !Session.GetHabbo().HasFuse("can_modify_group")) || User.MyGroups.Contains(GroupId))
                return;

            string ThisMonth = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[DateTime.Now.Month - 1].Substring(0, 3));
            string DateJoined = ThisMonth + " " + DateTime.Now.Day + ", " + DateTime.Now.Year;

            Group.MembersCount++;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE groups_users SET acepted = '1', rank = '2', joined = '" + DateJoined + "' WHERE groupid = '" + Group.Id + "' AND userid = '" + User.Id + "' AND acepted = '0'");
            }

            Group.Petitions.Remove(UserId);

            ServerMessage Message = new ServerMessage(Outgoing.AcceptNewMember);
            Message.AppendUInt(GroupId);
            Message.AppendUInt(2); // state
            Message.AppendUInt(User.Id);
            Message.AppendString(User.Username);
            Message.AppendString(User.Look);
            Message.AppendString(DateJoined);
            Session.SendMessage(Message);

            User.MyGroups.Add(Group.Id);

            if (User.FavoriteGroup == 0)
            {
                User.FavoriteGroup = Group.Id;

                if (User.CurrentRoom != null)
                {
                    ServerMessage UpdateUserGroup = new ServerMessage(Outgoing.SendGroup);
                    UpdateUserGroup.AppendInt32(1);
                    UpdateUserGroup.AppendUInt(Group.Id);
                    UpdateUserGroup.AppendString(Group.GroupImage);
                    User.CurrentRoom.SendMessage(UpdateUserGroup);

                    ServerMessage UpdateUserGroup2 = new ServerMessage(Outgoing.UpdateUserGroupRemoving);
                    UpdateUserGroup2.AppendInt32(User.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(User.Id).VirtualId);
                    UpdateUserGroup2.AppendUInt(Group.Id);
                    UpdateUserGroup2.AppendInt32(3); // state
                    UpdateUserGroup2.AppendString(Group.Name);
                    User.CurrentRoom.SendMessage(UpdateUserGroup2);
                }
            }
        }

        internal void DeleteFavoriteGroup()
        {
            ServerMessage Message = new ServerMessage(Outgoing.LeaveGroup);
            Message.AppendUInt(Session.GetHabbo().FavoriteGroup);
            Message.AppendUInt(Session.GetHabbo().Id);
            Session.SendMessage(Message);

            ServerMessage SendOwnerId = new ServerMessage(Outgoing.SendOwnerId);
            SendOwnerId.AppendUInt(Session.GetHabbo().Id);
            Session.SendMessage(SendOwnerId);

            ServerMessage ByeGroup = new ServerMessage(Outgoing.ByeGroup);
            ByeGroup.AppendUInt(Session.GetHabbo().FavoriteGroup);
            Session.SendMessage(ByeGroup);

            if (Session.GetHabbo().CurrentRoom != null)
            {
                ServerMessage UpdateUserGroup = new ServerMessage(Outgoing.UpdateUserGroupRemoving);
                UpdateUserGroup.AppendInt32(Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id).VirtualId);
                UpdateUserGroup.AppendInt32(-1);
                UpdateUserGroup.AppendInt32(-1);
                UpdateUserGroup.AppendString("");
                Session.GetHabbo().CurrentRoom.SendMessage(UpdateUserGroup);
            }

            Session.GetHabbo().FavoriteGroup = 0;
        }

        internal void ChangeFavoriteGroup()
        {
            uint GroupId = Request.PopWiredUInt();

            if (!Session.GetHabbo().MyGroups.Contains(GroupId))
                return;

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null)
                return;

            Session.GetHabbo().FavoriteGroup = Group.Id;

            ServerMessage SendOwnerId = new ServerMessage(Outgoing.SendOwnerId);
            SendOwnerId.AppendUInt(Session.GetHabbo().Id);
            Session.SendMessage(SendOwnerId);

            if (Session.GetHabbo().CurrentRoom != null)
            {
                ServerMessage UpdateUserGroup = new ServerMessage(Outgoing.SendGroup);
                UpdateUserGroup.AppendInt32(1);
                UpdateUserGroup.AppendUInt(Group.Id);
                UpdateUserGroup.AppendString(Group.GroupImage);
                Session.GetHabbo().CurrentRoom.SendMessage(UpdateUserGroup);

                ServerMessage UpdateUserGroup2 = new ServerMessage(Outgoing.UpdateUserGroupRemoving);
                UpdateUserGroup2.AppendInt32(Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id).VirtualId);
                UpdateUserGroup2.AppendUInt(Group.Id);
                UpdateUserGroup2.AppendInt32(3); // state
                UpdateUserGroup2.AppendString(Group.Name);
                Session.GetHabbo().CurrentRoom.SendMessage(UpdateUserGroup2);
            }
        }

        internal void GiveAdminGroup()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || (!(Session.GetHabbo().HasFuse("can_modify_group")) && !Group.Admins.ContainsKey(Session.GetHabbo().Id)))
                return;

            uint UserId = Request.PopWiredUInt();
            Habbo User = UsersCache.getHabboCache(UserId);
            if (User == null)
                return;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE groups_users SET rank = '1' WHERE groupid = '" + Group.Id + "' AND userid = '" + User.Id + "'");
            }

            if (!Group.Admins.ContainsKey(UserId))
                Group.Admins.Add(UserId, new GroupUser(UserId, 1, ""));

            ServerMessage Message = new ServerMessage(Outgoing.AcceptNewMember);
            Message.AppendUInt(GroupId);
            Message.AppendInt32(1); // state
            Message.AppendUInt(User.Id);
            Message.AppendString(User.Username);
            Message.AppendString(User.Look);
            Message.AppendString("");
            Session.SendMessage(Message);

            if (User.GetClient() != null)
            {
                if (User.CurrentRoom == null)
                    return;

                RoomUser user = User.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(UserId);
                if (user != null)
                {
                    user.RemoveStatus("flatctrl 0");
                    user.AddStatus("flatctrl 1", "");
                    user.UpdateNeeded = true;

                    Response.Init(Outgoing.RoomRightsLevel);
                    Response.AppendInt32(1);
                    user.GetClient().SendMessage(GetResponse());
                }
            }
        }

        internal void QuitAdminGroup()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || (!(Session.GetHabbo().HasFuse("can_modify_group")) && !Group.Admins.ContainsKey(Session.GetHabbo().Id)))
                return;

            uint UserId = Request.PopWiredUInt();
            Habbo User = UsersCache.getHabboCache(UserId);
            if (User == null)
                return;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE groups_users SET rank = '2' WHERE groupid = '" + Group.Id + "' AND userid = '" + User.Id + "'");
            }

            if (Group.Admins.ContainsKey(UserId))
                Group.Admins.Remove(UserId);

            ServerMessage Message = new ServerMessage(Outgoing.AcceptNewMember);
            Message.AppendUInt(GroupId);
            Message.AppendInt32(2); // state
            Message.AppendUInt(User.Id);
            Message.AppendString(User.Username);
            Message.AppendString(User.Look);
            Message.AppendString(""); // date joined
            Session.SendMessage(Message);

            if (User.GetClient() != null)
            {
                if (User.CurrentRoom == null)
                    return;

                RoomUser user = User.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(UserId);
                if (user != null)
                {
                    user.RemoveStatus("flatctrl 1");
                    user.AddStatus("flatctrl 0", "");
                    user.UpdateNeeded = true;

                    Response.Init(Outgoing.RoomRightsLevel);
                    Response.AppendInt32(0);
                    user.GetClient().SendMessage(GetResponse());
                }
            }
        }

        internal void LoadGroupsOnCata()
        {
            ServerMessage AddGuild = new ServerMessage(Outgoing.SendMyGroups);
            AddGuild.AppendInt32(Session.GetHabbo().MyGroups.Count); // Count of guilds
            foreach (uint xGroupId in Session.GetHabbo().MyGroups)
            {
                GroupItem xGroup = OtanixEnvironment.GetGame().GetGroup().LoadGroup(xGroupId);
                if (xGroup == null)
                    return;

                AddGuild.AppendUInt(xGroup.Id);
                AddGuild.AppendString(xGroup.Name);
                AddGuild.AppendString(xGroup.GroupImage);
                AddGuild.AppendString(xGroup.HtmlColor1);
                AddGuild.AppendString(xGroup.HtmlColor2);
                AddGuild.AppendBoolean((xGroup.Id == Session.GetHabbo().FavoriteGroup) ? true : false);
                AddGuild.AppendUInt(xGroup.OwnerId);
                AddGuild.AppendBoolean(xGroup.Forum == null ? false : true);
            }
            Session.SendMessage(AddGuild);
        }

        internal void ViewGroupForum()
        {
            uint GroupId = Request.PopWiredUInt();
            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || Group.Forum == null)
                return;

            int PagePosts = Request.PopWiredInt32();

            if (Session.GetHabbo().Rank < 4)
            {
                if (Group.Forum.CanReadForum == 1)
                {
                    if (!Group.IsMember(Session.GetHabbo().Id))
                    {
                        Session.SendMessage(Group.Forum.SerializeForum(Group, true, Session.GetHabbo().Id));
                        return;
                    }
                }
                else if (Group.Forum.CanReadForum == 2)
                {
                    if (!Group.Admins.ContainsKey(Session.GetHabbo().Id))
                    {
                        Session.SendMessage(Group.Forum.SerializeForum(Group, true, Session.GetHabbo().Id));
                        return;
                    }
                }
            }

            Session.SendMessage(Group.Forum.SerializeForum(Group, false, Session.GetHabbo().Id));

            int countelements = Group.Forum.Posts.Count - PagePosts;
            if (countelements < 1)
                countelements = Group.Forum.Posts.Count;
            else if (countelements > 20)
                countelements = 20;

            List<GroupPost> pp = new List<GroupPost>(Group.Forum.Posts.Values);
            pp = pp.GetRange(PagePosts, countelements);

            var ForumMessages = new ServerMessage(Outgoing.ForumMessages);
            ForumMessages.AppendUInt(GroupId);
            ForumMessages.AppendInt32(PagePosts);
            ForumMessages.AppendInt32(pp.Count);
            foreach (GroupPost Post in pp)
            {
                Post.Serialize(ForumMessages);
            }
            Session.SendMessage(ForumMessages);
        }

        internal void SaveForumSettings()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || Group.Forum == null || (Group.OwnerId != Session.GetHabbo().Id && !(Session.GetHabbo().HasFuse("can_modify_group"))))
                return;

            if (Group.OwnerId != Session.GetHabbo().Id)
                return;

            int canRead = Request.PopWiredInt32();
            int canWrite = Request.PopWiredInt32();
            int canCreate = Request.PopWiredInt32();
            int canModerate = Request.PopWiredInt32();

            if (canRead < 0 || canRead > 2)
                canRead = 1;

            if (canWrite < 0 || canWrite > 3)
                canWrite = 1;

            if (canCreate < 0 || canCreate > 3)
                canCreate = 1;

            if (canModerate < 0 || canModerate > 1)
                canModerate = 0;

            Group.Forum.CanReadForum = canRead;
            Group.Forum.CanWriteForum = canWrite;
            Group.Forum.CanCreateForum = canCreate;
            Group.Forum.CanModerateForum = canModerate;

            ServerMessage customAlert = new ServerMessage(Outgoing.CustomAlert);
            customAlert.AppendString("forums.forum_settings_updated");
            customAlert.AppendInt32(0);
            Session.SendMessage(customAlert);

            Session.SendMessage(Group.Forum.SerializeForum(Group, false, Session.GetHabbo().Id));

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE groups_forums SET can_read = '" + canRead + "', can_write = '" + canWrite + "', can_create = '" + canCreate + "', can_moderate = '" + canModerate + "' WHERE groupid = '" + GroupId + "'");
            }

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModForumCanReadSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModForumCanPostThrdSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModForumCanPostSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModForumCanModerateSeen", 1);
        }

        internal void CreateForumPost()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || Group.Forum == null)
                return;
    
            if (Group.Forum.CanCreateForum == 1)
            {
                if (!Group.IsMember(Session.GetHabbo().Id))
                {
                    ServerMessage customAlert = new ServerMessage(Outgoing.CustomAlert);
                    customAlert.AppendString("forums.error.access_denied");
                    customAlert.AppendInt32(0);
                    Session.SendMessage(customAlert);

                    return;
                }
            }
            else if (Group.Forum.CanCreateForum == 2)
            {
                if (!Group.Admins.ContainsKey(Session.GetHabbo().Id))
                {
                    ServerMessage customAlert = new ServerMessage(Outgoing.CustomAlert);
                    customAlert.AppendString("forums.error.access_denied");
                    customAlert.AppendInt32(0);
                    Session.SendMessage(customAlert);

                    return;
                }
            }
            else if (Group.Forum.CanCreateForum == 3)
            {
                if (Group.OwnerId != Session.GetHabbo().Id)
                {
                    ServerMessage customAlert = new ServerMessage(Outgoing.CustomAlert);
                    customAlert.AppendString("forums.error.access_denied");
                    customAlert.AppendInt32(0);
                    Session.SendMessage(customAlert);

                    return;
                }
            }

            uint ThreadId = Request.PopWiredUInt();
            string PostIssue = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            if (ThreadId == 0 && (PostIssue.Length < 10 || PostIssue.Length > 120))
                return;

            string PostMessage = Request.PopFixedString(); //OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            if (PostMessage.Length < 10 || PostMessage.Length > 4000)
                return;

            if (BlackWordsManager.Check(PostIssue, BlackWordType.Hotel, Session, "<CrearForoDeGrupo>"))
                PostIssue = "Mensaje bloqueado por el filtro bobba.";

            if (BlackWordsManager.Check(PostMessage, BlackWordType.Hotel, Session, "<CrearForoDeGrupo>"))
                PostMessage = "Mensaje bloqueado por el filtro bobba.";

            uint GroupForumMessageId = 0;
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE groups_forums SET totalPosts = totalPosts + 1 WHERE groupid = " + GroupId);

                dbClient.setQuery("INSERT INTO groups_forums_boards (sub_post_id, issue, message, owner_id, date_created, groupid) VALUES ('" + ThreadId + "', @issue, @message, '" + Session.GetHabbo().Id + "', '" + OtanixEnvironment.GetUnixTimestamp() + "','" + GroupId + "')");
                dbClient.addParameter("issue", PostIssue);
                dbClient.addParameter("message", PostMessage);
                GroupForumMessageId = (uint)dbClient.insertQuery();
            }

            Group.Forum.totalmessages++;
            Group.Forum.LastOwnerId = Session.GetHabbo().Id;
            Group.Forum.LastOwnerName = Session.GetHabbo().Username;
            Group.Forum.LastUnix = OtanixEnvironment.GetUnixTimestamp();

            if (ThreadId == 0) // Nuevo Hilo:
            {
                GroupPost groupPost = new GroupPost(GroupForumMessageId, ThreadId, PostIssue, PostMessage, Session.GetHabbo().Id, Session.GetHabbo().Username, (double)OtanixEnvironment.GetUnixTimestamp(), (uint)GroupId, false, false, 0, 1, "unknown", false);
                if (groupPost == null)
                    return;

                if (!Group.Forum.Posts.ContainsKey(GroupForumMessageId))
                    Group.Forum.Posts.Add(GroupForumMessageId, groupPost);

                ServerMessage creatingPost = new ServerMessage(Outgoing.CreatingForumPost);
                creatingPost.AppendUInt(GroupId);
                groupPost.Serialize(creatingPost);
                Session.SendMessage(creatingPost);

                groupPost.LastOwnerId = Session.GetHabbo().Id;
                groupPost.LastOwnerName = Session.GetHabbo().Username;
                groupPost.LastUnix = groupPost.UnixCreated;
            }
            else // Post:
            {
                if (!Group.Forum.Posts.ContainsKey(ThreadId))
                    return;

                GroupSubPost groupPost = new GroupSubPost(GroupForumMessageId, ThreadId, PostIssue, PostMessage, Session.GetHabbo().Id, Session.GetHabbo().Username, Session.GetHabbo().Look, (double)OtanixEnvironment.GetUnixTimestamp(), (uint)GroupId, 0, Group.Forum.Posts[ThreadId].SubPostsCount++, "unknown");
                if (groupPost == null)
                    return;

                if (!Group.Forum.Posts[ThreadId].SubPosts.ContainsKey(GroupForumMessageId))
                    Group.Forum.Posts[ThreadId].SubPosts.Add(GroupForumMessageId, groupPost);

                ServerMessage creatingPost = new ServerMessage(Outgoing.CreatePost);
                creatingPost.AppendUInt(GroupId);
                creatingPost.AppendUInt(ThreadId);
                groupPost.SerializeSubPost(creatingPost);
                Session.SendMessage(creatingPost);


                Group.Forum.Posts[ThreadId].LastOwnerId = Session.GetHabbo().Id;
                Group.Forum.Posts[ThreadId].LastOwnerName = Session.GetHabbo().Username;
                Group.Forum.Posts[ThreadId].LastUnix = groupPost.UnixCreated;
            }
        }

        internal void PostClosedAndDisabled()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || Group.Forum == null)
                return;

            if (!(Session.GetHabbo().HasFuse("can_modify_group")) && !Group.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                ServerMessage customAlertt = new ServerMessage(Outgoing.CustomAlert);
                customAlertt.AppendString("forums.error.access_denied");
                customAlertt.AppendInt32(0);
                Session.SendMessage(customAlertt);

                return;
            }

            uint ThreadId = Request.PopWiredUInt();
            if (!Group.Forum.Posts.ContainsKey(ThreadId))
                return;

            GroupPost Post = Group.Forum.Posts[ThreadId];
            if (Post == null)
                return;

            if (Group.Forum.CanModerateForum == 0)
            {
                if (!Group.Admins.ContainsKey(Session.GetHabbo().Id) || Group.OwnerId != Session.GetHabbo().Id)
                    return;
            }
            else if (Group.Forum.CanModerateForum == 1)
            {
                if (Group.OwnerId != Session.GetHabbo().Id)
                    return;
            }

            bool Disabled = Request.PopWiredBoolean();
            bool Closed = Request.PopWiredBoolean();

            Post.Disabled = Disabled;
            Post.Closed = Closed;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE groups_forums_boards SET closed = '" + OtanixEnvironment.BoolToEnum(Post.Closed) + "', disabled = '" + OtanixEnvironment.BoolToEnum(Post.Disabled) + "' WHERE id = '" + Post.Id + "'");
            }

            ServerMessage creatingPost = new ServerMessage(Outgoing.CreatingForumPost);
            creatingPost.AppendUInt(GroupId);
            Post.Serialize(creatingPost);
            Session.SendMessage(creatingPost);

            ServerMessage customAlert = new ServerMessage(Outgoing.CustomAlert);
            if (Disabled == false)
            {
                customAlert.AppendString("forums.thread.unpinned");
            }
            else if (Disabled == true)
            {
                customAlert.AppendString("forums.thread.pinned");
            }
            else if (Closed == false)
            {
                customAlert.AppendString("forums.thread.unlocked");
            }
            else if (Closed == true)
            {
                customAlert.AppendString("forums.thread.locked");
            }
            customAlert.AppendInt32(0);
            Session.SendMessage(customAlert);
        }

        internal void PostHidden()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || Group.Forum == null)
                return;

            if (!(Session.GetHabbo().HasFuse("can_modify_group")) && !Group.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                ServerMessage customAlertt = new ServerMessage(Outgoing.CustomAlert);
                customAlertt.AppendString("forums.error.access_denied");
                customAlertt.AppendInt32(0);
                Session.SendMessage(customAlertt);

                return;
            }

            uint ThreadId = Request.PopWiredUInt();
            if (!Group.Forum.Posts.ContainsKey(ThreadId))
                return;

            GroupPost Post = Group.Forum.Posts[ThreadId];
            if (Post == null)
                return;

            int HiddenMode = Request.PopWiredInt32();

            Post.HiddenMode = HiddenMode;
            if (HiddenMode == 10)
                Post.HiddenBy = Session.GetHabbo().Username;
            else
                Post.HiddenBy = "unknown";

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE groups_forums_boards SET hiddenmode = '" + Post.HiddenMode + "', hiddenby = '" + Post.HiddenBy + "' WHERE id = '" + Post.Id + "'");
            }

            ServerMessage creatingPost = new ServerMessage(Outgoing.CreatingForumPost);
            creatingPost.AppendUInt(GroupId);
            Post.Serialize(creatingPost);
            Session.SendMessage(creatingPost);

            ServerMessage customAlert = new ServerMessage(Outgoing.CustomAlert);
            if (HiddenMode == 10)
            {
                customAlert.AppendString("forums.thread.hidden");
            }
            else if (HiddenMode == 1)
            {
                customAlert.AppendString("forums.thread.restored");
            }
            customAlert.AppendInt32(0);
            Session.SendMessage(customAlert);
        }

        internal void OpenPost()
        {
            uint GroupId = Request.PopWiredUInt();
            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || Group.Forum == null)
                return;

            uint ThreadId = Request.PopWiredUInt();
            if (!Group.Forum.Posts.ContainsKey(ThreadId))
                return;

            GroupPost Post = Group.Forum.Posts[ThreadId];
            if (Post == null)
                return;

            int PagePosts = Request.PopWiredInt32();

            if (Post.HiddenMode == 10 && !(Session.GetHabbo().HasFuse("can_modify_group")) && !Group.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                ServerMessage customAlert = new ServerMessage(Outgoing.CustomAlert);
                customAlert.AppendString("forums.error.access_denied");
                customAlert.AppendInt32(0);
                Session.SendMessage(customAlert);

                return;
            }

            int countelements = Post.SubPosts.Count - PagePosts;
            if (Post.subPosts.Count == PagePosts)
            {
                countelements = 20;
                PagePosts = PagePosts - 20;
            }
            else
            {
                if (countelements < 0)
                    countelements = Post.SubPosts.Count;
                else if (countelements > 20)
                    countelements = 20;
            }

            List<GroupSubPost> pp = new List<GroupSubPost>(Post.SubPosts.Values);
            pp = pp.GetRange(PagePosts, countelements);

            ServerMessage Message = new ServerMessage(Outgoing.OpenPost);
            Message.AppendUInt(GroupId);
            Message.AppendUInt(ThreadId);
            Message.AppendInt32(PagePosts);
            Message.AppendInt32(pp.Count);
            foreach (GroupSubPost subpost in pp)
            {
                subpost.SerializeSubPost(Message);
            }
            Session.SendMessage(Message);
        }

        internal void SubPostHidden()
        {
            uint GroupId = Request.PopWiredUInt();
            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || Group.Forum == null)
                return;

            if (!(Session.GetHabbo().HasFuse("can_modify_group")) && !Group.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                ServerMessage customAlertt = new ServerMessage(Outgoing.CustomAlert);
                customAlertt.AppendString("forums.error.access_denied");
                customAlertt.AppendInt32(0);
                Session.SendMessage(customAlertt);

                return;
            }

            uint ThreadId = Request.PopWiredUInt();
            if (!Group.Forum.Posts.ContainsKey(ThreadId))
                return;

            GroupPost Post = Group.Forum.Posts[ThreadId];
            if (Post == null)
                return;

            uint PostID = Request.PopWiredUInt();
            if (!Post.SubPosts.ContainsKey(PostID))
                return;

            var SubPost = Post.SubPosts[PostID];
            if (SubPost == null)
                return;

            int HiddenMode = Request.PopWiredInt32();

            SubPost.HiddenMode = HiddenMode;
            if (HiddenMode == 10)
                SubPost.HiddenBy = Session.GetHabbo().Username;
            else
                SubPost.HiddenBy = "unknown";

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE groups_forums_boards SET hiddenmode = '" + SubPost.HiddenMode + "', hiddenby = '" + SubPost.HiddenBy + "' WHERE id = '" + SubPost.Id + "'");
            }

            ServerMessage creatingPost = new ServerMessage(Outgoing.CreatePost);
            creatingPost.AppendUInt(GroupId);
            creatingPost.AppendUInt(ThreadId);
            SubPost.SerializeSubPost(creatingPost);
            Session.SendMessage(creatingPost);

            if (HiddenMode == 10)
            {
                NotificaStaff.NotificaUser(LanguageLocale.GetValue("imagem.forums.img.escondido"), LanguageLocale.GetValue("imagem.forums.msg1"), Session);
            }
            else if (HiddenMode == 1)
            {
                NotificaStaff.NotificaUser(LanguageLocale.GetValue("imagem.forums.img.restaurado"), LanguageLocale.GetValue("imagem.forums.msg"), Session);
            }
        }

        internal void MyForums()
        {
            int Mode = Request.PopWiredInt32();
            List<GroupItem> GroupForumList = new List<GroupItem>();

            ServerMessage Message = new ServerMessage(Outgoing.MyGroupsForums);
            Message.AppendInt32(Mode);

            if (Mode == 0)
            {
                GroupForumList = OtanixEnvironment.GetGame().GetGroup().LoadForumsMoreActive();
            }
            else if (Mode == 1)
            {
                GroupForumList = OtanixEnvironment.GetGame().GetGroup().LoadForumsMoreActive();
            }
            else if (Mode == 2)
            {
                GroupForumList = Session.GetHabbo().ForumGroups();
            }

            Message.AppendInt32(GroupForumList.Count); // Hay un total de X (siempre va a ser <= 20)(Stable method)
            Message.AppendInt32(0); // ??
            Message.AppendInt32(GroupForumList.Count); // cojemos los 20 primeros (siempre va a ser <= 20)(Stable method)
            foreach (GroupItem group in GroupForumList)
            {
                Message.AppendUInt(group.Id);
                Message.AppendString(group.Name);
                Message.AppendString("");
                Message.AppendString(group.GroupImage);
                Message.AppendInt32(0); // ??
                Message.AppendInt32(0); // ??
                Message.AppendInt32(group.Forum.TotalMessages); // mensajes
                Message.AppendInt32(0); // no leidos
                Message.AppendInt32(0); // ??
                Message.AppendUInt(group.Forum.LastOwnerId); // ultimo mensaje de:
                Message.AppendString(group.Forum.LastOwnerName); // ultimo mensaje de:
                Message.AppendInt32((group.Forum.LastUnix == 0) ? 0 : (int)(OtanixEnvironment.GetUnixTimestamp() - group.Forum.LastUnix)); // hace X segundos
            }
            Session.SendMessage(Message);
        }

        internal void ReportForumPost()
        {
            Session.SendNotif("Esta función todavía está desactivada...");
            return;

            /*if (Session == null || Session.GetHabbo() == null)
                return;

            int GroupId = Request.PopWiredInt32();
            var Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || Group.Forum == null)
                return;

            uint ThreadId = Request.PopWiredUInt();
            if (!Group.Forum.Posts.ContainsKey(ThreadId))
                return;

            var Post = Group.Forum.Posts[ThreadId];
            if (Post == null)
                return;

            int Type = Request.PopWiredInt32();
            string Report = Request.PopFixedString();

            OtanixEnvironment.GetGame().GetModerationTool().SendNewTicket(Session, Type, 0, Report);

            ServerMessage Messagee = new ServerMessage(Outgoing.TicketAlert);
            Messagee.AppendInt32(0);
            Session.SendMessage(Messagee);*/
        }

        internal void DeleteGroup()
        {
            uint GroupId = Request.PopWiredUInt();

            GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
            if (Group == null || (Group.OwnerId != Session.GetHabbo().Id) && !(Session.GetHabbo().HasFuse("can_modify_group")))
                return;

            Group.Delete();
        }
    }
}