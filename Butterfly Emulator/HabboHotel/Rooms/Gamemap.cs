using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Butterfly.Core;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Pathfinding;
using Butterfly.HabboHotel.Rooms.Wired;
using ButterStorm;
using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;

namespace Butterfly.HabboHotel.Rooms
{
    class Gamemap
    {
        #region Variables
        /// <summary>
        /// La variable que mantiene la información de sala.
        /// </summary>
        private Room room;

        /// <summary>
        /// Modelo de sala que no varia NUNCA.
        /// </summary>
        private RoomModel mStaticModel;

        /// <summary>
        /// Items que encontramos sobre una baldosa.
        /// </summary>
        private Hashtable mCoordinatedItems;

        /// <summary>
        /// Todas las baldosas con altura única.
        /// </summary>
        private Hashtable mTileHeightApilable;

        /// <summary>
        /// Estado de la casilla (CERRADA = 0, ABIERTA = 1, SILLA = 2, ENTRADA = 3)
        /// </summary>
        private byte[,] mGameMap;

        /// <summary>
        /// Al pisar una casilla el efecto que se nos asignará.
        /// </summary>
        private byte[,] mUserItemEffect;

        /// <summary>
        /// Altura máxima de dicha casilla. (apilable tile)
        /// </summary>
        private double[,] mItemHeightMap;

        /// <summary>
        /// En esta podremos encontrar los usuarios que están sobre las baldosas.
        /// </summary>
        private Hashtable userMap;

        /// <summary>
        /// Caché de puertas de grupo en esta sala.
        /// </summary>
        internal Dictionary<Point, RoomItem> guildGates;

        /// <summary>
        /// Heightmap of the room.
        /// </summary>
        private ServerMessage StaticHeightmap;

        internal RoomModel Model
        {
            get
            {
                return mStaticModel;
            }
        }

        internal byte[,] EffectMap
        {
            get
            {
                return mUserItemEffect;
            }
        }

        internal Hashtable CoordinatedItems
        {
            get
            {
                return mCoordinatedItems;
            }
        }

        internal byte[,] GameMap
        {
            get
            {
                return mGameMap;
            }
        }

        internal double[,] ItemHeightMap
        {
            get
            {
                return mItemHeightMap;
            }
        }

        public ServerMessage GetStaticHeightmap()
        {
            return StaticHeightmap;
        }
        #endregion

        #region Constructor
        public Gamemap(Room room)
        {
            this.room = room;

            this.mStaticModel = OtanixEnvironment.GetGame().GetRoomManager().GetModel(this.room.RoomData.ModelName, this.room.RoomId);
            if (this.mStaticModel == null)
                throw new Exception("No modeldata found for roomID " + this.room.RoomId);

            this.StaticHeightmap = this.mStaticModel.SerializeRelativeHeightmap(room.RoomData.WallHeight);
            this.mCoordinatedItems = new Hashtable();
            this.mTileHeightApilable = new Hashtable();
            this.mGameMap = new byte[Model.MapSizeX, Model.MapSizeY];
            this.mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
            this.userMap = new Hashtable();
            this.guildGates = new Dictionary<Point, RoomItem>();
        }
        #endregion

        #region User Methods
        internal void UpdateUserMovement(Point oldCoord, Point newCoord, RoomUser user)
        {
            RemoveUserFromMap(user, oldCoord);
            AddUserToMap(user, newCoord);
        }

        internal void AddUserToMap(RoomUser user, Point coord)
        {
            if (userMap.ContainsKey(coord))
                ((List<RoomUser>)userMap[coord]).Add(user);
            else
                userMap.Add(coord, new List<RoomUser> { user });
        }

        internal void RemoveUserFromMap(RoomUser user, Point coord)
        {
            if (userMap.ContainsKey(coord))
            {
                if (((List<RoomUser>)userMap[coord]).Count == 1)
                    userMap.Remove(coord);
                else
                    ((List<RoomUser>)userMap[coord]).Remove(user);
            }
        }

        internal List<RoomUser> GetRoomUsers(Point coord)
        {
            if (userMap.ContainsKey(coord))
                return (List<RoomUser>)userMap[coord];
            else
                return new List<RoomUser>();
        }

        internal List<RoomUser> GetUsersOnItem(RoomItem Item)
        {
            List<RoomUser> userList = new List<RoomUser>();
            foreach (ThreeDCoord tile in Item.GetAffectedTiles.Values)
            {
                Point coord = new Point(tile.X, tile.Y);
                userList.AddRange(GetRoomUsers(coord));
            }
            return userList;
        }
        #endregion

        #region Items Methods
        internal void updateMapForItem(RoomItem item)
        {
            RemoveFromMap(item);
            AddItemToMap(item);
        }

        internal bool RemoveFromMap(RoomItem item)
        {
            if (room.GotWired() && WiredUtillity.TypeIsWired(item.GetBaseItem().InteractionType))
                room.GetWiredHandler().RemoveFurniture(item);

            if (guildGates.ContainsKey(item.Coordinate))
                guildGates.Remove(item.Coordinate);

            return RemoveFromMap(item, true);
        }

        internal bool AddItemToMap(RoomItem Item, bool handleGameItem = true)
        {
            if (handleGameItem)
            {
                if (room.GotWired())
                {
                    if (WiredUtillity.TypeIsWiredCondition(Item.GetBaseItem().InteractionType))
                        room.GetWiredHandler().conditionHandler.AddConditionToTile(Item.Coordinate, Item.wiredCondition);
                    else if (WiredUtillity.TypeIsWired(Item.GetBaseItem().InteractionType))
                        room.GetWiredHandler().AddFurniture(Item);
                }

                AddSpecialItems(Item);

                switch (Item.GetBaseItem().InteractionType)
                {
                    case InteractionType.footballgoalred:
                    case InteractionType.footballcounterred:
                    case InteractionType.banzaiscorered:
                    case InteractionType.banzaigatered:
                    case InteractionType.banzaiscoreyellow:
                    case InteractionType.banzaigateblue:
                    case InteractionType.banzaigateyellow:
                    case InteractionType.freezeredcounter:
                    case InteractionType.freezeredgate:
                    case InteractionType.footballgoalgreen:
                    case InteractionType.footballcountergreen:
                    case InteractionType.banzaiscoregreen:
                    case InteractionType.banzaigategreen:
                    case InteractionType.freezegreencounter:
                    case InteractionType.freezegreengate:
                    case InteractionType.footballgoalblue:
                    case InteractionType.footballcounterblue:
                    case InteractionType.banzaiscoreblue:                   
                    case InteractionType.freezebluecounter:
                    case InteractionType.freezebluegate:
                    case InteractionType.footballgoalyellow:
                    case InteractionType.footballcounteryellow:                                      
                    case InteractionType.freezeyellowcounter:
                    case InteractionType.freezeyellowgate:
                        {
                            room.GetGameManager().AddTeamItem(Item, Item.team);
                            break;
                        }
                    case InteractionType.roller:
                        {
                            if (!room.GetRoomItemHandler().mRollers.ContainsKey(Item.Id))
                                room.GetRoomItemHandler().mRollers.TryAdd(Item.Id, Item);
                            break;
                        }
                    case InteractionType.guildgate:
                        {
                            if (!guildGates.ContainsKey(Item.Coordinate))
                                guildGates.Add(Item.Coordinate, Item);
                            break;
                        }
                }
            }

            if (Item.GetBaseItem().Type != 's')
                return true;

            foreach (Point coord in Item.GetCoords)
            {
                AddCoordinatedItem(Item, coord);
            }

            foreach (Point coord in Item.GetCoords)
                if (!ConstructMapForItem(Item, coord))
                    return false;
            return true;
        }

        private void AddSpecialItems(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.banzaipuck:
                    {
                        room.GetGameManager().GetBanzai().AddPuck(item);
                        break;
                    }

                case InteractionType.banzaifloor:
                    {
                        room.GetGameManager().GetBanzai().AddTile(item);
                        break;
                    }

                case InteractionType.banzaipyramid:
                    {
                        room.GetGameItemHandler().AddPyramid(item, item.Id);
                        break;
                    }

                case InteractionType.banzaitele:
                    {
                        room.GetGameItemHandler().AddTeleport(item, item.Id);
                        item.ExtraData = "";
                        break;
                    }

                case InteractionType.football:
                    {
                        room.GetSoccer().AddBall(item);
                        break;
                    }
                case InteractionType.footballgoalred:
                case InteractionType.footballgoalgreen:
                case InteractionType.footballgoalblue:
                case InteractionType.footballgoalyellow:
                    {
                        room.GetSoccer().AddGoal(item);
                        break;
                    }
                case InteractionType.freezetileblock:
                    {
                        room.GetGameManager().GetFreeze().AddFreezeBlock(item);
                        break;
                    }
                case InteractionType.freezetile:
                    {
                        room.GetGameManager().GetFreeze().AddFreezeTile(item);
                        break;
                    }
                case InteractionType.freezeexit:
                    {
                        room.GetGameManager().GetFreeze().SetExitTile(item);
                        break;
                    }
            }
        }

        private void RemoveSpecialItem(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.banzaipuck:
                    {
                        room.GetGameManager().GetBanzai().RemovePuck(item.Id);
                        break;
                    }
                case InteractionType.banzaifloor:
                    room.GetGameManager().GetBanzai().RemoveTile(item);
                    break;
                case InteractionType.banzaipyramid:
                    room.GetGameItemHandler().RemovePyramid(item.Id);
                    break;
                case InteractionType.banzaitele:
                    room.GetGameItemHandler().RemoveTeleport(item.Id);
                    break;
                case InteractionType.football:
                    room.GetSoccer().RemoveBall();
                    break;
                case InteractionType.freezetile:
                    room.GetGameManager().GetFreeze().RemoveFreezeTile(item);
                    break;
                case InteractionType.freezetileblock:
                    room.GetGameManager().GetFreeze().RemoveFreezeBlock(item);
                    break;
                case InteractionType.freezeexit:
                    room.GetGameManager().GetFreeze().SetExitTile(null);
                    break;
                case InteractionType.banzaigatered:
                case InteractionType.banzaigateblue:
                case InteractionType.banzaigateyellow:
                case InteractionType.banzaigategreen:
                    room.GetGameManager().RemoveTeamItem(item, item.team);
                    break;
            }
        }

        internal bool RemoveFromMap(RoomItem item, bool handleGameItem)
        {
            if (handleGameItem)
                RemoveSpecialItem(item);

            if (room.GotSoccer())
                room.GetSoccer().onGateRemove(item);

            List<Point> Coords = item.GetCoords;
            if (Coords == null)
                return false;

            bool isRemoved = Coords.All(coord => RemoveCoordinatedItem(item, coord));

            foreach (Point tile in Coords)
            {
                SetDefaultValue(tile.X, tile.Y);
                if (mCoordinatedItems.ContainsKey(tile))
                {
                    RoomItem priorityItem = ((List<RoomItem>)mCoordinatedItems[tile]).Last();
                    ConstructMapForItem(priorityItem, tile);
                }
            }

            ItemCoords.ModifyGamemapTiles(room, Coords);

            return isRemoved;
        }

        private bool RemoveCoordinatedItem(RoomItem item, Point coord)
        {
            if (mCoordinatedItems.ContainsKey(coord))
            {
                if (((List<RoomItem>)mCoordinatedItems[coord]).Count <= 1)
                    mCoordinatedItems.Remove(coord);
                else
                    ((List<RoomItem>)mCoordinatedItems[coord]).Remove(item);

                return true;
            }
            return false;
        }

        private void SetDefaultValue(int x, int y)
        {
            mGameMap[x, y] = 0;
            mUserItemEffect[x, y] = 0;
            mItemHeightMap[x, y] = 0.0;

            if (x == Model.DoorX && y == Model.DoorY)
            {
                mGameMap[x, y] = 3;
            }
            else if (Model.SqState[x, y] == SquareState.OPEN)
            {
                mGameMap[x, y] = 1;
            }
        }

        private bool ConstructMapForItem(RoomItem Item, Point Coord)
        {
            if (Item == null)
                return false;

            if (mItemHeightMap[Coord.X, Coord.Y] <= Item.GetZ && !mTileHeightApilable.ContainsKey(Coord))
            {
                mItemHeightMap[Coord.X, Coord.Y] = Item.TotalHeight - Model.SqFloorHeight[Item.GetX, Item.GetY];
                mUserItemEffect[Coord.X, Coord.Y] = 0;

                switch (Item.GetBaseItem().InteractionType)
                {
                    case InteractionType.pool:
                        mUserItemEffect[Coord.X, Coord.Y] = 1;
                        break;
                    case InteractionType.normslaskates:
                        mUserItemEffect[Coord.X, Coord.Y] = 2;
                        break;
                    case InteractionType.iceskates:
                        mUserItemEffect[Coord.X, Coord.Y] = 3;
                        break;
                    case InteractionType.lowpool:
                        mUserItemEffect[Coord.X, Coord.Y] = 4;
                        break;
                    case InteractionType.haloweenpool:
                        mUserItemEffect[Coord.X, Coord.Y] = 5;
                        break;

                    // 6: PublicPool

                    case InteractionType.haloweenpool15:
                        mUserItemEffect[Coord.X, Coord.Y] = 7;
                        break;

                    case InteractionType.horsejump:
                        if (Item.Rot == 2)
                        {
                            mUserItemEffect[Item.GetX, Item.GetY] = 8;
                            mUserItemEffect[Item.GetX, Item.GetY + 1] = 8;
                        }
                        else if (Item.Rot == 4)
                        {
                            mUserItemEffect[Item.GetX, Item.GetY + 1] = 8;
                            mUserItemEffect[Item.GetX, Item.GetY] = 8;
                        }
                        break;

                    case InteractionType.snowboard:
                        mUserItemEffect[Coord.X, Coord.Y] = 9;
                        break;

                    case InteractionType.trampoline:
                        mUserItemEffect[Coord.X, Coord.Y] = 10;
                        break;

                    case InteractionType.treadmill:
                        mUserItemEffect[Coord.X, Coord.Y] = 11;
                        break;

                    case InteractionType.crosstrainer:
                        mUserItemEffect[Coord.X, Coord.Y] = 12;
                        break;
                }
                if ((Item.GetBaseItem().InteractionType == InteractionType.banzaigateyellow || Item.GetBaseItem().InteractionType == InteractionType.banzaigategreen || Item.GetBaseItem().InteractionType == InteractionType.banzaigatered || Item.GetBaseItem().InteractionType == InteractionType.banzaigateblue
                     || Item.GetBaseItem().InteractionType == InteractionType.freezeyellowgate || Item.GetBaseItem().InteractionType == InteractionType.freezegreengate || Item.GetBaseItem().InteractionType == InteractionType.freezeredgate || Item.GetBaseItem().InteractionType == InteractionType.freezebluegate) && room.GetGameManager().IsGameStarted())
                {
                    mGameMap[Coord.X, Coord.Y] = 0;
                }
                else if (Item.GetBaseItem().Walkable) // If this item is walkable and on the floor, allow users to walk here.
                {
                    if (mGameMap[Coord.X, Coord.Y] != 2)
                        mGameMap[Coord.X, Coord.Y] = 1;
                }
                else if (Item.GetBaseItem().InteractionType == InteractionType.footballgoalblue || Item.GetBaseItem().InteractionType == InteractionType.footballgoalgreen || Item.GetBaseItem().InteractionType == InteractionType.footballgoalred || Item.GetBaseItem().InteractionType == InteractionType.footballgoalyellow)
                {
                    mGameMap[Coord.X, Coord.Y] = 1;
                }
                else if (Item.GetBaseItem().InteractionType == InteractionType.gate && Item.ExtraData == "1") // If this item is a gate, open, and on the floor, allow users to walk here.
                {
                    mGameMap[Coord.X, Coord.Y] = 1;
                }
                else if (Item.GetBaseItem().IsSeat || Item.GetBaseItem().InteractionType == InteractionType.bed || Item.GetBaseItem().InteractionType == InteractionType.tent || Item.GetBaseItem().InteractionType == InteractionType.wobench)
                {
                    mGameMap[Coord.X, Coord.Y] = 2;
                }
                else // Finally, if it's none of those, block the square.
                {
                    if (mGameMap[Coord.X, Coord.Y] != 2)
                        mGameMap[Coord.X, Coord.Y] = 0;
                }
            }
            return true;
        }

        private void AddCoordinatedItem(RoomItem item, Point coord)
        {
            if (!mCoordinatedItems.ContainsKey(coord))
            {
                mCoordinatedItems.Add(coord, new List<RoomItem> { item });
            }
            else
            {
                List<RoomItem> Items = ((List<RoomItem>)mCoordinatedItems[coord]);

                if (!Items.Contains(item))
                    Items.Add(item);
            }
        }

        internal void GenerateMaps()
        {
            try
            {
                mCoordinatedItems = new Hashtable();
                mUserItemEffect = new byte[Model.MapSizeX, Model.MapSizeY];
                mGameMap = new byte[Model.MapSizeX, Model.MapSizeY];
                mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];

                for (var line = 0; line < Model.MapSizeY; line++)
                {
                    for (var chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        mGameMap[chr, line] = 0;
                        mUserItemEffect[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY)
                        {
                            mGameMap[chr, line] = 3;
                        }
                        else if (Model.SqState[chr, line] == SquareState.OPEN)
                        {
                            mGameMap[chr, line] = 1;
                        }
                    }
                }

                RoomItem[] tmpItems = room.GetRoomItemHandler().mFloorItems.Values.ToArray();
                foreach (RoomItem Item in tmpItems)
                {
                    AddItemToMap(Item, true);
                }
                Array.Clear(tmpItems, 0, tmpItems.Length);
                tmpItems = null;

                if (!room.RoomData.AllowWalkthrough)
                {
                    foreach (RoomUser user in room.GetRoomUserManager().UserList.Values)
                    {
                        user.SqState = mGameMap[user.X, user.Y];
                        mGameMap[user.X, user.Y] = 0;
                    }
                }

                mGameMap[Model.DoorX, Model.DoorY] = 3;
            }
            catch (Exception e)
            {
                Logging.LogPacketException("NULL -> Erro ao gerar o quarto: [" + room.Id + "], "+ e.Source + " - " + e.Message, e.ToString());
            }
        }
        #endregion

        #region Extra Methods
        /// <summary>
        /// Return a random open tile of the room.
        /// </summary>
        public Point getRandomWalkableSquare()
        {
            List<Point> openSquares = new List<Point>();
            try
            {
                for (int y = 0; y < Model.MapSizeY; y++)
                {
                    for (int x = 0; x < Model.MapSizeX; x++)
                    {
                        if (mGameMap[x, y] == 1 && !(Model.DoorX == x && Model.DoorY == y))
                            openSquares.Add(new Point(x, y));
                    }
                }

                return openSquares[new Random().Next(openSquares.Count)];
            }
            catch { return new Point(); }
            finally
            {
                openSquares.Clear();
                openSquares = null;
            }
        }

        public bool IsValidStep(Vector2D From, Vector2D To, bool EndOfPath, RoomUser User, bool GeneratingPath, bool DiagMove)
        {
            if (!ValidTile(To.X, To.Y))
            {
                return false;
            }

            // Si tenemos el comando overrido activo, evitamos todo.
            if (User.AllowOverride)
            {
                return true;
            }

            if (DiagMove)
            {
                int XValue = To.X - From.X;
                int YValue = To.Y - From.Y;

                if (XValue == -1 && YValue == -1) // Cima Esquerdo
                {
                    Point itemEntreCoords1 = new Point(To.X+1, To.Y), itemEntreCoords2 = new Point(To.X, To.Y+1);
                    List<RoomItem> itensEsquerda = GetCoordinatedItems(itemEntreCoords1), itensDireita = GetCoordinatedItems(itemEntreCoords2);
                    if(itensDireita.Count > 0 && itensEsquerda.Count > 0)
                    {
                        bool esquerdaBool = false, direitaBool = false;
                        foreach (RoomItem meuItem in itensEsquerda)
                            if (meuItem.GetBaseItem().Name.StartsWith("CF_"))
                                esquerdaBool = true;
                        

                        foreach (RoomItem meuItem in itensDireita)
                            if (meuItem.GetBaseItem().Name.StartsWith("CF_"))
                                direitaBool = true;
                           
                        if (esquerdaBool && direitaBool == true)
                            return false;
                    }
                    if (mGameMap[To.X + 1, To.Y + 1] != 1 && mGameMap[To.X + 1, To.Y + 1] != 2)// && mGameMap[To.X, To.Y + 1] != 1)
                        return false;
                }
                else if (XValue == 1 && YValue == -1) // Cima direita
                {
                    Point itemEntreCoords1 = new Point(To.X - 1, To.Y), itemEntreCoords2 = new Point(To.X, To.Y + 1);
                    List<RoomItem> itensEsquerda = GetCoordinatedItems(itemEntreCoords1), itensDireita = GetCoordinatedItems(itemEntreCoords2);
                    if (itensDireita.Count > 0 && itensEsquerda.Count > 0)
                    {
                        bool esquerdaBool = false, direitaBool = false;
                        foreach (RoomItem meuItem in itensEsquerda)
                            if (meuItem.GetBaseItem().Name.StartsWith("CF_"))
                                esquerdaBool = true;
                        

                        foreach (RoomItem meuItem in itensDireita)
                            if (meuItem.GetBaseItem().Name.StartsWith("CF_"))
                                direitaBool = true;
                        
                        if (esquerdaBool && direitaBool == true)
                            return false;
                    }
                    if (mGameMap[To.X - 1, To.Y + 1] != 1 && mGameMap[To.X - 1, To.Y + 1] != 2)// && mGameMap[To.X, To.Y + 1] != 1)
                        return false;
                }
                else if (XValue == 1 && YValue == 1) // Baixo direito
                {
                    Point itemEntreCoords1 = new Point(To.X - 1, To.Y), itemEntreCoords2 = new Point(To.X, To.Y - 1);
                    List<RoomItem> itensEsquerda = GetCoordinatedItems(itemEntreCoords1), itensDireita = GetCoordinatedItems(itemEntreCoords2);
                    if (itensDireita.Count > 0 && itensEsquerda.Count > 0)
                    {
                        bool esquerdaBool = false, direitaBool = false;
                        foreach (RoomItem meuItem in itensEsquerda)
                            if (meuItem.GetBaseItem().Name.StartsWith("CF_"))
                                esquerdaBool = true;

                        foreach (RoomItem meuItem in itensDireita)
                            if (meuItem.GetBaseItem().Name.StartsWith("CF_"))
                                direitaBool = true;

                        if (esquerdaBool && direitaBool == true)
                            return false;
                    }
                    if (mGameMap[To.X - 1, To.Y - 1] != 1 && mGameMap[To.X - 1, To.Y - 1] != 2)// && mGameMap[To.X, To.Y - 1] != 1)
                        return false;
                }
                else if (XValue == -1 && YValue == 1) // Baixo esquerda
                {
                    Point itemEntreCoords1 = new Point(To.X + 1, To.Y), itemEntreCoords2 = new Point(To.X, To.Y - 1);
                    List<RoomItem> itensEsquerda = GetCoordinatedItems(itemEntreCoords1), itensDireita = GetCoordinatedItems(itemEntreCoords2);
                    if (itensDireita.Count > 0 && itensEsquerda.Count > 0)
                    {
                        bool esquerdaBool = false, direitaBool = false;
                        foreach (RoomItem meuItem in itensEsquerda)
                            if (meuItem.GetBaseItem().Name.StartsWith("CF_"))
                                esquerdaBool = true;

                        foreach (RoomItem meuItem in itensDireita)
                            if (meuItem.GetBaseItem().Name.StartsWith("CF_"))
                                direitaBool = true;

                        if (esquerdaBool && direitaBool == true)
                            return false;
                    }
                    if (mGameMap[To.X + 1, To.Y - 1] != 1 && mGameMap[To.X + 1, To.Y - 1] != 2)// && mGameMap[To.X, To.Y - 1] != 1)
                        return false;
                }
            }

            // Si es una puerta de grupo y pertenecemos al grupo, podemos pisar por esta baldosa.
            Point square = new Point(To.X, To.Y);
            if (this.guildGates.ContainsKey(square))
            {
                uint GuildId = 0;
                string[] strArr = this.guildGates[square].GroupData.Split(';');
                if (strArr.Length < 2)
                    return false;

                uint.TryParse(strArr[1], out GuildId);

                if (GuildId > 0)
                {
                    if (!User.IsBot)
                    {
                        if (User.GetClient().GetHabbo().MyGroups.Contains(GuildId))
                        {
                            if (!GeneratingPath) // Si ya estamos andando, activaremos la puerta cuando estemos delante
                            {
                                RoomItem roomItem = this.guildGates[square];
                                roomItem.ExtraData = "1";
                                roomItem.UpdateState();
                            }

                            return true;
                        }
                    }
                }
            }

            // Si hay un usuario o la baldosa está cerrada.
            if (!tileIsWalkable(To.X, To.Y, true, EndOfPath))
            {
                if (EndOfPath && User.walkingToPet != null) { }
                else
                {
                    if (!GeneratingPath && !EndOfPath)
                        User.PathRecalcNeeded = true;

                    return false;
                }
            }

            // Si es una silla y no es el último paso antes de sentarnos.
            if ((this.mGameMap[To.X, To.Y] == 2 && !EndOfPath))
            {
                if (GeneratingPath)
                    return false;
                else if (!TileContainsChair(square))
                    return false;
            }

            // Si la diferencia entre cada baldosa es mayor de 1.5, no podemos pisarla.
            double HeightDiff = this.SqAbsoluteHeight(To.X, To.Y) - this.SqAbsoluteHeight(From.X, From.Y);
            return !(HeightDiff > 1.5);
        }

        /// <summary>
        /// Obtiene una lista con los RoomItem que hay en una determinada baldosa.
        /// </summary>
        public List<RoomItem> GetCoordinatedItems(Point coord)
        {
            if (mCoordinatedItems.ContainsKey(coord))
                return (List<RoomItem>)mCoordinatedItems[coord];

            return new List<RoomItem>();
        }

        internal bool TileContainsItems(Point coord)
        {
            return mCoordinatedItems.ContainsKey(coord);
        }

        internal bool SquareHasUsers(int X, int Y)
        {
            return userMap.ContainsKey(new Point(X, Y));
        }

        internal List<RoomUser> SquareHasUsersNear(int X, int Y)
        {
            List<RoomUser> users = new List<RoomUser>();

            if (SquareHasUsers(X - 1, Y))
            {
                users.AddRange(GetRoomUsers(new Point(X - 1, Y)));
            }
            if (SquareHasUsers(X + 1, Y))
            {
                users.AddRange(GetRoomUsers(new Point(X + 1, Y)));
            }
            if (SquareHasUsers(X, Y - 1))
            {
                users.AddRange(GetRoomUsers(new Point(X, Y - 1)));
            }
            if (SquareHasUsers(X, Y + 1))
            {
                users.AddRange(GetRoomUsers(new Point(X, Y + 1)));
            }

            return users;
        }

        internal List<RoomUser> SquareHasUsersInFront(int X, int Y)
        {
            List<RoomUser> users = new List<RoomUser>();
            if (SquareHasUsers(X, Y))
            {
                users.AddRange(GetRoomUsers(new Point(X, Y)));
            }

            return users;
        }

        public List<RoomUser> SquareHasUsersArround(Point tile)
        {
             List<RoomUser> users = new List<RoomUser>();

            for (int i = 0; i < PathFinder.TilesArround.Length; i++)
            {
                Point newTile = new Point(tile.X + PathFinder.TilesArround[i].X, tile.Y + PathFinder.TilesArround[i].Y);
                if (SquareHasUsers(newTile.X, newTile.Y))
                users.AddRange(GetRoomUsers(newTile));
            }
            return users;
        }


        internal MovementState GetChasingMovement(int X, int Y)
        {
            bool moveToLeft = true;
            bool moveToRight = true;
            bool moveToUp = true;
            bool moveToDown = true;

            for (int i = 1; i < 4; i++)
            {
                // Left
                if (moveToLeft)
                {
                    if (moveToLeft && SquareHasUsers(X - i, Y))
                        return MovementState.left;
                    else if (i == 1 && !tileIsWalkable(X - i, Y, false))
                        moveToLeft = false;
                }

                // Right
                if (moveToRight)
                {
                    if (moveToRight && SquareHasUsers(X + i, Y))
                        return MovementState.right;
                    else if (i == 1 && !tileIsWalkable(X + i, Y, false))
                        moveToRight = false;
                }

                // Up
                if (moveToUp)
                {
                    if (moveToUp && SquareHasUsers(X, Y - i))
                        return MovementState.up;
                    else if (i == 1 && !tileIsWalkable(X, Y - i, false))
                        moveToUp = false;
                }

                // Down
                if (moveToDown)
                {
                    if (moveToDown && SquareHasUsers(X, Y + i))
                        return MovementState.down;
                    else if (i == 1 && !tileIsWalkable(X, Y + i, false))
                        moveToDown = false;
                }

                // Breaking bucle
                if (i == 1 && !moveToLeft && !moveToRight && !moveToUp && !moveToDown)
                    return MovementState.none;
            }

            List<MovementState> movements = new List<MovementState>();
            if (moveToLeft)
                movements.Add(MovementState.left);
            if (moveToRight)
                movements.Add(MovementState.right);
            if (moveToUp)
                movements.Add(MovementState.up);
            if (moveToDown)
                movements.Add(MovementState.down);

            return movements[new Random().Next(movements.Count)];
        }

        internal MovementState GetEscapeMovement(int X, int Y, MovementState state)
        {
            if (state == MovementState.none)
                state = MovementState.right;

            for (int i = 1; i < 4; i++)
            {
                if (state == MovementState.left)
                {
                    if (i == 1 && !tileIsWalkable(X - i, Y, false))
                    {
                        if (itemCanBePlacedHere(X, Y - i))
                            return MovementState.up;
                        else if (itemCanBePlacedHere(X, Y + i))
                            return MovementState.down;
                        else
                            return MovementState.right;
                    }
                    else if (SquareHasUsers(X - i, Y))
                    {
                        return MovementState.right;
                    }

                    continue;
                }
                else if (state == MovementState.right)
                {
                    if (i == 1 && !tileIsWalkable(X + i, Y, false))
                    {
                        if (itemCanBePlacedHere(X, Y - i))
                            return MovementState.up;
                        else if (itemCanBePlacedHere(X, Y + i))
                            return MovementState.down;
                        else
                            return MovementState.left;
                    }
                    else if (SquareHasUsers(X + i, Y))
                    {
                        return MovementState.right;
                    }

                    continue;
                }
                else if (state == MovementState.up)
                {
                    if (i == 1 && !tileIsWalkable(X, Y - i, false))
                    {
                        if (itemCanBePlacedHere(X - i, Y))
                            return MovementState.left;
                        else if (itemCanBePlacedHere(X + i, Y))
                            return MovementState.right;
                        else
                            return MovementState.down;
                    }
                    else if (SquareHasUsers(X, Y - i))
                    {
                        return MovementState.down;
                    }

                    continue;
                }
                else if (state == MovementState.down)
                {
                    if (i == 1 && !tileIsWalkable(X, Y + i, false))
                    {
                        if (itemCanBePlacedHere(X - i, Y))
                            return MovementState.left;
                        else if (itemCanBePlacedHere(X + i, Y))
                            return MovementState.right;
                        else
                            return MovementState.up;
                    }
                    else if (SquareHasUsers(X, Y + i))
                    {
                        return MovementState.up;
                    }

                    continue;
                }
            }

            return state;
        }

        /// <summary>
        /// Determina si se puede andar/poner furni en una baldosa.
        /// </summary>
        public bool tileIsWalkable(int pX, int pY, bool isUser, bool endPath = false, bool guildGate = false)
        {
            // Si hay un usuario en la baldosa.
            // Comprobamos si se puede pisar la baldosa.
            if (!isUser)
            {
                if (SquareHasUsers(pX, pY))
                    return false;

                if (!ValidTile(pX, pY) || GameMap[pX, pY] != 1)
                    return false;
            }
            else
            {
                if (SquareHasUsers(pX, pY))
                {
                    if (endPath)
                        return false;
                    if (!room.RoomData.AllowWalkthrough && !(room.GetGameMap().Model.SquareInFrontDoor.X == pX && room.GetGameMap().Model.SquareInFrontDoor.Y == pY))
                        return false;
                }

                if (!ValidTile(pX, pY))
                    return false;

                if (guildGate == false)
                {
                    if (GameMap[pX, pY] == 0)
                        return false;
                }
            }

            return Model.SqState[pX, pY] == SquareState.OPEN;
        }

        internal static bool TilesTouching(int X1, int Y1, int X2, int Y2)
        {
            if (!(Math.Abs(X1 - X2) > 1 || Math.Abs(Y1 - Y2) > 1))
                return true;
            if (X1 == X2 && Y1 == Y2)
                return true;

            return false;
        }

        internal static bool TilesTouching2x2(int X1, int Y1, int X2, int Y2)
        {
            if (TilesTouching(X1, Y1, X2, Y2) || TilesTouching(X1, Y1 + 1, X2, Y2) || TilesTouching(X1 + 1, Y1, X2, Y2) || TilesTouching(X1 + 1, Y1 + 1, X2, Y2))
                return true;

            return false;
        }

        internal void TeleportToItem(RoomUser user, RoomItem item)
        {
            int coordAtualX = user.IsWalking ? user.SetX : user.X; // se está andando pega o lugar que ele está indo, senão, o lugar atual que está
            int coordAtualY = user.IsWalking ? user.SetY : user.Y; // se está andando pega o lugar que ele está indo, senão, o lugar atual que está

            user.X = item.GetX;
            user.Y = item.GetY;
            user.IsTeleporting = true;

            GameMap[coordAtualX, coordAtualY] = user.SqState;
            user.SqState = GameMap[item.GetX, item.GetY];            

            UpdateUserMovement(new Point(coordAtualX, coordAtualY), new Point(item.Coordinate.X, item.Coordinate.Y), user);
            room.GetRoomUserManager().UpdateUserStatus(user, false);
            user.ClearMovement(true);
        }

        internal bool itemCanBePlacedHere(int x, int y)
        {
            if (Model.MapSizeX - 1 < x || Model.MapSizeY - 1 < y || (x == Model.DoorX && y == Model.DoorY))
                return false;

            return mGameMap[x, y] == 1;
        }

        /// <summary>
        /// Obtiene las baldosas que están ocupadas por un item.
        /// </summary>
        internal static Dictionary<int, ThreeDCoord> GetAffectedTiles(int Length, int Width, int PosX, int PosY, int Rotation)
        {
            var x = 0;

            var PointList = new Dictionary<int, ThreeDCoord>();

            if (Length == 1 && Width == 1)
            {
                PointList.Add(x++, new ThreeDCoord(PosX, PosY, 0));
            }

            if (Length > 1)
            {
                if (Rotation == 0 || Rotation == 4)
                {
                    for (var i = 0; i < Length; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (var j = 1; j < Width; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i));
                        }
                    }
                }
                else if (Rotation == 2 || Rotation == 6)
                {
                    for (var i = 0; i < Length; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (var j = 1; j < Width; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, (i < j) ? j : i));
                        }
                    }
                }
            }
            else if (Width > 1)
            {
                if (Rotation == 0 || Rotation == 4)
                {
                    for (var i = 0; i < Width; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (var j = 1; j < Length; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, (i < j) ? j : i));
                        }
                    }
                }
                else if (Rotation == 2 || Rotation == 6)
                {
                    for (var i = 0; i < Width; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (var j = 1; j < Length; j++)
                        {
                            PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i));
                        }
                    }
                }
            }

            return PointList;
        }

        /// <summary>
        /// Comprueba si un item se puede mover con un roller a una determinada baldosa.
        /// </summary>
        internal bool CanRollItemHere(int x, int y, double z, bool isUser)
        {
            if (!ValidTile(x, y))
                return false;
            else if (Model.SqState[x, y] == SquareState.BLOCKED)
                return false;
            else if (SquareHasUsers(x, y) && !room.RoomData.AllowWalkthrough)
                return false;
            else if (isUser ? mGameMap[x, y] == 0 : (mGameMap[x, y] == 0 && !tileIsStackeable(new Point(x, y))))
                return false;
            else if (isUser ? (ItemHeightMap[x, y] - z > 1.5) : ItemHeightMap[x, y] > z)
                return false;

            return true;
        }

        internal double SqAbsoluteHeightGameMap(int X, int Y)
        {
            try
            {
                var point = new Point(X, Y);
                if (mCoordinatedItems.ContainsKey(point))
                {
                    var items = (List<RoomItem>)mCoordinatedItems[point];

                    /*RoomItem PosibleItem = items.Find(x => x.GetBaseItem().Name.ToLower().StartsWith("tile_stackmagic"));
                    if(PosibleItem != null)
                    {
                        return PosibleItem.TotalHeight;
                    }*/

                    return SqAbsoluteHeightGameMap(X, Y, items);
                }

                return Model.SqFloorHeight[X, Y];
            }
            catch
            {
                return 0;
            }
        }

        internal double SqAbsoluteHeightGameMap(int x, int y, List<RoomItem> itemsOnSquare)
        {
            try
            {
                var highestStack = (double)Model.SqFloorHeight[x, y];
                var deductable = 0.0;
                var stackable = true;

                foreach (var item in itemsOnSquare.Where(item => item.TotalHeight > highestStack || (item.GetBaseItem().Name.ToLower().StartsWith("tile_stackmagic"))))
                {
                    if (item.GetBaseItem().Name.ToLower().StartsWith("tile_stackmagic"))
                        return item.TotalHeight;
                    else if (item.GetBaseItem().IsSeat || item.GetBaseItem().InteractionType == InteractionType.bed)
                        deductable = item.GetBaseItem().Height;

                    stackable = item.GetBaseItem().Stackable;
                    highestStack = item.TotalHeight;
                }

                highestStack -= deductable;

                if (!stackable)
                    return 64;
                else
                    return highestStack < 0 ? 0 : highestStack;
            }
            catch
            {
                return 0.0;
            }
        }

        internal double SqAbsoluteHeight(int X, int Y)
        {
            var point = new Point(X, Y);
            if (mCoordinatedItems.ContainsKey(point))
            {
                var items = (List<RoomItem>)mCoordinatedItems[point];
                return SqAbsoluteHeight(X, Y, items);
            }

            return Model.SqFloorHeight[X, Y];
        }

        internal double SqAbsoluteHeight(int x, int y, List<RoomItem> itemsOnSquare)
        {
            try
            {
                var highestStack = (double)Model.SqFloorHeight[x, y];
                var deductable = 0.0;

                foreach (var item in itemsOnSquare.Where(item => item != null && item.GetBaseItem() != null && item.TotalHeight > highestStack))
                {
                    if (GameMap[item.GetX, item.GetY] == 2)
                        deductable = item.GetBaseItem().Height;

                    highestStack = item.TotalHeight;
                }

                highestStack -= deductable;
                return highestStack < 0 ? 0 : highestStack;
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Comprueba si la baldosa está dentro del tamaño de sala.
        /// </summary>
        public bool ValidTile(int X, int Y, bool validaCorreto = false)
        {

            if (X < 0 || Y < 0 || X >= Model.MapSizeX || Y >= Model.MapSizeY && validaCorreto == false)
            {
                return false;
            }

            if (X < 0 || Y < 0 || X >= Model.MapSizeX || Y >= Model.MapSizeY || (Model.SqState[X,Y] == SquareState.BLOCKED && validaCorreto == true))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Devuelve la lista de items que están más elevados que otro.
        /// </summary>
        /// <param name="pX"></param>
        /// <param name="pY"></param>
        /// <param name="pZ"></param>
        /// <returns></returns>
        public List<RoomItem> GetRoomItemForMinZ(int pX, int pY, double pZ)
        {
            var itemsToReturn = new List<RoomItem>();
            var coord = new Point(pX, pY);

            if (mCoordinatedItems.ContainsKey(coord))
            {
                var itemsFromSquare = (List<RoomItem>)mCoordinatedItems[coord];
                foreach (var item in itemsFromSquare)
                {
                    if (pZ <= item.GetZ)
                        itemsToReturn.Add(item);
                }
            }

            return itemsToReturn;
        }

        /// <summary>
        /// Verifica si se puede subir de altura un furni con varias alturas.
        /// </summary>
        internal Boolean CanUpdateMultiHeight(RoomItem Item)
        {
            foreach (ThreeDCoord tile in Item.GetAffectedTiles.Values)
            {
                Point coord = new Point(tile.X, tile.Y);
                RoomItem priorityItem = getMaxHeightItem(coord);
                if (Item != priorityItem)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Obtiene el furni con prioridad (el más alto).
        /// </summary>
        public RoomItem getMaxHeightItem(Point tile)
        {
            if (CoordinatedItems.ContainsKey(tile))
                return ((List<RoomItem>)CoordinatedItems[tile]).Last();

            return null;
        }

        /// <summary>
        /// Determina si el furni con prioridad es una silla.
        /// </summary>
        private bool TileContainsChair(Point tile)
        {
            RoomItem Item = getMaxHeightItem(tile);

            if (Item != null)
                return Item.GetBaseItem().IsSeat;

            return false;
        }

        /// <summary>
        /// Determina si una baldosa se puede stackear o no.
        /// </summary>
        private bool tileIsStackeable(Point tile)
        {
            RoomItem Item = getMaxHeightItem(tile);
            if (Item == null)
                return false;

            return Item.GetBaseItem().Stackable;
        }
        #endregion

        #region Destroy
        /// <summary>
        /// Limpia el valor de las variables que están guardadas en caché de este GameMap.
        /// </summary>
        internal void Destroy()
        {
            userMap.Clear();
            mCoordinatedItems.Clear();
            mTileHeightApilable.Clear();

            if (room.RoomData.LastModelName == "custom" && room.RoomData.ModelName == "custom")
            {
                mStaticModel.Destroy();
                mStaticModel = null;
            }

            Array.Clear(mGameMap, 0, mGameMap.Length);
            Array.Clear(mUserItemEffect, 0, mUserItemEffect.Length);
            Array.Clear(mItemHeightMap, 0, mItemHeightMap.Length);

            StaticHeightmap = null;
            userMap = null;
            mGameMap = null;
            mUserItemEffect = null;
            mItemHeightMap = null;
            mCoordinatedItems = null;
            mTileHeightApilable = null;
            room = null;
        }
        #endregion
    }
}
