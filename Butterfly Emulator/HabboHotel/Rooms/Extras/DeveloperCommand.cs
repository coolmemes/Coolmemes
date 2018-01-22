using Butterfly.HabboHotel.Items;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Extras
{
    class DeveloperCommand
    {
        internal static bool CheckDeveloper(RoomUser User, int pX, int pY, Room Room)
        {
            if (User.DeveloperState == 1)
            {
                User.DeveloperX = pX;
                User.DeveloperY = pY;
                User.DeveloperState = 2;
                User.GetClient().SendNotif("Baldosa copiada con éxito.");

                return true;
            }
            else if (User.DeveloperState == 3)
            {
                User.DeveloperState = 0;

                if (Room.GetGameMap().TileContainsItems(new Point(pX, pY)))
                {
                    User.GetClient().SendNotif("Para poder copiar items en la baldosa debe estar vacía de items.");
                    return true;
                }

                List<RoomItem> items = Room.GetGameMap().GetCoordinatedItems(new Point(User.DeveloperX, User.DeveloperY)).OrderBy(i => i.TotalHeight).ToList();
                if (AreValidItems(items, User))
                {
                    foreach (RoomItem item in items)
                    {
                        uint Id = 0;

                        using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("INSERT INTO items (base_id) VALUES (" + item.BaseItem + ")");
                            Id = (uint)dbClient.insertQuery();

                            if (!string.IsNullOrEmpty(item.ExtraData))
                            {
                                dbClient.setQuery("INSERT INTO items_extradata VALUES (" + Id + ",@extradata)");
                                dbClient.addParameter("extradata", item.ExtraData);
                                dbClient.runQuery();
                            }
                        }

                        RoomItem newItem = new RoomItem(Id, Room.RoomId, item.BaseItem, item.ExtraData, item.OwnerId, pX, pY, item.GetZ, item.Rot, Room, false);
                        Room.GetRoomItemHandler().SetFloorItem(User.GetClient(), newItem, pX, pY, item.Rot, true, false, true, false, false, true);
                    }
                }
            }

            return false;
        }

        private static bool AreValidItems(List<RoomItem> items, RoomUser User)
        {
            if (items.Count <= 0)
                return false;

            bool ContainsWired = false;

            foreach (RoomItem item in items)
            {
                if (item.GetBaseItem().LimitedStack > 0)
                {
                    User.GetClient().SendNotif("No puedes clonar furnis LTD.");
                    return false;
                }
                else if(Wired.WiredUtillity.TypeIsWired(item.GetBaseItem().InteractionType))
                {
                    ContainsWired = true;
                }
                else if (item.IsPremiumItem)
                {
                    return false;
                }
            }

            if(ContainsWired)
            {
                User.GetClient().SendNotif("Recuerda que la configuración de los wired no se aplicará.");
            }

            return true;
        }
    }
}
