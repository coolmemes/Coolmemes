using Butterfly.HabboHotel.Support;
using Butterfly.Core;
using ButterStorm;
using HabboEvents;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using Butterfly.HabboHotel.ChatMessageStorage;
using Butterfly.HabboHotel.Alfas.Manager;
using Butterfly.HabboHotel.Users;

namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        internal void SubmitHelpTicket()
        {
            bool errorOccured = OtanixEnvironment.GetGame().GetModerationTool().UsersHasPendingTicket(Session.GetHabbo().Id);

            if (!errorOccured)
            {
                string Message = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
                int Type = Request.PopWiredInt32();
                int ReportedUser = Request.PopWiredInt32(); // can be a room(-1)
                uint RoomId = Request.PopWiredUInt();
                int messageCount = Request.PopWiredInt32();
                string[] chats = new string[messageCount];

                for (int i = 0; i < messageCount; i++ )
                {
                    Request.PopWiredInt32();
                    chats[i] = Request.PopFixedString();
                }

                ServerMessage Messagee = new ServerMessage(Outgoing.TicketAlert);
                Messagee.AppendInt32(Type);
                if(OtanixEnvironment.GetGame().GetModerationTool().UsersHasPendingTicket(Session.GetHabbo().Id))
                {
                    Messagee.AppendString("¡Hey, tus llamadas de ayuda echan humo! Por favor, un poquito de paciencia para no saturar el sistema.");
                }
                else
                {
                    Messagee.AppendString("¡Recibido! Vamos a revisar tu problema enseguida. Recuerda que siempre puedes estar al día de todas las actualizaciones y notificaciones consultando nuestras FAQs en la sección de preguntas frecuentes.");
                }
                Session.SendMessage(Messagee);

                OtanixEnvironment.GetGame().GetModerationTool().SendNewTicket(Session, Type, ReportedUser, Message, chats);
            }
        }

        internal void ModGetUserInfo()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
            {
                return;
            }

            uint UserId = Request.PopWiredUInt();

            if (OtanixEnvironment.GetGame().GetClientManager().GetNameById(UserId) != "")
            {
                Session.SendMessage(ModerationTool.SerializeUserInfo(UserId));
            }
            else
            {
                Session.SendNotif(LanguageLocale.GetValue("user.loadusererror"));
            }
        }

        internal void ModGetUserChatlog()
        {
            if (!Session.GetHabbo().HasFuse("fuse_chatlogs"))
            {
                return;
            }

            Session.SendMessage(ModerationTool.SerializeUserChatlog(Request.PopWiredUInt()));
        }

        internal void ModGetRoomChatlog()
        {
            if (!Session.GetHabbo().HasFuse("fuse_chatlogs"))
            {
                return;
            }

            int Junk = Request.PopWiredInt32();
            uint RoomId = Request.PopWiredUInt();

            Session.SendMessage(ModerationTool.SerializeRoomChatlog(RoomId));
        }

        internal void ModGetRoomTool()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
            {
                return;
            }

            uint RoomId = Request.PopWiredUInt();
            Session.SendMessage(ModerationTool.SerializeRoomTool(RoomId));
        }

        internal void ModPickTicket()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
            {
                return;
            }

            int Junk = Request.PopWiredInt32();
            uint TicketId = Request.PopWiredUInt();

            OtanixEnvironment.GetGame().GetModerationTool().PickTicket(Session, TicketId);
        }

        internal void ModReleaseTicket()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
            {
                return;
            }

            int amount = Request.PopWiredInt32();

            for (int i = 0; i < amount; i++)
            {
                uint TicketId = Request.PopWiredUInt();

                OtanixEnvironment.GetGame().GetModerationTool().ReleaseTicket(Session, TicketId);
            }
        }

        internal void ModCloseTicket()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
            {
                return;
            }

            int Result = Request.PopWiredInt32(); // result, 1 = useless, 2 = abusive, 3 = resolved
            int Junk = Request.PopWiredInt32(); // ? 
            uint TicketId = Request.PopWiredUInt(); // id

            OtanixEnvironment.GetGame().GetModerationTool().CloseTicket(Session, TicketId, Result);
        }

        internal void ModGetTicketChatlog()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
            {
                return;
            }

            SupportTicket Ticket = OtanixEnvironment.GetGame().GetModerationTool().GetTicket(Request.PopWiredUInt());

            if (Ticket == null)
            {
                return;
            }

            RoomData Data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(Ticket.RoomId);

            if (Data == null)
            {
                return;
            }

            Session.SendMessage(ModerationTool.SerializeTicketChatlog(Ticket, Data, Ticket.Timestamp));
        }

        internal void ModGetRoomVisits()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
            {
                return;
            }

            uint UserId = Request.PopWiredUInt();

            Session.SendMessage(ModerationTool.SerializeRoomVisits(UserId));
        }

        internal void RequestHelp()
        {
            GetResponse().Init(Outgoing.HelpRequest);
            GetResponse().AppendInt32(0);
            SendResponse();
        }

        internal void ModSendRoomAlert()
        {
            if (!Session.GetHabbo().HasFuse("fuse_room_alert"))
            {
                return;
            }

            int One = Request.PopWiredInt32();
            string Message = Request.PopFixedString();

            ServerMessage Alert = new ServerMessage(Outgoing.SendNotif);
            Alert.AppendString(Message);
            Alert.AppendString("");
            Session.GetHabbo().CurrentRoom.SendMessage(Alert);
        }

        internal void ModPerformRoomAction()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
            {
                return;
            }

            uint RoomId = Request.PopWiredUInt();
            bool ActOne = (Request.PopWiredInt32() == 1); // set room lock to doorbell
            bool ActTwo = (Request.PopWiredInt32() == 1); // set room to inappropiate
            bool ActThree = (Request.PopWiredInt32() == 1); // kick all users

            ModerationTool.PerformRoomAction(Session, RoomId, ActThree, ActOne, ActTwo);
        }

        internal void ModSendUserMessage()
        {
            if (!Session.GetHabbo().HasFuse("fuse_user_alert"))
            {
                return;
            }

            uint UserId = Request.PopWiredUInt();
            string Message = Request.PopFixedString();

            ModerationTool.AlertUser(Session, UserId, Message, true);
        }

        internal void ModKickUser()
        {
            if (!Session.GetHabbo().HasFuse("fuse_user_kick"))
            {
                return;
            }

            var UserId = Request.PopWiredUInt();
            var Message = Request.PopFixedString();

            ModerationTool.KickUser(Session, UserId, Message, false);
        }

        internal void ModMuteUser()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mute"))
            {
                return;
            }

            var UserId = Request.PopWiredUInt();
            var Message = Request.PopFixedString();
            var MuteMinutes = Request.PopWiredInt32();
            // 2 str: 1,2

            Habbo TargetHabbo = UsersCache.getHabboCache(UserId);
            if (TargetHabbo == null)
                return;

            if (MuteMinutes == 34)
                MuteMinutes = 60;

            ModerationTool.MuteUser(Session, TargetHabbo, MuteMinutes, Message);
        }

        internal void ModBanUser()
        {
            if (!Session.GetHabbo().HasFuse("fuse_ban"))
            {
                return;
            }

            var UserId = Request.PopWiredUInt();
            var Message = Request.PopFixedString();
            var Length = Request.PopWiredInt32() * 3600;

            ModerationTool.BanUser(Session, UserId, Length, Message);
        }

        internal void ReportarAcoso()
        {
            ServerMessage message = new ServerMessage(Outgoing.ReportAcoso);
            if (OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().UserStartedBully(Session.GetHabbo().Id))
            {
                OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().Bullies[Session.GetHabbo().Id].Serialize(message);
            }
            else
            {
                message.AppendInt32(0);
            }
            Session.SendMessage(message);
        }

        internal void ReportarAcosoMessage()
        {
            uint UserReportedId = Request.PopWiredUInt();
            GameClient _client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserReportedId); // podemos usar esto para si está online
            
            uint RoomId = Request.PopWiredUInt();
            var Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            if (Room == null)
                return;

            ServerMessage message = new ServerMessage(Outgoing.ReportAcosoMessage);
            if (_client != null && _client.GetHabbo() != null && _client.GetHabbo().GetChatMessageManager().messageCount <= 0)
            {
                message.AppendInt32(2);
            }
            else if (OtanixEnvironment.GetGame().GetModerationTool().UsersHasPendingTicket(Session.GetHabbo().Id) || OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().UserStartedBully(Session.GetHabbo().Id))
            {
                message.AppendInt32(3);
            }
            else
            {
                if (OtanixEnvironment.GetUnixTimestamp() - Session.GetHabbo().LastAlfaSend < 1200)
                {
                    Session.GetMessageHandler().GetResponse().Init(Outgoing.onGuideSessionError);
                    Session.GetMessageHandler().GetResponse().AppendInt32(0);
                    Session.GetMessageHandler().SendResponse();

                    return;
                }

                Bully bully = new Bully(Session.GetHabbo().Id, UserReportedId, Room.GetChatMessageManager().GetRoomChatMessage());
                if (!bully.SearchGuardian())
                {
                    bully.SerializeNoGuardians();
                    return;
                }

                OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().AddBullie(bully);

                message.AppendInt32(0);
            }
            Session.SendMessage(message);
        }
    }
}
