using Butterfly.Messages;
using System;
using System.Collections.Generic;
using ButterStorm;
using Butterfly.HabboHotel.Rooms;

namespace Butterfly.HabboHotel.RoomBots
{
    class RoomBot
    {
        internal uint BotId;
        internal uint OwnerId;

        internal string Name;
        internal string Motto;
        internal string Gender;
        internal string Look;

        internal UInt32 RoomId;

        internal bool WalkingEnabled;
        internal int X;
        internal int Y;
        internal int Z;
        internal int Rot;

        internal bool ChatEnabled;
        internal string ChatText;
        internal List<RandomSpeech> RandomSpeech;
        internal int ChatSeconds;

        internal List<uint> SoplonOnRoom;
        internal List<uint> SoplonLeaveRoom;

        internal RoomUser followingUser;

        internal bool IsDancing;

        internal AIType AiType;

        internal bool IsPet
        {
            get
            {
                return (AiType == AIType.Pet);
            }
        }

        internal string OwnerName
        {
            get
            {
                return OtanixEnvironment.GetGame().GetClientManager().GetNameById(OwnerId);
            }
        }

        internal RoomBot(uint BotId, UInt32 OwnerId, UInt32 RoomId, AIType AiType, bool WalkingEnabled, string Name, string Motto, string Gender, string Look,
            int X, int Y, int Z, int Rot, bool ChatEnabled, string ChatText, int ChatSeconds, bool IsDancing)
        {
            this.BotId = BotId;
            this.OwnerId = OwnerId;
            this.RoomId = RoomId;
            this.AiType = AiType;
            this.Name = Name;
            this.Motto = Motto;
            this.Gender = Gender;
            this.Look = Look;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Rot = Rot;
            this.WalkingEnabled = WalkingEnabled;
            this.ChatEnabled = ChatEnabled;
            this.ChatText = ChatText;
            if (ChatSeconds >= int.MaxValue / 2)
                this.ChatSeconds = 7;
            else
                this.ChatSeconds = ChatSeconds;
            this.IsDancing = IsDancing;
            this.SoplonOnRoom = new List<uint>();
            this.SoplonLeaveRoom = new List<uint>();
            this.followingUser = null;

            LoadRandomSpeech(this.BotId, this.ChatText);
        }

        internal void LoadRandomSpeech(uint BotId, string Text)
        {
            RandomSpeech = new List<RandomSpeech>();
            if (Text.Length > 0 && Text != "")
            {
                var Lines = Text.Split(';');
                foreach (var Line in Lines)
                {
                    RandomSpeech.Add(new RandomSpeech(Line, BotId));
                }
            }
        }

        internal RandomSpeech GetRandomSpeech()
        {
            return RandomSpeech[new Random().Next(0, (RandomSpeech.Count))];
        }

        internal BotAI GenerateBotAI(int VirtualId)
        {
            switch (AiType)
            {
                default:
                case AIType.Generic:
                case AIType.Waiter:
                    return new GenericBot(VirtualId);
                case AIType.Soplon:
                    return new SoplonBot(VirtualId);
                case AIType.Frank:
                    return new FrankBot(VirtualId);
                case AIType.Pet:
                    return new PetBot(VirtualId);
            }
        }

        internal void SerializeInventory(ServerMessage Message)
        {
            Message.AppendUInt(BotId);
            Message.AppendString(Name);
            Message.AppendString(Motto);
            Message.AppendString(Gender);
            Message.AppendString(Look);
        }

        internal void Destroy()
        {
            this.SoplonOnRoom.Clear();
            this.SoplonLeaveRoom.Clear();

            this.SoplonOnRoom = null;
            this.SoplonLeaveRoom = null;
        }
    }

    internal enum AIType
    {
        Pet,
        Generic,
        Waiter,
        Frank,
        Soplon
    }
}
