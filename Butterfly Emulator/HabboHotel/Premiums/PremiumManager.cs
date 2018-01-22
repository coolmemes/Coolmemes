using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Premiums.Users;
using Butterfly.HabboHotel.Users;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Premiums
{
    class PremiumManager
    {
        public static Premium LoadPremiumData(uint UserId)
        {
            DataRow dRow = null;

            using(IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM user_premiums WHERE user_id = " + UserId);
                dRow = dbClient.getRow();
            }

            if (dRow == null)
                return null;

            double unixStart = (double)dRow["unix_start"];
            double unixEnd = (double)dRow["unix_end"];
            uint maxItems = Convert.ToUInt32(dRow["max_items"]);

            if (SubscriptionEnds(unixEnd, UserId))
                return null;

            return new Premium(UserId, unixStart, unixEnd, maxItems);
        }

        public static ServerMessage SerializePremiumItemsCount(Habbo User)
        {
            ServerMessage Message = new ServerMessage(Outgoing.LoadPremiumItemsCount);
            Message.AppendUInt(User.IsPremium() ? User.GetPremiumManager().GetActualItems() : 0); // Items Alquilados
            return Message;
        }

        public static bool UserIsSubscribed(uint UserId)
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT unix_end FROM user_premiums WHERE user_id = " + UserId);
                int PremiumEnds = dbClient.getInteger();

                if (PremiumEnds == 0 || SubscriptionEnds(PremiumEnds, UserId))
                    return false;
            }

            return true;
        }

        private static bool SubscriptionEnds(double PremiumEnds, uint UserId)
        {
            if (PremiumEnds < OtanixEnvironment.GetUnixTimestamp())
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("DELETE FROM items_premium WHERE user_id = " + UserId);
                    dbClient.runFastQuery("DELETE FROM user_premiums WHERE user_id = " + UserId);
                }

                return true;
            }

            return false;
        }
    }
}
