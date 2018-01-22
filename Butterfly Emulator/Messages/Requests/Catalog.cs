using System;
using System.Collections.Generic;
using System.Data;
using Butterfly.HabboHotel.Catalogs;
using Butterfly.Core;
using Butterfly.HabboHotel.Pets;
using Butterfly.HabboHotel.Rooms;
using ButterStorm;
using HabboEvents;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Camera;
using Butterfly.HabboHotel.Premiums;
using Butterfly.HabboHotel.Premiums.Catalog;

namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        internal void GetCatalogIndex()
        {
            string CatalogType = Request.PopFixedString();
            if (CatalogType.Equals("NORMAL"))
            {
                Session.SendMessage(OtanixEnvironment.GetGame().GetCatalog().GetIndexMessageForRank(Session.GetHabbo().Rank));
            }
            else if(CatalogType.Equals("BUILDERS_CLUB"))
            {
                Session.SendMessage(PremiumManager.SerializePremiumItemsCount(Session.GetHabbo()));
                Session.SendMessage(OtanixEnvironment.GetGame().GetCatalogPremium().CatalogPagesCache);
            }
        }

        internal void GetCatalogPage()
        {
            int PageId = Request.PopWiredInt32();
            Request.PopWiredInt32();
            string CatalogType = Request.PopFixedString();

            if (CatalogType == "NORMAL")
            {
                CatalogPage Page = OtanixEnvironment.GetGame().GetCatalog().GetPage(PageId);

                if (Page == null || !Page.Enabled || !Page.Visible || Page.MinRank > Session.GetHabbo().Rank)
                {
                    return;
                }

                Session.SendMessage(Page.GetMessage);
            }
            else if(CatalogType == "BUILDERS_CLUB")
            {
                CatalogPremiumPage Page = OtanixEnvironment.GetGame().GetCatalogPremium().GetPage(PageId);

                if (Page == null || !Page.Enable || !Page.Visible)
                    return;

                Session.SendMessage(Page.GetMessage());
            }
        }

        internal void RedeemVoucher()
        {
            VoucherHandler.TryRedeemVoucher(Session, Request.PopFixedString());
        }

        internal void PromotedGetMyRooms()
        {
            List<RoomData> rooms = new List<RoomData>();
            foreach (uint roomId in Session.GetHabbo().UsersRooms)
            {
                RoomData data = OtanixEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
                if (data != null && data.Event == null)
                    rooms.Add(data);
            }

            GetResponse().Init(Outgoing.RoomsCanBePromoted);
            GetResponse().AppendBoolean(false); // catalog.club_gift.vip_missing | catalog.club_gift.club_missing (Variables)
            GetResponse().AppendInt32(rooms.Count);
            foreach (var data in rooms)
            {
                GetResponse().AppendUInt(data.Id);
                GetResponse().AppendString(data.Name);
                GetResponse().AppendBoolean(true);
            }
            SendResponse();
        }

        internal void HandlePurchase()
        {
            int PageId = Request.PopWiredInt32();
            uint ItemId = Request.PopWiredUInt();
            string ExtraData = Request.PopFixedString();
            uint Amount = Request.PopWiredUInt();

            OtanixEnvironment.GetGame().GetCatalog().BuyAnItem(Session, PageId, ItemId, ExtraData, Amount);
        }

        internal void PurchaseGift()
        {
            int PageId = Request.PopWiredInt32();
            uint ItemId = Request.PopWiredUInt();
            string ExtraData = Request.PopFixedString();
            string GiftUser = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            string GiftMessage = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            int SpriteId = Request.PopWiredInt32();
            int Lazo = Request.PopWiredInt32();
            int Color = Request.PopWiredInt32();

            OtanixEnvironment.GetGame().GetCatalog().BuyAGift(Session, PageId, ItemId, ExtraData, GiftUser, GiftMessage, SpriteId, Lazo, Color);
        }

        internal void GetRecyclerRewards()
        {
            GetResponse().Init(Outgoing.FurniMaticLookRewards);
            GetResponse().AppendInt32(3);

            for (uint i = 3; i >= 1; i--)
            {
                GetResponse().AppendUInt(i);

                if (i <= 1)
                {
                    GetResponse().AppendInt32(1);
                }
                else if (i == 2)
                {
                    GetResponse().AppendInt32(15);
                }
                else if (i == 3)
                {
                    GetResponse().AppendInt32(35);
                }

                List<EcotronReward> Rewards = OtanixEnvironment.GetGame().GetCatalog().GetEcotronRewardsForLevel(i);

                GetResponse().AppendInt32(Rewards.Count);

                foreach (EcotronReward Reward in Rewards)
                {
                    GetResponse().AppendString(Reward.GetBaseItem().Name);
                    GetResponse().AppendUInt(Reward.DisplayId);
                    GetResponse().AppendString(Reward.GetBaseItem().Type.ToString().ToLower());
                    GetResponse().AppendInt32(Reward.GetBaseItem().SpriteId);
                }
            }

            SendResponse();
        }

        internal void GetCataData1()
        {
            GetResponse().Init(Outgoing.ShopData1);
            GetResponse().AppendBoolean(true);
            GetResponse().AppendInt32(1);
            GetResponse().AppendInt32(0);
            GetResponse().AppendInt32(0);
            GetResponse().AppendInt32(1);
            GetResponse().AppendInt32(10000);
            GetResponse().AppendInt32(48);
            GetResponse().AppendInt32(7);
            SendResponse();

            GetResponse().Init(Outgoing.ShopData2);
            GetResponse().AppendBoolean(true);
            GetResponse().AppendInt32(1);
            GetResponse().AppendInt32(10);
            for (int i = 3080; i < 3090; i++)
            {
                GetResponse().AppendInt32(i);
            }
            GetResponse().AppendInt32(7);
            for (int i = 0; i < 7; i++)
            {
                GetResponse().AppendInt32(i);
            }

            GetResponse().AppendInt32(11);
            for (int i = 0; i < 11; i++)
            {
                GetResponse().AppendInt32(i);
            }

            GetResponse().AppendInt32(7);
            for (int i = 187; i < 194; i++)
            {
                GetResponse().AppendInt32(i);
            }
            SendResponse();

            GetResponse().Init(Outgoing.Offer);
            GetResponse().AppendInt32(100);
            GetResponse().AppendInt32(6);
            GetResponse().AppendInt32(1);
            GetResponse().AppendInt32(1);
            GetResponse().AppendInt32(2);
            GetResponse().AppendInt32(40);
            GetResponse().AppendInt32(99);
            SendResponse();

            GetRecyclerRewards();
        }

        internal void MarketplaceCanSell()
        {
            GetResponse().Init(Outgoing.CanSellInMarketplace);
            GetResponse().AppendInt32(1); // result
            GetResponse().AppendInt32(0); // no use ??
            SendResponse();
        }

        internal void MarketplaceSetPrice()
        {
            int junk = Request.PopWiredInt32();
            uint SpriteId = Request.PopWiredUInt();

            Session.SendMessage(Marketplace.SerializeStatistics(junk, SpriteId));
        }

        internal void MarketplacePostItem()
        {
            if (Session.GetHabbo().GetInventoryComponent() == null)
                return;

            uint sellingPrice = Request.PopWiredUInt();
            int junk = Request.PopWiredInt32();
            uint itemId = Request.PopWiredUInt();

            UserItem Item = Session.GetHabbo().GetInventoryComponent().GetItem(itemId);
            Marketplace.SellItem(Session, Item, sellingPrice);
        }

        internal void MarketplaceGetOwnOffers()
        {
            Session.SendMessage(Marketplace.SerializeOwnOffers(Session.GetHabbo().Id));
        }

        internal void MarketplaceGetOffers()
        {
            var MinPrice = Request.PopWiredInt32();
            var MaxPrice = Request.PopWiredInt32();
            var SearchQuery = Request.PopFixedString();
            var FilterMode = Request.PopWiredInt32();

            Session.SendMessage(Marketplace.SerializeOffers(Session, MinPrice, MaxPrice, SearchQuery, FilterMode));
        }

        internal void MarketplaceTakeBack()
        {
            var ItemId = Request.PopWiredUInt();
            DataRow Row = null;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT item_id, user_id, extra_data, offer_id, state FROM catalog_marketplace_offers WHERE offer_id = " + ItemId + " LIMIT 1");
                Row = dbClient.getRow();
            }

            if (Row == null || Convert.ToUInt32(Row["user_id"]) != Session.GetHabbo().Id || Convert.ToUInt32(Row["state"]) != 1)
            {
                return;
            }

            var Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(Convert.ToUInt32(Row["item_id"]));

            if (Item == null)
            {
                return;
            }

            OtanixEnvironment.GetGame().GetCatalog().DeliverItems(Session, Item, 1, (String)Row["extra_data"], false, 0);

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM catalog_marketplace_offers WHERE offer_id = " + ItemId + "");
            }

            GetResponse().Init(Outgoing.MarketTakeBack);
            GetResponse().AppendUInt(Convert.ToUInt32(Row["offer_id"]));
            GetResponse().AppendBoolean(true);
            SendResponse();

            if(Session != null && Session.GetHabbo() != null && Session.GetHabbo().GetInventoryComponent() != null)
                Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
        }

        internal void MarketplaceClaimCredits()
        {
            DataTable Results = null;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT asking_price FROM catalog_marketplace_offers WHERE user_id = " + Session.GetHabbo().Id + " AND state = 2");
                Results = dbClient.getTable();
            }

            if (Results == null)
            {
                return;
            }

            uint Profit = 0;

            foreach (DataRow Row in Results.Rows)
            {
                Profit += Convert.ToUInt32(Row["asking_price"]);
            }

            if (Profit > 0)
            {
                Session.GetHabbo().Diamonds += Profit;
                Session.GetHabbo().UpdateExtraMoneyBalance();
            }

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM catalog_marketplace_offers WHERE user_id = " + Session.GetHabbo().Id + " AND state = 2");
            }
        }
       
        internal void MarketplacePurchase()
        {
            uint ItemId = Request.PopWiredUInt();
            DataRow Row = null;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT state, timestamp, total_price, extra_data, item_id FROM catalog_marketplace_offers WHERE offer_id = " + ItemId + " ");
                Row = dbClient.getRow();
            }

            if (Row == null || (string)Row["state"] != "1" || (double)Row["timestamp"] < OtanixEnvironment.GetUnixTimestamp())
            {
                Session.SendNotif(LanguageLocale.GetValue("catalog.offerexpired"));
                return;
            }

            uint prize = Convert.ToUInt32(Row["total_price"]);
            if (prize < 2 || Session.GetHabbo().Diamonds < prize)
            {
                Session.SendNotif(LanguageLocale.GetValue("catalog.crystalerror"));
                return;
            }

            var Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(Convert.ToUInt32(Row["item_id"]));
            if (Item == null)
                return;

            List<UserItem> items = OtanixEnvironment.GetGame().GetCatalog().DeliverItems(Session, Item, 1, (String)Row["extra_data"], false, 0);
            if (items.Count == 0)
                return;

            Session.GetHabbo().Diamonds -= prize;
            Session.GetHabbo().UpdateExtraMoneyBalance();

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE catalog_marketplace_offers SET state = 2 WHERE offer_id = " + ItemId + "");

                Marketplace.AddItemToStatistics(Item.ItemId, prize, dbClient);
            }

            Session.GetMessageHandler().GetResponse().Init(Outgoing.MarketBuyItem);
            Session.GetMessageHandler().GetResponse().AppendInt32(1); // result (1,2,3,4)
            Session.GetMessageHandler().GetResponse().AppendUInt(items[0].Id); // 
            Session.GetMessageHandler().GetResponse().AppendUInt(prize); //
            Session.GetMessageHandler().GetResponse().AppendUInt(ItemId);
            Session.GetMessageHandler().SendResponse();

            Session.GetMessageHandler().GetResponse().Init(Outgoing.SendPurchaseAlert);
            Session.GetMessageHandler().GetResponse().AppendInt32(1); // items
            Session.GetMessageHandler().GetResponse().AppendInt32(1);
            Session.GetMessageHandler().GetResponse().AppendInt32(1);
            Session.GetMessageHandler().GetResponse().AppendUInt(items[0].Id);
            Session.GetMessageHandler().SendResponse();

            if (Session != null && Session.GetHabbo() != null && Session.GetHabbo().GetInventoryComponent() != null)
                Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
        }

        internal void CheckPetName()
        {
            string PetName = Request.PopFixedString();

            Session.GetMessageHandler().GetResponse().Init(Outgoing.CheckPetName);
            Session.GetMessageHandler().GetResponse().AppendInt32(Catalog.CheckPetName(PetName.ToLower()) ? 0 : 2);
            Session.GetMessageHandler().GetResponse().AppendString(PetName);
            Session.GetMessageHandler().SendResponse();
        }

        internal void PetRaces()
        {
            string PetType = Request.PopFixedString();

            int petid = 0;
            int.TryParse(PetType.Substring(6), out petid);

            GetResponse().Init(Outgoing.PetRace);
            GetResponse().AppendString(PetType);

            if (PetRace.RaceGotRaces(petid))
            {
                List<PetRace> Races = PetRace.GetRacesForRaceId(petid);
                GetResponse().AppendInt32(Races.Count);
                foreach (PetRace r in Races)
                {
                    GetResponse().AppendInt32(petid); // pet id
                    GetResponse().AppendInt32(r.Color1); // color1
                    GetResponse().AppendInt32(r.Color2); // color2
                    GetResponse().AppendBoolean(r.Has1Color); // has1color
                    GetResponse().AppendBoolean(r.Has2Color); // has2color
                }
            }
            else
            {
                Session.SendNotif("¡Ha ocurrido un error cuando ibas a ver esta mascota, repórtalo a un administrador!");
                GetResponse().AppendInt32(0);
            }

            SendResponse();
        }

        internal void OpenFurniMaticPage()
        {
            GetResponse().Init(Outgoing.FurniMaticNoRoomError);
            GetResponse().AppendInt32(1);
            GetResponse().AppendInt32(0);
            SendResponse();
        }

        internal void RecycleItem()
        {
            if (!Session.GetHabbo().InRoom)
                return;

            int ItemsCount = Request.PopWiredInt32(); // 5
            for (int i = 0; i < ItemsCount; i++)
            {
                uint ItemId = Request.PopWiredUInt();

                if (!Session.GetHabbo().GetInventoryComponent().ContainsItem(ItemId))
                    return;

                Session.GetHabbo().GetInventoryComponent().RemoveItem(ItemId, true);
            }

            uint newItemId;
            EcotronReward Reward = OtanixEnvironment.GetGame().GetCatalog().GetRandomEcotronReward();
            if (Reward == null)
                return;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO items (base_id) VALUES (" + EmuSettings.FURNIMATIC_BOX_ITEMID + ")");
                newItemId = (uint)dbClient.insertQuery();

                dbClient.runFastQuery("INSERT INTO items_users VALUES (" + newItemId + "," + Session.GetHabbo().Id + ")");

                dbClient.setQuery("INSERT INTO items_extradata VALUES (" + newItemId + ",@data)");
                dbClient.addParameter("data", DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day);
                dbClient.runQuery();

                dbClient.runFastQuery("INSERT INTO user_presents (item_id,base_id,amount,extra_data) VALUES (" + newItemId + "," + Reward.BaseId + ",1,'')");
            }

            UserItem u = Session.GetHabbo().GetInventoryComponent().AddNewItem(newItemId, (uint)EmuSettings.FURNIMATIC_BOX_ITEMID, DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day, false, false, false, Reward.GetBaseItem().Name, Session.GetHabbo().Id, 0);

            Session.GetMessageHandler().GetResponse().Init(Outgoing.SendPurchaseAlert);
            Session.GetMessageHandler().GetResponse().AppendInt32(1); // items
            Session.GetMessageHandler().GetResponse().AppendInt32(1); // type (gift) == s
            Session.GetMessageHandler().GetResponse().AppendInt32(1);
            Session.GetMessageHandler().GetResponse().AppendUInt(u.Id);
            Session.GetMessageHandler().SendResponse();

            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            Response.Init(Outgoing.FurniMaticReceiveItem);
            Response.AppendInt32(1);
            Response.AppendUInt(newItemId);
            SendResponse();
        }

        internal void SearchCatalogItem()
        {
            uint ItemID = Request.PopWiredUInt();
            CatalogItem iItem = (CatalogItem)OtanixEnvironment.GetGame().GetCatalog().catalogItems[ItemID];
            if (iItem == null || EmuSettings.CATALOG_NOSEARCH_PAGES.Contains(iItem.PageID))
                return;

            ServerMessage Message = new ServerMessage(Outgoing.SearchCatalogItem);
            iItem.Serialize(Message);
            Session.SendMessage(Message);
        }

        internal void BuyServerCameraPhoto()
        {
            if ((OtanixEnvironment.GetUnixTimestamp() - Session.GetHabbo().UltimaFotoComprada) < 20)
            {
                Session.SendNotif("Você deve esperar 20 segundos antes de comprar outra foto.");
                return;
            }

            if (!Session.GetHabbo().lastPhotoPreview.Contains("-"))
                return;           
            
            string roomId = Session.GetHabbo().lastPhotoPreview.Split('-')[0];
            string timestamp = Session.GetHabbo().lastPhotoPreview.Split('-')[1];
            string md5image = URLPost.GetMD5(Session.GetHabbo().lastPhotoPreview);

            Item Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(EmuSettings.CAMERA_BASEID);
            if (Item == null)
                return;

            ServerMessage Message = new ServerMessage(Outgoing.BuyPhoto);
            Session.SendMessage(Message);

            OtanixEnvironment.GetGame().GetCatalog().DeliverItems(Session, Item, 1, "{\"timestamp\":\"" + timestamp + "\", \"id\":\"" + md5image + "\"}", true, 0);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_CameraPhotoCount", 1);

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("REPLACE INTO items_camera VALUES (@id, '" + Session.GetHabbo().Id + "',@creator_name, '" + roomId + "','" + timestamp + "')");
                dbClient.addParameter("id", md5image);
                dbClient.addParameter("creator_name", Session.GetHabbo().Username);
                dbClient.runQuery();
            }
            Session.GetHabbo().UltimaFotoComprada = OtanixEnvironment.GetUnixTimestamp();
        }

        internal void BuyTargetedOffer()
        {
            uint TargetedId = Request.PopWiredUInt();
            TargetedOffer to = OtanixEnvironment.GetGame().GetTargetedOfferManager().GetTargetedOffer(TargetedId);
            if (to == null)
                return;

            uint Amount = Request.PopWiredUInt();
            if (Session.GetHabbo().TargetedOffers.ContainsKey(TargetedId) && Session.GetHabbo().TargetedOffers[TargetedId] + Amount > to.PurchaseLimit)
                return; // ya has alcanzado el límite.

            if (Session.GetHabbo().Diamonds < to.PriceInDiamonds)
                return; // no tienes suficientes diamantes.

            if (OtanixEnvironment.GetUnixTimestamp() > to.ExpirationTime)
                return; // fecha finalizada.

            Session.GetHabbo().Diamonds -= to.PriceInDiamonds;
            Session.GetHabbo().UpdateExtraMoneyBalance();

            if (Session.GetHabbo().TargetedOffers.ContainsKey(TargetedId))
                Session.GetHabbo().TargetedOffers[TargetedId] += Amount;
            else
                Session.GetHabbo().TargetedOffers.Add(TargetedId, Amount);

            foreach (KeyValuePair<uint, uint> Item in to.Items)
            {
                Item item = OtanixEnvironment.GetGame().GetItemManager().GetItem(Item.Key);
                if (item != null)
                {
                    for (int i = 0; i < Item.Value; i++)
                    {
                        OtanixEnvironment.GetGame().GetCatalog().DeliverItems(Session, item, 1, "", false, 0);
                    }
                }
            }

            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
        }
    }
}
