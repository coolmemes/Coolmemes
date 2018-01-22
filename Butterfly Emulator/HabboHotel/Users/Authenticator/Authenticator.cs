using System;
using System.Data;
using ButterStorm;
using System.Collections.Generic;
using Butterfly.HabboHotel.Users.Navigator;

namespace Butterfly.HabboHotel.Users.Authenticator
{
    static class HabboFactory
    {
        internal static Habbo GenerateHabbo(DataRow dRow)
        {
            var id = Convert.ToUInt32(dRow["id"]);
            var username = (string)dRow["username"];
            var realname = (string)dRow["real_name"];
            var rank = Convert.ToUInt32(dRow["rank"]);
            var motto = (string)dRow["motto"];
            var look = (string)dRow["look"];
            var created = Convert.ToDouble(dRow["account_created"]);
            var gender = (string)dRow["gender"];
            var NameChanges =  Convert.ToUInt32(dRow["namechanges"]);
            var diamonds =  Convert.ToUInt32(dRow["diamonds"]);
            var homeRoom = Convert.ToUInt32(dRow["home_room"]);
            var respect =  Convert.ToUInt32(dRow["respect"]);
            var dailyRespect =  Convert.ToUInt32(dRow["daily_respect_points"]);
            var dailyPetRespect =  Convert.ToUInt32(dRow["daily_pet_respect_points"]);
            var blockFriends = OtanixEnvironment.EnumToBool(dRow["block_newfriends"].ToString());
            var followenable = OtanixEnvironment.EnumToBool(dRow["follow_enable"].ToString());
            var questID = Convert.ToUInt32(dRow["currentquestid"]);
            var questProgress =  Convert.ToUInt32(dRow["currentquestprogress"]);
            var achiecvementPoints = Convert.ToUInt32(dRow["achievement_points"]);
            var tradeBlocked = OtanixEnvironment.EnumToBool((string)dRow["block_trade"]);
            var favoriteGroup =  Convert.ToUInt32(dRow["favoriteGroup"]);
            var volumenSystem = (string)dRow["volumenSystem"];
            var preferOldChat = OtanixEnvironment.EnumToBool((string)dRow["prefer_old_chat"]);
            var lastPurchase = (string)dRow["last_purchase"];
            var pollparticipation = new List<UInt32>();
            var pollparticipationstring = (string)dRow["poll_participation"];
            if (pollparticipationstring.Length > 0)
            {
                foreach (string value in pollparticipationstring.Split(';'))
                {
                    pollparticipation.Add(Convert.ToUInt32(value));
                }
            }
            var votedrooms = new List<UInt32>();
            var votedroomstring = (string)dRow["voted_rooms"];
            if (votedroomstring.Length > 0)
            {
                foreach (string value in votedroomstring.Split(';'))
                {
                    votedrooms.Add(Convert.ToUInt32(value));
                }
            }
            var lastfollowinglogin = (string)dRow["lastfollowinglogin"];
            var ignoreroominvitations = OtanixEnvironment.EnumToBool((string)dRow["ignoreRoomInvitations"]);
            var frankJaApareceu = OtanixEnvironment.EnumToBool(dRow["frankJaApareceu"].ToString());
            var citizenshiplevel = Convert.ToInt32(dRow["citizenship_level"]);
            var helperlevel = Convert.ToInt32(dRow["helper_level"]);
            var wiredactrewards = (string)dRow["actrewards"];
            var dontfocususers = OtanixEnvironment.EnumToBool((string)dRow["dontfocususers"]);
            var navilogs = new Dictionary<Int32, NaviLogs>();
            var navilogstring = (string)dRow["navilogs"];
            if (navilogstring.Length > 0)
            {
                foreach (string value in navilogstring.Split(';'))
                {
                    try
                    {
                        if (!navilogstring.Contains(","))
                            continue;

                        NaviLogs naviLogs = new NaviLogs(int.Parse(value.Split(',')[0]), value.Split(',')[1], value.Split(',')[2]);
                        if (!navilogs.ContainsKey(naviLogs.Id))
                            navilogs.Add(naviLogs.Id, naviLogs);
                    }
                    catch { }
                }
            }
            var targetedoffers = new Dictionary<UInt32, UInt32>();
            var targetedoffersstring = (string)dRow["targeted_offers"];
            if (!string.IsNullOrEmpty(targetedoffersstring))
            {
                foreach (string value in targetedoffersstring.Split(';'))
                {
                    if (value.Contains("-"))
                    {
                        if(!targetedoffers.ContainsKey(uint.Parse(value.Split('-')[0])))
                            targetedoffers.Add(uint.Parse(value.Split('-')[0]), uint.Parse(value.Split('-')[1]));
                    }
                }
            }
            var chatColor = (string)dRow["chat_color"];
            var newIdentity = (int)dRow["new_identity"];
            var newBot = (int)dRow["new_bot"];
            var moedas = Convert.ToInt32(dRow["moedas"]);
            var corAtual = Convert.ToInt32(dRow["corAtual"]);
            var coresjaTenho = Convert.ToString(dRow["coresJaTenho"]);
            var coinsPurchased = Convert.ToUInt32(dRow["coins_purchased"]);
            return new Habbo(id, username, realname, rank, motto, created, look, gender, diamonds, homeRoom, respect, dailyRespect, dailyPetRespect, blockFriends, followenable, questID, questProgress, achiecvementPoints, NameChanges, favoriteGroup, tradeBlocked, volumenSystem, preferOldChat, lastPurchase, pollparticipation, votedrooms, lastfollowinglogin, ignoreroominvitations, citizenshiplevel, helperlevel, wiredactrewards, dontfocususers, navilogs, targetedoffers, chatColor, newIdentity, newBot, frankJaApareceu, moedas, corAtual, coresjaTenho, coinsPurchased);
        }

        internal static Habbo GenerateHabboCache(DataRow dRow)
        {
            uint id = Convert.ToUInt32(dRow["id"]);
            string username = (string)dRow["username"];
            string realname = (string)dRow["real_name"];
            string motto = (string)dRow["motto"];
            uint rank = Convert.ToUInt32(dRow["rank"]);
            uint diamonds = Convert.ToUInt32(dRow["diamonds"]);
            string machineid = (string)dRow["machine_last"];
            string look = (string)dRow["look"];
            double created = Convert.ToDouble(dRow["account_created"]);
            string gender = (string)dRow["gender"];
            double lastonline = Convert.ToDouble(dRow["last_online"]);
            uint currentGroup = Convert.ToUInt32(dRow["favoriteGroup"]);
            uint achievementPoints = Convert.ToUInt32(dRow["achievement_points"]);
            bool blocknewfriends = OtanixEnvironment.EnumToBool(dRow["block_newfriends"].ToString());
            bool blocktrade = OtanixEnvironment.EnumToBool(dRow["block_trade"].ToString());
            bool ignoreroominvitations = OtanixEnvironment.EnumToBool((string)dRow["ignoreRoomInvitations"]);
            bool dontfocususers = OtanixEnvironment.EnumToBool((string)dRow["dontfocususers"]);
            bool preferOldChat = OtanixEnvironment.EnumToBool((string)dRow["prefer_old_chat"]);
            uint coins_purchased = Convert.ToUInt32(dRow["coins_purchased"]);

            return new Habbo(id, username, realname, rank, motto, created, look, gender, diamonds, machineid, achievementPoints, lastonline, currentGroup, blocknewfriends, blocktrade, ignoreroominvitations, dontfocususers, preferOldChat, coins_purchased);
        }
    }
}
