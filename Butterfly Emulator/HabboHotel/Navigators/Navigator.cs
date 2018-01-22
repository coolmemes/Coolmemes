using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using Butterfly.HabboHotel.Users.Navigator;

namespace Butterfly.HabboHotel.Navigators
{
    class Navigator
    {
        /// <summary>
        /// Categoría de sala. Ejemplo: [AMIGOS], [JUEGOS], [MASCOTAS]...
        /// </summary>
        private Hashtable PrivateCategories;

        /// <summary>
        /// Categoría de evento. Ejemplo: Eventos on fire!, Música y Fiestas...
        /// </summary>
        private Dictionary<int, PromCat> PromotionsCategories;

        internal void Initialize(IQueryAdapter dbClient)
        {
            PrivateCategories = new Hashtable();
            PromotionsCategories = new Dictionary<int, PromCat>();

            dbClient.setQuery("SELECT id,caption,min_rank FROM navigator_flatcats WHERE enabled = '1'");
            var dPrivCats = dbClient.getTable();

            dbClient.setQuery("SELECT * FROM promotion_cats");
            var dPromCats = dbClient.getTable();

            if (dPrivCats != null)
            {
                foreach (DataRow Row in dPrivCats.Rows)
                {
                    PrivateCategories.Add((int)Row["id"], new FlatCat((int)Row["id"], (string)Row["caption"], (int)Row["min_rank"]));
                }
            }

            if (dPromCats != null)
            {
                foreach (DataRow Row in dPromCats.Rows)
                {
                    PromotionsCategories.Add((int)Row["id"], new PromCat((int)Row["id"], (string)Row["name"], OtanixEnvironment.EnumToBool((string)Row["enable"])));
                }

                PromotionsCategories.Reverse();
            }
        }

        internal Hashtable GetPrivateCategories
        {
            get
            {
                return PrivateCategories;
            }
        }

        internal Int32 FlatCatsCount
        {
            get
            {
                return PrivateCategories.Count;
            }
        }

        internal Int32 PromCatsCount
        {
            get
            {
                return PromotionsCategories.Count;
            }
        }

        internal Int32 GetFlatCatIdByName(string FlatName)
        {
            foreach (FlatCat flat in PrivateCategories.Values)
            {
                if (flat.Caption == FlatName)
                    return flat.Id;
            }

            return -1;
        }

        internal FlatCat GetFlatCat(Int32 Id)
        {
            if (PrivateCategories.ContainsKey(Id))
                return (FlatCat)PrivateCategories[Id];

            return null;
        }

        internal ServerMessage SerializeFlatCategories(GameClient Session)
        {
            var Cats = new ServerMessage(Outgoing.FlatCats);
            Cats.AppendInt32(PrivateCategories.Count);

            foreach (FlatCat FlatCat in PrivateCategories.Values)
            {
                Cats.AppendInt32(FlatCat.Id);
                Cats.AppendString(FlatCat.Caption);
                Cats.AppendBoolean(FlatCat.MinRank <= Session.GetHabbo().Rank);
                Cats.AppendBoolean(false); // ??
                Cats.AppendString("NONE");
                Cats.AppendString("");
                Cats.AppendBoolean(false); // ??
            }

            return Cats;
        }

        internal ServerMessage SerializePromotionsCategories()
        {
            var Promotions = new ServerMessage(Outgoing.PromotionsCategories);
            Promotions.AppendInt32(PromotionsCategories.Count);

            foreach (PromCat prom in PromotionsCategories.Values)
            {
                Promotions.AppendInt32(prom.Id);
                Promotions.AppendString(prom.Caption);
                Promotions.AppendBoolean(prom.Enabled);
            }

            return Promotions;
        }

        public List<RoomData> SerializeNavigatorPopularRoomsNews(ref ServerMessage reply, List<RoomData> rooms, int Category, bool direct)
        {
            List<RoomData> roomsCategory = new List<RoomData>();
            foreach (var pair in rooms)
            {
                if (pair != null && pair.Category.Equals(Category))
                {
                    roomsCategory.Add(pair);
                    if (roomsCategory.Count == (direct ? 40 : 8))
                        break;
                }
            }

            return roomsCategory;
        }
    }
}
