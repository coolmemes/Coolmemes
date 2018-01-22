using System.Collections.Generic;
using System.Data;
using System.Linq;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Quests.Composer;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database;
using Database_Manager.Database.Session_Details.Interfaces;
using System;

namespace Butterfly.HabboHotel.Quests
{
    class QuestManager
    {
        private Dictionary<uint, Quest> quests;
        private Dictionary<string, int> questCount;

        public QuestManager()
        { }

        public void Initialize(IQueryAdapter dbClient)
        {
            quests = new Dictionary<uint, Quest>();
            questCount = new Dictionary<string, int>();

            ReloadQuests(dbClient);
        }

        public void ReloadQuests(IQueryAdapter dbClient)
        {
            quests.Clear();

            dbClient.setQuery("SELECT * FROM quests");
            var dTable = dbClient.getTable();

            uint id;
            string category;
            int num;
            int type;
            uint goalData;
            string name;
            string dataBit;

            foreach (DataRow dRow in dTable.Rows)
            {
                id = Convert.ToUInt32(dRow["id"]);
                category = (string)dRow["category"];
                num = (int)dRow["series_number"];
                type = (int)dRow["goal_type"];
                goalData = Convert.ToUInt32(dRow["goal_data"]);
                name = (string)dRow["name"];
                dataBit = (string)dRow["data_bit"];

                var quest = new Quest(id, category, num, (QuestType)type, goalData, name, dataBit);
                quests.Add(id, quest);
                AddToCounter(category);
            }
        }

        private void AddToCounter(string category)
        {
            int count;
            if (questCount.TryGetValue(category, out count))
            {
                questCount[category] = count + 1;
            }
            else
            {
                questCount.Add(category, 1);
            }
        }

        internal Quest GetQuest(uint Id)
        {
            Quest quest;
            quests.TryGetValue(Id, out quest);
            return quest;
        }

        internal int GetAmountOfQuestsInCategory(string Category)
        {
            int count;
            questCount.TryGetValue(Category, out count);
            return count;
        }

        internal void ProgressUserQuest(GameClient Session, QuestType QuestType, uint EventData = 0)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentQuestId <= 0)
            {
                return;
            }

            var UserQuest = GetQuest(Session.GetHabbo().CurrentQuestId);

            if (UserQuest == null || UserQuest.GoalType != QuestType)
            {
                return;
            }

            var CurrentProgress = Session.GetHabbo().GetQuestProgress(UserQuest.Id);
            var NewProgress = CurrentProgress;
            var PassQuest = false;

            switch (QuestType)
            {
                default:

                    NewProgress++;

                    if (NewProgress >= UserQuest.GoalData)
                    {
                        PassQuest = true;
                    }

                    break;

                case QuestType.EXPLORE_FIND_ITEM:

                    if (EventData != UserQuest.GoalData)
                        return;

                    NewProgress = (int)UserQuest.GoalData;
                    PassQuest = true;
                    break;
            }

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE user_quests SET progress = " + NewProgress + " WHERE user_id = " + Session.GetHabbo().Id + " AND quest_id =  " + UserQuest.Id);
            }

            Session.GetHabbo().quests[Session.GetHabbo().CurrentQuestId] = NewProgress;
            Session.SendMessage(QuestStartedComposer.Compose(Session, UserQuest));

            if (PassQuest)
            {
                Session.GetHabbo().CurrentQuestId = 0;
                Session.GetHabbo().LastCompleted = UserQuest.Id;
                Session.SendMessage(QuestCompletedComposer.Compose(Session, UserQuest));
                GetList(Session, null);
            }
        }

        internal Quest GetNextQuestInSeries(string Category, int Number)
        {
            return quests.Values.FirstOrDefault(Quest => Quest.Category == Category && Quest.Number == Number);
        }

        internal void GetList(GameClient Session, ClientMessage Message)
        {
            Session.SendMessage(QuestListComposer.Compose(Session, quests.Values.ToList(), (Message != null)));
        }

        internal void ActivateQuest(GameClient Session, ClientMessage Message)
        {
            var Quest = GetQuest(Message.PopWiredUInt());

            if (Quest == null)
            {
                return;
            }

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {    
                dbClient.runFastQuery("REPLACE INTO user_quests VALUES (" + Session.GetHabbo().Id + ", " + Quest.Id + ", 0)");
            }

            Session.GetHabbo().CurrentQuestId = Quest.Id;
            GetList(Session, null);
            Session.SendMessage(QuestStartedComposer.Compose(Session, Quest));
        }

        internal void GetCurrentQuest(GameClient Session, ClientMessage Message)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var UserQuest = GetQuest(Session.GetHabbo().LastCompleted);
            var NextQuest = GetNextQuestInSeries(UserQuest.Category, UserQuest.Number + 1);

            if (NextQuest == null)
            {
                return;
            }

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("REPLACE INTO user_quests VALUES (" + Session.GetHabbo().Id + ", " + NextQuest.Id + ", 0)");
            }

            Session.GetHabbo().CurrentQuestId = NextQuest.Id;
            GetList(Session, null);
            Session.SendMessage(QuestStartedComposer.Compose(Session, NextQuest));
        }

        internal void CancelQuest(GameClient Session, ClientMessage Message)
        {
            var Quest = GetQuest(Session.GetHabbo().CurrentQuestId);

            if (Quest == null)
            {
                return;
            }

            Session.GetHabbo().CurrentQuestId = 0;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM user_quests WHERE user_id = " + Session.GetHabbo().Id + " AND quest_id = " + Quest.Id);
            }

            Session.SendMessage(QuestAbortedComposer.Compose());
            GetList(Session, null);
        }
    }
}