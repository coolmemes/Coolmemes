using System.Collections.Generic;
using Butterfly.HabboHotel.Subscriptions.HabboClub;
using Butterfly.HabboHotel.Users.Badges;
using Butterfly.HabboHotel.Users.Inventory;
using Butterfly.HabboHotel.Users.Messenger;
using Otanix.HabboHotel.Sanctions;

namespace Butterfly.HabboHotel.Users.UserDataManagement
{
    class UserData
    {
        internal List<uint> favouritedRooms;
        internal List<AvatarEffect> effects;
        internal Dictionary<uint, MessengerBuddy> friends;
        internal Dictionary<uint, MessengerRequest> requests;
        internal Habbo user;
        internal Dictionary<string, Club> clubSubscriptions;
        internal Dictionary<uint, Sanction> sanctions;

        public UserData(List<uint> favouritedRooms, List<AvatarEffect> effects, Dictionary<uint, MessengerBuddy> friends, Dictionary<uint, MessengerRequest> requests, Habbo user, Dictionary<string, Club> clubSubscriptions, Dictionary<uint, Sanction> sanctions)
        {
            this.favouritedRooms = favouritedRooms;
            this.effects = effects;
            this.friends = friends;
            this.requests = requests;
            this.user = user;
            this.clubSubscriptions = clubSubscriptions;
            this.sanctions = sanctions;
        }
    }
}
