using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Misc
{
    class Ranks
    {
        public static uint MAX_RANK_ID;

        public static void LoadMaxRankId(IQueryAdapter dbClient)
        {
            dbClient.setQuery("SELECT MAX(id) FROM ranks");
            MAX_RANK_ID = (uint)dbClient.getInteger();
        }
    }
}
