using System;
using System.Text;
using ButterStorm;
using SharedPacketLib;
using Butterfly.Messages;
using Butterfly.Core;
using ConnectionManager;
using Butterfly.Messages.ClientMessages;
using Butterfly.HabboHotel.GameClients;
using HabboEvents;
using System.Threading.Tasks;
using Butterfly.Util;

namespace Butterfly.Net
{
    public class GamePacketParser : IDataParser
    {
        public delegate void HandlePacket(ClientMessage message);
        public event HandlePacket onNewPacket;

        public void SetConnection()
        {
            onNewPacket = null;
        }

        public void handlePacketData(byte[] data)
        {
            int position = 0;
            while (position < data.Length)
            {
                try
                {
                    var MessageLength = HabboEncoding.DecodeInt32(new byte[] { data[position++], data[position++], data[position++], data[position++] });
                    if (MessageLength < 2 || MessageLength > 4096)
                    {
                        continue;
                    }

                    var MessageId = HabboEncoding.DecodeInt16(new byte[] { data[position++], data[position++] });
                    var Content = new byte[MessageLength - 2];

                    Buffer.BlockCopy(data, position, Content, 0, MessageLength - 2);
                    position += MessageLength - 2;

                    if (onNewPacket != null)
                    {
                        using (ClientMessage message = ClientMessageFactory.GetClientMessage(MessageId, Content))
                        {
                            onNewPacket.Invoke(message);
                        }
                    }
                }
                catch { }
            }
        }

        public void Dispose()
        {
            onNewPacket = null;
        }

        public object Clone()
        {
            return new GamePacketParser();
        }
    }
}
