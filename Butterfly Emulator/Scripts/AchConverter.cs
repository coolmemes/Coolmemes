using Butterfly.HabboHotel.Achievements;
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
    class AchConverter
    {
        internal static void Fix()
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM achievements");
                DataTable dTable = dbClient.getTable();

                foreach (DataRow dRow in dTable.Rows)
                {
                    int AchId = int.Parse(dRow["id"].ToString());
                    int i = 1;
                    string progress = (string)dRow["progress_needed"];
                    string[] arr = progress.Split(';');

                    foreach (string a in arr)
                    {
                        dbClient.runFastQuery("INSERT INTO achievements_progress VALUES ('" + AchId + "','" + i++ + "','" + a + "')");
                    }
                }
            }

            Environment.Exit(0);
        }

        internal static void ConvertVoid()
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id, achievements_progress FROM users");
                DataTable dTable = dbClient.getTable();

                foreach (DataRow dRow in dTable.Rows)
                {
                    uint UserId = Convert.ToUInt32(dRow["id"]);
                    string AllProgress = (string)dRow["achievements_progress"];

                    int i = 0;
                    foreach (string Achievement in OtanixEnvironment.GetGame().GetAchievementManager().Achievements.Keys)
                    {
                        uint Progress = uint.Parse(AllProgress.Split(';')[i++]);
                        uint Level = GetAchievementLevel(Achievement, Progress);

                        if (Level == 0 && Progress == 0)
                            continue;

                        dbClient.runFastQuery("INSERT INTO user_achievements VALUES (" + UserId + ", '" + Achievement + "', " + Level + ", " + Progress + ")");
                    }
                }
                Console.WriteLine("Anderson Gay -> Conquistas atualizadas com sucesso!");
            }
        }

        private static uint GetAchievementLevel(string type, uint progress)
        {
            Achievement ach = OtanixEnvironment.GetGame().GetAchievementManager().Achievements[type];
            for (int i = 0; i < ach.Levels.Length; i++)
            {
                if (ach.Levels[i] > progress)
                    return (uint)i;
            }

            return (uint)ach.Levels.Length; // cuando hemos llegado al máximo nivel
        }
    }
}