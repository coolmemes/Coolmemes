namespace Butterfly.HabboHotel.SoundMachine
{
    class SongData
    {
        private readonly int mId;
        private readonly string mName;
        private readonly string mSound;
        private readonly int mCreatorId;
        private readonly string mArtist;

        public int Id
        {
            get
            {
                return mId;
            }
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public string Sound
        {
            get
            {
                return mSound;
            }
        }

        public int CreatorId
        {
            get
            {
                return mCreatorId;
            }
        }

        public string Artist
        {
            get
            {
                return mArtist;
            }
        }

        public SongData(int _Id, string _Name, string _Sound, int _CreatorId, string _Artist)
        {
            mId = _Id;
            mName = _Name;
            mSound = _Sound;
            mCreatorId = _CreatorId;
            mArtist = _Artist;
        }
    }
}
