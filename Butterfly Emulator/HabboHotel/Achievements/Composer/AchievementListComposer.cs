using System;
using System.Collections.Generic;
using Butterfly.Messages;
using Butterfly.HabboHotel.GameClients;
using HabboEvents;
namespace Butterfly.HabboHotel.Achievements.Composer
{
    class AchievementListComposer
    {
        public static ServerMessage Compose(GameClient Session, List<Achievement> Achievements)
        {
            ServerMessage Message = new ServerMessage(Outgoing.AchievementList);
            Message.AppendInt32(Achievements.Count);
            foreach (Achievement Achievement in Achievements)
            {

                UserAchievement UserData = Session.GetHabbo().GetAchievementData(Achievement.AchievementName);
                uint TargetLevel = (UserData != null ? UserData.Level + 1 : 1);
                int TotalLevels = Achievement.Levels.Length;

                if (TargetLevel > TotalLevels)
                    TargetLevel = (uint)TotalLevels;

                uint TargetRequirement = Achievement.Levels[TargetLevel - 1];

                uint PreviuosRequeriment = 0;
                if (TargetLevel > 1)
                    PreviuosRequeriment = Achievement.Levels[TargetLevel - 2];

                Message.AppendUInt(Achievement.Id); // achievementId
                Message.AppendUInt(TargetLevel); // level
                Message.AppendString(Achievement.AchievementName + TargetLevel); // badgeId
                Message.AppendUInt(PreviuosRequeriment); // Requerimiento anterior
                Message.AppendUInt(TargetRequirement); // Requerimiento actual    
                Message.AppendInt32(0); // Coins de recompensa
                Message.AppendUInt(Achievement.Reward); // Puntos de recompensa
                Message.AppendInt32(UserData != null ? UserData.Progress : 0); // Current progress
                Message.AppendBoolean(UserData != null ? TargetLevel == TotalLevels && UserData.Progress >= TargetRequirement : false);  // Set 100% completed(??)
                Message.AppendString(Achievement.Category); // Category
                Message.AppendString(String.Empty);
                Message.AppendInt32(TotalLevels); // Total amount of levels 
                Message.AppendInt32(0);
            }
            Message.AppendString("");
            return Message;
        }
    }
}
