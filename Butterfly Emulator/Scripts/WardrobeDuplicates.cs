using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly
{
    class WardrobeDuplicates
    {
        internal static void Fix()
        {
            List<string> datasaved = new List<string>();

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM user_wardrobe");

                DataTable dTable = dbClient.getTable();
                int total = dTable.Rows.Count;
                int i = 0;

                foreach (DataRow dRow in dTable.Rows)
                {
                    int userid = Convert.ToInt32(dRow["user_id"]);
                    int slotid = Convert.ToInt32(dRow["slot_id"]);
                    string look = (string)dRow["look"];
                    string gender = (string)dRow["gender"];

                    if(datasaved.Contains(userid + "-" + slotid))
                    {
                        dbClient.setQuery("DELETE FROM user_wardrobe WHERE user_id = '" + userid + "' AND slot_id = '" + slotid + "' AND look = @look AND gender = @gender");
                        dbClient.addParameter("look", look);
                        dbClient.addParameter("gender", gender);
                        dbClient.runQuery();

                        Console.WriteLine("Deleted duplicated key... <" + i + "/" + total + ">");
                    }
                    else
                    {
                        datasaved.Add(userid + "-" + slotid);
                    }

                    i++;
                }
            }

            Console.WriteLine("Ended.");
        }
    }
}
