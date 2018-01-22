using Butterfly.HabboHotel.ChatMessageStorage;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Support;
using Butterfly.HabboHotel.Users;
using Butterfly.Messages;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Alfas.Manager
{
    class Bully
    {
        internal UInt32 ReporterId;
        internal UInt32 ReportedId;
        internal UInt32 GuardianId;
        internal List<UInt32> nullGuardians;
        internal DateTime startedTimer;
        internal DateTime customTimer;
        internal BullyState bullyState;
        internal Boolean NeedUpdate;
        internal BullySolution bullySolution;
        internal List<ChatMessage> conversation;
        private String AlfaChatLog;

        internal GameClient Reporter
        {
            get
            {
                return OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(ReporterId);
            }
        }

        internal GameClient Guardian
        {
            get
            {
                return OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(GuardianId);
            }
        }

        internal Bully(UInt32 reporterId, UInt32 reportedId, List<ChatMessage> _conversation)
        {
            this.ReporterId = reporterId;
            this.ReportedId = reportedId;
            this.GuardianId = 0;
            this.nullGuardians = new List<UInt32>();
            this.startedTimer = DateTime.Now;
            this.bullyState = BullyState.SEARCHING_USER;
            this.bullySolution = BullySolution.NONE;
            this.conversation = _conversation;
            this.AlfaChatLog = "";
        }

        internal void OnCycle()
        {
            if(this.bullyState == BullyState.SEARCHING_USER)
            {
                if((DateTime.Now - customTimer).Seconds >= 15)
                {
                    GameClient guardianClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(this.GuardianId);
                    if(guardianClient != null)
                    {
                        ServerMessage Message3 = new ServerMessage(Outgoing.CancelAlfaAlert);
                        guardianClient.SendMessage(Message3);

                        guardianClient.GetHabbo().AlfaServiceId = 0;
                        guardianClient.GetHabbo().ExitAlfaState();
                    }

                    if(!this.nullGuardians.Contains(this.GuardianId))
                        this.nullGuardians.Add(this.GuardianId);

                    if (!this.SearchGuardian())
                    {
                        this.SerializeNoGuardians();
                        OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().RemoveBullie(this.ReporterId);
                        return;
                    }
                }
                else if (this.NeedUpdate)
                {
                    this.NeedUpdate = false;

                    if (!this.nullGuardians.Contains(this.GuardianId))
                        this.nullGuardians.Add(this.GuardianId);

                    if (!this.SearchGuardian())
                    {
                        this.SerializeNoGuardians();
                        OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().RemoveBullie(this.ReporterId);
                        return;
                    }
                }
            }
            else if (this.bullyState == BullyState.WAITING_RESPONSE)
            {
                if ((DateTime.Now - customTimer).Seconds >= 59)
                {
                    GameClient guardianClient = Guardian;
                    if (guardianClient != null)
                    {
                        ServerMessage Message3 = new ServerMessage(Outgoing.CancelAlfaAlert);
                        guardianClient.SendMessage(Message3);
                    }

                    this.bullySolution = BullySolution.EXIT;
                    this.bullyState = BullyState.FINISHED;
                }
            }
            else if(this.bullyState == BullyState.FINISHED)
            {
                switch (this.bullySolution)
                {
                    case BullySolution.NONE:
                    case BullySolution.EXIT:
                    case BullySolution.RELOAD:
                        {
                            GameClient _Reporter = Reporter;
                            if (_Reporter != null)
                                OtanixEnvironment.GetGame().GetModerationTool().SendNewTicket(_Reporter, 104, (int)ReportedId, "Acoso", new string[0]);

                            break;
                        }
                    case BullySolution.ACCEPTABLE:
                        {
                            // none (:
                            break;
                        }
                    case BullySolution.BULLY:
                    case BullySolution.HORROR:
                        {
                            Habbo habbo = UsersCache.getHabboCache(ReportedId);
                            if (habbo == null)
                                return;

                            ModerationTool.MuteUser(null, habbo, 10, "");
                            break;
                        }
                    /*case BullySolution.HORROR:
                        {
                            GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(ReportedId);
                            Habbo habbo = UsersCache.getHabboCache(ReportedId);

                            if (habbo != null && habbo.Rank <= 1)
                                //OtanixEnvironment.GetGame().GetBanManager().BanUser(TargetClient, habbo.Username, "", "Baneado por los HabboAlfas.", 21600, null);
                            break;
                        }*/
                }

                GameClient guardianClient = Guardian;
                if (guardianClient != null && guardianClient.GetHabbo() != null)
                {
                    guardianClient.GetHabbo().AlfaServiceId = 0;
                    guardianClient.GetHabbo().AlfaHelpEnabled = false;

                    if (bullySolution == BullySolution.ACCEPTABLE || bullySolution == BullySolution.BULLY || bullySolution == BullySolution.HORROR)
                    {
                        ServerMessage EndVotation = new ServerMessage(Outgoing.FinishAlfaVotation);
                        EndVotation.AppendInt32((Int32)bullySolution);
                        EndVotation.AppendInt32((Int32)bullySolution);
                        EndVotation.AppendInt32(0); // array
                        guardianClient.SendMessage(EndVotation);
                    }

                    // Con esto enviaremos al Guardián al inicio de la lista para que puedan ayudar los últimos guardianes.
                    // OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().RemoveGuardian(GuardianId);
                    // OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().AddGuardian(GuardianId);
                }

                if (this.AlfaChatLog.Length > 0)
                    OtanixEnvironment.GetGame().GetAlfaManager().LoadAlfaLog(this.GuardianId, "BULLY", this.AlfaChatLog, (Int32)this.bullySolution);

                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(GuardianId, "ACH_GuideChatReviewer", 1);

                GameClient reporterClient = Reporter;
                if (reporterClient != null)
                {
                    ServerMessage reporterAlert = new ServerMessage(Outgoing.ResolvedAlfa);
                    reporterAlert.AppendInt32(3);
                    reporterClient.SendMessage(reporterAlert);
                }

                OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().RemoveBullie(this.ReporterId);
            }
        }

        internal bool SearchGuardian()
        {
            if (this.bullyState != BullyState.SEARCHING_USER)
                return false;
        
            this.customTimer = DateTime.Now;
            this.GuardianId = 0;

            List<UInt32> onlineGuardians = OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().Guardians;
            foreach (UInt32 guardianId in onlineGuardians)
            {
                if (this.nullGuardians.Contains(guardianId) || this.ReporterId == guardianId)
                    continue;

                GameClient guardian = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(guardianId);
                if (guardian == null || guardian.GetHabbo() == null || guardian.GetHabbo().AlfaServiceId != 0)
                {
                    //OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().RemoveGuardian(guardian.Key);
                    continue;
                }

                ServerMessage Message = new ServerMessage(Outgoing.ReceiveAcosoAlert);
                Message.AppendInt32(15);
                guardian.SendMessage(Message);

                guardian.GetHabbo().AlfaServiceId = this.ReporterId;
                this.GuardianId = guardianId;

                return true;
            }

            return false;
        }

        internal void Serialize(ServerMessage Message)
        {
            Habbo user = UsersCache.getHabboCache(this.ReportedId);

            Message.AppendInt32(1); // length
            Message.AppendInt32(3); // type: Bully
            Message.AppendInt32((DateTime.Now - startedTimer).Seconds); // timer sec
            Message.AppendBoolean(user == null); // false = usuario, true = null
            if (user != null)
            {
                Message.AppendString(user.Username);
                Message.AppendString(user.Look);
                Message.AppendString((user.CurrentRoom == null) ? "" : user.CurrentRoom.RoomData.Name);
            }
        }

        internal void SerializeNoGuardians()
        {
            GameClient _Reporter = Reporter;
            if (_Reporter == null)
            {
                return;
            }

            ServerMessage Message2 = new ServerMessage(Outgoing.onGuideSessionError);
            Message2.AppendInt32(2);
            _Reporter.SendMessage(Message2);

            OtanixEnvironment.GetGame().GetModerationTool().SendNewTicket(_Reporter, 104, (int)ReportedId, "Acoso", new string[0]);
        }

        internal void SerializeAlfaChat()
        {
            GameClient _Guardian = Guardian;
            if (_Guardian == null)
            {
                this.bullySolution = BullySolution.EXIT;
                this.bullyState = BullyState.FINISHED;

                return;
            }

            StringBuilder strBuilder = new StringBuilder();
            Dictionary<UInt32, Int32> usersIds = new Dictionary<uint, int>();
            usersIds.Add(ReportedId, 0);

            foreach (ChatMessage chatMessage in this.conversation)
            {
                int i = usersIds.Count;
                if (usersIds.ContainsKey(chatMessage.userID))
                    i = usersIds[chatMessage.userID];
                else
                    usersIds.Add(chatMessage.userID, i);

                strBuilder.Append(chatMessage.timeSpoken + ";" + i + ";" + chatMessage.message + Convert.ToChar(13));
            }

            this.AlfaChatLog = strBuilder.ToString();

            ServerMessage Message = new ServerMessage(Outgoing.GetAlfaMessages);
            Message.AppendInt32(60); // seconds.
            Message.AppendString(strBuilder.ToString());
            _Guardian.SendMessage(Message);
        }
    }
}
