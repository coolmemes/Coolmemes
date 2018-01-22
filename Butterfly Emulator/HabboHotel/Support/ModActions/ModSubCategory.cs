using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Support.ModActions
{
    class ModSubCategory
    {
        public uint SubCategoryId;
        public string SubCategoryName;
        public string SubCategoryAction;

        public ModSubCategory(uint subCategoryId, string subCategoryName, string subCategoryAction)
        {
            this.SubCategoryId = subCategoryId;
            this.SubCategoryName = subCategoryName;
            this.SubCategoryAction = subCategoryAction;
        }
    }
}
