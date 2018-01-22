using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Alfas.Manager
{
    class Help
    {
        internal UInt32 ReporterId;
        internal UInt32 HelperId;
        internal String reportMessage;
        internal List<UInt32> nullAlfas;
        internal DateTime customTimer;
        internal HelpState helpState;
        internal Boolean NeedUpdate;
        internal List<HelpChatMessage> chatQueueMessage;
        internal Queue addChatMessage;
        private String AlfaChatLog;

        internal GameClient Reporter
        {
            get
            {
                return OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(ReporterId);
            }
        }

        internal GameClient Alfa
        {
            get
            {
                return OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(HelperId);
            }
        }

        internal Help(UInt32 reporterId, String reportMessage)
        {
            this.ReporterId = reporterId;
            this.reportMessage = reportMessage;
            this.HelperId = 0;
            this.nullAlfas = new List<UInt32>();
            this.helpState = HelpState.SEARCHING_USER;
            this.NeedUpdate = false;
            this.chatQueueMessage = new List<HelpChatMessage>();
            this.addChatMessage = new Queue();
            this.AlfaChatLog = "";
        }

        internal void AddChatMessage(HelpChatMessage hcm)
        {
            lock (addChatMessage.SyncRoot)
            {
                addChatMessage.Enqueue(hcm);
            }
        }

        internal void OnCycle()
        {
            if (this.helpState == HelpState.SEARCHING_USER)
            {
                if ((DateTime.Now - customTimer).Seconds >= 15)
                {
                    GameClient alfaClient = Alfa;
                    if (alfaClient != null)
                    {
                        ServerMessage Message3 = new ServerMessage(Outgoing.CancelAlfaAlert);
                        alfaClient.SendMessage(Message3);

                        alfaClient.GetHabbo().AlfaServiceId = 0;
                        alfaClient.GetHabbo().ExitAlfaState();
                    }

                    if (!this.nullAlfas.Contains(this.HelperId))
                        this.nullAlfas.Add(this.HelperId);

                    if (!this.SearchAlfa())
                    {
                        this.SerializeNoAlfas();
                        OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().RemoveHelp(this.ReporterId);
                        return;
                    }
                }
                else if (this.NeedUpdate)
                {
                    this.NeedUpdate = false;

                    if (!this.nullAlfas.Contains(this.HelperId))
                        this.nullAlfas.Add(this.HelperId);

                    if (!this.SearchAlfa())
                    {
                        this.SerializeNoAlfas();
                        OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().RemoveHelp(this.ReporterId);
                        return;
                    }
                }
            }
            else if (this.helpState == HelpState.TALKING)
            {
                if (addChatMessage.Count > 0)
                {
                    lock (addChatMessage.SyncRoot)
                    {
                        while (addChatMessage.Count > 0)
                        {
                            var hcm = (HelpChatMessage)addChatMessage.Dequeue();
                            this.AlfaChatLog += hcm.UserId + ":" + hcm.Message + Environment.NewLine;

                            ServerMessage talkingAlfa = new ServerMessage((hcm.NormalChat) ? Outgoing.onGuideSessionMessage : Outgoing.onGuideSessionInvitedToGuideRoom);
                            if (hcm.NormalChat)
                            {
                                talkingAlfa.AppendString(hcm.Message);
                                talkingAlfa.AppendUInt(hcm.UserId);
                            }
                            else
                            {
                                talkingAlfa.AppendUInt(hcm.RoomId);
                                talkingAlfa.AppendString(hcm.Message);
                            }
                            sendMessage(talkingAlfa);
                        }
                    }
                }
            }
            else if (this.helpState == HelpState.FINISHED)
            {
                GameClient alfaClient = Alfa;
                if (alfaClient != null && alfaClient.GetHabbo() != null)
                {
                    alfaClient.GetHabbo().AlfaServiceId = 0;
                    alfaClient.GetHabbo().AlfaHelpEnabled = false;
                }

                if (this.AlfaChatLog.Length > 0)
                    OtanixEnvironment.GetGame().GetAlfaManager().LoadAlfaLog(this.HelperId, "HELP", this.AlfaChatLog, 0);

                OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().RemoveHelp(this.ReporterId);
            }
        }

        internal bool SearchAlfa()
        {
            if (this.helpState != HelpState.SEARCHING_USER)
                return false;

            this.customTimer = DateTime.Now;
            this.HelperId = 0;

            List<UInt32> onlineAlfas = OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Alfas;
            foreach (UInt32 alfaId in onlineAlfas)
            {
                if (this.nullAlfas.Contains(alfaId) || this.ReporterId == alfaId)
                    continue;

                GameClient alfa = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(alfaId);
                if (alfa == null || alfa.GetHabbo() == null || alfa.GetHabbo().AlfaServiceId != 0)
                {
                    continue;
                }

                ServerMessage Message = new ServerMessage(Outgoing.onGuideSessionAttached);
                Message.AppendBoolean(true); // true = tour,help - false = bully
                Message.AppendInt32(1); // 0 = tour, 1 = help
                Message.AppendString(reportMessage);
                Message.AppendInt32(15); // seconds
                alfa.SendMessage(Message);

                alfa.GetHabbo().AlfaServiceId = this.ReporterId;
                this.HelperId = alfaId;

                return true;
            }

            return false;
        }

        internal void sendMessage(ServerMessage Message)
        {
            GameClient _Reporter = Reporter;
            if (_Reporter == null)
            {
                this.helpState = HelpState.FINISHED;
                return;
            }

            GameClient _Alfa = Alfa;
            if (_Alfa == null)
            {
                this.helpState = HelpState.FINISHED;
                return;
            }

            _Reporter.SendMessage(Message);
            _Alfa.SendMessage(Message);
        }

        internal void SerializeAlfaChat()
        {
            if (this.helpState != HelpState.TALKING)
                return;

            GameClient _Reporter = Reporter;
            if (_Reporter == null)
            {
                this.helpState = HelpState.FINISHED;
                return; 
            }

            GameClient _Alfa = Alfa;
            if (_Alfa == null)
            {
                this.helpState = HelpState.FINISHED;
                return;
            }

            ServerMessage onGuideSessionStarted = new ServerMessage(Outgoing.onGuideSessionStarted);
            onGuideSessionStarted.AppendUInt(_Reporter.GetHabbo().Id);
            onGuideSessionStarted.AppendString(_Reporter.GetHabbo().Username);
            onGuideSessionStarted.AppendString(_Reporter.GetHabbo().Look);
            onGuideSessionStarted.AppendUInt(_Alfa.GetHabbo().Id);
            onGuideSessionStarted.AppendString(_Alfa.GetHabbo().Username);
            onGuideSessionStarted.AppendString(_Alfa.GetHabbo().Look);
            _Reporter.SendMessage(onGuideSessionStarted);
            _Alfa.SendMessage(onGuideSessionStarted);
        }

        internal void SerializeNoAlfas()
        {
            GameClient _Reporter = Reporter;
            if (_Reporter == null)
                return;

            // 0: Vaya, algo ha ido mal.
            // 1: Petición rechazada.
            ServerMessage Message = new ServerMessage(Outgoing.onGuideSessionError);
            Message.AppendInt32(1);
            _Reporter.SendMessage(Message);
        }
    }
}
