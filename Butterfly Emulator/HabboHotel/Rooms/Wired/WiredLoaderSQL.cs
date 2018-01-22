using Butterfly;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Rooms.Wired;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otanix.HabboHotel.Rooms.Wired
{
    class WiredLoaderSQL
    {
        internal List<RoomItem> wiredItems { get; private set; }
        internal string[] StringSettings { get; private set; }
        internal Dictionary<uint, OriginalItemLocation> originalPositionList { get; private set; }

        internal WiredLoaderSQL(RoomItem Item, Room Room, IQueryAdapter dbClient)
        {
            wiredItems = new List<RoomItem>(5);
            StringSettings = new string[3] { "", "", "" };
            originalPositionList = new Dictionary<uint, OriginalItemLocation>();

            dbClient.setQuery("SELECT * FROM items_wired WHERE item_id = " + Item.Id + " LIMIT 1");
            DataRow dRow = dbClient.getRow();
            if (dRow == null)
                return;

            if (WiredUtillity.NeedsFurnitures(Item.GetBaseItem().InteractionType))
            {
                string result = (string)dRow["wired_to_item"];

                if (result.Contains(";"))
                {
                    foreach (string itemId in result.Split(';'))
                    {
                        RoomItem targetItem = Room.GetRoomItemHandler().GetItem(Convert.ToUInt32(itemId));
                        if (targetItem != null && !wiredItems.Contains(targetItem))
                            wiredItems.Add(targetItem);
                    }
                }
                else if (result.Length > 0)
                {
                    RoomItem targetItem = Room.GetRoomItemHandler().GetItem(Convert.ToUInt32(result));
                    if (targetItem != null && !wiredItems.Contains(targetItem))
                        wiredItems.Add(targetItem);
                }
            }

            if (WiredUtillity.HaveSettings(Item.GetBaseItem().InteractionType))
            {
                string result = (string)dRow["wired_data"];
                for (int i = 0; i < 3; i++)
                    StringSettings[i] = result.Split(';')[i].ToString();
            }

            if (WiredUtillity.HaveLocations(Item.GetBaseItem().InteractionType))
            {
                string result = (string)dRow["wired_original_location"];
                foreach (string value in result.Split(';'))
                {
                    try
                    {
                        uint itemID = Convert.ToUInt32(value.Split(',')[0]);
                        int x = Convert.ToInt32(value.Split(',')[1].ToString());
                        int y = Convert.ToInt32(value.Split(',')[2].ToString());
                        double height = Double.Parse(value.Split(',')[4], OtanixEnvironment.cultureInfo);
                        int rot = Convert.ToInt32(value.Split(',')[3].ToString());
                        string extradata = (string)value.Split(',')[5];

                        originalPositionList.Add(itemID, new OriginalItemLocation(itemID, x, y, height, rot, extradata));
                    }
                    catch { }
                }
            }
        }
    }
}
