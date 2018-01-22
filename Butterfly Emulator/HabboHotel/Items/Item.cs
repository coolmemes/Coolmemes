using ButterStorm;
using System;
using System.Collections.Generic;

namespace Butterfly.HabboHotel.Items
{
    class Item
    {
        private readonly uint Id;

        internal int SpriteId;

        internal string Name;
        internal char Type;

        internal int Width;
        internal int Length;
        internal double Height;

        internal bool Stackable;
        internal bool Walkable;
        internal bool IsSeat;

        internal bool AllowRecycle;
        internal bool AllowTrade;
        internal bool AllowMarketplaceSell;
        internal bool AllowInventoryStack;
        internal bool AllowRotations;

        internal InteractionType InteractionType;

        internal List<int> VendingIds;
        internal List<double> MultiHeight;

        internal int Modes;

        internal bool IsGroupItem;
        internal int LimitedStack;

        internal uint ItemId
        {
            get
            {
                return Id;
            }
        }

        internal Item(UInt32 Id, int Sprite, string Name, string Type, int Width, int Length, double Height, bool Stackable, bool Walkable, bool IsSeat, bool AllowRecycle, bool AllowTrade, bool AllowMarketplaceSell, bool AllowInventoryStack, bool AllowRotations, InteractionType InteractionType, int Modes, string VendingIds, int LimitedStack, string multiHeight)
        {
            this.Id = Id;
            this.SpriteId = Sprite;
            this.Name = Name;
            this.Type = char.Parse(Type);
            this.Width = Width;
            this.Length = Length;
            this.Height = Height;
            this.Stackable = Stackable;
            this.Walkable = Walkable;
            this.IsSeat = IsSeat;
            this.AllowRecycle = AllowRecycle;
            this.AllowTrade = AllowTrade;
            this.AllowMarketplaceSell = AllowMarketplaceSell;
            this.AllowInventoryStack = AllowInventoryStack;
            this.AllowRotations = AllowRotations;
            this.InteractionType = InteractionType;
            this.Modes = Modes;
            this.VendingIds = new List<int>();
            this.IsGroupItem = (this.Name.ToLower().StartsWith("gld_") || this.Name.ToLower().StartsWith("guild_") || this.Name.ToLower().Contains("grp"));
            this.LimitedStack = LimitedStack;

            if (VendingIds.Contains(","))
            {
                foreach (var VendingId in VendingIds.Split(','))
                {
                    this.VendingIds.Add(int.Parse(VendingId));
                }
            }
            else if (!VendingIds.Equals("") && (int.Parse(VendingIds)) > 0)
            {
                this.VendingIds.Add(int.Parse(VendingIds));
            }

            this.MultiHeight = new List<double>();
            if(multiHeight.Contains(";"))
            {
                foreach (string value in multiHeight.Split(';'))
                {
                    double value2 = Double.Parse(value, OtanixEnvironment.cultureInfo);
                    this.MultiHeight.Add(value2);
                }
            }
        }
    }
}
