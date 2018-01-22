using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Navigators.Bonus
{
    class BonusManager
    {
        private static uint ItemId;
        private static string ItemName;
        private static int SpriteId = -1;
        private static uint Amount;

        public static void Initialize(IQueryAdapter dbClient)
        {
            dbClient.setQuery("SELECT * FROM bonus_bag WHERE enable = '1' ORDER BY id DESC LIMIT 1");
            DataRow dRow = dbClient.getRow();

            if (dRow != null)
            {
                Item Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(Convert.ToUInt32(dRow["item_id"]));
                if (Item != null)
                {
                    ItemId = Item.ItemId;
                    ItemName = Item.Name;
                    SpriteId = Item.SpriteId;
                }
                Amount = Convert.ToUInt32(dRow["amount"]);
            }
        }

        public static ServerMessage GenerateMessage(GameClient Session)
        {
            if (Session.GetHabbo().CoinsPurchased >= Amount)

            ExchangeCoins(Session);

            ServerMessage Message = new ServerMessage(Outgoing.BonusBag);
            Message.AppendString(ItemName);
            Message.AppendInt32(SpriteId);
            Message.AppendUInt(Amount);
            Message.AppendUInt(((int)Amount - (int)Session.GetHabbo().CoinsPurchased) < 0 ? 0 : Amount - Session.GetHabbo().CoinsPurchased);
            return Message;
        }

        public static void ExchangeCoins(GameClient Session)
        {
            if (Session.GetHabbo().CoinsPurchased >= Amount)
            return;

            if (Amount > Session.GetHabbo().CoinsPurchased)
            return;

            Item Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(ItemId);
            if (Item == null)
                return;
           
            OtanixEnvironment.GetGame().GetCatalog().DeliverItems(Session, Item, Session.GetHabbo().CoinsPurchased / Amount, "", false);
            Session.GetHabbo().CoinsPurchased -= (Session.GetHabbo().CoinsPurchased / Amount) * Amount;
            Session.GetHabbo().UpdateExtraMoneyBalance();

             Session.SendWindowManagerAlert("Você ganhou um " + Item.Name + ". Parabens!");
        }
    }
}
