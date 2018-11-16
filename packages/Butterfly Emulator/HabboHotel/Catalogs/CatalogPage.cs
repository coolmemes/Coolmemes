using System.Collections;
using Butterfly.Messages;
using ButterStorm;
using System;
using System.Collections.Generic;

namespace Butterfly.HabboHotel.Catalogs
{
    class CatalogPage
    {
        private readonly int Id;
        internal int ParentId;

        internal string Caption;
        internal int IconImage;

        internal bool Visible;
        internal bool Enabled;

        internal uint MinRank;

        internal string PageName;
        internal string Layout;

        internal string LayoutHeadline;
        internal string LayoutTeaser;
        internal string LayoutSpecial;
        internal string Text1;
        internal string Text2;
        internal string TextDetails;
        internal string TextTeaser;

        internal Hashtable Items;
        internal List<CatalogPageExtra> PageExtra;
        private ServerMessage mMessage;

        internal int PageId
        {
            get
            {
                return Id;
            }
        }

        internal CatalogPage(int Id, int ParentId, string Caption, bool Visible, bool Enabled,
            uint MinRank, int IconImage, string PageName, string Layout, string LayoutHeadline,
            string LayoutTeaser, string LayoutSpecial, string Text1, string Text2, string TextDetails,
            string TextTeaser, ref Hashtable CataItems, ref List<CatalogPageExtra> PagesExtra)
        {
            this.Items = new Hashtable();
            this.PageExtra = new List<CatalogPageExtra>();
            this.Id = Id;
            this.ParentId = ParentId;
            this.Caption = Caption;
            this.Visible = Visible;
            this.Enabled = Enabled;
            this.MinRank = MinRank;
            this.IconImage = IconImage;
            this.PageName = PageName;
            this.Layout = Layout;
            this.LayoutHeadline = LayoutHeadline;
            this.LayoutTeaser = LayoutTeaser;
            this.LayoutSpecial = LayoutSpecial;
            this.Text1 = Text1;
            this.Text2 = Text2;
            this.TextDetails = TextDetails;
            this.TextTeaser = TextTeaser;

            foreach (CatalogItem Item in CataItems.Values)
            {
                if (Item.PageID == Id)
                    Items.Add(Item.Id, Item);
            }

            foreach (CatalogPageExtra Page in PagesExtra)
            {
                if (Page.GetPageId() == Id)
                    PageExtra.Add(Page);
            }
        }

        internal void InitMsg()
        {
            mMessage = Catalog.SerializePage(this);
        }

        internal CatalogItem GetItem(uint pId)
        {
            if (Items.ContainsKey(pId))
                return (CatalogItem)Items[pId];

            return null;
        }

        internal ServerMessage GetMessage
        {
            get
            {
                return mMessage;
            }
        }

        internal void Serialize(int Rank, ServerMessage Message)
        {
            Message.AppendBoolean(Visible); // Visible
            Message.AppendInt32(IconImage); // Icon
            Message.AppendInt32(Enabled ? Id : -1); // PageID
            Message.AppendString(PageName); // PageName
            Message.AppendString(Caption + (Rank > 5 ? "("+Id+")" : "")); // Localization
            Message.AppendInt32(Items.Count); // Items Count
            foreach (CatalogItem item in Items.Values)
            {
                Message.AppendUInt(item.Id); // ItemID
            }
            Message.AppendInt32(OtanixEnvironment.GetGame().GetCatalog().GetTreeSize(Rank, Id)); // SubPages Count
        }
    }
}
