using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Misc.API
{
    class InfoModelo
    {
        public string uptime;
        public int hotel,onlines, loadRooms;

        public InfoModelo(int hotel, string uptime, int onlines, int loadRooms)
        {
            this.hotel = hotel;
            this.uptime = uptime;
            this.onlines = onlines;
            this.loadRooms = loadRooms;
        }
    }
}
