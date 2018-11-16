namespace Plus.HabboHotel.Catalog.Bundles
{
    internal class BundleParedeMobis
    {
        internal uint BaseItem;
        internal string WallCoord;
        internal string ExtraData;

        internal BundleParedeMobis(uint baseItem, string wallCoord, string extraData)
        {
            BaseItem = baseItem;
            WallCoord = wallCoord;
            ExtraData = extraData;
        }
    }
}