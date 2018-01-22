using System;
using Butterfly.Messages;
using HabboEvents;

namespace Butterfly.HabboHotel.Achievements.Composer
{
    class AchievementProgressComposer
    {
        internal static ServerMessage Compose(Achievement Achievement, UserAchievement UserData)
        {
            uint TargetLevel = (UserData != null ? UserData.Level + 1 : 1);
            int TotalLevels = Achievement.Levels.Length;

            if (TargetLevel > TotalLevels)
                TargetLevel = (uint)TotalLevels;

            uint TargetRequirement = Achievement.Levels[TargetLevel - 1];

            uint PreviuosRequeriment = 0;
            if (TargetLevel > 1)
                PreviuosRequeriment = Achievement.Levels[TargetLevel - 2];

            var Message = new ServerMessage(Outgoing.AchievementProgress);
            Message.AppendUInt(Achievement.Id);
            Message.AppendUInt(TargetLevel);
            Message.AppendString(Achievement.AchievementName + TargetLevel);
            Message.AppendUInt(PreviuosRequeriment);
            Message.AppendUInt(TargetRequirement);
            Message.AppendInt32(0);
            Message.AppendUInt(Achievement.Reward);
            Message.AppendInt32(UserData != null ? UserData.Progress : 0);
            Message.AppendBoolean(UserData != null ? TargetLevel == TotalLevels && UserData.Progress >= TargetRequirement : false);
            Message.AppendString(Achievement.Category);
            Message.AppendString(String.Empty);
            Message.AppendInt32(TotalLevels);
            Message.AppendInt32(0);
            return Message;
        }
    }
}
