using Database_Manager.Database.Session_Details.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace ButterStorm.HabboHotel.Items
{
    class YoutubeManager
    {
        public Dictionary<int, YoutubeTV> Videos;

        public void Initialize(IQueryAdapter dbClient)
        {
            Videos = new Dictionary<int, YoutubeTV>();

            dbClient.setQuery("SELECT * FROM youtube_videos");
            var dTable = dbClient.getTable();

            foreach (DataRow dRow in dTable.Rows)
            {
                var tv = new YoutubeTV(dRow);
                Videos.Add(tv.item_id, tv);
            }
        }
    }

    class YoutubeTV
    {
        internal int item_id;
        internal string favVideo;
        internal string loadStringVideos;
        internal Dictionary<string, string> Videos;

        internal YoutubeTV(DataRow dRow)
        {
            item_id = (int)dRow["item_id"];
            favVideo = (string)dRow["main_video"];
            loadStringVideos = (string)dRow["videos"];
            Videos = new Dictionary<string, string>();
            foreach (var str in loadStringVideos.Split(';'))
            {
                Videos.Add(str.Split('>')[0], str);
            }
        }
    }
}
