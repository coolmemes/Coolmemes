using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
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

namespace Butterfly.HabboHotel.Users.Talents
{
    class TalentManager
    {
        internal Dictionary<int, TalentTrackLevel> citizenshipTalents;
        internal Dictionary<int, TalentTrackLevel> helperTalents;

        internal TalentManager()
        {
            this.citizenshipTalents = new Dictionary<int, TalentTrackLevel>();
            this.helperTalents = new Dictionary<int, TalentTrackLevel>();
        }

        internal void Initialize(IQueryAdapter dbClient)
        {
            dbClient.setQuery("SELECT * FROM talents");
            DataTable dTable = dbClient.getTable();
            foreach (DataRow dRow in dTable.Rows)
            {
                string talent_type = (string)dRow["talent_type"];
                int level = (int)dRow["level"];
                string ach_ids = (string)dRow["ach_ids"];
                string gifts = (string)dRow["gifts"];

                if (talent_type == "citizenship")
                {
                    if (!this.citizenshipTalents.ContainsKey(level))
                        this.citizenshipTalents.Add(level, new TalentTrackLevel(ach_ids, gifts));
                }
                else if (talent_type == "helper")
                {
                    if (!this.helperTalents.ContainsKey(level))
                        this.helperTalents.Add(level, new TalentTrackLevel(ach_ids, gifts));
                }
            }
        }

        internal ServerMessage Serialize(GameClient Session, string Type)
        {
            ServerMessage Message = new ServerMessage(Outgoing.SerializeTalents);
            Message.AppendString(Type);
            if (Type == "citizenship")
            {
                Message.AppendInt32(this.citizenshipTalents.Count);
                for (int i = 0; i < this.citizenshipTalents.Count;i++ )
                {
                    int ValidLevel = GetTalentValue(Session.GetHabbo().CitizenshipLevel, i);

                    Message.AppendInt32(i);
                    Message.AppendInt32(ValidLevel); // enabled (0 = null, 1 = enabled, 2 = passed) ??
                    this.citizenshipTalents[i].Serialize(Message, Session, (ValidLevel > 0));
                }
            }
            else if (Type == "helper")
            {
                Message.AppendInt32(this.helperTalents.Count);
                for (int i = 0; i < this.helperTalents.Count; i++)
                {
                    int ValidLevel = GetTalentValue(Session.GetHabbo().HelperLevel, i);

                    Message.AppendInt32(i);
                    Message.AppendInt32(ValidLevel); // enabled (0 = null, 1 = enabled, 2 = passed) ??
                    this.helperTalents[i].Serialize(Message, Session, (ValidLevel > 0));
                }
            }

            return Message;
        }

        internal Int32 GetTalentValue(Int32 Level, Int32 ActualLevel)
        {
            if (Level < ActualLevel)
                return 0;
            else if (Level == ActualLevel)
                return 1;

            return 2;
        }

        internal void UpdateTalentTravel(GameClient Session, string Type)
        {
            if (Type == "citizenship")
            {
                if (Session.GetHabbo().CitizenshipLevel < 4) // citizenship
                {
                    TalentTrackLevel talentTrackLevel = this.citizenshipTalents[Session.GetHabbo().CitizenshipLevel];
                    foreach (TalentTrackSubLevel talenttracksublevel in talentTrackLevel.TalentTrackSubLevelDict.Values)
                    {
                        if (talenttracksublevel.GetSubTalentValue(Session, talenttracksublevel.AchievementName, true) != 2)
                            return;
                    }

                    Session.GetHabbo().CitizenshipLevel++;

                    if (Session.GetHabbo().CitizenshipLevel >= 4)
                    {
                        Session.GetHabbo().CitizenshipLevel = 4;
                        Session.GetHabbo().HelperLevel = 1;

                        var Allows = new ServerMessage(Outgoing.PerkAllowancesMessageParser);
                        Allows.AppendInt32(1);
                        Allows.AppendString("CITIZEN");
                        Allows.AppendString("");
                        Allows.AppendBoolean(true);
                        Session.SendMessage(Allows);
                    }

                    ServerMessage Message = new ServerMessage(Outgoing.TalentLevelUpMessageParser);
                    Message.AppendString("citizenship");
                    Message.AppendInt32(Session.GetHabbo().CitizenshipLevel);
                    Message.AppendInt32(talentTrackLevel.TalentActionGift.Count);
                    foreach (string value in talentTrackLevel.TalentActionGift)
                    {
                        Message.AppendString(value);
                    }
                    Message.AppendInt32(talentTrackLevel.TalentFurniGift.Count);
                    foreach (Item item in talentTrackLevel.TalentFurniGift)
                    {
                        Message.AppendString(item.Name);
                        Message.AppendInt32(0);
                    }
                    Session.SendMessage(Message);
                }
            }
            else if (Type == "helper")
            {
                if (Session.GetHabbo().HelperLevel < 9) // helper
                {
                    TalentTrackLevel talentTrackLevel = this.helperTalents[Session.GetHabbo().HelperLevel];
                    foreach (TalentTrackSubLevel talenttracksublevel in talentTrackLevel.TalentTrackSubLevelDict.Values)
                    {
                        if (talenttracksublevel.GetSubTalentValue(Session, talenttracksublevel.AchievementName, true) != 2)
                            return;
                    }

                    Session.GetHabbo().HelperLevel++;
                    if (Session.GetHabbo().HelperLevel == 4)
                    {
                        if (Session.GetHabbo().Rank < 2)
                        {
                            Session.GetHabbo().Rank = 2;

                            var Allows = new ServerMessage(Outgoing.PerkAllowancesMessageParser);
                            Allows.AppendInt32(1);
                            Allows.AppendString("USE_GUIDE_TOOL");
                            Allows.AppendString("");
                            Allows.AppendBoolean(true);
                            Session.SendMessage(Allows);

                            using(IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbClient.runFastQuery("UPDATE users SET rank = '2' WHERE id = " + Session.GetHabbo().Id);
                            }
                        }
                    }
                    else if (Session.GetHabbo().HelperLevel == 6)
                    {
                        if (Session.GetHabbo().Rank < 3)
                        {
                            Session.GetHabbo().Rank = 3;

                            var Allows = new ServerMessage(Outgoing.PerkAllowancesMessageParser);
                            Allows.AppendInt32(1);
                            Allows.AppendString("JUDGE_CHAT_REVIEWS");
                            Allows.AppendString("");
                            Allows.AppendBoolean(true);
                            Session.SendMessage(Allows);

                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbClient.runFastQuery("UPDATE users SET rank = '3' WHERE id = " + Session.GetHabbo().Id);
                            }
                        }
                    }

                    if (Session.GetHabbo().HelperLevel > 9)
                        Session.GetHabbo().HelperLevel = 9;

                    ServerMessage Message = new ServerMessage(Outgoing.TalentLevelUpMessageParser);
                    Message.AppendString("helper");
                    Message.AppendInt32(Session.GetHabbo().HelperLevel);
                    Message.AppendInt32(talentTrackLevel.TalentActionGift.Count);
                    foreach (string value in talentTrackLevel.TalentActionGift)
                    {
                        Message.AppendString(value);
                    }
                    Message.AppendInt32(talentTrackLevel.TalentFurniGift.Count);
                    foreach (Item item in talentTrackLevel.TalentFurniGift)
                    {
                        Message.AppendString(item.Name);
                        Message.AppendInt32(0);
                    }
                    Session.SendMessage(Message);
                }
            }
        }
    }
}
