using System;
using System.Collections.Generic;
using System.Data;
using Butterfly.HabboHotel.Users.Inventory;
using Butterfly.HabboHotel.Users.Messenger;
using ButterStorm;
using Butterfly.HabboHotel.Users.Authenticator;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Subscriptions.HabboClub;
using Otanix.HabboHotel.Sanctions;

namespace Butterfly.HabboHotel.Users.UserDataManagement
{
    class UserDataFactory
    {
        internal static UserData GetUserData(string sessionTicket)
        {
            DataRow dUserInfo;
            DataTable dFavouriteRooms;
            DataTable dEffects;
            DataTable dFriends;
            DataTable dRequests;
            DataTable dClub;
            DataTable dSanctions;
            UInt32 userID;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM users RIGHT JOIN user_tickets ON user_tickets.userid = users.id WHERE user_tickets.sessionticket = @sso");
                dbClient.addParameter("sso", sessionTicket);
                dUserInfo = dbClient.getRow();

                if (dUserInfo == null)
                {
                    return null;
                }

                userID = Convert.ToUInt32(dUserInfo["id"]);

                if (OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(userID) != null)
                {
                    // Desconectamos al usuario conectado ya.
                    OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(userID).Disconnect();
                    
                    dbClient.setQuery("SELECT * FROM users WHERE id = '" + userID + "'");
                    dUserInfo = dbClient.getRow();
                }

                // Manda las salas favoritas al Login, por lo tanto se queda aquí esto.
                dbClient.setQuery("SELECT room_id FROM user_favorites WHERE user_id = " + userID);
                dFavouriteRooms = dbClient.getTable();

                // Manda los efectos al conectarse, por lo tanto lo dejamos así.
                dbClient.setQuery("SELECT * FROM user_effects WHERE user_id =  " + userID + " GROUP by effect_id");
                dEffects = dbClient.getTable();

                // Los amigos son urgentes para conectarse
                dbClient.setQuery("SELECT users.id,users.username,users.motto,users.look,users.last_online " +
                                        "FROM users " +
                                        "JOIN messenger_friendships " +
                                        "ON users.id = messenger_friendships.sender " +
                                        "WHERE messenger_friendships.receiver = " + userID + " " +
                                        "UNION ALL " +
                                        "SELECT users.id,users.username,users.motto,users.look,users.last_online " +
                                        "FROM users " +
                                        "JOIN messenger_friendships " +
                                        "ON users.id = messenger_friendships.receiver " +
                                        "WHERE messenger_friendships.sender = " + userID + " LIMIT " + EmuSettings.FRIENDS_LIMIT);
                dFriends = dbClient.getTable();

                // Comprobamos si hay peticiones de amistad.
                dbClient.setQuery("SELECT messenger_requests.sender,messenger_requests.receiver,users.username " +
                                        "FROM users " +
                                        "JOIN messenger_requests " +
                                        "ON users.id = messenger_requests.sender " +
                                        "WHERE messenger_requests.receiver = " + userID + " LIMIT " + EmuSettings.FRIENDS_REQUEST_LIMIT);
                dRequests = dbClient.getTable();

                dbClient.setQuery("SELECT * FROM user_subscriptions WHERE user_id = '" + userID + "'");
                dClub = dbClient.getTable();

                dbClient.setQuery("SELECT * FROM moderation_sanctions WHERE user_id = '" + userID + "'");
                dSanctions = dbClient.getTable();
            }
           
            List<uint> favouritedRooms = new List<uint>();
            foreach (DataRow dRow in dFavouriteRooms.Rows)
            {
                favouritedRooms.Add(Convert.ToUInt32(dRow["room_id"]));
            }

            List<AvatarEffect> effects = new List<AvatarEffect>();
            foreach (DataRow dRow in dEffects.Rows)
            {
                effects.Add(new AvatarEffect((int)dRow["effect_id"], (int)dRow["total_duration"], OtanixEnvironment.EnumToBool((string)dRow["is_activated"]), (double)dRow["activated_stamp"], (int)dRow["effect_count"]));
            }

            Dictionary<uint, MessengerBuddy> friends = new Dictionary<uint, MessengerBuddy>();
            foreach (DataRow dRow in dFriends.Rows)
            {
                if (Convert.ToUInt32(dRow["id"]) == userID)
                    continue;

                if (!friends.ContainsKey(Convert.ToUInt32(dRow["id"])))
                    friends.Add(Convert.ToUInt32(dRow["id"]), new MessengerBuddy(Convert.ToUInt32(dRow["id"]), (string)dRow["username"], (string)dRow["look"], (string)dRow["motto"]));
            }

            Dictionary<uint, MessengerRequest> requests = new Dictionary<uint, MessengerRequest>();
            foreach (DataRow dRow in dRequests.Rows)
            {
                if (Convert.ToUInt32(dRow["sender"]) != userID)
                {
                    if (!requests.ContainsKey(Convert.ToUInt32(dRow["sender"])))
                        requests.Add(Convert.ToUInt32(dRow["sender"]), new MessengerRequest(userID, Convert.ToUInt32(dRow["sender"]), (string)dRow["username"]));
                }
                else
                {
                    if (!requests.ContainsKey(Convert.ToUInt32(dRow["receiver"])))
                        requests.Add(Convert.ToUInt32(dRow["receiver"]), new MessengerRequest(userID, Convert.ToUInt32(dRow["receiver"]), (string)dRow["username"]));
                }
            }

            Dictionary<string, Club> clubSubscriptions = new Dictionary<string, Club>();
            foreach (DataRow dRow in dClub.Rows)
            {
                clubSubscriptions.Add(Convert.ToString(dRow["subscription_id"]), new Club(Convert.ToString(dRow["subscription_id"]), Convert.ToInt32(dRow["timestamp_activated"]), Convert.ToInt32(dRow["timestamp_expire"]), Convert.ToBoolean(dRow["received_pay"])));
            }

            Dictionary<uint, Sanction> Sanctions = new Dictionary<uint, Sanction>();
            foreach (DataRow dRow in dSanctions.Rows)
            {
                Sanctions.Add(Convert.ToUInt32(dRow["user_id"]), new Sanction(Convert.ToUInt32(dRow["user_id"]), Convert.ToInt32(dRow["reason"]), Convert.ToInt32(dRow["start_time"]), Convert.ToInt32(dRow["remaining_time"]), Convert.ToString(dRow["next_sanction"])));
            }

            Habbo user = HabboFactory.GenerateHabbo(dUserInfo);

            dUserInfo = null;
            dFavouriteRooms = null;
            dEffects = null;
            dFriends = null;
            dRequests = null;

            return new UserData(favouritedRooms, effects, friends, requests, user, clubSubscriptions, Sanctions);
        }

        internal static Habbo GetUserDataCache(uint userId)
        {
            DataRow dUser;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id, username, real_name, rank, motto, account_created, look, gender, diamonds, machine_last, last_online, achievement_points, favoriteGroup, block_newfriends, block_trade, ignoreRoomInvitations, dontfocususers, prefer_old_chat, alertasAtivados, frankJaApareceu, moedas, corAtual, coresJaTenho, coins_purchased FROM users WHERE id = " + userId);
                dUser = dbClient.getRow();
            }

            if (dUser == null)
                return null;

            return HabboFactory.GenerateHabboCache(dUser);
        }

        internal static Habbo GetUserDataCache(string username)
        {
            DataRow dUser;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id, username, real_name, rank, motto, account_created, look, gender, diamonds, machine_last, last_online, achievement_points, favoriteGroup, block_newfriends, block_trade, ignoreRoomInvitations, dontfocususers, prefer_old_chat, alertasAtivados, frankJaApareceu, moedas, corAtual, coresJaTenho, coins_purchased FROM users WHERE username = @username");
                dbClient.addParameter("username", username);
                dUser = dbClient.getRow();
            }

            if (dUser == null)
                return null;

            return HabboFactory.GenerateHabboCache(dUser);
        }
    }
}
