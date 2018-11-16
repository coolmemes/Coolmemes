using Butterfly.Messages;
using Butterfly.HabboHotel.GameClients;
using HabboEvents;

namespace Butterfly.HabboHotel.Quests.Composer
{
    class QuestCompletedComposer
    {
        internal static ServerMessage Compose(GameClient Session, Quest Quest)
        {
            var Message = new ServerMessage(Outgoing.CompleteQuests);
            QuestListComposer.SerializeQuest(Message, Session, Quest, Quest.Category);
            Message.AppendBoolean(true);
            return Message;
        }
    }
}
