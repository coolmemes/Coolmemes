using System.Collections.Generic;

namespace Plus.HabboHotel.Catalog.Bundles
{
    public class BundleContent
    {
        internal int CatalogId;
        internal Dictionary<int, int> Items;

        public BundleContent(int catalogId, Dictionary<int, int> items)
        {
            CatalogId = catalogId;
            Items = items;
        }
    }
}