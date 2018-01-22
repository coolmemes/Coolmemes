using System;
using System.Text;
using Butterfly.Messages.ClientMessages;
using ButterStorm;
using Butterfly.Util;

namespace Butterfly.Messages
{
    public class ClientMessage : IDisposable
    {
        private int MessageId;
        internal byte[] Body;
        private int Pointer;

        internal int Id
        {
            get
            {
                return MessageId;
            }
        }

        internal int RemainingLength
        {
            get
            {
                return Body.Length - Pointer;
            }
        }

        internal ClientMessage(int messageID, byte[] body)
        {
            Init(messageID, body);
        }

        internal void Init(int messageID, byte[] body)
        {
            if (body == null)
                body = new byte[0];

            MessageId = messageID;
            Body = body;

            Pointer = 0;
        }

        public override string ToString()
        {
            return "[" + Id + "] BODY: " + (OtanixEnvironment.GetDefaultEncoding().GetString(Body).Replace(Convert.ToChar(0).ToString(), "[0]").Replace(Convert.ToChar(1).ToString(), "[1]").Replace(Convert.ToChar(2).ToString(), "[2]").Replace(Convert.ToChar(3).ToString(), "[3]").Replace(Convert.ToChar(4).ToString(), "[4]").Replace(Convert.ToChar(5).ToString(), "[5]").Replace(Convert.ToChar(6).ToString(), "[6]").Replace(Convert.ToChar(7).ToString(), "[7]").Replace(Convert.ToChar(8).ToString(), "[8]").Replace(Convert.ToChar(9).ToString(), "[9]"));
        }

        internal byte[] ReadBytes(int Bytes)
        {
            if (Bytes > RemainingLength)
                Bytes = RemainingLength;

            var data = new byte[Bytes];
            Buffer.BlockCopy(Body, Pointer, data, 0, Bytes);
            Pointer += Bytes;
            return data;
        }

        internal byte[] PlainReadBytes(int Bytes)
        {
            if (Bytes > RemainingLength)
                Bytes = RemainingLength;

            var data = new byte[Bytes];
            Buffer.BlockCopy(Body, Pointer, data, 0, Bytes);
            return data;
        }

        internal string PopFixedString()
        {
            return OtanixEnvironment.GetDefaultEncoding().GetString(ReadBytes(HabboEncoding.DecodeInt16(ReadBytes(2))));
        }

        internal Boolean PopWiredBoolean()
        {
            if (RemainingLength > 0 && Body[Pointer++] == Convert.ToChar(1))
            {
                return true;
            }

            return false;
        }

        internal Int32 PopWiredInt32()
        {
            if (RemainingLength < 1)
            {
                return 0;
            }

            var Data = PlainReadBytes(4);

            var i = HabboEncoding.DecodeInt32(Data);

            Pointer += 4;

            return i;
        }

        internal uint PopWiredUInt()
        {
            return uint.Parse(PopWiredInt32().ToString());
        }

        public void Dispose()
        {
            ClientMessageFactory.ObjectCallback(this);
            GC.SuppressFinalize(this);
        }
    }
}
