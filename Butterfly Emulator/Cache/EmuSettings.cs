using Butterfly.Core;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ButterStorm
{
    class EmuSettings
    {
        private static ConfigurationData Configuration;

        // HOTEL
        internal static string HOTEL_LINK;
        internal static string PIN_CLIENTE;
        //internal static int HOTEL_ID;
        internal static string TILE_STACKING_VALUE;
        internal static string FURNIDATA_LINK;
        internal static string FIGUREDATA_LINK;
        internal static bool EXCHANGE_DIAMONDS;
        internal static bool CRYPTO_CLIENT_SIDE;
        internal static bool SHOW_PACKETS;
        internal static bool HOTEL_LUCRATIVO;
        internal static bool HOTEL_LUCRATIVO_DARMOEDAS;
        internal static int HOTEL_LUCRATIVO_QUANTIDADE_MOEDAS;
        internal static int HOTEL_LUCRATIVO_MOEDAS_TEMPO;
        internal static bool HOTEL_LUCRATIVO_ALERTAR_GANHOU_MOEDAS;

        // CAMERA
        internal static string CAMERA_SERVER_LOAD;
        internal static uint CAMERA_BASEID;

        // CHATS IDS
        internal static List<int> CHAT_TYPES_USERS;
        internal static uint CHAT_USER_ID;

        // ANTI SPAM CONFIGURATION
        internal static int CHECK_EVERY;
        internal static int SENDINGS_TO_WHITE_LIST;
        internal static int PROCESS_FOR_X_MINUTES;

        // FRIENDS LIMITS
        internal static int FRIENDS_REQUEST_LIMIT;
        internal static int FRIENDS_LIMIT;

        // INVENTARY LIMITS
        internal static uint INVENTARY_ITEMS_LIMIT;
        internal static int JUKEBOX_CD_BASEID;

        // SHOP CONFIGURATION
        internal static uint FIRST_PRESENT_ID;
        internal static uint LAST_PRESENT_SPRITEID;
        internal static uint FIRST_BALLOON_PRESENT_ID;
        internal static uint LAST_BALLOON_PRESENT_ID;
        internal static Int32 CLUB_PAGE_ID;
        internal static Int32 FURNIMATIC_BOX_ITEMID;
        internal static int[] PET_FOODS;

        // MAXIMUMS
        internal static int MAX_NAME_CHANGES;
        internal static int MAX_BOTS_PER_ROOM;
        internal static int MAX_BOTS_MESSAGES;
        internal static int MAX_PETS_PER_ROOM;

        // BOOLS AND URLS
        internal static bool LOG_EXCEPTIONS;

        // CATALOG
        internal static List<int> CATALOG_NOSEARCH_PAGES;

        // DIAMONDS
        internal static bool DIAMONDS_ENABLED;
        internal static uint DIAMONDS_AMOUNT;
        internal static uint DIAMONDS_MINUTES;
        internal static bool DIAMONDS_ALERT;

        // HORSE
        internal static uint HORSECHAIR1;
        internal static uint HORSECHAIR2;

        // NAVIGATOR
        internal static string NAVIGATOR_STAFF_SELECTION;
        internal static int ROOMS_X_PESTAÑA;

        // PREMIUM
        internal static uint PREMIUM_BASEID;

        // IMAGES
        internal static string EVENTHA_ICON;

        // PRISAO
        internal static uint PRISAOID;

        internal static void Initialize(IQueryAdapter dbClient)
        {
            Configuration = new ConfigurationData(Path.Combine(Application.StartupPath, @"Settings/values.ini"));

            HOTEL_LINK = Configuration.data["hotel.link"];
            PIN_CLIENTE = Configuration.data["pin.client"];
            //HOTEL_ID = int.Parse(Configuration.data["hotel.id"]);
            TILE_STACKING_VALUE = Configuration.data["tile.stacking.value"];
            FURNIDATA_LINK = Configuration.data["hotel.furnidata"];
            FIGUREDATA_LINK = Configuration.data["hotel.figuredata"];
            EXCHANGE_DIAMONDS = Configuration.data["exchange.diamonds"].ToLower() == "true";
            CRYPTO_CLIENT_SIDE = Configuration.data["crypto.client.side"].ToLower() == "true";
            SHOW_PACKETS = Configuration.data["show.packets"].ToLower() == "true";
            HOTEL_LUCRATIVO = Configuration.data["hotel.lucrativo"].ToLower() == "true";
            HOTEL_LUCRATIVO_ALERTAR_GANHOU_MOEDAS = Configuration.data["hotel.lucrativo.alertar.ganhou.moedas"].ToLower() == "true";
            HOTEL_LUCRATIVO_DARMOEDAS = Configuration.data["hotel.lucrativo.darmoedas"].ToLower() == "true";
            HOTEL_LUCRATIVO_MOEDAS_TEMPO = int.Parse(Configuration.data["hotel.lucrativo.quantidade.tempo.minutos"]);
            HOTEL_LUCRATIVO_QUANTIDADE_MOEDAS = int.Parse(Configuration.data["hotel.lucrativo.quantidade.moedas"]);

            // CAMERA
            CAMERA_SERVER_LOAD = Configuration.data["camera.server.load"];
            CAMERA_BASEID = uint.Parse(Configuration.data["camera.baseid"]);

            // CHATS IDS
            CHAT_TYPES_USERS = new List<int>();
            string chattypesusers = Configuration.data["chat_types_users"];
            foreach (string value in chattypesusers.Split(','))
            {
                if (!CHAT_TYPES_USERS.Contains(int.Parse(value)))
                    CHAT_TYPES_USERS.Add(int.Parse(value));
            }
            CHAT_USER_ID = uint.Parse(Configuration.data["chat_user_id"]);

            // ANTI SPAM CONFIGURATION
            CHECK_EVERY = int.Parse(Configuration.data["checkEveryXMessages"]);
            SENDINGS_TO_WHITE_LIST = int.Parse(Configuration.data["whitelistAfterXSendings"]);
            PROCESS_FOR_X_MINUTES = int.Parse(Configuration.data["FullProcessingForXMinutes"]);

            // FRIENDS LIMITS
            FRIENDS_REQUEST_LIMIT = Int32.Parse(Configuration.data["client.maxrequests"]);
            FRIENDS_LIMIT = Int32.Parse(Configuration.data["client.maxpossiblefriends"]);

            // INVENTARY LIMITS
            INVENTARY_ITEMS_LIMIT = uint.Parse(Configuration.data["client.maxitemsininventary"]);

            // MAXIMUMS
            MAX_NAME_CHANGES = Int32.Parse(Configuration.data["game.maxnamechanges"]);
            MAX_BOTS_PER_ROOM = Int32.Parse(Configuration.data["max.bots.room"]);
            MAX_BOTS_MESSAGES = Int32.Parse(Configuration.data["max.bots.messages"]);
            MAX_PETS_PER_ROOM = Int32.Parse(Configuration.data["max.pets.room"]);

            // BOOLS AND URLS
            LOG_EXCEPTIONS = Configuration.data["logExceptions"].ToLower() == "true";
            PRISAOID = uint.Parse(Configuration.data["prisaoId"]);

            // EXTRA INFO
            FIRST_PRESENT_ID = UInt32.Parse(Configuration.data["first.present.wrap.id"]);
            LAST_PRESENT_SPRITEID = UInt32.Parse(Configuration.data["last.present.wrap.id"]);
            FIRST_BALLOON_PRESENT_ID = UInt32.Parse(Configuration.data["first.balloon.present.id"]);
            LAST_BALLOON_PRESENT_ID = UInt32.Parse(Configuration.data["last.balloon.present.id"]);
            CLUB_PAGE_ID = Int32.Parse(Configuration.data["club.hc.page"]);

            dbClient.setQuery("SELECT item_id FROM items_base WHERE item_name = 'ecotron_box' LIMIT 1");
            FURNIMATIC_BOX_ITEMID = dbClient.getInteger();

            dbClient.setQuery("SELECT item_id FROM items_base WHERE item_name = 'song_disk' LIMIT 1");
            JUKEBOX_CD_BASEID = dbClient.getInteger();

            // CATALOGUE
            CATALOG_NOSEARCH_PAGES = new List<int>();
            if (Configuration.data["catalog.nosearch.pages"].Length > 0)
            {
                foreach (string value in Configuration.data["catalog.nosearch.pages"].Split(','))
                {
                    CATALOG_NOSEARCH_PAGES.Add(Int32.Parse(value));
                }
            }

            // DIAMONDS
            DIAMONDS_ENABLED = Configuration.data["diamonds.enable"].ToLower() == "true";
            DIAMONDS_AMOUNT = UInt32.Parse(Configuration.data["diamonds.amount"]);
            DIAMONDS_MINUTES = UInt32.Parse(Configuration.data["diamonds.minutes"]);
            DIAMONDS_ALERT = Configuration.data["diamonds.alert"].ToLower() == "true";

            // HORSE
            dbClient.setQuery("SELECT item_id FROM items_base WHERE item_name = 'horse_saddle1' LIMIT 1");
            HORSECHAIR1 = (uint)dbClient.getInteger();

            dbClient.setQuery("SELECT item_id FROM items_base WHERE item_name = 'horse_saddle2' LIMIT 1");
            HORSECHAIR2 = (uint)dbClient.getInteger();

            PET_FOODS = new int[38];

            // NAVIGATOR
            NAVIGATOR_STAFF_SELECTION = Configuration.data["navigator.selection.staff"];
            ROOMS_X_PESTAÑA = Int32.Parse(Configuration.data["rooms.x.tab"]);

            // PREMIUM
            PREMIUM_BASEID = UInt32.Parse(Configuration.data["premium.baseid"]);

            EVENTHA_ICON = Configuration.data["eventha.icon"];

            if (OtanixEnvironment.GetGame() != null)
                OtanixEnvironment.GetGame().GetRoomRankConfig().Initialize();
        }

        internal static int GetPetFood(int PetType)
        {
            if (PET_FOODS[PetType] > 0)
                return PET_FOODS[PetType];

            using(IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT item_id FROM items_base WHERE item_name = 'petfood" + PetType + "' LIMIT 1");
                int itemId = dbClient.getInteger();
                PET_FOODS[PetType] = itemId;
                return itemId;
            }
        }

        internal static ConfigurationData GetConfig()
        {
            return Configuration;
        }
    }
}
