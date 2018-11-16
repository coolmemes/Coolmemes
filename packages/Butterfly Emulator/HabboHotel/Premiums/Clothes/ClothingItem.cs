using ButterStorm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Premiums.Clothes
{
    class ClothingItem
    {
        private string ItemName;
        private int[] ClothesId;

        public string GetItemName()
        {
            return ItemName;
        }

        public int[] GetClothes()
        {
            return ClothesId;
        }

        public ClothingItem(DataRow dRow)
        {
            this.ItemName = (string)dRow["item_name"];
            string[] ClothesParts = ((string)dRow["clothes_id"]).Split(',');
            this.ClothesId = new int[ClothesParts.Length];
            for (int i = 0; i < this.ClothesId.Length; i++)
            {
                this.ClothesId[i] = Convert.ToInt32(ClothesParts[i]);
                OtanixEnvironment.GetGame().GetClothingManager().AddClothesPart(this.ClothesId[i]);
            }
        }
    }
}
