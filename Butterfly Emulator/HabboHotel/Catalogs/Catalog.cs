using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Pets;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using Butterfly.HabboHotel.Group;
using Butterfly.HabboHotel.RoomBots;
using ButterStorm.HabboHotel.Catalogs;
using Butterfly.HabboHotel.Filter;
using Butterfly.HabboHotel.Misc;
using Plus.HabboHotel.Catalog.Bundles;
using Butterfly.HabboHotel.Rooms;

namespace Butterfly.HabboHotel.Catalogs
{
    class Catalog
    {
        internal Dictionary<int, CatalogPage> Pages;
        internal Dictionary<int, string> coresDisponiveis;
        internal List<EcotronReward> EcotronRewards;
        internal ServerMessage mGroupPage;
        private ServerMessage[] mCataIndexCache;
        internal Hashtable catalogItems;
        internal List<CatalogPageExtra> extraPages;
        private BundleQuartoManager _predesignedManager;
        private Dictionary<int, BundleContent> _predesignedItems;

        internal void Initialize(IQueryAdapter dbClient)
        {
            Pages = new Dictionary<int, CatalogPage>();
            coresDisponiveis = new Dictionary<int, string>();
            EcotronRewards = new List<EcotronReward>();
            catalogItems = new Hashtable();
            extraPages = new List<CatalogPageExtra>();

            this._predesignedManager = new BundleQuartoManager();
            this._predesignedManager.Initialize();
            this._predesignedItems = new Dictionary<int, BundleContent>();


            dbClient.setQuery("SELECT * FROM catalog_pages ORDER BY order_num");
            var Data = dbClient.getTable();

            dbClient.setQuery("SELECT * FROM catalog_coresNick");
            var Cores = dbClient.getTable();

            dbClient.setQuery("SELECT * FROM catalog_pages_extra");
            var Extra = dbClient.getTable();

            dbClient.setQuery("SELECT * FROM ecotron_rewards ORDER BY item_id");
            var EcoData = dbClient.getTable();

            dbClient.setQuery("SELECT * FROM catalog_items");
            var CatalogueItems = dbClient.getTable();

            if (CatalogueItems != null)
            {
                foreach (DataRow Row in CatalogueItems.Rows)
                {
                    if (string.IsNullOrEmpty(Row["item_ids"].ToString()) || (int)Row["amount"] <= 0)
                    {
                        continue;
                    }

                    int ItemId = Convert.ToInt32(Row["id"]);
                    int PageId = Convert.ToInt32(Row["page_id"]);
                    uint PredesignedId = Convert.ToUInt32(Row["predesigned_id"]);

                    if (PredesignedId > 0)
                    {
                        var roomPack = _predesignedManager.predesignedRoom[PredesignedId];
                        if (roomPack == null) continue;
                        if (roomPack.CatalogItems.Contains(";"))
                        {
                            var cataItems = new Dictionary<int, int>();
                            var itemArray = roomPack.CatalogItems.Split(new char[] { ';' });
                            foreach (var item in itemArray)
                            {
                                var items = item.Split('-');
                                CatalogItem PredesignedData = FindItem(Convert.ToUInt32(items[0]));
                                if (PredesignedData == null)
                                {
                                    continue;
                                }

                                cataItems.Add(Convert.ToInt32(items[0]), Convert.ToInt32(items[1]));
                            }

                            this._predesignedItems[PageId] = new BundleContent(ItemId, cataItems);
                        }
                    }








                    catalogItems.Add(Convert.ToUInt32(Row["id"]), new CatalogItem(Row));
                }
            }

            if (Cores != null)
            {
                foreach (DataRow Row in Cores.Rows)
                {
                    if (string.IsNullOrEmpty(Row["cor"].ToString()))
                    {
                        continue;
                    }
                    coresDisponiveis.Add(Convert.ToInt32(Row["id"]), (string)Row["cor"]);
                }
            }

            if (Extra != null)
            {
                foreach (DataRow Row in Extra.Rows)
                {
                    extraPages.Add(new CatalogPageExtra(Row));
                }
            }

            if (Data != null)
            {
                foreach (DataRow Row in Data.Rows)
                {
                    Pages.Add((int)Row["id"], new CatalogPage((int)Row["id"], (int)Row["parent_id"],
                        (string)Row["caption"], (Row["visible"].ToString() == "1") ? true : false, (Row["enabled"].ToString() == "1") ? true : false,
                        Convert.ToUInt32(Row["min_rank"]),
                        (int)Row["icon_image"], (string)Row["page_name"], (string)Row["page_layout"], (string)Row["page_headline"], (string)Row["page_teaser"], (string)Row["page_special"], (string)Row["page_text1"],
                        (string)Row["page_text2"], (string)Row["page_text_details"], (string)Row["page_text_teaser"], ref catalogItems, ref extraPages));
                }
            }

            if (EcoData != null)
            {
                foreach (DataRow Row in EcoData.Rows)
                {
                    EcotronRewards.Add(new EcotronReward(Convert.ToUInt32(Row["display_id"]), Convert.ToUInt32(Row["item_id"]), Convert.ToUInt32(Row["reward_level"])));
                }
            }

            RestackByFrontpage();
        }
        internal BundleQuartoManager GetPredesignedRooms()
        {
            return this._predesignedManager;
        }
        internal string pegaCor(int idcor)
        {
            if (coresDisponiveis.ContainsKey(idcor))
                return coresDisponiveis[idcor];
                    
            return string.Empty;
        }

        internal void RestackByFrontpage()
        {
            var fronpage = Pages[1];
            var restOfCata = new Dictionary<int, CatalogPage>(Pages);

            restOfCata.Remove(1);
            Pages.Clear();

            Pages.Add(fronpage.PageId, fronpage);

            foreach (var pair in restOfCata)
                Pages.Add(pair.Key, pair.Value);
        }

        internal void InitCache()
        {
            mCataIndexCache = new ServerMessage[Ranks.MAX_RANK_ID + 1];

            for (var i = 1; i < Ranks.MAX_RANK_ID + 1; i++)
            {
                mCataIndexCache[i] = SerializeIndexForCache(i);
            }

            foreach (var Page in Pages.Values)
            {
                Page.InitMsg();
            }
        }

        internal CatalogItem FindItem(uint ItemId)
        {
            foreach (var Page in Pages.Values)
            {
                if (Page.Items.ContainsKey(ItemId))
                    return (CatalogItem)Page.Items[ItemId];
            }

            return null;
        }

        internal int GetTreeSize(int rank, int TreeId)
        {
            var i = 0;

            foreach (var Page in Pages.Values)
            {
                if (Page.MinRank > rank)
                {
                    continue;
                }

                if (Page.ParentId == TreeId)
                {
                    i++;
                }
            }

            return i;
        }

        internal CatalogPage GetPage(int Page)
        {
            if (!Pages.ContainsKey(Page))
            {
                return null;
            }

            return Pages[Page];
        }

        internal void BuyAnItem(GameClient Session, int PageId, uint ItemId, string ExtraData, uint priceAmount)
        {
            #region CHECK_RETURN_VALUES
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            if ((Session.GetHabbo().GetInventoryComponent().GetTotalItems + priceAmount) > EmuSettings.INVENTARY_ITEMS_LIMIT)
            {
                Session.SendNotif("Al realizar esta compra usted sobrepasará el límite de items permitidos en un catálogo, por eso no la podemos generar.");
                return;
            }

            CatalogPage Page;
            if (!Pages.TryGetValue(PageId, out Page))
                return;

            if (Page == null || !Page.Enabled || !Page.Visible)
                return;

            if (Page.MinRank > Session.GetHabbo().Rank)
                return;



            var Item = Page.GetItem(ItemId);
            if (Item == null)
                return;

            if (Item.Name.StartsWith("badge_display"))
            {
                if (!Session.GetHabbo().GetBadgeComponent().HasBadge(ExtraData))
                    return;
            }

            var ExtraDataBackup = ExtraData;
            #endregion

            #region CHECK_PRICE_ITEMS
            uint costDiamonds = 0;
            if (Item.DiamondsCost > 0)
            {
                costDiamonds = FreeItemsPatch.GetFreeItems(Item.HaveOffer, Item.DiamondsCost, priceAmount);
                if (Session.GetHabbo().Diamonds < costDiamonds)
                {
                    // no tienes suficientes diamantes.
                    return;
                }
            }

            uint costMoedas = 0;
            if (Item.CreditsCost > 0 && EmuSettings.HOTEL_LUCRATIVO)
            {
                costMoedas = FreeItemsPatch.GetFreeItems(Item.HaveOffer, Item.CreditsCost, priceAmount);
                if (Session.GetHabbo().Moedas < costMoedas)
                {
                    // não tem moedas suficientes
                    return;
                }
            }
            #endregion

            #region BUY_PROGRESS
            if (Item.Items.Count == 1)
            {
                UInt32 iId = new List<UInt32>(Item.Items.Keys)[0];
                if (Item.GetBaseItem(iId).LimitedStack > 0)
                {
                    if (Item.GetBaseItem(iId).LimitedStack <= Item.LimitedSelled)
                    {
                        Session.SendMessage(new ServerMessage(Outgoing.FinishLimitedItems));
                        return;
                    }

                    Item.LimitedSelled++;
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("UPDATE catalog_items SET limited_sells = " + Item.LimitedSelled + " WHERE id = " + Item.Id);
                    }
                    Page.InitMsg(); // update page!
                }
            }

            if (costDiamonds > 0)
            {
                Session.GetHabbo().Diamonds -= costDiamonds;
                Session.GetHabbo().UpdateExtraMoneyBalance();
            }

            if (costMoedas > 0 && EmuSettings.HOTEL_LUCRATIVO)
            {
                Session.GetHabbo().Moedas -= Convert.ToInt32(costMoedas);
                Session.GetHabbo().UpdateCreditsBalance();
            }

            bool umItem = Item.ItemIdString.Split(';').Length == 1;

            if (Item.BadgeName.Length > 0 && umItem)
            {
                if (Item.BadgeName.StartsWith("cor_"))
                {
                    string corTenho = Item.BadgeName.Replace("cor_", "");

                    if (Session.GetHabbo().tenhoCor(Convert.ToInt32(corTenho)))
                    {
                        Session.SendNotif(LanguageLocale.GetValue("cor.japossui"));
                        Session.GetHabbo().Diamonds += costDiamonds;
                        Session.GetHabbo().Moedas += Convert.ToInt32(costMoedas);
                        Session.GetHabbo().UpdateExtraMoneyBalance();
                    }
                    else
                    {
                        Session.SendNotif(LanguageLocale.GetValue("cor.sucesso"));
                        Session.GetHabbo().LastPurchase = DateTime.Now.ToString();
                        Session.GetHabbo().coresjaTenho += ";" + corTenho;
                    }
                }
                else
                {
                    if (Session.GetHabbo().GetBadgeComponent().HasBadge(Item.BadgeName)) {
                        Session.SendNotif(LanguageLocale.GetValue("emblema.japossui"));
                        Session.GetHabbo().Diamonds += costDiamonds;
                        Session.GetHabbo().Moedas += Convert.ToInt32(costMoedas);
                        Session.GetHabbo().UpdateExtraMoneyBalance();
                    } else
                    {
                        Session.GetHabbo().GetBadgeComponent().GiveBadge(Item.BadgeName);
                        Session.SendNotif(LanguageLocale.GetValue("emblema.sucesso"));
                        Session.GetHabbo().LastPurchase = DateTime.Now.ToString();
                    }
                }

                Session.GetMessageHandler().GetResponse().Init(Outgoing.PurchaseOKMessageOfferData);
                Session.GetMessageHandler().GetResponse().AppendUInt(0);
                Session.GetMessageHandler().GetResponse().AppendString("LuL");
                Session.GetMessageHandler().GetResponse().AppendBoolean(false); // false = comprar, true = alquilar
                Session.GetMessageHandler().GetResponse().AppendUInt(0);
                Session.GetMessageHandler().GetResponse().AppendUInt((Item.DiamondsCost > 0) ? Item.DiamondsCost : Item.DucketsCost);
                Session.GetMessageHandler().GetResponse().AppendInt32((Item.DiamondsCost > 0) ? 105 : 0);
                Session.GetMessageHandler().GetResponse().AppendBoolean(false); // gift button
                Session.GetMessageHandler().GetResponse().AppendInt32(1);
                Session.GetMessageHandler().GetResponse().AppendString("s");
                Session.GetMessageHandler().GetResponse().AppendInt32(-1);
                Session.GetMessageHandler().GetResponse().AppendString(string.Empty);
                Session.GetMessageHandler().GetResponse().AppendUInt(1);
                Session.GetMessageHandler().GetResponse().AppendBoolean(false); // is limited.
                Session.GetMessageHandler().GetResponse().AppendInt32(0); // 0 = todos, 2 = club.
                Session.GetMessageHandler().GetResponse().AppendBoolean(false);
                Session.GetMessageHandler().SendResponse();
            }

            if (Item.PredesignedId > 0 && OtanixEnvironment.GetGame().GetCatalog().GetPredesignedRooms().predesignedRoom.ContainsKey((uint)Item.PredesignedId))
            {
                _predesignedManager.buyBundle(Session, Item);
                return;
            }                

            foreach (var i in Item.Items.Keys)
            {
                #region GIVE_EXTRADATA_VALUE
                switch (Item.GetBaseItem(i).InteractionType)
                {
                    case InteractionType.none:
                        ExtraData = "";
                        break;

                    case InteractionType.musicdisc:
                        ExtraData = Item.songID.ToString();
                        break;

                    #region Pet handling
                    case InteractionType.pet:
                        try
                        {
                            var Bits = ExtraData.Split('\n');
                            var PetName = Bits[0];
                            var Race = Bits[1];
                            var Color = Bits[2];

                            int.Parse(Race); // to trigger any possible errors
   
                            if (!CheckPetName(PetName.ToLower()))
                                return;

                            if (Race.Length != 1 && Race.Length != 2)
                                return;

                            if (Color.Length != 6)
                                return;
                        }
                        catch //(Exception e)
                        {
                            //Logging.WriteLine(e.ToString());
                            //Logging.HandleException(e, "Catalog.HandlePurchase");
                            return;
                        }

                        break;

                    #endregion

                    case InteractionType.roomeffect:

                        Double Number = 0;

                        try
                        {
                            if (string.IsNullOrEmpty(ExtraData))
                                Number = 0;
                            else
                                Number = Double.Parse(ExtraData, OtanixEnvironment.cultureInfo);
                        }
                        catch (Exception e) { Logging.HandleException(e, "Catalog.HandlePurchase: " + ExtraData); }

                        ExtraData = Number.ToString().Replace(',', '.');
                        break; // maintain extra data // todo: validate

                    case InteractionType.postit:
                        ExtraData = "FFFF33";
                        break;

                    case InteractionType.dimmer:
                        ExtraData = "1,1,1,#000000,255";
                        break;

                    case InteractionType.trophy:
                        ExtraData = Session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) + OtanixEnvironment.FilterInjectionChars(ExtraData, true);
                        break;

                    case InteractionType.bot:
                        ExtraData = Item.BotLook;
                        break;

                    case InteractionType.badge_display:
                        ExtraData = ExtraData + ";" + Session.GetHabbo().Username + ";" + DateTime.Now.Date.ToString().Replace("/", "-");
                        break;

                    case InteractionType.guildforum:
                        // none...
                        break;

                    case InteractionType.maniqui:
                        ExtraData = "M;lg-270-82.ch-210-66;";
                        break;

                    case InteractionType.yttv:

                        if (OtanixEnvironment.GetGame().GetYoutubeManager().Videos.ContainsKey((int)Item.GetBaseItem(i).ItemId))
                        {
                            var ytbTV = OtanixEnvironment.GetGame().GetYoutubeManager().Videos[(int)Item.GetBaseItem(i).ItemId];
                            ExtraData = "1;" + ytbTV.favVideo;
                        }

                        break;

                    case InteractionType.ads_mpu:
                        ExtraData = "0";
                        break;

                    case InteractionType.userslock:
                    case InteractionType.piñata:
                    case InteractionType.balloon15:
                    case InteractionType.dalia:
                        ExtraData = "0";
                        break;

                    case InteractionType.fbgate:
                        ExtraData = "hd-180-14.ch-210-1408.lg-270-1408,hd-600-14.ch-630-1408.lg-695-1408";
                        break;

                    case InteractionType.wiredClassification:
                        ExtraData = "0;" + OtanixEnvironment.GetUnixTimestamp();
                        break;

                    case InteractionType.seed:
                        if(Item.GetBaseItem(i).Name.Contains("rare"))
                            ExtraData = new Random().Next(8, 12).ToString();
                        else
                            ExtraData = new Random().Next(12).ToString();
                        break;

                    default:
                        ExtraData = "";
                        break;
                }

                if (Item.GetBaseItem(i).InteractionType == InteractionType.guildforum)
                {
                    uint groupId = 0;
                    uint.TryParse(ExtraData, out groupId);

                    var Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(groupId);
                    if (Group != null)
                    {
                        Session.GetMessageHandler().GetResponse().Init(Outgoing.CustomAlert);
                        Session.GetMessageHandler().GetResponse().AppendString("forums.delivered");
                        Session.GetMessageHandler().GetResponse().AppendInt32(2);
                        Session.GetMessageHandler().GetResponse().AppendString("groupId");
                        Session.GetMessageHandler().GetResponse().AppendString(ExtraData); // groupid
                        Session.GetMessageHandler().GetResponse().AppendString("groupName");
                        Session.GetMessageHandler().GetResponse().AppendString(Group.Name);
                        Session.GetMessageHandler().SendResponse();

                        if (Group.Forum == null)
                        {
                            Group.CreateGroupForum(Group.Id, 1, 1, 1, 0);

                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbClient.setQuery("INSERT INTO groups_forums (groupid) VALUES (@groupid)");
                                dbClient.addParameter("groupid", ExtraData);
                                dbClient.runQuery();
                            }
                        }
                    }
                }

                if (Item.GetBaseItem(i).IsGroupItem)
                {
                    uint GroupId = 0;
                    if (uint.TryParse(ExtraDataBackup, out GroupId))
                    {
                        var Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
                        ExtraData = "0;" + Group.Id + ";" + Group.HtmlColor1 + ";" + Group.HtmlColor2;
                    }
                }

                if (Item.GetBaseItem(i).LimitedStack > 0)
                {
                    if (ExtraData == "")
                        ExtraData = "0;" + Item.LimitedSelled;
                    else
                        ExtraData = ExtraData + ";" + Item.LimitedSelled;
                }
                #endregion

                #region GENERATE_FURNI_AND_SEND_PACKET
                Session.GetMessageHandler().GetResponse().Init(Outgoing.PurchaseOKMessageOfferData);
                Session.GetMessageHandler().GetResponse().AppendUInt(Item.GetBaseItem(i).ItemId);
                Session.GetMessageHandler().GetResponse().AppendString(Item.GetBaseItem(i).Name);
                Session.GetMessageHandler().GetResponse().AppendBoolean(false); // false = comprar, true = alquilar
                Session.GetMessageHandler().GetResponse().AppendUInt(Item.CreditsCost);
                Session.GetMessageHandler().GetResponse().AppendUInt((Item.DiamondsCost > 0) ? Item.DiamondsCost : Item.DucketsCost);
                Session.GetMessageHandler().GetResponse().AppendInt32((Item.DiamondsCost > 0) ? 105 : 0);
                Session.GetMessageHandler().GetResponse().AppendBoolean(true); // gift button
                Session.GetMessageHandler().GetResponse().AppendInt32(1);
                Session.GetMessageHandler().GetResponse().AppendString(Item.GetBaseItem(i).Type.ToString().ToLower());
                Session.GetMessageHandler().GetResponse().AppendInt32(Item.GetBaseItem(i).SpriteId);
                Session.GetMessageHandler().GetResponse().AppendString(string.Empty);
                Session.GetMessageHandler().GetResponse().AppendUInt(Item.Amount);
                Session.GetMessageHandler().GetResponse().AppendBoolean(false); // is limited.
                Session.GetMessageHandler().GetResponse().AppendInt32(0); // 0 = todos, 2 = club.
                Session.GetMessageHandler().GetResponse().AppendBoolean(Item.HaveOffer);
                Session.GetMessageHandler().SendResponse();

                if (Item.GetBaseItem(i).Type.ToString().ToLower().Equals("e"))
                {
                    DeliverItems(Session, Item.GetBaseItem(i), (priceAmount * ((Item.Items.Count > 1) ? Item.Items[i] : Item.Amount)), ExtraData, (Page.MinRank > 1), Item.songID);
                }
                else
                {
                    // 1 = Posesión
                    // 2 = Alquiler
                    // 3 = Mascotas
                    // 4 = Placas
                    // 5 = Bots

                    int Type = 1;
                    if (Item.GetBaseItem(i).Type.ToString().ToLower().Equals("s"))
                    {
                        if (Item.GetBaseItem(i).InteractionType == InteractionType.pet)
                            Type = 3;
                        else
                            Type = 1;
                    }
                    else if (Item.GetBaseItem(i).Type.ToString().ToLower().Equals("r"))
                    {
                        Type = 5;
                    }

                    var items = DeliverItems(Session, Item.GetBaseItem(i), (priceAmount * ((Item.Items.Count > 1) ? Item.Items[i] : Item.Amount)), ExtraData, (Page.MinRank > 1), Item.songID);

                    if (Item.GetBaseItem(i).InteractionType == InteractionType.pet)
                    {
                        Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
                        return;
                    }

                    if (items == null || items.Count <= 0)
                        return;

                    if (Item.GetBaseItem(i).Name == "song_disk")
                        OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_MusicCollector", 1);

                    if (Item.BadgeName.Length > 0 && !Session.GetHabbo().GetBadgeComponent().HasBadge(Item.BadgeName))
                        Session.GetHabbo().GetBadgeComponent().GiveBadge(Item.BadgeName);
                   

                    Session.GetMessageHandler().GetResponse().Init(Outgoing.SendPurchaseAlert);
                    Session.GetMessageHandler().GetResponse().AppendInt32(1); // items
                    Session.GetMessageHandler().GetResponse().AppendInt32(Type);
                    Session.GetMessageHandler().GetResponse().AppendInt32(items.Count);
                    foreach (var u in items)
                        Session.GetMessageHandler().GetResponse().AppendUInt(u.Id);
                    Session.GetMessageHandler().SendResponse();

                    if (items[0].mBaseItem.Type.ToString().ToLower().Equals("r")) // bot
                    {
                        var Bot = GenerateBot(Session, items[0], ExtraData);

                        Session.GetMessageHandler().GetResponse().Init(Outgoing.BuyBot2);
                        Session.GetMessageHandler().GetResponse().AppendUInt(Bot.BotId);
                        Session.GetMessageHandler().GetResponse().AppendString(Bot.Name);
                        Session.GetMessageHandler().GetResponse().AppendString(Bot.Motto);
                        Session.GetMessageHandler().GetResponse().AppendString(Bot.Gender);
                        Session.GetMessageHandler().GetResponse().AppendString(Bot.Look);
                        Session.GetMessageHandler().GetResponse().AppendBoolean(true);
                        Session.GetMessageHandler().SendResponse();

                        return;
                    }

                    Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    Session.GetHabbo().LastPurchase = DateTime.Now.ToString();
                }
                #endregion
            }
            #endregion
        }

        internal void BuyAGift(GameClient Session, int PageId, uint ItemId, string ExtraData, string GiftUser, string GiftMessage, int GiftSpriteId, int GiftLazo, int GiftColor)
        {
            #region CHECK_RETURN_VALUES
            if (Session == null || Session.GetHabbo() == null)
                return;

            if ((Session.GetHabbo().GetInventoryComponent().GetTotalItems + 1) > EmuSettings.INVENTARY_ITEMS_LIMIT)
            {
                Session.SendNotif("Al realizar esta compra usted sobrepasará el límite de items permitidos en un catálogo, por eso no la podemos generar.");
                return;
            }

            CatalogPage Page;
            if (!Pages.TryGetValue(PageId, out Page))
                return;

            if (Page == null || !Page.Enabled || !Page.Visible)
                return;

            if (Page.MinRank > Session.GetHabbo().Rank)
                return;

            var Item = Page.GetItem(ItemId);
            if (Item == null || !Item.AllowGift)
                return;

            if (Item.Name.StartsWith("badge_display"))
            {
                if (!Session.GetHabbo().GetBadgeComponent().HasBadge(ExtraData))
                    return;
            }

            var ExtraDataBackup = ExtraData;
            uint GiftUserId = 0;
            #endregion

            #region CHECK_PRICE_ITEMS
            if (Item.DiamondsCost > 0)
            {
                if (Session.GetHabbo().Diamonds < Item.DiamondsCost)
                {
                    // no tienes suficientes diamantes.
                    return;
                }
                Session.GetHabbo().Diamonds -= Item.DiamondsCost;
                Session.GetHabbo().UpdateExtraMoneyBalance();
            }

            if (Item.CreditsCost > 0 && EmuSettings.HOTEL_LUCRATIVO)
            {
                if (Session.GetHabbo().Moedas < Item.CreditsCost)
                {
                    // não tem moedas suficientes
                    return;
                }
                Session.GetHabbo().Moedas -= Convert.ToInt32(Item.CreditsCost);
                Session.GetHabbo().UpdateCreditsBalance();
            }
            #endregion

            #region BUY_PROGRESS
            if (Item.Items.Count == 1)
            {
                UInt32 iId = new List<UInt32>(Item.Items.Keys)[0];
                if (Item.GetBaseItem(iId).LimitedStack > 0)
                {
                    if (Item.GetBaseItem(iId).LimitedStack <= Item.LimitedSelled)
                    {
                        Session.SendMessage(new ServerMessage(Outgoing.FinishLimitedItems));
                        return;
                    }

                    Item.LimitedSelled++;
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("UPDATE catalog_items SET limited_sells = " + Item.LimitedSelled + " WHERE id = " + Item.Id);
                    }
                    Page.InitMsg(); // update page!
                }
            }

            foreach (var i in Item.Items.Keys)
            {
                #region gift
                if (BlackWordsManager.Check(GiftMessage, BlackWordType.Insult, Session, "<Nombre de Regalo>"))
                {
                    Session.SendNotif("Descripción del regalo inválida.");
                    return;
                }
                if (Session.GetHabbo().PresentBuyStopwatch.ElapsedMilliseconds < 30000 && Session.GetHabbo().Rank < 4)
                {
                    Session.SendNotif("Aguarde " + (int)(30 - (Session.GetHabbo().PresentBuyStopwatch.ElapsedMilliseconds / 1000)) + " segundos para comprar otro regalo.");
                    return;
                }

                DataRow dRow;
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT id FROM users WHERE username = @gift_user");
                    dbClient.addParameter("gift_user", GiftUser);

                    dRow = dbClient.getRow();
                }

                if (dRow == null)
                {
                    Session.GetMessageHandler().GetResponse().Init(Outgoing.GiftError);
                    Session.GetMessageHandler().GetResponse().AppendString(GiftUser);
                    Session.GetMessageHandler().SendResponse();

                    return;
                }

                GiftUserId = Convert.ToUInt32(dRow[0]);

                if (GiftUserId == 0)
                {
                    Session.GetMessageHandler().GetResponse().Init(Outgoing.GiftError);
                    Session.GetMessageHandler().GetResponse().AppendString(GiftUser);
                    Session.GetMessageHandler().SendResponse();

                    return;
                }

                Session.GetHabbo().PresentBuyStopwatch.Restart();

                if (Item.DiamondsCost > 0)
                {
                    Session.GetHabbo().Diamonds -= Item.DiamondsCost;
                    Session.GetHabbo().UpdateExtraMoneyBalance();
                }

                if (Item.CreditsCost > 0 && EmuSettings.HOTEL_LUCRATIVO)
                {
                    Session.GetHabbo().Moedas -= Convert.ToInt32(Item.CreditsCost);
                    Session.GetHabbo().UpdateCreditsBalance();
                }

                #endregion

                if (Item.GetBaseItem(i).Type == 'e')
                {
                    Session.SendNotif(LanguageLocale.GetValue("catalog.gift.send.error"));
                    return;
                }

                switch (Item.GetBaseItem(i).InteractionType)
                {
                    case InteractionType.none:
                        ExtraData = "";
                        break;

                    case InteractionType.musicdisc:
                        ExtraData = Item.songID.ToString();
                        break;

                    #region Pet handling
                    case InteractionType.pet:
                        try
                        {
                            var Bits = ExtraData.Split('\n');
                            if (Bits.Length < 3)
                                return;

                            var PetName = Bits[0];
                            var Race = Bits[1];
                            var ii = 0;
                            var Color = Bits[2];

                            if (int.TryParse(Race, out ii) == false) // to trigger any possible errors
                                return;

                            if (!CheckPetName(PetName.ToLower()))
                                return;

                            if (Race.Length != 1 && Race.Length != 2)
                                return;

                            if (Color.Length != 6)
                                return;
                        }
                        catch (Exception e)
                        {
                            Logging.HandleException(e, "Catalog.HandlePurchase");
                            return;
                        }

                        break;

                    #endregion

                    case InteractionType.roomeffect:

                        Double Number = 0;

                        try
                        {
                            if (string.IsNullOrEmpty(ExtraData))
                                Number = 0;
                            else
                                Number = Double.Parse(ExtraData, OtanixEnvironment.cultureInfo);
                        }
                        catch (Exception e) { Logging.HandleException(e, "Catalog.HandlePurchase: " + ExtraData); }

                        ExtraData = Number.ToString().Replace(',', '.');
                        break; // maintain extra data // todo: validate

                    case InteractionType.postit:
                        ExtraData = "FFFF33";
                        break;

                    case InteractionType.dimmer:
                        ExtraData = "1,1,1,#000000,255";
                        break;

                    case InteractionType.trophy:
                        ExtraData = Session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) + OtanixEnvironment.FilterInjectionChars(ExtraData, true);
                        break;

                    case InteractionType.bot:
                        ExtraData = Item.BotLook;
                        break;

                    case InteractionType.badge_display:
                        ExtraData = ExtraData + ";" + Session.GetHabbo().Username + ";" + DateTime.Now.Date.ToString().Replace("/", "-");
                        break;

                    case InteractionType.maniqui:
                        ExtraData = "M;lg-270-82.ch-210-66;";
                        break;

                    case InteractionType.yttv:

                        if (OtanixEnvironment.GetGame().GetYoutubeManager().Videos.ContainsKey((int)Item.GetBaseItem(i).ItemId))
                        {
                            var ytbTV = OtanixEnvironment.GetGame().GetYoutubeManager().Videos[(int)Item.GetBaseItem(i).ItemId];
                            ExtraData = "1;" + ytbTV.favVideo;
                        }

                        break;

                    case InteractionType.userslock:
                    case InteractionType.piñata:
                    case InteractionType.balloon15:
                    case InteractionType.dalia:
                        ExtraData = "0";
                        break;

                    case InteractionType.fbgate:
                        ExtraData = "hd-180-14.ch-210-1408.lg-270-1408,hd-600-14.ch-630-1408.lg-695-1408";
                        break;

                    case InteractionType.seed:
                        if (Item.GetBaseItem(i).Name.Contains("rare"))
                            ExtraData = new Random().Next(8, 12).ToString();
                        else
                            ExtraData = new Random().Next(12).ToString();
                        break;

                    default:
                        ExtraData = "";
                        break;
                }

                if (Item.GetBaseItem(i).IsGroupItem)
                {
                    var Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(uint.Parse(ExtraDataBackup));
                    ExtraData = "0;" + Group.Id + ";" + Group.HtmlColor1 + ";" + Group.HtmlColor2;
                }

                if (Item.GetBaseItem(i).LimitedStack > 0)
                {
                    if (ExtraData == "")
                        ExtraData = "0;" + Item.LimitedSelled;
                    else
                        ExtraData = ExtraData + ";" + Item.LimitedSelled;
                }

                Session.GetMessageHandler().GetResponse().Init(Outgoing.PurchaseOKMessageOfferData);
                Session.GetMessageHandler().GetResponse().AppendUInt(Item.GetBaseItem(i).ItemId);
                Session.GetMessageHandler().GetResponse().AppendString(Item.GetBaseItem(i).Name);
                Session.GetMessageHandler().GetResponse().AppendBoolean(false);
                Session.GetMessageHandler().GetResponse().AppendUInt(Item.CreditsCost);
                Session.GetMessageHandler().GetResponse().AppendUInt((Item.DiamondsCost > 0) ? Item.DiamondsCost : Item.DucketsCost);
                Session.GetMessageHandler().GetResponse().AppendInt32((Item.DiamondsCost > 0) ? 105 : 0);
                Session.GetMessageHandler().GetResponse().AppendBoolean(true);
                Session.GetMessageHandler().GetResponse().AppendInt32(1);
                Session.GetMessageHandler().GetResponse().AppendString(Item.GetBaseItem(i).Type.ToString().ToLower());
                Session.GetMessageHandler().GetResponse().AppendInt32(Item.GetBaseItem(i).SpriteId);
                Session.GetMessageHandler().GetResponse().AppendString(string.Empty);
                Session.GetMessageHandler().GetResponse().AppendUInt(Item.Amount);
                Session.GetMessageHandler().GetResponse().AppendBoolean(false); // is limited.
                Session.GetMessageHandler().GetResponse().AppendInt32(0); // 0 = todos, 2 = club.
                Session.GetMessageHandler().GetResponse().AppendBoolean(Item.HaveOffer);
                Session.GetMessageHandler().SendResponse();

                #region Gift
                uint itemID = (uint)GiftSpriteId;
                if (itemID < EmuSettings.FIRST_PRESENT_ID || itemID > EmuSettings.LAST_PRESENT_SPRITEID) // Only Gift items
                    return;

                var Present = OtanixEnvironment.GetGame().GetItemManager().GetItem(itemID);
                if (Present == null)
                {
                    //Console.WriteLine("Error loading gift template from items, check your db! " + itemID);
                    return;
                }

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("INSERT INTO items (base_id) VALUES (" + Present.ItemId + ")");
                    itemID = (uint)dbClient.insertQuery();

                    dbClient.runFastQuery("INSERT INTO items_users VALUES (" + itemID + "," + GiftUserId + ")");

                    dbClient.setQuery("INSERT INTO items_extradata VALUES (" + itemID + ",@data)");
                    dbClient.addParameter("data", Session.GetHabbo().Id + ";" + (GiftLazo * 1000 + GiftColor) + ";" + GiftMessage);
                    dbClient.runQuery();

                    dbClient.setQuery("INSERT INTO user_presents (item_id,base_id,amount,extra_data) VALUES (" + itemID + "," + Item.GetBaseItem(i).ItemId + "," + Item.Amount + ",@extra_data)");
                    dbClient.addParameter("extra_data", ExtraData);
                    dbClient.runQuery();
                }

                var Receiver = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(GiftUserId);

                if (Receiver != null)
                {
                    /*if (Receiver.GetHabbo().Rank <= 5)
                    {
                        Receiver.SendNotif(LanguageLocale.GetValue("catalog.gift.received") + Session.GetHabbo().Username);
                    }*/

                    var u = Receiver.GetHabbo().GetInventoryComponent().AddNewItem(itemID, Present.ItemId, (Session.GetHabbo().Id + ";" + (GiftLazo * 1000 + GiftColor) + ";" + GiftMessage), false, false, (Page.MinRank > 1), Item.Name, Session.GetHabbo().Id, 0);

                    if (Item.BadgeName.Length > 0)
                        Receiver.GetHabbo().GetBadgeComponent().GiveBadge(Item.BadgeName);

                    Receiver.GetHabbo().GetInventoryComponent().UpdateItems(false);

                    Receiver.GetMessageHandler().GetResponse().Init(Outgoing.SendPurchaseAlert);
                    Receiver.GetMessageHandler().GetResponse().AppendInt32(1); // items
                    Receiver.GetMessageHandler().GetResponse().AppendInt32(1); // type (gift) == s
                    Receiver.GetMessageHandler().GetResponse().AppendInt32(1);
                    Receiver.GetMessageHandler().GetResponse().AppendUInt(u.Id);
                    Receiver.GetMessageHandler().SendResponse();
                }

                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(GiftUserId, "ACH_GiftReceiver", 1);
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_GiftGiver", 1);
                Session.SendNotif(LanguageLocale.GetValue("catalog.gift.sent"));
                #endregion
            }
            #endregion
        }

        internal static bool CheckPetName(string PetName)
        {
            if (PetName.Length < 1 || PetName.Length > 16)
            {
                return false;
            }

            if (!OtanixEnvironment.IsValidAlphaNumeric(PetName))
            {
                return false;
            }

            return true;
        }

        internal List<UserItem> DeliverItems(GameClient Session, Item Item, uint Amount, String ExtraData, Boolean SaveRareLog, uint songID = 0)
        {
            var result = new List<UserItem>();

            if (Session == null)
                return result;

            switch (Item.Type.ToString())
            {
                case "i":
                case "s":
                    for (var i = 0; i < Amount; i++)
                    {
                        switch (Item.InteractionType)
                        {
                            #region Pet Cases

                            case InteractionType.pet:
                                var petData = ExtraData.Split('\n');
                                var petId = int.Parse(Item.Name.Replace("a0 pet", ""));
                                var generatedPet = CreatePet(Session, petData[0], petId, petData[1], petData[2]);

                                Session.GetHabbo().GetInventoryComponent().AddPet(generatedPet);
                                // result.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0, (uint)EmuSettings.GetPetFood(petId), "0", true, false, SaveRareLog, Item.Name, Session.GetHabbo().Id, 0));

                                break;

                            #endregion

                            case InteractionType.teleport:

                                var one = Session.GetHabbo().GetInventoryComponent().AddNewItem(0, Item.ItemId, String.Empty, true, false, SaveRareLog, Item.Name, Session.GetHabbo().Id, 0);
                                var idOne = one.Id;
                                var two = Session.GetHabbo().GetInventoryComponent().AddNewItem(0, Item.ItemId, String.Empty, true, false, SaveRareLog, Item.Name, Session.GetHabbo().Id, 0);
                                var idTwo = two.Id;

                                one.ExtraData = "0;" + two.Id;
                                two.ExtraData = "0;" + one.Id;

                                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                {
                                    dbClient.runFastQuery("INSERT INTO items_extradata VALUES (" + one.Id + ",'" + one.ExtraData + "');");
                                    dbClient.runFastQuery("INSERT INTO items_extradata VALUES (" + two.Id + ",'" + two.ExtraData + "');");
                                }

                                result.Add(one);
                                result.Add(two);

                                break;

                            case InteractionType.saltasalas:

                                var item = Session.GetHabbo().GetInventoryComponent().AddNewItem(0, Item.ItemId, ExtraData, true, false, SaveRareLog, Item.Name, Session.GetHabbo().Id, songID);

                                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                {
                                    dbClient.runFastQuery("INSERT INTO items_jumping_rooms VALUES ('" + item.Id + "','" + Item.SpriteId + "','0')");
                                }

                                result.Add(item);

                                break;

                            case InteractionType.dimmer:

                                var it = Session.GetHabbo().GetInventoryComponent().AddNewItem(0, Item.ItemId, ExtraData, true, false, SaveRareLog, Item.Name, Session.GetHabbo().Id, 0);
                                var id = it.Id;
                                result.Add(it);
                                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                {
                                    dbClient.runFastQuery("INSERT INTO items_moodlight (item_id,enabled,current_preset,preset_one,preset_two,preset_three) VALUES (" + id + ",0,1,'#000000,255,0','#000000,255,0','#000000,255,0')");
                                }


                                break;

                            case InteractionType.musicdisc:
                                {
                                    result.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0, Item.ItemId, songID.ToString(), true, false, SaveRareLog, Item.Name, Session.GetHabbo().Id, songID));
                                    break;
                                }

                            default:

                                result.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0, Item.ItemId, ExtraData, true, false, SaveRareLog, Item.Name, Session.GetHabbo().Id, songID));

                                break;
                        }
                    }
                    return result;

                case "r":

                    result.Add(Session.GetHabbo().GetInventoryComponent().AddNewBot(0, Item.ItemId, ExtraData, true, false));

                    return result;

                case "e":

                    for (var i = 0; i < Amount; i++)
                    {
                        Session.GetHabbo().GetAvatarEffectsInventoryComponent().AddEffect(Item.SpriteId, 3600);
                    }

                    return result;

                default:

                    Session.SendNotif(LanguageLocale.GetValue("catalog.buyerror"));
                    return result;
            }
        }

        internal static RoomBot CreateBot(uint BotId, uint UserId, string OwnerName, string Name, string Motto, string Gender, string Look, AIType aitype)
        {
            var bot = new RoomBot(BotId, UserId, 0, aitype, false, Name, Motto, Gender, Look, 0, 0, 0, 0, false, "", 0, false);

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO bots (id, ownerId, name, motto, gender, look) VALUES (" + bot.BotId + ", " + bot.OwnerId + ", @botName, @botMotto, @botGender, @botLook)");
                dbClient.addParameter("botName", bot.Name);
                dbClient.addParameter("botMotto", bot.Motto);
                dbClient.addParameter("botGender", bot.Gender);
                dbClient.addParameter("botLook", bot.Look);
                dbClient.runQuery();
            }
            return bot;
        }

        internal RoomBot GenerateBot(GameClient Session, UserItem Item, String ExtraData)
        {
            if (Item.mBaseItem.Type == 'r')
            {
                RoomBot GenerateBot = null;
                if (Item.mBaseItem.Name.ToLower() == "bot_generic")
                    GenerateBot = CreateBot(Item.Id, Session.GetHabbo().Id, Session.GetHabbo().Username, "Robbie", "Beep beep.", "m", ExtraData, AIType.Generic);
                else if (Item.mBaseItem.Name.ToLower() == "bot_bartender")
                    GenerateBot = CreateBot(Item.Id, Session.GetHabbo().Id, Session.GetHabbo().Username, "Paula", "Serve bebidas!", "f", ExtraData, AIType.Waiter);
                else if (Item.mBaseItem.Name.ToLower() == "bot_visitor_logger")
                    GenerateBot = CreateBot(Item.Id, Session.GetHabbo().Id, Session.GetHabbo().Username, "Belle", "Te diz quem lhe visitou enquanto não estava.", "f", ExtraData, AIType.Soplon);

                if (GenerateBot != null)
                    Session.GetHabbo().GetInventoryComponent().AddBot(GenerateBot);

                return GenerateBot;
            }
            return null;
        }

        internal static RoomBot GenerateBotFromRow(DataRow Row, uint RoomId, AIType aitype)
        {
            if (Row == null)
            {
                return null;
            }

            return new RoomBot(Convert.ToUInt32(Row["id"]), Convert.ToUInt32(Row["ownerId"]), RoomId, aitype,
                    ((string)Row["walk_enabled"] == "1"), (String)Row["name"], (string)Row["motto"], (string)Row["gender"], (string)Row["look"],
                    (int)Row["x"], (int)Row["y"], (int)Row["z"], (int)Row["rotation"], ((string)Row["chat_enabled"] == "1"),
                    (string)Row["chat_text"], (int)Row["chat_seconds"], ((string)Row["is_dancing"] == "1"));
        }

        internal static Pet CreatePet(GameClient Session, string Name, int Type, string Race, string Color)
        {
            var pet = new Pet(404, Session.GetHabbo().Id, 0, Name, (uint)Type, Race, Color, 0, 100, 100, 0, OtanixEnvironment.GetUnixTimestamp(), 0, 0, 0.0, false, 0, 1, -1, GetAccessoriesPet(Type))
            {
                DBState = DatabaseUpdateState.NeedsUpdate
            };

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO user_pets (user_id,name,type,race,color,expirience,energy,createstamp,accessories) VALUES (" + pet.OwnerId + ",@" + pet.PetId + "name," + pet.Type + ",@" + pet.PetId + "race,@" + pet.PetId + "color,0,100,'" + pet.CreationStamp + "',@" + pet.PetId + "acc)");
                dbClient.addParameter(pet.PetId + "name", pet.Name);
                dbClient.addParameter(pet.PetId + "race", pet.Race);
                dbClient.addParameter(pet.PetId + "color", pet.Color);
                dbClient.addParameter(pet.PetId + "acc", pet.Accessories);
                pet.PetId = (uint)dbClient.insertQuery();
            }

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_PetLover", 1);

            return pet;
        }

        internal static string GetAccessoriesPet(int Type)
        {
            if (Type == 15) // horse
            {
                return " 2 2 -1 1 3 -1 1"; // default hair...
            }
            else if (Type == 26) // gnome
            {
                return Catalog.GenerateRandomGnomeLook(); // party colours
            }

            return "";
        }

        internal static string GenerateRandomGnomeLook()
        {
            string Look = " 5 ";

            // piel:
            Look += "0 -1 0 ";
            // pantalones:
            Look += "1 102 " + new Random().Next(6, 13) + " ";
            // camisa:
            Look += "2 201 " + new Random().Next(1, 7) + " ";
            // barba:
            Look += "3 301 " + new Random().Next(1, 8) + " ";
            // gorro:
            Look += "4 40" + new Random().Next(1, 3) + " " + new Random().Next(1, 10);

            return Look;
        }

        internal static Pet GeneratePetFromRow(DataRow Row)
        {
            if (Row == null)
            {
                return null;
            }

            return new Pet(Convert.ToUInt32(Row["id"]), Convert.ToUInt32(Row["user_id"]), Convert.ToUInt32(Row["room_id"]), (string)Row["name"], Convert.ToUInt32(Row["type"]), (string)Row["race"], (string)Row["color"], (int)Row["expirience"], (int)Row["energy"], (int)Row["nutrition"], (int)Row["respect"], (double)Row["createstamp"], (int)Row["x"], (int)Row["y"], (double)Row["z"], OtanixEnvironment.EnumToBool(Row["all_can_mount"].ToString()), (int)Row["have_saddle"], (int)Row["hairdye"], (int)Row["pethair"], (string)Row["accessories"]);
        }

        internal EcotronReward GetRandomEcotronReward()
        {
            uint Level = 1;

            if (new Random().Next(1, 2000) == 2000)
            {
                Level = 5;
            }
            else if (new Random().Next(1, 200) == 200)
            {
                Level = 4;
            }
            else if (new Random().Next(1, 40) == 40)
            {
                Level = 3;
            }
            else if (new Random().Next(1, 4) == 4)
            {
                Level = 2;
            }

            List<EcotronReward> PossibleRewards = GetEcotronRewardsForLevel(Level);

            if (PossibleRewards != null && PossibleRewards.Count >= 1)
            {
                return PossibleRewards[new Random().Next(0, (PossibleRewards.Count - 1))];
            }
            else
            {
                return new EcotronReward(0, 1479, 0); // eco lamp two :D
            }
        }

        internal List<EcotronReward> GetEcotronRewardsForLevel(uint Level)
        {
            List<EcotronReward> Rewards = new List<EcotronReward>();

            foreach (EcotronReward R in EcotronRewards)
            {
                if (R.RewardLevel == Level)
                {
                    Rewards.Add(R);
                }
            }

            return Rewards;
        }

        internal ServerMessage SerializeIndexForCache(int rank)
        {
            var Index = new ServerMessage(Outgoing.OpenShop);
            Index.AppendBoolean(true); // visible
            Index.AppendInt32(0); // icon
            Index.AppendInt32(-1); // pageId
            Index.AppendString("root"); // pageName
            Index.AppendString(""); // localization
            Index.AppendInt32(0); // items Count
            Index.AppendInt32(GetTreeSize(rank, -1)); // subPages Count

            foreach (var Page in Pages.Values)
            {
                if (Page.ParentId != -1 || Page.MinRank > rank)
                    continue;

                Page.Serialize(rank, Index); // Pestañas

                foreach (var _Page in Pages.Values)
                {
                    if (_Page.ParentId != Page.PageId)
                        continue;

                    if (_Page.MinRank > rank)
                        continue;

                    _Page.Serialize(rank, Index); // Páginas

                    foreach (var __Page in Pages.Values)
                    {
                        if (__Page.ParentId != _Page.PageId)
                            continue;

                        if (__Page.MinRank > rank)
                            continue;

                        __Page.Serialize(rank, Index); // SubPáginas
                    }
                }
            }

            Index.AppendBoolean(false); // is updated
            Index.AppendString("NORMAL"); // catalogType

            return Index;
        }

        internal ServerMessage GetIndexMessageForRank(uint Rank)
        {
            if (Rank < 1)
                Rank = 1;
            else if (Rank > Ranks.MAX_RANK_ID)
                Rank = Ranks.MAX_RANK_ID;

            return mCataIndexCache[Rank];
        }

        internal static ServerMessage SerializePage(CatalogPage Page)
        {
            var PageData = new ServerMessage(Outgoing.CatalogPageMessageParser);
            PageData.AppendInt32(Page.PageId);
            PageData.AppendString("NORMAL");

            switch (Page.Layout)
            {
                case "frontpage4":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.TextDetails);

                    break;

                case "guild_frontpage":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendInt32(3);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.TextDetails.Replace("[10]", Convert.ToChar(10).ToString()).Replace("[13]", Convert.ToChar(13).ToString()));
                    PageData.AppendString(Page.TextTeaser);

                    break;

                case "guild_custom_furni":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(3);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendString(Page.LayoutSpecial);
                    PageData.AppendInt32(3);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.TextDetails);
                    PageData.AppendString(Page.TextTeaser);

                    break;

                case "recycler_info":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendInt32(3);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.Text2);
                    PageData.AppendString(Page.TextDetails);

                    break;

                case "recycler_prizes":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(1);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendInt32(1);
                    PageData.AppendString(Page.Text1);

                    break;

                case "spaces_new":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(1);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendInt32(1);
                    PageData.AppendString(Page.Text1);

                    break;

                case "recycler":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendInt32(1);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.Text2);
                    PageData.AppendString(Page.TextDetails);

                    break;

                case "trophies":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(1);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.TextDetails);

                    break;

                case "pets":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendInt32(4);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(LanguageLocale.GetValue("catalog.pickname"));
                    PageData.AppendString(LanguageLocale.GetValue("catalog.pickcolor"));
                    PageData.AppendString(LanguageLocale.GetValue("catalog.pickrace"));

                    break;

                case "roomads":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.TextDetails);

                    break;

                case "soundmachine":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.TextDetails);

                    break;

                case "guild_forum":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(0);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.TextDetails.Replace("[10]", Convert.ToChar(10).ToString()));

                    break;

                case "single_bundle":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(3);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendString(Page.LayoutSpecial);
                    PageData.AppendInt32(4);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.TextDetails.Replace("[10]", Convert.ToChar(10).ToString()).Replace("[13]", Convert.ToChar(13).ToString()));
                    PageData.AppendString(Page.TextTeaser.Replace("[10]", Convert.ToChar(10).ToString()).Replace("[13]", Convert.ToChar(13).ToString()));
                    PageData.AppendString(Page.Text2.Replace("[10]", Convert.ToChar(10).ToString()).Replace("[13]", Convert.ToChar(13).ToString()));

                    break;

                case "pets2":
                case "pets3":

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(2);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendInt32(4);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.TextDetails.Replace("[10]", Convert.ToChar(10).ToString()).Replace("[13]", Convert.ToChar(13).ToString()));
                    PageData.AppendString(Page.TextTeaser.Replace("[10]", Convert.ToChar(10).ToString()).Replace("[13]", Convert.ToChar(13).ToString()));
                    PageData.AppendString(Page.Text2.Replace("[10]", Convert.ToChar(10).ToString()).Replace("[13]", Convert.ToChar(13).ToString()));

                    break;

                default:

                    PageData.AppendString(Page.Layout);
                    PageData.AppendInt32(3);
                    PageData.AppendString(Page.LayoutHeadline);
                    PageData.AppendString(Page.LayoutTeaser);
                    PageData.AppendString(Page.LayoutSpecial);
                    PageData.AppendInt32(3);
                    PageData.AppendString(Page.Text1);
                    PageData.AppendString(Page.TextDetails);
                    PageData.AppendString(Page.TextTeaser);

                    break;
            }

            PageData.AppendInt32(Page.Items.Count);
            foreach (CatalogItem Item in Page.Items.Values)
            {
                Item.Serialize(PageData);
            }

            PageData.AppendInt32(-1);
            PageData.AppendBoolean(false);

            if (Page.PageExtra.Count > 0)
            {
                PageData.AppendInt32(Page.PageExtra.Count);
                foreach (CatalogPageExtra PageExtra in Page.PageExtra)
                {
                    PageData.AppendInt32(PageExtra.GetId());
                    PageData.AppendString(PageExtra.GetCaption());
                    PageData.AppendString(PageExtra.GetImage());
                    PageData.AppendInt32(0);
                    PageData.AppendString(PageExtra.GetCode());
                    PageData.AppendInt32(-1);
                }
            }

            return PageData;
        }
    }
}
