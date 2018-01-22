using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Alfas.Manager
{
    enum BullyState
    {
        SEARCHING_USER,
        WAITING_RESPONSE,
        FINISHED
    }

    enum BullySolution
    {
        NONE = -1,
        ACCEPTABLE = 0,
        BULLY = 1,
        HORROR =2,
        EXIT = 3,
        RELOAD = 4
    }
}
