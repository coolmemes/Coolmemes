using ConnectionManager;
using System.Collections.Generic;

namespace Butterfly.Messages
{
    public class QueuedServerMessage
    {
        private readonly List<byte> packet;
        private ConnectionInformation userConnection;

        internal byte[] getPacket
        {
            get
            {
                return packet.ToArray();
            }
        }

        public QueuedServerMessage(ConnectionInformation connection)
        {
            userConnection = connection;
            packet = new List<byte>();
        }

        internal void Dispose()
        {
            packet.Clear();
            userConnection = null;
        }

        private void appendBytes(byte[] bytes)
        {
            packet.AddRange(bytes);
        }

        internal void appendResponse(ServerMessage message)
        {
            appendBytes(message.GetBytes());
        }

        internal void addBytes(byte[] bytes)
        {
            appendBytes(bytes);
        }

        internal void sendResponse()
        {
            userConnection.SendMuchData(packet.ToArray());
            Dispose();
        }
    }
}
