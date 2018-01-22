namespace Database_Manager.Database
{
    using Database_Exceptions;
    using Session_Details.Interfaces;
    using Managers.Database;
    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Data.SqlClient;
    using Database_Manager.Database.Session_Details;

    public class DatabaseManager
    {
        private string connectionString;
        private List<MySqlClient> databaseClients;
        private bool isConnected = false;
        private readonly uint maxPoolSize;
        private DatabaseServer server;
        private readonly Queue connections;

        public static bool dbEnabled = true;

        public DatabaseManager(uint maxPoolSize, int clientAmount)
        {
            if (maxPoolSize < clientAmount)
                throw new DatabaseException("The poolsize can not be larger than the client amount!");

            this.maxPoolSize = maxPoolSize;
            this.connections = new Queue();
        }

        private void addConnection(int id)
        {
            var item = new MySqlClient(this);
            item.connect();
            databaseClients.Add(item);
        }

        private void createNewConnectionString()
        {
            var connectionString = new MySqlConnectionStringBuilder
            {
                Server = server.getHost(),
                Port = server.getPort(),
                UserID = server.getUsername(),
                Password = server.getPassword(),
                Database = server.getDatabaseName(),
                MinimumPoolSize = maxPoolSize / 2,
                MaximumPoolSize = maxPoolSize,
                AllowZeroDateTime = true,
                Pooling = true,
                ConvertZeroDateTime = true,
                DefaultCommandTimeout = 300,
                ConnectionTimeout = 10
            };

            setConnectionString(connectionString.ToString());
        }

        public void destroy()
        {
            lock (this)
            {
                isConnected = false;
                if (databaseClients != null)
                {
                    foreach (var client in databaseClients)
                    {
                        if (!client.isAvailable())
                        {
                            client.Dispose();
                        }
                        client.disconnect();
                    }
                    databaseClients.Clear();
                }
            }
        }

        private void disconnectUnusedClients()
        {
            lock (this)
            {
                foreach (var client in databaseClients)
                {
                    if (client.isAvailable())
                    {
                        client.disconnect();
                    }
                }
            }
        }

        internal string getConnectionString()
        {
            return connectionString;
        }

        public IQueryAdapter getQueryreactor()
        {
            IDatabaseClient dbClient = null;
            lock (connections.SyncRoot)
            {
                if (connections.Count > 0)
                {
                    dbClient = (IDatabaseClient)connections.Dequeue();
                }
            }

            if (dbClient != null)
            {
                dbClient.connect();
                dbClient.prepare();
                return dbClient.getQueryReactor();
            }
            else
            {
                IDatabaseClient connection = new MySqlClient(this);
                connection.connect();
                connection.prepare();
                return connection.getQueryReactor();
            }
        }

        internal void FreeConnection(IDatabaseClient dbClient)
        {
            lock (connections.SyncRoot)
            {
                connections.Enqueue(dbClient);
            }
        }

        public void init()
        {
            try
            {
                createNewConnectionString();
                databaseClients = new List<MySqlClient>((int) maxPoolSize);
            }
            catch (MySqlException exception)
            {
                isConnected = false;
                throw new Exception("Could not connect the clients to the database: " + exception.Message);
            }
            isConnected = true;
        }

        public bool isConnectedToDatabase()
        {
            return isConnected;
        }

        private void setConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool setServerDetails(string host, uint port, string username, string password, string databaseName)
        {
            try
            {
                server = new DatabaseServer(host, port, username, password, databaseName);
                return true;
            }
            catch (DatabaseException)
            {
                isConnected = false;
                return false;
            }
        }
    }
}

