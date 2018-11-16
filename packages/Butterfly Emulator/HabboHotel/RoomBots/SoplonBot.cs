using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.RoomBots
{
    class SoplonBot : BotAI
    {
        private int SpeechTimer;
        private int ActionTimer;
        private int FollowTimer;

        internal SoplonBot(int VirtualId)
        {
            SpeechTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 40);
            ActionTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 30);
            FollowTimer = 1;
        }

        internal override void OnSelfEnterRoom()
        {
            if (GetRoom() == null)
                return;

            RoomUser User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(GetBotData().OwnerId);
            if (User == null)
                return;

            ServerMessage ChatMessage = new ServerMessage(Outgoing.Talk);
            ChatMessage.AppendInt32(GetRoomUser().VirtualId);
            ChatMessage.AppendString(LanguageLocale.GetValue("bot.soplon.enter.onself"));
            ChatMessage.AppendInt32(0);
            ChatMessage.AppendInt32(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR);
            ChatMessage.AppendInt32(0);
            ChatMessage.AppendInt32(-1);
            User.GetClient().SendMessage(ChatMessage);
        }

        internal override void OnSelfLeaveRoom(bool Kicked)
        {
            if(GetBotData() != null)
                GetBotData().Destroy();
        }

        internal override void OnUserEnterRoom(RoomUser User)
        {
            if (User == null || User.GetClient() == null)
                return;

            if (GetRoom() == null)
                return;

            if (GetRoom().RoomData.OwnerId == User.HabboId)
            {
                if (GetBotData().SoplonOnRoom.Count > 0 || GetBotData().SoplonLeaveRoom.Count > 0)
                {
                    ServerMessage ChatMessage = new ServerMessage(Outgoing.Talk);
                    ChatMessage.AppendInt32(GetRoomUser().VirtualId);
                    ChatMessage.AppendString(LanguageLocale.GetValue("bot.soplon.enter.owner"));
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR);
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(-1);
                    User.GetClient().SendMessage(ChatMessage);
                }
                else
                {
                    ServerMessage ChatMessage = new ServerMessage(Outgoing.Talk);
                    ChatMessage.AppendInt32(GetRoomUser().VirtualId);
                    ChatMessage.AppendString(LanguageLocale.GetValue("bot.soplon.enter.owner.nomessage"));
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR);
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(-1);
                    User.GetClient().SendMessage(ChatMessage);
                }

                return;
            }

            if (GetRoom().GetRoomUserManager().GetRoomUserByHabbo(GetRoom().RoomData.OwnerId) == null)
            {
                if (!GetBotData().SoplonOnRoom.Contains(User.HabboId))
                {
                    GetBotData().SoplonOnRoom.Add(User.HabboId);

                    ServerMessage ChatMessage = new ServerMessage(Outgoing.Talk);
                    ChatMessage.AppendInt32(GetRoomUser().VirtualId);
                    ChatMessage.AppendString(LanguageLocale.GetValue("bot.soplon.enter.keko"));
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR);
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(-1);
                    User.GetClient().SendMessage(ChatMessage);
                }
            }
        }

        internal override void OnUserLeaveRoom(GameClient Client)
        {
            if (Client == null || Client.GetHabbo() == null)
                return;

            if (GetRoom() == null || GetRoom().RoomData == null)
                return;

            if (GetRoom().RoomData.OwnerId == Client.GetHabbo().Id)
            {
                GetBotData().SoplonOnRoom.Clear();
                GetBotData().SoplonLeaveRoom.Clear();
            }
            else
            {
                if (GetBotData().SoplonOnRoom.Contains(Client.GetHabbo().Id))
                {
                    GetBotData().SoplonOnRoom.Remove(Client.GetHabbo().Id);

                    if (!GetBotData().SoplonLeaveRoom.Contains(Client.GetHabbo().Id))
                    {
                        GetBotData().SoplonLeaveRoom.Add(Client.GetHabbo().Id);
                    }
                }
            }
        }

        internal override void OnUserSay(RoomUser User, string Message)
        {
            if (User == null || User.GetClient() == null)
                return;

            if (GetRoom() == null)
                return;

            if (GetRoom().RoomData.OwnerId == User.HabboId)
            {
                if (Message.ToLower().Equals(LanguageLocale.GetValue("bot.soplon.keyword")))
                {
                    StringBuilder botMessage = new StringBuilder();
                    if (GetBotData().SoplonLeaveRoom.Count > 0)
                    {
                        foreach (UInt32 HabboId in GetBotData().SoplonLeaveRoom)
                        {
                            string Username = UsersCache.getUsernameById(HabboId);
                            if (String.IsNullOrEmpty(Username))
                                continue;

                            if ((LanguageLocale.GetValue("bot.soplon.leave").Length + botMessage.Length + Username.Length) >= 100)
                            {
                                botMessage = botMessage.Remove(botMessage.Length - 2, 2);

                                ServerMessage ChatMessage = new ServerMessage(Outgoing.Talk);
                                ChatMessage.AppendInt32(GetRoomUser().VirtualId);
                                ChatMessage.AppendString(botMessage + " " + LanguageLocale.GetValue("bot.soplon.leave"));
                                ChatMessage.AppendInt32(0);
                                ChatMessage.AppendInt32(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR);
                                ChatMessage.AppendInt32(0);
                                ChatMessage.AppendInt32(-1);
                                User.GetClient().SendMessage(ChatMessage);

                                botMessage.Clear();
                            }

                            botMessage.Append(Username + ", ");
                        }

                        if (botMessage.Length > 0)
                        {
                            botMessage = botMessage.Remove(botMessage.Length - 2, 2);

                            ServerMessage ChatMessage = new ServerMessage(Outgoing.Talk);
                            ChatMessage.AppendInt32(GetRoomUser().VirtualId);
                            ChatMessage.AppendString(botMessage + " " + LanguageLocale.GetValue("bot.soplon.leave"));
                            ChatMessage.AppendInt32(0);
                            ChatMessage.AppendInt32(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR);
                            ChatMessage.AppendInt32(0);
                            ChatMessage.AppendInt32(-1);
                            User.GetClient().SendMessage(ChatMessage);

                            botMessage.Clear();
                        }

                        GetBotData().SoplonLeaveRoom.Clear();
                    }

                    if (GetBotData().SoplonOnRoom.Count > 0)
                    {
                        foreach (UInt32 HabboId in GetBotData().SoplonOnRoom)
                        {
                            string Username = UsersCache.getUsernameById(HabboId);
                            if (String.IsNullOrEmpty(Username))
                                continue;

                            if ((LanguageLocale.GetValue("bot.soplon.onroom").Length + botMessage.Length + Username.Length) >= 100)
                            {
                                botMessage = botMessage.Remove(botMessage.Length - 2, 2);

                                ServerMessage ChatMessage = new ServerMessage(Outgoing.Talk);
                                ChatMessage.AppendInt32(GetRoomUser().VirtualId);
                                ChatMessage.AppendString(botMessage + " " + LanguageLocale.GetValue("bot.soplon.onroom"));
                                ChatMessage.AppendInt32(0);
                                ChatMessage.AppendInt32(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR);
                                ChatMessage.AppendInt32(0);
                                ChatMessage.AppendInt32(-1);
                                User.GetClient().SendMessage(ChatMessage);

                                botMessage.Clear();
                            }

                            botMessage.Append(Username + ", ");
                        }

                        if (botMessage.Length > 0)
                        {
                            botMessage = botMessage.Remove(botMessage.Length - 2, 2);

                            ServerMessage ChatMessage = new ServerMessage(Outgoing.Talk);
                            ChatMessage.AppendInt32(GetRoomUser().VirtualId);
                            ChatMessage.AppendString(botMessage + " " + LanguageLocale.GetValue("bot.soplon.onroom"));
                            ChatMessage.AppendInt32(0);
                            ChatMessage.AppendInt32(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR);
                            ChatMessage.AppendInt32(0);
                            ChatMessage.AppendInt32(-1);
                            User.GetClient().SendMessage(ChatMessage);

                            botMessage.Clear();
                        }

                        GetBotData().SoplonOnRoom.Clear();
                    }
                }
            }
        }

        internal override void OnUserShout(RoomUser User, string Message)
        {

        }

        internal override void OnTimerTick()
        {
            if (GetBotData() == null)
                return;

            if (SpeechTimer <= 0)
            {
                if (GetBotData().RandomSpeech.Count > 0 && GetBotData().ChatEnabled)
                {
                    RandomSpeech Speech = GetBotData().GetRandomSpeech();
                    GetRoomUser().Chat(null, Speech.Message, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);
                }

                try
                {
                    SpeechTimer = GetBotData().ChatSeconds * 2;
                }
                catch (Exception e)
                {
                    Logging.LogThreadException(e.ToString(), "Error in SpeechTimer Bot: (" + GetBotData().ChatSeconds + "): ");
                    SpeechTimer = 30;
                }
            }
            else
            {
                SpeechTimer--;
            }

            if (ActionTimer <= 0)
            {
                if (GetBotData().WalkingEnabled && GetBotData().followingUser == null)
                {
                    var nextCoord = GetRoom().GetGameMap().getRandomWalkableSquare();
                    GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                }

                ActionTimer = new Random().Next(1, 30);
            }
            else
            {
                ActionTimer--;
            }

            if (FollowTimer <= 0)
            {
                if (GetBotData().followingUser != null)
                {
                    RoomUser user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(GetBotData().followingUser.HabboId);
                    if (user != null)
                    {
                        GetRoomUser().MoveTo(GetBotData().followingUser.SquareInFront);
                    }
                }
                else
                {
                    GetBotData().followingUser = null;
                }

                FollowTimer = 1;
            }
            else
            {
                FollowTimer--;
            }
        }
    }
}