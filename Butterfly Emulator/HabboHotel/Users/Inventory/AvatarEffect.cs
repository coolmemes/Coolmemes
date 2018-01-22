using System;
using ButterStorm;

namespace Butterfly.HabboHotel.Users.Inventory
{
    class AvatarEffect
    {
        internal int EffectId;
        internal int TotalDuration;
        internal bool Activated;
        internal double StampActivated;
        internal int EffectCount;

        internal int TimeLeft
        {
            get
            {
                if (!Activated)
                {
                    return -1;
                }

                var diff = OtanixEnvironment.GetUnixTimestamp() - StampActivated;

                if (diff >= TotalDuration)
                {
                    return 0;
                }

                return (int)(TotalDuration - diff);
            }
        }

        internal Boolean HasExpired
        {
            get
            {
                if (TimeLeft == -1)
                {
                    return false;
                }

                if (TimeLeft <= 0)
                {
                    return true;
                }

                return false;
            }
        }

        internal AvatarEffect(int EffectId, int TotalDuration, bool Activated, double ActivateTimestamp, int EffectCount)
        {
            this.EffectId = EffectId;
            this.TotalDuration = TotalDuration;
            this.Activated = Activated;
            this.StampActivated = ActivateTimestamp;
            this.EffectCount = EffectCount;
        }

        internal void Activate()
        {
            Activated = true;
            StampActivated = OtanixEnvironment.GetUnixTimestamp();
        }
    }
}
