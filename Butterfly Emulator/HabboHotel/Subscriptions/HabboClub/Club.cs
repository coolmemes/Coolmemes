using ButterStorm;
using System;

namespace Butterfly.HabboHotel.Subscriptions.HabboClub
{
    class Club
    {
        internal string SubscriptionId;
        internal int TimestampActivated;
        internal int TimestampExpire;
        internal bool ReceivedPay;

        internal Club(string SubscriptionId, int TimestampActivated, int TimestampExpire, bool ReceivedPay)
        {
            this.SubscriptionId = SubscriptionId;
            this.TimestampActivated = TimestampActivated;
            this.TimestampExpire = TimestampExpire;
            this.ReceivedPay = ReceivedPay;
        }

        internal bool IsValid
        {
            get
            {
                return TimestampExpire > OtanixEnvironment.GetUnixTimestamp();
            }
        }


        internal int StreakDurationInDays
        {
            get
            {
                return (OtanixEnvironment.UnixTimeStampToDateTime(OtanixEnvironment.GetUnixTimestamp()) - OtanixEnvironment.UnixTimeStampToDateTime(TimestampActivated)).Days;
            }
        }

        internal int DaysLeft
        {
            get
            {
                return (OtanixEnvironment.UnixTimeStampToDateTime(TimestampExpire) - OtanixEnvironment.UnixTimeStampToDateTime(OtanixEnvironment.GetUnixTimestamp())).Days;
            }
        }

        internal int HoursLeft
        {
            get
            {
                return (OtanixEnvironment.UnixTimeStampToDateTime(TimestampExpire) - OtanixEnvironment.UnixTimeStampToDateTime(OtanixEnvironment.GetUnixTimestamp())).Hours;
            }
        }

        internal int MinutesLeft
        {
            get
            {
                return (OtanixEnvironment.UnixTimeStampToDateTime(TimestampExpire) - OtanixEnvironment.UnixTimeStampToDateTime(OtanixEnvironment.GetUnixTimestamp())).Minutes;
            }
        }

        internal int SecondsLeft
        {
            get
            {
                return (OtanixEnvironment.UnixTimeStampToDateTime(TimestampExpire) - OtanixEnvironment.UnixTimeStampToDateTime(OtanixEnvironment.GetUnixTimestamp())).Seconds;
            }
        }

        internal int TimeLeft
        {
            get
            {
                double left = TimestampExpire - OtanixEnvironment.GetUnixTimestamp();
                int TotalTimeLeft = (int)Math.Ceiling(left / 60);

                return TotalTimeLeft;
            }
        }

        internal int TimeLeftInHours
        {
            get
            {
                double left = TimestampExpire - OtanixEnvironment.GetUnixTimestamp();
                int TotalHoursLeft = (int)Math.Ceiling(left / 3600);

                return TotalHoursLeft;
            }
        }

        internal int HabboClubBonusStreak
        {
            get
            {
                int credits = 0;

                if (StreakDurationInDays == 7 && StreakDurationInDays < 30)
                    credits = 5;

                else if (StreakDurationInDays == 30 && StreakDurationInDays < 60)
                    credits = 10;

                else if (StreakDurationInDays == 60 && StreakDurationInDays < 90)
                    credits = 15;

                else if (StreakDurationInDays == 90 && StreakDurationInDays < 180)
                    credits = 20;

                else if (StreakDurationInDays == 180 && StreakDurationInDays < 365)
                    credits = 25;

                else if (StreakDurationInDays >= 365)
                    credits = 30;

                else
                    credits = 0;

                return credits;
            }
        }

        internal int TotalSpent(uint SpentCredits)
        {
            int percentage = 10;
            var total = (double)(HabboClubBonusStreak + SpentCredits) / percentage;

            return total > 0 ? (int)Math.Round(total, percentage) : 0;
        }

        internal void SetEndTime(int time)
        {
            TimestampExpire = time;
        }

        internal void ExtendSubscription(int Time)
        {
            try
            {
                TimestampExpire = TimestampExpire + Time;
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
