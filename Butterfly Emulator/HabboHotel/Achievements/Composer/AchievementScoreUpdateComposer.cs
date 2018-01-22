using Butterfly.Messages;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Achievements.Composer
{
    class AchievementScoreUpdateComposer
    {
        public static ServerMessage Compose(uint AchievementPoints)
        {
            ServerMessage Message = new ServerMessage(Outgoing.AchievementPoints);
            Message.AppendUInt(AchievementPoints);
            return Message;
        }
    }
}
