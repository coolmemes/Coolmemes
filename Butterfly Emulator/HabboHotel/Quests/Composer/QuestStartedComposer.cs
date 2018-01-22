using Butterfly.Messages;
using Butterfly.HabboHotel.GameClients;
using HabboEvents;

namespace Butterfly.HabboHotel.Quests.Composer
{
    class QuestStartedComposer
    {
        internal static ServerMessage Compose(GameClient Session, Quest Quest)
        {
            var Message = new ServerMessage(Outgoing.ActivateQuest);
            QuestListComposer.SerializeQuest(Message, Session, Quest, Quest.Category);
            return Message;
        }
    }
}
