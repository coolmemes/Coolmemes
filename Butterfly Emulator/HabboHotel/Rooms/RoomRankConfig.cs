using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButterStorm.HabboHotel.Rooms
{
    class RoomRankConfig
    {
        internal List<int> ROOMS_TO_MODIFY;
        internal int BOTS_DEFAULT_COLOR;
        internal String BOTS_DEFAULT_BADGE;

        internal void Initialize()
        {
            ROOMS_TO_MODIFY = new List<int>();

            var roomWithsColors = EmuSettings.GetConfig().data["cant.modify.rooms"];
            if (roomWithsColors.Length > 0)
            {
                var v = roomWithsColors.Split(',');
                for (var i = 0; i < v.Length; i++)
                    ROOMS_TO_MODIFY.Add(Int32.Parse(v[i]));
            }

            BOTS_DEFAULT_COLOR = Int32.Parse(EmuSettings.GetConfig().data["game.botdefaultcolor"]);
            BOTS_DEFAULT_BADGE = EmuSettings.GetConfig().data["game.botbadge"];
        }
    }
}
