using System;
using System.Data;
using Butterfly.Messages;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;

namespace Butterfly.HabboHotel.News
{
    class NewsManager
    {
        internal ServerMessage cachedMessage;

        public void Initialize(IQueryAdapter dbClient)
        {
            dbClient.setQuery("SELECT * FROM server_promos ORDER BY id DESC LIMIT 3");
            var Data = dbClient.getTable();

            cachedMessage = new ServerMessage(Outgoing.SerializeNews);
            cachedMessage.AppendInt32(Data.Rows.Count);
            foreach (DataRow row in Data.Rows)
            {
                var currentNew = new News(row);
                currentNew.Serialize(cachedMessage);
            }

            Data.Clear();
        }

        public ServerMessage getCachedMessage()
        {
            return cachedMessage;
        }
    }

    class News
    {
        public int Id;
        public String Title;
        public String Content;
        public String ButtonCaption;
        public int Type;
        public String Url;
        public String Action;
        public String ImageUrl;

        public News(DataRow row)
        {
            Id = (int)row["id"];
            Title = (string)row["title"];
            Content = (string)row["content"];
            ButtonCaption = (string)row["button_caption"];
            Type = Int32.Parse((String)row["type"]);
            Url = (string)row["url"];
            Action = (string)row["action"];
            ImageUrl = (string)row["image_url"];
        }

        public void Serialize(ServerMessage Message)
        {
            Message.AppendInt32(Id);
            Message.AppendString(Title);
            Message.AppendString(Content);
            Message.AppendString(ButtonCaption);
            Message.AppendInt32(Type); // type (0, 1, 2)  [0 = normal promo, 1 = promo with button, 2 = promo without button]
            // ACTIONS

            // navigator/ (example navigator/search/sometag):
            // navigator/search/[PARAM] SEARCH BY [PARAM]
            // navigator/goto/[PARAM] GO TO ROOM [PARAM] (ID)

            // avatareditor/
            // avatareditor/open Open 'change look'

            // friendbar/
            // friendbar/findfriends (findNewFriends action)   (TWO requests => 211, 649)

            // talent/
            // talent/open/[PARAM] Open talents ([PARAM]: citizenship, helper)

            // inventory/
            // inventory/open Open inventory

            // questengine
            // questengine/gotorooms Go to rooms with quests (random)

            // catalog
            // catalog/open/[PARAM] Go to page [PARAM] (id)
            Message.AppendString((Type == 0) ? Url : Action); // [if type = 0, url; if type = 1, action]
            Message.AppendString(ImageUrl);
        }
    }
}
