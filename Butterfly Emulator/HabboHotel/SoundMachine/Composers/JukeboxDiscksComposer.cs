using System.Collections;
using System.Collections.Generic;
using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using HabboEvents;
using System.Collections.Specialized;

namespace Butterfly.HabboHotel.SoundMachine
{
    class JukeboxDiscksComposer
    {
        public static ServerMessage Compose(int PlaylistCapacity, List<SongInstance> Playlist)
        {
            var Message = new ServerMessage(Outgoing.SerializeJukeSongs);
            Message.AppendInt32(PlaylistCapacity);
            Message.AppendInt32(Playlist.Count);

            foreach (var Song in Playlist)
            {
                Message.AppendInt32(Song.DiskItem.itemID);
                Message.AppendInt32(Song.SongData.Id);
            }

            return Message;
        }

        public static ServerMessage Compose(List<SongData> Songs)
        {
            var Message = new ServerMessage(Outgoing.ListenPreviewSong);
            Message.AppendInt32(Songs.Count);

            foreach (var Song in Songs)
            {
                Message.AppendInt32(Song.Id);
                Message.AppendString(Song.Name);
                Message.AppendString(Song.Name.Replace("_", " "));
                Message.AppendString(Song.Sound);
                Message.AppendInt32(Song.CreatorId);
                Message.AppendString(Song.Artist);
            }

            return Message;
        }

        public static ServerMessage ComposePlayingComposer(int SongId, int PlaylistItemNumber, int SyncTimestampMs)
        {
            var Message = new ServerMessage(Outgoing.PlayStopMusic);

            if (SongId == 0)
            {
                Message.AppendInt32(-1);
                Message.AppendInt32(-1);
                Message.AppendInt32(-1);
                Message.AppendInt32(-1);
                Message.AppendInt32(0);
            }
            else
            {
                Message.AppendInt32(SongId);
                Message.AppendInt32(PlaylistItemNumber);
                Message.AppendInt32(SongId);
                Message.AppendInt32(0);
                Message.AppendInt32(SyncTimestampMs);
            }

            return Message;
        }

        public static ServerMessage SerializeSongInventory(HybridDictionary songs)
        {
            var Message = new ServerMessage(Outgoing.SerializeInvSongs);
            Message.AppendInt32(songs.Count);
            foreach (UserItem userItem in songs.Values)
            {
                var songId = TextHandling.Parse(userItem.ExtraData);
                Message.AppendUInt(userItem.Id);
                Message.AppendInt32(songId);
            }
            return Message;
        }
    }
}
