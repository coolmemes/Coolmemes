using Butterfly.HabboHotel.Items;
using ButterStorm;

namespace Butterfly.HabboHotel.SoundMachine
{
    class SongItem
    {
        public int itemID;
        public int songID;
        public Item baseItem;

        public SongItem(int itemID, int songID, int baseItem)
        {
            this.itemID = itemID;
            this.songID = songID;
            this.baseItem = OtanixEnvironment.GetGame().GetItemManager().GetItem((uint)baseItem);
        }

        public SongItem(UserItem item)
        {
            itemID = (int)item.Id;
            songID = TextHandling.Parse(item.ExtraData);
            baseItem = item.mBaseItem;
        }

        public void SaveToDatabase(int roomID)
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("INSERT INTO items_rooms_songs VALUES (" + itemID + "," + roomID + "," + songID + ")");
                dbClient.runFastQuery("DELETE FROM items WHERE item_id = '" + itemID + "'");
            }
        }

        public void RemoveFromDatabase()
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM items_rooms_songs WHERE itemid = " + itemID);
                dbClient.runFastQuery("INSERT INTO items (item_id, base_id) VALUES ('" + itemID + "','" + baseItem.ItemId + "')");
            }
        }
    }
}
