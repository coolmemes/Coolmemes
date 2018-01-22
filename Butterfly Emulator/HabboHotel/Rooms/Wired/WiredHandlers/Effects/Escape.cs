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
    class Escape : MovementManagement, IWiredEffect, IWiredTrigger
    {
        private Room room;
        private WiredHandler handler;
        private RoomItem item;
        private List<RoomItem> items;
        private bool isDisposed;

        public Escape(List<RoomItem> items, int delay, Room room, WiredHandler handler, RoomItem itemID)
        {
            this.items = items;
            this.room = room;
            this.handler = handler;
            this.item = itemID;
            this.isDisposed = false;
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public void Handle(RoomUser user, Team team, RoomItem iItem)
        {
            //InteractorGenericSwitch.DoAnimation(item);
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
            // Lo bueno del sistema sería hacer lo siguiente:
            // 1 - Comprobamos si hay un usuario al lado del item para poder pasar al colisión ya.
            // 2 - Si no hay un usuario, miramos que en las 3 casillas de alrededor no haya usuario.
            // 3 - En caso de no encontrar, mover a una aleatoria de las posibles

            List<RoomUser> RoomUsers = room.GetGameMap().SquareHasUsersNear(item.GetX, item.GetY);
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

            item.escapeMovement = room.GetGameMap().GetEscapeMovement(item.GetX, item.GetY, item.escapeMovement);
            if (item.escapeMovement == MovementState.none)
                return;

            var newPoint = base.HandleMovement(item.Coordinate, item.escapeMovement, item.Rot);

            if (newPoint != item.Coordinate)
            {
                if (room.GetGameMap().tileIsWalkable(newPoint.X, newPoint.Y, false))
                {
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
            isDisposed = true;
            room = null;
            handler = null;
            if (items != null)
                items.Clear();
            items = null;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = ";;false";
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

        public bool Disposed()
        {
            return isDisposed;
        }
    }
}
