using System.Collections.Generic;
using System.Linq;
using Butterfly.HabboHotel.Achievements.Composer;
using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Users;
using System;
using HabboEvents;
using System.Data;
using Butterfly.Core;

namespace Butterfly.HabboHotel.Achievements
{
    public class AchievementManager
    {
        /// <summary>
        /// Paquete predeterminado que envía al usuario al iniciar sesión con las recompensas disponibles.
        /// </summary>
        public ServerMessage AchievementPrede;

        /// <summary>
        /// Lista con todas las recompensas que existen.
        /// </summary>
        public Dictionary<string, Achievement> Achievements;

        /// <summary>
        /// Devuelve el número de recompensas totales.
        /// </summary>
        public int AchievementsCount
        {
            get { return Achievements.Count; }
        }

        /// <summary>
        /// Obtiene la clase Achievement mediante el Id.
        /// </summary>
        /// <param name="AchId"></param>
        /// <returns></returns>
        public Achievement GetAchievementById(uint Id)
        {
            return Achievements.Values.Where(t => t.Id == Id).First();
        }

        /// <summary>
        /// Obtiene la clase Achievement mediante el Nombre.
        /// </summary>
        /// <param name="AchName"></param>
        /// <returns></returns>
        public Achievement GetAchievementByName(string Name)
        {
            try
            {
                return Achievements[Name];
            }
            catch (Exception e)
            {
                Logging.LogException("Achievement no encontrado en la librería (" + Name + "): " + e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Carga las recompensas.
        /// </summary>
        /// <param name="dbClient"></param>
        public void Initialize(IQueryAdapter dbClient)
        {
            Load(dbClient);
            AchievementPrede = AchievementStatic.Compose(Achievements.Values.ToList());
        }

        /// <summary>
        /// Obtiene y genera las recompensas de la DB.
        /// </summary>
        /// <param name="dbClient"></param>
        private void Load(IQueryAdapter dbClient)
        {
            Achievements = new Dictionary<string, Achievement>();

            dbClient.setQuery("SELECT * FROM achievements");
            DataTable dTable = dbClient.getTable();

            uint id;
            string category;
            string groupName;
            uint rewardPoints;

            foreach (DataRow dRow in dTable.Rows)
            {
                id = Convert.ToUInt32(dRow["id"]);
                groupName = (string)dRow["group_name"];
                category = (string)dRow["category"];
                rewardPoints = Convert.ToUInt32(dRow["reward_points"]);

                Achievement achievement = new Achievement(id, groupName, category, rewardPoints);
                achievement.LoadLevels(dbClient);

                Achievements.Add(groupName, achievement);
            }
        }

        /// <summary>
        /// Envía el paquete que muestra las recompensas.
        /// </summary>
        /// <param name="Session"></param>
        /// <param name="Message"></param>
        public void GetList(GameClient Session)
        {
            Session.SendMessage(AchievementListComposer.Compose(Session, Achievements.Values.ToList()));
        }

        /// <summary>
        /// Actualiza el progreso una recompensa.
        /// </summary>
        /// <param name="Session"></param>
        /// <param name="UserId"></param>
        /// <param name="AchievementGroup"></param>
        /// <param name="ProgressAmount"></param>
        /// <returns></returns>
        public bool ProgressUserAchievement(uint UserId, string AchievementGroup, int ProgressAmount)
        {
            Habbo Habbo = UsersCache.getHabboCache(UserId);
            if (Habbo == null)
                return false;

            GameClient Session = Habbo.GetClient();

            Achievement AchievementData = GetAchievementByName(AchievementGroup);
            if (AchievementData == null)
            {
                return false;
            }

            UserAchievement UserData = Habbo.GetAchievementData(AchievementGroup);
            if (UserData == null)
            {
                UserData = new UserAchievement(AchievementGroup, 0, 0);
                Habbo.Achievements.Add(AchievementGroup, UserData);
            }

            uint TotalLevels = (uint)AchievementData.Levels.Length;
            if (UserData.Level == TotalLevels)
            {
                return false; // ya está todo completado
            }

            uint TargetLevel = UserData.Level + 1;
            if (TargetLevel > TotalLevels)
            {
                TargetLevel = TotalLevels;
            }

            uint TargetRequeriment = AchievementData.Levels[TargetLevel - 1];
            UserData.Progress += ProgressAmount;

            if (UserData.Progress >= TargetRequeriment)
            {
                UserData.Level++;

                if (TargetLevel == 1)
                    Habbo.GetBadgeComponent().GiveBadge(AchievementGroup + TargetLevel);
                else
                    Habbo.GetBadgeComponent().UpdateBadge(AchievementGroup + TargetLevel);

                Habbo.AchievementPoints += AchievementData.Reward;

                if (Session != null)
                {
                    Session.SendMessage(AchievementUnlockedComposer.Compose(AchievementData, TargetLevel, AchievementData.Reward));
                    Session.SendMessage(AchievementScoreUpdateComposer.Compose(Habbo.AchievementPoints));
                }
            }

            if (Session != null)
                Session.SendMessage(AchievementProgressComposer.Compose(AchievementData, UserData));

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("REPLACE INTO user_achievements VALUES (" + Habbo.Id + ", @group, " + UserData.Level + ", " + UserData.Progress + ")");
                dbClient.addParameter("group", AchievementGroup);
                dbClient.runQuery();
            }

            if (Session == null)
            {
                Habbo.saveBadges();
            }

            return true;
        }

        /// <summary>
        /// Resetea el progreso actual de una recompensa.
        /// </summary>
        /// <param name="Habbo"></param>
        /// <param name="AchievementGroup"></param>
        public void ResetAchievement(GameClient Session, string AchievementGroup)
        {
            Achievement AchievementData = GetAchievementByName(AchievementGroup);
            if (AchievementData == null)
            {
                return;
            }

            UserAchievement UserData = Session.GetHabbo().GetAchievementData(AchievementGroup);
            if (UserData == null)
            {
                return;
            }

            if (UserData.Level > 0)
                UserData.Progress = Convert.ToInt32(AchievementData.Levels[UserData.Level - 1]);
            else
                UserData.Progress = 0;
        }
    }
}