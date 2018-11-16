using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.ChatMessageStorage
{
    class SpyChatMessage
    {
        private static List<uint> UsersSpying;

        public static void Initialize()
        {
            UsersSpying = new List<uint>();
        }

        public static bool ContainsUser(uint UserId)
        {
            return UsersSpying.Contains(UserId);
        }

        public static void AddUserToSpy(uint UserId)
        {
            if (!UsersSpying.Contains(UserId))
                UsersSpying.Add(UserId);
        }

        public static void RemoveUserToSpy(uint UserId)
        {
            if (UsersSpying.Contains(UserId))
                UsersSpying.Remove(UserId);
        }

        public static void SaveUserLog(uint UserId, uint RoomId, uint ReceiverId, string Message)
        {
            if (ContainsUser(UserId))
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("INSERT INTO user_chat_logs VALUES ('" + UserId + "','" + RoomId + "','" + ReceiverId + "','" + DateTime.Now + "',@msg)");
                    dbClient.addParameter("msg", Message);
                    dbClient.runQuery();
                }
            }
        }

        public static void SaveUserLog(uint UserId, uint RoomId, string Message, int ReceiverId)
        {
            if (ContainsUser(UserId))
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("INSERT INTO user_chat_logs VALUES ('" + UserId + "','" + RoomId + "','" + ReceiverId + "','" + DateTime.Now + "',@msg)");
                    dbClient.addParameter("msg", Message);
                    dbClient.runQuery();
                }
            }
        }
    }
}
