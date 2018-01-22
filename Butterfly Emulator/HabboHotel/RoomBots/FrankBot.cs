using System;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Pathfinding;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Core;
using Butterfly.HabboHotel.Pets;
using ButterStorm;
using System.Drawing;
using Butterfly.Messages;
using HabboEvents;

namespace Butterfly.HabboHotel.RoomBots
{
    class FrankBot : BotAI
    {
        private int SpeechTimer;
        private int ActionTimer;
        private int fraseAtual;

        internal FrankBot(Int32 VirtualId)
        {
            SpeechTimer = 0;
            ActionTimer = 0;
            fraseAtual = 0;
        }

        internal override void OnSelfEnterRoom()
        {
            var nextCoord = GetRoom().GetGameMap().getRandomWalkableSquare();
            if (GetRoomUser() != null)
                GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
        }

        internal override void OnSelfLeaveRoom(bool Kicked)
        {

        }

        internal override void OnUserEnterRoom(RoomUser User)
        {

        }

        internal override void OnUserLeaveRoom(GameClient Client)
        {

        }

        #region Commands
        internal override void OnUserSay(RoomUser User, string Message)
        {

        }
        #endregion

        internal override void OnUserShout(RoomUser User, string Message) { }

        internal override void OnTimerTick()
        {
            #region Speech
            if (GetBotData() == null || fraseAtual > 8)
                return;

            if (SpeechTimer <= 0)
            {
                var Pet = GetRoomUser();
                string[] Chat = new string[7];
                Chat[0] = LanguageLocale.GetValue("bot.frank.msg1");
                Chat[1] = LanguageLocale.GetValue("bot.frank.msg2");
                Chat[2] = LanguageLocale.GetValue("bot.frank.msg3");
                Chat[3] = LanguageLocale.GetValue("bot.frank.msg4");
                Chat[4] = LanguageLocale.GetValue("bot.frank.msg5");
                Chat[5] = LanguageLocale.GetValue("bot.frank.msg6");
                Chat[6] = LanguageLocale.GetValue("bot.frank.msg7");

                if (fraseAtual == 7){ 
                    GetRoomUser().Chat(null, "Até mais!", 33, false, true);
                    GetRoomUser().MoveTo(GetRoom().GetGameMap().Model.DoorX, GetRoom().GetGameMap().Model.DoorY);
                    GetRoomUser().isKicking = true;
                }else 
                if(fraseAtual == 8){
                    try{
                        GetRoom().GetRoomUserManager().RemoveBot(Pet.VirtualId, false);
                    }
                    catch (Exception e) { Logging.LogThreadException(e.ToString(), "Erro ao remover o bot da sala: "); }
                } else
                {
                    var rSpeech = Chat[fraseAtual];
                    GetRoomUser().Chat(null, rSpeech, 33, false, true);
                }

                try
                {
                    SpeechTimer = new Random().Next(5, 12);
                    fraseAtual++;
                }
                catch (Exception e)
                {
                    Logging.LogThreadException(e.ToString(), "Error in SpeechTimer Bot: (" + GetBotData().ChatSeconds + "): ");
                    SpeechTimer = 30;
                    fraseAtual = 3;
                }
            }
            else
            {
                SpeechTimer--;
            }
            #endregion

            if (ActionTimer <= 0)
            {
                try
                {
                    if (GetRoomUser().FollowingOwner != null)
                        ActionTimer = 2;
                    else
                        ActionTimer = new Random().Next(5, 12);

                    if (GetRoomUser().montandoBol != true)
                    {

                        if (GetRoomUser().FollowingOwner != null)
                        {
                            GetRoomUser().MoveTo(GetRoomUser().FollowingOwner.SquareBehind);
                        }
                        else
                        {
                            var nextCoord = GetRoom().GetGameMap().getRandomWalkableSquare();
                            GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.HandleException(e, "PetBot.OnTimerTick");
                }
            }
            else
            {
                ActionTimer--;
            }
        }
    }
}
