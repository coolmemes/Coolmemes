using System;
using System.Collections.Generic;
using System.Data;
using Butterfly.Core;
using Database_Manager.Database.Session_Details.Interfaces;
using ButterStorm;

namespace Butterfly.HabboHotel.Items
{
    class ItemManager
    {
        private Dictionary<UInt32, Item> Items;

        internal ItemManager()
        {
            Items = new Dictionary<uint, Item>();
        }

        internal void LoadItems(IQueryAdapter dbClient)
        {
            Items = new Dictionary<uint, Item>();

            dbClient.setQuery("SELECT * FROM items_base");
            DataTable ItemData = dbClient.getTable();

            if (ItemData != null)
            {
                uint id;
                int spriteID;
                string itemName;
                string type;
                int width;
                int length;
                double height;
                bool allowStack;
                bool allowWalk;
                bool allowSit;
                bool allowRecycle;
                bool allowTrade;
                bool allowMarketplace;
                bool allowInventoryStack;
                bool allowRotations;
                InteractionType interactionType;
                int cycleCount;
                string vendingIDS;
                int limitedStack;
                string multiHeight;

                foreach (DataRow dRow in ItemData.Rows)
                {
                    try
                    {
                        id = Convert.ToUInt16(dRow["item_id"]);
                        spriteID = (int)dRow["sprite_id"];
                        itemName = (string)dRow["item_name"];
                        type = (string)dRow["type"];
                        width = (int)dRow["width"];
                        length = (int)dRow["length"];
                        height = Convert.ToDouble(dRow["height"]);
                        allowStack = Convert.ToInt32(dRow["allow_stack"]) == 1;
                        allowWalk = Convert.ToInt32(dRow["allow_walk"]) == 1;
                        allowSit = Convert.ToInt32(dRow["allow_sit"]) == 1;
                        allowRecycle = Convert.ToInt32(dRow["allow_recycle"]) == 1;
                        allowTrade = Convert.ToInt32(dRow["allow_trade"]) == 1;
                        allowMarketplace = Convert.ToInt32(dRow["allow_marketplace_sell"]) == 1;
                        allowInventoryStack = Convert.ToInt32(dRow["allow_inventory_stack"]) == 1;
                        allowRotations = Convert.ToInt32(dRow["allow_rotation"]) == 1;
                        interactionType = InterractionTypes.GetTypeFromString((string)dRow["interaction_type"]);
                        cycleCount = (int)dRow["cycle_count"];
                        vendingIDS = (string)dRow["vending_ids"];
                        limitedStack = (int)dRow["maxLtdItems"];
                        multiHeight = (string)dRow["multi_height"];

                        Item item = new Item(id, spriteID, itemName, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowInventoryStack, allowRotations, interactionType, cycleCount, vendingIDS, limitedStack, multiHeight);
                        Items.Add(id, item);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.ReadKey();
                        Logging.WriteLine("Could not load item #" + Convert.ToUInt32(dRow[0]) + ", please verify the data is okay.");
                    }
                }
            }
        }

        internal Boolean ContainsItem(uint Id)
        {
            if (Items == null)
                return false;

            return Items.ContainsKey(Id);
        }

        internal Item GetItem(uint Id)
        {
            if (ContainsItem(Id))
                return Items[Id];

            Logging.LogException("Unknown baseID: " + Id);
            return null;
        }

        internal uint GetBaseIdFromSpriteId(uint SpriteId)
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT item_id FROM items_base WHERE sprite_id = '" + SpriteId + "'");
                return Convert.ToUInt32(dbClient.getInteger());
            }
        }

        internal uint GetBaseIdFromItemName(string ItemName)
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT item_id FROM items_base WHERE item_name = @iname");
                dbClient.addParameter("iname", ItemName);
                return Convert.ToUInt32(dbClient.getInteger());
            }
        }

        internal void reloaditems()
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                LoadItems(dbClient);
            }
        }

        internal string GetItemNameByGiftId(uint GiftId)
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT base_id FROM user_presents WHERE item_id = " + GiftId + "");
                uint BaseId = (uint)dbClient.getInteger();
       
                Item Item = GetItem(BaseId);
                if (Item != null)
                    return Item.Name;

                return "";
            }
        }
    }
}
