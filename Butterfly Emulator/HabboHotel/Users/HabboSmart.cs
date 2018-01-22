using Butterfly.HabboHotel.Achievements;
using Butterfly.HabboHotel.Users.Badges;
using Butterfly.HabboHotel.Users.Relationships;
using ButterStorm;
using ButterStorm.HabboHotel.Users.Inventory;
using System;
using System.Collections.Generic;
using System.Data;

namespace Butterfly.HabboHotel.Users
{
    partial class Habbo
    {
        internal void LoadAchievements()
        {
            Achievements = new Dictionary<string, UserAchievement>();

            DataTable dTable;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM user_achievements WHERE user_id = " + Id);
                dTable = dbClient.getTable();
            }

            string achievementId;
            uint achievementLevel;
            int achievementProgress;

            foreach (DataRow dRow in dTable.Rows)
            {
                achievementId = (string)dRow["achievement"];
                achievementLevel = Convert.ToUInt32(dRow["level"]);
                achievementProgress = (int)dRow["progress"];

                UserAchievement achievement = new UserAchievement(achievementId, achievementLevel, achievementProgress);
                Achievements.Add(achievementId, achievement);
            }

            AchievementsLoaded = true;
        }
        internal void _LoadBadgeComponent()
        {
            DataTable dTable;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM user_badges WHERE user_id = " + Id);
                dTable = dbClient.getTable();
            }

            List<Badge> badges = new List<Badge>();
            foreach (DataRow dRow in dTable.Rows)
            {
                badges.Add(new Badge((string)dRow["badge_id"], (string)dRow["badge_level"], (int)dRow["badge_slot"]));
            }

            BadgeComponent = new BadgeComponent(Id, badges);
            BadgeComponentLoaded = true;
        }

        internal void saveBadges()
        {
            if (BadgeComponent != null && BadgeComponent.BadgeList != null && BadgeComponent.BadgeList.Count > 0)
            {
                foreach (Badge badge in BadgeComponent.BadgeList.Values)
                {
                    if (badge.needInsert && !badge.needDelete)
                    {
                        using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("REPLACE INTO user_badges VALUES ('" + Id + "',@badge,'" + badge.Level + "','" + badge.Slot + "')");
                            dbClient.addParameter("badge", badge.Code);
                            dbClient.runQuery();
                        }
                        badge.needInsert = false;
                    }

                    if(badge.needDelete)
                    {
                        using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("DELETE FROM user_badges WHERE user_id = @userid AND badge_id = @badge");
                            dbClient.addParameter("userid", Id);
                            dbClient.addParameter("badge", badge.Code);
                            dbClient.runQuery();
                        }
                        badge.needDelete = false;
                    }
                }
            }
        }

        internal void _LoadQuests()
        {
            DataTable dTable = null;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM user_quests WHERE user_id = " + Id);
                dTable = dbClient.getTable();
            }

            foreach (DataRow dRow in dTable.Rows)
            {
                quests.Add(Convert.ToUInt32(dRow["quest_id"]), (int)dRow["progress"]);
            }

            QuestsLoaded = true;
        }

        internal void _LoadWardrobe()
        {
            DataTable dTable = null;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT slot_id, look, gender FROM user_wardrobe WHERE user_id = " + Id);
                dTable = dbClient.getTable();
            }

            int slotId;
            string look;
            string gender;
            foreach (DataRow dRow in dTable.Rows)
            {
                slotId = Convert.ToInt32(dRow["slot_id"]);
                look = (string)dRow["look"];
                gender = dRow["gender"].ToString().ToUpper();

                if(!wardrobes.ContainsKey(slotId))
                    wardrobes.Add(slotId, new Wardrobe(slotId, look, gender));
            }

            WardrobeLoaded = true;
        }

        internal void saveWardrobe()
        {
            if (wardrobes != null && wardrobes.Count > 0)
            {
                foreach (var wardrobe in wardrobes.Values)
                {
                    if (wardrobe.needInsert)
                    {
                        using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("REPLACE INTO user_wardrobe (user_id,slot_id,look,gender) VALUES (" + Id + "," + wardrobe.slotId + ",@look,@gender)");
                            dbClient.addParameter("look", wardrobe.look);
                            dbClient.addParameter("gender", wardrobe.gender);
                            dbClient.runQuery();
                        }
                        wardrobe.needInsert = false;
                    }
                }
            }
        }

        internal void _LoadRelationships()
        {
            DataTable dTable;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT member_id, relation_type FROM user_relationships WHERE user_id = " + Id);
                dTable = dbClient.getTable();
            }

            uint memberid;
            int relationtype;
            var myrelations = new List<Relationship>();
            foreach (DataRow dRow in dTable.Rows)
            {
                memberid = Convert.ToUInt32(dRow["member_id"]);
                relationtype = (int)dRow["relation_type"];
                myrelations.Add(new Relationship(memberid, relationtype));
            }

            RelationshipComposer = new RelationshipComposer(myrelations);
            RelationsLoaded = true;
        }

        internal void _LoadMyGroups()
        {
            DataTable dTable;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT groupid FROM groups_users WHERE userid = " + Id + " AND acepted = '1'");
                dTable = dbClient.getTable();
            }

            foreach (DataRow dRow in dTable.Rows)
            {
                mygroups.Add(Convert.ToUInt32(dRow["groupid"]));
            }

            LoadedMyGroups = true;
        }

        internal void _LoadFriendsCount()
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT COUNT(*) FROM messenger_friendships WHERE " + (((Id % 2) == 0) ? "receiver" : "sender") + " = " + Id);
                friendsCount = dbClient.getInteger();
            }
        }

  
    }
}
