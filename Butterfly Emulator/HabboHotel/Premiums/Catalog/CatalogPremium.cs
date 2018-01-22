using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Premiums.Catalog
{
    class CatalogPremium
    {
        public Dictionary<int, CatalogPremiumPage> Pages;

        public ServerMessage CatalogPagesCache;
        private Hashtable CatalogItems;

        public void Initialize(IQueryAdapter dbClient)
        {
            Pages = new Dictionary<int, CatalogPremiumPage>();
            CatalogItems = new Hashtable();

            dbClient.setQuery("SELECT * FROM catalog_premium_pages ORDER BY order_num");
            DataTable dPages = dbClient.getTable();

            dbClient.setQuery("SELECT * FROM catalog_premium_items");
            DataTable dItems = dbClient.getTable();

            if (dItems != null)
            {
                foreach (DataRow dRow in dItems.Rows)
                {
                    CatalogItems.Add(Convert.ToUInt32(dRow["id"]), new CatalogPremiumItem(dRow));
                }
            }

            if (dPages != null)
            {
                foreach (DataRow Row in dPages.Rows)
                {
                    Pages.Add((int)Row["id"], new CatalogPremiumPage((int)Row["id"], (int)Row["parent_id"],
                        (string)Row["caption"], OtanixEnvironment.EnumToBool(Row["visible"].ToString()), OtanixEnvironment.EnumToBool(Row["enabled"].ToString()),
                        (int)Row["icon_image"], (string)Row["page_name"], (string)Row["page_layout"], (string)Row["page_headline"], (string)Row["page_teaser"], (string)Row["page_special"], (string)Row["page_text1"],
                        (string)Row["page_text_details"], (string)Row["page_text_teaser"], ref CatalogItems));
                }
            }

            CatalogPagesCache = SerializeCatalogPages();
        }

        public ServerMessage SerializeCatalogPages()
        {
            ServerMessage Message = new ServerMessage(Outgoing.OpenShop);
            Message.AppendBoolean(true);
            Message.AppendInt32(0); // icon
            Message.AppendInt32(-1);
            Message.AppendString("root");
            Message.AppendString("");
            Message.AppendInt32(0);
            Message.AppendInt32(GetTreeSize(-1));
            foreach (CatalogPremiumPage Page in Pages.Values)
            {
                if (Page.ParentId != -1)
                    continue;

                Page.Serialize(Message);
                Message.AppendInt32(GetTreeSize(Page.Id));

                foreach (CatalogPremiumPage SubPage in Pages.Values)
                {
                    if (SubPage.ParentId != Page.Id)
                        continue;

                    SubPage.Serialize(Message);
                    Message.AppendInt32(GetTreeSize(SubPage.Id));

                    foreach (CatalogPremiumPage _SubPage in Pages.Values)
                    {
                        if (_SubPage.ParentId != SubPage.Id)
                            continue;

                        _SubPage.Serialize(Message);
                        Message.AppendInt32(GetTreeSize(_SubPage.Id));
                    }
                }
            }
            Message.AppendBoolean(false);
            Message.AppendString("BUILDERS_CLUB");

            return Message;
        }

        private int GetTreeSize(int TreeId)
        {
            int count = 0;

            foreach (CatalogPremiumPage Page in Pages.Values)
            {
                if (Page.ParentId == TreeId)
                    count++;
            }

            return count;
        }

        public static ServerMessage SerializePage(CatalogPremiumPage Page)
        {
            ServerMessage Message = new ServerMessage(Outgoing.CatalogPageMessageParser);
            Message.AppendInt32(Page.Id);
            Message.AppendString("BUILDERS_CLUB");
            Message.AppendString(Page.PageLayout);

            switch (Page.PageLayout)
            {
                case "frontpage4":

                   
                    Message.AppendInt32(2);
                    Message.AppendString(Page.PageHeadline);
                    Message.AppendString(Page.PageTeaser);
                    Message.AppendInt32(2);
                    Message.AppendString(Page.PageText);
                    Message.AppendString(Page.PageTextDetails);

                    break;

                default:

                    Message.AppendInt32(3);
                    Message.AppendString(Page.PageHeadline);
                    Message.AppendString(Page.PageTeaser);
                    Message.AppendString(Page.PageSpecial);
                    Message.AppendInt32(3);
                    Message.AppendString(Page.PageText);
                    Message.AppendString(Page.PageTextDetails);
                    Message.AppendString(Page.PageTextTeaser);

                    break;
            }

            Message.AppendInt32(Page.Items.Count);
            foreach (CatalogPremiumItem Item in Page.Items.Values)
            {
                Item.Serialize(Message);
            }
            Message.AppendInt32(-1);
            Message.AppendBoolean(false);

            return Message;
        }

        public CatalogPremiumPage GetPage(int Page)
        {
            if (!Pages.ContainsKey(Page))
            {
                return null;
            }

            return Pages[Page];
        }
    }
}

