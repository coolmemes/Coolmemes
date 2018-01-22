using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Navigator
{
    class NaviLogs
    {
        internal int Id;
        internal string Value1;
        internal string Value2;

        internal NaviLogs(int id, string value1, string value2)
        {
            this.Id = id;
            this.Value1 = value1;
            this.Value2 = value2;
        }
    }
}
