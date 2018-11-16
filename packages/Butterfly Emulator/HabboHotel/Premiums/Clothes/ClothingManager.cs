using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Premiums.Clothes
{
    class ClothingManager
    {
        private Dictionary<string, ClothingItem> clothes;
        private List<int> clothesParts;

        public void Initialize(IQueryAdapter dbClient)
        {
            this.clothes = new Dictionary<string, ClothingItem>();
            this.clothesParts = new List<int>();

            dbClient.setQuery("SELECT * FROM premium_clothing");
            DataTable dTable = dbClient.getTable();

            if (dTable != null)
            {
                foreach (DataRow dRow in dTable.Rows)
                {
                    string ItemName = (string)dRow["item_name"];
                    ClothingItem ClothesItem = new ClothingItem(dRow);

                    if (!clothes.ContainsKey(ItemName))
                        clothes.Add(ItemName, ClothesItem);
                }
            }
        }

        public ClothingItem GetClothingItemByName(string ItemName)
        {
            if (clothes.ContainsKey(ItemName))
                return clothes[ItemName];

            return null;
        }

        public void AddClothesPart(int Part)
        {
            if (!clothesParts.Contains(Part))
                clothesParts.Add(Part);
        }

        public bool IsPremiumPart(int Part)
        {
            return clothesParts.Contains(Part);
        }
    }
}
