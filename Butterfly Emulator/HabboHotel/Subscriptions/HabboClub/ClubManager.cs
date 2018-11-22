using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Users.UserDataManagement;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Butterfly.HabboHotel.Subscriptions.HabboClub
{
    class ClubManager
    {
        private readonly uint UserID;
        private readonly Dictionary<string, Club> clubSubscriptions;

        internal ClubManager(uint UserID, UserData userData)
        {
            this.UserID = UserID;
            clubSubscriptions = userData.clubSubscriptions;
        }

        internal void Clear()
        {
            clubSubscriptions.Clear();
        }

        internal Club GetSubscription(string SubscriptionId)
        {
            if (clubSubscriptions.ContainsKey(SubscriptionId))
            {
                return clubSubscriptions[SubscriptionId];
            }

            else
            {
                return null;
            }
        }

        internal bool UserHasSubscription(string SubscriptionId)
        {
            if (!clubSubscriptions.ContainsKey(SubscriptionId))
            {
                return false;
            }

            Club Club = clubSubscriptions[SubscriptionId];
            return Club.IsValid;
        }

        internal void AddOrExtendSubscription(GameClient Session, string SubscriptionId, int DurationSeconds, uint Cost, int MonthProgress)
        {
            SubscriptionId = SubscriptionId.ToLower();

            var clientByUserId = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserID);
            if (clubSubscriptions.ContainsKey(SubscriptionId))
            {
                Club Club = clubSubscriptions[SubscriptionId];

                if (Club.IsValid)
                {
                    Club.ExtendSubscription(DurationSeconds);
                }

                else
                {
                    Club.SetEndTime(OtanixEnvironment.GetUnixTimestamp() + DurationSeconds);
                }

                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("UPDATE user_subscriptions SET timestamp_expire = timestamp_expire + " + DurationSeconds + " WHERE user_id = " + UserID + " AND subscription_id = '" + Club.SubscriptionId + "'");

                }
                OtanixEnvironment.GetGame().GetAchievementManager().TryProgressHabboClubAchievements(clientByUserId, MonthProgress);
            }

            else
            {
                int TimestampActivated = OtanixEnvironment.GetUnixTimestamp();
                int TimestampExpire = OtanixEnvironment.GetUnixTimestamp() + DurationSeconds;

                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("INSERT INTO user_subscriptions (subscription_id, user_id, timestamp_activated, timestamp_expire) VALUES (@subscription_id, @user_id, @timestamp_activated, @timestamp_expire)");
                    dbClient.addParameter("user_id", UserID);
                    dbClient.addParameter("subscription_id", SubscriptionId);
                    dbClient.addParameter("timestamp_activated", TimestampActivated);
                    dbClient.addParameter("timestamp_expire", TimestampExpire);
                    dbClient.runQuery();

                    dbClient.runFastQuery("UPDATE users SET spent_credits = spent_credits + " + Cost + " WHERE id = " + Session.GetHabbo().Id);
                }

                clubSubscriptions.Add(SubscriptionId, new Club(SubscriptionId, TimestampActivated, TimestampExpire, false));

                Session.GetHabbo().SpentCredits += Cost;

                var fuse = new ServerMessage(Outgoing.Fuserights);
                fuse.AppendInt32(Session.GetHabbo().GetClubManager().UserHasSubscription("club_habbo") ? 2 : 1); // normal|hc|vip
                fuse.AppendUInt(Session.GetHabbo().Rank);
                fuse.AppendBoolean(Session.GetHabbo().HasFuse("fuse_ambassador")); // embajador ?
                // fuserights.AppendInt32(0); // New Identity (1 == 1 min and Alert!)
                Session.SendMessage(fuse);

                OtanixEnvironment.GetGame().GetAchievementManager().TryProgressHabboClubAchievements(clientByUserId, MonthProgress);
            }
            Session.GetHabbo().UpdateHabboClubStatus();
            Session.GetMessageHandler().ClubCenterData();
        }

        public int GetTotalMembershipLength
        {
            get
            {
                int Length = 0;

                foreach (Club Club in clubSubscriptions.Values.ToList())
                {
                    if (Club.TimestampExpire >= OtanixEnvironment.GetUnixTimestamp())
                        continue; //Skip current.

                    Length += (OtanixEnvironment.UnixTimeStampToDateTime(Club.TimestampExpire) - OtanixEnvironment.UnixTimeStampToDateTime(Club.TimestampActivated)).Days;
                }

                if (UserHasSubscription("club_habbo"))
                {
                    Club Active = clubSubscriptions.FirstOrDefault().Value;
                    if (Active != null)
                        Length += (OtanixEnvironment.UnixTimeStampToDateTime(OtanixEnvironment.GetUnixTimestamp()) - OtanixEnvironment.UnixTimeStampToDateTime(Active.TimestampActivated)).Days;
                }

                return Length;
            }
        }
    }
}
