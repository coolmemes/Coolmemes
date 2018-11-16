using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Filter
{
    internal struct BlackWord
    {
        public string Word;
        public BlackWordType Type;

        public BlackWordTypeSettings TypeSettings 
        { 
            get 
            { 
                return BlackWordsManager.GetSettings(Type);
            } 
        }

        public BlackWord(string word, BlackWordType type)
        {
            Word = word;
            Type = type;
        }
    }

    internal struct BlackWordTypeSettings
    {
        public string Filter, Alert, ImageAlert;
        public uint MaxAdvices;
        public bool AutoBan, ShowMessage;

        public BlackWordTypeSettings(string filter, string alert, uint maxAdvices, string imageAlert, bool autoBan, bool showMessage)
        {
            Filter = filter;
            Alert = alert;
            MaxAdvices = maxAdvices;
            ImageAlert = imageAlert;
            AutoBan = autoBan;
            ShowMessage = showMessage;
        }
    }

    internal enum BlackWordType
    {
        Hotel,
        Insult
    }

    internal static class BlackWordsManager
    {
        private static List<BlackWord> Words;
        private static Dictionary<BlackWordType, BlackWordTypeSettings> Replaces;

        public static void Load(IQueryAdapter dbClient)
        {
            Words = new List<BlackWord>();
            Replaces = new Dictionary<BlackWordType, BlackWordTypeSettings>();

            dbClient.setQuery("SELECT * FROM server_blackwords");
            
            DataTable table = dbClient.getTable();
            if (table == null)
                return;

            foreach(DataRow row in table.Rows)
            {
                string word = (string)row["word"];
                string typeStr = (string)row["type"];

                AddPrivateBlackWord(typeStr, word);
            }
        }

        public static void AddPrivateBlackWord(string typeStr, string word)
        {
            BlackWordType type;
            switch(typeStr)
            {
                case "hotel":
                    type = BlackWordType.Hotel;
                    break;

                case "insult":
                    type = BlackWordType.Insult;
                    break;

                default:
                    return;
            }

            Words.Add(new BlackWord(word, type));

            if (Replaces.ContainsKey(type))
                return;

            string filter = Filter.Default,
                   alert = "User [{0}] with Id: {1} has said a blackword. Word: {2}. Type: {3}. Message: {4}",
                   imageAlert = "bobba";
            var maxAdvices = 7u;
            bool autoBan = true, showMessage = true;

            if(File.Exists(@"Settings/BlackWords/" + typeStr + ".ini"))
            {
                foreach (string[] array in File.ReadAllLines(@"Settings/BlackWords/" + typeStr + ".ini").Where(line => line.Contains("=")).Select(line => line.Split('=')))
                {
                    if (array[0] == "filterType") filter = array[1];
                    if (array[0] == "maxAdvices") maxAdvices = uint.Parse(array[1]);
                    if (array[0] == "alertImage") imageAlert = array[1];
                    if (array[0] == "autoBan") autoBan = array[1] == "true";
                    if (array[0] == "showMessage") showMessage = array[1] == "true";
                }
            }

            if (File.Exists(@"Settings/BlackWords/" + typeStr + ".alert.txt"))
                alert = File.ReadAllText(@"Settings/BlackWords/" + typeStr + ".alert.txt");

            Replaces.Add(type, new BlackWordTypeSettings(filter, alert, maxAdvices, imageAlert, autoBan, showMessage));
        }

        public static BlackWordTypeSettings GetSettings(BlackWordType type)
        {
            return Replaces[type];
        }

        public static bool Check(string str, BlackWordType type, GameClient Session, string WhereInfo)
        {
            BlackWord word = new BlackWord();
            if (!Replaces.ContainsKey(type)) 
                return false;

            var strParsed = Filter.Replace(Replaces[type].Filter, str);

            word = Words.FirstOrDefault(wordStruct => wordStruct.Type == type && strParsed.Contains(wordStruct.Word));

            #region En caso de ser una palabra prohibida
            if (!string.IsNullOrEmpty(word.Word))
            {
                if (Session == null || Session.GetHabbo() == null)
                    return true;

                if (word.TypeSettings.ShowMessage && !Session.GetHabbo().HasFuse("fuse_mod"))
                {
                    Room room = Session.GetHabbo().CurrentRoom;
                    if (room != null)
                    {
                        RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                        if (user != null)
                        {
                            user.WhisperComposer(LanguageLocale.GetValue("filter.message"));
                        }
                    }
                }

                Session.GetHabbo().publicHotelCount++;
                if (Session.GetHabbo().publicHotelCount >= 4)
                    return true;

                ServerMessage serverAlert = new ServerMessage(Outgoing.InstantChat);
                serverAlert.AppendUInt(EmuSettings.CHAT_USER_ID);
                serverAlert.AppendString(Session.GetHabbo().Username + " foi pego no filtro " + WhereInfo + ":\n" + str);
                serverAlert.AppendInt32(0);
                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(serverAlert, "fuse_chat_staff", 0);

                return true;
            }
            #endregion
            #region ¿Posible Flood?
            if (Session.GetHabbo().LastMessage == str)
            {
                Session.GetHabbo().LastMessageCount++;

                if (Session.GetHabbo().LastMessageCount >= 3)
                {
                    ServerMessage serverAlert = new ServerMessage(Outgoing.InstantChat);
                    serverAlert.AppendUInt(EmuSettings.CHAT_USER_ID);
                    serverAlert.AppendString(Session.GetHabbo().Username + " ha escrito " + Session.GetHabbo().LastMessageCount + " veces en  " + WhereInfo + ":\n" + str);
                    serverAlert.AppendInt32(0);
                    OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(serverAlert, "fuse_chat_staff", 0);

                    Session.GetHabbo().LastMessageCount = 0;
                }

                return false;
            }

            Session.GetHabbo().LastMessageCount = 0;
            Session.GetHabbo().LastMessage = str;
            #endregion

            return false;
        }

        public static bool CheckRoomFilter(string str, List<string> roomWords)
        {
            try
            {
                str = Filter.Replace(Replaces[BlackWordType.Hotel].Filter, str);
                string word = roomWords.FirstOrDefault(wordStruct => str.Contains(wordStruct));

                return (!string.IsNullOrEmpty(word));
            }
            catch
            {
                return false;
            }
        }

        public static string SpecialReplace(string baseMessage, RoomUser user)
        {
            string message = baseMessage.ToString();
            Room room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(user.RoomId);
            string tempoprisao = OtanixEnvironment.UnixTimeStampToDateTime(user.GetTempoPreso()).ToString();
                
            message = message.Replace("%username%", user.GetUsername());
            message = message.Replace("%name%", user.GetUsername());
            message = message.Replace("%userid%", user.HabboId.ToString());
            if (user.GetTempoPreso() > OtanixEnvironment.GetUnixTimestamp())
                message = message.Replace("%tempoprisao%", tempoprisao);
            else
                message = message.Replace("%tempoprisao%", "");
            message = message.Replace("%usersonline%", OtanixEnvironment.GetGame().GetClientManager().connectionCount.ToString());
            message = message.Replace("%roomname%", room.RoomData.Name);
            message = message.Replace("%roomid%", room.RoomData.Id.ToString());
            message = message.Replace("%user_count%", room.RoomData.UsersNow.ToString());
            message = message.Replace("%floor_item_count%", room.GetRoomItemHandler().mFloorItems.Count.ToString());
            message = message.Replace("%wall_item_count%", room.GetRoomItemHandler().mWallItems.Count.ToString());
            message = message.Replace("%roomowner%", room.RoomData.Owner);
            message = message.Replace("%owner%", room.RoomData.Owner);
            message = message.Replace("%item_count%", (room.GetRoomItemHandler().mFloorItems.Count + room.GetRoomItemHandler().mWallItems.Count).ToString());
          
            return message;
        }

        public static string SpecialReplace(string baseMessage, GameClient Session)
        {
            string message = baseMessage.ToString();

            message = message.Replace("%username%", Session.GetHabbo().Username);

            return message;
        }
    }
}
