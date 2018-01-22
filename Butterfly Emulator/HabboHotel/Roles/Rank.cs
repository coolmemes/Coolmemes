using ButterStorm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Roles
{
    class Rank
    {
        private uint RankId;
        private string RankName, corNick, tagNick;
        private Dictionary<string, bool> Fuses;

        public Rank(DataRow dRow)
        {
            this.RankId = Convert.ToUInt32(dRow["id"]);
            this.RankName = (string)dRow["name"];
            this.corNick = Convert.ToString(dRow["nick_color"]);
            this.tagNick = Convert.ToString(dRow["nick_tag"]);
            this.Fuses = new Dictionary<string, bool>();

            foreach (DataColumn c in dRow.Table.Columns)
            {
                if (c.ColumnName.Equals("id") || c.ColumnName.Equals("name") || c.ColumnName.Equals("nick_color") || c.ColumnName.Equals("nick_tag"))
                    continue;

                this.Fuses.Add(c.ColumnName, OtanixEnvironment.EnumToBool((string)dRow[c.ColumnName]));
            }
        }

        public bool HasFuse(string Fuse)
        {
            if (this.Fuses.ContainsKey(Fuse))
                return this.Fuses[Fuse];

            return false;
        }

        public string GetHtmlName()
        {
            if (string.IsNullOrEmpty(tagNick) || string.IsNullOrEmpty(corNick))
                return "";

            return "[<font color=\"" + corNick + "\">" + tagNick + "</font>] ";
        }
    }
}
