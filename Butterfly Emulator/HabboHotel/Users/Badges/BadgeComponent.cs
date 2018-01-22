using System;
using System.Collections.Generic;
using System.Linq;
using Butterfly.Messages;
using System.Collections;
using ButterStorm;
using HabboEvents;
using System.Collections.Specialized;

namespace Butterfly.HabboHotel.Users.Badges
{
    class BadgeComponent
    {
        private readonly HybridDictionary Badges;
        private readonly UInt32 UserId;

        internal void Destroy()
        {
            Badges.Clear();
        }

        internal int Count
        {
            get
            {
                return Badges.Count;
            }
        }

        internal int EquippedCount
        {
            get
            {
                return Badges.Values.Cast<Badge>().Count(Badge => Badge.Slot > 0);
            }
        }

        internal int temEmblemaEquipado(string emblema)
        {
             return Badges.Values.Cast<Badge>().Count(Badge => Badge.Slot > 0 && Badge.Code == emblema);
        }

        internal HybridDictionary BadgeList
        {
            get
            {
                return Badges;
            }
        }

        internal BadgeComponent(uint userId, List<Badge> badges)
        {
            Badges = new HybridDictionary();
            foreach (var badge in badges)
            {
                if (!Badges.Contains(badge.Code + badge.Level))
                    Badges.Add(badge.Code + badge.Level, badge);
            }

            UserId = userId;
        }

        internal Badge GetBadge(string Badge)
        {
            if (Badges.Contains(Badge))
                return (Badge)Badges[Badge];

            return null;
        }

        internal Boolean HasBadge(string Badge)
        {
            return Badges.Contains(Badge);
        }

        internal void GiveBadge(string Badge)
        {
            GiveBadge(Badge, 0);
        }

        internal void GiveBadge(string Badge, int Slot)
        {
            if (HasBadge(Badge))
            {
                Badge bdg = (Badge)Badges[Badge];
                if (bdg.needDelete)
                    bdg.needDelete = false;

                return;
            }

            string BadgeName = Badge;
            string Level = "";
            if (Badge.StartsWith("ACH_"))
            {
                BadgeName = Badge.Substring(0, Badge.Length - 1);
                Level = Badge.Substring(Badge.Length - 1, 1);
            }

            var _badge = new Badge(BadgeName, Level, Slot) { needInsert = true };
            Badges.Add(Badge, _badge);

            var Session = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if(Session != null)
                Session.SendMessage(Serialize());
        }

        internal void RemoveBadge(string Badge)
        {
            if (Badges.Contains(Badge))
            {
                Badge bdg = (Badge)Badges[Badge];
                bdg.needDelete = true;
            }
        }

        internal void UpdateBadge(string Badge)
        {
            string BadgeName = Badge.Substring(0, Badge.Length - 1);
            int BadgeLevel = int.Parse(Badge.Substring(Badge.Length - 1, 1));

            if (!HasBadge(BadgeName + (BadgeLevel - 1)))
                return;

            if (!Badges.Contains(BadgeName + (BadgeLevel - 1)))
                return;

            Badge badge = (Badge)Badges[BadgeName + (BadgeLevel - 1)];
            Badges.Remove(BadgeName + (BadgeLevel - 1));

            var _badge = new Badge(BadgeName, BadgeLevel.ToString(), badge.Slot) { needInsert = true };
            if(!Badges.Contains(Badge))
                Badges.Add(Badge, _badge);

            var Session = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if(Session != null)
                Session.SendMessage(Serialize());
        }

        internal void ResetSlots()
        {
            foreach (Badge Badge in Badges.Values)
            {
                Badge.Slot = 0;
                Badge.needInsert = true;
            }
        }

        internal ServerMessage Serialize()
        {
            var EquippedBadges = new List<Badge>();

            var Message = new ServerMessage(Outgoing.BadgesInventory);
            Message.AppendInt32(Count);

            foreach (Badge Badge in Badges.Values)
            {
                Message.AppendInt32(0);
                Message.AppendString(Badge.Code + Badge.Level);

                if (Badge.Slot > 0)
                {
                    EquippedBadges.Add(Badge);
                }
            }

            Message.AppendInt32(EquippedBadges.Count);

            foreach (var Badge in EquippedBadges)
            {
                Message.AppendInt32(Badge.Slot);
                Message.AppendString(Badge.Code + Badge.Level);
            }

            return Message;
        }
    }
}
