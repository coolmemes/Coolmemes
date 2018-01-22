using System;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class ScoreAchieved : IWiredTrigger 
    {
        private RoomItem item;
        private WiredHandler handler;
        private int scoreLevel;
        private bool used;
        private readonly TeamScoreChangedDelegate scoreChangedDelegate;
        private readonly RoomEventDelegate gameEndDeletgate;

        public ScoreAchieved(RoomItem item, WiredHandler handler, int scoreLevel, GameManager gameManager)
        {
            this.item = item;
            this.handler = handler;
            this.scoreLevel = scoreLevel;
            used = false;
            scoreChangedDelegate = gameManager_OnScoreChanged;
            gameEndDeletgate = gameManager_OnGameEnd;

            gameManager.OnScoreChanged += scoreChangedDelegate;
            gameManager.OnGameEnd += gameEndDeletgate;
        }

        public int Score
        {
            get
            {
                return scoreLevel;
            }
        }

        private void gameManager_OnGameEnd(object sender, EventArgs e)
        {
            used = false;
        }

        private void gameManager_OnScoreChanged(object sender, TeamScoreChangedArgs e)
        {
            if (e.Points >= scoreLevel && !used)
            {
                used = true;
                handler.RequestStackHandle(item, null, e.user, e.Team);
            }
        }

        public void Dispose()
        {
            handler.GetRoom().GetGameManager().OnScoreChanged -= scoreChangedDelegate;
            handler.GetRoom().GetGameManager().OnGameEnd -= gameEndDeletgate;
            item = null;
            handler = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = scoreLevel.ToString()+ ";;false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }
    }
}
