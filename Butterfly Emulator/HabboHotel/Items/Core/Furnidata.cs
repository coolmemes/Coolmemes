using ButterStorm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Items.Core
{
    class Furnidata
    {
        public static List<Furni> Items;

        public static void Initialize()
        {
            Items = new List<Furni>();

            string htmlCode = "";
            if (EmuSettings.FURNIDATA_LINK.StartsWith("Gamedata"))
            {
                StreamReader reader = new StreamReader("Settings/" + EmuSettings.FURNIDATA_LINK);
                htmlCode = reader.ReadToEnd();
            }
            else
            {
                using (WebClient client = new WebClient())
                {
                    htmlCode = client.DownloadString(EmuSettings.FURNIDATA_LINK);
                }
            }

            string[] furnis = htmlCode.Split(']');

            foreach (string furni in furnis)
            {
                try
                {
                    if (furni.Length < 10)
                        continue;

                    uint BaseId = uint.Parse(furni.Split('\"')[3]);
                    string ItemName = furni.Split('\"')[5];
                    string PublicName = furni.Split('\"')[17];

                    Furni f = new Furni(BaseId, ItemName, PublicName);
                    Items.Add(f);
                }
                catch { }
            }
        }
        

        public static string GetPublicNameByBaseId(uint BaseId)
        {
            List<Furni> query = Items.Where(t => t.BaseId == BaseId).ToList();

            if (query.Count() > 0)
                return query[0].PublicName;
            else
                return "";
        }

        public static string GetPublicNameByItemName(string ItemName)
        {
            List<Furni> query = Items.Where(t => t.ItemName == ItemName).ToList();

            if (query.Count() > 0)
                return query[0].PublicName;
            else
                return ItemName;
        }
    }

    class Furni
    {
        public uint BaseId;
        public string ItemName;
        public string PublicName;

        public Furni(uint baseid, string itemname, string publicname)
        {
            this.BaseId = baseid;
            this.ItemName = itemname;
            this.PublicName = publicname;
        }
    }
}
