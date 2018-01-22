
using Butterfly.HabboHotel.Items;
using ButterStorm;

namespace Butterfly.HabboHotel.Catalogs
{
    class EcotronReward
    {
        internal uint DisplayId;
        internal uint BaseId;
        internal uint RewardLevel;

        internal EcotronReward(uint DisplayId, uint BaseId, uint RewardLevel)
        {
            this.DisplayId = DisplayId;
            this.BaseId = BaseId;
            this.RewardLevel = RewardLevel;
        }

        internal Item GetBaseItem()
        {
            return OtanixEnvironment.GetGame().GetItemManager().GetItem(BaseId);
        }
    }
}
