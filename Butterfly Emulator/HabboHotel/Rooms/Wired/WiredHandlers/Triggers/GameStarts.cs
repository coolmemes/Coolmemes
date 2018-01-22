using System;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class GameStarts : IWiredTrigger 
    {
        private RoomItem item;
        private WiredHandler handler;
        private readonly RoomEventDelegate gameStartsDeletgate;

        public GameStarts(RoomItem item, WiredHandler handler, GameManager gameManager)
        {
            this.item = item;
            this.handler = handler;
            gameStartsDeletgate = gameManager_OnGameStart;

            gameManager.OnGameStart += gameStartsDeletgate;
        }

        private void gameManager_OnGameStart(object sender, EventArgs e)
        {
            handler.RequestStackHandle(item, null, null, Team.none);
            //InteractorGenericSwitch.DoAnimation(item);
        }

        public void Dispose()
        {
            handler.GetRoom().GetGameManager().OnGameStart -= gameStartsDeletgate;
            item = null;
            handler = null;
        }


        public void SaveToDatabase(QueryChunk wiredInserts)
        {
        }
    }
}
