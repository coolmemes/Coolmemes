using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Butterfly.HabboHotel.Group
{
    class GuildsParts
    {
        /// <summary>
        /// Id de la parte de placa.
        /// </summary>
        internal int Id;

        /// <summary>
        /// Imagen o color de la parte de placa.
        /// </summary>
        internal string ExtraData1;

        /// <summary>
        /// Imagen o color de la parte de placa.
        /// </summary>
        internal string ExtraData2;

        internal GuildsParts(int id, string extradata1, string extradata2)
        {
            this.Id = id;
            this.ExtraData1 = extradata1;
            this.ExtraData2 = extradata2;
        }
    }

    class GuildsPartsManager
    {
        internal static Hashtable htmlBadges1;
        internal static Hashtable htmlBadges2;

        internal static void InitGroups(IQueryAdapter dbClient)
        {
            htmlBadges1 = new Hashtable();
            htmlBadges2 = new Hashtable();

            List<GuildsParts> BaseBadges = new List<GuildsParts>();
            List<GuildsParts> SymbolBadges = new List<GuildsParts>();
            Dictionary<int, GuildsParts> ColorBadges1 = new Dictionary<int, GuildsParts>();
            Dictionary<int, GuildsParts> ColorBadges2 = new Dictionary<int, GuildsParts>();
            Dictionary<int, GuildsParts> ColorBadges3 = new Dictionary<int, GuildsParts>();

            dbClient.setQuery("SELECT * FROM group_badges_part");
            DataTable Row = dbClient.getTable();

            foreach (DataRow Data in Row.Rows)
            {
                if (Data["type"].ToString() == "base")
                {
                    BaseBadges.Add(new GuildsParts(Convert.ToInt32(Data["id"]), (string)Data["code"], (string)Data["code2"]));
                }
                else if (Data["type"].ToString() == "symbol")
                {
                    SymbolBadges.Add(new GuildsParts(Convert.ToInt32(Data["id"]), (string)Data["code"], (string)Data["code2"]));
                }
                else if (Data["type"].ToString() == "base_color")
                {
                    ColorBadges1.Add(Convert.ToInt32(Data["id"]), new GuildsParts(Convert.ToInt32(Data["id"]), (string)Data["code"], (string)Data["code2"]));
                }
                else if (Data["type"].ToString() == "symbol_color")
                {
                    ColorBadges2.Add(Convert.ToInt32(Data["id"]), new GuildsParts(Convert.ToInt32(Data["id"]), (string)Data["code"], (string)Data["code2"]));

                    if (!htmlBadges1.ContainsKey(Convert.ToInt32(Data["id"])))
                        htmlBadges1.Add(Convert.ToInt32(Data["id"]), (string)Data["code"]);
                }
                else if (Data["type"].ToString() == "other_color")
                {
                    ColorBadges3.Add(Convert.ToInt32(Data["id"]), new GuildsParts(Convert.ToInt32(Data["id"]), (string)Data["code"], (string)Data["code2"]));

                    if (!htmlBadges2.ContainsKey(Convert.ToInt32(Data["id"])))
                        htmlBadges2.Add(Convert.ToInt32(Data["id"]), (string)Data["code"]);
                }
            }

            ServerMessage Itemm = new ServerMessage(Outgoing.SendGroupsElements);
            Itemm.AppendInt32(BaseBadges.Count); // Count of items 'Bases'
            foreach (var GD in BaseBadges)
            {
                Itemm.AppendInt32(GD.Id);
                Itemm.AppendString(GD.ExtraData1);
                Itemm.AppendString(GD.ExtraData2);
            }

            Itemm.AppendInt32(SymbolBadges.Count); // count symbols
            foreach (var GD in SymbolBadges)
            {
                Itemm.AppendInt32(GD.Id);
                Itemm.AppendString(GD.ExtraData1);
                Itemm.AppendString(GD.ExtraData2);
            }

            Itemm.AppendInt32(ColorBadges1.Count);
            foreach (var GD in ColorBadges1.Values)
            {
                Itemm.AppendInt32(GD.Id);
                Itemm.AppendString(GD.ExtraData1);
            }

            Itemm.AppendInt32(ColorBadges2.Count);
            foreach (var GD in ColorBadges2.Values)
            {
                Itemm.AppendInt32(GD.Id);
                Itemm.AppendString(GD.ExtraData1);
            }

            Itemm.AppendInt32(ColorBadges3.Count);
            foreach (var GD in ColorBadges3.Values)
            {
                Itemm.AppendInt32(GD.Id);
                Itemm.AppendString(GD.ExtraData1);
            }

            OtanixEnvironment.GetGame().GetCatalog().mGroupPage = Itemm;

            BaseBadges.Clear();
            SymbolBadges.Clear();
            ColorBadges1.Clear();
            ColorBadges2.Clear();
            ColorBadges3.Clear();

            BaseBadges = null;
            SymbolBadges = null;
            ColorBadges1 = null;
            ColorBadges2 = null;
            ColorBadges3 = null;
        }
    }
}
