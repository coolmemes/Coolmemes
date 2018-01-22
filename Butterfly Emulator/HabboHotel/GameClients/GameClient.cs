using System;
using Butterfly.Core;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.Users;
using Butterfly.HabboHotel.Users.UserDataManagement;
using Butterfly.Messages;
using Butterfly.Net;
using Butterfly.Util;
using ButterStorm;
using ConnectionManager;
using System.Drawing;
using HabboEvents;
using System.Threading.Tasks;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Support;
using Butterfly.HabboHotel.Catalogs;
using Butterfly.HabboHotel.Filter;
using System.Data;
using Database_Manager.Database.Session_Details.Interfaces;

namespace Butterfly.HabboHotel.GameClients
{
    public class GameClient
    {
        private uint Id;
        private ConnectionInformation Connection;
        private GameClientMessageHandler MessageHandler;
        private Habbo Habbo;
        private GamePacketParser packetParser;
        internal string MachineId;
        internal bool PacketSaverEnable;

        internal uint ConnectionID
        {
            get
            {
                return Id;
            }
        }

        internal GameClient(uint ClientId, ConnectionInformation pConnection)
        {
            this.Id = ClientId;
            this.Connection = pConnection;
            this.packetParser = new GamePacketParser();
            this.PacketSaverEnable = false;
        }

        private void SwitchParserRequest(byte[] data)
        {
            if (MessageHandler == null)
            {
                InitHandler();
            }

            packetParser.SetConnection();
            packetParser.onNewPacket += new GamePacketParser.HandlePacket(parser_onNewPacket);
            Connection.parser.Dispose();
            Connection.parser = packetParser;
            Connection.parser.handlePacketData(data);
        }       

        private void parser_onNewPacket(ClientMessage Message)
        {
            try
            {
                MessageHandler.HandleRequest(Message);
            }
            catch (Exception e) { Logging.LogPacketException(Message.ToString() + " : " + (GetHabbo().CurrentRoomId > 0 ? "CurrentRoomId: " + GetHabbo().CurrentRoomId : ""), e.ToString()); }
        }

        private void PolicyRequest()
        {
            Connection.SendData(OtanixEnvironment.GetDefaultEncoding().GetBytes(CrossdomainPolicy.GetXmlPolicy()));
        }

        internal ConnectionInformation GetConnection()
        {
            return Connection;
        }

        internal GameClientMessageHandler GetMessageHandler()
        {
            return MessageHandler;
        }

        internal Habbo GetHabbo()
        {
            return Habbo;
        }

        internal void StartConnection()
        {
            if (Connection == null)
            {
                return;
            }

            (Connection.parser as InitialPacketParser).PolicyRequest += PolicyRequest;
            (Connection.parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;

            Connection.startPacketProcessing();
        }

        internal void InitHandler()
        {
            MessageHandler = new GameClientMessageHandler(this);
        }

        internal bool tryLogin(string AuthTicket)
        {
            try
            {
                if (GetConnection() == null)
                    return false;

                var userData = UserDataFactory.GetUserData(AuthTicket);
                if (userData == null)
                {
                    this.Disconnect();
                    return false;
                }

                OtanixEnvironment.GetGame().GetClientManager().RegisterClient(this, userData.user.Id, userData.user.Username);
                Habbo = userData.user;

                if (userData.user.Username == null || GetHabbo() == null)
                {
                    SendBanMessage("Você não possui um nome.");
                    return false;
                }

                userData.user.Init(userData);
                Habbo.MachineId = MachineId;

                var response = new QueuedServerMessage(Connection);

                var authok = new ServerMessage(Outgoing.AuthenticationOK);
                response.appendResponse(authok);

                var HomeRoom = new ServerMessage(Outgoing.HomeRoom);
                HomeRoom.AppendUInt((OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(GetHabbo().Id)) ? OtanixEnvironment.prisaoId() : GetHabbo().HomeRoom); // first home
                HomeRoom.AppendUInt((OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(GetHabbo().Id)) ? OtanixEnvironment.prisaoId() : GetHabbo().HomeRoom); // current home
                response.appendResponse(HomeRoom);

                var FavouriteRooms = new ServerMessage(Outgoing.FavouriteRooms);
                FavouriteRooms.AppendInt32(30); // max rooms
                FavouriteRooms.AppendInt32(userData.user.FavoriteRooms.Count);
                foreach (var Id in userData.user.FavoriteRooms.ToArray())
                {
                    FavouriteRooms.AppendUInt(Id);
                }
                response.appendResponse(FavouriteRooms);

                var sendClub = new ServerMessage(Outgoing.SerializeClub);
                sendClub.AppendString("club_habbo");
                sendClub.AppendInt32(0); // days left
                sendClub.AppendInt32(0); // days multiplier
                sendClub.AppendInt32(0); // months left
                sendClub.AppendInt32(0); // ???
                sendClub.AppendBoolean(true); // HC PRIVILEGE
                sendClub.AppendBoolean(true); // VIP PRIVILEGE
                sendClub.AppendInt32(0); // days i have on hc
                sendClub.AppendInt32(0); // days i've purchased
                sendClub.AppendInt32(495); // value 4 groups
                response.appendResponse(sendClub);

                var roomAccessConfig = new ServerMessage(Outgoing.RoomAccessConfig);
                roomAccessConfig.AppendBoolean(true); // isOpen
                roomAccessConfig.AppendBoolean(false);
                roomAccessConfig.AppendBoolean(true);
                response.appendResponse(roomAccessConfig);

                var fuserights = new ServerMessage(Outgoing.Fuserights);
                fuserights.AppendInt32(2); // normal|hc|vip
                fuserights.AppendUInt(GetHabbo().Rank);
                fuserights.AppendBoolean(GetHabbo().HasFuse("fuse_ambassador")); // embajador ?
                // fuserights.AppendInt32(0); // New Identity (1 == 1 min and Alert!)
                response.appendResponse(fuserights);

                var newidentity = new ServerMessage(Outgoing.SendNewIdentityState);
                newidentity.AppendInt32(GetHabbo().NewIdentity);
                response.appendResponse(newidentity);

                var HabboInformation = new ServerMessage(Outgoing.HabboInfomation);
                HabboInformation.AppendUInt(GetHabbo().Id);
                HabboInformation.AppendString(GetHabbo().Username);
                HabboInformation.AppendString(GetHabbo().Look);
                HabboInformation.AppendString(GetHabbo().Gender.ToUpper());
                HabboInformation.AppendString(GetHabbo().Motto);
                HabboInformation.AppendString(GetHabbo().RealName);
                HabboInformation.AppendBoolean(false);
                HabboInformation.AppendUInt(GetHabbo().Respect);
                HabboInformation.AppendUInt(GetHabbo().DailyRespectPoints); // respect to give away
                HabboInformation.AppendUInt(GetHabbo().DailyPetRespectPoints);
                HabboInformation.AppendBoolean(true);
                HabboInformation.AppendString(OtanixEnvironment.UnixTimeStampToDateTime(GetHabbo().LastOnline).ToString());
                HabboInformation.AppendBoolean(GetHabbo().NameChanges < EmuSettings.MAX_NAME_CHANGES); // CHANGENAME - HabboInformation.AppendBoolean((this.GetHabbo().Diamonds<=0||this.GetHabbo().NameChanges>=ButterflyEnvironment.maxNameChanges)?false:true);
                HabboInformation.AppendBoolean(false);
                response.appendResponse(HabboInformation);

                var IsGuide = (Habbo.Rank > 1) ? true : false;
                var VoteInCompetitions = false;
                var Trade = true;
                var Citizien = (Habbo.CitizenshipLevel >= 4) ? true : false;
                var JudgeChat = (Habbo.Rank > 2) ? true : false;
                var NavigatorThumbailCamera = false;
                var navigatorphaseTwo = true;
                var Camera = true;
                var CallHelpers = true;
                var BuilderAtWork = true;
                var MouseZoom = false;

                var Allows = new ServerMessage(Outgoing.PerkAllowancesMessageParser);
                Allows.AppendInt32(11); // count
                Allows.AppendString("TRADE");
                Allows.AppendString((!Trade) ? "requirement.unfulfilled.citizenship_level_3" : "");
                Allows.AppendBoolean(Trade);
                Allows.AppendString("NAVIGATOR_ROOM_THUMBNAIL_CAMERA");
                Allows.AppendString((!NavigatorThumbailCamera) ? "" : "");
                Allows.AppendBoolean(NavigatorThumbailCamera);
                Allows.AppendString("NAVIGATOR_PHASE_TWO_2014");
                Allows.AppendString((!navigatorphaseTwo) ? "requirement.unfulfilled.feature_disabled" : "");
                Allows.AppendBoolean(navigatorphaseTwo);
                Allows.AppendString("VOTE_IN_COMPETITIONS");
                Allows.AppendString((!VoteInCompetitions) ? "requirement.unfulfilled.helper_level_2" : "");
                Allows.AppendBoolean(VoteInCompetitions);
                Allows.AppendString("BUILDER_AT_WORK");
                Allows.AppendString((!BuilderAtWork) ? "requirement.unfulfilled.group_membership" : "");
                Allows.AppendBoolean(BuilderAtWork);
                Allows.AppendString("MOUSE_ZOOM");
                Allows.AppendString((!MouseZoom) ? "requirement.unfulfilled.feature_disabled" : "");
                Allows.AppendBoolean(MouseZoom);
                Allows.AppendString("CAMERA");
                Allows.AppendString((!Camera) ? "requirement.unfulfilled.feature_disabled" : "");
                Allows.AppendBoolean(Camera);
                Allows.AppendString("CALL_ON_HELPERS");
                Allows.AppendString((!CallHelpers) ? "requirement.unfulfilled.citizenship_level_1" : "");
                Allows.AppendBoolean(CallHelpers);
                Allows.AppendString("CITIZEN");
                Allows.AppendString((!Citizien) ? "requirement.unfulfilled.citizenship_level_3" : "");
                Allows.AppendBoolean(Citizien);
                Allows.AppendString("USE_GUIDE_TOOL");
                Allows.AppendString((!IsGuide) ? "requirement.unfulfilled.helper_level_4" : "");
                Allows.AppendBoolean(IsGuide);
                Allows.AppendString("JUDGE_CHAT_REVIEWS");
                Allows.AppendString((!JudgeChat) ? "requirement.unfulfilled.citizenship_level_6" : "");
                Allows.AppendBoolean(JudgeChat);
                response.appendResponse(Allows);

                var enabledBuilderClub = new ServerMessage(Outgoing.EnableBuilderClub);
                enabledBuilderClub.AppendInt32(GetHabbo().IsPremium() ? GetHabbo().GetPremiumManager().GetRemainingTime() : 0); // Tiempo restante de Constructor (2678400 = 1 mes entero (s))
                enabledBuilderClub.AppendUInt(GetHabbo().IsPremium() ? GetHabbo().GetPremiumManager().GetMaxItems() : 50);  // Furnis que puedo alquilar
                enabledBuilderClub.AppendInt32(20000);  // Se puede ampliar la alquilación hasta..
                enabledBuilderClub.AppendInt32(0);
                response.appendResponse(enabledBuilderClub);

                response.appendResponse(GetHabbo().GetUserClothingManager().SerializeClothes());

                var achivPoints = new ServerMessage(Outgoing.AchievementPoints);
                achivPoints.AppendUInt(GetHabbo().AchievementPoints);
                response.appendResponse(achivPoints);

                var loadVolumen = new ServerMessage(Outgoing.LoadVolumen);
                loadVolumen.AppendInt32(int.Parse(GetHabbo().volumenSystem.Split(';')[0]));
                loadVolumen.AppendInt32(int.Parse(GetHabbo().volumenSystem.Split(';')[1]));
                loadVolumen.AppendInt32(int.Parse(GetHabbo().volumenSystem.Split(';')[2]));
                loadVolumen.AppendBoolean(GetHabbo().preferOldChat);
                loadVolumen.AppendBoolean(GetHabbo().IgnoreRoomInvitations);
                loadVolumen.AppendBoolean(GetHabbo().DontFocusUser); // fcus user
                loadVolumen.AppendInt32(0); // 
                loadVolumen.AppendInt32(0); // freeFlowChat
                response.appendResponse(loadVolumen);

                var muteUsers = new ServerMessage(Outgoing.SerializeMuteUsers);
                muteUsers.AppendInt32(GetHabbo().MutedUsers.Count);
                foreach(string IgnoreName in GetHabbo().MutedUsers)
                {
                    muteUsers.AppendString(IgnoreName);
                }
                response.appendResponse(muteUsers);

                TargetedOffer to = OtanixEnvironment.GetGame().GetTargetedOfferManager().GetRandomStaticTargetedOffer();
                if (to != null)
                {
                    if(!GetHabbo().TargetedOffers.ContainsKey(to.Id) || GetHabbo().TargetedOffers[to.Id] < to.PurchaseLimit)
                        response.appendResponse(OtanixEnvironment.GetGame().GetTargetedOfferManager().SerializeTargetedOffer(to));
                }

                /*var giftOptions = new ServerMessage(Outgoing.NewUserExperienceGiftOfferParser);
                giftOptions.AppendInt32(1); // foreach
                {
                    giftOptions.AppendInt32(0);
                    giftOptions.AppendInt32(0);
                    giftOptions.AppendInt32(1); // foreach (items?)
                    {
                        giftOptions.AppendString("Testeando"); // itemName ??
                        giftOptions.AppendInt32(1); // foreach
                        {
                            giftOptions.AppendString("a1_kumiankka"); // item 1
                            giftOptions.AppendString(""); // item 2 (if is empty == null)
                        }
                    }
                }
                response.appendResponse(giftOptions);*/

                response.appendResponse(OtanixEnvironment.GetGame().GetAchievementManager().AchievementPrede);

                if (GetHabbo().HomeRoom <= 0)
                {
                    var homeRoom = new ServerMessage(Outgoing.OutOfRoom);
                    response.appendResponse(homeRoom);
                }
                else
                {
                    Room room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(GetHabbo().HomeRoom);
                    if (room != null)
                        this.GetMessageHandler().enterOnRoom3(room);
                }
                 
                response.sendResponse();

                // Verifica a conta staff
                if (GetHabbo().Rank > 5)
                {
                    ServerMessage VerificaSenha = new ServerMessage(Outgoing.MobilePhoneNumero);
                    VerificaSenha.AppendInt32(1);
                    VerificaSenha.AppendInt32(1);
                    SendMessage(VerificaSenha);
                }
                // Termina de verificar a conta staff

                Ban BanReason = OtanixEnvironment.GetGame().GetBanManager().GetBanReason(Habbo.Username, Habbo.MachineId);
                if (BanReason != null)
                {
                    SendScrollNotif("Você tem um banimento do tipo: " + BanReason.Type + "\r\nMotivo: " + BanReason.ReasonMessage);
                    Disconnect();
                    return false;
                }

                GetHabbo().InitMessenger();

                if(GetHabbo().GetAvatarEffectsInventoryComponent() != null)
                    SendMessage(GetHabbo().GetAvatarEffectsInventoryComponent().Serialize());

                SendMessage(OtanixEnvironment.GetGame().GetModerationTool().SerializeCfhTopics());

                if (LanguageLocale.welcomeAlertEnabled)
                {
                    string strAlert = BlackWordsManager.SpecialReplace(LanguageLocale.welcomeAlert, this);

                    if (LanguageLocale.welcomeAlertType == 0)
                        SendScrollNotif(strAlert);
                    else if (LanguageLocale.welcomeAlertType == 1)
                        SendNotif(strAlert);
                    else if (LanguageLocale.welcomeAlertType == 2)
                        SendNotifWithImage(strAlert, LanguageLocale.welcomeAlertImage);
                }

                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Habbo.Id, "ACH_EmailVerification", 1);

                GetHabbo().UpdateCreditsBalance();
                GetHabbo().UpdateExtraMoneyBalance();
                GetHabbo().setMeOnline();
                GetHabbo().InitExtra();

                UsersCache.enterProvisionalRoom(this);
                
                return true;
            }
            catch (UserDataNotFoundException e)
            {
                SendScrollNotif(LanguageLocale.GetValue("login.invalidsso") + "extra data: " + e);
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Invalid Dario bug duing user login: " + e);
                SendScrollNotif("Login error: " + e);
            }
            return false;
        }

        internal void SendBanMessage(string Message)
        {
            var BanMessage = new ServerMessage(Outgoing.SendNotif);
            BanMessage.AppendString(LanguageLocale.GetValue("moderation.banmessage"));
            BanMessage.AppendString(Message);
            GetConnection().SendData(BanMessage.GetBytes());
        }

        internal void SendScrollNotif(string Message)
        {
            var notif = new ServerMessage(Outgoing.SendNotifWithScroll);
            notif.AppendInt32(1);
            notif.AppendString(Message);
            SendMessage(notif);
        }

        internal void SendWindowManagerAlert(string Message)
        {
            ServerMessage notif = new ServerMessage(Outgoing.WindowManagerAlert);
            notif.AppendInt32(0); // "${help.cfh.closed." + Int
            notif.AppendString(Message); // Message
            SendMessage(notif);
        }

        internal void SendNotif(string Message)
        {
            if (Message.Length <= 0)
                return;

            var notif = new ServerMessage(Outgoing.SendNotif);
            notif.AppendString(Message);
            notif.AppendString(""); // link
            SendMessage(notif);
        }

        internal void SendNotifWithImage(string Message, string Image, string Title = "Alerta", string linkTitle = "Entendido!", string linkUrl = "event:")
        {
            ServerMessage serverAlert = new ServerMessage(Outgoing.GeneratingNotification);
            serverAlert.AppendString("info." + EmuSettings.HOTEL_LINK);
            serverAlert.AppendInt32(5);
            serverAlert.AppendString("image");
            serverAlert.AppendString(Image);
            serverAlert.AppendString("title");
            serverAlert.AppendString(Title);
            serverAlert.AppendString("message");
            serverAlert.AppendString(Message);
            serverAlert.AppendString("linkTitle");
            serverAlert.AppendString(linkTitle);
            serverAlert.AppendString("linkUrl");
            serverAlert.AppendString(linkUrl);
            SendMessage(serverAlert);
        }

        internal void Stop()
        {
            if (GetMessageHandler() != null)
            {
                MessageHandler.Destroy();
            }
            if (GetHabbo() != null)
            {
                Habbo.OnDisconnect();
            }

            MessageHandler = null;
            Habbo = null;
            Connection = null;
        }

        private bool Disconnected = false;
        internal void Disconnect()
        {
            if (GetHabbo() != null && GetHabbo().GetInventoryComponent() != null)
                GetHabbo().GetInventoryComponent().RunDBUpdate();

            if (!Disconnected)
            {
                if (Connection != null)
                    Connection.Dispose();
                Disconnected = true;
            }
        }

        internal void SendMessage(ServerMessage Message)
        {
            if (Message == null)
                return;

            if (GetConnection() == null)
                return;

            if (PacketSaverEnable)
                Logging.LogPacketData("UserName: " + GetHabbo().Username + ": " + Message.ToString());

            GetConnection().SendData(Message.GetBytes());
        }

        internal void UnsafeSendMessage(ServerMessage Message)
        {
            if (Message == null)
                return;
            if (GetConnection() == null)
                return;
            GetConnection().SendUnsafeData(Message.GetBytes());
        }
    }
}
