
namespace Butterfly.HabboHotel.Users.Badges
{
    class Badge
    {
        internal string Code;
        internal string Level;
        internal int Slot;
        internal bool needInsert;
        internal bool needDelete;

        internal Badge(string Code, string Level, int Slot)
        {
            this.Code = Code;
            this.Level = Level;
            this.Slot = Slot;
            this.needInsert = false;
            this.needDelete = false;
        }
    }
}
