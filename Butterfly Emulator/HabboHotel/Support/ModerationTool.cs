using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Rooms.RoomIvokedItems;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using Butterfly.HabboHotel.Users;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.Support.ModActions;
using Butterfly.HabboHotel.Mutes;
using Butterfly.HabboHotel.ChatMessageStorage;

namespace Butterfly.HabboHotel.Support
{
    public class ModerationTool
    {
        #region General

        internal List<SupportTicket> Tickets;
        internal Dictionary<uint, ModCategory> ModCategories;
        internal List<string> UserMessagePresets;
        internal List<string> RoomMessagePresets;

        internal ModerationTool()
        {
            Tickets = new List<SupportTicket>();
            UserMessagePresets = new List<string>();
            RoomMessagePresets = new List<string>();
        }

        internal void LoadModActions(IQueryAdapter dbClient)
        {
            ModCategories = new Dictionary<uint, ModCategory>();

            dbClient.setQuery("SELECT * FROM moderation_categories");
            foreach (DataRow dRow in dbClient.getTable().Rows)
            {
                uint id = Convert.ToUInt32(dRow["id"]);
                uint parentId = Convert.ToUInt32(dRow["parent_id"]);
                string topicName = (string)dRow["topicName"];
                uint topicId = Convert.ToUInt32(dRow["topicId"]);
                string topicAction = (string)dRow["topicAction"];

                if (parentId == 0) // main case:
                {
                    ModCategory modCategory = new ModCategory(id, topicName);
                    ModCategories.Add(id, modCategory);
                }
                else
                {
                    if (ModCategories.ContainsKey(parentId))
                    {
                        ModCategories[parentId].AddSubCategory(topicId, topicName, topicAction);
                    }
                }
            }
        }

        internal ServerMessage SerializeCfhTopics()
        {
            ServerMessage Message = new ServerMessage(Outgoing.CfhTopicsInitMessageParser);
            Message.AppendInt32(ModCategories.Count);
            foreach(ModCategory modCategory in ModCategories.Values)
            {
                Message.AppendString(modCategory.CategoryName);
                Message.AppendInt32(modCategory.SubCategories.Count);
                foreach(ModSubCategory modSubCategory in modCategory.SubCategories)
                {
                    Message.AppendString(modSubCategory.SubCategoryName); // NAME
                    Message.AppendUInt(modSubCategory.SubCategoryId); // ID
                    Message.AppendString(modSubCategory.SubCategoryAction); // CONSEQUENCE
                }
            }
            
            return Message;
        }

        internal ServerMessage SerializeTool(Habbo habbo)
        {
            var Response = new ServerMessage(Outgoing.OpenModTools);
            Response.AppendInt32(Tickets.Count);
            foreach (SupportTicket ticket in Tickets)
            {
                ticket.SerializeBody(Response);
            }

            Response.AppendInt32(UserMessagePresets.Count);
            foreach (var Preset in UserMessagePresets)
            {
                Response.AppendString(Preset);
            }

            Response.AppendInt32(0);
            /*Response.AppendInt32(ModActions.Count);
            foreach (ModAction modAction in ModActions)
            {
                Response.AppendString(modAction.ModName);
                Response.AppendBoolean(modAction.Used);
                Response.AppendInt32(modAction.SubActions.Count);

                foreach (ModSubAction modSubAction in modAction.SubActions)
                {
                    Response.AppendString(modSubAction.ModName);
                    Response.AppendString(modSubAction.Message);
                    Response.AppendInt32(modSubAction.BanHours);
                    Response.AppendInt32(modSubAction.BanAvatarHours);
                    Response.AppendInt32(modSubAction.MuteHours);
                    Response.AppendInt32(modSubAction.TradingHours);
                    Response.AppendString(modSubAction.ReminderText);
                    Response.AppendBoolean(false); // showHabboWay
                }
            }*/

            Response.AppendBoolean(habbo.HasFuse("fuse_tickets")); // ticket_queue fuse
            Response.AppendBoolean(habbo.HasFuse("fuse_chatlogs")); // chatlog fuse
            Response.AppendBoolean(habbo.HasFuse("fuse_user_alert")); // message / caution fuse - user info
            Response.AppendBoolean(habbo.HasFuse("fuse_user_kick")); // kick fuse - user info
            Response.AppendBoolean(habbo.HasFuse("fuse_ban")); // ban fuse
            Response.AppendBoolean(habbo.HasFuse("fuse_room_alert")); // send caution
            Response.AppendBoolean(habbo.HasFuse("fuse_room_kick")); // kick check

            Response.AppendInt32(RoomMessagePresets.Count);
            foreach (var Preset in RoomMessagePresets)
            {
                Response.AppendString(Preset);
            }

            return Response;
        }

        #endregion

        #region Message Presets

        internal void LoadMessagePresets(IQueryAdapter dbClient)
        {
            UserMessagePresets.Clear();
            RoomMessagePresets.Clear();

            dbClient.setQuery("SELECT type,message FROM moderation_presets WHERE enabled = 2");
            var Data = dbClient.getTable();

            if (Data == null)
                return;

            foreach (DataRow Row in Data.Rows)
            {
                var Message = (String)Row["message"];

                switch (Row["type"].ToString().ToLower())
                {
                    case "message":

                        UserMessagePresets.Add(Message);
                        break;

                    case "roommessage":

                        RoomMessagePresets.Add(Message);
                        break;
                }
            }
        }

        #endregion

        #region Support Tickets
        internal void SendNewTicket(GameClient Session, int Category, int ReportedUser, String Message, string[] Chats)
        {
            if (Session.GetHabbo().CurrentRoomId <= 0)
            {
                return;
            }

            var Data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(Session.GetHabbo().CurrentRoomId);
            if (Data == null)
                return;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE user_info SET cfhs = cfhs + 1 WHERE user_id = " + Session.GetHabbo().Id + "");
            }

            var Ticket = new SupportTicket((Tickets.Count + 1), 1, Category, Session.GetHabbo().Id, Session.GetHabbo().Username, ReportedUser, Message, Data.Id, Data.Name, OtanixEnvironment.GetUnixTimestamp(), Chats);


            NotificaStaff.Notifica(LanguageLocale.GetValue("alertastaff.imagem"), LanguageLocale.GetValue("alertastaff.mensagem"));

            Tickets.Add(Ticket);
            SendTicketToModerators(Ticket);
        }

        internal SupportTicket GetTicket(uint TicketId)
        {
            foreach (var Ticket in Tickets)
            {
                if (Ticket.TicketId == TicketId)
                {
                    return Ticket;
                }
            }
            return null;
        }

        internal void PickTicket(GameClient Session, uint TicketId)
        {
            var Ticket = GetTicket(TicketId);

            if (Ticket == null || Ticket.Status != TicketStatus.OPEN)
            {
                return;
            }

            Ticket.Status = TicketStatus.PICKED;
            Ticket.ModeratorId = Session.GetHabbo().Id;
            Ticket.Timestamp = OtanixEnvironment.GetUnixTimestamp();
            SendTicketToModerators(Ticket);
        }

        internal void ReleaseTicket(GameClient Session, uint TicketId)
        {
            var Ticket = GetTicket(TicketId);

            if (Ticket == null || Ticket.Status != TicketStatus.PICKED || Ticket.ModeratorId != Session.GetHabbo().Id)
            {
                return;
            }

            Ticket.Status = TicketStatus.OPEN;
            SendTicketToModerators(Ticket);
        }

        internal void CloseTicket(GameClient Session, uint TicketId, int Result)
        {
            var Ticket = GetTicket(TicketId);

            if (Ticket == null || Ticket.Status != TicketStatus.PICKED || Ticket.ModeratorId != Session.GetHabbo().Id)
            {
                return;
            }

            var Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Ticket.SenderId);

            TicketStatus NewStatus;
            string Message = "";

            var sinceLast = DateTime.Now - Session.GetHabbo().lastTicketRead;

            if (sinceLast.Seconds < 3)
                Session.GetHabbo().skippedTickets++;

            Session.GetHabbo().readTickets++;
            Session.GetHabbo().lastTicketRead = DateTime.Now;

            switch (Result)
            {
                case 1:

                    // Close as Useless
                    Message = "Ehem... ¿estás segur@ de que nos has pedido ayuda? No parece que tengas ningún problema que debamos resolver. Para evitar malentendidos, señala la parte de la conversación que quieres denunciar.";
                    NewStatus = TicketStatus.INVALID;
                    break;

                case 2:

                    // Close as Abusive
                    Message = "¡Hey, tus llamadas de ayuda echan humo! Por favor, un poquito de paciencia para no saturar el sistema.";
                    NewStatus = TicketStatus.ABUSIVE;
                   
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("UPDATE user_info SET cfhs_abusive = cfhs_abusive + 1 WHERE user_id = " + Ticket.SenderId + "");
                    }

                    break;

                case 3:
                default:

                    // Close As Resolved
                    Message = "¡Listo! Llamada recibida. Movimos unos cuantos hilos para solucionar el problema. Y no dudes en usar la opción \"Ignorar\": ojos que no ven, corazón que no siente.";
                    NewStatus = TicketStatus.RESOLVED;
                    break;
            }

            if (Client != null)
            {
                var notifyResult = new ServerMessage(Outgoing.ModResponse);
                notifyResult.AppendString(Message);
                Client.SendMessage(notifyResult);
            }

            Ticket.Status = NewStatus;
            SendTicketToModerators(Ticket);
        }

        internal Boolean UsersHasPendingTicket(UInt32 Id)
        {
            foreach (var Ticket in Tickets)
            {
                if (Ticket.SenderId == Id && Ticket.Status == TicketStatus.OPEN)
                {
                    return true;
                }
            }
            return false;
        }

        internal void DeletePendingTicketForUser(UInt32 Id)
        {
            foreach (var Ticket in Tickets)
            {
                if (Ticket.SenderId == Id)
                {
                    Ticket.Status = TicketStatus.DELETED;
                    SendTicketToModerators(Ticket);
                    return;
                }
            }
        }

        internal static void SendTicketToModerators(SupportTicket Ticket)
        {
            ServerMessage Message = new ServerMessage(Outgoing.SerializeIssue);
            Ticket.SerializeBody(Message);
            OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(Message, "fuse_mod", 0);
        }

        internal void LogStaffEntry(string modName, string target, string type, string description)
        {
            GameClient Session = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(modName);

            if (Session == null || Session.GetConnection() == null || Session.GetConnection().getIp() == null)
                return;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO staff_logs (staffuser,target,action_type,description,ip) VALUES (@username,@target,@type,@desc,@ip)");
                dbClient.addParameter("username", modName);
                dbClient.addParameter("target", target);
                dbClient.addParameter("type", type);
                dbClient.addParameter("desc", description);
                dbClient.addParameter("ip", Session.GetConnection().getIp());
                dbClient.runQuery();
            }
        }
        #endregion

        #region Room Moderation

        internal static void PerformRoomAction(GameClient ModSession, uint RoomId, Boolean KickUsers, Boolean LockRoom, Boolean InappropriateRoom)
        {
            var Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);

            if (Room == null)
            {
                return;
            }

            if (OtanixEnvironment.GetGame().GetRoomRankConfig().ROOMS_TO_MODIFY.Contains(Convert.ToInt32(RoomId)))
            {
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(ModSession.GetHabbo().Username, string.Empty, "-Modify blocked room-", "Tried to MOD-TOOL in room:" + RoomId);
                return;
            }

            if (LockRoom)
            {
                Room.RoomData.State = 1;
                Room.RoomData.Name = "Inappropriate to Hotel Management";
                Room.RoomData.roomNeedSqlUpdate = true;
            }

            if (InappropriateRoom)
            {
                Room.RoomData.Name = LanguageLocale.GetValue("moderation.room.roomclosed");
                Room.RoomData.Description = LanguageLocale.GetValue("moderation.room.roomclosed");
                Room.RoomData.Tags.Clear();
                Room.RoomData.roomNeedSqlUpdate = true;
            }

            if (KickUsers)
            {
                Room.onRoomKick();
            }

            ServerMessage Update = new ServerMessage(Outgoing.UpdateRoom);
            Update.AppendUInt(Room.Id);
            Room.SendMessage(Update);
        }

        internal static ServerMessage SerializeRoomTool(UInt32 RoomId)
        {
            var Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            if (Room == null || Room.RoomData == null)
                return null;

            Habbo Owner = UsersCache.getHabboCache(Room.RoomData.OwnerId);

            var Message = new ServerMessage(Outgoing.RoomTool);
            Message.AppendUInt(RoomId); // flatId
            Message.AppendInt32(Room.RoomData.UsersNow); // userCount
            Message.AppendBoolean(Owner == null ? false : Owner.CurrentRoom == Room); // ownerInRoom
            Message.AppendUInt(Room.RoomData.OwnerId); // ownerId
            Message.AppendString(Room.RoomData.Owner); // ownerName
            Message.AppendBoolean(Room != null); // show data?
            if (Room != null)
            {
                Message.AppendString(Room.RoomData.Name); // roomName
                Message.AppendString(Room.RoomData.Description); // roomDesc
                Message.AppendInt32(Room.RoomData.Tags.Count); // tagsCount

                foreach (var Tag in Room.RoomData.Tags) // tags
                {
                    Message.AppendString(Tag);
                }
            }

            return Message;
        }

        #endregion

        #region User Moderation

        internal static void KickUser(GameClient ModSession, uint UserId, String Message, Boolean Soft)
        {
            var Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (Client == null || Client.GetHabbo().CurrentRoomId < 1 || Client.GetHabbo().Id == ModSession.GetHabbo().Id)
            {
                return;
            }

            if (Client.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
            {
                ModSession.SendNotif(LanguageLocale.GetValue("moderation.kick.missingrank"));
                return;
            }

            var Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Client.GetHabbo().CurrentRoomId);

            if (Room == null)
            {
                return;
            }

            Room.GetRoomUserManager().RemoveUserFromRoom(Client, true, false);

            if (!Soft)
            {
                Client.SendNotif(Message);

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("UPDATE user_info SET cautions = cautions + 1 WHERE user_id = " + UserId + "");
                }
            }
        }

        internal static void AlertUser(GameClient ModSession, uint UserId, String Message, Boolean Caution)
        {
            var Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client == null || Client.GetHabbo().Id == ModSession.GetHabbo().Id)
            {
                return;
            }
            if (Caution && Client.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
            {
                ModSession.SendNotif(LanguageLocale.GetValue("moderation.caution.missingrank"));
                Caution = false;
            }
            Client.SendNotif(Message); //, Caution);

            if (Caution)
            {
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("UPDATE user_info SET cautions = cautions + 1 WHERE user_id = " + UserId + "");
                }
            }
        }

        internal static void BanUser(GameClient ModSession, uint UserId, int Length, String Message)
        {
            var Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (Client == null || Client.GetHabbo().Id == ModSession.GetHabbo().Id)
            {
                return;
            }

            if (Client.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
            {
                ModSession.SendNotif(LanguageLocale.GetValue("moderation.ban.missingrank"));
                return;
            }

            OtanixEnvironment.GetGame().GetBanManager().BanUser(Client, Client.GetHabbo().Username, "", Message, Length, ModSession);
        }

        internal static void MuteUser(GameClient ModSession, Habbo Client, int Length, String Message)
        {
            if (OtanixEnvironment.GetGame().GetMuteManager().UserIsMuted(Client.Id))
            {
                if(ModSession != null)
                    ModSession.SendNotif("El usuario ya está muteado.");

                return;
            }

            OtanixEnvironment.GetGame().GetMuteManager().AddUserMute(Client.Id, Length);

            if (Client.GetClient() != null)
            {
                DateTime newDateTime = OtanixEnvironment.UnixTimeStampToDateTime(OtanixEnvironment.GetGame().GetMuteManager().UsersMuted[Client.Id].ExpireTime);

                ServerMessage nMessage = new ServerMessage(Outgoing.SendLinkNotif);
                nMessage.AppendString("Tu keko no podrá hablar hasta " + newDateTime.ToString() + " Eh, levanta el pie. Tú también puedes hacer que tod@s pasen un rato agradable en " + EmuSettings.HOTEL_LINK + ". ¡Respeto sí! ¡Bullyng no! Vale, vale, ¡tiempo muerto! No vendría mal releer la Manera " + EmuSettings.HOTEL_LINK + " y los Términos de Servicio.");
                nMessage.AppendString("http://www." + EmuSettings.HOTEL_LINK + "/legal/terms-of-service");
                Client.GetClient().SendMessage(nMessage);
            }
        }

        #endregion

        #region User Info

        internal static ServerMessage SerializeUserInfo(uint UserId)
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id, username, mail, look, last_purchase, last_online, account_created FROM users WHERE id = " + UserId + "");
                var User = dbClient.getRow();

                dbClient.setQuery("SELECT cfhs, cfhs_abusive, cautions, bans FROM user_info WHERE user_id = " + UserId + "");
                var Info = dbClient.getRow();

                if (User == null)
                {
                    throw new NullReferenceException("No user found in database");
                }

                double lastOnlinetimer = 0;
                double.TryParse((Convert.ToDouble(User["last_online"])).ToString(), out lastOnlinetimer);
                double LastLoginMinutes = OtanixEnvironment.GetUnixTimestamp() - lastOnlinetimer;
                int RegisterTimer = (DateTime.Now - OtanixEnvironment.UnixTimeStampToDateTime(Convert.ToUInt32(User["account_created"]))).Minutes;
                string LastPurchase = "";
                if (OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToUInt32(User["id"])) != null)
                {
                    Habbo habbo = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToUInt32(User["id"])).GetHabbo();
                    if (habbo != null)
                    {
                        LastLoginMinutes = OtanixEnvironment.GetUnixTimestamp() - habbo.LastOnline;
                        LastPurchase = habbo.LastPurchase;
                    }
                }

                var Message = new ServerMessage(Outgoing.UserTool);
                Message.AppendUInt(Convert.ToUInt32(User["id"]));
                Message.AppendString((string)User["username"]);
                Message.AppendString((string)User["look"]);
                Message.AppendInt32(RegisterTimer);
                Message.AppendInt32((Int32)LastLoginMinutes);
                Message.AppendBoolean(OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToUInt32(User["id"])) != null);

                if (Info != null)
                {
                    Message.AppendInt32((int)Info["cfhs"]);
                    Message.AppendInt32((int)Info["cfhs_abusive"]);
                    Message.AppendInt32((int)Info["cautions"]);
                    Message.AppendInt32((int)Info["bans"]);
                    Message.AppendInt32(0); // Added on RELEASE20140108
                }
                else
                {
                    Message.AppendInt32(0); // cfhs
                    Message.AppendInt32(0); // abusive cfhs
                    Message.AppendInt32(0); // cautions
                    Message.AppendInt32(0); // bans
                    Message.AppendInt32(0); // Added on RELEASE20140108
                }
                Message.AppendString(""); // trading_lock_expiry_txt
                Message.AppendString(LastPurchase == "" ? (string)User["last_purchase"] : LastPurchase); // last_purchase_txt
                Message.AppendInt32(0); // identityinformationtool.url + this
                Message.AppendInt32(0); // id_bans_txt
                Message.AppendString((string)User["mail"]); // email_address_txt
                Message.AppendString(""); // user_classification
                return Message;
            }
        }

        internal static ServerMessage SerializeRoomVisits(UInt32 UserId)
        {
            Habbo User = UsersCache.getHabboCache(UserId);

            ServerMessage Message = new ServerMessage(Outgoing.RoomsVisits);
            Message.AppendUInt(UserId);
            Message.AppendString(UsersCache.getUsernameById(UserId));
            if (User == null)
            {
                Message.AppendInt32(0);
            }
            else
            {
                Message.AppendInt32(User.RoomsVisited.Count);
                foreach (RoomVisits Room in User.RoomsVisited)
                {
                    // Message.AppendBoolean(Room.IsPublic);
                    Message.AppendUInt(Room.RoomId);
                    Message.AppendString(Room.RoomName);
                    Message.AppendInt32(Room.Hour);
                    Message.AppendInt32(Room.Minute);
                }
            }
            return Message;
        }

        #endregion

        #region Chatlogs

        internal static ServerMessage SerializeUserChatlog(UInt32 UserId)
        {
            var client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (client == null || client.GetHabbo() == null || client.GetHabbo().GetChatMessageManager() == null)
            {
                var Message = new ServerMessage(Outgoing.UserChatlog);
                Message.AppendUInt(UserId);
                Message.AppendString("User not online");
                Message.AppendInt32(0);

                return Message;
            }
            else
            {
                Dictionary<uint, List<ChatMessage>> messages = client.GetHabbo().GetChatMessageManager().GetSortedMessages();

                ServerMessage Message = new ServerMessage(Outgoing.UserChatlog);
                Message.AppendUInt(UserId);
                Message.AppendString(client.GetHabbo().Username);
                Message.AppendInt32(messages.Count);
                foreach (var valuePair in messages)
                {
                    var sortedMessages = valuePair.Value;

                    RoomData room = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(valuePair.Key);
                    if (room == null)
                        return null;

                    Message.AppendByted(1);
                    Message.AppendShort(3);

                    Message.AppendString("");
                    Message.AppendByted(0);
                    Message.AppendBoolean(room.IsPublicRoom);

                    Message.AppendString("roomId");
                    Message.AppendByted(1);
                    Message.AppendUInt(room.Id);

                    Message.AppendString("roomName");
                    Message.AppendByted(2);
                    Message.AppendString(room.Name);

                    Message.AppendShort(valuePair.Value.Count);
                    foreach (var message in valuePair.Value)
                    {
                        message.Serialize(ref Message);
                    }
                }

                return Message;
            }
        }

        internal static ServerMessage SerializeTicketChatlog(SupportTicket Ticket, RoomData RoomData, Double Timestamp)
        {
            var currentRoom = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomData.Id);

            var Message = new ServerMessage(Outgoing.IssueChatlog);
            Message.AppendUInt(Ticket.TicketId);
            Message.AppendUInt(Ticket.SenderId);
            Message.AppendInt32(Ticket.ReportedId);
            Message.AppendUInt(RoomData.Id); //maybe?
            Message.AppendBoolean(RoomData.IsPublicRoom);
            Message.AppendUInt(RoomData.Id);
            Message.AppendString(RoomData.Name);

            if (currentRoom == null)
            {
                Message.AppendInt32(0);
                return Message;
            }
            else
            {
                var manager = currentRoom.GetChatMessageManager();
                Message.AppendInt32(manager.messageCount);
                manager.Serialize(ref Message);

                return Message;
            }
        }

        internal static ServerMessage SerializeRoomChatlog(UInt32 roomID)
        {
            var currentRoom = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(roomID);
            if (currentRoom == null)
                return null;

            var Message = new ServerMessage(Outgoing.RoomChatlog);
            Message.AppendByted(1);
            Message.AppendShort(2);

            Message.AppendString("roomId");
            Message.AppendByted(1);
            Message.AppendUInt(currentRoom.RoomId);

            Message.AppendString("roomName");
            Message.AppendByted(2);
            Message.AppendString(currentRoom.RoomData.Name);

            Message.AppendShort(currentRoom.GetChatMessageManager().messageCount);
            currentRoom.GetChatMessageManager().Serialize(ref Message);

            return Message;
        }
        #endregion
    }
}
