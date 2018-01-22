using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Butterfly.Core;
using Butterfly.HabboHotel.Catalogs;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Pets;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database;
using Butterfly.Util;
using HabboEvents;
using Butterfly.HabboHotel.RoomBots;
using System.Collections.Specialized;
using Database_Manager.Database.Session_Details.Interfaces;

namespace Butterfly.HabboHotel.Users.Inventory
{
    class InventoryComponent
    {
        private readonly Dictionary<uint, AIType> inventoryHaveBots;

        private readonly HybridDictionary floorItems;
        private readonly HybridDictionary wallItems;
        private readonly HybridDictionary discs;

        private readonly Dictionary<UInt32, Pet> InventoryPets;
        private readonly Dictionary<UInt32, RoomBot> InventoryBots;
        private readonly HybridDictionary mAddedItems;
        private readonly ArrayList mRemovedItems;
        private GameClient mClient;

        internal bool inventoryDefined = false;
        internal bool inventoryPetsDefined = false;
        internal bool inventoryBotsDefined = false;

        internal uint UserId;
        internal uint InventaryUserId;

        internal void Destroy()
        {
            mClient = null;
            mAddedItems.Clear();
            mRemovedItems.Clear();
            floorItems.Clear();
            wallItems.Clear();
            discs.Clear();
            InventoryPets.Clear();
            InventoryBots.Clear();

        }
        internal InventoryComponent(uint UserId, GameClient Client)
        {
            this.mClient = Client;
            this.UserId = UserId;
            this.floorItems = new HybridDictionary();
            this.wallItems = new HybridDictionary();
            this.discs = new HybridDictionary();
            this.InventoryPets = new Dictionary<UInt32, Pet>();
            this.InventoryBots = new Dictionary<UInt32, RoomBot>();
            this.inventoryHaveBots = new Dictionary<uint, AIType>();
            this.mAddedItems = new HybridDictionary();
            this.mRemovedItems = new ArrayList();
            this.isUpdated = false;
            this.InventaryUserId = 0;
        }

        internal int GetTotalItems
        {
            get
            {
                return (floorItems.Count + wallItems.Count + discs.Count);
            }
        }

        internal void ClearItems()
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM items_users WHERE user_id = " + GetUserInventaryId());
            }

            mAddedItems.Clear();
            mRemovedItems.Clear();
            floorItems.Clear();
            wallItems.Clear();
            discs.Clear();
            InventoryPets.Clear();
            InventoryBots.Clear();
            isUpdated = true;

            mClient.GetMessageHandler().GetResponse().Init(Outgoing.UpdateInventary);
            GetClient().GetMessageHandler().SendResponse();
        }

        internal void ClearPets()
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM user_pets WHERE user_id = " + UserId + " AND room_id = 0");
            }

            InventoryPets.Clear();
            SerializePetInventory();
        }

        internal void ClearBots()
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT items.item_id FROM items JOIN items_users ON items_users.item_id = items.item_id WHERE items_users.user_id = " + UserId);
                DataTable dTable = dbClient.getTable();

                if(dTable != null)
                {
                    foreach(DataRow dRow in dTable.Rows)
                    {
                        int ItemId = int.Parse(dRow["item_id"].ToString());

                        dbClient.runFastQuery("DELETE FROM bots WHERE id = " + ItemId);
                        dbClient.runFastQuery("DELETE FROM items WHERE item_id = " + ItemId);
                        dbClient.runFastQuery("DELETE FROM items_users WHERE item_id = " + ItemId);
                    }
                }
            }

            InventoryBots.Clear();
            SerializeBotInventory();
        }

        internal void SetActiveState(GameClient client)
        {
            mClient = client;
            userAttatched = true;
        }

        internal void SetIdleState()
        {
            userAttatched = false;
            mClient = null;
        }

        #region PETS
        internal Pet GetPet(uint Id)
        {
            if (InventoryPets.ContainsKey(Id))
                return InventoryPets[Id] as Pet;
            return null;
        }

        internal bool RemovePet(uint PetId)
        {
            isUpdated = false;
            InventoryPets.Remove(PetId);
            return true;
        }

        internal void MovePetToRoom(UInt32 PetId)
        {
            isUpdated = false;
            RemovePet(PetId);
        }

        internal void AddPet(Pet Pet)
        {
            isUpdated = false;
            if (Pet == null || InventoryPets.ContainsKey(Pet.PetId))
                return;

            Pet.PlacedInRoom = false;
            Pet.RoomId = 0;

            InventoryPets.Add(Pet.PetId, Pet);
        }
        #endregion

        #region BOTS
        internal RoomBot GetBot(uint Id)
        {
            if (InventoryBots.ContainsKey(Id))
                return InventoryBots[Id] as RoomBot;
            return null;
        }

        internal bool RemoveBot(uint BotId)
        {
            isUpdated = false;
            InventoryBots.Remove(BotId);
            return true;
        }

        internal void MoveBotToRoom(UInt32 BotId)
        {
            isUpdated = false;
            RemoveBot(BotId);
        }

        internal void AddBot(RoomBot Bot)
        {
            isUpdated = false;

            if (!inventoryHaveBots.ContainsKey(Bot.BotId))
                inventoryHaveBots.Add(Bot.BotId, Bot.AiType);
           
            if (Bot == null)
                return;

            Bot.RoomId = 0;
            if (!InventoryBots.ContainsKey(Bot.BotId))
                InventoryBots.Add(Bot.BotId, Bot);
        }
#endregion

        internal void LoadInventory()
        {
            floorItems.Clear();
            wallItems.Clear();
            inventoryHaveBots.Clear();

            DataTable Data;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("CALL getuseritems(@userid)");
                dbClient.addParameter("userid", (int)GetUserInventaryId());

                Data = dbClient.getTable();
            }

            if (Data.Rows.Count >= EmuSettings.INVENTARY_ITEMS_LIMIT)
            {
                mClient.SendNotif("Se cargarán los primeros " + EmuSettings.INVENTARY_ITEMS_LIMIT + " items de tu inventario, deja algunos para mostrar el resto.");
            }

            uint id;
            uint baseitem;
            string extradata;
            foreach (DataRow Row in Data.Rows)
            {
                id = Convert.ToUInt32(Row[0]);
                baseitem = Convert.ToUInt32(Row[1]);

                if (!DBNull.Value.Equals(Row[2]))
                    extradata = (string)Row[2];
                else
                    extradata = string.Empty;

                var item = new UserItem(id, baseitem, extradata);

                if (item.mBaseItem.Type.ToString().ToLower().Equals("r"))
                {
                    AIType typeBot = AIType.Generic;
                    if (item.mBaseItem.Name.ToLower() == "bot_bartender")
                        typeBot = AIType.Waiter;
                    else if (item.mBaseItem.Name.ToLower() == "bot_visitor_logger")
                        typeBot = AIType.Soplon;

                    inventoryHaveBots.Add(item.Id, typeBot);
                    continue;
                }

                if (item.mBaseItem.InteractionType == InteractionType.musicdisc)
                {
                    if (!discs.Contains(id))
                        discs.Add(id, item);
                }
                
                if (item.isWallItem)
                {
                    if (!wallItems.Contains(id))
                        wallItems.Add(id, item);
                }
                else
                {
                    if (!floorItems.Contains(id))
                        floorItems.Add(id, item);
                }
            }
        }

        internal void LoadPetsInventory()
        {
            InventoryPets.Clear();
            DataTable Data2;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id, user_id, room_id, name, type, race, color, expirience, energy, nutrition, respect, createstamp, x, y, z, all_can_mount, have_saddle, hairdye, pethair, accessories FROM user_pets WHERE user_id = " + UserId + " AND room_id = 0");
                Data2 = dbClient.getTable();
            }

            if (Data2 != null)
            {
                foreach (DataRow Row in Data2.Rows)
                {
                    var newPet = Catalog.GeneratePetFromRow(Row);
                    InventoryPets.Add(newPet.PetId, newPet);
                }
            }
        }

        internal void LoadBotsInventory()
        {
            if (inventoryHaveBots.Count > 0) // Nuevo método añadido en la [RELEASE 135]:
            {
                InventoryBots.Clear();
                foreach (var BotId in inventoryHaveBots)
                {
                    DataRow Data3;
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.setQuery("SELECT * FROM bots WHERE id = " + BotId.Key);
                        Data3 = dbClient.getRow();
                    }

                    if (Data3 != null)
                    {
                        var newBot = Catalog.GenerateBotFromRow(Data3, 0, BotId.Value);
                        InventoryBots.Add(newBot.BotId, newBot);
                    }
                }
            }
        }

        internal void UpdateItems(bool FromDatabase)
        {
            if (FromDatabase)
            {
                RunDBUpdate();
                LoadInventory();
            }

            if (mClient == null)
                return;
            if (mClient.GetMessageHandler() == null)
                return;
            if (mClient.GetMessageHandler().GetResponse() == null)
                return;

            mClient.GetMessageHandler().GetResponse().Init(Outgoing.UpdateInventary);
            mClient.GetMessageHandler().SendResponse();
        }

        internal int getInventoryAmountofItems()
        {
            return (getFloorInventoryAmount() + getWallInventoryAmount());
        }

        internal int getFloorInventoryAmount()
        {
            return floorItems.Count + discs.Count;
        }

        internal int getWallInventoryAmount()
        {
            return wallItems.Count;
        }

        internal UserItem GetItem(uint Id)
        {
            isUpdated = false;
            if (floorItems.Contains(Id))
                return (UserItem)floorItems[Id];
            else if (wallItems.Contains(Id))
                return (UserItem)wallItems[Id];

            return null;
        }

        internal void RemoveItemByBaseId(uint BaseId)
        {
            foreach (UserItem item in floorItems.Values)
            {
                if (item.BaseItem == BaseId)
                {
                    floorItems.Remove(item.Id); 
                    return;
                }
            }

            foreach (UserItem item in wallItems.Values)
            {
                if (item.BaseItem == BaseId)
                {
                    wallItems.Remove(item.Id);
                    return;
                }
            }
        }

        internal uint GetBaseIdCount(uint BaseId)
        {
            uint Count = 0;
            foreach (UserItem item in floorItems.Values)
            {
                if (item.BaseItem == BaseId)
                    Count++;
            }

            if (Count > 0)
                return Count;

            foreach(UserItem item in wallItems.Values)
            {
                if (item.BaseItem == BaseId)
                    Count++;
            }

            return Count;
        }

        internal bool ContainsItem(uint Id)
        {
            if (floorItems.Contains(Id))
                return true;
            else if (wallItems.Contains(Id))
                return true;
            else
                return false;
        }

        internal UserItem AddNewItem(UInt32 Id, UInt32 BaseItem, string ExtraData, bool insert, bool fromRoom, Boolean SaveRareLog, string ItemName, UInt32 UserId, UInt32 songID)
        {
            isUpdated = false;
            if (insert)
            {
                if (fromRoom)
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("REPLACE INTO items_users VALUES (" + Id + "," + UserId + ")");
                    }

                    var baseItem = OtanixEnvironment.GetGame().GetItemManager().GetItem(BaseItem);

                    if (baseItem != null && baseItem.InteractionType == InteractionType.musicdisc)
                    {
                        using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.runFastQuery("DELETE FROM items_rooms_songs WHERE itemid = " + Id);
                        }
                    }
                }
                else
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.setQuery("INSERT INTO items (base_id) VALUES (" + BaseItem + ")");
                        Id = (uint)dbClient.insertQuery();

                        if (!string.IsNullOrEmpty(ExtraData))
                        {
                            dbClient.setQuery("INSERT INTO items_extradata VALUES (" + Id + ",@extradata)");
                            dbClient.addParameter("extradata", ExtraData);
                            dbClient.runQuery();
                        }

                        dbClient.runFastQuery("INSERT INTO items_users VALUES (" + Id + "," + UserId + ")");
                        
                        if (SaveRareLog)
                        {
                            dbClient.setQuery("INSERT INTO catalog_rares_logs VALUES (" + Id + ",@itemname," + UserId + ")");
                            dbClient.addParameter("itemname", ItemName);
                            dbClient.runQuery();
                        }
                    }
                }
            }

            var ItemToAdd = new UserItem(Id, BaseItem, ExtraData);
            if (UserHoldsItem(Id))
            {
                RemoveItem(Id, false);
            }

            if (ItemToAdd.mBaseItem.InteractionType == InteractionType.musicdisc)
                discs.Add(ItemToAdd.Id, ItemToAdd);
            if (ItemToAdd.isWallItem)
                wallItems.Add(ItemToAdd.Id, ItemToAdd);
            else
                floorItems.Add(ItemToAdd.Id, ItemToAdd);

            if (mRemovedItems.Contains(Id))
                mRemovedItems.Remove(Id);

            if (!mAddedItems.Contains(Id))
                mAddedItems.Add(Id, ItemToAdd);

            return ItemToAdd;
        }

        internal UserItem AddNewBot(UInt32 Id, UInt32 BaseItem, string ExtraData, bool insert, bool fromRoom)
        {
            isUpdated = false;
            if (insert)
            {
                if (fromRoom)
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("REPLACE INTO items_users VALUES (" + Id + "," + UserId + ")");
                    }
                }
                else
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.setQuery("INSERT INTO items (base_id) VALUES (" + BaseItem + ")");
                        Id = (uint)dbClient.insertQuery();

                        dbClient.runFastQuery("INSERT INTO items_users VALUES (" + Id + "," + UserId + ")");
                    }
                }
            }

            return new UserItem(Id, BaseItem, ExtraData);
        }

        private bool UserHoldsItem(uint itemID)
        {
            if (discs.Contains(itemID))
                return true;
            if (floorItems.Contains(itemID))
                return true;
            if (wallItems.Contains(itemID))
                return true;
            return false;
        }

        internal void RemoveItem(UInt32 Id, bool PlacedInroom)
        {
            isUpdated = false;
            GetClient().GetMessageHandler().GetResponse().Init(Outgoing.RemoveObjectFromInventory);
            GetClient().GetMessageHandler().GetResponse().AppendUInt(Id);
            GetClient().GetMessageHandler().SendResponse();

            if (mAddedItems.Contains(Id))
                mAddedItems.Remove(Id);
            if (mRemovedItems.Contains(Id))
                return;

            discs.Remove(Id);
            floorItems.Remove(Id);
            wallItems.Remove(Id);
            if (PlacedInroom)
                mRemovedItems.Add(Id);
        }

        internal ServerMessage SerializeItemInventory()
        {
            if (inventoryDefined == false)
            {
                LoadInventory();
                inventoryDefined = true;
            }

            ServerMessage Message = new ServerMessage(Outgoing.Inventory);
            Message.AppendInt32(1);
            Message.AppendInt32(0);
            Message.AppendInt32(floorItems.Count + discs.Count + wallItems.Count);
            foreach (UserItem item in wallItems.Values)
            {
                item.SerializeWall(Message, true);
            }
            foreach (UserItem item in floorItems.Values)
            {
                item.SerializeFloor(Message, true);
            }
            foreach (UserItem item in discs.Values)
            {
                item.SerializeFloor(Message, true);
            }
            return Message;
        }

        internal ServerMessage SerializePetInventory()
        {
            if (inventoryPetsDefined == false)
            {
                LoadPetsInventory();
                inventoryPetsDefined = true;
            }

            ServerMessage Message = new ServerMessage(Outgoing.PetInventory);
            Message.AppendInt32(1); // ??
            Message.AppendInt32(0); // ??
            Message.AppendInt32(InventoryPets.Count); // petsCount

            foreach (var Pet in InventoryPets.Values)
            {
                Pet.SerializeInventory(Message);
            }

            return Message;
        }

        internal ServerMessage SerializeBotInventory()
        {
            if (inventoryBotsDefined == false)
            {
                LoadBotsInventory();
                inventoryBotsDefined = true;
            }

            ServerMessage Message = new ServerMessage(Outgoing.SerializeBotInventory);
            Message.AppendInt32(InventoryBots.Count);

            foreach (var Bot in InventoryBots.Values)
            {
                Bot.SerializeInventory(Message);
            }

            return Message;
        }

        private GameClient GetClient()
        {
            return OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
        }

        internal void AddItem(RoomItem item)
        {
            AddNewItem(item.Id, item.BaseItem, item.ExtraData, true, true, false, item.GetBaseItem().Name, GetUserInventaryId(), 0);
        }


        private bool isUpdated;
        internal void RunCycleUpdate()
        {
            isUpdated = true;
            RunDBUpdate();
        }

        internal void RunDBUpdate()
        {
            try
            {
                if (mRemovedItems.Count > 0 || mAddedItems.Count > 0 || InventoryPets.Count > 0)
                {
                    QueryChunk queries = new QueryChunk();

                    if (mAddedItems.Count > 0) // This should be checked more carefully
                    {
                        foreach (UserItem Item in mAddedItems.Values)
                        {
                            queries.AddQuery("UPDATE items_users SET user_id = " + GetUserInventaryId() + " WHERE item_id = " + Item.Id);
                        }

                        mAddedItems.Clear();
                    }

                    if (mRemovedItems.Count > 0)
                    {
                        foreach (UInt32 ItemID in mRemovedItems.ToArray())
                        {
                            queries.AddQuery("DELETE FROM items_users WHERE item_id=" + ItemID + " AND user_id=" + GetUserInventaryId());
                        }

                        mRemovedItems.Clear();
                    }

                    foreach (var pet in InventoryPets.Values)
                    {
                        if (pet.DBState == DatabaseUpdateState.NeedsUpdate)
                        {

                            queries.AddParameter(pet.PetId + "name", pet.Name);
                            queries.AddParameter(pet.PetId + "race", pet.Race);
                            queries.AddParameter(pet.PetId + "color", pet.Color);
                            queries.AddQuery("UPDATE user_pets SET room_id = " + pet.RoomId + ", name = @" + pet.PetId + "name, race = @" + pet.PetId + "race, color = @" + pet.PetId + "color, type = " + pet.Type + ", expirience = " + pet.Expirience + ", " +
                                "energy = " + pet.Energy + ", nutrition = " + pet.Nutrition + ", respect = " + pet.Respect + ", createstamp = '" + pet.CreationStamp + "', x = " + pet.X + ", Y = " + pet.Y + ", Z = " + pet.Z + " WHERE id = " + pet.PetId);
                        }
                        pet.DBState = DatabaseUpdateState.Updated;
                    }

                    using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        queries.Execute(dbClient);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogCacheError("FATAL ERROR DURING USER INVENTORY DB UPDATE: " + e);
            }
        }

        private bool userAttatched = false;

        internal bool NeedsUpdate
        {
            get
            {
                return (!userAttatched && !isUpdated);
            }
        }

        internal List<Pet> GetPets()
        {
            return InventoryPets.Values.ToList();
        }

        public bool isInactive
        {
            get
            {
                return !userAttatched;
            }
        }

        internal void SendInventoryUpdate()
        {
            mClient.SendMessage(SerializeItemInventory());
        }

        internal HybridDictionary songDisks
        {
            get
            {
                return discs;
            }
        }

        internal void LoadUserInventory(uint InventaryId)
        {
            this.RunDBUpdate();

            this.InventaryUserId = InventaryId;
            this.LoadInventory();
        }

        private uint GetUserInventaryId()
        {
            if (this.InventaryUserId > 0)
                return this.InventaryUserId;
            else
                return UserId;
        }
    }
}
