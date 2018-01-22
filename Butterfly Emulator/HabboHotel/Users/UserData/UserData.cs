using System.Collections.Generic;
using Butterfly.HabboHotel.Users.Inventory;
using Butterfly.HabboHotel.Users.Messenger;
using Butterfly.HabboHotel.Users.Relationships;

namespace Butterfly.HabboHotel.Users.UserDataManagement
{
    class UserData
    {
        internal List<uint> favouritedRooms;
        internal List<AvatarEffect> effects;
        internal Dictionary<uint, MessengerBuddy> friends;
        internal Dictionary<uint, MessengerRequest> requests;
        internal Habbo user;

        public UserData(List<uint> favouritedRooms, List<AvatarEffect> effects, Dictionary<uint, MessengerBuddy> friends, Dictionary<uint, MessengerRequest> requests, Habbo user)
        {
            this.favouritedRooms = favouritedRooms;
            this.effects = effects;
            this.friends = friends;
            this.requests = requests;
            this.user = user;
        }
    }
}
