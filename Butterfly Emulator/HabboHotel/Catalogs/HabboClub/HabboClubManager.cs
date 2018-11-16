using System;
using System.Collections.Generic;
using System.Data;
using Butterfly.HabboHotel.Items;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;

namespace Butterfly.Catalogs.HabboClub
{
    public class HabboClubManager
    {
        private readonly Dictionary<int, CatalogClubOffer> _clubOffers;
        private readonly Dictionary<int, CatalogClubGift> _clubGifts;

        public HabboClubManager()
        {
            this._clubOffers = new Dictionary<int, CatalogClubOffer>();
            this._clubGifts = new Dictionary<int, CatalogClubGift>();
        }

        internal void Init(ItemManager ItemManager)
        {
            if (this._clubOffers.Count > 0)
                this._clubOffers.Clear();

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.SetQuery("SELECT * FROM `catalog_club_subscriptions`;");
                DataTable GetClubSubscriptions = dbClient.getTable();

                if (GetClubSubscriptions != null)
                {
                    foreach (DataRow Row in GetClubSubscriptions.Rows)
                    {
                        if (!this._clubOffers.ContainsKey(Convert.ToInt32(Row["id"])))
                            this._clubOffers.Add(Convert.ToInt32(Row["id"]), new CatalogClubOffer(Convert.ToInt32(Row["id"]), Convert.ToString(Row["name"]),
                                Convert.ToString(Row["type"]), Convert.ToInt32(Row["cost_credits"]), Convert.ToInt32(Row["cost_diamonds"]), Convert.ToInt32(Row["length_days"])));
                    }
                }
            }

            if (this._clubGifts.Count > 0)
                this._clubGifts.Clear();

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM `catalog_club_gifts` WHERE `enabled` = 'Y' ORDER BY `days_required` ASC;");
                DataTable GetClubGifts = dbClient.getTable();

                if (GetClubGifts != null)
                {
                    foreach (DataRow Row in GetClubGifts.Rows)
                    {
                        if (!ItemManager.ContainsItem(Convert.ToUInt32(Row["base_id"]), out Item Item))
                        {
                            Console.WriteLine("Failed to load Catalog Club Gift with id: " + Convert.ToInt32(Row["id"]) + " this item will be skipped.");
                            continue;
                        }

                        if (!this._clubGifts.ContainsKey(Convert.ToInt32(Row["id"])))
                            this._clubGifts.Add(Convert.ToInt32(Row["id"]), new CatalogClubGift(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["base_id"]), Convert.ToInt32(Row["days_required"]), Item));
                    }
                }
            }

            Console.WriteLine("Loaded " + this._clubOffers.Count + " catalog subscriptions and " + this._clubGifts.Count + " club gifts.");
        }

        public bool TryGetClubOffer(int ItemId, out CatalogClubOffer Offer)
        {
            return this._clubOffers.TryGetValue(ItemId, out Offer);
        }

        public ICollection<CatalogClubOffer> GetClubOffers()
        {
            return this._clubOffers.Values;
        }

        public ICollection<CatalogClubGift> GetClubGifts()
        {
            return this._clubGifts.Values;
        }
    }
}
