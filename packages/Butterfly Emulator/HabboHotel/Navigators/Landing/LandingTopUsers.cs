using Butterfly.HabboHotel.Users;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Navigators.Landing
{
    class LandingTopUsers
    {
        private uint[] TopUsers;
        private ServerMessage Message;

        public bool Enable;
        public string EventName;
        public string EventMessage;
        public int Count;

        public void Initialize(IQueryAdapter dbClient)
        {
            dbClient.setQuery("SELECT name, message, count FROM landview_topusers WHERE enable = '1' ORDER BY id DESC LIMIT 1");
            DataRow dRow = dbClient.getRow();

            if (dRow != null)
            {
                Enable = true;
                EventName = (string)dRow["name"];
                EventMessage = (string)dRow["message"];
                Count = (int)dRow["count"];
            }
        }

        public void Load()
        {
            if (!Enable)
                return;

            TopUsers = new uint[Count];

            int i = 0;
            DataTable dTable = null;
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id FROM users WHERE rank = '1' ORDER BY coins_purchased DESC LIMIT " + Count);
                dTable = dbClient.getTable();
            }

            foreach (DataRow dRow in dTable.Rows)
            {
                uint UserId = Convert.ToUInt32(dRow["id"]);
                TopUsers[i++] = UserId;
            }


            GenerateMessage(i);
        }

        public ServerMessage GetMessage
        {
            get
            {
                return Message;
            }
        }

        private void GenerateMessage(int UsersCount)
        {
            Message = new ServerMessage(Outgoing.CommunityGoalHallOfFame);
            Message.AppendString(EventName); // landing.view.competition.hof." + x + ".rankdesc.X
            Message.AppendInt32(UsersCount);
            for (int i = 0; i < UsersCount; i++)
            {
                uint UserId = TopUsers[i];
                if (UserId == 0)
                    break;

                Habbo User = UsersCache.getHabboCache(UserId);
                if (User == null)
                    break;

                Message.AppendUInt(User.Id);
                Message.AppendString(User.Username);
                Message.AppendString(User.Look);
                Message.AppendInt32(i); // rank
                Message.AppendUInt(User.CoinsPurchased); // piruletas 
            }
        }
    }
}
