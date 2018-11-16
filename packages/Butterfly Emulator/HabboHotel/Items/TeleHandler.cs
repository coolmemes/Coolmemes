using System;
using Butterfly.HabboHotel.Rooms;
using ButterStorm;
using System.Data;

namespace Butterfly.HabboHotel.Items
{
    static class TeleHandler
    {
        internal static bool GetSaltaSalasRoomId(Int32 SpriteId, out UInt32 TeleId, out UInt32 RoomId)
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM items_jumping_rooms WHERE sprite_id = '" + SpriteId + "' AND room_id > 0 ORDER BY RAND() LIMIT 1");
                DataRow dRow = dbClient.getRow();
                if (dRow != null)
                {
                    TeleId = Convert.ToUInt32(dRow["item_id"].ToString());
                    RoomId = Convert.ToUInt32(dRow["room_id"].ToString());

                    return true;
                }
            }

            TeleId = 0;
            RoomId = 0;

            return false;
        }

        internal static UInt32 GetTeleRoomId(UInt32 TeleId, Room pRoom)
        {
            if (pRoom.GetRoomItemHandler().GetItem(TeleId) != null) // si está en la sala actual
                return pRoom.RoomId;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT room_id FROM items_rooms WHERE item_id = " + TeleId);
                var Row = dbClient.getRow();

                if (Row == null)
                {
                    return 0;
                }

                return Convert.ToUInt32(Row[0]);
            }
        }

        internal static bool IsTeleLinked(UInt32 teleLink, Room pRoom)
        {
            var LinkId = teleLink;
            if (LinkId == 0 || pRoom.GetRoomItemHandler()._mRemovedItems.Contains(LinkId))
                return false;

            var RoomId = GetTeleRoomId(LinkId, pRoom);
            if (RoomId == 0)
                return false;

            return true;
        }
    }
}
