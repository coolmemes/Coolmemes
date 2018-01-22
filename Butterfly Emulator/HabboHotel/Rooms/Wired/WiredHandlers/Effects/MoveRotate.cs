using System;
using System.Collections.Generic;
using System.Data;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Database_Manager.Database;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class MoveRotate : MovementManagement, IWiredEffect, IWiredTrigger, IWiredCycleable
    {
        private Room room;
        private WiredHandler handler;
        private RoomItem item;

        private MovementState movement;
        private RotationState rotation;
        private List<RoomItem> items;
        
        private int delay;
        private int cycles;

        private bool isDisposed;

        public MoveRotate(MovementState movement, RotationState rotation, List<RoomItem> items, int delay, Room room, WiredHandler handler, RoomItem itemID)
        {
            this.movement = movement;
            this.rotation = rotation;
            this.items = items;
            this.delay = delay;
            this.room = room;
            this.handler = handler;
            this.cycles = 0;
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

        public int Movement
        {
            get
            {
                return (int)movement;
            }
        }

        public int Rotation
        {
            get
            {
                return (int)rotation;
            }
        }

        public int Time
        {
            get
            {
                return delay;
            }
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            cycles = 0;

            if (delay == 0)
            {
                HandleItems();
            }
            else
            {
                handler.RequestCycle(this);
            }
        }

        public bool OnCycle()
        {
            if (room == null)
                return false;

            cycles++;
            if (cycles > delay)
            {
                cycles = 0;
                HandleItems();
                return false;
            }

            return true;
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

        private bool HandleItems()
        {
            //InteractorGenericSwitch.DoAnimation(item);
            var itemHandled = false;
            Queue<RoomItem> toRemove = new Queue<RoomItem>();
            foreach (var _item in items)
            {
                if (room!=null && room.GetRoomItemHandler()!=null && room.GetRoomItemHandler().GetItem(_item.Id) == null)
                {
                    toRemove.Enqueue(_item);
                    continue;
                }
                if (HandleMovement(_item))
                    itemHandled = true;
            }

            while (toRemove.Count > 0)
            {
                RoomItem itemToRemove = toRemove.Dequeue();
                if (items.Contains(itemToRemove))
                    items.Remove(itemToRemove);
            }
            return itemHandled;
        }

        private bool HandleMovement(RoomItem item)
        {
            var newPoint = HandleMovement(item.Coordinate, movement, item.Rot);
            var newRotation = HandleRotation(item.Rot, rotation);

            if (newPoint != item.Coordinate && newRotation == item.Rot)
            {
                if (room.GetGameMap().tileIsWalkable(newPoint.X, newPoint.Y, false))
                    return room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, newRotation, false, false, true, true);

                if (room.GetGameMap().SquareHasUsers(newPoint.X, newPoint.Y))
                {
                    List<RoomUser> users = room.GetGameMap().SquareHasUsersNear(item.GetX, item.GetY);
                    foreach (RoomUser user in users)
                    {
                        if (user != null && !user.IsBot && !user.IsPet)
                        {
                            user.wiredItemIdTrigger = item.Id;
                            TriggerCollision(user, room);
                        }
                    }

                    users.Clear();
                    users = null;
                }
            }
            else if (newPoint != item.Coordinate)
            {
                if (room.GetGameMap().tileIsWalkable(newPoint.X, newPoint.Y, false))
                    return room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, newRotation, false, false, true, false);
            }
            else if (newRotation != item.Rot)
            {
                return room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, newRotation, false, false, true, false);
            }
            return false;
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
        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = delay.ToString() + ";" + Movement + "," + Rotation + ";false";
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

        public void ResetTimer()
        {

        }
    }
}
