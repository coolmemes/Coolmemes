using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Util;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class MoveToDir : MovementManagement, IWiredEffect, IWiredTrigger
    {
        private Room room;
        private WiredHandler handler;
        private RoomItem item;
        private List<RoomItem> items;
        private MovementDirection startDirection;
        private WhenMovementBlock whenMoveIsBlocked;
        private Boolean needChange;

        public MoveToDir(List<RoomItem> items, MovementDirection startDirection, WhenMovementBlock whenMoveIsBlocked, Room room, WiredHandler handler, RoomItem itemID)
        {
            this.items = items;
            this.room = room;
            this.handler = handler;
            this.item = itemID;
            this.startDirection = startDirection;
            this.whenMoveIsBlocked = whenMoveIsBlocked;
            this.needChange = true;
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public int StartDirection
        {
            get
            {
                return (int)startDirection;
            }
        }

        public int WhenMoveIsBlocked
        {
            get
            {
                return (int)whenMoveIsBlocked;
            }
        }

        public void Handle(RoomUser user, Team team, RoomItem iItem)
        {
            Queue<RoomItem> toRemove = new Queue<RoomItem>();

            foreach (var _item in items)
            {
                if (room.GetRoomItemHandler().GetItem(_item.Id) == null)
                {
                    toRemove.Enqueue(_item);
                    continue;
                }

                HandleMovement(_item);
            }

            while (toRemove.Count > 0)
            {
                RoomItem itemToRemove = toRemove.Dequeue();
                if (items.Contains(itemToRemove))
                    items.Remove(itemToRemove);
            }
        }

        private void HandleMovement(RoomItem item)
        {
            if (item.movetodirMovement == MovementDirection.none || needChange)
            {
                item.movetodirMovement = startDirection;
                needChange = false;
            }

            var newPoint = base.HandleMovementDir(item.Coordinate, item.movetodirMovement, item.Rot);
            if (newPoint != item.Coordinate)
            {
                List<RoomUser> RoomUsers;

                if (item.movetodirMovement == MovementDirection.right)
                {
                    RoomUsers = room.GetGameMap().SquareHasUsersInFront(item.GetX + 1, item.GetY);
                }
                else if(item.movetodirMovement == MovementDirection.down)
                {
                    RoomUsers = room.GetGameMap().SquareHasUsersInFront(item.GetX, item.GetY + 1);
                }
                else if(item.movetodirMovement == MovementDirection.left)
                {
                    RoomUsers = room.GetGameMap().SquareHasUsersInFront(item.GetX - 1, item.GetY);
                }
                else
                {
                    RoomUsers = room.GetGameMap().SquareHasUsersInFront(item.GetX, item.GetY - 1);
                }

                if (RoomUsers.Count > 0) // colisión
                {
                    foreach (RoomUser roomUser in RoomUsers)
                    {
                        if (roomUser != null && !roomUser.IsBot && !roomUser.IsPet)
                        {
                            roomUser.wiredItemIdTrigger = item.Id;
                            TriggerCollision(roomUser, room);
                        }
                    }

                    RoomUsers.Clear();
                    RoomUsers = null;

                    return;
                }
                else if (room.GetGameMap().tileIsWalkable(newPoint.X, newPoint.Y, false))
                {
                    room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, item.Rot, false, false, true, true);
                }
                else
                {
                    switch (whenMoveIsBlocked)
                    {
                        #region None
                        case WhenMovementBlock.none:
                            {
                                item.movetodirMovement = MovementDirection.none;
                                break;
                            }
                        #endregion
                        #region Right45
                        case WhenMovementBlock.right45:
                            {
                                if (item.movetodirMovement == MovementDirection.right)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.left)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.up)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.down)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.upleft)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.upright)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    return;
                                }
                                else if (item.movetodirMovement == MovementDirection.downright)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.downleft)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                }

                                break;
                            }
                        #endregion
                        #region Right90
                        case WhenMovementBlock.right90:
                            {
                                if (item.movetodirMovement == MovementDirection.right)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.left)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.up)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.down)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.upleft)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.upright)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    return;
                                }
                                else if (item.movetodirMovement == MovementDirection.downright)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.downleft)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                }

                                break;
                            }
                        #endregion
                        #region Left45
                        case WhenMovementBlock.left45:
                            {
                                if (item.movetodirMovement == MovementDirection.right)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.left)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.up)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.down)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.upleft)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.upright)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.downright)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.downleft)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                }

                                break;
                            }
                        #endregion
                        #region Left90
                        case WhenMovementBlock.left90:
                            {
                                if (item.movetodirMovement == MovementDirection.right)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.left)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.up)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.down)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY, false)) // derecha
                                    {
                                        item.movetodirMovement = MovementDirection.right;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY - 1, false)) // arriba
                                    {
                                        item.movetodirMovement = MovementDirection.up;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY, false)) // izq
                                    {
                                        item.movetodirMovement = MovementDirection.left;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX, item.GetY + 1, false)) // abajo
                                    {
                                        item.movetodirMovement = MovementDirection.down;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.upleft)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.upright)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.downright)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                }
                                else if (item.movetodirMovement == MovementDirection.downleft)
                                {
                                    if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY + 1, false)) // abajo derecha
                                    {
                                        item.movetodirMovement = MovementDirection.downright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX + 1, item.GetY - 1, false)) // arriba derecha
                                    {
                                        item.movetodirMovement = MovementDirection.upright;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY - 1, false)) // arriba izq
                                    {
                                        item.movetodirMovement = MovementDirection.upleft;
                                        break;
                                    }
                                    else if (room.GetGameMap().tileIsWalkable(item.GetX - 1, item.GetY + 1, false)) // abajo izq
                                    {
                                        item.movetodirMovement = MovementDirection.downleft;
                                        break;
                                    }
                                }

                                break;
                            }
                        #endregion
                        #region Turn Back
                        case WhenMovementBlock.turnback:
                            {
                                int ActualMovement = (int)item.movetodirMovement + 4;
                                if (ActualMovement > 7)
                                    ActualMovement -= 8;

                                item.movetodirMovement = (MovementDirection)ActualMovement;
                                break;
                            }
                        #endregion
                        #region Random
                        case WhenMovementBlock.turnrandom:
                            {
                                item.movetodirMovement = (MovementDirection)new Random().Next(1, 7);
                                break;
                            }
                        #endregion
                    }

                    newPoint = base.HandleMovementDir(item.Coordinate, item.movetodirMovement, item.Rot);
                    room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, item.Rot, false, false, true, true);
                }
            }
        }

        public void TriggerCollision(RoomUser user, Room room)
        {
            List<RoomItem> collisions = new List<RoomItem>();
            collisions.AddRange(room.GetWiredHandler().GetWiredsInteractor(InteractionType.triggercollision));

            foreach (RoomItem wired in collisions)
            {
                if (!wired.wiredEventUser.Contains(user))
                    wired.wiredEventUser.Add(user);
            }

            collisions.Clear();
            collisions = null;
        }

        public void Dispose()
        {
            room = null;
            handler = null;
            if (items != null)
                items.Clear();
            items = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = ((int)startDirection).ToString() + ";" + ((int)whenMoveIsBlocked).ToString() + ";false";
            string wired_to_item = "";
            if (items.Count > 0)
            {
                lock (items)
                {
                    foreach (var id in items)
                    {
                        wired_to_item += id.Id + ";";
                    }
                    if (wired_to_item.Length > 0)
                        wired_to_item = wired_to_item.Substring(0, wired_to_item.Length - 1);
                }
            }
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }
    }
}
