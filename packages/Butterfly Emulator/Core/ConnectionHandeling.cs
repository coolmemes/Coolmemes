using System;
using ButterStorm;
using ConnectionManager;
using Butterfly.Net;
using System.Collections;

namespace Butterfly.Core
{
    public class ConnectionHandeling
    {
        internal static SocketManager manager;
        private readonly Hashtable liveConnections;

        public ConnectionHandeling(int port, int maxConnections, int connectionsPerIP, bool enabeNagles)
        {
            liveConnections = new Hashtable();
            manager = new SocketManager();
            manager.init(port, maxConnections, connectionsPerIP, new InitialPacketParser(), !enabeNagles);
        }

        internal void init()
        {
            manager.connectionEvent += manager_connectionEvent;
        }

        private void manager_connectionEvent(ConnectionInformation connection)
        {
            liveConnections.Add(connection.getConnectionID(), connection);
            connection.connectionChanged += connectionChanged;
            OtanixEnvironment.GetGame().GetClientManager().CreateAndStartClient((uint)connection.getConnectionID(), connection);
        }

        private void connectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.closed)
            {
                CloseConnection(information.getConnectionID());
                liveConnections.Remove(information.getConnectionID());
            }
        }

        internal void Start()
        {
            manager.initializeConnectionRequests();
        }

        internal void CloseConnection(int p)
        {
            try
            {
                var info = liveConnections[p];
                if (info != null)
                {
                    var con = (ConnectionInformation)info;
                    con.Dispose();
                    OtanixEnvironment.GetGame().GetClientManager().DisposeConnection((uint)p);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }
        }

        internal void Destroy()
        {
            manager.destroy();   
        }
    }
}
