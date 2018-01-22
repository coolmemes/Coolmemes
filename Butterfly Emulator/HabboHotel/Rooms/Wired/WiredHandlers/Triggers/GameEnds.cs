using System;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class GameEnds : IWiredTrigger 
    {
        private RoomItem item;
        private WiredHandler handler;
        private readonly RoomEventDelegate gameEndsDeletgate;

        public GameEnds(RoomItem item, WiredHandler handler, GameManager gameManager)
        {
            this.item = item;
            this.handler = handler;
            gameEndsDeletgate = gameManager_OnGameEnd;

            gameManager.OnGameEnd += gameEndsDeletgate;
        }

        private void gameManager_OnGameEnd(object sender, EventArgs e)
        {
            handler.RequestStackHandle(item, null, null, Team.none);
            //InteractorGenericSwitch.DoAnimation(item);
        }

        public void Dispose()
        {
            handler.GetRoom().GetGameManager().OnGameEnd -= gameEndsDeletgate;
            item = null;
            handler = null;
        }


        public void SaveToDatabase(QueryChunk wiredInserts)
        {
        }
    }
}
