using Butterfly.HabboHotel.Rooms;

namespace Uber.HabboHotel.Rooms
{
    struct InvokedChatMessage
    {
        internal RoomUser user;
        internal string message;
        internal bool shout;
        internal int color;

        public InvokedChatMessage(RoomUser user, string message, int c, bool shout)
        {
            this.user = user;
            this.message = message;
            this.shout = shout;
            this.color = c;
        }

        internal void Dispose()
        {
            this.user = null;
            this.message = null;
        }
    }
}
