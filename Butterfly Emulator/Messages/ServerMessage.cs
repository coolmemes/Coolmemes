using System;
using System.Collections.Generic;
using ButterStorm;

namespace Butterfly.Messages
{
    public class ServerMessage
    {
        private List<byte> Message = new List<byte>();
        private int MessageId = 0;

        public int Id
        {
            get
            {
                return MessageId;
            }
        }

        public ServerMessage()
        {
        }

        public ServerMessage(int Header)
        {
            Init(Header);
        }

        public void Init(int Header)
        {
            Message = new List<byte>();
            MessageId = Header;
            AppendShort(Header);
        }

        public void setInt(int i, int startOn)
        {
            try
            {
                List<byte> n = Message;
                var intvalue = AppendBytesTo(BitConverter.GetBytes(i), true);
                n.RemoveRange(startOn, intvalue.Count);
                n.InsertRange(startOn, intvalue);
                Message = n;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"Error on setInt: " + e);
            }
        }

        public void AppendShort(int i)
        {
            var s = (Int16)i;
            AppendBytes(BitConverter.GetBytes(s), true);
        }

        public void AppendShort(short s)
        {
            AppendBytes(BitConverter.GetBytes(s), true);
        }

        public void AppendInt32(int i)
        {
            AppendBytes(BitConverter.GetBytes(i), true);
        }

        public void AppendUInt(uint i)
        {
            AppendInt32((int)i);
        }

        public void AppendBoolean(bool b)
        {
            AppendBytes(new byte[] { (byte)(b ? 1 : 0) }, false);
        }

        public void AppendString(string s)
        {
            var toAdd = OtanixEnvironment.GetDefaultEncoding().GetBytes(s);
            AppendShort(toAdd.Length);
            AppendBytes(toAdd, false);
        }

        public void AppendBytes(byte[] b, bool IsInt)
        {
            if (IsInt)
            {
                for (var i = (b.Length - 1); i > -1; i--)
                    Message.Add(b[i]);
            }
            else
                Message.AddRange(b);
        }

        public void AppendByted(int Number)
        {
            AppendBytes(new byte[] { (byte)Number }, false);
        }

        public List<byte> AppendBytesTo(byte[] b, bool IsInt)
        {
            var message = new List<byte>();
            if (IsInt)
            {
                for (var i = (b.Length - 1); i > -1; i--)
                    message.Add(b[i]);
            }
            else
                message.AddRange(b);
            return message;
        }

        public byte[] GetBytes()
        {
            var Final = new List<byte>();
            Final.AddRange(BitConverter.GetBytes(Message.Count)); // packet len
            Final.Reverse();
            Final.AddRange(Message); // Add Packet
            return Final.ToArray();
        }

        public override string ToString()
        {
            return (OtanixEnvironment.GetDefaultEncoding().GetString(GetBytes()).Replace(Convert.ToChar(0).ToString(), "[0]").Replace(Convert.ToChar(1).ToString(), "[1]").Replace(Convert.ToChar(2).ToString(), "[2]").Replace(Convert.ToChar(3).ToString(), "[3]").Replace(Convert.ToChar(4).ToString(), "[4]").Replace(Convert.ToChar(5).ToString(), "[5]").Replace(Convert.ToChar(6).ToString(), "[6]").Replace(Convert.ToChar(7).ToString(), "[7]").Replace(Convert.ToChar(8).ToString(), "[8]").Replace(Convert.ToChar(9).ToString(), "[9]").Replace(Convert.ToChar(10).ToString(), "[10]").Replace(Convert.ToChar(11).ToString(), "[11]").Replace(Convert.ToChar(12).ToString(), "[12]").Replace(Convert.ToChar(13).ToString(), "[13]").Replace(Convert.ToChar(14).ToString(), "").Replace(Convert.ToChar(15).ToString(), "").Replace(Convert.ToChar(16).ToString(), "").Replace(Convert.ToChar(17).ToString(), ""));
        }
    }
}
