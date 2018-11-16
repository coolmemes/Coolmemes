using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Navigators
{
    class PromCat
    {
        internal int Id;
        internal string Caption;
        internal bool Enabled;

        internal PromCat(int Id, string Caption, bool Enabled)
        {
            this.Id = Id;
            this.Caption = Caption;
            this.Enabled = Enabled;
        }
    }
}
