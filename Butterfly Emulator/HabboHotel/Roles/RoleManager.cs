using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Butterfly.HabboHotel.Users;
using Database_Manager.Database.Session_Details.Interfaces;

namespace Butterfly.HabboHotel.Roles
{
    class RoleManager
    {
        private Dictionary<uint, Rank> Ranks;

        public RoleManager()
        {
            Ranks = new Dictionary<uint, Rank>();
        }

        internal void LoadRanks(IQueryAdapter dbClient)
        {
            Ranks.Clear();

            dbClient.setQuery("SELECT * FROM ranks");
            DataTable Data = dbClient.getTable();

            if (Data != null)
            {
                foreach (DataRow Row in Data.Rows)
                {
                    Ranks.Add(Convert.ToUInt32(Row["id"]), new Rank(Row));
                }
            }
        }

        public bool RankHasRight(uint RankId, string Fuse)
        {
            if (!Ranks.ContainsKey(RankId))
                return false;

            return Ranks[RankId].HasFuse(Fuse);
        }

        public string GetRankHtmlName(uint RankId)
        {
            if (!Ranks.ContainsKey(RankId))
                return string.Empty;

            return Ranks[RankId].GetHtmlName();
        }
    }
}
