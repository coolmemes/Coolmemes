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
    class ChangeRoomState
    {
        internal static void Fix()
        {
            Dictionary<int, string> values = new Dictionary<int, string>();

            using(IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id, state FROM rooms");
                DataTable dTable = dbClient.getTable();

                foreach(DataRow dRow in dTable.Rows)
                {
                    values.Add(Convert.ToInt32(dRow["id"]), (string)dRow["state"]);
                }

                Console.WriteLine("Pulse una tecla para que se apliquen los cambios del roomstate:");
                Console.ReadLine();

                foreach(KeyValuePair<int, string> vv in values)
                {
                    int State = 0;

                    if (vv.Value == "open")
                        State = 0;
                    else if (vv.Value == "locked")
                        State = 1;
                    else if (vv.Value == "password")
                        State = 2;
                    else if (vv.Value == "invisible")
                        State = 3;

                    dbClient.runFastQuery("UPDATE rooms SET state = " + State + " WHERE id = '" + vv.Key + "'");
                }
            }
        }
    }
}
