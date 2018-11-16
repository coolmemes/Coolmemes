using System;
using System.Data;
using System.Text;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database;
using HabboEvents;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Items.Core;
using Butterfly.Core;
using System.Collections.Generic;

namespace Butterfly.HabboHotel.Catalogs
{
    class Marketplace
    {
        private static string WhereString = "state = '1' AND timestamp >= " + OtanixEnvironment.GetUnixTimestamp();

        internal static Boolean CanSellItem(UserItem Item)
        {
            if (!Item.mBaseItem.AllowTrade || !Item.mBaseItem.AllowMarketplaceSell)
            {
                return false;
            }

            return true;
        }

        internal static void SellItem(GameClient Session, UserItem Item, uint SellingPrice)
        {
            if (Item == null || SellingPrice > 10000 || !CanSellItem(Item))
            {
                Session.GetMessageHandler().GetResponse().Init(Outgoing.MarketConfirmPost);
                Session.GetMessageHandler().GetResponse().AppendInt32(2);
                Session.GetMessageHandler().SendResponse();
                return;
            }

            var Comission = CalculateComissionPrice(SellingPrice);
            var TotalPrice = SellingPrice + Comission;
            var ItemType = 1;

            if (Item.mBaseItem.Type == 'i')
                ItemType++;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO catalog_marketplace_offers (item_id,user_id,asking_price,total_price,public_name,item_type,timestamp,extra_data) VALUES (" + Item.BaseItem + "," + Session.GetHabbo().Id + "," + SellingPrice + "," + TotalPrice + ",@public_name," + ItemType + "," + (OtanixEnvironment.GetUnixTimestamp() + 172800) + ",@extra_data)");
                dbClient.addParameter("public_name", Furnidata.GetPublicNameByItemName(Item.mBaseItem.Name));
                dbClient.addParameter("extra_data", Item.ExtraData);
                dbClient.runQuery();
            }

            Session.GetMessageHandler().GetResponse().Init(Outgoing.MarketConfirmPost);
            Session.GetMessageHandler().GetResponse().AppendInt32(1);
            Session.GetMessageHandler().SendResponse();

            Session.GetHabbo().GetInventoryComponent().RemoveItem(Item.Id, true);
            Session.GetHabbo().GetInventoryComponent().RunDBUpdate();
        }

        internal static int CalculateComissionPrice(float SellingPrice)
        {
            return (int)Math.Ceiling(SellingPrice / 100);
        }

        internal static ServerMessage SerializeStatistics(int junk, uint SpriteId)
        {
            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            double UnixActualDay = OtanixEnvironment.DateTimeToUnixTimestamp(dt);
            double Unix30DaysAgo = UnixActualDay - (86400 * 30);

            uint ItemId = OtanixEnvironment.GetGame().GetItemManager().GetBaseIdFromSpriteId(SpriteId);

            uint Average = 0;
            uint Count = 0;
            GetAverageAndCountFromBaseId(ItemId, out Average, out Count);

            DataTable dTable = null;

            ServerMessage Message = new ServerMessage(Outgoing.MarketSpriteItemStatics);
            Message.AppendUInt(Average); // average
            Message.AppendUInt(Count); // count (Número de ofertas)
            Message.AppendInt32(30); // days (Precio medio de venta en los últimos X días)

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT date, price_avg, trade_count FROM catalog_marketplace_statistics WHERE item_id = '" + ItemId + "' AND date > '" + Unix30DaysAgo + "'");
                dTable = dbClient.getTable();
            }

            Message.AppendInt32(dTable.Rows.Count); // foreach

            foreach (DataRow dRow in dTable.Rows)
            {
                DateTime rowTime = OtanixEnvironment.UnixTimeStampToDateTime((double)dRow["date"]);

                Message.AppendInt32((rowTime - dt).Days); // day(-29, -28... 0)
                Message.AppendUInt(Convert.ToUInt32(dRow["price_avg"])); // Evolución de los precios
                Message.AppendUInt(Convert.ToUInt32(dRow["trade_count"])); // Volumen de tradeos
            }

            Message.AppendInt32(junk);
            Message.AppendUInt(SpriteId);
            return Message;
        }

        internal static ServerMessage SerializeOffers(GameClient Session, int MinCost, int MaxCost, String SearchQuery, int FilterMode)
        {
            var Data = new DataTable();
            var WhereClause = new StringBuilder();
            var OrderMode = "";

            WhereClause.Append("WHERE " + WhereString);

            if (MinCost > 0)
            {
                WhereClause.Append(" AND total_price >= " + MinCost);
            }

            if (MaxCost > 0)
            {
                WhereClause.Append(" AND total_price <= " + MaxCost);
            }

            switch (FilterMode)
            {
                case 1:

                    // objetos más caros
                    OrderMode = " ORDER BY total_price DESC";
                    break;

                case 2:

                    // objetos más baratos
                    OrderMode = " ORDER BY total_price ASC";
                    break;

                case 3:

                    WhereClause.Append(" AND items_traded_logs.item_id = catalog_marketplace_offers.item_id");
                    OrderMode = " GROUP BY item_id ORDER BY COUNT(item_id) DESC";
                    // objetos más tradeados
                    break;

                case 4:

                    WhereClause.Append(" AND items_traded_logs.item_id = catalog_marketplace_offers.item_id");
                    OrderMode = " GROUP BY item_id ORDER BY COUNT(item_id) ASC";
                    // objetos menos tradeados
                    break;

                case 5:

                    // objetos con más ofertas
                    OrderMode = " GROUP BY item_id ORDER BY COUNT(item_id) DESC";
                    break;

                case 6:

                    // objetos con menos ofertas
                    OrderMode = " GROUP BY item_id ORDER BY COUNT(item_id) ASC";
                    break;
            }

            if (SearchQuery.Length >= 1)
                WhereClause.Append(" AND public_name LIKE @search_query");

            if (FilterMode == 3 || FilterMode == 4)
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT DISTINCT item_id FROM items_traded_logs WHERE date = '" + DateTime.Now.ToShortDateString() + "' AND EXISTS(SELECT item_id FROM catalog_marketplace_offers " + WhereClause + ")" + OrderMode);
                    dbClient.addParameter("search_query", "%" + SearchQuery + "%");

                    Data = dbClient.getTable();
                }
            }
            else
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT DISTINCT item_id FROM catalog_marketplace_offers " + WhereClause + OrderMode + " LIMIT 100");
                    dbClient.addParameter("search_query", "%" + SearchQuery + "%");

                    Data = dbClient.getTable();
                }
            }

            ServerMessage Message = new ServerMessage(Outgoing.MarketSerializeOffers);

            if (Data != null)
            {
                Message.AppendInt32(Data.Rows.Count);

                foreach (DataRow dRow in Data.Rows)
                {
                    uint BaseId = Convert.ToUInt32(dRow["item_id"]);
                    Item Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(BaseId);
                    if (Item == null)
                        return null;

                    DataRow Row = GetRowFromBaseId(BaseId);
                    if (Row == null)
                        return null;

                    int MinutesLeft = Convert.ToInt32(((Double)Row["timestamp"] - OtanixEnvironment.GetUnixTimestamp()) / 60);
                    uint Average = 0;
                    uint Count = 0;
                    GetAverageAndCountFromBaseId(BaseId, out Average, out Count);

                    Message.AppendUInt(Convert.ToUInt32(Row["offer_id"]));              // offerId
                    Message.AppendInt32(1);                                             // state: 1=active, 2=sold, 3=expired
                    Message.AppendInt32(int.Parse(Row["item_type"].ToString()));        // itemType: (1)(2)(3)
                    switch (int.Parse(Row["item_type"].ToString()))
                    {
                        case 1:
                            {
                                Message.AppendInt32(Item.SpriteId);                     // spriteId
                                Message.AppendInt32(0);                                 // Depende de este valor indicará un tipo de furni u otro.
                                Message.AppendString((string)Row["extra_data"]);
                                break;
                            }

                        case 2:
                            {
                                Message.AppendInt32(Item.SpriteId);                     // spriteId
                                Message.AppendString("");                               // poster_X_name
                                break;
                            }

                        case 3:
                            {
                                Message.AppendInt32(Item.SpriteId);                     // spriteId
                                Message.AppendInt32(0);
                                Message.AppendInt32(0);
                                break;
                            }
                    }

                    Message.AppendInt32((int)Row["total_price"]);                       // precio
                    Message.AppendInt32(MinutesLeft); // ??
                    Message.AppendUInt(Average); // Avg
                    Message.AppendUInt(Count);   // Offers
                }

                Message.AppendInt32(Data.Rows.Count);
            }
            else
            {
                Message.AppendInt32(0);
                Message.AppendInt32(0);
            }

            return Message;
        }

        internal static ServerMessage SerializeOwnOffers(uint HabboId)
        {
            var Profits = 0;
            DataTable Data;
            String RawProfit;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT item_id, timestamp, state, offer_id, item_type, total_price, extra_data FROM catalog_marketplace_offers WHERE user_id = " + HabboId);
                Data = dbClient.getTable();

                dbClient.setQuery("SELECT SUM(asking_price) FROM catalog_marketplace_offers WHERE state = '2' AND user_id = " + HabboId);
                RawProfit = dbClient.getRow()[0].ToString();
            }

            if (Data == null)
                return null;

            if (RawProfit.Length > 0)
                Profits = int.Parse(RawProfit);

            ServerMessage Message = new ServerMessage(Outgoing.MarketGetOwnOffers);
            Message.AppendInt32(Profits);
            if (Data != null)
            {
                Message.AppendInt32(Data.Rows.Count);
                foreach (DataRow Row in Data.Rows)
                {
                    uint BaseId = Convert.ToUInt32(Row["item_id"]);
                    Item Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(BaseId);
                    if (Item == null)
                        return null;

                    uint Average = 0;
                    uint Count = 0;
                    GetAverageAndCountFromBaseId(BaseId, out Average, out Count);

                    int MinutesLeft = Convert.ToInt32(((Double)Row["timestamp"] - OtanixEnvironment.GetUnixTimestamp()) / 60);
                    int state = int.Parse(Row["state"].ToString());
                    int type = int.Parse(Row["item_type"].ToString());

                    if (MinutesLeft <= 0)
                    {
                        state = 3;
                        MinutesLeft = 0;
                    }

                    Message.AppendUInt(Convert.ToUInt32(Row["offer_id"]));              // offerId
                    Message.AppendInt32(state);                                         // state: 1=active, 2=sold, 3=expired
                    Message.AppendInt32(type);                                          // itemType: (1)(2)(3)
                    switch (type)
                    {
                        case 1:
                            {
                                Message.AppendInt32(Item.SpriteId);                     // spriteId
                                Message.AppendInt32(0);                                 // Depende de este valor indicará un tipo de furni u otro.
                                Message.AppendString((string)Row["extra_data"]);
                                break;
                            }

                        case 2:
                            {
                                Message.AppendInt32(Item.SpriteId);                     // spriteId
                                Message.AppendString("");                               // poster_X_name
                                break;
                            }

                        case 3:
                            {
                                Message.AppendInt32(Item.SpriteId);                     // spriteId
                                Message.AppendInt32(0);
                                Message.AppendInt32(0);
                                break;
                            }
                    }

                    Message.AppendInt32((int)Row["total_price"]);                       // precio
                    Message.AppendInt32(MinutesLeft);                                   // tiempo restante en minutos
                    Message.AppendUInt(Average);                                        // precio media
                }
            }
            else
            {
                Message.AppendInt32(0);
            }

            return Message;
        }

        private static DataRow GetRowFromBaseId(uint BaseId)
        {
            DataRow dRow = null;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT offer_id, item_type, total_price, extra_data, timestamp FROM catalog_marketplace_offers WHERE item_id = '" + BaseId + "' AND " + WhereString + " ORDER BY total_price ASC LIMIT 1");
                dRow = dbClient.getRow();
            }

            return dRow;
        }

        private static void GetAverageAndCountFromBaseId(uint BaseId, out uint Average, out uint Count)
        {
            DataRow dRow = null;
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT SUM(total_price), COUNT(total_price) FROM catalog_marketplace_offers WHERE item_id = '" + BaseId + "' AND " + WhereString);
                dRow = dbClient.getRow();
            }

            try
            {
                if (dRow != null)
                {
                    Average = Convert.ToUInt32(dRow[0]) / Convert.ToUInt32(dRow[1]);
                    Count = Convert.ToUInt32(dRow[1]);
                }
                else
                {
                    Average = 0;
                    Count = 0;
                }
            }
            catch
            {
                Average = 0;
                Count = 0;
            }
        }

        internal static void AddItemToStatistics(uint BaseId, uint Price, IQueryAdapter dbClient)
        {
            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            dbClient.setQuery("SELECT price_avg FROM catalog_marketplace_statistics WHERE item_id = '" + BaseId + "' AND date = '" + OtanixEnvironment.DateTimeToUnixTimestamp(dt) + "'");
            uint price_avg = Convert.ToUInt32(dbClient.getInteger());

            if (price_avg > 0)
            {
                price_avg = ((price_avg + Price) / 2);
                dbClient.runFastQuery("UPDATE catalog_marketplace_statistics SET price_avg = '" + price_avg + "', trade_count = trade_count + 1 WHERE item_id = '" + BaseId + "' AND date = '" + OtanixEnvironment.DateTimeToUnixTimestamp(dt) + "'");
            }
            else
            {
                dbClient.runFastQuery("INSERT INTO catalog_marketplace_statistics VALUES (NULL,'" + BaseId + "','" + OtanixEnvironment.DateTimeToUnixTimestamp(dt) + "','" + Price + "','1')");
            }
        }
    }
}
