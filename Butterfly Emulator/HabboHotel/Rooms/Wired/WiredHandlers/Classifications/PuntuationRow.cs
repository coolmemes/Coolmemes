using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Classifications
{
    class PuntuationRow
    {
        internal string names;
        internal uint puntuation;
        internal bool needUpdate;

        internal PuntuationRow(string names, uint puntuation)
        {
            this.names = names;
            this.puntuation = puntuation;
            this.needUpdate = false;
        }
    }
}
