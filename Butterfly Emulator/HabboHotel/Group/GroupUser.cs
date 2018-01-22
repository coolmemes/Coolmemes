namespace Butterfly.HabboHotel.Groups
{
    class GroupUser
    {
        internal uint UserId;
        internal uint GroupRank;
        internal string DateJoined;

        internal GroupUser(uint userid, uint rank, string datejoined)
        {
            UserId = userid;
            GroupRank = rank;
            DateJoined = datejoined;
        }
    }
}
