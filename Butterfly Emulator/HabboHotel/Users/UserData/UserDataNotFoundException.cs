using System;

namespace Butterfly.HabboHotel.Users.UserDataManagement
{
    class UserDataNotFoundException : Exception
    {
        public UserDataNotFoundException(string reason)
            : base(reason)
        { }
    }
}
