using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Chat
{
    class ChatSettingsManager
    {
        private ChatSettings chatSettings;
        private string PrefixHtml;
        private string SuffixHtml;

        public ChatSettingsManager(uint UserId)
        {
            using(IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM user_chat WHERE user_id = '" + UserId + "'");
                DataRow dRow = dbClient.getRow();
                if(dRow != null)
                {
                    chatSettings = new ChatSettings((string)dRow["color"], Convert.ToUInt32(dRow["size"]), OtanixEnvironment.EnumToBool((string)dRow["bold"]), OtanixEnvironment.EnumToBool((string)dRow["italics"]), OtanixEnvironment.EnumToBool((string)dRow["underlined"]));
                }
            }

            GenerateHtmlStrings();
        }

        public string GetPrefixHtml()
        {
            return PrefixHtml;
        }

        public string GetSuffixHtml()
        {
            return SuffixHtml;
        }

        public void GenerateHtmlStrings()
        {
            PrefixHtml = "";
            SuffixHtml = "";

            if (chatSettings == null)
                return;

            if (!string.IsNullOrEmpty(chatSettings.GetColor()) || chatSettings.GetSize() != 0)
            {
                PrefixHtml += "<font";
                SuffixHtml += "</font>";

                if (!string.IsNullOrEmpty(chatSettings.GetColor()))
                {
                    PrefixHtml += " color=\"" + chatSettings.GetColor() + "\"";
                }

                if(chatSettings.GetSize() != 0)
                {
                    PrefixHtml += " size=\"" + chatSettings.GetSize() + "\"";
                }

                PrefixHtml += ">";
            }

            if (chatSettings.GetBold())
            {
                PrefixHtml += "<b>";
                SuffixHtml += "</b>";
            }

            if (chatSettings.GetItalics())
            {
                PrefixHtml += "<i>";
                SuffixHtml += "</i>";
            }

            if (chatSettings.GetUnderlined())
            {
                PrefixHtml += "<u>";
                SuffixHtml += "</u>";
            }
        }
    }
}
