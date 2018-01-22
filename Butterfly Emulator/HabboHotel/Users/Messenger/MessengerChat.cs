using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Messenger
{
    class MessengerChat
    {
        private static Dictionary<uint, List<MessengerChatInfo>> MessengerMessages = new Dictionary<uint,List<MessengerChatInfo>>();

        internal static Int32 MessagesCount
        {
            get
            {
                return MessengerMessages.Count;
            }
        }

        internal static List<MessengerChatInfo> GetMessagesByUserId(uint UserId)
        {
            if (MessengerMessages.ContainsKey(UserId))
                return MessengerMessages[UserId];
            else
                return null;
        }

        internal static void AddMessageToId(uint UserId, uint ToId, string Message)
        {
            MessengerChatInfo message = new MessengerChatInfo(UserId, Message);

            if (MessengerMessages.ContainsKey(ToId))
            {
                MessengerMessages[ToId].Add(message);
            }
            else
            {
                List<MessengerChatInfo> mci = new List<MessengerChatInfo>();
                mci.Add(message);

                MessengerMessages.Add(ToId, mci);
            }
        }

        internal static void ClearMessageToId(uint UserId)
        {
            if (MessengerMessages.ContainsKey(UserId))
                MessengerMessages.Remove(UserId);
        }

        internal static void ClearMessages()
        {
            MessengerMessages.Clear();
        }
    }

    class MessengerChatInfo
    {
        internal uint UserID;
        internal string Message;
        internal int timeSended;

        internal MessengerChatInfo(uint userID, string message)
        {
            this.UserID = userID;
            this.Message = message;
            this.timeSended = OtanixEnvironment.GetUnixTimestamp();
        }
    }
}
