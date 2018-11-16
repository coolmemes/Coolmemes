using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Mutes
{
    class MuteUser
    {
        internal uint UserId;
        internal double ExpireTime;

        internal MuteUser(uint userId, double expireTime)
        {
            this.UserId = userId;
            this.ExpireTime = expireTime;
        }
    }
}
