namespace Plus.HabboHotel.Catalog.Bundles
{
    internal class BundleFloorMobis
    {
        internal uint BaseItem;
        internal int X, Y, Rot;
        internal double Z;
        internal string ExtraData;

        internal BundleFloorMobis(uint baseItem, int x, int y, int rot, double z, string extraData)
        {
            BaseItem = baseItem;
            X = x;
            Y = y;
            Rot = rot;
            Z = z;
            ExtraData = extraData;
        }
    }
}