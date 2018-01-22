using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class GiveTeamScore : IWiredEffect, IWiredTrigger
    {
        private int maxCountPerGame;
        private int currentGameCount;
        private int scoreToGive;
        private Team staticTeam;
        private GameManager gameManager;
        private RoomEventDelegate delegateFunction;
        private RoomItem itemID;

        public GiveTeamScore(int maxCountPerGame, int scoreToGive, Team _staticTeam, GameManager gameManager, RoomItem itemID)
        {
            this.maxCountPerGame = maxCountPerGame;
            this.currentGameCount = 0;
            this.scoreToGive = scoreToGive;
            this.delegateFunction = gameManager_OnGameStart;
            this.gameManager = gameManager;
            this.itemID = itemID;
            this.staticTeam = _staticTeam;

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

        public Team Team
        {
            get
            {
                return staticTeam;
            }
        }

        private void gameManager_OnGameStart(object sender, EventArgs e)
        {
            currentGameCount = 0;
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            if (team == staticTeam && maxCountPerGame > currentGameCount)
            {
                //InteractorGenericSwitch.DoAnimation(itemID);
                currentGameCount++;
                gameManager.AddPointsToTeam(team, scoreToGive, user);
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
            string wired_data = scoreToGive.ToString() + "," + maxCountPerGame.ToString() + ";" + ((int)staticTeam).ToString() + ";false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID.Id + "', @data" + itemID.Id + ", @to_item" + itemID.Id + ", @original_location" + itemID.Id + ")");
            wiredInserts.AddParameter("data" + itemID.Id, wired_data);
            wiredInserts.AddParameter("to_item" + itemID.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID.Id, wired_original_location);
        }
    }
}
