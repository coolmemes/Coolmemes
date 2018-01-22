namespace Butterfly.HabboHotel.Users.Relationships
{
    class Relationship
    {
        internal uint MemberId;
        internal int RelationType;

        internal Relationship(uint _memberId, int _relationType)
        {
            MemberId = _memberId;
            RelationType = _relationType;
        }
    }
}
