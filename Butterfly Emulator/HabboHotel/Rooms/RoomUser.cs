using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Butterfly.HabboHotel.ChatMessageStorage;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.Pets;
using Butterfly.HabboHotel.Quests;
using Butterfly.HabboHotel.RoomBots;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.Messages;
using ButterStorm;
using Uber.HabboHotel.Rooms;
using System.Drawing;
using Butterfly.Core;
using HabboEvents;
using Butterfly.HabboHotel.Pathfinding;
using Butterfly.HabboHotel.Rooms.Extras;
using Butterfly.HabboHotel.Filter;
using Butterfly.HabboHotel.Items;

namespace Butterfly.HabboHotel.Rooms
{
    public class RoomUser : IEquatable<RoomUser>
    {
        internal UInt32 HabboId;
        internal Int32 VirtualId;
        internal UInt32 RoomId;

        internal UInt32 IdleTime;

        internal Int32 X;
        internal Int32 Y;
        internal Double Z;
        internal Byte SqState;

        internal Int32 CarryItemID;
        internal Int32 CarryTimer;

        internal Int32 RotHead;
        internal Int32 RotBody;

        internal bool CanWalk;
        internal bool AllowOverride;
        internal bool TeleportEnabled;
        internal bool isKicking;

        internal int GoalX;
        internal int GoalY;

        internal uint tentId;

        internal int DeveloperState;
        internal int DeveloperX;
        internal int DeveloperY;

        internal List<Vector2D> Path = new List<Vector2D>();
        internal bool PathRecalcNeeded = false;
        internal int PathStep = 1;
        internal bool fastWalk = false;
        internal bool comandoFreeze = false;

        internal int handelingBallStatus = 0;
        internal UInt32 wiredItemIdTrigger;

        internal Boolean SetStep;
        internal int SetX;
        internal int SetY;
        internal double SetZ;

        internal RoomBot BotData;
        internal BotAI BotAI;

        internal ItemEffectType CurrentItemEffect;
        internal Boolean skatingTimerSecond;
        internal Boolean normalSkateTimerSecond;
        internal Boolean Freezed;
        internal Boolean throwBallAtGoal;
        internal int FreezeCounter;
        internal Team team;
        internal FreezePowerUp banzaiPowerUp;
        internal int FreezeLives;

        internal bool shieldActive;
        internal int shieldCounter;

        internal bool IgnoreChat;
        internal bool IgnorePets;
        internal bool IgnoreBots;

        internal Boolean needSqlUpdate;

        internal bool moonwalkEnabled = false;
        internal bool isFlying = false;
        internal int flyk = 0;
        internal RoomUser walkingToPet = null;
        internal bool montandoBol = false;
        internal bool sentadoBol = false;
        internal bool acostadoBol = false;
        internal int montandoID = 0;
        internal double lastTeleBanzai = 0;

        internal int BanzaiPoints = 0;
        internal int FreezePoints = 0;
        internal uint classPoints = 0;

        internal Stopwatch lastActionStopwatch;

        internal Stopwatch enteredStopwatch;
        internal Point Coordinate
        {
            get
            {
                return new Point(X, Y);
            }
        }

        internal Point SquareInFront
        {
            get
            {
                var Sq = new Point(X, Y);

                if (RotBody == 0)
                {
                    Sq.Y--;
                }
                else if (RotBody == 1)
                {
                    Sq.X++;
                    Sq.Y--;
                }
                else if (RotBody == 2)
                {
                    Sq.X++;
                }
                else if (RotBody == 3)
                {
                    Sq.X++;
                    Sq.Y++;
                }
                else if (RotBody == 4)
                {
                    Sq.Y++;
                }
                else if (RotBody == 5)
                {
                    Sq.X--;
                    Sq.Y++;
                }
                else if (RotBody == 6)
                {
                    Sq.X--;
                }
                else if (RotBody == 7)
                {
                    Sq.X--;
                    Sq.Y--;
                }

                return Sq;
            }
        }

        internal Point SquareBehind
        {
            get
            {
                var Sq = new Point(X, Y);

                if (RotBody == 0)
                {
                    Sq.Y++;
                }
                else if (RotBody == 1)
                {
                    Sq.Y++;
                    Sq.X--;
                }
                else if (RotBody == 2)
                {
                    Sq.X--;
                }
                else if (RotBody == 3)
                {
                    Sq.Y--;
                    Sq.X--;
                }
                else if (RotBody == 4)
                {
                    Sq.Y--;
                }
                else if (RotBody == 5)
                {
                    Sq.Y--;
                    Sq.X++;
                }
                else if (RotBody == 6)
                {
                    Sq.X++;
                }
                else if (RotBody == 7)
                {
                    Sq.Y++;
                    Sq.X++;
                }

                return Sq;
            }
        }

        public bool Equals(RoomUser comparedUser)
        {
            return (comparedUser.HabboId == HabboId);
        }

        internal bool IsPet
        {
            get
            {
                return (IsBot && BotData.IsPet);
            }
        }

        internal string GetUsername()
        {
            if (IsBot)
            {
                return BotData.Name;
            }

            if (GetClient() != null && GetClient().GetHabbo() != null)
            {
                return GetClient().GetHabbo().Username;
            }

            return string.Empty;
        }

        internal double GetTempoPreso()
        {
            if (GetClient() != null && GetClient().GetHabbo() != null)
            {
                return (OtanixEnvironment.GetGame().GetPrisaoManager().tempoRestantePreso(GetClient().GetHabbo().Id));
            }
            return 0;
        }

        internal string GetMotto()
        {
            if (IsBot)
            {
                return BotData.Motto;
            }
            else
            {
                if (GetClient() != null && GetClient().GetHabbo() != null)
                {
                    return GetClient().GetHabbo().Motto;
                }
            }

            return string.Empty;
        }

        internal string GetLook()
        {
            if (IsPet)
            {
                return BotData.Look.ToLower() + PetData.Accessories;
            }
            else if (IsBot)
            {
                return BotData.Look;
            }
            else
            {
                if (GetClient() != null && GetClient().GetHabbo() != null)
                {
                    if (GetClient().GetHabbo().ConvertedOnPet)
                        return GetClient().GetHabbo().PetData;
                    else
                        return GetClient().GetHabbo().Look;
                }
            }

            return string.Empty;
        }

        internal int GetGhost()
        {
            if (IsPet)
                return 2;
            else if (IsBot)
                return 4;
            else
            {
                if (GetClient() != null && GetClient().GetHabbo() != null && GetClient().GetHabbo().ConvertedOnPet)
                    return 2;
                else
                    return 1;
            }
        }

        internal int CurrentEffect
        {
            get
            {
                return GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect;
            }
        }

        internal Pet PetData;

        internal Boolean IsWalking;
        internal int SeatCount;
        internal RoomUser FollowingOwner;
        internal Boolean UpdateNeeded;
        internal int UpdateNeededCounter;
        internal Boolean IsAsleep;
        internal bool IsTeleporting;

        internal Dictionary<string, string> Statusses;

        internal int DanceId;

        internal int FloodCount;

        internal Boolean IsDancing
        {
            get
            {
                if (DanceId >= 1)
                {
                    return true;
                }

                return false;
            }
        }

        internal Boolean NeedsAutokick
        {
            get
            {
                if (IsBot)
                    return false;
                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return true;
                if (GetClient().GetHabbo().Rank >= 9)
                    return false;
                if (lastActionStopwatch.ElapsedMilliseconds >= 600000)
                    return true;

                return false;
            }
        }

        internal bool IsTrading
        {
            get
            {
                if (IsBot)
                {
                    return false;
                }

                if (Statusses.ContainsKey("trd"))
                {
                    return true;
                }

                return false;
            }
        }

        internal bool IsOwner()
        {
            if (IsBot)
                return false;

            return (GetUsername() == GetRoom().RoomData.Owner);
        }

        internal bool IsBot
        {
            get
            {
                if (BotData != null)
                {
                    return true;
                }

                return false;
            }
        }

        internal bool IsSpectator;

        internal int InternalRoomID;

        private Queue events;

        internal RoomUser(uint HabboId, uint RoomId, int VirtualId, Room room, bool isSpectator)
        {
            this.enteredStopwatch = new Stopwatch();
            this.enteredStopwatch.Start();
            this.Freezed = false;
            this.HabboId = HabboId;
            this.RoomId = RoomId;
            this.VirtualId = VirtualId;
            this.IdleTime = 0;
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
            this.RotHead = room.GetGameMap().Model.DoorOrientation;
            this.RotBody = room.GetGameMap().Model.DoorOrientation;
            this.UpdateNeeded = true;
            this.Statusses = new Dictionary<string, string>();
            this.lastActionStopwatch = new Stopwatch();
            this.lastActionStopwatch.Start();
            this.mRoom = room;

            this.IgnoreChat = false;

            this.AllowOverride = false;
            this.CanWalk = true;

            this.IsSpectator = isSpectator;
            this.SqState = 3;

            this.InternalRoomID = 0;
            this.CurrentItemEffect = ItemEffectType.None;
            this.events = new Queue();
            this.FreezeLives = 0;
        }

        internal void Unidle()
        {
            IdleTime = 0;
            lastActionStopwatch.Restart();

            if (IsAsleep)
            {
                IsAsleep = false;

                var Message = new ServerMessage(Outgoing.IdleStatus);
                Message.AppendInt32(VirtualId);
                Message.AppendBoolean(false);

                GetRoom().SendMessage(Message);
            }
        }

        internal void OnFly()
        {
            if (flyk == 0)
            {
                flyk++;
                return;
            }

            flyk++;
            GetRoom().SendMessage(GetRoom().GetRoomItemHandler().UpdateUserOnRoller(this, Coordinate, 0));
        }

        internal void Dispose()
        {
            Statusses.Clear();
            mRoom = null;
            mClient = null;
        }

        internal void Chat(GameClient Session, string Message, int Color, bool Shout, bool frank = false)
        {
            #region Progress
            #region Checks
            if (frank)
                goto NoCheckings;

            if (Message.Length <= 0 || Message.Length > 100) // si el mensaje es mayor que la máxima longitud (scripter)
                return;

            if (OtanixEnvironment.ContainsHTMLCode(Message))
            {
                WhisperComposer(LanguageLocale.GetValue("chat.html.detected"));
                return;
            }

            if (IsPet || IsBot) // si no es un usuario, directamente saltamos a mandar el mensaje
                goto NoCheckings;

            if (IsSpectator)
                return;

            if (Session == null || Session.GetHabbo() == null) // si el usuario ya está desconectado, pasamos de todo
                return;

            if (!Session.GetHabbo().passouPin)
            {
                WhisperComposer("Você precisa digitar o pin staff");
                return;
            }
            #endregion

            #region Muted
            if (!GetRoom().CheckRights(Session, true)) // Si no es un staff comprobamos si está muteado.
            {
                if (GetRoom().RoomMuted)
                    return;

                int timeToEndRoomMute = GetRoom().HasMuteExpired(Session.GetHabbo().Id);
                int timeToEndGlobalMute = OtanixEnvironment.GetGame().GetMuteManager().HasMuteExpired(Session.GetHabbo().Id);
                int timeMuted = (timeToEndGlobalMute > timeToEndRoomMute) ? timeToEndGlobalMute : timeToEndRoomMute;

                if (timeMuted > 0)
                {
                    ServerMessage message = new ServerMessage(Outgoing.MuteTimerMessageComposer);
                    message.AppendInt32(timeMuted);
                    Session.SendMessage(message);
                    return;
                }
            }
            #endregion       

            if (Message.StartsWith("@red@") || Message.StartsWith("@blue@") || Message.StartsWith("@cyan@") || Message.StartsWith("@green@") || Message.StartsWith("@purple@") || Message.StartsWith("@normal@"))
            {
                if (Message.StartsWith("@red@")) { 
                    Session.GetHabbo().ChatColor = "@red@";
                    Message = Message.Replace("@red@", "");
                }
                else if (Message.StartsWith("@blue@")) { 
                    Session.GetHabbo().ChatColor = "@blue@";
                    Message = Message.Replace("@blue@", "");
                }
                else if (Message.StartsWith("@cyan@")){
                    Session.GetHabbo().ChatColor = "@cyan@";
                    Message = Message.Replace("@cyan@", "");
                }
                else if (Message.StartsWith("@green@")){
                    Session.GetHabbo().ChatColor = "@green@";
                    Message = Message.Replace("@green@", "");
                }
                else if (Message.StartsWith("@purple@")){
                    Session.GetHabbo().ChatColor = "@purple@";
                    Message = Message.Replace("@purple@", "");
                }
                else if (Message.StartsWith("@normal@")){
                    Session.GetHabbo().ChatColor = "";
                    Message = Message.Replace("@normal@", "");
                }
            }

            #region Commands
            if (Message.StartsWith(":")) // Si el mensaje comienza por :
            {
                if (ChatCommandRegister.IsChatCommand(Message.Split(' ')[0].ToLower().Substring(1))) // si está en nuestra lista de comandos
                {
                    ChatCommandHandler handler = new ChatCommandHandler(Message.Split(' '), Session, mRoom, this);

                    try
                    {
                        if (handler.WasExecuted())
                            return;
                    }
                    finally
                    {
                        handler.Dispose();
                    }
                }
            } else if (Message.StartsWith("@"))
            {
                string nomeFinal = String.Empty;
                var nomeSplitado = Message.Replace("@", "").Split(' ');
                if(nomeSplitado.Length != 0)
                {
                    nomeFinal = Convert.ToString(nomeSplitado[0]);
                }

                GameClient buscaUsuario = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(nomeFinal);
                if (buscaUsuario == null || buscaUsuario.GetHabbo() == null)
                    goto naoMarcar;

                ServerMessage Alert = new ServerMessage(Outgoing.CustomAlert);
                Alert.AppendString("furni_placement_error");
                Alert.AppendInt32(3);
                Alert.AppendString("message");
                Alert.AppendString("O usuário " + Session.GetHabbo().Username + " te marcou em uma conversa, clique aqui para ir ao quarto.");
                Alert.AppendString("image");
                Alert.AppendString("${image.library.url}notifications/" + EmuSettings.EVENTHA_ICON + ".png");
                Alert.AppendString("linkUrl");
                Alert.AppendString("event:navigator/goto/" + Session.GetHabbo().CurrentRoomId);
                buscaUsuario.SendMessage(Alert);

                WhisperComposer("Você marcou o usuário " + buscaUsuario.GetHabbo().Username + " com sucesso.");

            }
            naoMarcar:
            #endregion
            #region Flood
            if (!Session.GetHabbo().HasFuse("ignore_flood_filter") && Session.GetHabbo().Id != GetRoom().RoomData.OwnerId && !IsBot)
            {
                TimeSpan SinceLastMessage = DateTime.Now - Session.GetHabbo().spamFloodTime;
                if (SinceLastMessage.Seconds > 3)
                {
                    FloodCount = 0;
                }
                else if (FloodCount > 5)
                {
                    ServerMessage Packet = new ServerMessage(Outgoing.FloodFilter);
                    Packet.AppendInt32(30);
                    GetClient().SendMessage(Packet);

                    OtanixEnvironment.GetGame().GetMuteManager().AddUserMute(Session.GetHabbo().Id, 0.5);
                    return;
                }
                Session.GetHabbo().spamFloodTime = DateTime.Now;
                FloodCount++;
            }
            #endregion
            #region Filter
            if (!Session.GetHabbo().HasFuse("ignore_spam_filter"))
            {
                if (BlackWordsManager.Check(Message, BlackWordType.Hotel, Session, "<ID do Quarto:" + Session.GetHabbo().CurrentRoomId + ">"))
                    return;

                if (BlackWordsManager.CheckRoomFilter(Message, mRoom.RoomFilterWords))
                    return;
            }
            #endregion

            #region Show Message Progress
            if (Session.GetHabbo().Rank < 2 && EmuSettings.CHAT_TYPES_USERS.Contains(Color))
                Color = 0;

            // if (Session.GetHabbo().GetBadgeComponent().HasBadge(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_BADGE) && Session.GetHabbo().GetBadgeComponent().GetBadge(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_BADGE).Slot > 0 && OtanixEnvironment.GetGame().GetRoomRankConfig().ROOMS_TO_MODIFY.Contains((int)GetRoom().RoomId))
            //     Color = OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR; // si la sala está elegida como sala para bots, mejor que cada bot hable con su tipo de chat, no?

            Unidle();
            OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_CHAT); // miramos el reto

            SpyChatMessage.SaveUserLog(Session.GetHabbo().Id, mRoom.RoomId, 0, Message);
            var Mess = new ChatMessage(Session.GetHabbo().Id, Session.GetHabbo().Username, mRoom.RoomId, Message, DateTime.Now, true); // creamos la clase para el Mensaje
            Session.GetHabbo().GetChatMessageManager().AddMessage(Mess); // Mod Tools: User Message
            mRoom.GetChatMessageManager().AddMessage(Mess); // Mod Tools: Room Message

            OtanixEnvironment.GetGame().CorManager().atualizaPracolorido(Session);

            NoCheckings:
            GetRoom().QueueChatMessage(new InvokedChatMessage(this, Message, Color, Shout));
           
            if (IsBot)
                BotCommands(VirtualId, Message, mRoom);
            #endregion
            #endregion
        }


        private void BotCommands(int VirtualId, string message, Room Room)
        {
            switch(message.ToLower())
            {
                case ":kiss":
                    {
                        var Message = new ServerMessage(Outgoing.Action);
                        Message.AppendInt32(VirtualId);
                        Message.AppendInt32(2);
                        Room.SendMessage(Message);

                        break;
                    }

                case "o/":
                    {
                        var Message = new ServerMessage(Outgoing.Action);
                        Message.AppendInt32(VirtualId);
                        Message.AppendInt32(1);
                        Room.SendMessage(Message);

                        break;
                    }

                case "_b":
                    {
                        var Message = new ServerMessage(Outgoing.Action);
                        Message.AppendInt32(VirtualId);
                        Message.AppendInt32(7);
                        Room.SendMessage(Message);

                        break;
                    }
                case "_enableCustom":
                    {
                        var Message = new ServerMessage(Outgoing.ApplyEffects);
                        Message.AppendInt32(VirtualId);
                        Message.AppendInt32(187);
                        Message.AppendInt32(0);
                        Room.SendMessage(Message);

                        break;
                    }
            }
        }

        internal void WhisperComposer(string Message, int ChatStyle = 34)
        {
            if (Message.Length == 0)
                return;

            ServerMessage ChatMessage = new ServerMessage(Outgoing.Whisp);
            ChatMessage.AppendInt32(VirtualId);
            ChatMessage.AppendString(Message.Replace("<", "¤"));
            ChatMessage.AppendInt32(RoomUser.GetSpeechEmotion(Message));
            ChatMessage.AppendInt32(ChatStyle);
            ChatMessage.AppendInt32(0);
            ChatMessage.AppendInt32(-1);
            GetClient().SendMessage(ChatMessage);
        }

        internal void OnChat(InvokedChatMessage message)
        {
            #region Progress
            var Message = message.message; // mejoramos el mensaje.

            if (GetRoom() == null || GetRoom().SayWired(this, Message)) // si la sala es nula o no se muestra (es wired)
                return;

            RoomChat Chat = new RoomChat(this, message);
            GetRoom().SendChatMessage(Chat);

            // GetRoom().GetRoomUserManager().TurnHeads(X, Y, HabboId);

            if (!IsBot)
            {
                GetRoom().OnUserSay(this, Message, message.shout);
            }

            message.Dispose();
            #endregion
        }

        internal static int GetSpeechEmotion(string Message)
        {
            Message = Message.ToLower();

            if (Message.Contains(":)") || Message.Contains(":d") || Message.Contains("=]") ||
                Message.Contains("=d") || Message.Contains(":>"))
            {
                return 1;
            }

            if (Message.Contains(">:(") || Message.Contains(":@"))
            {
                return 2;
            }

            if (Message.Contains(":o"))
            {
                return 3;
            }

            if (Message.Contains(":(") || Message.Contains("=[") || Message.Contains(":'(") || Message.Contains("='["))
            {
                return 4;
            }

            return 0;
        }

        internal void ClearMovement(bool Update)
        {
            IsWalking = false;
            IsTeleporting = false;
            Statusses.Remove("mv");
            GoalX = 0;
            GoalY = 0;
            SetStep = false;
            SetX = 0;
            SetY = 0;
            SetZ = 0;
            Path.Clear();
            PathRecalcNeeded = false;
            PathStep = 1;
            SeatCount = 0;

            if (Update)
            {
                UpdateNeeded = true;
            }
        }

        internal void MoveTo(Point c)
        {
            MoveTo(c.X, c.Y);
        }

        internal void MoveTo(int pX, int pY)
        {
            // Comando developer activado?
            if (DeveloperCommand.CheckDeveloper(this, pX, pY, GetRoom()))
                return;

            bool guildGateUser = false;
            Gamemap roomGameMap = GetRoom().GetGameMap();

            Point square = new Point(pX, pY);
            if (roomGameMap.guildGates.ContainsKey(square))
            {
                uint GuildId = 0;
                string[] strArr = roomGameMap.guildGates[square].GroupData.Split(';');
                if (strArr.Length < 2)
                    return;

                uint.TryParse(strArr[1], out GuildId);

                if (GuildId > 0)
                {
                    if (!IsBot)
                    {
                        if (GetClient().GetHabbo().MyGroups.Contains(GuildId))
                        {
                            guildGateUser = true;
                        }
                    }
                }
            }

            // Si hay un usuario o un item, evitamos crear un nuevo path.
            if (!GetRoom().GetGameMap().tileIsWalkable(pX, pY, true, false, guildGateUser) && !AllowOverride && walkingToPet == null)
                return;

            if (ByteToItemEffectEnum.Parse(GetRoom().GetGameMap().EffectMap[pX, pY]) == ItemEffectType.HorseJump)
                return;

            if (isKicking)
                return;

            Unidle();

            if (TeleportEnabled)
            {
                if (IsWalking)
                {
                    GetRoom().GetGameMap().RemoveUserFromMap(this, new Point(SetX, SetY));
                    ClearMovement(true);
                }

                GetRoom().SendMessage(GetRoom().GetRoomItemHandler().UpdateUserOnRoller(this, new Point(pX, pY), 0));
                GetRoom().GetRoomUserManager().UpdateUserStatus(this, false);
                return;
            }

            IsWalking = true;
            GoalX = pX;
            GoalY = pY;
            PathRecalcNeeded = true;
            throwBallAtGoal = false;
            // GetRoom().GetRoomUserManager().UpdateUsersPath = true;
        }

        internal void UnlockWalking()
        {
            AllowOverride = false;
            CanWalk = true;
        }

        internal void SetPos(int pX, int pY, double pZ)
        {
            X = pX;
            Y = pY;
            Z = pZ;
            if (isFlying)
                Z += 4 + 0.5 * Math.Sin(0.7 * flyk);

            if (IsPet && PetData != null)
            {
                PetData.X = pX;
                PetData.Y = pY;
            }
        }

        internal void CarryItem(int Item)
        {
            this.CarryItemID = Item;

            if (Item > 0)
            {
                this.CarryTimer = 240;
            }
            else
            {
                this.CarryTimer = 0;
            }

            var Message = new ServerMessage(Outgoing.ApplyCarryItem);
            Message.AppendInt32(VirtualId);
            Message.AppendInt32(Item);
            GetRoom().SendMessage(Message);
        }

        internal void SetRot(int Rotation, bool HeadOnly, bool All = false)
        {
            if (Statusses.ContainsKey("lay") || IsWalking)
            {
                return;
            }

            var diff = RotBody - Rotation;

            RotHead = RotBody;

            if ((Statusses.ContainsKey("sit") && !sentadoBol) || HeadOnly)
            {
                if (RotBody == 2 || RotBody == 4)
                {
                    if (diff > 0)
                    {
                        RotHead = RotBody - 1;
                    }
                    else if (diff < 0)
                    {
                        RotHead = RotBody + 1;
                    }
                }
                else if (RotBody == 0 || RotBody == 6)
                {
                    if (diff > 0)
                    {
                        RotHead = RotBody - 1;
                    }
                    else if (diff < 0)
                    {
                        RotHead = RotBody + 1;
                    }
                }
            }
            else if (diff <= -2 || diff >= 2 || All)
            {
                RotHead = Rotation;
                RotBody = Rotation;
            }
            else
            {
                RotHead = Rotation;
            }

            UpdateNeeded = true;
        }

        internal void SetDiagonalRotation()
        {
            if (RotBody % 2 != 0)
            {
                if (RotBody == 7)
                    RotBody = 0;
                else
                    RotBody++;
            }
        }

        internal void AddStatus(string Key, string Value)
        {
            Statusses[Key] = Value;
        }

        internal void RemoveStatus(string Key)
        {
            if (Statusses.ContainsKey(Key))
            {
                Statusses.Remove(Key);
            }
        }

        internal void ApplyEffect(int effectID)
        {
            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null || GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() == null)
                return;

            GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(effectID);
        }

        internal void Serialize(ServerMessage Message)
        {
            if (Message == null)
                return;

            if (IsSpectator)
                return;

            Message.AppendUInt(HabboId); // Id
            Message.AppendString(GetUsername()); // Name
            Message.AppendString(GetMotto()); // Motto
            Message.AppendString(GetLook()); // Look
            Message.AppendInt32(VirtualId); // VirtualID
            Message.AppendInt32(X); // X
            Message.AppendInt32(Y); // Y
            Message.AppendString(TextHandling.GetString(Z)); // Z
            Message.AppendInt32(RotBody); // Dir / Rot
            Message.AppendInt32(GetGhost()); // Type: (User, Pet, Bot...)
            switch (GetGhost())
            {
                case 1: // human
                    {
                        if (GetClient() == null || GetClient().GetHabbo() == null)
                            return;

                        var User = GetClient().GetHabbo();
                        Message.AppendString(User.Gender); // Gender
           
                        var Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(User.FavoriteGroup);
                        if (Group == null)
                        {
                            User.FavoriteGroup = 0;

                            Message.AppendInt32(-1); // FavGroupId
                            Message.AppendInt32(-1); // non value
                            Message.AppendString("");
                            Message.AppendString(""); // ??
                        }
                        else
                        {
                            Message.AppendUInt(User.FavoriteGroup); // FavGroupId
                            Message.AppendInt32(1); // non value
                            Message.AppendString(Group != null ? Group.Name : "");
                            Message.AppendString(""); // ??
                        }
                        Message.AppendUInt(User.AchievementPoints); // AchPoints
                        Message.AppendBoolean(false); // ??

                        break;
                    }
                case 2: // pet
                    {
                        if(PetData == null && GetClient() != null && GetClient().GetHabbo() != null)
                        {
                            if (GetClient().GetHabbo().ConvertedOnPet)
                            {
                                Message.AppendInt32(GetClient().GetHabbo().PetType);
                                Message.AppendUInt(GetClient().GetHabbo().Id); // userid
                                Message.AppendString(GetClient().GetHabbo().Username); // username
                                Message.AppendInt32(1); // raretyLevel
                                Message.AppendBoolean(false); // HaveSaddle
                                Message.AppendBoolean(false); // HaveUserOn
                                Message.AppendBoolean(false);
                                Message.AppendBoolean(false);
                                Message.AppendBoolean(false);
                                Message.AppendBoolean(false);
                                Message.AppendInt32(0);
                                Message.AppendString("");

                                break;
                            }
                        }

                        Message.AppendUInt(PetData.Type);
                        Message.AppendUInt(PetData.OwnerId); // userid
                        Message.AppendString(PetData.OwnerName); // username
                        Message.AppendInt32(1); // raretyLevel
                        Message.AppendBoolean(PetData.HaveSaddle != 0); // HaveSaddle
                        Message.AppendBoolean(montandoBol); // HaveUserOn
                        Message.AppendBoolean(false);
                        Message.AppendBoolean(false);
                        Message.AppendBoolean(false);
                        Message.AppendBoolean(false);
                        Message.AppendInt32(0);
                        Message.AppendString("");

                        break;
                    }
                case 4: // bot
                    {
                        Message.AppendString(BotData.Gender);
                        Message.AppendUInt(BotData.OwnerId);
                        Message.AppendString(BotData.OwnerName);
                        Message.AppendInt32(5);
                        for (var i = 1; i < 6; i++)
                            Message.AppendShort(i);

                        break;
                    }
            }
        }

        internal void SerializeStatus(ServerMessage Message)
        {
            if (IsSpectator)
            {
                return;
            }

            Message.AppendInt32(VirtualId);
            Message.AppendInt32(X);
            Message.AppendInt32(Y);
            Message.AppendString(TextHandling.GetString(Z));
            Message.AppendInt32(RotHead);
            Message.AppendInt32(RotBody);
            var StatusComposer = new StringBuilder();
            StatusComposer.Append("/");

            foreach (var Status in Statusses)
            {
                StatusComposer.Append(Status.Key);

                if (Status.Value != string.Empty)
                {
                    StatusComposer.Append(" ");
                    StatusComposer.Append(Status.Value);
                }

                StatusComposer.Append("/");
            }

            StatusComposer.Append("/");
            Message.AppendString(StatusComposer.ToString());

            if (Statusses.ContainsKey("sign"))
            {
                RemoveStatus("sign");
                UpdateNeeded = true;
            }
        }

        internal void SerializeStatus(ServerMessage Message, String Status)
        {
            if (IsSpectator)
                return;

            Message.AppendInt32(VirtualId);
            Message.AppendInt32(X);
            Message.AppendInt32(Y);
            Message.AppendString(Z < 0 ? TextHandling.GetString(0) : TextHandling.GetString(Z));
            Message.AppendInt32(RotHead);
            Message.AppendInt32(RotBody);
            Message.AppendString(Status);
        }

        internal ServerMessage SerializeInfo()
        {
            ServerMessage Nfo = new ServerMessage(Outgoing.PetInformation);
            Nfo.AppendUInt(HabboId);
            Nfo.AppendString(GetUsername());
            Nfo.AppendUInt(GetClient().GetHabbo().Rank);
            Nfo.AppendInt32(9);
            Nfo.AppendInt32(100);
            Nfo.AppendInt32(100);
            Nfo.AppendInt32(100);
            Nfo.AppendInt32(100);
            Nfo.AppendInt32(100);
            Nfo.AppendInt32(100);
            Nfo.AppendUInt(GetClient().GetHabbo().Respect);
            Nfo.AppendUInt(HabboId);
            Nfo.AppendInt32((int)(DateTime.Now - GetClient().GetHabbo().Created).TotalDays);
            Nfo.AppendString(GetUsername());
            Nfo.AppendInt32(1);
            Nfo.AppendBoolean(false);
            Nfo.AppendBoolean(false);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendString("");
            Nfo.AppendBoolean(false);
            Nfo.AppendInt32(-1);
            Nfo.AppendInt32(-1);
            Nfo.AppendInt32(-1);
            Nfo.AppendBoolean(false);
            return Nfo;
        }

        private GameClient mClient;

        internal GameClient GetClient()
        {
            if (IsBot)
            {
                return null;
            }

            if (mClient == null)
                mClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(HabboId);

            return mClient;
        }

        private Room mRoom;

        private Room GetRoom()
        {
            if (mRoom == null)
                mRoom = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            return mRoom;
        }
    }

    internal enum ItemEffectType
    {
        None,
        Swim,
        SwimLow,
        SwimHalloween,
        Iceskates,
        Normalskates,
        PublicPool,
        SwimHalloween15,
        HorseJump,
        Snowboard,
        Trampoline,
        Treadmill,
        Crosstrainer
    }

    internal static class ByteToItemEffectEnum
    {
        internal static ItemEffectType Parse(byte pByte)
        {
            switch (pByte)
            {
                case 0:
                    return ItemEffectType.None;
                case 1:
                    return ItemEffectType.Swim;
                case 2:
                    return ItemEffectType.Normalskates;
                case 3:
                    return ItemEffectType.Iceskates;
                case 4:
                    return ItemEffectType.SwimLow;
                case 5:
                    return ItemEffectType.SwimHalloween;
                case 6:
                    return ItemEffectType.PublicPool;
                case 7:
                    return ItemEffectType.SwimHalloween15;
                case 8:
                    return ItemEffectType.HorseJump;
                case 9:
                    return ItemEffectType.Snowboard;
                case 10:
                    return ItemEffectType.Trampoline;
                case 11:
                    return ItemEffectType.Treadmill;
                case 12:
                    return ItemEffectType.Crosstrainer;
                default:
                    return ItemEffectType.None;
            }
        }
    }
}
