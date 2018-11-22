using Butterfly.HabboHotel.GameClients;
using ButterStorm;

namespace Otanix.HabboHotel.Misc
{
    class Club
    {
        internal static void UpdateClubCycleExpiration(GameClient Session)
        {
            int seconds = 0;

            if (Session.GetHabbo().GetClubManager().UserHasSubscription("club_habbo"))
            {
                if (Session.GetHabbo().GetClubManager().GetSubscription("club_habbo").DaysLeft >= 1)
                    seconds = 86400;

                else if (Session.GetHabbo().GetClubManager().GetSubscription("club_habbo").HoursLeft >= 1 && Session.GetHabbo().GetClubManager().GetSubscription("club_habbo").DaysLeft < 1)
                    seconds = 3600;

                else if (Session.GetHabbo().GetClubManager().GetSubscription("club_habbo").MinutesLeft >= 1 && Session.GetHabbo().GetClubManager().GetSubscription("club_habbo").HoursLeft < 1)
                    seconds = 60;
            }

            if ((OtanixEnvironment.GetUnixTimestamp() - Session.GetHabbo().ClubExpirationCycleUpdate) > seconds || !Session.GetHabbo().GetClubManager().UserHasSubscription("club_habbo"))
            {
                Session.GetHabbo().UpdateHabboClubStatus();
            }
        }
    }
}
