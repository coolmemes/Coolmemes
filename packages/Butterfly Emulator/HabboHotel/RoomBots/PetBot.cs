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
    class PetBot : BotAI
    {
        private int SpeechTimer;
        private int ActionTimer;
        private int EnergyTimer;

        internal PetBot(Int32 VirtualId)
        {
            SpeechTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
            ActionTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 30 + VirtualId);
            EnergyTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
        }

        private void RemovePetStatus()
        {
            var Pet = GetRoomUser();

            // Remove Status
            Pet.Statusses.Remove("sit");
            Pet.Statusses.Remove("lay");
            Pet.Statusses.Remove("snf");
            Pet.Statusses.Remove("eat");
            Pet.Statusses.Remove("ded");
            Pet.Statusses.Remove("jmp");
            Pet.Statusses.Remove("gst sml");
            Pet.Statusses.Remove("wng");
            Pet.Statusses.Remove("beg");
            Pet.Statusses.Remove("flm");
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
            var Pet = GetRoomUser();
            if (Pet == null || Pet.PetData == null)
                return;

            if (Pet.PetData.DBState != DatabaseUpdateState.NeedsInsert)
                Pet.PetData.DBState = DatabaseUpdateState.NeedsUpdate;

            if (Message.ToLower().Equals(Pet.PetData.Name.ToLower()))
            {
                Pet.SetRot(Rotation.Calculate(Pet.X, Pet.Y, User.X, User.Y), false);
                return;
            }

            if (Message.ToLower().StartsWith(Pet.PetData.Name.ToLower() + " ") && User.GetClient().GetHabbo().Username.ToLower() == Pet.PetData.OwnerName.ToLower())
            {
                var Command = Message.Substring(Pet.PetData.Name.ToLower().Length + 1);

                if (PetOrders.PetCanDoCommand(Pet.PetData.Type, Pet.PetData.Level, Command) == false)
                {
                    var Speech = PetLocale.GetValue("pet.unknowncommand");
                    var rSpeech = Speech[new Random().Next(0, Speech.Length - 1)];

                    Pet.Chat(null, rSpeech, 0, false);

                    return;
                }

                int randomNumber = new Random().Next(4);
                if ((randomNumber == 3 || Pet.PetData.Energy < 10) && Command != "COMER" && Command != "Cruzar") // no hacemos el comando.
                {
                    var Speech = PetLocale.GetValue("pet.tired");
                    var rSpeech = Speech[new Random().Next(0, Speech.Length - 1)];

                    Pet.Chat(null, rSpeech, 0, false);
                    Pet.PetData.PetEnergy(false, false);

                    return;
                }
    
                switch (Command)
                {
                    case "DESCANSA":
                        {
                            // nothing
                            break;
                        }

                    case "HABLA":
                        {
                            var Speech = PetLocale.GetValue("speech.pet" + Pet.PetData.Type);
                            var rSpeech = Speech[new Random().Next(0, Speech.Length - 1)];

                            Pet.Chat(null, rSpeech, 0, false);

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "JUEGA":
                        {


                            break;
                        }

                    case "CALLA":
                        {
                            // nothing.. i think.. JEP
                            break;
                        }

                    case "A CASA":
                        {
                            RemovePetStatus();

                            Point coord = GetRoom().GetRoomItemHandler().getRandomHome();
                            if (coord == new Point())
                            {
                                Pet.Chat(null, "*buscando un juguete*", 0, false);
                                return;
                            }

                            Pet.MoveTo(coord);

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            // Pet.Statusses.Add("eat", TextHandling.GetString(Pet.Z));
                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "BEBE":
                        {
                            RemovePetStatus();

                            Point coord = GetRoom().GetRoomItemHandler().getRandomWaterbowl();
                            if (coord == new Point())
                            {
                                Pet.Chat(null, "*sediento*", 0, false);
                                return;
                            }

                            Pet.MoveTo(coord);

                            Pet.PetData.AddExpirience(5);
                            Pet.PetData.PetEnergy(false, true);

                            Pet.Statusses.Add("eat", TextHandling.GetString(Pet.Z));
                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "IZQUIERDA":
                        {
                            RemovePetStatus();

                            Pet.RotHead--;
                            Pet.RotBody--;
                            if(Pet.RotHead < 0 || Pet.RotBody < 0)
                            {
                                Pet.RotHead = 7;
                                Pet.RotBody = 7;
                            }
                           
                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "DERECHA":
                        {
                            RemovePetStatus();

                            Pet.RotHead++;
                            Pet.RotBody++;
                            if (Pet.RotHead >7 || Pet.RotBody > 7)
                            {
                                Pet.RotHead = 0;
                                Pet.RotBody = 0;
                            }

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "FÚTBOL":
                        {
                            RemovePetStatus();

                            Pet.PetData.AddExpirience(15);
                            Pet.PetData.PetEnergy(false, true);

                            if (GetRoom().GotSoccer())
                            {
                                Pet.MoveTo(GetRoom().GetSoccer().Ball.Coordinate);
                            }

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "ARRODÍLLATE":
                        {

                            break;
                        }

                    case "BOTA":
                        {

                            break;
                        }

                    case "SIÉNTATE":
                        {
                            RemovePetStatus();

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            Pet.Statusses.Add("sit", TextHandling.GetString(Pet.Z));
                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "ESTATUA":
                        {

                            break;
                        }

                    case "BAILA":
                        {

                            break;
                        }

                    case "GIRA":
                        {

                            break;
                        }

                    case "ENCIENDE TV":
                        {

                            break;
                        }

                    case "ADELANTE":
                        {
                            RemovePetStatus();

                            Pet.PetData.AddExpirience(15);
                            Pet.PetData.PetEnergy(false, true);

                            Pet.MoveTo(Pet.SquareInFront);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "RELAX":
                        {

                            break;
                        }

                    case "CROA":
                        {

                            break;
                        }

                    case "INMERSIÓN":
                        {
                            RemovePetStatus();

                            Pet.Statusses.Add("lay", TextHandling.GetString(Pet.Z));

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "TÚMBATE":
                        {
                            RemovePetStatus();

                            Pet.Statusses.Add("lay", TextHandling.GetString(Pet.Z));

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "SALUDA":
                        {

                            break;
                        }

                    case "MARCHA":
                        {

                            break;
                        }

                    case "GRAN SALTO":
                        {

                            break;
                        }

                    case "BAILE POLLO":
                        {

                            break;
                        }

                    case "TRIPLE SALTO":
                        {

                            break;
                        }

                    case "MUESTRA ALAS":
                        {
                            RemovePetStatus();

                            Pet.Statusses.Add("wng", TextHandling.GetString(Pet.Z));

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "ECHA FUEGO":
                        {
                            RemovePetStatus();

                            Pet.Statusses.Add("flm", TextHandling.GetString(Pet.Z));

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "PLANEA":
                        {

                            break;
                        }

                    case "ANTORCHA":
                        {

                            break;
                        }

                    case "VEN AQUÍ":
                        {
                            RemovePetStatus();

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            Pet.MoveTo(User.SquareInFront);
                            ActionTimer = 25;

                            break;
                        }

                    case "CAMBIA VUELO":
                        {

                            break;
                        }

                    case "VOLTERETA":
                        {

                            break;
                        }

                    case "ANILLO FUEGO":
                        {

                            break;
                        }

                    case "COMER":
                        {
                            RemovePetStatus();

                            Point coord = GetRoom().GetRoomItemHandler().getRandomPetfood();
                            if (coord == new Point())
                            {
                                Pet.Chat(null, "*hambriento*", 0, false);
                                return;
                            }

                            Pet.MoveTo(coord);

                            Pet.PetData.AddExpirience(5);
                            Pet.PetData.PetEnergy(true, true);

                            var Session = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Pet.PetData.OwnerId);
                            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Pet.PetData.OwnerId, "ACH_PetFeeding", 10);
                            if(Pet.PetData.Type == 26) // gnome
                                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Pet.PetData.OwnerId, "ACH_GnomeFeeding", 10);

                            Pet.Statusses.Add("eat", TextHandling.GetString(Pet.Z));
                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "MOVER COLA":
                        {

                            break;
                        }

                    case "Cuenta":
                        {

                            break;
                        }

                    case "Cruzar":
                        {
                            RemovePetStatus();

                            Point coord = new Point();
                            if(Pet.PetData.Type == 0 || Pet.PetData.Type == 1 ||Pet.PetData.Type == 2 ||Pet.PetData.Type == 3 ||Pet.PetData.Type == 4)
                                coord = GetRoom().GetRoomItemHandler().getRandomBreedingPet(Pet.PetData);

                            if (coord == new Point())
                            {
                                ServerMessage alert = new ServerMessage(Outgoing.NoBreedingFurni);
                                alert.AppendInt32(0);
                                User.GetClient().SendMessage(alert);

                                return;
                            }

                            Pet.MoveTo(coord);
                            Pet.PetData.AddExpirience(0);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "PIDE":
                        {
                            RemovePetStatus();

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            Pet.Statusses.Add("beg", TextHandling.GetString(Pet.Z));
                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "HAZ EL MUERTO":
                        {
                            RemovePetStatus();

                            Pet.Statusses.Add("ded", TextHandling.GetString(Pet.Z));

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "QUIETO":
                        {
                            RemovePetStatus();

                            Pet.PetData.AddExpirience(5);
                            Pet.PetData.PetEnergy(false, true);

                            Pet.IsWalking = false;
                            Pet.FollowingOwner = null;

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "SÍGUEME":
                        {
                            RemovePetStatus();

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            Pet.FollowingOwner = User;
                            Pet.MoveTo(User.SquareBehind);

                            ActionTimer = 2;
                            EnergyTimer = 120;

                            break;
                        }

                    case "LEVANTA":
                        {
                            RemovePetStatus();

                            Pet.Statusses.Add("std", TextHandling.GetString(Pet.Z));

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 25;
                            EnergyTimer = 120;

                            break;
                        }

                    case "SALTA":
                        {

                            RemovePetStatus();

                            Pet.Statusses.Add("jmp", TextHandling.GetString(Pet.Z));

                            Pet.PetData.AddExpirience(10);
                            Pet.PetData.PetEnergy(false, true);

                            ActionTimer = 6;
                            EnergyTimer = 120;

                            break;
                        }
                }
            }
        }
        #endregion

        internal override void OnUserShout(RoomUser User, string Message) { }

        internal override void OnTimerTick()
        {
            #region Speech
            if (SpeechTimer <= 0)
            {
                var Pet = GetRoomUser();
                if (Pet.PetData.DBState != DatabaseUpdateState.NeedsInsert)
                    Pet.PetData.DBState = DatabaseUpdateState.NeedsUpdate;

                if (Pet != null)
                {
                    var RandomSpeech = new Random();
                    RemovePetStatus();

                    var Speech = PetLocale.GetValue("speech.pet" + Pet.PetData.Type);

                    var rSpeech = Speech[RandomSpeech.Next(0, Speech.Length - 1)];

                    if (rSpeech.Length != 3)
                        Pet.Chat(null, rSpeech, 0, false);
                    else
                        Pet.Statusses.Add(rSpeech, TextHandling.GetString(Pet.Z));
                }
                SpeechTimer = new Random().Next(20, 120);
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
                    RemovePetStatus();

                    if (GetRoomUser().FollowingOwner != null)
                        ActionTimer = 2;
                    else
                        ActionTimer = new Random().Next(15, 40 + GetRoomUser().PetData.VirtualId);

                    if (GetRoomUser().montandoBol != true)
                    {
                        RemovePetStatus();

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

            if (EnergyTimer <= 0)
            {
                RemovePetStatus(); // Remove Status

                var Pet = GetRoomUser();

                Pet.PetData.PetEnergy(true, false); // Add Energy

                EnergyTimer = new Random().Next(30, 120); // 2 Min Max
            }
            else
            {
                EnergyTimer--;
            }
        }
    }
}
