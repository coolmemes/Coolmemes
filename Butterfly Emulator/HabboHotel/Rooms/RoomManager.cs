using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Users;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Events;
using System.Collections.Specialized;
using System.Threading;

namespace Butterfly.HabboHotel.Rooms
{
    class RoomManager
    {
        #region Fields
        internal Dictionary<uint, Room> loadedRooms;
        private List<Room> loadedBallRooms;

        private readonly Queue roomsToAddQueue;
        private readonly Queue roomsToRemoveQueue;
        private readonly Queue roomDataToAddQueue;

        private readonly Queue votedRoomsAddQueue;
        private readonly Queue votedRoomsRemoveQueue;

        private Queue ballRoomsAddQueue;
        private Queue ballRoomsRemoveQueue;

        private readonly Queue activeRoomsAddQueue;
        private readonly Queue activeRoomsRemoveQueue;

        private readonly Hashtable roomModels;
        internal Dictionary<uint, RoomData> loadedRoomData;

        internal Dictionary<RoomData, int> votedRooms;
        private IEnumerable<KeyValuePair<RoomData, int>> orderedVotedRooms;

        private readonly List<RoomData> activeRooms;
        private IEnumerable<RoomData> orderedActiveRooms;

        private readonly List<Room> mToRemove;

        private EventManager eventManager;
        #endregion

        #region Return values
        internal int LoadedRoomsCount
        {
            get
            {
                return loadedRooms.Count;
            }
        }

        internal EventManager GetEventManager()
        {
            return eventManager;
        }

        internal List<RoomData> GetActiveRooms()
        {
            SortActiveRooms();
            return orderedActiveRooms.ToList();
        }

        internal KeyValuePair<RoomData, int>[] GetVotedRooms()
        {
            return orderedVotedRooms.ToArray();
        }

        private static RoomModel GetCustomData(UInt32 roomID)
        {
            DataRow RoomData;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT doorx,doory,height,door_orientation,modeldata FROM room_models_customs WHERE roomid = " + roomID);
                RoomData = dbClient.getRow();
            }

            if (RoomData == null)
                throw new Exception("The custom room model for room " + roomID + " was not found");

            return new RoomModel((int)RoomData["doorx"], (int)RoomData["doory"], (Double)RoomData["height"], (Int32)RoomData["door_orientation"], (string)RoomData["modeldata"]);
        }

        internal void LoadModels(IQueryAdapter dbClient)
        {
            roomModels.Clear();

            dbClient.setQuery("SELECT id,door_x,door_y,door_z,door_dir,heightmap FROM room_models");
            var Data = dbClient.getTable();

            if (Data == null)
                return;

            foreach (DataRow Row in Data.Rows)
            {
                roomModels.Add((string)Row["id"], new RoomModel((int)Row["door_x"], (int)Row["door_y"], (Double)Row["door_z"], (int)Row["door_dir"], (string)Row["heightmap"]));
            }
        }

        internal RoomModel GetModel(string Model, UInt32 RoomID)
        {
            if (Model == "custom")
                return GetCustomData(RoomID);

            if (roomModels.ContainsKey(Model))
                return (RoomModel)roomModels[Model];

            return null;
        }


        internal RoomData GenerateRoomData(UInt32 RoomId)
        {
            if (loadedRoomData.ContainsKey(RoomId))
                return loadedRoomData[RoomId];

            var Data = new RoomData();
            if (IsRoomLoaded(RoomId))
            {
                return GetRoom(RoomId).RoomData;
            }
            else
            {
                DataRow Row = null;

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT * FROM rooms WHERE id = " + RoomId);
                    Row = dbClient.getRow();
                }

                if (Row == null)
                {
                    return null;
                }

                Data.Fill(Row);

                if (!loadedRoomData.ContainsKey(RoomId))
                    loadedRoomData.Add(RoomId, Data);

                return Data;
            }
        }
 
        internal Boolean IsRoomLoaded(UInt32 RoomId)
        {
            return loadedRooms.ContainsKey(RoomId);
        }

        internal Room LoadRoom(UInt32 Id)
        {
            if (IsRoomLoaded(Id))
            {
                return GetRoom(Id);
            }

            var Data = GenerateRoomData(Id);

            if (Data == null)
                return null;

            var Room = new Room(Data);

            lock (roomsToAddQueue.SyncRoot)
            {
                roomsToAddQueue.Enqueue(Room);
            }

            Room.InitPets();

            return Room;
        }

        internal RoomData FetchRoomData(UInt32 RoomId, DataRow dRow)
        {
            if (loadedRoomData.ContainsKey(RoomId))
            {
                return loadedRoomData[RoomId];
            }
            else
            {
                var data = new RoomData();
                data.Fill(dRow);

                if (!loadedRoomData.ContainsKey(RoomId))
                    loadedRoomData.Add(RoomId, data);

                return data;
            }
        }

        internal void UnloadRoomData(UInt32 RoomId)
        {
            if (loadedRoomData.ContainsKey(RoomId))
                loadedRoomData.Remove(RoomId);
        }

        internal Room GetRoom(uint roomID)
        {
            Room room;
            if (loadedRooms.TryGetValue(roomID, out room))
                return room;

            return null;
        }

        internal RoomData CreateRoom(GameClient Session, string Name, string Description, string Model, int Type, int MaxUsers, int TradeSettings, uint roomID = 0)
        {
            Name = OtanixEnvironment.FilterInjectionChars(Name);

            if (!roomModels.ContainsKey(Model) && Model != "custom")
            {
                Session.SendNotif(LanguageLocale.GetValue("room.modelmissing"));
                return null;
            }

            if (Name.Length < 3)
            {
                Session.SendNotif(LanguageLocale.GetValue("room.namelengthshort"));
                return null;
            }

            if (Session.GetHabbo().UsersRooms.Count > EmuSettings.ROOMS_X_PESTAÑA)
            {
                Session.SendNotif("Você atingiu o limite de "+ EmuSettings.ROOMS_X_PESTAÑA + " quartos criados...");
                return null;
            }

            UInt32 RoomId = 0;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO rooms (roomtype,caption,owner,model_name,description,category,users_max,trade_settings) VALUES ('private',@caption,@username,@model,@description,'" + Type + "','" + MaxUsers + "','" + TradeSettings + "')");
                dbClient.addParameter("caption", Name);
                dbClient.addParameter("model", Model);
                dbClient.addParameter("username", Session.GetHabbo().Username);
                dbClient.addParameter("description", Description);

                RoomId = roomID != 0 ? roomID : (UInt32)dbClient.insertQuery();
            }

            var newRoomData = GenerateRoomData(RoomId);
            Session.GetHabbo().UsersRooms.Add(newRoomData.Id);
            return newRoomData;
        }
        #endregion

        #region Boot
        internal RoomManager()
        {
            removeTaskStopwatch = new Stopwatch();
            removeTaskStopwatch.Start();

            loadedRooms = new Dictionary<uint, Room>();
            loadedBallRooms = new List<Room>();

            roomModels = new Hashtable();
            loadedRoomData = new Dictionary<uint, RoomData>();
            mToRemove = new List<Room>();

            votedRooms = new Dictionary<RoomData, int>();
            activeRooms = new List<RoomData>();

            roomsToAddQueue = new Queue();
            roomsToRemoveQueue = new Queue();
            roomDataToAddQueue = new Queue();

            votedRoomsRemoveQueue = new Queue();
            votedRoomsAddQueue = new Queue();

            ballRoomsRemoveQueue = new Queue();
            ballRoomsAddQueue = new Queue();

            activeRoomsRemoveQueue = new Queue();
            activeRoomsAddQueue = new Queue();

            eventManager = new EventManager();
        }

        internal void InitVotedRooms(IQueryAdapter dbClient)
        {
            dbClient.setQuery("SELECT * FROM room_voted ORDER BY voted DESC LIMIT 40");
            
            var dTable = dbClient.getTable();
            if (dTable == null)
                return;

            foreach (DataRow dRow in dTable.Rows)
            {
                var data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData((UInt32)dRow["room_id"]);
                if(data != null)
                    QueueVoteAdd(data);
            }
        }

        internal int ClearRoomDataCache()
        {
            int rdCount = loadedRoomData.Count;
            SaveRoomDataSettings();
            return rdCount;
        }

        internal void SaveRoomDataSettings()
        {
            Dictionary<uint, RoomData> roomDataBackup = new Dictionary<uint, RoomData>();
            foreach (RoomData data in loadedRoomData.Values)
            {
                if (data == null)
                    continue;

                data.SaveRoomDataSettings();

                if (IsRoomLoaded(data.Id) && !roomDataBackup.ContainsKey(data.Id))
                    roomDataBackup.Add(data.Id, data);
            }

            loadedRoomData.Clear();
            loadedRoomData = roomDataBackup;
            
            roomDataBackup.Clear();
            roomDataBackup = null;
        }

        internal void SaveRoomDataSettingsAll()
        {
            foreach (RoomData data in loadedRoomData.Values)
            {
                data.SaveRoomDataSettings();
            }
            loadedRoomData.Clear();
        }
        #endregion

        #region Threading
        internal void OnCycle()
        {
            WorkRoomDataQueue();
            WorkRoomsToAddQueue();
            WorkRoomsToRemoveQueue();

            WorkBallRoomsAddQueue();
            WorkBallRoomsRemoveQueue();

            RoomCycleTask();
            RemoveTask(); // Queries

            if (WorkActiveRoomsAddQueue() || WorkActiveRoomsRemoveQueue())
            {
                SortActiveRooms();
            }

            var votedRoomsAdded = WorkVotedRoomsAddQueue();
            var votedRoomsRemoved = WorkVotedRoomsRemoveQueue();

            if (votedRoomsAdded || votedRoomsRemoved)
                SortVotedRooms();

            eventManager.onCycle();
        }

        internal RoomData GetRandomActivePopularRoom()
        {
            if (activeRooms.Count == 0)
                return null;

            return activeRooms.OrderByDescending(t => t.UsersNow).Take(8).ElementAt(new Random().Next(0, (activeRooms.Count >= 8 ? 7 : activeRooms.Count - 1)));
        }

        internal List<RoomData> GetMostActiveRooms(int count)
        {
            try
            {
                return activeRooms.OrderByDescending(t => t.UsersNow).Take(count).ToList();
            }
            catch
            {
                return null;
            }
        }

        private void SortActiveRooms()
        {
            orderedActiveRooms = activeRooms.OrderByDescending(t => t.UsersNow).Take(40);
        }

        private void SortVotedRooms()
        {
            orderedVotedRooms = votedRooms.OrderByDescending(t => t.Value).Take(40);
        }

        private KeyValuePair<RoomData, int> GetSmallerVoted()
        {
            return votedRooms.Take(40).OrderByDescending(t => t.Value).Last();
        }

        internal void CheckNewVotedTop(RoomData data)
        {
            if (votedRooms.ContainsKey(data))
            {
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("UPDATE room_voted SET voted = " + data.Score + " WHERE room_id = " + data.Id);
                }
            }
            else if (votedRooms.Count < 40)
            {
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                { 
                    dbClient.runFastQuery("INSERT IGNORE INTO room_voted VALUES(" + data.Id + "," + data.Score + ")"); 
                }
            }
            else
            {
                var smallVotedRoom = GetSmallerVoted();

                if (data.Score > smallVotedRoom.Value)
                {
                    QueueVoteRemove(smallVotedRoom.Key);

                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("UPDATE room_voted SET room_id = " + data.Id + ", voted = " + data.Score + " WHERE room_id = " + smallVotedRoom.Key.Id);                   
                    }
                }
            }
        }

        private bool WorkActiveRoomsAddQueue()
        {
            if (activeRoomsAddQueue.Count > 0)
            {
                lock (activeRoomsAddQueue.SyncRoot)
                {
                    while (activeRoomsAddQueue.Count > 0)
                    {
                        var data = (RoomData)activeRoomsAddQueue.Dequeue();
                        if (!activeRooms.Contains(data))
                            activeRooms.Add(data);
                    }
                }
                return true;
            }
            return false;
        }

        private bool WorkActiveRoomsRemoveQueue()
        {
            if (activeRoomsRemoveQueue.Count > 0)
            {
                lock (activeRoomsRemoveQueue.SyncRoot)
                {
                    while (activeRoomsRemoveQueue.Count > 0)
                    {
                        var data = (RoomData)activeRoomsRemoveQueue.Dequeue();
                        activeRooms.Remove(data);
                    }
                }
                return true;
            }
            return false;
        }

        private bool WorkBallRoomsAddQueue()
        {
            if (ballRoomsAddQueue.Count > 0)
            {
                lock (ballRoomsAddQueue.SyncRoot)
                {
                    while (ballRoomsAddQueue.Count > 0)
                    {
                        Room data = (Room)ballRoomsAddQueue.Dequeue();
                        loadedBallRooms.Add(data);
                    }
                }
                return true;
            }
            return false;
        }

        private bool WorkBallRoomsRemoveQueue()
        {
            if (ballRoomsRemoveQueue.Count > 0)
            {
                lock (ballRoomsRemoveQueue.SyncRoot)
                {
                    while (ballRoomsRemoveQueue.Count > 0)
                    {
                        Room data = (Room)ballRoomsRemoveQueue.Dequeue();
                        loadedBallRooms.Remove(data);
                    }
                }
                return true;
            }
            return false;
        }

        private bool WorkVotedRoomsAddQueue()
        {
            if (votedRoomsAddQueue.Count > 0)
            {
                lock (votedRoomsAddQueue.SyncRoot)
                {
                    while (votedRoomsAddQueue.Count > 0)
                    {
                        var data = (RoomData)votedRoomsAddQueue.Dequeue();
                        if (!votedRooms.ContainsKey(data))
                            votedRooms.Add(data, data.Score);
                        else
                            votedRooms[data] = data.Score;
                    }
                }
                return true;
            }
            return false;
        }

        private bool WorkVotedRoomsRemoveQueue()
        {
            if (votedRoomsRemoveQueue.Count > 0)
            {
                lock (votedRoomsRemoveQueue.SyncRoot)
                {
                    while (votedRoomsRemoveQueue.Count > 0)
                    {
                        var data = (RoomData)votedRoomsRemoveQueue.Dequeue();
                        votedRooms.Remove(data);
                    }
                }
                return true;
            }
            return false;
        }

        private void WorkRoomsToAddQueue()
        {
            if (roomsToAddQueue.Count > 0)
            {
                lock (roomsToAddQueue.SyncRoot)
                {
                    while (roomsToAddQueue.Count > 0)
                    {
                        var room = (Room)roomsToAddQueue.Dequeue();
                        if (!loadedRooms.ContainsKey(room.RoomId))
                            loadedRooms.Add(room.RoomId, room);
                    }
                }
            }
        }

        private void WorkRoomsToRemoveQueue()
        {
            if (roomsToRemoveQueue.Count > 0)
            {
                lock (roomsToRemoveQueue.SyncRoot)
                {
                    while (roomsToRemoveQueue.Count > 0)
                    {
                        var roomID = (uint)roomsToRemoveQueue.Dequeue();
                        loadedRooms.Remove(roomID);
                    }
                }
            }
        }

        private void WorkRoomDataQueue()
        {
            if (roomDataToAddQueue.Count > 0)
            {
                lock (roomDataToAddQueue.SyncRoot)
                {
                    while (roomDataToAddQueue.Count > 0)
                    {
                        var data = (RoomData)roomDataToAddQueue.Dequeue();
                        if (!loadedRooms.ContainsKey(data.Id))
                            loadedRoomData.Add(data.Id, data);
                    }
                }
            }
        }

        internal static bool roomCyclingEnabled = true;
        private DateTime cycleLastExecution;
        private DateTime cycleBallLastExecution;

        internal void RoomCycleTask()
        {
            if (this.loadedBallRooms.Count > 0)
            {
                TimeSpan sinceBallLastTime = DateTime.Now - cycleBallLastExecution;
                if (sinceBallLastTime.TotalMilliseconds >= 145 && roomCyclingEnabled)
                {
                    cycleBallLastExecution = DateTime.Now;
                    foreach (Room Room in this.loadedBallRooms)
                    {
                        try
                        {
                            if (Room.GotSoccer())
                                Room.GetSoccer().OnCycle();
                        }
                        catch (Exception e)
                        {
                            Logging.LogCriticalException("INVALID MARIO BUG IN BALLMOVEMENT: <" + Room.Id + "> :" + e);
                        }
                    }
                }
            }

            TimeSpan sinceLastTime = DateTime.Now - cycleLastExecution;
            if (sinceLastTime.TotalMilliseconds >= 500 && roomCyclingEnabled)
            {
                cycleLastExecution = DateTime.Now;
                foreach (Room Room in loadedRooms.Values)
                {
                    if (Room.procesoEnCurso == false)
                    {
                        ThreadPool.UnsafeQueueUserWorkItem(Room.ProcessRoom, null);
                        Room.procesoIntentos = 0;
                    }
                    else
                    {
                        Room.procesoIntentos++;
                        if (Room.procesoIntentos > 30)
                        {
                            mToRemove.Add(Room);
                        }
                    }
                }
            }
        }

        private static Stopwatch removeTaskStopwatch;

        internal void RemoveTask()
        {
            if (removeTaskStopwatch.ElapsedMilliseconds >= 5000)
            {
                removeTaskStopwatch.Restart();
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    foreach (var room in mToRemove)
                    {
                        try
                        {
                            Logging.WriteLine("[RoomMgr] Requesting unload of idle room - ID#: " + room.RoomId);

                            room.GetRoomItemHandler().SaveFurniture(dbClient);
                            UnloadRoom(room);
                        }
                        catch { }
                    }
                }
            }
        }
        #endregion

        #region Methods
        internal void QueueBallAdd(Room data)
        {
            lock (ballRoomsAddQueue.SyncRoot)
            {
                ballRoomsAddQueue.Enqueue(data);
            }
        }

        internal void QueueBallRemove(Room data)
        {
            lock (ballRoomsRemoveQueue.SyncRoot)
            {
                ballRoomsRemoveQueue.Enqueue(data);
            }
        }

        internal void QueueVoteAdd(RoomData data)
        {
            lock (votedRoomsAddQueue.SyncRoot)
            {
                votedRoomsAddQueue.Enqueue(data);
            }
        }

        internal void QueueVoteRemove(RoomData data)
        {
            lock (votedRoomsRemoveQueue.SyncRoot)
            {
                votedRoomsRemoveQueue.Enqueue(data);
            }
        }

        internal void QueueActiveRoomAdd(RoomData data)
        {
            lock (activeRoomsAddQueue.SyncRoot)
            {
                activeRoomsAddQueue.Enqueue(data);
            }
        }

        internal void QueueActiveRoomRemove(RoomData data)
        {
            lock (activeRoomsRemoveQueue.SyncRoot)
            {
                activeRoomsRemoveQueue.Enqueue(data);
            }
        }

        internal void RemoveAllRooms()
        {
            var length = loadedRooms.Count;
            var i = 0;

            SaveRoomDataSettingsAll();
            foreach (var Room in loadedRooms.Values)
            {
                UnloadRoom(Room);
                Console.Clear();
                Console.WriteLine("<<- SERVER SHUTDOWN ->> ROOM ITEM SAVE: " + String.Format("{0:0.##}", ((double)i / length) * 100) + "%");
                i++;
            }
            
            Console.WriteLine("Done disposing rooms!");
        }

        internal void UnloadRoom(Room Room)
        {
            if (Room == null)
            {
                return;
            }

            lock (roomsToRemoveQueue.SyncRoot)
            {
                roomsToRemoveQueue.Enqueue(Room.RoomId);
            }

            if(OtanixEnvironment.GetGame().GetRoomManager().loadedBallRooms.Contains(Room))
                OtanixEnvironment.GetGame().GetRoomManager().QueueBallRemove(Room);

            if (Room.HasOngoingEvent)
                eventManager.QueueRemoveEvent(Room.RoomData);

            if (OtanixEnvironment.GetGame().RoomIdEvent == Room.Id)
                OtanixEnvironment.GetGame().RoomIdEvent = 0;

            Room.Destroy();
            Logging.WriteLine("[RoomMgr] Unloaded room: \"" + Room.RoomData.Name + "\" (ID: " + Room.RoomId + ")");
        }

        #endregion
    }

    class TeleUserData
    {
        private readonly UInt32 RoomId;
        private readonly UInt32 TeleId;

        private readonly GameClientMessageHandler mHandler;
        private readonly Habbo mUserRefference;

        internal TeleUserData(GameClientMessageHandler pHandler, Habbo pUserRefference, UInt32 RoomId, UInt32 TeleId)
        {
            //this.User = User;
            mHandler = pHandler;
            mUserRefference = pUserRefference;
            this.RoomId = RoomId;
            this.TeleId = TeleId;
        }

        internal void Execute()
        {
            if (mHandler == null || mUserRefference == null)
            {
                return;
            }

            mUserRefference.IsTeleporting = true;
            mUserRefference.TeleporterId = TeleId;
            mHandler.PrepareRoomForUser(RoomId, "");
        }
    }
}
