using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces
{
    interface IWiredEffect
    {
        void Handle(RoomUser user, Team team, RoomItem item);
    }
}
