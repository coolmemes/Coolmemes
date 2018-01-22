namespace Butterfly.HabboHotel.SoundMachine
{
    class SongInstance
    {
        private readonly SongItem mDiskItem;
        private readonly SongData mSongData;

        public SongItem DiskItem
        {
            get
            {
                return mDiskItem;
            }
        }

        public SongData SongData
        {
            get
            {
                return mSongData;
            }
        }

        public SongInstance(SongItem Item, SongData SongData)
        {
            mDiskItem = Item;
            mSongData = SongData;
        }
    }
}
