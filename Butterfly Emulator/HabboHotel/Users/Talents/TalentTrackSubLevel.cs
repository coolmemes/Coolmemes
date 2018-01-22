using Butterfly.HabboHotel.Achievements;
using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Talents
{
    class TalentTrackSubLevel
    {
        internal uint AchievementId;
        internal int AchievementLevel;
        internal string AchievementName;

        internal TalentTrackSubLevel(uint _AchievementId, int _AchievementLevel)
        {
            this.AchievementId = _AchievementId;
            this.AchievementLevel = _AchievementLevel;
            this.AchievementName = OtanixEnvironment.GetGame().GetAchievementManager().GetAchievementById(this.AchievementId).AchievementName;
        }

        internal string AchievementTotalName
        {
            get
            {
                return AchievementName + AchievementLevel;
            }
        }

        internal void Serialize(ServerMessage Message, GameClient Session, bool ValidLevel)
        {
            int SubTalentValue = GetSubTalentValue(Session, AchievementName, ValidLevel);

            Message.AppendUInt(AchievementId);
            Message.AppendInt32(AchievementLevel);
            Message.AppendString(AchievementTotalName);
            Message.AppendInt32(SubTalentValue); // enabled (0 = null, 1 = enabled, 2 = passed) ??

            try
            {
                Message.AppendUInt((SubTalentValue == 2) ? OtanixEnvironment.GetGame().GetAchievementManager().GetAchievementByName(AchievementName).Levels[(uint)AchievementLevel - 1] : (uint)Session.GetHabbo().GetAchievementData(AchievementName).Progress);
                Message.AppendUInt(OtanixEnvironment.GetGame().GetAchievementManager().GetAchievementByName(AchievementName).Levels[(uint)AchievementLevel - 1]);
            }
            catch
            {
                Message.AppendUInt(0);
                Message.AppendUInt(0);
            }
        }

        internal Int32 GetSubTalentValue(GameClient Session, string AchievementName, bool ValidLevel)
        {
            if (!ValidLevel)
                return 0;

            UserAchievement userACH = Session.GetHabbo().GetAchievementData(AchievementName);
            if (userACH != null)
            {
                if (OtanixEnvironment.GetGame().GetAchievementManager().GetAchievementByName(AchievementName).Levels[AchievementLevel - 1] <= userACH.Progress)
                    return 2;
            }

            return 1;
        }
    }
}
