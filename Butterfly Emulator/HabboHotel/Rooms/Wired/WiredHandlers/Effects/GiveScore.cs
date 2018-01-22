using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Items;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class GiveScore : IWiredEffect, IWiredTrigger
    {
        private int maxCountPerGame;
        private int currentGameCount;
        private int scoreToGive;
        // private GameManager gameManager;
        private GameManager gameManager;
        private RoomEventDelegate delegateFunction;
        private RoomItem itemID;
        
        public GiveScore(int maxCountPerGame, int scoreToGive, GameManager gameManager, RoomItem itemID)
        {
            this.maxCountPerGame = maxCountPerGame;
            currentGameCount = 0;
            this.scoreToGive = scoreToGive;
            delegateFunction = gameManager_OnGameStart;
            this.gameManager = gameManager;
            this.itemID = itemID;

            gameManager.OnGameStart += delegateFunction;
        }

        public int ScoreToGive
        {
            get
            {
                return scoreToGive;
            }
        }

        public int MaxCountPerGame
        {
            get
            {
                return maxCountPerGame;
            }
        }

        private void gameManager_OnGameStart(object sender, EventArgs e)
        {
            currentGameCount = 0;
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            if (team != Team.none && maxCountPerGame > currentGameCount)
            {
                currentGameCount++;
                gameManager.AddPointsToTeam(team, scoreToGive, user);

                if (user.team != Team.none)
                    user.classPoints += (uint)scoreToGive;
            }
        }

        public void Dispose()
        {
            gameManager.OnGameStart -= delegateFunction;
            gameManager = null;
            delegateFunction = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = scoreToGive.ToString() + ";" + maxCountPerGame.ToString() + ";false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID.Id + "', @data" + itemID.Id + ", @to_item" + itemID.Id + ", @original_location" + itemID.Id + ")");
            wiredInserts.AddParameter("data" + itemID.Id, wired_data);
            wiredInserts.AddParameter("to_item" + itemID.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID.Id, wired_original_location);
        }
    }
}
