using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Catalogs
{
    class CatalogPageExtra
    {
        private int Id;
        private int PageId;
        private string Caption;
        private string Image;
        private string Code;

        public int GetId()
        {
            return Id;
        }

        public int GetPageId()
        {
            return PageId;
        }

        public string GetCaption()
        {
            return Caption;
        }

        public string GetImage()
        {
            return Image;
        }

        public string GetCode()
        {
            return Code;
        }

        public CatalogPageExtra(DataRow Row)
        {
            this.Id = (int)Row["id"];
            this.PageId = (int)Row["page_id"];
            this.Caption = (string)Row["caption"];
            this.Image = (string)Row["image"];
            this.Code = (string)Row["code"];
        }
    }
}
