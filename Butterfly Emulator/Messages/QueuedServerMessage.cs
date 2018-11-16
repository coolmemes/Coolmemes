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

        private void AppendBytes(byte[] bytes)
        {
            packet.AddRange(bytes);
        }

        internal void AppendResponse(ServerMessage message)
        {
            AppendBytes(message.GetBytes());
        }

        internal void AddBytes(byte[] bytes)
        {
            AppendBytes(bytes);
        }

        internal void SendResponse()
        {
            userConnection.SendMuchData(packet.ToArray());
            Dispose();
        }
    }
}
