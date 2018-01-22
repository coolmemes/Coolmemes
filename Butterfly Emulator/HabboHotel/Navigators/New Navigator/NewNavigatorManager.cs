using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Users.Navigator;
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

namespace Butterfly.HabboHotel.Navigators
{
    class NewNavigatorManager
    {
        private List<string> NavigatorTabs;
        private List<string> SubCategories;
        private List<string> SubCategoriesCollapsed;
        private Dictionary<string, List<NewNavigatorCategory>> Categories;
        private Dictionary<string, List<uint>> RoomsInCategories;

        internal void Initialize(IQueryAdapter dbClient)
        {
            NavigatorTabs = new List<string>();
            SubCategories = new List<string>();
            SubCategoriesCollapsed = new List<string>();
            Categories = new Dictionary<string, List<NewNavigatorCategory>>();
            RoomsInCategories = new Dictionary<string, List<uint>>();

            dbClient.setQuery("SELECT * FROM navigator_new_tabs WHERE enabled = '1' ORDER BY order_number ASC");
            DataTable dTabs = dbClient.getTable();

            dbClient.setQuery("SELECT * FROM navigator_new_categories WHERE enabled = '1' ORDER BY order_number ASC");
            DataTable dTable = dbClient.getTable();

            dbClient.setQuery("SELECT * FROM navigator_new_selections");
            DataTable dSelect = dbClient.getTable();

            foreach(DataRow dRow in dTabs.Rows)
            {
                NavigatorTabs.Add((string)dRow["name"]);
            }

            foreach (DataRow dRow in dTable.Rows)
            {
                NewNavigatorCategory newCategory = new NewNavigatorCategory(dRow);

                if (!Categories.ContainsKey(newCategory.MainCategory))
                    Categories.Add(newCategory.MainCategory, new List<NewNavigatorCategory>());

                SubCategories.Add(newCategory.SubCategory);
                Categories[newCategory.MainCategory].Add(newCategory);

                if (newCategory.Collapsed)
                {
                    if (!SubCategoriesCollapsed.Contains(newCategory.SubCategory))
                    {
                        SubCategoriesCollapsed.Add(newCategory.SubCategory);
                    }
                }
            }

            foreach (DataRow dRow in dSelect.Rows)
            {
                string name = (string)dRow["id"];
                uint roomId = Convert.ToUInt32(dRow["room_id"]);

                if (!RoomsInCategories.ContainsKey(name))
                    RoomsInCategories.Add(name, new List<uint>());

                RoomsInCategories[name].Add(roomId);
            }
        }

        /*internal Boolean CategoryExists(string Name)
        {
            return RoomsInCategories.ContainsKey(Name);
        }*/

        internal void AddRoomToCategory(string category, uint roomid)
        {
            if(RoomsInCategories.ContainsKey(category) && RoomsInCategories[category].Contains(roomid) == false)
            {
                RoomsInCategories[category].Add(roomid);
            }
        }

        internal void RemoveRoomToCategory(string category, uint roomid)
        {
            if (RoomsInCategories.ContainsKey(category) && RoomsInCategories[category].Contains(roomid) == true)
            {
                RoomsInCategories[category].Remove(roomid);
            }
        }

        internal List<uint> GetRoomsInCategory(string name)
        {
            if (RoomsInCategories.ContainsKey(name))
            {
                return RoomsInCategories[name];
            }

            return new List<uint>();
        }

        internal void SerializeNewNavigator(GameClient Session)
        {
            ServerMessage NavigatorMetaDataParser = new ServerMessage(Outgoing.NavigatorMetaDataParser);
            NavigatorMetaDataParser.AppendInt32(NavigatorTabs.Count); // número de pestañas
            foreach (string TabName in NavigatorTabs)
            {
                NavigatorMetaDataParser.AppendString(TabName); // string para reconocer la pestaña
                NavigatorMetaDataParser.AppendInt32(0); // int - foreach: (saved searched)
            }
            Session.SendMessage(NavigatorMetaDataParser);

            ServerMessage NavigatorSavedSearchesParser = new ServerMessage(Outgoing.NavigatorSavedSearchesParser);
            NavigatorSavedSearchesParser.AppendInt32(Session.GetHabbo().navigatorLogs.Count);
            foreach (NaviLogs navi in Session.GetHabbo().navigatorLogs.Values)
            {
                NavigatorSavedSearchesParser.AppendInt32(navi.Id);
                NavigatorSavedSearchesParser.AppendString(navi.Value1); // searchCode
                NavigatorSavedSearchesParser.AppendString(navi.Value2); // filter
                NavigatorSavedSearchesParser.AppendString(""); // localization
            }
            Session.SendMessage(NavigatorSavedSearchesParser);

            ServerMessage NavigatorLiftedRoomsParser = new ServerMessage(Outgoing.NavigatorLiftedRoomsParser);
            NavigatorLiftedRoomsParser.AppendInt32(0); // NavigatorHeaders.Count); // count:
            /*foreach (NavigatorHeader navHeader in NavigatorHeaders)
            {
                NavigatorLiftedRoomsParser.AppendUInt(navHeader.RoomId); // roomId
                NavigatorLiftedRoomsParser.AppendInt32(0); // ??
                NavigatorLiftedRoomsParser.AppendString(navHeader.Image);
                NavigatorLiftedRoomsParser.AppendString(navHeader.Caption);
            }*/
            Session.SendMessage(NavigatorLiftedRoomsParser);

            ServerMessage CollapsedCategoriesMessageParser = new ServerMessage(Outgoing.CollapsedCategoriesMessageParser);
            CollapsedCategoriesMessageParser.AppendInt32(OtanixEnvironment.GetGame().GetNavigator().FlatCatsCount + SubCategoriesCollapsed.Count);
            foreach (FlatCat flat in OtanixEnvironment.GetGame().GetNavigator().GetPrivateCategories.Values)
            {
                CollapsedCategoriesMessageParser.AppendString("category__" + flat.Caption);
            }
            foreach (string subcategory in SubCategoriesCollapsed)
            {
                CollapsedCategoriesMessageParser.AppendString(subcategory);
            }
            Session.SendMessage(CollapsedCategoriesMessageParser);

            ServerMessage NavigatorTamaño = new ServerMessage(Outgoing.NavigatorTamaño);
            NavigatorTamaño.AppendInt32(197); // x
            NavigatorTamaño.AppendInt32(185); // y
            NavigatorTamaño.AppendInt32(425); // width
            NavigatorTamaño.AppendInt32(535); // height
            NavigatorTamaño.AppendInt32(0);
            NavigatorTamaño.AppendBoolean(false); // Mostrar as pesquisas salvas?
            Session.SendMessage(NavigatorTamaño);
        }

        internal ServerMessage SerlializeNewNavigator(string nametype, string textbox, GameClient Session)
        {
            ServerMessage newNavigator = new ServerMessage(Outgoing.NewNavigator);
            newNavigator.AppendString(nametype); // Codigo de la pestaña en la que estamos.
            newNavigator.AppendString(textbox); // Texto escrito en el TextBox.
            newNavigator.AppendInt32((textbox.Length > 0) ? 1 : (!Categories.ContainsKey(nametype) ? 0 : Categories[nametype].Count)); // Número de desplegables que tenemos.

            if (textbox.Length > 0)
            {
                SearchResultList.SerializeNewNavigatorType("query", textbox, "", 0, false, Session, newNavigator);
            }
            else if (Categories.ContainsKey(nametype))
            {
                foreach(NewNavigatorCategory Category in Categories[nametype])
                {
                    SearchResultList.SerializeNewNavigatorType(Category.SubCategory, textbox, Category.Title, Category.ViewMode, Category.Collapsed, Session, newNavigator);
                }
            }

            return newNavigator;
        }
    }
}
