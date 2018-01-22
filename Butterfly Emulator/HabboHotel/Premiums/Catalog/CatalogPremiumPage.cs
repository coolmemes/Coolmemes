using Butterfly.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Premiums.Catalog
{
    class CatalogPremiumPage
    {
        public int Id;
        public int ParentId;
        public string Caption;
        public int IconImage;
        public bool Visible;
        public bool Enable;
        public string PageName;
        public string PageLayout;
        public string PageHeadline;
        public string PageTeaser;
        public string PageSpecial;
        public string PageText;
        public string PageTextDetails;
        public string PageTextTeaser;
        public Hashtable Items;

        private ServerMessage mMessage;

        public CatalogPremiumPage(int Id, int ParentId, string Caption, bool Visible, bool Enable, int IconImage, string PageName, string PageLayout, string PageHeadline,
            string PageTeaser, string PageSpecial, string PageText, string PageTextDetails, string PageTextTeaser, ref Hashtable CataItems)
        {
            this.Id = Id;
            this.ParentId = ParentId;
            this.Caption = Caption;
            this.Visible = Visible;
            this.Enable = Enable;
            this.IconImage = IconImage;
            this.PageName = PageName;
            this.PageLayout = PageLayout;
            this.PageHeadline = PageHeadline;
            this.PageTeaser = PageTeaser;
            this.PageSpecial = PageSpecial;
            this.PageText = PageText;
            this.PageTextDetails = PageTextDetails;
            this.PageTextTeaser = PageTextTeaser;
            this.Items = new Hashtable();

            foreach (CatalogPremiumItem Item in CataItems.Values)
            {
                if (Item.PageID == Id)
                    Items.Add(Item.Id, Item);
            }

            this.mMessage = CatalogPremium.SerializePage(this);
        }

        public ServerMessage GetMessage()
        {
            return mMessage;
        }

        public void Serialize(ServerMessage Message)
        {
            Message.AppendBoolean(Visible);
            Message.AppendInt32(IconImage);
            Message.AppendInt32(Id);
            Message.AppendString(PageName);
            Message.AppendString(Caption);
            Message.AppendInt32(Items.Count);
            foreach (CatalogPremiumItem item in Items.Values)
            {
                Message.AppendUInt(item.Id);
            }
        }

        public CatalogPremiumItem GetItem(uint pId)
        {
            if (Items.ContainsKey(pId))
                return (CatalogPremiumItem)Items[pId];

            return null;
        }
    }
}
