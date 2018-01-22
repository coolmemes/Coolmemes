using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Butterfly.HabboHotel.Achievements
{
    public class Achievement
    {
        public uint Id;
        public string AchievementName;
        public string Category;
        public uint Reward;
        public uint[] Levels;

        public Achievement(uint Id, string AchievementName, string Category, uint Reward)
        {
            this.Id = Id;
            this.AchievementName = AchievementName;
            this.Category = Category;
            this.Reward = Reward;
        }

        public void LoadLevels(IQueryAdapter dbClient)
        {
            dbClient.setQuery("SELECT level, progress FROM achievements_progress WHERE id = " + Id + " ORDER BY level ASC");
            DataTable dTable = dbClient.getTable();

            this.Levels = new uint[dTable.Rows.Count];

            foreach(DataRow dRow in dTable.Rows)
            {
                uint Level = Convert.ToUInt32(dRow["level"]);
                uint Progress = Convert.ToUInt32(dRow["progress"]);

                Levels[Level - 1] =  Progress;
            }
        }
    }
}
