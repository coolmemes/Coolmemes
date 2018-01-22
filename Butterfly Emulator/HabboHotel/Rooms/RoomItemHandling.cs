using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Rooms.Wired;
using Butterfly.Util;
using HabboEvents;
using Butterfly.HabboHotel.RoomBots;
using Butterfly.HabboHotel.Catalogs;
using Butterfly.HabboHotel.Pets;
using System.Collections.Specialized;
using Butterfly.HabboHotel.Pathfinding;
using Butterfly.HabboHotel.Users;
using System.Collections.Concurrent;
using Butterfly.HabboHotel.Premiums;

namespace Butterfly.HabboHotel.Rooms
{
    class RoomItemHandling
    {
        private Room room;

        internal ConcurrentDictionary<uint, RoomItem> mFloorItems;
        internal ConcurrentDictionary<uint, RoomItem> mWallItems;

        private HybridDictionary mRemovedItems;
        private HybridDictionary mMovedItems;
        private HybridDictionary mAddedItems;
        private HybridDictionary mWiredItems;

        internal Dictionary<UInt32, RoomItem> breedingPet;
        internal Dictionary<UInt32, RoomItem> waterBowls;
        internal Dictionary<UInt32, RoomItem> petHomes;
        internal Dictionary<UInt32, RoomItem> petFoods;

        internal ConcurrentDictionary<uint, RoomItem> mRollers;
        private readonly List<uint> rollerItemsMoved;
        private readonly List<uint> rollerUsersMoved;
        private readonly List<ServerMessage> rollerMessages;

        private bool mGotRollers;
        private int mRoolerCycle;

        private Queue roomItemUpdateQueue;

        internal bool GotRollers
        {
            get
            {
                return mGotRollers;
            }
            set
            {
                mGotRollers = value;
            }
        }

        internal HybridDictionary _mRemovedItems
        {
            get
            {
                return mRemovedItems;
            }
        }

        public RoomItemHandling(Room room)
        {
            this.room = room;

            mRemovedItems = new HybridDictionary();
            mMovedItems = new HybridDictionary();
            mAddedItems = new HybridDictionary();
            mWiredItems = new HybridDictionary();
            mRollers = new ConcurrentDictionary<uint, RoomItem>();

            breedingPet = new Dictionary<uint, RoomItem>();
            waterBowls = new Dictionary<UInt32, RoomItem>();
            petHomes = new Dictionary<uint, RoomItem>();
            petFoods = new Dictionary<UInt32, RoomItem>();

            mWallItems = new ConcurrentDictionary<uint, RoomItem>();
            mFloorItems = new ConcurrentDictionary<uint, RoomItem>();
            roomItemUpdateQueue = new Queue();
            mGotRollers = false;
            mRoolerCycle = 0;

            rollerItemsMoved = new List<uint>();
            rollerUsersMoved = new List<uint>();
            rollerMessages = new List<ServerMessage>();
        }

        internal void QueueRoomItemUpdate(RoomItem item)
        {
            lock (roomItemUpdateQueue.SyncRoot)
            {
                roomItemUpdateQueue.Enqueue(item);
            }
        }

        internal void RemoveAllFurniture(GameClient Session)
        {
            if (Session.GetHabbo().HasFuse("fuse_any_room_rights"))
            {
                #region Code
                foreach (var Item in mFloorItems.Values.ToArray())
                {
                    Item.GetRoom().GetRoomItemHandler().RemoveFurniture(Session, Item);
                    if(!Item.IsPremiumItem)
                        Session.GetHabbo().GetInventoryComponent().AddItem(Item);
                }

                foreach (var Item in mWallItems.Values.ToArray())
                {
                    Item.GetRoom().GetRoomItemHandler().RemoveFurniture(Session, Item);
                    if (!Item.IsPremiumItem)
                        Session.GetHabbo().GetInventoryComponent().AddItem(Item);
                }

                Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                #endregion
            }
            else
            {
                List<Habbo> mHabbos = new List<Habbo>();

                foreach (var Item in mFloorItems.Values.ToArray())
                {
                    Item.GetRoom().GetRoomItemHandler().RemoveFurniture(Session, Item);
                    
                    Habbo User = UsersCache.getHabboCache(Item.OwnerId);
                    if (User != null && User.GetInventoryComponent() != null)
                    {
                        if (!mHabbos.Contains(User))
                            mHabbos.Add(User);

                        if (!Item.IsPremiumItem)
                            User.GetInventoryComponent().AddItem(Item);
                    }
                    else
                    {
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.runFastQuery("REPLACE INTO items_users VALUES (" + Item.Id + "," + Item.OwnerId + ")");
                        }
                    }
                }

                foreach (var Item in mWallItems.Values.ToArray())
                {
                    Item.GetRoom().GetRoomItemHandler().RemoveFurniture(Session, Item);

                    Habbo User = UsersCache.getHabboCache(Item.OwnerId);
                    if (User != null && User.GetInventoryComponent() != null)
                    {
                        if (!mHabbos.Contains(User))
                            mHabbos.Add(User);

                        if (!Item.IsPremiumItem)
                            User.GetInventoryComponent().AddItem(Item);
                    }
                    else
                    {
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.runFastQuery("REPLACE INTO items_users VALUES (" + Item.Id + "," + Item.OwnerId + ")");
                        }
                    }
                }

                foreach (Habbo mHabbo in mHabbos)
                {
                    mHabbo.GetInventoryComponent().UpdateItems(false);
                }

                mHabbos.Clear();
                mHabbos = null;
            }

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                SaveFurniture(dbClient);
            }

            room.GetGameMap().GenerateMaps();
            room.GetRoomUserManager().OnUserUpdateStatus();
            // room.GetRoomUserManager().UpdateUsersPath = true;

            if (room.GotWired())
            {
                room.GetWiredHandler().OnPickall();
            }
        }

        internal void RemoveUserFurniture(GameClient Session)
        {
            foreach (RoomItem Item in mFloorItems.Values.ToArray())
            {
                if (Item.OwnerId != room.RoomData.OwnerId)
                {
                    Item.GetRoom().GetRoomItemHandler().RemoveFurniture(Session, Item);

                    GameClient OwnerSession = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Item.OwnerId);
                    if (OwnerSession != null && OwnerSession.GetHabbo() != null && OwnerSession.GetHabbo().GetInventoryComponent() != null)
                    {
                        OwnerSession.GetHabbo().GetInventoryComponent().AddItem(Item);
                    }
                    else
                    {
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.runFastQuery("REPLACE INTO items_users VALUES (" + Item.Id + "," + Item.OwnerId + ")");
                        }
                    }
                }
            }

            foreach (RoomItem Item in mWallItems.Values.ToArray())
            {
                if (Item.OwnerId != room.RoomData.OwnerId)
                {
                    Item.GetRoom().GetRoomItemHandler().RemoveFurniture(Session, Item);

                    GameClient OwnerSession = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Item.OwnerId);
                    if (OwnerSession != null && OwnerSession.GetHabbo() != null && OwnerSession.GetHabbo().GetInventoryComponent() != null)
                    {
                        OwnerSession.GetHabbo().GetInventoryComponent().AddItem(Item);
                    }
                    else
                    {
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.runFastQuery("REPLACE INTO items_users VALUES (" + Item.Id + "," + Item.OwnerId + ")");
                        }
                    }
                }
            }

            room.GetGameMap().GenerateMaps();
            room.GetRoomUserManager().OnUserUpdateStatus();

            if (room.GotWired())
            {
                room.GetWiredHandler().OnPickall();
            }
        }

        private void ClearRollers()
        {
            mRollers.Clear();
        }

        internal void SetSpeed(uint p)
        {
            room.RoomData.RollerSpeed = p;
            room.RoomData.roomNeedSqlUpdate = true;
        }

        internal void LoadFurniture()
        {
            mFloorItems.Clear();
            mWallItems.Clear();
            DataTable Data;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("CALL getroomitems(@roomid)");
                dbClient.addParameter("roomid", room.RoomId);

                Data = dbClient.getTable();

                uint premiumID;
                uint itemID;
                decimal x;
                decimal y;
                sbyte n;
                string w;
                uint baseID;
                string extradata;
                uint ownerid;
                WallCoordinate wallCoord;
                foreach (DataRow dRow in Data.Rows)
                {
                    itemID = Convert.ToUInt32(dRow[0]);
                    x = Convert.ToDecimal(dRow[1]);
                    y = Convert.ToDecimal(dRow[2]);
                    n = Convert.ToSByte(dRow[3]);
                    w = (string)dRow[4];
                    baseID = Convert.ToUInt32(dRow[5]);
                    if (DBNull.Value.Equals(dRow[6]))
                        extradata = string.Empty;
                    else
                        extradata = (string)dRow[6];
                    if (DBNull.Value.Equals(dRow[7]))
                        ownerid = room.RoomData.OwnerId;
                    else
                        ownerid = Convert.ToUInt32(dRow[7]);

                    if ((baseID >= EmuSettings.FIRST_PRESENT_ID && baseID <= EmuSettings.FIRST_PRESENT_ID + 9) && extradata == string.Empty)
                    {
                        extradata = "1;" + "Regalo Auto-Fixeado" + (char)5 + 1 + (char)5 + 1;
                        dbClient.setQuery("INSERT INTO items_extradata VALUES (" + itemID + ",@data)");
                        dbClient.addParameter("data", "1;" + "Regalo Auto-Fixeado" + (char)5 + 1 + (char)5 + 1);
                        dbClient.runQuery();
                    }

                    if (w.Length > 0) // Is wallitem
                    {
                        if (room.roomUserWallItems.ContainsKey(ownerid))
                            room.roomUserWallItems[ownerid]++;
                        else
                            room.roomUserWallItems.Add(ownerid, 1);

                        wallCoord = new WallCoordinate(w);
                        var item = new RoomItem(itemID, room.RoomId, baseID, extradata, ownerid, wallCoord, room, false);

                        if (!mWallItems.ContainsKey(itemID))
                            mWallItems.TryAdd(itemID, item);

                        if (item.GetBaseItem().InteractionType == InteractionType.dimmer)
                        {
                            if (room.MoodlightData == null)
                                room.MoodlightData = new MoodlightData(item.Id);
                        }
                    }
                    else // Is flooritem
                    {
                        if (room.roomUserFloorItems.ContainsKey(ownerid))
                            room.roomUserFloorItems[ownerid]++;
                        else
                            room.roomUserFloorItems.Add(ownerid, 1);

                        int coordX, coordY;
                        TextHandling.Split((double)x, out coordX, out coordY);

                        var item = new RoomItem(itemID, room.RoomId, baseID, extradata, ownerid, coordX, coordY, (double)y, n, room, false);
                        if (item.GetBaseItem().Type.ToString().ToLower().Equals("r"))
                        {
                            #region Generate Bot || Stable Method [RELEASE135]
                            DataRow dBotRow = null;
                            dbClient.setQuery("SELECT * FROM bots WHERE id = " + item.Id + " LIMIT 1");
                            dBotRow = dbClient.getRow();
                            if (Data == null)
                                continue;

                            AIType typeBot = AIType.Generic;
                            if (item.GetBaseItem().Name.ToLower() == "bot_bartender")
                                typeBot = AIType.Waiter;
                            else if (item.GetBaseItem().Name.ToLower() == "bot_visitor_logger")
                                typeBot = AIType.Soplon;

                            var Bot = Catalog.GenerateBotFromRow(dBotRow, room.Id, typeBot);
                            if (Bot == null)
                                continue;

                            var BotUser = room.GetRoomUserManager().DeployBot(new RoomBot(Bot.BotId, Bot.OwnerId, Bot.RoomId, Bot.AiType, Bot.WalkingEnabled, Bot.Name, Bot.Motto, Bot.Gender, Bot.Look, Bot.X, Bot.Y, 0, Bot.Rot, Bot.ChatEnabled, Bot.ChatText, Bot.ChatSeconds, Bot.IsDancing), null);

                            if (Bot.IsDancing)
                                BotUser.DanceId = 3;

                            continue;
                            #endregion
                        }

                        if (!mFloorItems.ContainsKey(itemID))
                            mFloorItems.TryAdd(itemID, item);

                        if (item.IsRoller)
                        {
                            mGotRollers = true;
                        }               
                    }
                }

                // Premium Loading.
                if (PremiumManager.UserIsSubscribed(room.RoomData.OwnerId))
                {
                    dbClient.setQuery("SELECT * FROM items_premium WHERE room_id = " + room.RoomId);
                    Data = dbClient.getTable();

                    foreach (DataRow Row in Data.Rows)
                    {
                        premiumID = Convert.ToUInt32(Row["item_id"]);
                        baseID = Convert.ToUInt32(Row["base_id"]);
                        itemID = EmuSettings.PREMIUM_BASEID + Convert.ToUInt32(Row["premium_id"]);
                        x = Convert.ToDecimal(Row["x"]);
                        y = Convert.ToDecimal(Row["y"]);
                        n = Convert.ToSByte(Row["n"]);
                        w = (string)Row["w"];
                        extradata = (string)Row["data"];

                        if (w.Length > 0) // wallItem
                        {
                            wallCoord = new WallCoordinate(w);
                            RoomItem item = new RoomItem(itemID, room.RoomId, baseID, extradata, room.RoomData.OwnerId, wallCoord, room, true);
                            item.PremiumId = premiumID;

                            if (!mWallItems.ContainsKey(itemID))
                                mWallItems.TryAdd(itemID, item);
                        }
                        else
                        {
                            int coordX, coordY;
                            TextHandling.Split((double)x, out coordX, out coordY);

                            RoomItem item = new RoomItem(itemID, room.RoomId, baseID, extradata, room.RoomData.OwnerId, coordX, coordY, (double)y, n, room, true);
                            item.PremiumId = premiumID;

                            if (!mFloorItems.ContainsKey(itemID))
                                mFloorItems.TryAdd(itemID, item);
                        }
                    }
                }

                // esto se puede mejorar.
                foreach (RoomItem item in this.mFloorItems.Values)
                {
                    if (WiredUtillity.TypeIsWired(item.GetBaseItem().InteractionType))
                    {
                        WiredLoader.LoadWiredItem(item, room, dbClient);
                    }
                }
            }
        }

        internal RoomItem GetItem(uint pId)
        {
            if (mFloorItems.ContainsKey(pId))
                return mFloorItems[pId];
            else if (mWallItems.ContainsKey(pId))
                return mWallItems[pId];
            else
                return null;
        }

        internal void RemoveFurniture(GameClient Session, RoomItem Item)
        {
            if (Item.GetBaseItem().InteractionType != InteractionType.gift)
                Item.Interactor.OnRemove(Session, Item);

            if (Item.IsPremiumItem)
            {
                GameClient Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Item.OwnerId);
                if (Client != null && Client.GetHabbo() != null)
                {
                    if (Item.OwnerId == Client.GetHabbo().Id)
                    {
                        Client.GetHabbo().GetPremiumManager().ModifyItemPosition((int)(Item.Id - EmuSettings.PREMIUM_BASEID), false);
                        Client.GetHabbo().GetPremiumManager().DecreaseItems();
                        Client.SendMessage(PremiumManager.SerializePremiumItemsCount(Client.GetHabbo()));
                    }
                }
            }

            RemoveRoomItem(Item);

            if (Item.wiredHandler != null)
            {
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    Item.wiredHandler.Dispose();
                    room.GetWiredHandler().RemoveFurniture(Item);
                }

                Item.wiredHandler = null;
            }

            if (Item.wiredCondition != null)
            {
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    Item.wiredCondition.Dispose();
                    room.GetWiredHandler().conditionHandler.RemoveConditionToTile(Item.Coordinate, Item.wiredCondition);
                }
                Item.wiredCondition = null;
            }
        }

        private void RemoveRoomItem(RoomItem Item)
        {
            if (Item.IsWallItem)
            {
                if (room.roomUserWallItems.ContainsKey(Item.OwnerId))
                {
                    if (room.roomUserWallItems[Item.OwnerId] > 1)
                        room.roomUserWallItems[Item.OwnerId]--;
                    else
                        room.roomUserWallItems.Remove(Item.OwnerId);
                }

                var Message = new ServerMessage(Outgoing.PickUpWallItem);
                Message.AppendString(Item.Id + String.Empty);
                Message.AppendUInt(Item.OwnerId);
                room.SendMessage(Message);
            }
            else if (Item.IsFloorItem)
            {
                if (room.roomUserFloorItems.ContainsKey(Item.OwnerId))
                {
                    if (room.roomUserFloorItems[Item.OwnerId] > 1)
                        room.roomUserFloorItems[Item.OwnerId]--;
                    else
                        room.roomUserFloorItems.Remove(Item.OwnerId);
                }

                var Message = new ServerMessage(Outgoing.PickUpFloorItem);
                Message.AppendString(Item.Id + String.Empty);
                Message.AppendBoolean(false);
                Message.AppendUInt(Item.OwnerId);
                Message.AppendInt32(0);
                room.SendMessage(Message);
            }

            if (Item.GetBaseItem().InteractionType != InteractionType.teleport && Item.GetBaseItem().InteractionType != InteractionType.saltasalas)
                RemoveItem(Item);

            if (Item.IsWallItem)
            {
                RoomItem junk;
                mWallItems.TryRemove(Item.Id, out junk);
            }
            else
            {
                if (Item.GetBaseItem().InteractionType == InteractionType.teleport)
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("DELETE FROM items_rooms WHERE item_id = " + Item.Id + " AND room_id = " + room.RoomId);
                    }
                }
                else if (Item.GetBaseItem().InteractionType == InteractionType.saltasalas)
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("UPDATE items_jumping_rooms SET room_id = '0' WHERE item_id = '" + Item.Id + "'");
                    }
                }
                else if (Item.GetBaseItem().Name.ToLower().StartsWith("tile_stackmagic"))
                {
                    room.GetGameMap().ItemHeightMap[Item.GetX, Item.GetY] = 0.0;
                }

                RoomItem junk;
                mFloorItems.TryRemove(Item.Id, out junk);
                room.GetGameMap().RemoveFromMap(Item);
                Item.ClearCoordinates();

                room.GetRoomUserManager().OnUserUpdateStatus();
                // room.GetRoomUserManager().UpdateUsersPath = true;
            }
        }

        internal void UpdateWallItems(int WallHeightOld, int WallHeight)
        {
            //bool bulean = (WallHeightOld > WallHeight); // si backup > nuevo, entonces estamos haciendo la pared pequeña: dividimos. En caso contrario, multiplicamos.
            int bulean = 0;
            if (WallHeightOld < WallHeight)
                bulean = 1; // Se a parede nova for maior, bulean = 1
            else if (WallHeightOld == WallHeight)
                bulean = 2; // Se a parede nova for igual a antiga, bulean = 2
            else
               bulean = 3; // Se a parede nova for menor que a antiga, bulean = 3
              
            int valor = Math.Abs(WallHeightOld - WallHeight);

            foreach (RoomItem Item in mWallItems.Values)
            {
                Item.wallCoord.UpdateLengthY(bulean, valor, WallHeightOld, WallHeight);
                UpdateItem(Item);
            }
        }

        private List<ServerMessage> CycleRollers()
        {
            if (mGotRollers)
            {
                if (mRoolerCycle >= room.RoomData.RollerSpeed || room.RoomData.RollerSpeed == 0)
                {
                    rollerItemsMoved.Clear();
                    rollerUsersMoved.Clear();
                    rollerMessages.Clear();

                    foreach (RoomItem Item in mRollers.Values)
                    {
                        // Obtenemos la baldosa siguiente donde se movera el item/user.
                        Point NextCoord = Item.SquareInFront;

                        // Obtenemos el Usuario que será movido si este existe.
                        RoomUser UserOnRoller = room.GetRoomUserManager().GetUserForSquare(Item.GetX, Item.GetY);

                        // Obtenemos los items que están encima del roller los cuales se moverán.
                        List<RoomItem> ItemsOnRoller = room.GetGameMap().GetRoomItemForMinZ(Item.GetX, Item.GetY, Item.TotalHeight);

                        if (ItemsOnRoller.Count > 0 || UserOnRoller != null)
                        {
                            // Obtenemos los items que están en la baldosa destino.
                            List<RoomItem> ItemsOnNext = room.GetGameMap().GetCoordinatedItems(NextCoord);

                            var NextRoller = false;

                            var NextRollerZ = 0.0;
                            var NextRollerClear = true;

                            foreach (RoomItem tItem in ItemsOnNext)
                            {
                                // Si en la siguiente baldosa hay un roller:
                                if (tItem.IsRoller)
                                {
                                    NextRoller = true;
                                    if (tItem.TotalHeight > NextRollerZ)
                                        NextRollerZ = tItem.TotalHeight;
                                }
                                else if(tItem.GetBaseItem().Name.Contains("doormat_"))
                                {
                                    NextRollerClear = false;
                                }
                                else if (NextRoller)
                                {
                                    // En el caso que exista, comprueba si hay un item encima
                                    if (tItem.TotalHeight > NextRollerZ)
                                        NextRollerClear = false;

                                    break;
                                }
                            }

                            // Comprueba si hay un usuario en el siguiente roller.
                            bool userOnNext = room.GetGameMap().SquareHasUsers(NextCoord.X, NextCoord.Y);

                            if (ItemsOnRoller.Count > 0)
                            {
                                foreach (RoomItem tItem in ItemsOnRoller)
                                {
                                    double NextZ = tItem.GetZ + (!NextRoller ? -Item.TotalHeight : 0);
                                    if (room.GetGameMap().CanRollItemHere(NextCoord.X, NextCoord.Y, tItem.GetZ, false))
                                    {
                                        if (!rollerItemsMoved.Contains(tItem.Id) && NextRollerClear && !userOnNext)
                                        {
                                            if (tItem.GetZ - room.GetGameMap().ItemHeightMap[NextCoord.X, NextCoord.Y] <= 1.5)
                                                NextZ = room.GetGameMap().ItemHeightMap[NextCoord.X, NextCoord.Y];

                                            rollerMessages.Add(UpdateItemOnRoller(tItem, NextCoord, Item.Id, NextZ));
                                            SetFloorItem(tItem, NextCoord.X, NextCoord.Y, NextZ);
                                            ItemCoords.ModifyGamemapTiles(room, Item.GetAffectedTiles, Item.GetBackupAffectedTiles);
                                            rollerItemsMoved.Add(tItem.Id);
                                        }
                                    }
                                }
                            }

                            if (UserOnRoller != null && !UserOnRoller.Statusses.ContainsKey("sit") && !UserOnRoller.SetStep && NextRollerClear && !userOnNext && room.GetGameMap().CanRollItemHere(NextCoord.X, NextCoord.Y, UserOnRoller.Z, true))
                            {
                                if (!rollerUsersMoved.Contains(UserOnRoller.HabboId))
                                {
                                    rollerMessages.Add(UpdateUserOnRoller(UserOnRoller, NextCoord, Item.Id));
                                    rollerUsersMoved.Add(UserOnRoller.HabboId);
                                }
                            }
                        }
                    }
                
                    mRoolerCycle = 0;
                    return rollerMessages;
                }
                else
                    mRoolerCycle++;
            }

            return new List<ServerMessage>();
        }

        private ServerMessage UpdateItemOnRoller(RoomItem pItem, Point NextCoord, uint pRolledID, Double NextZ)
        {
            var mMessage = new ServerMessage(Outgoing.ObjectOnRoller); // Cf
            mMessage.AppendInt32(pItem.GetX);
            mMessage.AppendInt32(pItem.GetY);
            mMessage.AppendInt32(NextCoord.X);
            mMessage.AppendInt32(NextCoord.Y);
            mMessage.AppendInt32(1);
            mMessage.AppendUInt(pItem.Id);
            mMessage.AppendString(TextHandling.GetString(pItem.GetZ));
            mMessage.AppendString(TextHandling.GetString(NextZ));
            mMessage.AppendUInt(pRolledID);
            return mMessage;
        }

        public ServerMessage UpdateUserOnRoller(RoomUser pUser, Point pNextCoord, uint pRollerID)
        {
            // La baldosa del roller la devolvemos a su normalidad.
            room.GetGameMap().GameMap[pUser.X, pUser.Y] = 1;

            // La baldosa donde aparecerá el usuario queda guardado su estado.
            pUser.SqState = room.GetGameMap().GameMap[pNextCoord.X, pNextCoord.Y];

            // Actualizamos la caché de la posición del usuario.
            room.GetGameMap().UpdateUserMovement(new Point(pUser.X, pUser.Y), new Point(pNextCoord.X, pNextCoord.Y), pUser);

            // Hacemos una copia de las Coordenadas donde estaba el usuario.
            int OldX = pUser.X, OldY = pUser.Y;
            double OldZ = pUser.Z;

            // Actualizamos las Coordenadas del Usuario.
            pUser.X = pNextCoord.X;
            pUser.Y = pNextCoord.Y;

            // Retrasamos 1 cyclo (0.5s) su actualización.
            pUser.UpdateNeededCounter = 1;

            // Comprobamos si se trata de una silla, ducha... para meterle el efecto.
            room.GetRoomUserManager().UpdateUserStatus(pUser, true);

            // Enviamos el packet del efecto del roller.
            ServerMessage mMessage = new ServerMessage(Outgoing.ObjectOnRoller);
            mMessage.AppendInt32(OldX);
            mMessage.AppendInt32(OldY);
            mMessage.AppendInt32(pUser.X);
            mMessage.AppendInt32(pUser.Y);
            mMessage.AppendInt32(0);
            mMessage.AppendUInt(pRollerID);
            mMessage.AppendInt32(2);
            mMessage.AppendInt32(pUser.VirtualId);
            mMessage.AppendString(TextHandling.GetString(OldZ));
            mMessage.AppendString(TextHandling.GetString(pUser.Z));

            return mMessage;
        }

        internal void SaveFurniture(IQueryAdapter dbClient)
        {
            try
            {
                if (mAddedItems.Count > 0 || mRemovedItems.Count > 0 || mMovedItems.Count > 0 || mWiredItems.Count > 0 || room.GetRoomUserManager().PetCount > 0)
                {
                    var standardQueries = new QueryChunk();
                    var itemInserts = new QueryChunk("REPLACE INTO items_rooms (item_id,room_id,x,y,n,w) VALUES ");
                    var extradataInserts = new QueryChunk("REPLACE INTO items_extradata (item_id,data) VALUES ");
                    var ownersInserts = new QueryChunk("REPLACE INTO items_rooms_owners (item_id,owner_id) VALUES ");
                    var wiredInserts = new QueryChunk("REPLACE INTO items_wired (item_id, wired_data, wired_to_item, wired_original_location) VALUES ");

                    foreach (RoomItem Item in mRemovedItems.Values)
                    {
                        if (Item == null || Item.GetBaseItem() == null)
                            continue;

                        if (Item.IsPremiumItem)
                        {
                            standardQueries.AddQuery("DELETE FROM items_premium WHERE item_id = " + Item.PremiumId);
                        }
                        else
                        {
                            standardQueries.AddQuery("DELETE FROM items_rooms WHERE item_id = " + Item.Id + " AND room_id = " + room.RoomId); //Do join + function

                            if (room.RoomData.OwnerId != Item.OwnerId)
                            {
                                standardQueries.AddQuery("DELETE FROM items_rooms_owners WHERE item_id = " + Item.Id);
                            }
                        }
                    }

                    foreach (RoomItem Item in mAddedItems.Values)
                    {
                        if (Item == null || Item.GetBaseItem() == null)
                            continue;

                        if (Item.IsPremiumItem)
                        {
                            double combinedCoords = TextHandling.Combine(Item.GetX, Item.GetY);
                            if (Item.IsPremiumItem)
                            {
                                standardQueries.AddQuery("INSERT INTO items_premium VALUES (NULL," + Item.OwnerId + "," + Item.BaseItem + "," + (Item.Id - EmuSettings.PREMIUM_BASEID) + "," + Item.RoomId + ",'" + TextHandling.GetString(combinedCoords) + "','" + TextHandling.GetString(Item.GetZ) + "'," + Item.Rot + ",'" + (Item.IsWallItem ? Item.wallCoord.ToString() : "") + "',@extradata" + Item.Id + ")");
                                standardQueries.AddParameter("extradata" + Item.Id, Item.ExtraData);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(Item.ExtraData) && Item.GetBaseItem().InteractionType != InteractionType.teleport && Item.GetBaseItem().InteractionType != InteractionType.saltasalas)
                            {
                                if (Item.GetBaseItem().IsGroupItem && Item.GroupData != null)
                                {
                                    if (Item.GroupData.Split(';').Length < 4)
                                        continue;

                                    Item.ExtraData = Item.ExtraData + ";" + Item.GroupData.Split(';')[1] + ";" + Item.GroupData.Split(';')[2] + ";" + Item.GroupData.Split(';')[3];
                                }
                                if (Item.GetBaseItem().LimitedStack > 0)
                                {
                                    Item.ExtraData = Item.ExtraData + ";" + Item.LimitedValue;
                                }

                                extradataInserts.AddQuery("(" + Item.Id + ",@data_id" + Item.Id + ")");
                                extradataInserts.AddParameter("@data_id" + Item.Id, Item.ExtraData);
                            }

                            if (Item.IsFloorItem)
                            {
                                var combinedCoords = TextHandling.Combine(Item.GetX, Item.GetY);
                                itemInserts.AddQuery("(" + Item.Id + "," + Item.RoomId + "," + TextHandling.GetString(combinedCoords) + "," + TextHandling.GetString(Item.GetZ) + "," + Item.Rot + ",'')");
                            }
                            else
                            {
                                if (Item.wallCoord == null)
                                    continue;

                                itemInserts.AddQuery("(" + Item.Id + "," + Item.RoomId + ",'','','','" + Item.wallCoord.ToString() + "')");
                            }

                            if (room.RoomData.OwnerId != Item.OwnerId)
                            {
                                ownersInserts.AddQuery("(" + Item.Id + "," + Item.OwnerId + ")");
                            }
                        }
                    }

                    foreach (RoomItem Item in mMovedItems.Values)
                    {
                        if (Item == null || Item.GetBaseItem() == null || Item.GetBaseItem().Name == "guild_forum" || Item.GetBaseItem().InteractionType == InteractionType.guildgate)
                            continue;

                        if (Item.IsPremiumItem)
                        {
                            if (Item.IsWallItem)
                            {
                                standardQueries.AddQuery("UPDATE items_premium SET w='" + Item.wallCoord.ToString() + "', data=@data" + Item.Id + "  WHERE item_id = " + Item.PremiumId);
                                standardQueries.AddParameter("data" + Item.Id, Item.ExtraData);
                            }
                            else
                            {
                                double combinedCoords = TextHandling.Combine(Item.GetX, Item.GetY);

                                standardQueries.AddQuery("UPDATE items_premium SET x=" + TextHandling.GetString(combinedCoords) + ", y=" + TextHandling.GetString(Item.GetZ) + ", n=" + Item.Rot + ", data=@data" + Item.Id + " WHERE item_id = " + Item.PremiumId);
                                standardQueries.AddParameter("data" + Item.Id, Item.ExtraData);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(Item.ExtraData) && Item.GetBaseItem().InteractionType != InteractionType.teleport && Item.GetBaseItem().InteractionType != InteractionType.saltasalas)
                            {
                                if (Item.GetBaseItem().IsGroupItem)
                                {
                                    try { Item.ExtraData = Item.ExtraData + ";" + Item.GroupData.Split(';')[1] + ";" + Item.GroupData.Split(';')[2] + ";" + Item.GroupData.Split(';')[3]; }
                                    catch { Item.ExtraData = Item.originalExtraData; }
                                }
                                else if (Item.GetBaseItem().LimitedStack > 0)
                                {
                                    Item.ExtraData = Item.ExtraData + ";" + Item.LimitedValue;
                                }
                                else if (Item.GetBaseItem().InteractionType == InteractionType.wiredClassification)
                                {
                                    if (Item.wiredPuntuation != null)
                                        Item.ExtraData = Item.wiredPuntuation.SavePuntuations();
                                }

                                standardQueries.AddQuery("UPDATE items_extradata SET data = @data" + Item.Id + " WHERE item_id = " + Item.Id);
                                standardQueries.AddParameter("data" + Item.Id, Item.ExtraData);
                            }

                            if (Item.IsWallItem)
                            {
                                standardQueries.AddQuery("UPDATE items_rooms SET w='" + Item.wallCoord.ToString() + "' WHERE item_id = " + Item.Id);
                            }
                            else
                            {
                                var combinedCoords = TextHandling.Combine(Item.GetX, Item.GetY);
                                standardQueries.AddQuery("UPDATE items_rooms SET x=" + TextHandling.GetString(combinedCoords) + ", y=" + TextHandling.GetString(Item.GetZ) + ", n=" + Item.Rot + " WHERE item_id = " + Item.Id);
                            }
                        }
                    }

                    foreach (RoomItem Item in mWiredItems.Values)
                    {
                        if (Item == null || Item.GetBaseItem() == null || (Item.wiredHandler == null && Item.wiredCondition == null) || !WiredUtillity.TypeIsWired(Item.GetBaseItem().InteractionType))
                            continue;

                        if (Item.wiredHandler != null)
                            Item.wiredHandler.SaveToDatabase(wiredInserts);
                        else if (Item.wiredCondition != null)
                            Item.wiredCondition.SaveToDatabase(wiredInserts);
                    }

                    room.GetRoomUserManager().AppendPetsUpdateString(dbClient);

                    mAddedItems.Clear();
                    mRemovedItems.Clear();
                    mMovedItems.Clear();
                    mWiredItems.Clear();

                    standardQueries.Execute(dbClient);
                    itemInserts.Execute(dbClient);
                    extradataInserts.Execute(dbClient);
                    ownersInserts.Execute(dbClient);
                    wiredInserts.Execute(dbClient);

                    standardQueries.Dispose();
                    itemInserts.Dispose();
                    extradataInserts.Dispose();
                    ownersInserts.Dispose();
                    wiredInserts.Dispose();

                    standardQueries = null;
                    itemInserts = null;
                    extradataInserts = null;
                    ownersInserts = null;
                    wiredInserts = null;
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Error during saving furniture for room " + room.RoomId + ". Stack: " + e);
            }
        }
        public bool CheckPosItem(GameClient Session, RoomItem Item, int newX, int newY, int newRot, bool newItem, bool SendNotify = true)
        {
            try
            {
                Dictionary<int, ThreeDCoord> dictionary = Gamemap.GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX, newY, newRot);
                if (!room.GetGameMap().ValidTile(newX, newY))
                    return false;


                foreach (ThreeDCoord coord in dictionary.Values)
                {
                    if ((room.GetGameMap().Model.DoorX == coord.X) && (room.GetGameMap().Model.DoorY == coord.Y))
                        return false;
                }

                if ((room.GetGameMap().Model.DoorX == newX) && (room.GetGameMap().Model.DoorY == newY))
                    return false;


                foreach (ThreeDCoord coord in dictionary.Values)
                {
                    if (!room.GetGameMap().ValidTile(coord.X, coord.Y))
                        return false;
                }

                double num = room.GetGameMap().Model.SqFloorHeight[newX, newY];
                if ((((Item.Rot == newRot) && (Item.GetX == newX)) && (Item.GetY == newY)) && (Item.GetZ != num))
                    return false;

                if (room.GetGameMap().Model.SqState[newX, newY] != SquareState.OPEN)
                    return false;

                foreach (ThreeDCoord coord in dictionary.Values)
                {
                    if (room.GetGameMap().Model.SqState[coord.X, coord.Y] != SquareState.OPEN)
                        return false;
                }
                if (!Item.GetBaseItem().IsSeat)
                {
                    if (room.GetGameMap().SquareHasUsers(newX, newY))
                        return false;

                    foreach (ThreeDCoord coord in dictionary.Values)
                    {
                        if (room.GetGameMap().SquareHasUsers(coord.X, coord.Y))
                            return false;
                    }
                }

                List<RoomItem> furniObjects = room.GetGameMap().GetCoordinatedItems(new Point(newX, newY));
                List<RoomItem> collection = new List<RoomItem>();
                List<RoomItem> list3 = new List<RoomItem>();
                foreach (ThreeDCoord coord in dictionary.Values)
                {
                    List<RoomItem> list4 = room.GetGameMap().GetCoordinatedItems(new Point(coord.X, coord.Y));
                    if (list4 != null)
                        collection.AddRange(list4);

                }

                if (furniObjects == null)
                    furniObjects = new List<RoomItem>();

                list3.AddRange(furniObjects);
                list3.AddRange(collection);
                foreach (RoomItem item in list3)
                {
                    if ((item.Id != Item.Id) && !item.GetBaseItem().Stackable)
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        internal bool SetFloorItem(GameClient Session, RoomItem Item, int newX, int newY, int newRot, bool newItem, bool OnRoller, bool sendMessage, bool SpecialMove, bool isPiñata = false)
        {
            return SetFloorItem(Session, Item, newX, newY, newRot, newItem, OnRoller, sendMessage, true, SpecialMove, isPiñata);
        }

        internal bool SetFloorItem(GameClient Session, RoomItem Item, int newX, int newY, int newRot, bool newItem, bool OnRoller, bool sendMessage, bool updateRoomUserStatuses, bool SpecialMove, bool isPiñata)
        {
            if (Item == null || Item.GetBaseItem() == null)
                return false;

            var NeedsReAdd = false;
            if (!newItem)
                NeedsReAdd = room.GetGameMap().RemoveFromMap(Item);

            var AffectedTiles = Gamemap.GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX, newY, newRot);
            Double newZ = room.GetGameMap().Model.SqFloorHeight[newX, newY];
            RoomItem TileHeightItem = GetFurniObjects(newX, newY).Find(x => x.GetBaseItem().Name.ToLower().StartsWith("tile_stackmagic"));

            // TileHeight
            if (Item.GetBaseItem().Name.ToLower().StartsWith("tile_stackmagic") || TileHeightItem != null)
            {
                if (TileHeightItem != null)
                    newZ = TileHeightItem.TotalHeight;
                //else
                //    newZ = Item.TotalHeight;

                goto MagicTile;
            }

            if (Session != null && Session.GetHabbo() != null && Session.GetHabbo().tamanhoChao != 0)
            {
                newZ = Session.GetHabbo().tamanhoChao;

                goto MagicTile;
            }

            if (!room.GetGameMap().ValidTile(newX, newY) || room.GetGameMap().SquareHasUsers(newX, newY) && !Item.GetBaseItem().IsSeat && !isPiñata)
            {
                if (NeedsReAdd)
                {
                    AddItem(Item);
                    room.GetGameMap().AddItemToMap(Item);
                }

                var Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
                Item.Serialize(Message);
                room.SendMessage(Message);

                if (Session != null)
                {
                    ServerMessage messageError = new ServerMessage(Outgoing.CustomAlert);
                    messageError.AppendString("furni_placement_error");
                    messageError.AppendInt32(1);
                    messageError.AppendString("message");
                    messageError.AppendString("${room.error.cant_set_item}");
                    Session.SendMessage(messageError);
                }

                return false;
            }

            foreach (var Tile in AffectedTiles.Values)
            {
                if (!room.GetGameMap().ValidTile(Tile.X, Tile.Y) || (room.GetGameMap().SquareHasUsers(Tile.X, Tile.Y) && !Item.GetBaseItem().IsSeat && !isPiñata))
                {
                    if (NeedsReAdd)
                    {
                        AddItem(Item);
                        room.GetGameMap().AddItemToMap(Item);
                    }
                    return false;
                }
            }

            // Start calculating new Z coordinate
            newZ = room.GetGameMap().Model.SqFloorHeight[newX, newY];

            if (!OnRoller && (room.GetGameMap().Model.DoorX != newX && room.GetGameMap().Model.DoorY != newY) && !isPiñata)
            {
                if (room.GetGameMap().Model.SqState[newX, newY] != SquareState.OPEN && !Item.GetBaseItem().IsSeat)
                {
                    if (NeedsReAdd)
                    {
                        AddItem(Item);
                        room.GetGameMap().AddItemToMap(Item);
                    }
                    return false;
                }

                foreach (var Tile in AffectedTiles.Values)
                {
                    if (room.GetGameMap().Model.SqState[Tile.X, Tile.Y] != SquareState.OPEN && !Item.GetBaseItem().IsSeat)
                    {
                        if (NeedsReAdd)
                        {
                            AddItem(Item);
                            room.GetGameMap().AddItemToMap(Item);
                        }
                        return false;
                    }
                }

                // And that we have no users
                if (!Item.GetBaseItem().IsSeat && !Item.IsRoller)
                {
                    foreach (var Tile in AffectedTiles.Values)
                    {
                        if (room.GetGameMap().GetRoomUsers(new Point(Tile.X, Tile.Y)).Count > 0)
                        {
                            if (NeedsReAdd)
                            {
                                AddItem(Item);
                                room.GetGameMap().AddItemToMap(Item);
                            }
                            return false;
                        }
                    }
                }
            }

            // Find affected objects
            var ItemsOnTile = GetFurniObjects(newX, newY);
            var ItemsAffected = new List<RoomItem>();
            var ItemsComplete = new List<RoomItem>();

            foreach (var Tile in AffectedTiles.Values)
            {
                var Temp = GetFurniObjects(Tile.X, Tile.Y);

                if (Temp != null)
                {
                    ItemsAffected.AddRange(Temp);
                }
            }

            ItemsComplete.AddRange(ItemsOnTile);
            ItemsComplete.AddRange(ItemsAffected);

            if (!OnRoller)
            {
                // Check for items in the stack that do not allow stacking on top of them
                foreach (var I in ItemsComplete)
                {
                    if (I == null)
                        continue;

                    if (I.Id == Item.Id)
                    {
                        continue;
                    }

                    if (I.GetBaseItem() == null)
                        continue;

                    if (!I.GetBaseItem().Stackable)
                    {
                        if (NeedsReAdd)
                        {
                            AddItem(Item);
                            room.GetGameMap().AddItemToMap(Item);
                        }
                        return false;
                    }
                }
            }

            if (Item.Rot != newRot && Item.GetX == newX && Item.GetY == newY)
            {
                newZ = Item.GetZ;
            }

            // Are there any higher objects in the stack!?
            foreach (var I in ItemsComplete)
            {
                if (I.Id == Item.Id)
                {
                    continue; // cannot stack on self
                }
                // se for um roller ele não vai ficar subindo
                
                if (I.TotalHeight > newZ)
                {
                    newZ = I.TotalHeight; 
                }
            }

            // Verify the rotation is correct
            if (Item.GetBaseItem().AllowRotations)
            {
                if (newRot != 0 && newRot != 2 && newRot != 4 && newRot != 6 && newRot != 8)
                {
                    newRot = 0;
                }
            }
            else
            {
                if (newRot != 1 && newRot != 2 && newRot != 3 && newRot != 4 && newRot != 5 && newRot != 6 && newRot != 7 && newRot != 8)
                {
                    newRot = 0;
                }
            }

        MagicTile:

            Item.OldX = Item.GetX;
            Item.OldY = Item.GetY;
            Item.OldRot = Item.Rot;
            Item.Rot = newRot;
            Item.SetState(newX, newY, newZ, AffectedTiles);

            if (!OnRoller && Session != null)
                Item.Interactor.OnPlace(Session, Item);

            if (newItem)
            {
                if (mFloorItems.ContainsKey(Item.Id))
                {
                    if (Session != null)
                        Session.SendNotif(LanguageLocale.GetValue("room.itemplaced"));
                    return true;
                }

                if (Item.IsFloorItem && !mFloorItems.ContainsKey(Item.Id))
                    mFloorItems.TryAdd(Item.Id, Item);
                else if (Item.IsWallItem && !mWallItems.ContainsKey(Item.Id))
                    mWallItems.TryAdd(Item.Id, Item);

                if (Item.GetBaseItem().InteractionType == InteractionType.teleport) // to can use if added now, no problem ;D
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        var combinedCoords = TextHandling.Combine(Item.GetX, Item.GetY);
                        dbClient.runFastQuery("REPLACE INTO items_rooms (item_id,room_id,x,y,n) VALUES (" + Item.Id + "," + Item.RoomId + "," + TextHandling.GetString(combinedCoords) + "," + TextHandling.GetString(Item.GetZ) + "," + Item.Rot + ")");
                    }
                }
                else if (Item.GetBaseItem().InteractionType == InteractionType.saltasalas)
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("UPDATE items_jumping_rooms SET room_id = '" + Item.RoomId + "' WHERE item_id = '" + Item.Id + "'");
                    }
                    AddItem(Item);
                }
                else
                {
                    AddItem(Item);
                }

                if (sendMessage)
                {
                    var Message = new ServerMessage(Outgoing.AddFloorItemToRoom);
                    Item.Serialize(Message);
                    Message.AppendString(UsersCache.getUsernameById(Item.OwnerId));
                    room.SendMessage(Message);

                    // room.SendMessage(ItemCoords.GenerateFurniMap(room, Item.GenerateTilesMap(false, MagicTileEnabled)));
                }
            }
            else
            {
                UpdateItem(Item);

                if (!OnRoller && sendMessage)
                {
                    if (SpecialMove)
                    {
                        var Message = new ServerMessage(Outgoing.ObjectOnRoller);
                        Message.AppendInt32(Item.OldX);
                        Message.AppendInt32(Item.OldY);
                        Message.AppendInt32(Item.GetX);
                        Message.AppendInt32(Item.GetY);
                        Message.AppendInt32(1);
                        Message.AppendUInt(Item.Id);
                        Message.AppendString(String.Format("{0:0.00}", TextHandling.GetString(Item.GetZ))); // altura a la que se encuentra
                        Message.AppendString(String.Format("{0:0.00}", TextHandling.GetString(Item.GetZ))); // altura del furni
                        Message.AppendInt32(-1);
                        room.SendMessage(Message);
                    }
                    else
                    {
                        var Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
                        Item.Serialize(Message);
                        room.SendMessage(Message);
                    }
                }
            }

            room.GetGameMap().AddItemToMap(Item);

            if (TileHeightItem == null)
            {
                if (newItem)
                    ItemCoords.ModifyGamemapTiles(room, Item.GetAffectedTiles);
                else
                    ItemCoords.ModifyGamemapTiles(room, Item.GetAffectedTiles, Item.GetBackupAffectedTiles);
            }

            room.GetRoomUserManager().OnUserUpdateStatus();
            // room.GetRoomUserManager().UpdateUsersPath = true;

            return true;
        }

        internal List<RoomItem> GetFurniObjects(int X, int Y)
        {
            return room.GetGameMap().GetCoordinatedItems(new Point(X, Y));
        }

        internal void SetFloorItem(RoomItem Item, int newX, int newY, Double newZ)
        {
            if (Item == null || Item.GetBaseItem() == null)
                return;

            room.GetGameMap().RemoveFromMap(Item);
            Item.SetState(newX, newY, newZ, Gamemap.GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX, newY, Item.Rot));

            UpdateItem(Item);
            room.GetGameMap().AddItemToMap(Item);
        }

        internal bool SetWallItem(GameClient Session, RoomItem Item)
        {
            if (!Item.IsWallItem || mWallItems.ContainsKey(Item.Id))
                return false;

            Item.Interactor.OnPlace(Session, Item);

            if (Item.GetBaseItem().InteractionType == InteractionType.dimmer)
            {
                if (room.MoodlightData == null)
                {
                    room.MoodlightData = new MoodlightData(Item.Id);
                    Item.ExtraData = room.MoodlightData.GenerateExtraData();
                }
            }

            mWallItems.TryAdd(Item.Id, Item);
            AddItem(Item);

            var Message = new ServerMessage(Outgoing.AddWallItemToRoom);
            Item.Serialize(Message);
            Message.AppendString(UsersCache.getUsernameById(Item.OwnerId));
            room.SendMessage(Message);

            return true;
        }

        internal void UpdateItem(RoomItem item)
        {
            if (mAddedItems.Contains(item.Id))
                return;
            if (mRemovedItems.Contains(item.Id))
                mRemovedItems.Remove(item.Id);
            if (!mMovedItems.Contains(item.Id))
                mMovedItems.Add(item.Id, item);
        }

        internal void UpdateWiredItem(RoomItem item)
        {
            if (!mWiredItems.Contains(item.Id))
                mWiredItems.Add(item.Id, item);
        }

        internal void AddItem(RoomItem item)
        {
            if (mRemovedItems.Contains(item.Id) && !item.IsPremiumItem)
                mRemovedItems.Remove(item.Id);
            if (!mMovedItems.Contains(item.Id) && !mAddedItems.Contains(item.Id))
                mAddedItems.Add(item.Id, item);
        }

        internal void RemoveItem(RoomItem item)
        {
            if (mAddedItems.Contains(item.Id))
                mAddedItems.Remove(item.Id);

            if (mMovedItems.Contains(item.Id))
                mMovedItems.Remove(item.Id);
            if (!mRemovedItems.Contains(item.Id))
                mRemovedItems.Add(item.Id, item);

            RoomItem junkItem;
            mRollers.TryRemove(item.Id, out junkItem);
        }

        internal void OnCycle()
        {
            if (mGotRollers)
            {
                try
                {
                    room.SendMessage(CycleRollers());
                }
                catch (Exception e)
                {
                    Logging.LogThreadException(e.ToString(), "rollers for room with ID " + room.RoomId);
                    mGotRollers = false;
                }
            }

            if (roomItemUpdateQueue.Count > 0)
            {
                var addItems = new List<RoomItem>();
                lock (roomItemUpdateQueue.SyncRoot)
                {
                    while (roomItemUpdateQueue.Count > 0)
                    {
                        var item = (RoomItem)roomItemUpdateQueue.Dequeue();

                        item.ProcessUpdates();

                        if (item.IsTrans || item.UpdateCounter > 0)
                            addItems.Add(item);
                    }

                    foreach (var item in addItems)
                    {
                        roomItemUpdateQueue.Enqueue(item);
                    }
                }
            }
        }

        internal Point getRandomPetfood()
        {
            if (petFoods.Count > 0)
            {
                if (petFoods.Count == 1)
                {
                    int intExtradata = 0;
                    int.TryParse(petFoods.Values.First().ExtraData, out intExtradata);
                    if (intExtradata < 5)
                    {
                        petFoods.Values.First().ExtraData = (intExtradata + 1).ToString();
                        petFoods.Values.First().UpdateState();
                        if ((intExtradata + 1) == 5)
                        {
                            room.GetRoomItemHandler().RemoveRoomItem(petFoods.Values.First());
                        }

                        return petFoods.Values.First().Coordinate;
                    }
                }
                else
                {
                    List<UInt32> keys = new List<UInt32>(petFoods.Keys);
                    int size = petFoods.Count;
                    Random rand = new Random();
                    UInt32 randomKey = keys[rand.Next(size)];

                    int intExtradata = 0;
                    int.TryParse(petFoods[randomKey].ExtraData, out intExtradata);
                    if (intExtradata < 5)
                    {
                        petFoods[randomKey].ExtraData = (intExtradata + 1).ToString();
                        petFoods[randomKey].UpdateState();

                        if ((intExtradata + 1) == 5)
                        {
                            room.GetRoomItemHandler().RemoveRoomItem(petFoods[randomKey]);
                        }

                        return petFoods[randomKey].Coordinate;
                    }
                }
            }

            return new Point();
        }

        internal Point getRandomHome()
        {
            if (petHomes.Count > 0)
            {
                List<UInt32> keys = new List<UInt32>(petHomes.Keys);
                int size = petHomes.Count;
                Random rand = new Random();
                UInt32 randomKey = keys[rand.Next(size)];

                return petHomes[randomKey].Coordinate;
            }

            return new Point();
        }

        internal Point getRandomBreedingPet(Pet pet)
        {
            if (breedingPet.Count > 0)
            {
                List<RoomItem> itemsId = breedingPet.Values.Where(t => t.GetBaseItem().Name == "pet_breeding_" + PetBreeding.GetPetByPetId(pet.Type)).ToList();
                if (itemsId.Count == 0)
                    return new Point();
                
                RoomItem Item = itemsId[new Random().Next(0, itemsId.Count - 1)];

                if (!Item.havepetscount.Contains(pet))
                    Item.havepetscount.Add(pet);

                pet.waitingForBreading = Item.Id;
                pet.breadingTile = Item.Coordinate;
                return Item.Coordinate;
            }
            return new Point();
        }

        internal Point getRandomWaterbowl()
        {
            if (waterBowls.Count > 0)
            {
                if (waterBowls.Count == 1)
                {
                    int intExtradata = 0;
                    int.TryParse(waterBowls.Values.First().ExtraData, out intExtradata);
                    if (intExtradata > 0)
                    {
                        waterBowls.Values.First().ExtraData = (intExtradata - 1).ToString();
                        waterBowls.Values.First().UpdateState();

                        return waterBowls.Values.First().Coordinate;
                    }
                }
                else
                {
                    List<UInt32> keys = new List<UInt32>(waterBowls.Keys);
                    int size = waterBowls.Count;
                    Random rand = new Random();
                    UInt32 randomKey = keys[rand.Next(size)];

                    int intExtradata = 0;
                    int.TryParse(waterBowls[randomKey].ExtraData, out intExtradata);
                    if (intExtradata > 0)
                    {
                        waterBowls[randomKey].ExtraData = (intExtradata - 1).ToString();
                        waterBowls[randomKey].UpdateState();

                        return waterBowls[randomKey].Coordinate;
                    }
                }
            }

            return new Point();
        }

        internal void Destroy()
        {
            mFloorItems.Clear();
            mWallItems.Clear();
            mRemovedItems.Clear();
            mMovedItems.Clear();
            mAddedItems.Clear();
            mWiredItems.Clear();
            roomItemUpdateQueue.Clear();
            breedingPet.Clear();
            waterBowls.Clear();
            petHomes.Clear();
            petFoods.Clear();

            room = null;
            mFloorItems = null;
            mWallItems = null;
            mRemovedItems = null;
            mMovedItems = null;
            mAddedItems = null;
            mWiredItems = null;
            mWallItems = null;
            roomItemUpdateQueue = null;
        }
    }
}
