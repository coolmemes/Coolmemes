using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.ChatMessageStorage.ChatColorido
{
    class CorManager
    {
        public void atualizaPracolorido(GameClient Session)
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

                string RankPlugin = OtanixEnvironment.GetGame().GetRoleManager().GetRankHtmlName(Session.GetHabbo().Rank);

                if (!string.IsNullOrEmpty(RankPlugin) || Session.GetHabbo().corAtual > 0)
                {
                    ServerMessage Message = new ServerMessage(Outgoing.ChangeUserListName);
                    Message.AppendUInt(Room.Id);
                    Message.AppendInt32(User.VirtualId);
                    Message.AppendString(RankPlugin + GenerateColorName(Session.GetHabbo().corAtual, Session.GetHabbo().Username));

                    foreach (RoomUser roomUser in Room.GetRoomUserManager().UserList.Values)
                    {
                        if (roomUser.IsBot || roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null || roomUser.GetClient().GetHabbo().preferOldChat)
                            continue;

                        roomUser.GetClient().SendMessage(Message);
                    }

                }         
        }

        public void atualizaNomePraNormal(GameClient Session)
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            ServerMessage Message = new ServerMessage(Outgoing.ChangeUserListName);
            Message.AppendUInt(Room.Id);
            Message.AppendInt32(User.VirtualId);
            Message.AppendString(Session.GetHabbo().Username);

            foreach (RoomUser roomUser in Room.GetRoomUserManager().UserList.Values)
            {
                if (roomUser.IsBot || roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null || roomUser.GetClient().GetHabbo().preferOldChat)
                    continue;

                roomUser.GetClient().SendMessage(Message);
            }

        }

        public string GenerateColorName(int corAtual, string Username)
        {

            if (corAtual == 0)
                return Username;

            string Text = "";
            char[] UsernameArray = Username.ToCharArray();
            string[] Colors = OtanixEnvironment.GetGame().GetCatalog().pegaCor(corAtual).Split('-');

            if (Colors.Length == 2)
            {
                int num = 0;
                foreach (char c in UsernameArray)
                {
                    if (num % 2 == 0)
                        Text += "<font color='#" + Colors[0] + "'>" + c + "</font>";
                    else
                        Text += "<font color='#" + Colors[1] + "'>" + c + "</font>";

                    num++;
                }
            }
            else
            {
                Text += "<font color='#" + Colors[0] + "'>" + Username + "</font>";
            }

            return Text;
        }

        public StringBuilder gerarCorList(string MinhasCores, string Username)
        {
            StringBuilder minhasCores = new StringBuilder();
            minhasCores.Append(LanguageLocale.GetValue("cor.disponiveis") + "\n\n");
            foreach (string corAtual in MinhasCores.Split(';'))
            {
                int corIntAtual = Convert.ToInt32(corAtual);
                if (corIntAtual == 0)
                    continue;

                string minhaCor = OtanixEnvironment.GetGame().GetCatalog().pegaCor(corIntAtual);
                minhasCores.Append(":" + LanguageLocale.GetValue("cor.comando") + " " + corAtual + ", ex: " + OtanixEnvironment.GetGame().CorManager().GenerateColorName(corIntAtual, Username) + "\n");

            }

            return minhasCores;
        }
    }
}
