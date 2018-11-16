using Butterfly.Messages;
using HabboEvents;

namespace Butterfly.HabboHotel.Quests.Composer
{
    class QuestAbortedComposer
    {
        internal static ServerMessage Compose()
        {
            var CancelQuest = new ServerMessage(Outgoing.QuitAlertQ);
            CancelQuest.AppendBoolean(false);
            return CancelQuest;
        }
    }
}
