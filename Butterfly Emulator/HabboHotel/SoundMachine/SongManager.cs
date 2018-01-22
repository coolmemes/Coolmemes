using System;
using System.Collections.Generic;
using System.Data;
using ButterStorm;

namespace Butterfly.HabboHotel.SoundMachine
{
    class SongManager
    {
        private const int CACHE_LIFETIME = 180;

        private static Dictionary<int, SongData> songs;

        public static void Initialize()
        {
            songs = new Dictionary<int, SongData>();
            new Dictionary<int, double>();

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM songs");
                var dTable = dbClient.getTable();

                foreach (DataRow dRow in dTable.Rows)
                {
                    var song = GetSongFromDataRow(dRow);
                    songs.Add(song.Id, song);
                }
            }
        }

        public static SongData GetSongFromDataRow(DataRow dRow)
        {
            return new SongData(Convert.ToInt32(dRow["id"]), (string)dRow["name"], (string)dRow["song_data"],
                Convert.ToInt32(dRow["creator_id"]), (string)dRow["artist"]);
        }

        public static SongData GetSong(int SongId)
        {
            return songs[SongId];
        }
    }
}
