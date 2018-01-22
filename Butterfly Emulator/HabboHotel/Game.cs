// Otanix emulador [O melhor do melhores] By: Thiago Araujo
using System;
using System.Collections.Generic;
using System.Data;
using Butterfly.Core;
using Butterfly.HabboHotel.Achievements;
using Butterfly.HabboHotel.Catalogs;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.Navigators;
using Butterfly.HabboHotel.News;
using Butterfly.HabboHotel.Roles;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Support;
using Butterfly.HabboHotel.Pets;
using Butterfly.Messages.ClientMessages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Threading;
using Butterfly.HabboHotel.Quests;
using Butterfly.HabboHotel.SoundMachine;
using Butterfly.HabboHotel.Group;
using ButterStorm.HabboHotel.Items;
using ButterStorm.HabboHotel.Rooms;
using System.Diagnostics;
using Butterfly.HabboHotel.Rooms.Polls;
using Butterfly.Util;
using System.Collections;
using Butterfly.HabboHotel.Users.Talents;
using Butterfly.HabboHotel.Alfas;
using Database_Manager.Database.Session_Details;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Butterfly.HabboHotel.Users.Badges;
using ButterStorm.HabboHotel.Misc;
using Butterfly.HabboHotel.Mutes;
using Butterfly.HabboHotel.Users.Messenger;
using Butterfly.HabboHotel.Items.Core;
using Butterfly.HabboHotel.Navigators.RoomQueue;
using Butterfly.HabboHotel.Items.Craftable;
using Butterfly.HabboHotel.Premiums.Catalog;
using Butterfly.HabboHotel.Premiums.Clothes;
using Butterfly.HabboHotel.Users.Clothing;
using Butterfly.HabboHotel.ChatMessageStorage;
using Butterfly.HabboHotel.Users.Prisao;
using Butterfly.HabboHotel.Navigators.Bonus;
using Butterfly.HabboHotel.Navigators.Landing;
using Butterfly.HabboHotel.ChatMessageStorage.ChatColorido;
using Butterfly.HabboHotel.Users.Gifts;

namespace Butterfly.HabboHotel
{
    class Game
    {
        #region Fields
        private readonly GameClientManager ClientManager;
        private readonly ModerationBanManager BanManager;
        private readonly RoleManager RoleManager;
        private readonly Catalog Catalog;
        private readonly CatalogPremium CatalogPremium;
        private readonly Navigator Navigator;
        private readonly NewNavigatorManager NewNavigatorManager;
        private readonly ItemManager ItemManager;
        private readonly RoomManager RoomManager;
        private readonly AchievementManager AchievementManager;
        private readonly ModerationTool ModerationTool;
        private readonly QuestManager questManager;
        private readonly GroupManager GroupManager;
        private readonly NewsManager newsManager;
        private readonly YoutubeManager youtubeManager;
        private readonly PiñataHandler piñataManager;
        private readonly RoomRankConfig roomRankConfig;
        private readonly TalentManager talentManager;
        private readonly AlfaManager AlfaManager;
        private readonly MuteManager MuteManager;
        private readonly PromotionalBadges PromotionalBadgesManager;
        private readonly TargetedOfferManager TargetedOfferManager;
        private readonly RoomQueueManager RoomQueueManager;
        private readonly CraftableProductsManager CraftableProductsManager;
        private readonly ClothingManager ClothingManager;
        private readonly UserLook UserLookManager;
        private readonly PrisaoManager PrisaoManager;
        private readonly LandingTopUsers LandingTopUsersManager;
        private readonly CorManager corManager;
        private readonly GiftModeloManager giftManager;

        private Task gameLoop;
        internal bool gameLoopActive;
        private const int gameLoopSleepTime = 25;

        public uint RoomIdEvent = 0;
        #endregion

        #region Return values
        internal GameClientManager GetClientManager()
        {
            return ClientManager;
        }

        internal GiftModeloManager GetGiftManager()
        {
            return giftManager;
        }

        internal ModerationBanManager GetBanManager()
        {
            return BanManager;
        }

        internal RoleManager GetRoleManager()
        {
            return RoleManager;
        }

        internal Catalog GetCatalog()
        {
            return Catalog;
        }

        internal CatalogPremium GetCatalogPremium()
        {
            return CatalogPremium;
        }

        internal Navigator GetNavigator()
        {
            return Navigator;
        }

        internal NewNavigatorManager GetNewNavigatorManager()
        {
            return NewNavigatorManager;
        }

        internal ItemManager GetItemManager()
        {
            return ItemManager;
        }

        internal RoomManager GetRoomManager()
        {
            return RoomManager;
        }

        internal AchievementManager GetAchievementManager()
        {
            return AchievementManager;
        }

        internal ModerationTool GetModerationTool()
        {
            return ModerationTool;
        }

        internal QuestManager GetQuestManager()
        {
            return questManager;
        }

        internal GroupManager GetGroup()
        {
            return GroupManager;
        }

        internal NewsManager GetNewsManager()
        {
            return newsManager;
        }

        internal YoutubeManager GetYoutubeManager()
        {
            return youtubeManager;
        }

        internal PiñataHandler GetPiñataManager()
        {
            return piñataManager;
        }

        internal RoomRankConfig GetRoomRankConfig()
        {
            return roomRankConfig;
        }

        internal TalentManager GetTalentManager()
        {
            return talentManager;
        }

        internal AlfaManager GetAlfaManager()
        {
            return AlfaManager;
        }

        internal MuteManager GetMuteManager()
        {
            return MuteManager;
        }

        internal PromotionalBadges GetPromotionalBadges()
        {
            return PromotionalBadgesManager;
        }

        internal TargetedOfferManager GetTargetedOfferManager()
        {
            return TargetedOfferManager;
        }

        internal RoomQueueManager GetRoomQueueManager()
        {
            return RoomQueueManager;
        }

        internal CraftableProductsManager GetCraftableProductsManager()
        {
            return CraftableProductsManager;
        }


        internal ClothingManager GetClothingManager()
        {
            return ClothingManager;
        }

        internal UserLook GetUserLookManager()
        {
            return UserLookManager;
        }

        internal PrisaoManager GetPrisaoManager()
        {
            return PrisaoManager;
        }

        internal LandingTopUsers GetLandingTopUsersManager()
        {
            return LandingTopUsersManager;
        }

        internal CorManager CorManager()
        {
            return corManager;
        }
        #endregion

        #region Boot
        internal Game()
        {
            ClientManager = new GameClientManager();
            BanManager = new ModerationBanManager();
            RoleManager = new RoleManager();
            Catalog = new Catalog();
            CatalogPremium = new CatalogPremium();
            Navigator = new Navigator();
            NewNavigatorManager = new NewNavigatorManager();
            ItemManager = new ItemManager();
            RoomManager = new RoomManager();
            GroupManager = new GroupManager();
            newsManager = new NewsManager();
            ModerationTool = new ModerationTool();
            questManager = new QuestManager();
            youtubeManager = new YoutubeManager();
            piñataManager = new PiñataHandler();
            roomRankConfig = new RoomRankConfig();
            AchievementManager = new AchievementManager();
            talentManager = new TalentManager();
            AlfaManager = new AlfaManager();
            MuteManager = new MuteManager();
            PromotionalBadgesManager = new PromotionalBadges();
            TargetedOfferManager = new TargetedOfferManager();
            RoomQueueManager = new RoomQueueManager();
            CraftableProductsManager = new CraftableProductsManager();
            ClothingManager = new ClothingManager();
            UserLookManager = new UserLook();
            PrisaoManager = new PrisaoManager();
            LandingTopUsersManager = new LandingTopUsers();
            corManager = new CorManager();
            giftManager = new GiftModeloManager();
        }

        internal void ContinueLoading()
        {
            DateTime Start;
            TimeSpan TimeUsed;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                EmuSettings.Initialize(dbClient);

                Start = DateTime.Now;
                Ranks.LoadMaxRankId(dbClient);
                Catalog.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Catacache -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                Filter.Filter.Initialize();
                Filter.BlackWordsManager.Load(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Filtro -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                BanManager.LoadBans(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente de Ban -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                newsManager.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Notícia -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                ItemManager.LoadItems(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente de item -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                Furnidata.Initialize();
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente de Furnidata -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                PromotionalBadgesManager.loadPromotionalBadges(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Emblemas promocionais -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                RoleManager.LoadRanks(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente de papel -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                Navigator.Initialize(dbClient);
                NewNavigatorManager.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Navegador -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                RoomManager.LoadModels(dbClient);
                RoomManager.InitVotedRooms(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente do quarto -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                AchievementManager.Initialize(dbClient);
                questManager.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente de realização -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                ModerationTool.LoadMessagePresets(dbClient);
                ModerationTool.LoadModActions(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Ferramenta de moderação-> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                PetRace.Init(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Sistema de estimação-> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                PetOrders.Init(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Ordens para animais -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                GuildsPartsManager.InitGroups(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Elementos dos grupos -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                Catalog.InitCache();
                CatalogPremium.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente de catálogo -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                SongManager.Initialize();
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente de som -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                youtubeManager.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Youtube TV Manager -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                piñataManager.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente Piñata -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                talentManager.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente de talentos -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                TargetedOfferManager.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente da oferta direta -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                CraftableProductsManager.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente de Produtos Crafáveis -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                ClothingManager.Initialize(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Gerente de vestuário-> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                Start = DateTime.Now;
                PrisaoManager.Init(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Usuários presos -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");

                LandingTopUsersManager.Initialize(dbClient);
                LandingTopUsersManager.Load();
                giftManager.init(dbClient);
                SpyChatMessage.Initialize();
                StaffChat.Initialize(dbClient);
                BonusManager.Initialize(dbClient);
                roomRankConfig.Initialize();

                Start = DateTime.Now;
                DatabaseCleanup(dbClient);
                TimeUsed = DateTime.Now - Start;
                Logging.WriteLine("[Otanix] @ Database -> Limpeza realizada! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");
            }

            StartGameLoop();

            Logging.WriteLine("[Otanix] @ Gerente de jogo -> PRONTO! ");
        }
        #endregion

        #region Game loop
        internal void StartGameLoop()
        {
            gameLoopActive = true;
            gameLoop = new Task(MainGameLoop);
            gameLoop.Start();
        }

        internal void StopGameLoop()
        {
            gameLoopActive = false;
        }

        private void MainGameLoop()
        {
            while (gameLoopActive)
            {
                try
                {
                    LowPriorityWorker.Process();
                    ClientManager.OnCycle();
                    RoomManager.OnCycle();

                    GroupManager.OnCycle();
                    AlfaManager.OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogCriticalException("[Otanix] @ Alerta de erro: ERRO MARIO INVALIDO NO LOBO DO JOGO: " + e.StackTrace + " - " + e.Message + " - " + e);
                }

                Thread.Sleep(gameLoopSleepTime);
            }
        }
        #endregion

        #region Shutdown
        internal static void DatabaseCleanup(IQueryAdapter dbClient)
        {
            if (!Debugger.IsAttached)
            {
                dbClient.runFastQuery("DELETE FROM users_online");
                dbClient.runFastQuery("DELETE FROM user_tickets");
            }
        }

        internal void Destroy()
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                DatabaseCleanup(dbClient);
            }

            Console.WriteLine("Destruido Habbo Hotel.");
        }
        #endregion
    }
}
