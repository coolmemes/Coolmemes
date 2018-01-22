using Butterfly.Messages;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Achievements.Composer
{
    class AchievementStatic
    {
        public static ServerMessage Compose(List<Achievement> Achievements)
        {
            ServerMessage Message = new ServerMessage(Outgoing.AchievementRequirement);
            Message.AppendInt32(Achievements.Count);
            foreach (Achievement Ach in Achievements)
            {
                Message.AppendString(Ach.AchievementName.Replace("ACH_", ""));
                Message.AppendInt32(Ach.Levels.Length);
                for (int i = 0; i < Ach.Levels.Length; i++)
                {
                    Message.AppendInt32(i + 1);
                    Message.AppendUInt(Ach.Levels[i]);
                }
            }
            return Message;
        }
    }
}
