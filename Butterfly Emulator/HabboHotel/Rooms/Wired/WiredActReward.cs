using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired
{
    class WiredActReward
    {
        internal UInt32 ItemId;
        internal Double LastUpdate;
        internal Int32 ActualRewards;
        internal Int32 OriginalInt;

        internal WiredActReward(UInt32 itemId, Double lastUpdate, Int32 actualRewards, Int32 originalInt)
        {
            this.ItemId = itemId;
            this.LastUpdate = lastUpdate;
            this.ActualRewards = actualRewards;
            this.OriginalInt = originalInt;
        }
    }
}
