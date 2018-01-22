using System;
using System.Collections.Generic;

namespace Plus.HabboHotel.Catalog.Bundles
{
    internal class BundleQuarto
    {
        internal uint PredesignedId, RoomId;
        internal string RoomModel, CatalogItems;
        internal string[] FloorItems, WallItems, RoomDecoration;
        internal List<BundleFloorMobis> FloorItemData;
        internal List<BundleParedeMobis> WallItemData;

        internal BundleQuarto(uint predesignedId, uint roomId, string roomModel, string floorItems, string wallItems,
            string catalogItems, string roomDecoration)
        {
            PredesignedId = predesignedId;
            RoomId = roomId;
            RoomModel = roomModel;
            FloorItems = ((floorItems == string.Empty) ? null : floorItems.Split(';'));
            if (FloorItems != null)
            {
                FloorItemData = new List<BundleFloorMobis>();
                foreach (var item in FloorItems)
                {
                    var itemsData = item.Split(new string[] { "$$$$" }, StringSplitOptions.None);
                    FloorItemData.Add(new BundleFloorMobis(Convert.ToUInt32(itemsData[0]),
                        Convert.ToInt32(itemsData[1]), Convert.ToInt32(itemsData[2]),
                        Convert.ToInt32(itemsData[4]), Convert.ToDouble(itemsData[3]), itemsData[5]));
                }
            }

            WallItems = ((wallItems == string.Empty) ? null : wallItems.Split(';'));
            if (WallItems != null)
            {
                WallItemData = new List<BundleParedeMobis>();
                foreach (var item in WallItems)
                {
                    var itemsData = item.Split(new string[] { "$$$$" }, StringSplitOptions.None);
                    WallItemData.Add(new BundleParedeMobis(Convert.ToUInt32(itemsData[0]), itemsData[1], itemsData[2]));
                }
            }

            CatalogItems = catalogItems;
            RoomDecoration = roomDecoration.Split(';');
        }
    }
}



