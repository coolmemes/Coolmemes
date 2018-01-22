using System;
using System.Collections;
using System.Data;

using Butterfly.HabboHotel.GameClients;
using Butterfly.Core;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;

namespace Butterfly.HabboHotel.Support
{
    class ModerationBanManager
    {
        private Hashtable bannedMachinedId;
        private Hashtable bannedUsernames;

        internal ModerationBanManager()
        {
            bannedMachinedId = new Hashtable();
            bannedUsernames = new Hashtable();
        }

        internal void LoadBans(IQueryAdapter dbClient)
        {
            bannedMachinedId.Clear();
            bannedUsernames.Clear();

            dbClient.setQuery("SELECT bantype, value, reason, expire FROM bans");
            DataTable dTable = dbClient.getTable();

            foreach (DataRow dRow in dTable.Rows)
            {
                string variable = (string)dRow["value"];
                string reason = (string)dRow["reason"];
                double expire = (double)dRow["expire"];
                string rawvar = (string)dRow["bantype"];

                if (expire < OtanixEnvironment.GetUnixTimestamp())
                    continue;

                switch (rawvar)
                {
                    case "user":
                        {
                            Ban ban = new Ban(ModerationBanType.USERNAME, variable, reason, expire);
                            if (!bannedUsernames.ContainsKey(variable))
                                bannedUsernames.Add(variable, ban);

                            break;
                        }
                    case "machine":
                        {
                            Ban ban = new Ban(ModerationBanType.MACHINEID, variable, reason, expire);
                            if (!bannedMachinedId.ContainsKey(variable))
                                bannedMachinedId.Add(variable, ban);

                            break;
                        }
                }
            }
        }

        internal void SuperBan(GameClient Client, string Username, string MachineId, string Reason, double LengthSeconds, GameClient Session)
        {
            BanUser(Client, Username, "", Reason, LengthSeconds, Session);
            BanUser(Client, "", MachineId, Reason, LengthSeconds, Session);
        }

        internal void BanUser(GameClient Client, string Username, string MachineId, string Reason, double LengthSeconds, GameClient Session)
        {
            string Var = Username;
            string RawVar = "user";
            if (Username.Length > 0)
            {
                RawVar = "user";
                Var = Username;
            }
            else if (MachineId.Length > 0)
            {
                RawVar = "machine";
                Var = MachineId;
            }

            double Expire = OtanixEnvironment.GetUnixTimestamp() + LengthSeconds;

            if (RawVar == "user")
            {
                if (UsersCache.getIdByUsername(Username) == 0)
                {
                    Session.SendNotif(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }
            }

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("REPLACE INTO bans (bantype,value,reason,expire,added_by,added_date) VALUES (@rawvar,@var,@reason,'" + Expire + "',@mod,'" + DateTime.Now.ToLongDateString() + "')");
                dbClient.addParameter("rawvar", RawVar);
                dbClient.addParameter("var", Var);
                dbClient.addParameter("reason", Reason);
                dbClient.addParameter("mod", (Session == null) ? "Automatic-BAN" : Session.GetHabbo().Username);
                dbClient.runQuery();
            }

            switch (RawVar)
            {
                case "user":
                    {
                        Ban ban = new Ban(ModerationBanType.USERNAME, Var, Reason, Expire);
                        if (!bannedUsernames.ContainsKey(Var))
                            bannedUsernames.Add(Var, ban);

                        break;
                    }
                case "machine":
                    {
                        Ban ban = new Ban(ModerationBanType.MACHINEID, Var, Reason, Expire);
                        if (!bannedMachinedId.ContainsKey(Var))
                            bannedMachinedId.Add(Var, ban);

                        break;
                    }
            }

            if (Client != null && Client.GetConnection() != null)
            {
                Client.SendBanMessage(LanguageLocale.GetValue("moderation.banned") + " " + Reason);
                Client.Disconnect();
            }
        }

        internal void UnbanUser(string username)
        {
            if (bannedUsernames.ContainsKey(username))
                bannedUsernames.Remove(username);

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("DELETE FROM bans WHERE value = @user");
                dbClient.addParameter("user", username);
                dbClient.runQuery();
            }
        }

        internal Ban GetBanReason(string username, string MachineId)
        {
            if (bannedUsernames.ContainsKey(username))
            {
                return (Ban)bannedUsernames[username];
            }
            else if (bannedMachinedId.ContainsKey(MachineId))
            {
                return (Ban)bannedMachinedId[MachineId];
            }

            return null;
        }
    }
}
