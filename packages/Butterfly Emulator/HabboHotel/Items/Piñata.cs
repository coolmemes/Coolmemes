using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Items.Core;
using Butterfly.HabboHotel.Rooms;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace ButterStorm.HabboHotel.Items
{
    internal class PiñataItem
    {
        internal UInt32 item_id;
        internal List<uint> rewards;

        internal PiñataItem(DataRow dRow)
        {
            item_id = Convert.ToUInt32(dRow["item_baseid"]);
            string rewardsString = (string)dRow["rewards"];
            rewards = new List<uint>();
            foreach (var str in rewardsString.Split(';'))
            {
                rewards.Add(Convert.ToUInt32(str));
            }
        }
    }

    class PiñataHandler
    {
        internal Dictionary<UInt32, PiñataItem> Piñatas;

        internal void Initialize(IQueryAdapter dbClient)
        {
            Piñatas = new Dictionary<UInt32, PiñataItem>();


            dbClient.setQuery("SELECT * FROM piñatas_items");
            DataTable table = dbClient.getTable();

            foreach (DataRow dRow in table.Rows)
            {
                if (!Piñatas.ContainsKey(Convert.ToUInt32(dRow["item_baseid"])))
                {
                    var piñataAAgregar = new PiñataItem(dRow);
                    Piñatas.Add(Convert.ToUInt32(dRow["item_baseid"]), piñataAAgregar);
                }
            }
        }

        internal void DeliverPiñataRandomItem(RoomUser user, Room room, RoomItem item)
        {
            if (room == null)
                return;

            if (item == null)
                return;

            if (item.GetBaseItem().InteractionType != InteractionType.piñata && item.GetBaseItem().InteractionType != InteractionType.dalia)
                return;

            if (!Piñatas.ContainsKey(item.GetBaseItem().ItemId))
                return;

            PiñataItem piñata;
            Piñatas.TryGetValue(item.GetBaseItem().ItemId, out piñata);
            if (piñata == null)
                return;

            // backup de las variables:
            int X = item.GetX, Y = item.GetY;

            // Borramos la piñata.
            room.GetRoomItemHandler().RemoveFurniture(user.GetClient(), item);

            int randomId = new Random().Next(piñata.rewards.Count - 1);
            item.BaseItem = piñata.rewards[randomId];
            item.ExtraData = "0";
            item.SetState(X, Y);

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE items SET base_id = '" + item.BaseItem + "' WHERE item_id = " + item.Id);
                dbClient.setQuery("UPDATE items_extradata SET data = @extradata WHERE item_id = " + item.Id);
                dbClient.addParameter("extradata", item.ExtraData);
                dbClient.runQuery();
            }

            if (!room.GetRoomItemHandler().SetFloorItem(user.GetClient(), item, item.GetX, item.GetY, item.Rot, true, false, true, false, true))
            {
                user.GetClient().SendNotif("Ha ocurrido un error al crear la piñata!");
                return;
            }
        }

        internal void DeliverBalloonRandomItem(RoomUser User, Room Room, RoomItem Item)
        {
            if (!Piñatas.ContainsKey(Item.GetBaseItem().ItemId))
                return;

            PiñataItem piñata;
            Piñatas.TryGetValue(Item.GetBaseItem().ItemId, out piñata);
            if (piñata == null)
                return;

            // backup de las variables:
            int X = Item.GetX, Y = Item.GetY;

            // Borramos la piñata.
            Item.ExtraData = "1";
            Item.UpdateState();
            Room.GetRoomItemHandler().RemoveFurniture(User.GetClient(), Item);

            uint itemID = EmuSettings.FIRST_BALLOON_PRESENT_ID + (uint)(new Random().Next(0, (int)(EmuSettings.LAST_BALLOON_PRESENT_ID - EmuSettings.FIRST_BALLOON_PRESENT_ID)));

            uint randomId = piñata.rewards[new Random().Next(0, piñata.rewards.Count - 1)];
            Item rareItem = OtanixEnvironment.GetGame().GetItemManager().GetItem(randomId);
            if (rareItem == null)
                return;

            Item.BaseItem = itemID;
            Item.ExtraData = Item.OwnerId + ";0;" + Furnidata.GetPublicNameByItemName(rareItem.Name);
            Item.SetState(X, Y);

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE items SET base_id = '" + itemID + "' WHERE item_id = " + Item.Id);

                dbClient.setQuery("UPDATE items_extradata SET data = @extradata WHERE item_id = " + Item.Id);
                dbClient.addParameter("extradata", rareItem.Name);
                dbClient.runQuery();

                dbClient.setQuery("INSERT INTO user_presents (item_id,base_id,amount,extra_data) VALUES (" + Item.Id + "," + randomId + "," + 1 + ",@extra_data)");
                dbClient.addParameter("extra_data", "");
                dbClient.runQuery();
            }

            if (!Room.GetRoomItemHandler().SetFloorItem(User.GetClient(), Item, Item.GetX, Item.GetY, Item.Rot, true, false, true, false, true))
            {
                User.GetClient().SendNotif("Ha ocurrido un error al crear la piñata!");
                return;
            }
        }
    }
}