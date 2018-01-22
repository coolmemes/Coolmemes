namespace Butterfly.HabboHotel.Achievements
{
    class UserAchievement
    {
        public string AchievementId;
        public uint Level;
        public int Progress;

        public UserAchievement(string AchievementId, uint level, int progress)
        {
            this.AchievementId = AchievementId;
            this.Level = level;
            this.Progress = progress;
        }
    }
}
