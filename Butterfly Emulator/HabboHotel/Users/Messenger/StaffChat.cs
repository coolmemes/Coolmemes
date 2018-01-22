using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Messenger
{
    class StaffChat
    {
        internal static MessengerBuddy MessengerStaff;

        internal static void Initialize(IQueryAdapter dbClient)
        {
            dbClient.setQuery("SELECT username, look, motto FROM users WHERE id = '" + EmuSettings.CHAT_USER_ID + "'");
            DataRow dRow = dbClient.getRow();

            if (dRow != null)
            {
                MessengerStaff = new MessengerBuddy(EmuSettings.CHAT_USER_ID, (string)dRow["username"], (string)dRow["look"], (string)dRow["motto"]);
            }
        }
    }
}
