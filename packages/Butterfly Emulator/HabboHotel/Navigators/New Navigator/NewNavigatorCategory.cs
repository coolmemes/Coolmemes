using ButterStorm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Navigators
{
    class NewNavigatorCategory
    {
        internal string MainCategory;
        internal string SubCategory;
        internal string Title;
        internal int ViewMode;
        internal bool Collapsed;

        internal NewNavigatorCategory(DataRow Row)
        {
            MainCategory = (string)Row["main_category"];
            SubCategory = (string)Row["sub_category"];
            Title = (string)Row["title"];
            ViewMode = (int)Row["view_mode"];
            Collapsed = OtanixEnvironment.EnumToBool((string)Row["collapsed"]);
        }
    }
}
