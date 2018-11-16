using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Premiums.Catalog
{
    class CatalogPremiumItem
    {
        public readonly uint Id;
        public readonly uint BaseId;
        public readonly string Name;
        public readonly uint PageID;
        public readonly bool HaveOffer;

        public CatalogPremiumItem(DataRow Row)
        {
            this.Id = Convert.ToUInt32(Row["id"]);
            this.BaseId = Convert.ToUInt32(Row["base_id"]);
            this.Name = (string)Row["catalog_name"];
            this.PageID = Convert.ToUInt32(Row["page_id"]);
            this.HaveOffer = OtanixEnvironment.EnumToBool(Row["offer_active"].ToString());
        }

        public Item GetBaseItem(uint BaseId)
        {
            Item Return = OtanixEnvironment.GetGame().GetItemManager().GetItem(BaseId);
            if (Return == null)
            {
                if (BaseId != 0)
                    Console.WriteLine(@"UNKNOWN BaseId: " + BaseId);
            }

            return Return;
        }

        public void Serialize(ServerMessage Message)
        {
            Message.AppendUInt(Id);
            Message.AppendString(Name);
            Message.AppendBoolean(false);
            Message.AppendUInt(0);
            Message.AppendUInt(0);
            Message.AppendInt32(0);
            Message.AppendBoolean(false);
            Message.AppendInt32(1); // items on pack
            Message.AppendString(GetBaseItem(BaseId).Type.ToString());
            Message.AppendInt32(GetBaseItem(BaseId).SpriteId);
            Message.AppendString(string.Empty);
            Message.AppendUInt(1);
            Message.AppendBoolean(false); // IsLimited
            Message.AppendInt32(0); // club_level
            Message.AppendBoolean(HaveOffer); // IsOffer
            Message.AppendBoolean(false); // aun nada
            Message.AppendString(""); // previewImage
        }
    }
}
