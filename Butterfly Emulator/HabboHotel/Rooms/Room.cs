using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Butterfly.Core;
using Butterfly.HabboHotel.Catalogs;
using Butterfly.HabboHotel.ChatMessageStorage;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.RoomBots;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.RoomIvokedItems;
using Butterfly.Messages;
using ButterStorm;
using Uber.HabboHotel.Rooms;
using Butterfly.HabboHotel.Rooms.Wired;
using Butterfly.HabboHotel.SoundMachine;
using HabboEvents;
using Butterfly.HabboHotel.Group;
using Butterfly.HabboHotel.Mutes;
using Butterfly.HabboHotel.Rooms.Polls;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms
{
    public class Room
    {
        internal UInt32 Id;

        internal Boolean RoomMuted;

        internal bool IsRoomLoaded = false;
        internal bool muteSignalEnabled;
        internal bool mCycleEnded;

        internal bool procesoEnCurso = false;
        internal int procesoIntentos = 0;

        internal event RoomUserSaysDelegate OnUserSays;

        internal int IdleTime;
        internal int halfTime = 0;
        internal GameClient ownerSession;

        internal TeamManager gameTeam;

        internal List<uint> UsersWithRights;
        internal Dictionary<UInt32, Double> Bans;
        private Dictionary<UInt32, MuteUser> Mutes;
        private List<string> FilterWords;

        internal bool HeightMapLoaded;

        internal DateTime lastTimerReset;

        private GameManager games;
        private Gamemap gamemap;
        private RoomItemHandling roomItemHandling;
        private RoomUserManager roomUserManager;
        private Soccer soccer;
        private GameItemHandler gameItemHandler;
        private WiredHandler wiredHandler;
        private RoomMusicController musicController;
        private IRoomPolls roomPoll;

        internal Queue roomMessages;
        internal Queue roomAlerts;
        internal Queue roomBadge;
        internal Queue roomDiamonds;
        internal Queue roomCredits;
        internal Queue roomPiruletas;
        internal Queue roomKick;
        internal Queue roomServerMessages;
        internal Queue roomChatServerMessages;

        internal Queue groupRemoveQueue;
        internal Queue groupAddQueue;
        internal Dictionary<uint, int> groupsOnRoom;

        internal MoodlightData MoodlightData;

        private ChatMessageManager chatMessageManager;
        internal Queue chatMessageQueue;

        internal Dictionary<uint, uint> roomUserFloorItems;
        internal Dictionary<uint, uint> roomUserWallItems;

        internal Gamemap GetGameMap()
        {
            return gamemap;
        }

        internal void FixGameMap()
        {
            gamemap = new Gamemap(this);
           // GetGameMap().GenerateMaps();
        }

        internal RoomItemHandling GetRoomItemHandler()
        {
            return roomItemHandling;
        }

        internal RoomUserManager GetRoomUserManager()
        {
            return roomUserManager;
        }

        internal Soccer GetSoccer()
        {
            if (soccer == null)
                soccer = new Soccer(this);
            return soccer;
        }

        internal TeamManager GetTeamManager()
        {
            if (gameTeam == null)
                gameTeam = new TeamManager();

            return gameTeam;
        }

        internal GameManager GetGameManager()
        {
            return games;
        }

        internal GameItemHandler GetGameItemHandler()
        {
            if (gameItemHandler == null)
                gameItemHandler = new GameItemHandler(this);
            return gameItemHandler;
        }

        internal RoomMusicController GetRoomMusicController()
        {
            if (musicController == null)
                musicController = new RoomMusicController();
            return musicController;
        }

        internal WiredHandler GetWiredHandler()
        {
            if (wiredHandler == null)
                wiredHandler = new WiredHandler(this);
            return wiredHandler;
        }

        internal IRoomPolls GetRoomPoll()
        {
            return roomPoll;
        }

        internal bool GotRoomPoll()
        {
            return roomPoll != null;
        }

        internal bool GotMusicController()
        {
            return (musicController != null);
        }

        internal bool GotSoccer()
        {
            return (soccer != null);
        }

        internal bool GotWired()
        {
            return (wiredHandler != null);
        }

        internal Boolean HasOngoingEvent
        {
            get
            {
                if (RoomData.Event != null)
                {
                    return true;
                }

                return false;
            }
        }

        internal Int32 UserCount
        {
            get
            {
                if (roomUserManager == null)
                    return 0;

                return roomUserManager.GetRoomUserCount();
            }
        }

        internal uint RoomId
        {
            get
            {
                return Id;
            }
        }

        internal bool CanTradeInRoom(GameClient Session)
        {
            if (RoomData.TradeSettings == 0)
            {
                return false;
            }
            else if (RoomData.TradeSettings == 1)
            {
                return CheckRights(Session);
            }

            return true;
        }

        internal bool IsPublic
        {
            get
            {
                return (RoomData.Type == "public");
            }
        }

        private RoomData mRoomData;
        internal RoomData RoomData
        {
            get
            {
                return mRoomData;
            }
        }

        internal ChatMessageManager GetChatMessageManager()
        {
            return chatMessageManager;
        }

        internal Room(RoomData Data)
        {
            Initialize(Data);
        }

        private void Initialize(RoomData RoomData)
        {
            this.Id = RoomData.Id;

            this.mDisposed = false;
            this.RoomMuted = false;
            this.muteSignalEnabled = false;

            this.Bans = new Dictionary<UInt32, double>();
            this.Mutes = new Dictionary<UInt32, MuteUser>();
            this.FilterWords = new List<string>();
            this.chatMessageManager = new ChatMessageManager();
            this.lastTimerReset = DateTime.Now;
            this.IsRoomLoaded = false;

            this.mCycleEnded = false;
            this.HeightMapLoaded = false;

            this.mRoomData = RoomData;

            this.roomMessages = new Queue();
            this.chatMessageQueue = new Queue();

            this.roomMessages = new Queue();
            this.roomAlerts = new Queue();
            this.roomBadge = new Queue();
            this.roomDiamonds = new Queue();
            this.roomCredits = new Queue();
            this.roomPiruletas = new Queue();
            this.roomKick = new Queue();
            this.roomServerMessages = new Queue();
            this.roomChatServerMessages = new Queue();

            this.groupAddQueue = new Queue();
            this.groupRemoveQueue = new Queue();
            this.groupsOnRoom = new Dictionary<uint, int>();

            this.roomUserFloorItems = new Dictionary<uint, uint>();
            this.roomUserWallItems = new Dictionary<uint, uint>();

            this.gamemap = new Gamemap(this);
            this.roomUserManager = new RoomUserManager(this);
            this.roomItemHandling = new RoomItemHandling(this);
            this.wiredHandler = new WiredHandler(this);
            this.games = new GameManager(this);

            this.roomPoll = new RoomQuestionary();
            if (!this.roomPoll.LoadQuestionary(this.Id))
                this.roomPoll = null;

            this.LoadFilterWords();
            this.LoadRights();
            this.GetRoomItemHandler().LoadFurniture();
            this.GetGameMap().GenerateMaps();
            this.GetRoomUserManager().OnUserUpdateStatus(); // Update Bots State.
            this.LoadMusic();

            if (this.RoomData.State != 3)
                OtanixEnvironment.GetGame().GetRoomManager().QueueActiveRoomAdd(mRoomData);
        }

        internal void CreateNewQuestionary(PollType type)
        {
            if (type == PollType.ROOM_QUESTIONARY)
                this.roomPoll = new RoomQuestionary();
            else if (type == PollType.VOTE_QUESTIONARY)
                this.roomPoll = new VoteQuestionary();
        }

        internal bool SayWired(RoomUser user, string message)
        {
            bool handled = false;
            if (OnUserSays != null)
            {
                foreach (Delegate d in OnUserSays.GetInvocationList())
                {
                    if ((bool)d.DynamicInvoke(null, new UserSaysArgs(user, message)))
                    {
                        if (!handled)
                        {
                            if (user.FloodCount > 0)
                                user.FloodCount--;

                            handled = true;
                        }
                    }
                }
            }
            return handled;
        }

        internal void InitPets()
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id, user_id, room_id, name, type, race, color, expirience, energy, nutrition, respect, createstamp, x, y, z, all_can_mount, have_saddle, hairdye, pethair, accessories FROM user_pets WHERE room_id = " + RoomId);
                var Data = dbClient.getTable();

                if (Data == null)
                    return;

                foreach (DataRow Row in Data.Rows)
                {
                    var Pet = Catalog.GeneratePetFromRow(Row);
                    roomUserManager.DeployBot(new RoomBot(Pet.PetId, Pet.OwnerId, RoomId, AIType.Pet, true, Pet.Name, "", "", Pet.Look, Pet.X, Pet.Y, (int)Pet.Z, 0, false, "", 0, false), Pet);
                }
            }
        }

        internal void QueueAddGroupUser(uint group)
        {
            lock (groupAddQueue.SyncRoot)
            {
                groupAddQueue.Enqueue(group);
            }
        }

        internal void QueueRemoveGroupUser(uint group)
        {
            lock (groupRemoveQueue.SyncRoot)
            {
                groupRemoveQueue.Enqueue(group);
            }
        }

        internal void QueueChatMessage(InvokedChatMessage message)
        {
            lock (chatMessageQueue.SyncRoot)
            {
                chatMessageQueue.Enqueue(message);
            }
        }

        internal void QueueRoomKick(RoomKick kick)
        {
            lock (roomKick.SyncRoot)
            {
                roomKick.Enqueue(kick);
            }
        }

        internal void QueueRoomBadge(string badge)
        {
            lock (roomBadge.SyncRoot)
            {
                roomBadge.Enqueue(badge);
            }
        }

        internal void QueueRoomDiamonds(int diamonds)
        {
            lock (roomDiamonds.SyncRoot)
            {
                roomDiamonds.Enqueue(diamonds);
            }
        }

        internal void QueueRoomCredits(int diamonds)
        {
            lock (roomDiamonds.SyncRoot)
            {
                roomCredits.Enqueue(diamonds);
            }
        }
        internal void QueueRoomPiruletas(int diamonds)
        {
            lock (roomPiruletas.SyncRoot)
            {
                roomPiruletas.Enqueue(diamonds);
            }
        }
        internal void onRoomKick()
        {
            var ToRemove = new List<RoomUser>();

            foreach (var user in roomUserManager.UserList.Values)
            {
                if (!user.IsBot && user.GetClient().GetHabbo().Rank < 4)
                    ToRemove.Add(user);
            }

            for (var i = 0; i < ToRemove.Count; i++)
            {
                GetRoomUserManager().RemoveUserFromRoom(ToRemove[i].GetClient(), true, true);
            }
        }

        internal void OnUserSay(RoomUser User, string Message, bool Shout)
        {
            foreach (var user in roomUserManager.UserList.Values)
            {
                if (user == null || !user.IsBot)
                    continue;

                if (Shout)
                    user.BotAI.OnUserShout(User, Message);
                else
                    user.BotAI.OnUserSay(User, Message);
            }
        }

        internal void LoadMusic()
        {
            DataTable dTable;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM items_rooms_songs WHERE roomid = '" + RoomId + "'");
                dTable = dbClient.getTable();
            }

            int songID;
            uint itemID;
            int baseID;

            foreach (DataRow dRow in dTable.Rows)
            {
                itemID = (uint)dRow["itemid"];
                songID = (int)dRow["songid"];
                baseID = EmuSettings.JUKEBOX_CD_BASEID;

                var item = new SongItem((int)itemID, songID, baseID);
                GetRoomMusicController().AddDisk(item);
            }
        }

        internal void LoadRights()
        {
            UsersWithRights = new List<uint>();

            var Data = new DataTable();

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT room_rights.user_id FROM room_rights WHERE room_id = " + Id);
                Data = dbClient.getTable();
            }

            if (Data == null)
                return;

            foreach (DataRow Row in Data.Rows)
            {
                UsersWithRights.Add(Convert.ToUInt32(Row["user_id"]));
            }
        }

        internal void LoadFilterWords()
        {
            FilterWords = new List<string>();

            var Data = new DataTable();

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT word FROM room_filter WHERE room_id = " + Id);
                Data = dbClient.getTable();
            }

            if (Data == null)
                return;

            foreach (DataRow Row in Data.Rows)
            {
                FilterWords.Add((string)(Row["word"]));
            }
        }

        internal int GetRightsLevel(GameClient Session)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return 0;
            }

            if (Session.GetHabbo().Id == RoomData.OwnerId || Session.GetHabbo().HasFuse("fuse_any_room_rights"))
            {
                return 4;
            }

            GroupItem group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(RoomData.GroupId);
            if (group != null) // si la sala contiene un grupo
            {
                if (group.Admins.ContainsKey(Session.GetHabbo().Id))
                    return 3;
            }

            if (UsersWithRights.Contains(Session.GetHabbo().Id))
                return 1;

            if (RoomData.AllowRightsOverride)
                return 1;

            return 0;
        }

        internal Boolean CheckRights(GameClient Session)
        {
            return CheckRights(Session, false);
        }

        internal Boolean CheckRights(GameClient Session, bool RequireOwnership)
        {
            try
            {
                if (Session == null || Session.GetHabbo() == null)
                    return false;

                if (Session.GetHabbo().Id == RoomData.OwnerId)
                {
                    return true;
                }

                if (Session.GetHabbo().HasFuse("fuse_admin") || Session.GetHabbo().HasFuse("fuse_any_room_rights"))
                {
                    return true;
                }

                if (!RequireOwnership)
                {
                    GroupItem group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(RoomData.GroupId);
                    if (group != null) // si la sala contiene un grupo
                    {
                        if (group.RightsType == 0) // todos los miembros pueden mover
                        {
                            if (Session.GetHabbo().MyGroups.Contains(RoomData.GroupId)) // soy miembro.
                            {
                                return true;
                            }
                        }
                        else if (group.RightsType == 1) // solo los administradores pueden mover
                        {
                            if (group.Admins.ContainsKey(Session.GetHabbo().Id)) // soy administrador
                            {
                                return true;
                            }
                        }
                    }

                    if (UsersWithRights.Contains(Session.GetHabbo().Id))
                        return true;
                    if (RoomData.AllowRightsOverride)
                        return true;
                }
            }
            catch (Exception e) { Logging.HandleException(e, "Room.CheckRights"); }

            return false;
        }

        internal bool isCrashed = false;
        internal void OnRoomCrash(Exception e)
        {
            Logging.LogThreadException(e.ToString(), "Room cycle task for room " + RoomId);

            try
            {
                if (Debugger.IsAttached)
                {
                    foreach (var user in roomUserManager.UserList.Values)
                    {
                        if (user.IsBot || user.IsPet)
                            continue;
                        user.GetClient().SendNotif("Unhandled exception in room: " + e);
                        try
                        {
                            GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true, false);
                        }
                        catch
                        { }
                    }
                }
            }
            catch { }

            OtanixEnvironment.GetGame().GetRoomManager().UnloadRoom(this);

            isCrashed = true;
        }

        private bool ChatCheck(RoomUser user, RoomUser parent)
        {
            // Si el receptor es un bot o no está conectado.
            if (user == null || user.IsBot || user.GetClient() == null || user.GetClient().GetHabbo() == null || parent == null)
                return false;

            // Si habla una mascota y estamos ignorando a las mascotas.
            if (parent.IsPet && user.IgnorePets)
                return false;

            // Si habla un bot y estamos ignorando a los bots.
            if (parent.IsBot && user.IgnoreBots)
                return false;

            // Si es un usuario:
            if (!parent.IsBot)
            {
                // Comprobamos que esté conectado.
                if (parent.GetClient() == null || parent.GetClient().GetHabbo() == null)
                    return false;

                // Si es un mod, escuchamos.
                if (parent.GetClient().GetHabbo().HasFuse("fuse_mod"))
                    return true;

                // Si lo tiene ignorado.
                if (user.GetClient().GetHabbo().MutedUsers.Contains(parent.GetClient().GetHabbo().Username))
                    return false;

                // Si está más alejados de una cierta distancia.
                if (CoordinationUtil.GetDistance(user.Coordinate, parent.Coordinate) > RoomData.ChatDistance)
                    return false;

                // Si el que habla está dentro de una tienda de campaña.
                if (parent.tentId > 0)
                {
                    // El que recibe también está dentro?.
                    if (parent.tentId != user.tentId)
                        return false;
                }
            }

            // Si el usuario no quiere recibir mensajes.
            if (user.IgnoreChat)
                return false;

            return true;
        }

        internal void ProcessRoom(object pCallback)
        {
            try
            {
                procesoEnCurso = true;
                if (isCrashed || mDisposed)
                    return;

                try
                {
                    int idle = 0;
                    GetRoomItemHandler().OnCycle();
                    GetRoomUserManager().OnCycle(ref idle);

                    if (musicController != null)
                        musicController.Update(this);

                    if (idle > 0)
                    {
                        IdleTime++;
                    }
                    else
                    {
                        IdleTime = 0;
                    }

                    if (!mCycleEnded)
                    {
                        if (IdleTime >= 10)
                        {
                            OtanixEnvironment.GetGame().GetRoomManager().UnloadRoom(this);
                            return;
                        }
                        else
                        {
                            ServerMessage Updates = GetRoomUserManager().SerializeStatusUpdates(false);

                            if (Updates != null)
                                SendMessage(Updates);
                        }
                    }

                    if (GetGameItemHandler() != null)
                        GetGameItemHandler().OnCycle();

                    if (GetWiredHandler() != null)
                        GetWiredHandler().OnCycle();

                    WorkRoomBadgeQueue();
                    WorkRoomDiamondsQueue();
                    WorkRoomCreditsQueue();
                    WorkRoomPiruletasQueue();
                    WorkRoomKickQueue();
                    WorkChatQueue();
                    WorkGroupQueue();
                    WorkRemoveGroupQueue();
                    WorkRoomServerMessageThread();
                    WorkRoomChatServerMessageThread();

                    halfTime++;
                    if (halfTime == 120)
                    {
                        if (UserCount > 1 || (UserCount == 1 && GetRoomUserManager().getFirstUserOnRoom().HabboId != RoomData.OwnerId))
                        {
                            if (ownerSession == null)
                                ownerSession = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(RoomData.OwnerId);

                            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(RoomData.OwnerId, "ACH_RoomDecoHosting", 1);
                        }
                        halfTime = 0;
                    }
                }
                catch (Exception e)
                {
                    OnRoomCrash(e);
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Sub crash in room cycle: " + e);
            }
            finally
            {
                procesoEnCurso = false;
            }
        }

        #region Communication
        internal void SendFastMessage(ServerMessage Message)
        {
            try
            {
                lock (this.GetRoomUserManager().UserList)
                {
                    foreach (RoomUser user in this.GetRoomUserManager().UserList.Values)
                    {
                        if (user.IsBot)
                            continue;

                        GameClient UsersClient = user.GetClient();
                        if (UsersClient == null)
                            continue;

                        UsersClient.SendMessage(Message);
                    }
                }
            }
            catch { }
        }

        internal void SendMessage(ServerMessage Message)
        {
            try
            {
                if (Message == null)
                    return;

                byte[] PacketData = Message.GetBytes();

                lock (roomServerMessages.SyncRoot)
                {
                    roomServerMessages.Enqueue(PacketData);
                }
            }
            catch (Exception e) { Logging.LogException("Room.SendMessage: " + e.ToString()); }
        }

        internal void SendChatMessage(RoomChat Chat)
        {
            if (Chat == null)
                return;

            lock (roomChatServerMessages.SyncRoot)
            {
                roomChatServerMessages.Enqueue(Chat);
            }
        }

        internal void SendMessage(List<ServerMessage> Messages)
        {
            if (Messages.Count == 0)
                return;

            try
            {
                byte[] totalBytes = new byte[0];
                int currentWorking = 0;

                foreach (ServerMessage Message in Messages)
                {
                    byte[] toAppend = Message.GetBytes();
                    int newLength = totalBytes.Length + toAppend.Length;

                    Array.Resize(ref totalBytes, newLength);
                    for (int i = 0; i < toAppend.Length; i++)
                    {
                        totalBytes[currentWorking] = toAppend[i];
                        currentWorking++;
                    }
                }

                lock (roomServerMessages.SyncRoot)
                {
                    roomServerMessages.Enqueue(totalBytes);
                }
            }
            catch (Exception e) { Logging.HandleException(e, "Room.SendMessage List<ServerMessage>"); }
        }

        internal void SendMessageToUsersWithRights(ServerMessage Message, uint ExceptUserId = 0)
        {
            try
            {
                byte[] PacketData = Message.GetBytes();

                foreach (RoomUser user in roomUserManager.UserList.Values)
                {
                    if (user.IsBot)
                        continue;

                    GameClient UsersClient = user.GetClient();
                    if (UsersClient == null)
                        continue;

                    if (!CheckRights(UsersClient))
                        continue;

                    if (user.HabboId == ExceptUserId)
                        continue;

                    UsersClient.GetConnection().SendData(PacketData);
                }
            }
            catch{ }
        }

        internal void SendMessageToUsersWithSUPERRights(ServerMessage Message)
        {
            try
            {
                var PacketData = Message.GetBytes();

                foreach (var user in roomUserManager.UserList.Values)
                {
                    if (user.IsBot)
                        continue;

                    var UsersClient = user.GetClient();
                    if (UsersClient == null)
                        continue;

                    if (!CheckRights(UsersClient, true))
                        continue;

                    try
                    {
                        UsersClient.GetConnection().SendData(PacketData);
                    }
                    catch (Exception e) { Logging.HandleException(e, "Room.SendMessageToUsersWithSUPERRights"); }
                    //User.GetClient().SendMessage(Message);

                }
            }
            catch /*(Exception e)*/ { /*Logging.HandleException(e, "Room.SendMessageToUsersWithSUPERRights");*/ }
        }
        #endregion

        internal void Destroy()
        {
            SendMessage(new ServerMessage(Outgoing.OutOfRoom));
            Dispose();
        }

        internal bool mDisposed;

        #region IDisposable members

        private void Dispose()
        {
            if (!mDisposed)
            {
                try
                {
                    mDisposed = true;
                    mCycleEnded = true;
                    OtanixEnvironment.GetGame().GetRoomManager().QueueActiveRoomRemove(mRoomData);

                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        GetRoomItemHandler().SaveFurniture(dbClient);
                        saveBots();
                    }

                    WorkRoomServerMessageThread();
                    WorkRoomChatServerMessageThread();

                    ClearMute();
                    UsersWithRights.Clear();
                    Bans.Clear();

                    foreach (var item in GetRoomItemHandler().mFloorItems.Values)
                    {
                        item.Destroy();
                    }

                    foreach (var item in GetRoomItemHandler().mWallItems.Values)
                    {
                        item.Destroy();
                    }

                    roomUserManager.Destroy();
                    roomUserManager = null;

                    chatMessageManager.Destroy();
                    chatMessageManager = null;

                    roomItemHandling.Destroy();
                    roomItemHandling = null;

                    if (GetGameManager() != null)
                    {
                        GetGameManager().Destroy();
                        games = null;
                    }

                    if (GotSoccer())
                    {
                        GetSoccer().Destroy();
                        soccer = null;
                    }

                    if (GotWired())
                    {
                        GetWiredHandler().Destroy();
                        wiredHandler = null;
                    }

                    if (GotMusicController())
                    {
                        GetRoomMusicController().Destroy();
                        musicController = null;
                    }

                    if (GetGameMap() != null)
                    {
                        GetGameMap().Destroy();
                        gamemap = null;
                    }
                }
                catch (Exception e)
                {
                    Logging.LogCriticalException("Unload of room " + e);
                }
            }
        }
        #endregion

        #region Room Bans
        internal Boolean UserIsBanned(uint pId)
        {
            return Bans.ContainsKey(pId);
        }

        internal void RemoveBan(uint pId)
        {
            Bans.Remove(pId);
        }

        internal void AddBan(uint pId, int Time)
        {
            if (!Bans.ContainsKey(pId))
                Bans.Add(pId, OtanixEnvironment.GetUnixTimestamp() + Time);
        }

        internal Boolean HasBanExpired(uint pId)
        {
            if (!UserIsBanned(pId))
                return true;

            var diff = Bans[pId] - OtanixEnvironment.GetUnixTimestamp();

            if (diff > 0)
                return false;

            return true;
        }

        internal void RefreshBans()
        {
            List<uint> pExpired = new List<uint>();

            foreach (KeyValuePair<uint, double> bb in Bans)
            {
                if (bb.Value <= OtanixEnvironment.GetUnixTimestamp())
                    pExpired.Add(bb.Key);
            }

            foreach (uint userId in pExpired)
            {
                RemoveBan(userId);
            }

            pExpired.Clear();
            pExpired = null;
        }
        #endregion

        #region Filter Words
        internal Boolean WordExist(string Word)
        {
            return FilterWords.Contains(Word);
        }

        internal void AddFilterWord(string Word)
        {
            if (!FilterWords.Contains(Word))
                FilterWords.Add(Word);
        }

        internal void RemoveFilterWord(string Word)
        {
            if (FilterWords.Contains(Word))
                FilterWords.Remove(Word);
        }

        internal List<string> RoomFilterWords
        {
            get
            {
                return FilterWords;
            }
        }
        #endregion

        #region Room Mutes
        internal Boolean UserIsMuted(uint pId)
        {
            return Mutes.ContainsKey(pId);
        }

        internal void AddMute(uint pId, int Time)
        {
            if (!Mutes.ContainsKey(pId))
                Mutes.Add(pId, new MuteUser(pId, OtanixEnvironment.GetUnixTimestamp() + (Time * 60)));
        }

        internal void RemoveMute(uint pId)
        {
            if(Mutes.ContainsKey(pId))
                Mutes.Remove(pId);
        }

        internal int HasMuteExpired(uint pId)
        {
            if (!UserIsMuted(pId))
            {
                return 0;
            }
            else if (OtanixEnvironment.GetUnixTimestamp() >= Mutes[pId].ExpireTime)
            {
                RemoveMute(pId);
                return -1;
            }

            return (int)(Mutes[pId].ExpireTime - OtanixEnvironment.GetUnixTimestamp());
        }

        internal void ClearMute()
        {
            Mutes.Clear();
        }
        #endregion

        #region Trading
        internal bool HasActiveTrade(RoomUser User)
        {
            if (User.IsBot)
                return false;

            return HasActiveTrade(User.GetClient().GetHabbo().Id);
        }

        internal bool HasActiveTrade(uint UserId)
        {
            if(Trade.tradeMap.ContainsKey(UserId))
                return true;

            return false;
        }

        internal void TryStartTrade(GameClient UserOne, GameClient UserTwo)
        {
            try
            {
                if (!CanTradeInRoom(UserOne))
                {
                    UserOne.SendMessage(Trade.messageTradeError(6, UserTwo.GetHabbo().Username));
                    return;
                }

                var avatar = GetRoomUserManager().GetRoomUserByHabbo(UserOne.GetHabbo().Id);
                if (avatar == null)
                    return;

                if (Trade.tradeMap.ContainsKey(UserOne.GetHabbo().Id))
                {
                    UserOne.SendMessage(Trade.messageTradeError(7, ""));
                    return;
                }

                var User = GetRoomUserManager().GetRoomUserByHabbo(UserTwo.GetHabbo().Id);
                if (User == null)
                    return;

                if (UserOne.GetHabbo().BlockTrade)
                {
                    UserOne.SendMessage(Trade.messageTradeError(2, UserOne.GetHabbo().Username));
                    return;
                }

                if (UserTwo.GetHabbo().BlockTrade)
                {
                    UserOne.SendMessage(Trade.messageTradeError(4, UserTwo.GetHabbo().Username));
                    return;
                }

                if (Trade.tradeMap.ContainsKey(UserTwo.GetHabbo().Id))
                {
                    UserOne.SendMessage(Trade.messageTradeError(8, UserTwo.GetHabbo().Username));
                    return;
                }

                avatar.AddStatus("trd", "");
                avatar.UpdateNeeded = true;

                User.AddStatus("trd", "");
                User.UpdateNeeded = true;

                var trade = new Trade(UserOne, UserTwo);

                var Message = new ServerMessage(Outgoing.TradeStart);
                Message.AppendUInt(UserOne.GetHabbo().Id);
                Message.AppendInt32(1); // ready
                Message.AppendUInt(UserTwo.GetHabbo().Id);
                Message.AppendInt32(1); // ready
                trade.SendMessageToUsers(Message);
            }
            catch
            {
                UserOne.SendMessage(Trade.messageTradeError(3, ""));
                return;
            }
        }

        internal void TryStopTrade(uint UserId)
        {
            if (Trade.tradeMap.ContainsKey(UserId))
            {
                var trade = Trade.tradeMap[UserId];
                if (trade == null)
                    return;

                trade.CloseTrade(UserId);
            }
        }
        #endregion

        internal void SetMaxUsers(uint MaxUsers)
        {
            RoomData.UsersMax = MaxUsers;
            RoomData.roomNeedSqlUpdate = true;
        }

        #region procesado
        internal void WorkGroupQueue()
        {
            if (groupAddQueue.Count > 0)
            {
                lock (groupAddQueue.SyncRoot)
                {
                    while (groupAddQueue.Count > 0)
                    {
                        var group = (uint)groupAddQueue.Dequeue();
                        if (groupsOnRoom.ContainsKey(group))
                            groupsOnRoom[group]++;
                        else
                            groupsOnRoom.Add(group, 1);
                    }
                }
            }
        }

        internal void WorkRemoveGroupQueue()
        {
            if (groupRemoveQueue.Count > 0)
            {
                lock (groupRemoveQueue.SyncRoot)
                {
                    while (groupRemoveQueue.Count > 0)
                    {
                        var group = (uint)groupRemoveQueue.Dequeue();
                        if (groupsOnRoom.ContainsKey(group))
                        {
                            if (groupsOnRoom[group] > 1)
                                groupsOnRoom[group]--;
                            else
                                groupsOnRoom.Remove(group);
                        }
                    }
                }
            }
        }

        internal void WorkRoomServerMessageThread()
        {
            if (roomServerMessages.Count > 0)
            {
                var totalBytes = new List<byte>();

                lock (roomServerMessages.SyncRoot)
                {
                    while (roomServerMessages.Count > 0)
                    {
                        totalBytes.AddRange((byte[])roomServerMessages.Dequeue());
                    }
                }

                try
                {
                    lock (GetRoomUserManager().UserList)
                    {
                        foreach (var user in GetRoomUserManager().UserList.Values)
                        {
                            if (user.IsBot)
                                continue;

                            var UsersClient = user.GetClient();
                            if (UsersClient == null || UsersClient.GetConnection() == null)
                                continue;

                            UsersClient.GetConnection().SendData(totalBytes.ToArray());
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.LogCriticalException("Crash in Room Packet Distribution of room " + Id + ":" + e);
                }

                totalBytes.Clear();
                totalBytes = null;
            }
        }

        internal void WorkRoomChatServerMessageThread()
        {
            if (roomChatServerMessages.Count > 0)
            {
                lock (roomChatServerMessages.SyncRoot)
                {
                    while (roomChatServerMessages.Count > 0)
                    {
                        RoomChat Chat = (RoomChat)roomChatServerMessages.Dequeue();

                        lock (GetRoomUserManager().UserList)
                        {
                            foreach (var user in GetRoomUserManager().UserList.Values)
                            {
                                if (!ChatCheck(user, Chat.GetParent()))
                                    continue;

                                user.GetClient().SendMessage(Chat.GenerateMessage(user.GetClient().GetHabbo().preferOldChat));
                                OtanixEnvironment.GetGame().CorManager().atualizaNomePraNormal(user.GetClient());
                            }
                        }
                    }
                }
            }
        }       

        internal void WorkChatQueue()
        {
            if (chatMessageQueue.Count > 0)
            {
                lock (chatMessageQueue.SyncRoot)
                {
                    while (chatMessageQueue.Count > 0)
                    {
                        var message = (InvokedChatMessage)chatMessageQueue.Dequeue();
                        message.user.OnChat(message);
                    }
                }
            }
        }

        internal void WorkRoomKickQueue()
        {
            if (roomKick.Count > 0)
            {
                lock (roomKick.SyncRoot)
                {
                    while (roomKick.Count > 0)
                    {
                        var kick = (RoomKick)roomKick.Dequeue();


                        var roomUsersToRemove = new List<RoomUser>();
                        foreach (var RoomUser in GetRoomUserManager().UserList.Values)
                        {
                            if (RoomUser.IsBot)
                                continue;
                            if (RoomUser.GetClient().GetHabbo().Rank >= kick.minrank)
                                continue;
                            if (kick.allert.Length > 0)
                                RoomUser.GetClient().SendNotif(LanguageLocale.GetValue("roomkick.allert") + "\n\r" + kick.allert);

                            roomUsersToRemove.Add(RoomUser);

                        }

                        foreach (var user in roomUsersToRemove)
                        {
                            GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true, false);
                        }
                    }
                }
            }
        }

        internal void WorkRoomBadgeQueue()
        {
            if (roomBadge.Count > 0)
            {
                lock (roomBadge.SyncRoot)
                {
                    while (roomBadge.Count > 0)
                    {
                        var badge = (string)roomBadge.Dequeue();

                        foreach (var user in GetRoomUserManager().UserList.Values)
                        {
                            try
                            {
                                if (!user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null)
                                    user.GetClient().GetHabbo().GetBadgeComponent().GiveBadge(badge);
                            }
                            catch //(Exception e)
                            {
                                //Session.SendNotif(LanguageLocale.GetValue("roombadge.error") + e.ToString());
                            }
                        }
                    }
                }
            }
        }

        internal void WorkRoomDiamondsQueue()
        {
            if (roomDiamonds.Count > 0)
            {
                lock (roomDiamonds.SyncRoot)
                {
                    while (roomDiamonds.Count > 0)
                    {
                        int diamonds = (int)roomDiamonds.Dequeue();

                        foreach (RoomUser user in GetRoomUserManager().UserList.Values)
                        {
                            try
                            {
                                if (!user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null)
                                {
                                    user.GetClient().GetHabbo().GiveUserDiamonds(diamonds);
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }
        }

        internal void WorkRoomCreditsQueue()
        {
            if (roomCredits.Count > 0)
            {
                lock (roomCredits.SyncRoot)
                {
                    while (roomCredits.Count > 0)
                    {
                        int diamonds = (int)roomCredits.Dequeue();

                        foreach (RoomUser user in GetRoomUserManager().UserList.Values)
                        {
                            try
                            {
                                if (!user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null)
                                {
                                    user.GetClient().GetHabbo().darMoedas(diamonds);
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }
        }
        #endregion
        internal void WorkRoomPiruletasQueue()
        {
            if (roomPiruletas.Count > 0)
            {
                lock (roomPiruletas.SyncRoot)
                {
                    while (roomPiruletas.Count > 0)
                    {
                        int diamonds = (int)roomPiruletas.Dequeue();

                        foreach (RoomUser user in GetRoomUserManager().UserList.Values)
                        {
                            try
                            {
                                if (!user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null)
                                {
                                    user.GetClient().GetHabbo().GiveUserPiruleta(diamonds);
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }
        }
        internal void saveBots()
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                foreach (var Bot in roomUserManager.GetBots)
                {
                    if (Bot.needSqlUpdate && Bot.BotData.RoomId == Id)
                    {
                        dbClient.setQuery("UPDATE bots SET name = @botname, is_dancing = '" + ((Bot.BotData.IsDancing) ? "1" : "0") + "', walk_enabled = '" + ((Bot.BotData.WalkingEnabled) ? "1" : "0") + "', chat_enabled = '" + ((Bot.BotData.ChatEnabled) ? "1" : "0") + "', chat_text = @chttext, chat_seconds = '" + Bot.BotData.ChatSeconds + "', look = @look, gender = @gender, x = " + Bot.X + ", y = " + Bot.Y + ", rotation = " + Bot.RotBody + " WHERE id = " + Bot.BotData.BotId);
                        dbClient.addParameter("look", Bot.BotData.Look);
                        dbClient.addParameter("gender", Bot.BotData.Gender);
                        dbClient.addParameter("chttext", Bot.BotData.ChatText);
                        dbClient.addParameter("botname", Bot.BotData.Name);
                        dbClient.runQuery();

                        Bot.needSqlUpdate = false;
                    }
                }
            }
        }
    }
}
