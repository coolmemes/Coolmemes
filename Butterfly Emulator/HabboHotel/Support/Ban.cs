using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Support
{
    class Ban
    {
        internal ModerationBanType Type;
        internal string Variable;
        internal string ReasonMessage;
        internal double Expire;
        internal bool Expired
        {
            get
            {
                return (OtanixEnvironment.GetUnixTimestamp() >= this.Expire);
            }
        }

        internal Ban(ModerationBanType Type, string Variable, string ReasonMessage, double Expire)
        {
            this.Type = Type;
            this.Variable = Variable;
            this.ReasonMessage = ReasonMessage;
            this.Expire = Expire;
        }
    }
}
