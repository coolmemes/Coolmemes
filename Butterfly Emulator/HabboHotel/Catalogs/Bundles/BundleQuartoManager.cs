using Butterfly.HabboHotel.Catalogs;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Plus.HabboHotel.Catalog.Bundles
{
    internal class BundleQuartoManager
    {
        internal Dictionary<uint, BundleQuarto> predesignedRoom;
        internal void Initialize()
        {
            predesignedRoom = new Dictionary<uint, BundleQuarto>();
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM catalog_bundles");
                var table = dbClient.getTable();
                foreach (DataRow row in table.Rows)
                    predesignedRoom.Add(Convert.ToUInt32(row["id"]), new BundleQuarto(Convert.ToUInt32(row["id"]),
                        Convert.ToUInt32(row["id"]), (string)row["room_model"], (string)row["flooritems"].ToString().TrimEnd(';'),
                        (string)row["wallitems"].ToString().TrimEnd(';'), (string)row["catalogitems"].ToString().TrimEnd(';'),
                        (string)row["room_decoration"]));
            }
        }

        internal void buyBundle(GameClient Session, CatalogItem Item)
        {

            BundleQuarto predesigned = OtanixEnvironment.GetGame().GetCatalog().GetPredesignedRooms().predesignedRoom[(uint)Item.PredesignedId];
            if (predesigned == null)
                return;

            string[] decoration = predesigned.RoomDecoration;

            RoomData createRoom = OtanixEnvironment.GetGame().GetRoomManager().CreateRoom(Session, "Pack " + Item.Name, "Pack comprado na loja", predesigned.RoomModel, 1, 25, 1);

            createRoom.FloorThickness = int.Parse(decoration[0]);
            createRoom.WallThickness = int.Parse(decoration[1]);
            createRoom.WallHeight = int.Parse(decoration[2]);
            createRoom.Hidewall = ((decoration[3] == "True") ? true : false);
            createRoom.Wallpaper = decoration[4];
            createRoom.Landscape = decoration[5];
            createRoom.Floor = decoration[6];
            Room newRoom = OtanixEnvironment.GetGame().GetRoomManager().LoadRoom(createRoom.Id);

            if (predesigned.FloorItems != null)
                foreach (BundleFloorMobis floorItems in predesigned.FloorItemData)
                {
                    UserItem meuItem = Session.GetHabbo().GetInventoryComponent().AddNewItem(0, floorItems.BaseItem, floorItems.ExtraData, true, false, false, Item.Name, Session.GetHabbo().Id, 0);
                    RoomItem RoomItem = new RoomItem(meuItem.Id, createRoom.Id, meuItem.BaseItem, meuItem.ExtraData, createRoom.OwnerId, floorItems.X, floorItems.Y, floorItems.Z, floorItems.Rot, newRoom, false);

                    if(newRoom.GetRoomItemHandler().SetFloorItem(Session, RoomItem, floorItems.X, floorItems.Y, floorItems.Rot, true, false, false, false, false))
                        Session.GetHabbo().GetInventoryComponent().RemoveItem(meuItem.Id, true);
                }

            if (predesigned.WallItems != null)
                foreach (var wallItems in predesigned.WallItemData)
                {
                    UserItem meuItem = Session.GetHabbo().GetInventoryComponent().AddNewItem(0, wallItems.BaseItem, wallItems.ExtraData, true, false, false, Item.Name, Session.GetHabbo().Id, 0);
                    WallCoordinate wallCoord = new WallCoordinate(wallItems.WallCoord);
                    RoomItem RoomItem = new RoomItem(meuItem.Id, createRoom.Id, meuItem.BaseItem, meuItem.ExtraData, createRoom.OwnerId, wallCoord, newRoom, false);

                    if (newRoom.GetRoomItemHandler().SetWallItem(Session, RoomItem))
                        Session.GetHabbo().GetInventoryComponent().RemoveItem(meuItem.Id, true);
                }

            if (Item.BadgeName != string.Empty)
                Session.GetHabbo().GetBadgeComponent().GiveBadge(Item.BadgeName);

            if (newRoom != null)
                Session.GetMessageHandler().enterOnRoom3(newRoom);

            Session.GetMessageHandler().GetResponse().Init(Outgoing.PurchaseOKMessageOfferData);
            Session.GetMessageHandler().GetResponse().AppendUInt(0);
            Session.GetMessageHandler().GetResponse().AppendString("LuL");
            Session.GetMessageHandler().GetResponse().AppendBoolean(false); // false = comprar, true = alquilar
            Session.GetMessageHandler().GetResponse().AppendUInt(0);
            Session.GetMessageHandler().GetResponse().AppendUInt((Item.DiamondsCost > 0) ? Item.DiamondsCost : Item.DucketsCost);
            Session.GetMessageHandler().GetResponse().AppendInt32((Item.DiamondsCost > 0) ? 105 : 0);
            Session.GetMessageHandler().GetResponse().AppendBoolean(false); // gift button
            Session.GetMessageHandler().GetResponse().AppendInt32(1);
            Session.GetMessageHandler().GetResponse().AppendString("s");
            Session.GetMessageHandler().GetResponse().AppendInt32(-1);
            Session.GetMessageHandler().GetResponse().AppendString(string.Empty);
            Session.GetMessageHandler().GetResponse().AppendUInt(1);
            Session.GetMessageHandler().GetResponse().AppendBoolean(false); // is limited.
            Session.GetMessageHandler().GetResponse().AppendInt32(0); // 0 = todos, 2 = club.
            Session.GetMessageHandler().GetResponse().AppendBoolean(false);
            Session.GetMessageHandler().SendResponse();
        }

        internal bool criarBundle(Room TargetRoom)
        {   
            // itemsAmounts = Quantidade que tem de cada mobi no quarto
            StringBuilder itemAmounts = new StringBuilder(), 
            // floorItemsData = Id do mobi, posição e extradata dos mobi de chão
            floorItemsData = new StringBuilder(),
            // wallItemsData = Id do mobi, posição e extradata dos mobi de parede
            wallItemsData = new StringBuilder(), 
            // decoration = Configurações do quarto
            decoration = new StringBuilder();

            // todos mobis de chão
            var floorItems = TargetRoom.GetRoomItemHandler().mFloorItems;
            // todos mobis de parede
            var wallItems = TargetRoom.GetRoomItemHandler().mWallItems;

            // percorre os mobis de chão e coloca o id base (items_base na db) e a quantidade que tem dele no quarto
            foreach (var roomItem in floorItems)
            {
                // pega a quantidade dele no quarto
                var itemCount = floorItems.Count(item => item.Value.BaseItem == roomItem.Value.BaseItem);

                // verifica se o id do mobi e a quantidade dele já foi adicionado na string, se não foi, adicionada
                if (!itemAmounts.ToString().Contains(roomItem.Value.BaseItem + "-" + itemCount + ";"))
                    itemAmounts.Append(roomItem.Value.BaseItem + "-" + itemCount + ";");

                // adiciona na variavel de mobis de chão este item
                floorItemsData.Append(roomItem.Value.BaseItem + "$$$$" + roomItem.Value.GetX + "$$$$" + roomItem.Value.GetY + "$$$$" + roomItem.Value.GetZ +
                    "$$$$" + roomItem.Value.Rot + "$$$$" + roomItem.Value.ExtraData + ";");
            }
            foreach (var roomItem in wallItems)
            {
                var itemCount = wallItems.Count(item => item.Value.BaseItem == roomItem.Value.BaseItem);
                if (!itemAmounts.ToString().Contains(roomItem.Value.BaseItem + "-" + itemCount + ";"))
                    itemAmounts.Append(roomItem.Value.BaseItem + "-" + itemCount + ";");

                wallItemsData.Append(roomItem.Value.BaseItem + "$$$$" + roomItem.Value.wallCoord + "$$$$" + roomItem.Value.ExtraData + ";");
            }

            // Salva as configurações do quarto na variavel
            decoration.Append(TargetRoom.RoomData.FloorThickness + ";" + TargetRoom.RoomData.WallThickness + ";" +
                TargetRoom.RoomData.WallHeight + ";" + TargetRoom.RoomData.Hidewall + ";" + TargetRoom.RoomData.Wallpaper + ";" +
                TargetRoom.RoomData.Landscape + ";" + TargetRoom.RoomData.Floor);

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO catalog_bundles(room_model,flooritems,wallitems,catalogitems,room_id,room_decoration) VALUES('" + TargetRoom.RoomData.ModelName + "', '" + floorItemsData
                    + "', '" + wallItemsData + "', '" + itemAmounts + "', " + TargetRoom.RoomId + ", '" + decoration + "');");
                var predesignedId = (uint)dbClient.insertQuery();

                OtanixEnvironment.GetGame().GetCatalog().GetPredesignedRooms().predesignedRoom.Add(predesignedId,
                    new BundleQuarto(predesignedId, (uint)TargetRoom.Id, TargetRoom.RoomData.ModelName,
                        floorItemsData.ToString().TrimEnd(';'), wallItemsData.ToString().TrimEnd(';'),
                        itemAmounts.ToString().TrimEnd(';'), decoration.ToString()));
            }

            return true;
        }
    }
}