using System.Collections.Generic;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;
using System;
using Otanix.HabboHotel.Rooms.Wired;
using Butterfly.Messages;
using HabboEvents;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Util;
using System.Drawing;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class PositionReset : IWiredTrigger, IWiredCycleable, IWiredEffect
    {
        private RoomItemHandling roomItemHandler;
        private WiredHandler handler;
        private RoomItem itemID;

        private List<RoomItem> items;
        private Dictionary<uint, OriginalItemLocation> originalItemLocation;
        private int delay;
        private int cycles;

        private int FurniState;
        private int FurniDirection;
        private int FurniPosition;

        private bool disposed;

        public PositionReset(List<RoomItem> items, int delay, string data, Dictionary<uint, OriginalItemLocation> originalItemlocation, RoomItemHandling roomItemHandler, WiredHandler handler, RoomItem itemID)
        {
            this.items = items;
            this.originalItemLocation = originalItemlocation;

            if (originalItemlocation.Count <= 0 && this.items.Count > 0)
            {
                foreach (RoomItem nItem in items)
                {
                    this.originalItemLocation.Add(nItem.Id, new OriginalItemLocation(nItem.Id, nItem.GetX, nItem.GetY, nItem.TotalHeight, nItem.Rot, nItem.ExtraData));
                }
            }

            this.delay = delay;
            this.roomItemHandler = roomItemHandler;
            this.cycles = 0;
            this.itemID = itemID;
            this.handler = handler;
            this.disposed = false;

            if(data.Length > 0 && data.Contains(","))
            {
                this.FurniState = int.Parse(data.Split(',')[0]);
                this.FurniDirection = int.Parse(data.Split(',')[1]);
                this.FurniPosition = int.Parse(data.Split(',')[2]);
            }
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public int furniState
        {
            get
            {
                return FurniState;
            }
        }

        public int furniDirection
        {
            get
            {
                return FurniDirection;
            }
        }

        public int furniPosition
        {
            get
            {
                return FurniPosition;
            }
        }

        public int Time
        {
            get
            {
                return delay;
            }
        }

        public bool OnCycle()
        {
            cycles++;
            if (cycles > delay)
            {
                HandleItems();
                return false;
            }
            return true;
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

        private bool HandleItems()
        {
            //InteractorGenericSwitch.DoAnimation(itemID);

            var itemIsMoved = false;
            foreach (var item in items)
            {
                if (!item.GetRoom().GetRoomItemHandler().mFloorItems.ContainsKey(item.Id))
                    continue;

                OriginalItemLocation oldCoordinate = this.GetOriginalItemLocation(item);

                // CAMBIAMOS EL EXTRADATA AL INICIAL.
                if (FurniState == 1)
                {
                    if (item.ExtraData != oldCoordinate.ExtraData)
                    {
                        item.ExtraData = oldCoordinate.ExtraData;
                        item.UpdateState(false, true);
                        if(item.GetBaseItem().InteractionType == InteractionType.gate)
                            item.GetRoom().GetGameMap().updateMapForItem(item);

                        foreach (Point tile in item.GetCoords)
                        {
                            RoomUser user = item.GetRoom().GetRoomUserManager().GetUserForSquare(tile.X, tile.Y);
                            if(user != null)
                            {
                                user.SqState = item.GetRoom().GetGameMap().GameMap[tile.X, tile.Y];
                            }
                        }

                        itemIsMoved = true;
                    }
                }

                // SI LA ROTACIÓN DEL FURNI HA CAMBIADO, ENTONCES DEBEMOS MOVER DIRECTAMENTE EL ITEM A LA BALDOSA EN CUESTIÓN
                if(FurniDirection == 1)
                {
                    if (item.Rot != oldCoordinate.Rot)
                    {
                        if (roomItemHandler.SetFloorItem(null, item, oldCoordinate.X, oldCoordinate.Y, oldCoordinate.Rot, false, false, true, false, true))
                            return true;
                    }
                }

                // EN CASO DE NO HABERSE MOVIDO LA ROTACIÓN, HACEMOS EL EFECTO CHACHI PIRULI DE MOVER EL ITEM ^^
                if(FurniPosition == 1)
                {
                    if (item.GetX != oldCoordinate.X || item.GetY != oldCoordinate.Y || item.TotalHeight != oldCoordinate.Height)
                    {
                        item.SetHeight(oldCoordinate.Height);
                        if (roomItemHandler.SetFloorItem(null, item, oldCoordinate.X, oldCoordinate.Y, item.Rot, false, false, true, true, true))
                            itemIsMoved = true;
                    }
                }
            }

            return itemIsMoved;
        }

        private OriginalItemLocation GetOriginalItemLocation(RoomItem item)
        {
            return this.originalItemLocation[item.Id];
        }

        public void Dispose()
        {
            disposed = true;
            roomItemHandler = null;
            handler = null;
            if (items != null)
                items.Clear();
            items = null;
        }
  
        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = delay.ToString() + ";" + furniState + "," + furniDirection + "," + furniPosition + ";false";
            string wired_to_item = "";
            string wired_original_location = "";
            if (items.Count > 0)
            {
                lock (items)
                {
                    foreach (var id in items)
                    {
                        wired_to_item += id.Id + ";";
                        wired_original_location += id.Id + "," + originalItemLocation[id.Id].X + "," + originalItemLocation[id.Id].Y + "," + originalItemLocation[id.Id].Rot + "," + TextHandling.GetString(originalItemLocation[id.Id].Height) + "," + originalItemLocation[id.Id].ExtraData + ";";
                    }
                    if (wired_to_item.Length > 0)
                        wired_to_item = wired_to_item.Substring(0, wired_to_item.Length - 1);
                    if (wired_original_location.Length > 0)
                        wired_original_location = wired_original_location.Substring(0, wired_original_location.Length - 1);
                }
            }

            wiredInserts.AddQuery("('" + itemID.Id + "', @data" + itemID.Id + ", @to_item" + itemID.Id + ", @original_location" + itemID.Id + ")");
            wiredInserts.AddParameter("data" + itemID.Id, wired_data);
            wiredInserts.AddParameter("to_item" + itemID.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID.Id, wired_original_location);
        }

        public bool Disposed()
        {
            return disposed;
        }

        public void ResetTimer()
        {

        }
    }
}
