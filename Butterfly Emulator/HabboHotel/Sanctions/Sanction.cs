using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otanix.HabboHotel.Sanctions
{
    class Sanction
    {
        internal uint UserID;
        internal int ReasonId;
        internal int StartTime;
        internal int RemainingTime;
        internal string NextSanction;

        public Sanction(uint UserID, int ReasonId, int StartTime, int RemainingTime, string NextSanction)
        {
            this.UserID = UserID;
            this.ReasonId = ReasonId;
            this.StartTime = StartTime;
            this.RemainingTime = RemainingTime;
            this.NextSanction = NextSanction;
        }

        internal int RemainingDaysInHours
        {
            get
            {
                double left = RemainingTime - OtanixEnvironment.GetUnixTimestamp();
                int hours = (int)Math.Ceiling(left / 3700);
                return hours;
            }
        }
    }
}
