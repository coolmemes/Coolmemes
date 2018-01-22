using System;
using System.Collections.Generic;
using System.Linq;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Items;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;
using System.Drawing;
using Otanix.HabboHotel.Rooms.Wired;
using Butterfly.HabboHotel.Items.Interactors;
using ButterStorm;
using Butterfly.Util;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions
{
    class FurniStatePosMatch : IWiredCondition
    {
        private RoomItem item;
        private List<RoomItem> items;
        private bool isDisposed;
        private Dictionary<uint, OriginalItemLocation> originalItemLocation;

        private readonly int furniState;
        private readonly int furniDirection;
        private readonly int furniPosition;

        public FurniStatePosMatch(RoomItem item, List<RoomItem> items, string itemsState, Dictionary<uint, OriginalItemLocation> originalItemLocation)
        {
            this.originalItemLocation = originalItemLocation;
            this.item = item;
            this.items = items;
            this.isDisposed = false;

            if (itemsState.Length > 0 && itemsState.Contains(","))
            {
                this.furniState = int.Parse(itemsState.Split(',')[0]);
                this.furniDirection = int.Parse(itemsState.Split(',')[1]);
                this.furniPosition = int.Parse(itemsState.Split(',')[2]);
            } 
        }

        public List<RoomItem> Items
        {
            get
            {
                return items;
            }
        }

        public int FurniState
        {
            get
            {
                return furniState;
            }
        }

        public int FurniDirection
        {
            get
            {
                return furniDirection;
            }
        }

        public int FurniPosition
        {
            get
            {
                return furniPosition;
            }
        }

        public bool AllowsExecution(RoomUser user)
        {
            // InteractorGenericSwitch.DoAnimation(item);
           
            if(items.Count <= 0)
                return true;

            OriginalItemLocation location;
            foreach (RoomItem _item in items)
            {
                if (!this.originalItemLocation.ContainsKey(_item.Id))
                    continue;
                
                location = this.originalItemLocation[_item.Id];

                if (FurniPosition == 1 && !(location.X == _item.GetX && _item.GetY == location.Y && _item.TotalHeight == location.Height))
                {
                    return false;
                }
                if (FurniDirection == 1 && _item.Rot != location.Rot)
                {
                    return false;
                }

                if (FurniState == 1 && _item.ExtraData != location.ExtraData)
                {
                    if (string.IsNullOrEmpty(_item.ExtraData) && location.ExtraData == "0") { }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = furniState + "," + furniDirection + "," + furniPosition + ";;false";
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

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }

        public void Dispose()
        {
            isDisposed = true;
            item = null;
            if (items != null)
                items.Clear();
            items = null;
        }

        public bool Disposed()
        {
            return isDisposed;
        }
    }
}
