using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Nux
{
    class NuxUserInformation
    {
        private static string[] HELP_BUBLES = new string[]
        {
            "FRIENDS_BAR_ALL_FRIENDS",
            "FRIENDS_BAR_FIND_FRIENDS",
            "BOTTOM_BAR_BUILDERS_CLUB",
            "BOTTOM_BAR_HOME",
            "BOTTOM_BAR_RECEPTION",
            "BOTTOM_BAR_NAVIGATOR",
            "BOTTOM_BAR_CATALOGUE",
            "BOTTOM_BAR_INVENTORY",
            "BOTTOM_BAR_STORIES",
            "BOTTOM_BAR_MEMENU",
            "BOTTOM_BAR_QUESTS",
            "MEMENU_ACHIEVEMENTS",
            "MEMENU_CLOTHES",
            "MEMENU_FORUMS",
            "MEMENU_TALENTS",
            "MEMENU_GUIDE",
            "MEMENU_MAIL",
            "MEMENU_PROFILE",
            "MEMENU_ROOMS",
            "CHAT_INPUT",
            "HC_JOIN_BUTTON",
            "HELP_BUTTON",
            "SETTINGS_BUTTON",
            "CREDITS_BUTTON",
            "DUCKETS_BUTTON",
            "DIAMONDS_BUTTON",
            "LOGOUT_BUTTON",
            "ROOM_HISTORY_BACK_BUTTON",
            "ROOM_HISTORY_FORWARD_BUTTON",
            "ROOM_HISTORY_BUTTON",
            "CHAT_HISTORY_BUTTON",
            "LIKE_ROOM_BUTTON",
            "CAMERA_BUTTON"
        };

        public static string[] NewUserInformation = new string[] {
            "helpBubble/add/" + HELP_BUBLES[7] + "/nux.bot.info.inventory.1",
            "helpBubble/add/" + HELP_BUBLES[5] + "/nux.bot.info.navigator.1",
            "helpBubble/add/" + HELP_BUBLES[19] + "/nux.bot.info.chat.1",
            "helpBubble/add/" + HELP_BUBLES[2] + "/nux.bot.info.premium.1",
            "helpBubble/add/" + HELP_BUBLES[9] + "/nux.bot.info.memenu.1",
            "nux/lobbyoffer/show"
        };

        public static ServerMessage ShowInformation(int Step)
        {
            ServerMessage Message = new ServerMessage(Outgoing.ShowNewUserInformation);
            Message.AppendString(NewUserInformation[Step]);
            return Message;
        }
    }
}
