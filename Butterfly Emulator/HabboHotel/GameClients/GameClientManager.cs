using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Butterfly.Core;
using Butterfly.Messages;
using ButterStorm;
using ConnectionManager;
using System.Threading;
using Butterfly.HabboHotel.Users.Messenger;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.Group;

namespace Butterfly.HabboHotel.GameClients
{
    class GameClientManager
    {
        #region Fields
        internal Dictionary<uint, GameClient> clients;
        
        private readonly Queue clientsAddQueue;
        private readonly Queue clientsToRemove;
        private readonly Queue creditQueuee;
        private readonly Queue diamondsQueuee;
        private readonly Queue moedasQueuee;
        private readonly Queue badgeQueue;
        private readonly Queue authorizedPacketSending;
        private readonly Queue broadcastQueue;

        internal Hashtable usernameRegister;
        internal Hashtable userIDRegister;

        internal Queue consolesToUpdate;
        #endregion      

        #region Return values
        internal int connectionCount
        {
            get
            {
                return clients.Count;
            }
        }

        internal GameClient GetClientByUserID(uint userID)
        {
            if (userIDRegister.ContainsKey(userID))
                return (GameClient)userIDRegister[userID];
            return null;
        }

        internal GameClient GetClientByUsername(string username)
        {
            if (usernameRegister.ContainsKey(username.ToLower()))
                return (GameClient)usernameRegister[username.ToLower()];
            return null;
        }

        internal GameClient GetClient(uint clientID)
        {
            if (clients.ContainsKey(clientID))
                return clients[clientID];
            return null;
        }

        internal string GetNameById(uint Id)
        {
            var client = GetClientByUserID(Id);

            if (client != null)
                return client.GetHabbo().Username;

            string username;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT username FROM users WHERE id = " + Id);
                username = dbClient.getString();
            }

            return username;
        }

        internal IEnumerable<GameClient> GetClientsById(Dictionary<uint, MessengerBuddy>.KeyCollection users)
        {
            return users.Select(id => GetClientByUserID(id)).Where(client => client != null);
        }
        #endregion

        #region Constructor
        internal GameClientManager()
        {
            ConsoleUpdatesStopwatch = new Stopwatch();
            ConsoleUpdatesStopwatch.Start();
            OnlineTimeStopwatch = new Stopwatch();
            OnlineTimeStopwatch.Start();
            clients = new Dictionary<uint, GameClient>();
            clientsAddQueue = new Queue();
            clientsToRemove = new Queue();
            creditQueuee = new Queue();
            diamondsQueuee = new Queue();
            moedasQueuee = new Queue();
            badgeQueue = new Queue();
            broadcastQueue = new Queue();
            authorizedPacketSending = new Queue();
            usernameRegister = new Hashtable();
            userIDRegister = new Hashtable();
            consolesToUpdate = new Queue();
        }
        #endregion

        #region Threads procesado principal

        internal Stopwatch ConsoleUpdatesStopwatch;

        internal void handleConsoleUpdates()
        {
            if (ConsoleUpdatesStopwatch.ElapsedMilliseconds >= 15000)
            {
                if (consolesToUpdate.Count > 0)
                {
                    lock (consolesToUpdate.SyncRoot)
                    {
                        while (consolesToUpdate.Count > 0)
                        {
                            GameClient User = (GameClient)consolesToUpdate.Dequeue();
                            if (User != null && User.GetHabbo() != null && User.GetHabbo().GetMessenger() != null)
                            {
                                if (User.GetHabbo().GetMessenger().myFriends > 0)
                                    User.GetHabbo().GetMessenger().OnStatusChanged();
                            }
                        }
                    }
                }

                ConsoleUpdatesStopwatch.Restart();
            }
        }

        internal void OnCycle()
        {
            CheckOnlineTime();
            RemoveClients();
            AddClients();
            handleConsoleUpdates(); // revisar
            GiveDiamonds();
            GiveMoedas();
            GiveBadges();
            BroadcastPacketsWithRankRequirement();
            BroadcastPackets();
        }

        private Stopwatch OnlineTimeStopwatch;
        private void CheckOnlineTime()
        {
            try
            {
                if (OnlineTimeStopwatch.ElapsedMilliseconds >= 60000)
                {
                    foreach (GameClient client in clients.Values)
                    {
                        if (client == null || client.GetHabbo() == null)
                            continue;

                        OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(client.GetHabbo().Id, "ACH_AllTimeHotelPresence", 1);

                        if (client.GetHabbo().AlfaServiceEnabled)
                        {
                            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(client.GetHabbo().Id, "ACH_GuideOnDutyPresence", 1);
                        }

                        if (client.GetHabbo().CitizenshipLevel == 1 || client.GetHabbo().CitizenshipLevel == 2 || client.GetHabbo().CitizenshipLevel == 3)
                        {
                            OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(client, "citizenship");
                        }
                        else if (client.GetHabbo().HelperLevel == 3 || client.GetHabbo().HelperLevel == 4 || client.GetHabbo().HelperLevel == 5 || client.GetHabbo().HelperLevel == 6 || client.GetHabbo().HelperLevel == 7)
                        {
                            OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(client, "helper");
                        }

                        Diamonds.GiveCycleDiamonds(client);
                        Moedas.GiveCycleMoedas(client);
                        MensagensAutomaticas.MostraNotificacaoUser(client);
                    }

                    OnlineTimeStopwatch.Restart();
                }
            }
            catch { }
        }
        #endregion

        #region Collection modyfying
        private void AddClients()
        {
            if (clientsAddQueue.Count > 0)
            {
                lock (clientsAddQueue.SyncRoot)
                {
                    while (clientsAddQueue.Count > 0)
                    {
                        GameClient client = (GameClient)clientsAddQueue.Dequeue();
                        clients.Add(client.ConnectionID, client);
                        client.StartConnection();
                    }
                }
            }
        }

        private void RemoveClients()
        {
            if (clientsToRemove.Count > 0)
            {
                lock (clientsToRemove.SyncRoot)
                {
                    while (clientsToRemove.Count > 0)
                    {
                        uint clientID = (uint)clientsToRemove.Dequeue();
                        clients.Remove(clientID);
                    }
                }
            }
        }

        private void GiveDiamonds()
        {
            if (diamondsQueuee.Count > 0)
            {
                lock (diamondsQueuee.SyncRoot)
                {
                    while (diamondsQueuee.Count > 0)
                    {
                        int amount = (int)diamondsQueuee.Dequeue();
                        foreach (GameClient client in clients.Values)
                        {
                            if (client == null || client.GetHabbo() == null)
                                continue;

                            client.GetHabbo().GiveUserDiamonds(amount);
                        }
                    }
                }
            }
        }

        private void GiveMoedas()
        {
            if (moedasQueuee.Count > 0)
            {
                lock (moedasQueuee.SyncRoot)
                {
                    while (moedasQueuee.Count > 0)
                    {
                        int amount = (int)moedasQueuee.Dequeue();
                        foreach (GameClient client in clients.Values)
                        {
                            if (client == null || client.GetHabbo() == null)
                                continue;

                            client.GetHabbo().darMoedas(amount);
                        }
                    }
                }
            }
        }

        private void GiveBadges()
        {
            if (badgeQueue.Count > 0)
            {
                lock (badgeQueue.SyncRoot)
                {
                    while (badgeQueue.Count > 0)
                    {
                        string badgeID = (string)badgeQueue.Dequeue();
                        foreach (var client in clients.Values)
                        {
                            if (client == null || client.GetHabbo() == null || client.GetHabbo().GetBadgeComponent() == null)
                                continue;

                            client.GetHabbo().GetBadgeComponent().GiveBadge(badgeID);
                            client.SendNotif(LanguageLocale.GetValue("user.badgereceived"));
                        }
                    }
                }
            }
        }

        private void BroadcastPacketsWithRankRequirement()
        {
            if (authorizedPacketSending.Count > 0)
            {
                lock (authorizedPacketSending.SyncRoot)
                {
                    while (authorizedPacketSending.Count > 0)
                    {
                        FusedPacket packet = (FusedPacket)authorizedPacketSending.Dequeue();
                        foreach (var client in clients.Values)
                        {
                            if (packet.requirements.Contains("ingroup"))
                            {
                                var reqSplit = packet.requirements.Split('_');
                                GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(Convert.ToUInt32(reqSplit[1]));
                                if (!Group.IsMember(client.GetHabbo().Id) || client.GetHabbo().Id == packet.userId)
                                    continue;

                                client.SendMessage(packet.content);
                            }
                            else
                            {
                                if (client == null || client.GetHabbo() == null || !client.GetHabbo().HasFuse(packet.requirements) || client.GetHabbo().Id == packet.userId)
                                    continue;

                                client.SendMessage(packet.content);
                            }
                        }
                    }
                }
            }
        }

        private void BroadcastPackets()
        {
            if (broadcastQueue.Count > 0)
            {
                lock (broadcastQueue.SyncRoot)
                {
                    while (broadcastQueue.Count > 0)
                    {
                        ServerMessage message = (ServerMessage)broadcastQueue.Dequeue();

                        foreach (var client in clients.Values)
                        {
                            if (client == null || client.GetConnection() == null)
                                continue;

                            if (client.GetHabbo() != null && client.GetHabbo().alertasAtivados == false)
                                continue;

                            client.GetConnection().SendData(message.GetBytes());
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods
        internal void CreateAndStartClient(uint clientID, ConnectionInformation connection)
        {
            var client = new GameClient(clientID, connection);
            if (clients.ContainsKey(clientID))
                clients.Remove(clientID);

            lock (clientsAddQueue.SyncRoot)
            {
                clientsAddQueue.Enqueue(client);
            }
        }

        internal void DisposeConnection(uint clientID)
        {
            var Client = GetClient(clientID);
            if (Client != null)
            {
                Client.Stop();
            }

            lock (clientsToRemove.SyncRoot)
            {
                clientsToRemove.Enqueue(clientID);
            }
        }

        internal void QueueConsoleUpdate(GameClient Client)
        {
            lock (consolesToUpdate.SyncRoot)
            {
                if(!consolesToUpdate.Contains(Client))
                    consolesToUpdate.Enqueue(Client);
            }
        }

        internal void QueueBroadcaseMessage(ServerMessage message)
        {
            lock (broadcastQueue.SyncRoot)
            {
                broadcastQueue.Enqueue(message);
            }
        }

        internal void QueueBroadcaseMessage(ServerMessage message, string requirements, uint userId)
        {
            var packet = new FusedPacket(message, requirements, userId);
            lock (authorizedPacketSending.SyncRoot)
            {
                authorizedPacketSending.Enqueue(packet);
            }
        }

        internal void QueueDiamondsUpdate(int amount)
        {
            lock (diamondsQueuee.SyncRoot)
            {
                diamondsQueuee.Enqueue(amount);
            }
        }

        internal void QueueMoedasUpdate(int amount)
        {
            lock (moedasQueuee.SyncRoot)
            {
                moedasQueuee.Enqueue(amount);
            }
        }

        internal void QueueBadgeUpdate(string badge)
        {
            lock (badgeQueue.SyncRoot)
            {
                badgeQueue.Enqueue(badge);
            }
        }

        private void CheckEffects()
        {
            foreach (var client in clients.Values)
            {
                if (client.GetHabbo() == null || client.GetHabbo().GetAvatarEffectsInventoryComponent() == null)
                    continue;

                client.GetHabbo().GetAvatarEffectsInventoryComponent().CheckExpired();
            }
        }

        internal void LogClonesOut(uint UserID)
        {
            var client = GetClientByUserID(UserID);
            if (client != null)
                client.Disconnect();
        }

        internal void RegisterClient(GameClient client, uint userID, string username)
        {
            if (usernameRegister.ContainsKey(username.ToLower()))
                usernameRegister[username.ToLower()] = client;
            else
                usernameRegister.Add(username.ToLower(), client);

            if (userIDRegister.ContainsKey(userID))
                userIDRegister[userID] = client;
            else
                userIDRegister.Add(userID, client);
        }

        internal void UnregisterClient(uint userid, string username)
        {
            userIDRegister.Remove(userid);
            usernameRegister.Remove(username.ToLower());
        }

        internal void ChangeUsernameInUsernameRegisterUserName(string oldname, string newname)
        {
            try
            {
                if (usernameRegister.ContainsKey(oldname.ToLower()))
                {
                    GameClient cliente;
                    cliente = (GameClient) usernameRegister[oldname.ToLower()];
                    usernameRegister.Remove(oldname.ToLower());
                    usernameRegister.Add(newname.ToLower(), cliente);
                }

            }
            catch (Exception e)
            {
                Logging.LogCriticalException("UNameSession update -> " + e.ToString());
            }
        }

        internal void CloseAll()
        {
            var Count = clients.Values.Count(client => client.GetHabbo() != null);

            if (Count < 1)
                Count = 1;

            var current = 0;
            var ClientLength = clients.Count;
            foreach (var client in clients.Values)
            {
                current++;
                if (client.GetHabbo() != null)
                {
                    try
                    {
                        client.GetHabbo().OnDisconnect();

                        Console.Clear();
                        Console.WriteLine("<<- SERVER SHUTDOWN ->> INVENTORY SAVE: " + String.Format("{0:0.##}", ((double)current / Count) * 100) + "%");
                    }
                    catch { }
                }
            }

            Console.WriteLine("Done saving users inventory!");
            Console.WriteLine("Closing server connections...");

            try
            {
                var i = 0;
                foreach (var client in clients.Values)
                {
                    i++;
                    if (client.GetConnection() != null)
                    {
                        try
                        {
                            client.GetConnection().Dispose();
                        }
                        catch { }

                        Console.Clear();
                        Console.WriteLine("<<- SERVER SHUTDOWN ->> CONNECTION CLOSE: " + String.Format("{0:0.##}", ((double)i / ClientLength) * 100) + "%");
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException(e.ToString());
            }

            clients.Clear();
            Console.WriteLine("Connections closed!");
        }
        #endregion
    }
}
