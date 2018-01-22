using Butterfly.Messages;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Achievements.Composer
{
    class AchievementUnlockedComposer
    {
        internal static ServerMessage Compose(Achievement Achievement, uint Level, uint PointReward)
        {
            ServerMessage Message = new ServerMessage(Outgoing.UnlockAchievement);
            Message.AppendUInt(Achievement.Id);
            Message.AppendUInt(Level);
            Message.AppendInt32(0); // ??
            Message.AppendString(Achievement.AchievementName + Level);
            Message.AppendUInt(PointReward);
            Message.AppendInt32(0); // ??
            Message.AppendInt32(0); // ??
            Message.AppendInt32(10); // ??
            Message.AppendInt32(0); // ??
            Message.AppendString(Level > 1 ? Achievement.AchievementName + (Level - 1) : string.Empty);
            Message.AppendString(Achievement.Category);
            Message.AppendBoolean(false); // not used.
            return Message;
        }
    }
}
