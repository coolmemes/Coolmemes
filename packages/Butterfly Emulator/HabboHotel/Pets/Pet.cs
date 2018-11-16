using System;

using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using ButterStorm;
using HabboEvents;
using System.Drawing;

namespace Butterfly.HabboHotel.Pets
{
    class Pet
    {
        internal uint PetId;
        internal uint OwnerId;
        internal int VirtualId;

        internal uint Type;
        internal string Name;
        internal string Race;
        internal string Color;

        internal int Expirience;
        internal int Energy;
        internal int Nutrition;

        internal uint RoomId;
        internal int X;
        internal int Y;
        internal double Z;

        internal int Respect;

        internal double CreationStamp;
        internal bool PlacedInRoom;

        internal uint waitingForBreading;
        internal Point breadingTile;

        internal string Accessories;

        internal int[] experienceLevels = { 100, 200, 400, 600, 1000, 1300, 1800, 2400, 3200, 4300, 7200, 8500, 10100, 13300, 17500, 23000, 31000, 37500, 45000, 51900 }; // ty scott
        internal DatabaseUpdateState DBState;

        internal Room Room
        {
            get
            {
                if (!IsInRoom)
                {
                    return null;
                }

                return OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            }
        }

        internal bool IsInRoom
        {
            get
            {
                return (RoomId > 0);
            }
        }

        internal int Level
        {
            get
            {
                for (var level = 1; level < experienceLevels.Length; ++level)
                {
                    if (Expirience < experienceLevels[level])
                        return level;
                }
                return experienceLevels.Length;
            }
        }

        internal static int MaxLevel
        {
            get
            {
                return 20;
            }
        }

        internal int ExpirienceGoal
        {
            get
            {
                return experienceLevels[Level - 1];
            }
        }

        internal static int MaxEnergy
        {
            get
            {
                return 120;
            }
        }

        internal static int MaxNutrition
        {
            get
            {
                return 100;
            }
        }

        internal int Age
        {
            get
            {
                return (int)Math.Floor((OtanixEnvironment.GetUnixTimestamp() - CreationStamp) / 86400);
            }
        }

        internal string Look
        {
            get
            {
                return Type + " " + Race + " " + Color;
            }
        }

        internal string OwnerName
        {
            get
            {
                return OtanixEnvironment.GetGame().GetClientManager().GetNameById(OwnerId);
            }
        }

        internal bool AllCanMount;
        internal int HaveSaddle;
        internal int HairDye;
        internal int PetHair;

        internal Pet(uint PetId, uint OwnerId, uint RoomId, string Name, uint Type, string Race, string Color, int Expirience, int Energy, int Nutrition, int Respect, double CreationStamp, int X, int Y, double Z, bool allcanmount, int havesaddle, int hairdye, int PetHair, string Accessories)
        {
            this.PetId = PetId;
            this.OwnerId = OwnerId;
            this.RoomId = RoomId;
            this.Name = Name;
            this.Type = Type;
            this.Race = Race;
            this.Color = Color;
            this.Expirience = Expirience;
            this.Energy = Energy;
            this.Nutrition = Nutrition;
            this.Respect = Respect;
            this.CreationStamp = CreationStamp;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.PlacedInRoom = false;
            this.DBState = DatabaseUpdateState.Updated;
            this.AllCanMount = allcanmount;
            this.HaveSaddle = havesaddle;
            this.HairDye = hairdye;
            this.PetHair = PetHair;
            this.waitingForBreading = 0;
            this.Accessories = Accessories;
        }

        internal void OnRespect()
        {
            Respect++;

            if (DBState != DatabaseUpdateState.NeedsInsert)
                DBState = DatabaseUpdateState.NeedsUpdate;

            AddExpirience(10);
            AddNutrition(10);
        }

        internal void AddNutrition(int Amount)
        {
            Nutrition = Nutrition + 10;

            if (Nutrition > 100)
                Nutrition = 100;

            if (DBState != DatabaseUpdateState.NeedsInsert)
                DBState = DatabaseUpdateState.NeedsUpdate;
        }

        internal void AddExpirience(int Amount)
        {
            Expirience += Amount;

            if (Expirience >= 51900)
            {
                Expirience = 51900;
                return;
            }

            if (DBState != DatabaseUpdateState.NeedsInsert)
                DBState = DatabaseUpdateState.NeedsUpdate;

            if (Room != null)
            {
                var Message = new ServerMessage(Outgoing.AddExperience);
                Message.AppendUInt(PetId);
                Message.AppendInt32(VirtualId);
                Message.AppendInt32(Amount);
                Room.SendMessage(Message);

                if (Expirience >= ExpirienceGoal)
                {
                    var Session = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(OwnerId);

                    var NewLevel = new ServerMessage(Outgoing.PetNewLevel);
                    NewLevel.AppendUInt(PetId);
                    NewLevel.AppendString(Name);
                    NewLevel.AppendInt32(Level);
                    NewLevel.AppendUInt(Type);
                    NewLevel.AppendInt32(int.Parse(Race));
                    NewLevel.AppendString(Color);
                    NewLevel.AppendInt32(0);
                    NewLevel.AppendInt32(0);
                    if (Session != null)
                        Session.SendMessage(NewLevel);

                    var ChatMessage = new ServerMessage(Outgoing.Talk);
                    ChatMessage.AppendInt32(VirtualId);
                    ChatMessage.AppendString("*Nivel superado-¡Sabe más que tú!*");
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(-1);
                    Room.SendMessage(ChatMessage);

                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(OwnerId, "ACH_PetLevelUp", 1);
                    if (Type == 26) // gnome
                        OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(OwnerId, "ACH_GnomeLevelUp", 1);
                }
            }
        }

        internal void PetEnergy(bool Add, bool correctAction)
        {
            if (Add)
            {
                if (Energy == 100) // If Energy is 100, no point.
                    return;
            }
            else
            {
                if (correctAction)
                {
                    if (Energy < 10)
                    {
                        Energy = 0;
                        return;
                    }
                }
                else
                {
                    if (Energy < 5)
                    {
                        Energy = 0;
                        return;
                    }
                }
            }

            if (Add)
                Energy = Energy + 10;
            else
                Energy = Energy - ((correctAction) ? 10 : 5);

            if (DBState != DatabaseUpdateState.NeedsInsert)
                DBState = DatabaseUpdateState.NeedsUpdate;
        }

        internal void SerializeInventory(ServerMessage Message)
        {
            Message.AppendUInt(PetId); // Id
            Message.AppendString(Name); // Name

            // PetFigureData:
            Message.AppendUInt(Type); // TypeId
            Message.AppendInt32(int.Parse(Race)); // paletteId
            Message.AppendString(Color); // Color
            Message.AppendInt32(0); // raza? pet.breed.X.THIS
            if (Accessories.Length > 0 && Accessories.Contains(" "))
            {
                string[] arrParts = Accessories.Substring(1, Accessories.Length - 1).Split(' ');
                foreach (string str in arrParts)
                {
                    Message.AppendInt32(int.Parse(str));
                }
            }
            else
            {
                Message.AppendInt32(0);
            }
            Message.AppendInt32(Level); // level
        }

        internal ServerMessage SerializeInfo()
        {
            var Nfo = new ServerMessage(Outgoing.PetInformation);
            Nfo.AppendUInt(PetId);
            Nfo.AppendString(Name);
            Nfo.AppendInt32(Level);
            Nfo.AppendInt32(MaxLevel);
            Nfo.AppendInt32(Expirience);
            Nfo.AppendInt32(ExpirienceGoal);
            Nfo.AppendInt32(Energy);
            Nfo.AppendInt32(MaxEnergy);
            Nfo.AppendInt32(Nutrition);
            Nfo.AppendInt32(MaxNutrition);
            Nfo.AppendInt32(Respect);
            Nfo.AppendUInt(OwnerId);
            Nfo.AppendInt32(Age);
            Nfo.AppendString(OwnerName);
            Nfo.AppendInt32(1);

            Nfo.AppendBoolean(HaveSaddle != 0);

            if (Room != null && Room.GetRoomUserManager() != null && Room.GetRoomUserManager().GetRoomUserByVirtualId(VirtualId) != null)
                Nfo.AppendBoolean(Room.GetRoomUserManager().GetRoomUserByVirtualId(VirtualId).montandoBol);
            else
                Nfo.AppendBoolean(false);

            Nfo.AppendInt32(5);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(2);
            Nfo.AppendInt32(6);
            Nfo.AppendInt32(10);
            Nfo.AppendInt32(14);
            Nfo.AppendInt32(AllCanMount ? 1 : 0);
            Nfo.AppendInt32(0);
            Nfo.AppendString("");
            Nfo.AppendBoolean(false);
            Nfo.AppendInt32(-1);
            Nfo.AppendInt32(-1);
            Nfo.AppendInt32(-1);
            Nfo.AppendBoolean(false);
            return Nfo;
        }
    }

    internal enum DatabaseUpdateState
    {
        Updated,
        NeedsUpdate,
        NeedsInsert
    }
}
