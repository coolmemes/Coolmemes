using Butterfly.HabboHotel.Users.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Items.Craftable
{
    class CraftableProduct
    {
        private uint ItemId;
        private string ItemName;
        private uint[] ReqIds;

        internal uint GetItemId
        {
            get { return ItemId; }
        }

        internal uint[] GetReqIds
        {
            get
            {
                return ReqIds;
            }
        }

        internal string GetItemName()
        {
            return ItemName;
        }

        internal CraftableProduct(uint itemId, string itemName, uint[] reqIds)
        {
            this.ItemId = itemId;
            this.ItemName = itemName;
            this.ReqIds = reqIds;
        }

        internal List<uint> GetDifferentItems()
        {
            List<uint> uniqueIds = new List<uint>();

            for (int i = 0; i < this.ReqIds.Length; i++)
            {
                uint valor = this.ReqIds[i];

                if (!uniqueIds.Contains(valor))
                    uniqueIds.Add(valor);
            }

            return uniqueIds;
        }

        internal uint GetItemsCount(uint ItemId)
        {
            uint count = 0;
            foreach (uint i in ReqIds)
            {
                if (i == ItemId)
                    count++;
            }

            return count;
        }

        internal bool ContainsElements(InventoryComponent iComp)
        {
            List<uint> uniqueIds = GetDifferentItems();

            foreach (uint BaseId in uniqueIds)
            {
                uint ItemsCount = GetItemsCount(BaseId);

                if (iComp.GetBaseIdCount(BaseId) >= ItemsCount)
                    continue;

                return false;
            }

            uniqueIds.Clear();
            uniqueIds = null;

            return true;
        }
    }
}
