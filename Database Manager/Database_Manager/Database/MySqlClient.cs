using Database_Manager.Database.Session_Details;
using Database_Manager.Database.Session_Details.Interfaces;
using MySql.Data.MySqlClient;

namespace Database_Manager.Database
{
    public class MySqlClient : IDatabaseClient
    {
        private readonly MySqlConnection connection;
        private readonly DatabaseManager dbManager;
        private IQueryAdapter info;

        public MySqlClient(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
            connection = new MySqlConnection(dbManager.getConnectionString());
        }

        public void connect()
        {
            connection.Open();
        }

        public void disconnect()
        {
            try
            {
                connection.Close();
            }
            catch 
            { }
        }

        public void Dispose()
        {
            info = null;
            disconnect();
            dbManager.FreeConnection(this);
        }

        internal MySqlCommand getNewCommand()
        {
            return connection.CreateCommand();
        }

        public IQueryAdapter getQueryReactor()
        {
            return info;
        }

        internal MySqlTransaction getTransaction()
        {
            return connection.BeginTransaction();
        }

        public bool isAvailable()
        {
            return (info == null);
        }

        public void prepare()
        {
            info = new NormalQueryReactor(this);
        }

        public void reportDone()
        {
            Dispose();
        }
    }
}

