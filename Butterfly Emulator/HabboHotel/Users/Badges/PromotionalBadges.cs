using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Badges
{
    class PromotionalBadges
    {
        internal Dictionary<string, string> promotional_badges;

        internal void loadPromotionalBadges(IQueryAdapter dbClient)
        {
            promotional_badges = new Dictionary<string, string>();

            dbClient.setQuery("SELECT * FROM promotional_badges");
            DataTable resultado = dbClient.getTable();

            foreach (DataRow dRow in resultado.Rows)
            {
                if (!promotional_badges.ContainsKey((string)dRow["username"]))
                {
                    promotional_badges.Add((string)dRow["username"], (string)dRow["badge"]);
                }
            }
        }
    }
}
