using System.Collections.Generic;
using Butterfly.Messages;
using Butterfly.HabboHotel.GameClients;
using ButterStorm;
using HabboEvents;

namespace Butterfly.HabboHotel.Quests.Composer
{
    enum QuestRewardType
    {
        Pixels = 0,
        Snowflakes = 1,
        Love = 2,
        PixelsBROKEN = 3,
        Seashells = 4
    }

    class QuestListComposer
    {
        internal static ServerMessage Compose(GameClient Session, List<Quest> Quests, bool Send)
        {
            var UserQuestGoals = new Dictionary<string, int>();
            var UserQuests = new Dictionary<string, Quest>();

            foreach (var Quest in Quests)
            {
                if (!UserQuestGoals.ContainsKey(Quest.Category))
                {
                    UserQuestGoals.Add(Quest.Category, 1);
                    UserQuests.Add(Quest.Category, null);
                }

                if (Quest.Number >= UserQuestGoals[Quest.Category])
                {
                    var UserProgress = Session.GetHabbo().GetQuestProgress(Quest.Id);

                    if (Session.GetHabbo().CurrentQuestId != Quest.Id && UserProgress >= Quest.GoalData)
                    {
                        UserQuestGoals[Quest.Category] = Quest.Number + 1;
                    }
                }
            }

            foreach (var Quest in Quests)
            {
                foreach (var Goal in UserQuestGoals)
                {
                    if (Quest.Category == Goal.Key && Quest.Number == Goal.Value)
                    {
                        UserQuests[Goal.Key] = Quest;
                        break;
                    }
                }
            }

            var Message = new ServerMessage(Outgoing.LoadQuests);
            Message.AppendInt32(UserQuests.Count);

            // Active ones first
            foreach (var UserQuest in UserQuests)
            {
                if (UserQuest.Value == null)
                {
                    continue;
                }

                SerializeQuest(Message, Session, UserQuest.Value, UserQuest.Key);
            }

            // Dead ones last
            foreach (var UserQuest in UserQuests)
            {
                if (UserQuest.Value != null)
                {
                    continue;
                }

                SerializeQuest(Message, Session, UserQuest.Value, UserQuest.Key);
            }

            Message.AppendBoolean(Send);
            return Message;
        }

        internal static void SerializeQuest(ServerMessage Message, GameClient Session, Quest Quest, string Category)
        {
            var AmountInCat = OtanixEnvironment.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(Category);
            var Number = Quest == null ? AmountInCat : Quest.Number - 1;
            var UserProgress = Quest == null ? 0 : Session.GetHabbo().GetQuestProgress(Quest.Id);

            if (Quest != null && Quest.IsCompleted(UserProgress))
            {
                Number++;
            }

            Message.AppendString(Category);
            Message.AppendInt32(Number); // Quest progress in this cat
            Message.AppendInt32(AmountInCat); // Total quests in this cat
            Message.AppendInt32((int)QuestRewardType.Pixels); // Reward type (1 = Snowflakes, 2 = Love hearts, 3 = Pixels, 4 = Seashells, everything else is pixels
            Message.AppendUInt(Quest == null ? 0 : Quest.Id); // Quest id
            Message.AppendBoolean(Quest != null && Session.GetHabbo().CurrentQuestId == Quest.Id); // Quest started
            Message.AppendString(Quest == null ? string.Empty : Quest.ActionName);
            Message.AppendString(Quest == null ? string.Empty : Quest.DataBit);
            Message.AppendInt32(0);
            Message.AppendString(Quest == null ? string.Empty : Quest.Name);
            Message.AppendInt32(UserProgress); // Current progress
            Message.AppendUInt(Quest == null ? 0 : Quest.GoalData); // Target progress
            Message.AppendInt32(GetIntValue(Category)); // "Next quest available countdown" in seconds
            Message.AppendString("set_kuurna");
            Message.AppendString("MAIN_CHAIN");
            Message.AppendBoolean(true);
        }

        internal static int GetIntValue(string QuestCategory)
        {
            switch (QuestCategory)
            {
                case "room_builder":
                    return 2;
                case "social":
                    return 3;
                case "identity":
                    return 4;
                case "explore":
                    return 5;
                case "battleball":
                    return 7;
                case "freeze":
                    return 8;
                default:
                    return 0;
            }
        }
    }
}
