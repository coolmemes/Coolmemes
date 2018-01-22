using System;
using System.Collections.Generic;
using ButterStorm;
using Database_Manager.Database;
using System.Data;

namespace Butterfly.HabboHotel.Users.Messenger
{
    class SearchResultFactory
    {
        internal static List<SearchResult> GetSearchResult(string query)
        {
            var results = new List<SearchResult>();

            DataTable dTable;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id,username,motto,look,last_online FROM users WHERE username LIKE @query LIMIT 50");
                dbClient.addParameter("query", query + "%");
                dTable = dbClient.getTable();
            }

            uint userID;
            string username;
            string motto;
            string look;
            string last_online;
            foreach (DataRow dRow in dTable.Rows)
            {
                userID = Convert.ToUInt32(dRow[0]);
                username = (string)dRow[1];
                motto = (string)dRow[2];
                look = (string)dRow[3];
                last_online = (Convert.ToDouble(dRow[4])).ToString();

                var result = new SearchResult(userID, username, motto, look, last_online);
                results.Add(result);
            }

            return results;
        }
    }
}
