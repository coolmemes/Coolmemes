using Butterfly.HabboHotel.Groups;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Butterfly.HabboHotel.Group
{
    class GroupItem
    {
        /// <summary>
        /// Id del Grupo.
        /// </summary>
        internal uint Id;

        /// <summary>
        /// Nombre del Grupo.
        /// </summary>
        internal string Name;

        /// <summary>
        /// Descripción del Grupo.
        /// </summary>
        internal string Description;

        /// <summary>
        /// Id de la sala del Grupo.
        /// </summary>
        internal uint RoomId;

        /// <summary>
        /// Color principal del Grupo.
        /// </summary>
        internal int CustomColor1;

        /// <summary>
        /// Color secundario del Grupo.
        /// </summary>
        internal int CustomColor2;

        /// <summary>
        /// Item base de la imagen de la placa del Grupo.
        /// </summary>
        internal int GroupBase;

        /// <summary>
        /// Color base de la imagen de la placa del Grupo.
        /// </summary>
        internal int GroupBaseColor;

        /// <summary>
        /// Posición base de la imagen de la placa del Grupo.
        /// </summary>
        internal int GroupBasePosition;

        /// <summary>
        /// Elemento de la imagen de la placa de Grupo.
        /// </summary>
        internal int[] GroupItem1;

        /// <summary>
        /// Elemento de la imagen de la placa de Grupo.
        /// </summary>
        internal int[] GroupItem2;

        /// <summary>
        /// Elemento de la imagen de la placa de Grupo.
        /// </summary>
        internal int[] GroupItem3;

        /// <summary>
        /// Elemento de la imagen de la placa de Grupo.
        /// </summary>
        internal int[] GroupItem4;

        /// <summary>
        /// Código de la imagen de la placa de Grupo.
        /// </summary>
        internal string GroupImage;

        /// <summary>
        /// Color HTML principal del color de grupo.
        /// </summary>
        internal string HtmlColor1;

        /// <summary>
        /// Color HTML secundario del color de grupo.
        /// </summary>
        internal string HtmlColor2;

        /// <summary>
        /// Fecha de creación del grupo.
        /// </summary>
        internal string DateCreated;

        /// <summary>
        /// Id del dueño del Grupo.
        /// </summary>
        internal uint OwnerId;

        /// <summary>
        /// Nombre del dueño del Grupo.
        /// </summary>
        internal string OwnerName;

        /// <summary>
        /// Tipo de grupo: (0 = abierto, 1 = privado, 2 = cerrado)
        /// </summary>
        internal int Type;

        /// <summary>
        /// Los administradores del Grupo pueden decorar y gestionar la sala de reunión del grupo
        /// </summary>
        internal int RightsType;

        /// <summary>
        /// Número de miembros en el Grupo.
        /// </summary>
        internal int MembersCount;

        /// <summary>
        /// Se o grupo tem chat.
        /// </summary>
        internal bool temChat;

        /// <summary>
        /// Usuarios que están pendientes de ser aceptados en el Grupo.
        /// </summary>
        private List<uint> petitionList;

        /// <summary>
        /// Usuarios Administradores del Grupo.
        /// </summary>
        private Dictionary<uint, GroupUser> adminList;

        /// <summary>
        /// Usuarios do Grupo.
        /// </summary>
        private List<uint> membrosGrupo;


        /// <summary>
        /// Foro de Grupo.
        /// </summary>
        internal GroupForum Forum;

        internal Dictionary<uint, GroupUser> Admins
        {
            get
            {
                if (adminList == null)
                    LoadAdmins();

                return adminList;
            }
        }

        internal List<uint> membrosGruposPega
        {
            get
            {
                if (membrosGrupo == null)
                    getMembrosGrupo();

                return membrosGrupo;
            }
        }

        internal List<uint> Petitions
        {
            get
            {
                if (petitionList == null)
                    LoadPetitions();

                return petitionList;
            }
        }

        internal GroupItem()
        {
            GroupItem1 = null;
            GroupItem2 = null;
            GroupItem3 = null;
            GroupItem4 = null;
        }

        internal GroupItem(DataRow dRow)
        {
            Id = Convert.ToUInt32(dRow["id"]);
            Name = (string)dRow["name"];
            Description = (string)dRow["description"];
            RoomId = Convert.ToUInt32(dRow["roomid"]);
            CustomColor1 = (int)dRow["customcolor1"];
            CustomColor2 = (int)dRow["customcolor2"];
            temChat = OtanixEnvironment.EnumToBool((string)dRow["temChat"]);
            GroupBase = (int)dRow["groupbase"];
            GroupBaseColor = (int)dRow["groupbasecolor"];
            GroupBasePosition = (int)dRow["groupbaseposition"];
            var zGroupItem1 = ((string)dRow["groupitem1"]).Split(';');
            if (zGroupItem1.Count() == 3)
                GroupItem1 = new int[] { int.Parse(zGroupItem1[0]), int.Parse(zGroupItem1[1]), int.Parse(zGroupItem1[2]) };
            var zGroupItem2 = ((string)dRow["groupitem2"]).Split(';');
            if (zGroupItem2.Count() == 3)
                GroupItem2 = new int[] { int.Parse(zGroupItem2[0]), int.Parse(zGroupItem2[1]), int.Parse(zGroupItem2[2]) };
            var zGroupItem3 = ((string)dRow["groupitem3"]).Split(';');
            if (zGroupItem3.Count() == 3)
                GroupItem3 = new int[] { int.Parse(zGroupItem3[0]), int.Parse(zGroupItem3[1]), int.Parse(zGroupItem3[2]) };
            var zGroupItem4 = ((string)dRow["groupitem4"]).Split(';');
            if (zGroupItem4.Count() == 3)
                GroupItem4 = new int[] { int.Parse(zGroupItem4[0]), int.Parse(zGroupItem4[1]), int.Parse(zGroupItem4[2]) };
            GroupImage = (string)dRow["groupimage"];
            HtmlColor1 = (string)dRow["htmlcolor1"];
            HtmlColor2 = (string)dRow["htmlcolor2"];
            DateCreated = (string)dRow["datecreated"];
            OwnerId = Convert.ToUInt32(dRow["ownerid"]);
            OwnerName = UsersCache.getUsernameById(OwnerId);
            Type = Convert.ToInt32(dRow["type"]);
            RightsType = Convert.ToInt32(dRow["rightsType"]);

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT COUNT(*) FROM groups_users WHERE groupid = '" + Id + "' AND acepted = '1'");
                MembersCount = dbClient.getInteger();

                dbClient.setQuery("SELECT * FROM groups_forums WHERE groupid = " + Id);
                DataRow dForum = dbClient.getRow();

                if(dForum != null)
                {
                    Forum = new GroupForum(Id, Convert.ToInt32(dForum["can_read"]), Convert.ToInt32(dForum["can_write"]), Convert.ToInt32(dForum["can_create"]), Convert.ToInt32(dForum["can_moderate"]), true);
                }
            }
        }

        internal void CreateGroupForum(uint groupid, int canreadforum, int canwriteforum, int cancreateforum, int canmoderateforum)
        {
            Forum = new GroupForum(groupid, canreadforum, canwriteforum, cancreateforum, canmoderateforum, false);
        }

        internal void LoadAdmins()
        {
            adminList = new Dictionary<uint, GroupUser>();

            DataTable dTable = null;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT userid, rank, joined FROM groups_users WHERE groupid = '" + Id + "' AND rank < 2 ORDER BY rank ASC");
                dTable = dbClient.getTable();
            }

            foreach (DataRow dRow in dTable.Rows)
            {
                if (!adminList.ContainsKey(Convert.ToUInt32(dRow["userid"])))
                    adminList.Add(Convert.ToUInt32(dRow["userid"]), new GroupUser(Convert.ToUInt32(dRow["userid"]), Convert.ToUInt32(dRow["rank"]), (string)dRow["joined"]));
            }
        }

        internal List<uint> getMembrosGrupo()
        {
            membrosGrupo = new List<uint>();

            DataTable dTable = null;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT userid FROM groups_users WHERE groupid = '" + Id + "' ORDER BY userid ASC");
                dTable = dbClient.getTable();
            }

            foreach (DataRow dRow in dTable.Rows)
            {
                if (!membrosGrupo.Contains(Convert.ToUInt32(dRow["userid"])))
                    membrosGrupo.Add(Convert.ToUInt32(dRow["userid"]));
            }

            return membrosGrupo;
        }

        internal void LoadPetitions()
        {
            petitionList = new List<uint>();
            DataTable dTable = null;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT userid FROM groups_users WHERE groupid = '" + Id + "' AND acepted = '0'");
                dTable = dbClient.getTable();
            }

            foreach (DataRow dRow in dTable.Rows)
            {
                if(!petitionList.Contains(Convert.ToUInt32(dRow["userid"])))
                    petitionList.Add(Convert.ToUInt32(dRow["userid"]));
            }
        }

        internal void GetGroupUsersPage(int PageCount, ServerMessage Message)
        {
            List<GroupUser> users = new List<GroupUser>();

            DataTable dTable = null;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT userid, rank, joined FROM groups_users WHERE groupid = '" + Id + "' ORDER BY rank ASC LIMIT " + (PageCount * 14) + ", 14");
                dTable = dbClient.getTable();
            }

            Message.AppendInt32(dTable.Rows.Count);
            foreach (DataRow dRow in dTable.Rows)
            {
                var zUser = UsersCache.getHabboCache(Convert.ToUInt32(dRow["userid"]));
                if (zUser == null)
                {
                    Message.AppendInt32(2);
                    Message.AppendInt32(0);
                    Message.AppendString("BUGUED");
                    Message.AppendString("");
                    Message.AppendString(""); // Se ha unido el.

                    continue;
                }

                Message.AppendInt32((int)dRow["rank"]);
                Message.AppendInt32((int)dRow["userid"]);
                Message.AppendString(zUser.Username);
                Message.AppendString(zUser.Look);
                Message.AppendString((string)dRow["joined"]); // Se ha unido el.
            }
        }

        internal bool IsMember(uint Userid)
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT null FROM groups_users WHERE groupid = '" + Id + "' AND userid = '" + Userid + "' AND acepted = '1'");
                if (dbClient.getRow() != null)
                {
                    return true;
                }
            }
            return false;
        }

        internal void Delete()
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM groups WHERE id = '" + Id + "'");
                dbClient.runFastQuery("DELETE FROM groups_users WHERE groupid = '" + Id + "'");

                if (Forum != null)
                {
                    dbClient.runFastQuery("DELETE FROM groups_forums WHERE groupid = '" + Id + "'");
                    dbClient.runFastQuery("DELETE FROM groups_forums_boards WHERE groupid = '" + Id + "'");
                }

                dbClient.runFastQuery("UPDATE users SET FavoriteGroup = '0' WHERE FavoriteGroup = '" + Id + "'");
                dbClient.runFastQuery("UPDATE rooms SET groupId = '0' WHERE id = '" + RoomId + "'");
            }

            OtanixEnvironment.GetGame().GetGroup().RemoveGroupQueue(Id);

            RoomData roomData = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            if (roomData != null)
            {
                roomData.GroupId = 0;

                // Reload Room:
                Room TargetRoom = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(roomData.Id);
                if (TargetRoom != null && TargetRoom.GetRoomUserManager() != null)
                {
                    List<RoomUser> users = new List<RoomUser>(TargetRoom.GetRoomUserManager().UserList.Values);

                    OtanixEnvironment.GetGame().GetRoomManager().UnloadRoom(TargetRoom);

                    if (users != null && users.Count > 0)
                    {
                        foreach (RoomUser user in users)
                        {
                            if (user != null && user.GetClient() != null)
                                user.GetClient().GetMessageHandler().enterOnRoom3(TargetRoom);
                        }
                    }
                }
            }
        }
    }
}
