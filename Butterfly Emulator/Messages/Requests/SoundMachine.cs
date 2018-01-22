using System.Collections.Generic;
using System.Linq;
using Butterfly.HabboHotel.SoundMachine;
using Butterfly.HabboHotel.Items;

namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        public void GetMusicData()
        {
            try
            {
                var Amount = Request.PopWiredInt32();
                var Songs = new List<SongData>();

                for (var i = 0; i < Amount; i++)
                {
                    var Song = SongManager.GetSong(Request.PopWiredInt32());
                    if (Song == null)
                        continue;
                    Songs.Add(Song);
                }
                Session.SendMessage(JukeboxDiscksComposer.Compose(Songs));

                Songs.Clear();
            }
            catch { }
        }

        public void LoadInvSongs()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            Session.SendMessage(JukeboxDiscksComposer.SerializeSongInventory(Session.GetHabbo().GetInventoryComponent().songDisks));
        }

        public void LoadJukeSongs()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;

            var currentRoom = Session.GetHabbo().CurrentRoom;

            if (!currentRoom.CheckRights(Session, true) || !currentRoom.GotMusicController())
                return;

            var musicController = currentRoom.GetRoomMusicController();
            Session.SendMessage(JukeboxDiscksComposer.Compose(musicController.PlaylistCapacity, musicController.Playlist.Values.ToList()));
        }

        public void AddNewCdToJuke()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;

            var currentRoom = Session.GetHabbo().CurrentRoom;

            if (!currentRoom.CheckRights(Session, true))
                return;

            var musicController = currentRoom.GetRoomMusicController();
            if (musicController.PlaylistSize >= musicController.PlaylistCapacity)
                return;

            var itemId = Request.PopWiredInt32();
            var item = Session.GetHabbo().GetInventoryComponent().GetItem((uint)itemId);
            if (item == null || item.mBaseItem.InteractionType != InteractionType.musicdisc)
                return;

            var sitem = new SongItem(item);

            var NewOrder = musicController.AddDisk(sitem);
            if (NewOrder < 0)
                return;

            sitem.SaveToDatabase((int)currentRoom.RoomId);
            Session.GetHabbo().GetInventoryComponent().RemoveItem((uint)itemId, true);
            Session.SendMessage(JukeboxDiscksComposer.SerializeSongInventory(Session.GetHabbo().GetInventoryComponent().songDisks));
            Session.SendMessage(JukeboxDiscksComposer.Compose(musicController.PlaylistCapacity, musicController.Playlist.Values.ToList()));
        }

        public void RemoveCdToJuke()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;

            var currentRoom = Session.GetHabbo().CurrentRoom;

            if (!currentRoom.CheckRights(Session, true) || !currentRoom.GotMusicController())
                return;

            var musicController = currentRoom.GetRoomMusicController();

            var item = musicController.RemoveDisk(Request.PopWiredInt32());
            if (item == null)
                return;

            item.RemoveFromDatabase();
            Session.GetHabbo().GetInventoryComponent().AddNewItem((uint)item.itemID, item.baseItem.ItemId, item.songID.ToString(), true, true, false, item.baseItem.Name, Session.GetHabbo().Id, (uint)item.songID);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            Session.SendMessage(JukeboxDiscksComposer.SerializeSongInventory(Session.GetHabbo().GetInventoryComponent().songDisks));
            Session.SendMessage(JukeboxDiscksComposer.Compose(musicController.PlaylistCapacity, musicController.Playlist.Values.ToList()));
        }
    }
}
