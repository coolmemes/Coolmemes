using Butterfly.HabboHotel.GameClients;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Misc
{
    class Diamonds
    {
        internal static void GiveCycleDiamonds(GameClient Session)
        {
            if (EmuSettings.DIAMONDS_ENABLED == false)
                return;

            if ((OtanixEnvironment.GetUnixTimestamp() - Session.GetHabbo().DiamondsCycleUpdate) > EmuSettings.DIAMONDS_MINUTES * 60)
            {
                int DiamondsAmount = (int)(Session.GetHabbo().IsPremium() ? EmuSettings.DIAMONDS_AMOUNT * 2 : EmuSettings.DIAMONDS_AMOUNT);

                Session.GetHabbo().DiamondsCycleUpdate = OtanixEnvironment.GetUnixTimestamp();
                Session.GetHabbo().GiveUserDiamonds(DiamondsAmount);
            }
        }
    }
}
