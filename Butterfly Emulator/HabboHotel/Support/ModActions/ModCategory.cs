using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Support.ModActions
{
    class ModCategory
    {
        public uint CategoryId;
        public string CategoryName;
        public List<ModSubCategory> SubCategories;

        public ModCategory(uint categoryId, string categoryName)
        {
            this.CategoryId = categoryId;
            this.CategoryName = categoryName;
            this.SubCategories = new List<ModSubCategory>();
        }

        public void AddSubCategory(uint subCategoryId, string subCategoryName, string subCategoryAction)
        {
            ModSubCategory subCategory = new ModSubCategory(subCategoryId, subCategoryName, subCategoryAction);
            this.SubCategories.Add(subCategory);
        }
    }
}
