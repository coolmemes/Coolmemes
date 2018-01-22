using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.Scripts
{
    class GiftFix
    {
        internal static void Fix()
        {
            DataTable dTable = null;
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM items_extradata JOIN user_presents ON user_presents.item_id = items_extradata.item_id");
                dTable = dbClient.getTable();

                if (dTable != null)
                {
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        try
                        {
                            int itemId = Convert.ToInt32(dRow["item_id"]);
                            string data = (string)dRow["data"];

                            uint userId = Convert.ToUInt32(data.Split(';')[0]);
                            string message = data.Split(';')[1].Split((char)5)[0];
                            int lazo = int.Parse(data.Split(';')[1].Split((char)5)[1]);
                            int color = int.Parse(data.Split(';')[1].Split((char)5)[2]);

                            string newStr = userId + ";" + (lazo * 1000 + color) + ";" + message;

                            dbClient.setQuery("UPDATE items_extradata SET data = @message WHERE item_id = " + itemId);
                            dbClient.addParameter("message", newStr);
                            dbClient.runQuery();
                        }
                        catch { Console.WriteLine("Error!"); }
                    }
                }
            }

            Console.WriteLine("Regalos actualizados.");
        }
    }
}
