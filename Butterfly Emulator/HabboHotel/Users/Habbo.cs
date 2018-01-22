using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Butterfly.Core;
using Butterfly.HabboHotel.Achievements;
using Butterfly.HabboHotel.ChatMessageStorage;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Users.Badges;
using Butterfly.HabboHotel.Users.Inventory;
using Butterfly.HabboHotel.Users.Messenger;
using Butterfly.Messages;
using ButterStorm;
using Butterfly.HabboHotel.Users.UserDataManagement;
using HabboEvents;
using Butterfly.HabboHotel.Users.Relationships;
using ButterStorm.HabboHotel.Users.Inventory;
using Butterfly.HabboHotel.Group;
using Butterfly.HabboHotel.Users.Talents;
using Butterfly.HabboHotel.Alfas;
using Butterfly.HabboHotel.Rooms.Wired;
using Butterfly.HabboHotel.Alfas.Manager;
using Butterfly.HabboHotel.Users.Navigator;
using Butterfly.HabboHotel.Pets;
using Butterfly.HabboHotel.Users.Chat;
using Butterfly.HabboHotel.Premiums.Users;
using Butterfly.HabboHotel.Premiums;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Users.Clothing;

namespace Butterfly.HabboHotel.Users
{
    partial class Habbo
    {
        #region Habbo Variables
        // Database Settings:
        internal uint Id;
        internal string Username;
        internal string RealName;
        internal uint Rank;
        internal uint Diamonds;
        internal int Moedas;
        internal string Look;
        internal string BackupLook;
        internal bool LastMovFGate;
        internal bool frankJaApareceu;
        internal string Gender;
        internal string BackupGender;
        internal string Motto;
        internal double LastOnline;
        internal string MachineId;
        internal uint HomeRoom;
        internal uint Respect;
        internal uint DailyRespectPoints;
        internal uint DailyPetRespectPoints;
        internal bool HasFriendRequestsDisabled;
        internal bool FollowEnable;
        internal bool BlockTrade;
        internal uint AchievementPoints;
        internal uint FavoriteGroup;
        internal uint NameChanges;
        internal uint CurrentQuestId;
        internal uint LastCompleted;
        internal uint CurrentQuestProgress;
        internal uint LastQuestId;
        internal string volumenSystem;
        internal DateTime Created;
        internal bool preferOldChat;
        internal bool IgnoreRoomInvitations;
        internal string LastPurchase;
        internal List<UInt32> PollParticipation;
        internal DateTime LastFollowingLogin;
        internal int CitizenshipLevel;
        internal int HelperLevel;
        internal Dictionary<uint, WiredActReward> WiredRewards;
        internal bool AlfaGuideEnabled;
        internal bool AlfaHelperEnabled;
        internal bool AlfaGuardianEnabled;
        internal double tamanhoChao = 0;
        internal bool AlfaServiceEnabled
        {
            get
            {
                if (!AlfaGuideEnabled && !AlfaHelperEnabled && !AlfaGuardianEnabled)
                    return false;
                
                return true;
            }
        }
        internal uint AlfaServiceId;
        internal bool AlfaHelpEnabled;
        internal uint HabboAlfaUserId;
        internal double LastAlfaSend;
        internal bool DontFocusUser;
        internal Dictionary<int, NaviLogs> navigatorLogs;
        internal double DiamondsCycleUpdate;
        internal double MoedasCycleUpdate;
        internal int UltimaNotificacao = 0;
        internal bool exibeNotifi = true;
        internal Dictionary<uint, UInt32> TargetedOffers;
        internal string LastMessage;
        internal uint LastMessageCount;
        internal double LastChangeLookTime;
        internal double LastChangePetTime;
        internal string ChatColor;
        internal int NewIdentity;
        internal int NewBot;
        internal int NewUserInformationStep;
        internal int tentativasLogin = 0;
        internal bool _passouPin = false;

        internal uint CoinsPurchased;

        // Extra Settings:
        internal uint comingRoom;
        internal uint roomIdQueue;
        internal uint goToQueuedRoom;
        internal string lastPhotoPreview;
        internal int ultimaFotoComprada;
        internal bool ownRoomsSerialized = false;
        internal bool notifyOnRoomEnter = true;
        //internal Boolean centralHotelView = false;
        internal Stopwatch PresentBuyStopwatch;
        internal uint LoadingRoom;
        internal bool LoadingChecksPassed;
        internal uint CurrentRoomId;
        internal bool IsTeleporting;
        internal uint TeleportingRoomID;
        internal uint TeleporterId;
        internal bool alertasAtivados = true;
        internal int corAtual;
        internal string coresjaTenho;
        internal bool SpectatorMode;
        internal bool Disconnected;
        internal DateTime spamFloodTime;
        internal int publicHotelCount = 0;
        internal int curseHotelCount = 0;
        internal int skippedTickets = 0;
        internal int readTickets = 0;
        internal DateTime lastTicketRead;
        internal uint onlineTimeInRooms = 0;
        private int friendsCount = -1;
        internal bool showingStaffBadge;
        internal uint DiceNumber;
        internal bool ConvertedOnPet;
        internal int PetType;
        internal string PetData
        {
            get
            {
                return PetType + " " + PetRace.RandomRace(PetType) + " FFFFFF" + (PetType == 26 ? Catalogs.Catalog.GenerateRandomGnomeLook() : "");
            }
        }

        public bool tenhoCor(int corID)
        {
            foreach (string cor in coresjaTenho.Split(';'))
                if (corID == Convert.ToInt32(cor))
                    return true;

            return false;
        }

        public string minhasCores()
        {
            return coresjaTenho;
        }
        internal int UltimaFotoComprada
        {
            get
            {
                return ultimaFotoComprada;
            }
            set
            {
                ultimaFotoComprada = value;
            }
        }
        internal int FriendsCount
        {
            get
            {
                if (friendsCount == -1)
                    _LoadFriendsCount();

                return friendsCount;
            }
        }

        internal List<uint> UsersRooms;
        internal LinkedList<RoomVisits> RoomsVisited;
        internal List<uint> FavoriteRooms;
        internal List<string> MutedUsers;
        internal List<uint> RatedRooms;
        internal List<int> HabboQuizQuestions;
        internal uint CurrentRingId;

        // Stability Method:
        internal Dictionary<string, UserAchievement> Achievements;
        internal bool AchievementsLoaded;
        private BadgeComponent BadgeComponent;
        internal bool BadgeComponentLoaded;
        internal Dictionary<uint, int> quests;
        internal bool QuestsLoaded;
        internal Dictionary<int, Wardrobe> wardrobes;
        internal bool WardrobeLoaded;
        internal bool RelationsLoaded;

        // Clases:
        private HabboMessenger Messenger;
        private InventoryComponent InventoryComponent;
        private AvatarEffectsInventoryComponent AvatarEffectsInventoryComponent;
        private RelationshipComposer RelationshipComposer;
        private ChatMessageManager chatMessageManager;
        private Premium premiumManager;
        private UserClothing userclothingManager;
        // private ChatSettingsManager chatSettingsManager;

        private List<uint> mygroups;
        private bool LoadedMyGroups;
        internal List<uint> MyGroups
        {
            get
            {
                if (!LoadedMyGroups)
                    _LoadMyGroups();

                return mygroups;
            }
        }


        private GameClient mClient
        {
            get
            {
                return OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Id);
            }
        }

        internal void setMeOnline()
        {
            //loginTime = DateTime.Now;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("REPLACE INTO users_online VALUES ('" + Id + "')");
            }
        }

        internal bool InRoom
        {
            get
            {
                if (CurrentRoomId >= 1)
                {
                    return true;
                }

                return false;
            }
        }

        internal Room CurrentRoom
        {
            get
            {
                if (CurrentRoomId <= 0)
                {
                    return null;
                }

                return OtanixEnvironment.GetGame().GetRoomManager().GetRoom(CurrentRoomId);
            }
        }

        internal bool passouPin
        {
            get
            {
                return Rank > 5 ? _passouPin : true;
            }
            set
            {
                this._passouPin = value;
            }
        }
        #endregion

        #region Creating Habbo
        internal Habbo(uint Id, string Username, string RealName, uint Rank, string Motto, double UnixTime, string Look, string Gender, uint Diamonds,
            uint HomeRoom, uint Respect, uint DailyRespectPoints, uint DailyPetRespectPoints, bool HasFriendRequestsDisabled, bool FollowEnable, uint currentQuestID, uint currentQuestProgress, uint achievementPoints,
            uint nameChanges, uint FavoriteGroup, bool block_trade, string _volumenSystem, bool preferoldchat, string lastpurchase, List<uint> pollparticipation, List<uint> votedrooms,
            string lastfollowinglogin, bool ignoreRoomInvitations, int citizenshipLevel, int helperLevel, string wiredactrewards, bool dontFocusUser, Dictionary<int, NaviLogs> naviLogs, Dictionary<uint, uint> targetedOffers, 
            string chatColor, int newIdentity, int newBot, bool frankJaApareceu, int moedas, int corAtual, string coresjaTenho, uint coinsPurchased)
        {
            this.Id = Id;
            this.Username = Username;
            this.RealName = RealName;
            this.Rank = Rank;
            this.Diamonds = Diamonds;
            this.Look = OtanixEnvironment.FilterFigure(Look.ToLower());
            this.Gender = Gender.ToLower();
            this.Motto = Motto;
            this.alertasAtivados = true;
            this.LastOnline = OtanixEnvironment.GetUnixTimestamp();
            this.HomeRoom = HomeRoom;
            this.Respect = Respect;
            this.DailyRespectPoints = DailyRespectPoints;
            this.DailyPetRespectPoints = DailyPetRespectPoints;
            this.HasFriendRequestsDisabled = HasFriendRequestsDisabled;
            this.FollowEnable = FollowEnable;
            this.BlockTrade = block_trade;
            this.AchievementPoints = achievementPoints;
            this.FavoriteGroup = FavoriteGroup;
            this.NameChanges = nameChanges;
            this.CurrentQuestId = currentQuestID;
            this.CurrentQuestProgress = currentQuestProgress;
            this.LastQuestId = 0;
            this.volumenSystem = _volumenSystem;
            this.Created = OtanixEnvironment.UnixTimeStampToDateTime(UnixTime);
            this.preferOldChat = preferoldchat;
            this.LastPurchase = lastpurchase;
            this.PollParticipation = pollparticipation;
            this.IgnoreRoomInvitations = ignoreRoomInvitations;
            this.CitizenshipLevel = citizenshipLevel;
            this.HelperLevel = helperLevel;
            this.AlfaGuideEnabled = false;
            this.AlfaHelperEnabled = false;
            this.AlfaGuardianEnabled = false;
            this.WiredRewards = new Dictionary<uint, WiredActReward>();
            this.CoinsPurchased = coinsPurchased;
            if (wiredactrewards.Length > 0)
            {
                uint itemId = 0;
                double dTime = 0;
                int aRewards = 0;
                int originalInt = 0;
                foreach (string str in wiredactrewards.Split(';'))
                {
                    itemId = Convert.ToUInt32(str.Split(',')[0]);
                    dTime = Convert.ToDouble(str.Split(',')[1]);
                    aRewards = Convert.ToInt32(str.Split(',')[2]);
                    originalInt = Convert.ToInt32(str.Split(',')[3]);

                    this.WiredRewards.Add(itemId, new WiredActReward(itemId, dTime, aRewards, originalInt));
                }
            }
            this.LastAlfaSend = OtanixEnvironment.GetUnixTimestamp() - 1200; // 20 min.
            this.DontFocusUser = dontFocusUser;
            this.navigatorLogs = naviLogs;
            this.ConvertedOnPet = false;
            this.PetType = -1;
            this.DiamondsCycleUpdate = OtanixEnvironment.GetUnixTimestamp();
            this.MoedasCycleUpdate = OtanixEnvironment.GetUnixTimestamp();
            this.TargetedOffers = targetedOffers;
            this.ChatColor = chatColor;
            this.NewIdentity = newIdentity;
            this.NewBot = newBot;

            this.LoadingRoom = 0;
            this.LoadingChecksPassed = false;
            this.CurrentRoomId = 0;
            this.IsTeleporting = false;
            this.TeleporterId = 0;
            this.SpectatorMode = false;
            this.Disconnected = false;

            this.FavoriteRooms = new List<uint>();
            this.MutedUsers = new List<string>();
            this.RatedRooms = votedrooms;
            this.UsersRooms = new List<uint>();
            this.RoomsVisited = new LinkedList<RoomVisits>();
            this.mygroups = new List<uint>();
            this.quests = new Dictionary<uint, int>();
            this.wardrobes = new Dictionary<int, Wardrobe>();
            this.HabboQuizQuestions = new List<int>(5);
            this.showingStaffBadge = true;
            this.frankJaApareceu = frankJaApareceu;
            this.Moedas = moedas;
            this.corAtual = corAtual;
            this.coresjaTenho = coresjaTenho;
            if (lastfollowinglogin == "" || !DateTime.TryParse(lastfollowinglogin, out this.LastFollowingLogin))
                this.LastFollowingLogin = DateTime.Now;
        }

        internal Habbo(uint Id, string Username, string RealName, uint Rank, string Motto, double Created, string Look, string Gender, uint Diamonds, string MachineId, uint achievementPoints, double LastOnline, uint FavoriteGroup, bool blocknewfriends, bool blocktrade, bool ignoreRoomInvitations, bool dontfocususers, bool preferoldchat, uint coins_purchased)
        {
            this.Id = Id;
            this.Username = Username;
            this.RealName = RealName;
            this.Rank = Rank;
            this.Motto = Motto;
            this.Created = OtanixEnvironment.UnixTimeStampToDateTime(Created);
            this.Look = OtanixEnvironment.FilterFigure(Look.ToLower());
            this.Gender = Gender.ToLower();
            this.Diamonds = Diamonds;
            this.MachineId = MachineId;
            this.LastOnline = LastOnline;
            this.AchievementPoints = achievementPoints;
            this.FavoriteGroup = FavoriteGroup;
            this.HasFriendRequestsDisabled = blocknewfriends;
            this.BlockTrade = blocktrade;
            this.IgnoreRoomInvitations = ignoreRoomInvitations;
            this.DontFocusUser = dontfocususers;
            this.preferOldChat = preferoldchat;
            this.alertasAtivados = true;
            this.RoomsVisited = new LinkedList<RoomVisits>();
            this.mygroups = new List<uint>();
            this.CoinsPurchased = coins_purchased;
        }

        internal void Init(UserData data)
        {
            this.PresentBuyStopwatch = new Stopwatch();
            this.PresentBuyStopwatch.Start();

            this.FavoriteRooms = data.favouritedRooms;
            this.InventoryComponent = new InventoryComponent(Id, mClient);
            this.InventoryComponent.SetActiveState(mClient);
            this.AvatarEffectsInventoryComponent = new AvatarEffectsInventoryComponent(Id, mClient, data);
            this.chatMessageManager = new ChatMessageManager();
            this.premiumManager = PremiumManager.LoadPremiumData(Id);
            this.userclothingManager = new UserClothing(Id);

            if (HasFuse("fuse_chat_staff"))
            {
                data.friends.Add(EmuSettings.CHAT_USER_ID, StaffChat.MessengerStaff);
            }

            this.Messenger = new HabboMessenger(Id, data.friends, data.requests);
            this.UpdateRooms();
            this.LoadMuteUsers();
        }

        internal void InitExtra()
        {
            this.UpdateLastFollowingLogin();
            this.UpdateHabboClubAchievement();
        }
        #endregion

        internal List<GroupItem> ForumGroups()
        {
            List<GroupItem> groupsList = new List<GroupItem>();
            foreach (UInt32 g in MyGroups)
            {
                GroupItem Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(g);
                if (Group != null && Group.Forum != null)
                    groupsList.Add(Group);

                if (groupsList.Count >= 20)
                    break;
            }

            return groupsList;
        }

        internal void ExitAlfaState()
        {
            if (this.AlfaServiceEnabled)
            {
                if (this.AlfaGuideEnabled)
                {
                    OtanixEnvironment.GetGame().GetAlfaManager().GetTourManager().RemoveAlfa(this.Id);
                    this.AlfaGuideEnabled = false;
                }
                if (this.AlfaHelperEnabled)
                {
                    OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().RemoveAlfa(this.Id);
                    this.AlfaHelperEnabled = false;
                }
                if (this.AlfaGuardianEnabled)
                {
                    OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().RemoveGuardian(this.Id);
                    this.AlfaGuardianEnabled = false;
                }
            }
        }

        internal void UpdateLastFollowingLogin()
        {
            if ((DateTime.Now - this.LastFollowingLogin).TotalDays > 2) // reseteamos
            {
                OtanixEnvironment.GetGame().GetAchievementManager().ResetAchievement(this.GetClient(), "ACH_Login");
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Id, "ACH_RegistrationDuration", 1);
                if (this.Rank > 1)
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Id, "ACH_GuideEnrollmentLifetime", 1);

                if (CitizenshipLevel == 2 || CitizenshipLevel == 3)
                    OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(mClient, "citizenship");
                else if (HelperLevel == 3 || HelperLevel == 4 || HelperLevel == 5 || HelperLevel == 6 || HelperLevel == 7)
                    OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(mClient, "helper");

                this.DailyRespectPoints = 3;
                this.DailyPetRespectPoints = 3;
                this.LastFollowingLogin = DateTime.Now;
            }
            else if ((DateTime.Now - this.LastFollowingLogin).TotalDays > 1)
            {
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Id, "ACH_Login", 1);
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Id, "ACH_RegistrationDuration", 1);
                if(this.Rank > 1)
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Id, "ACH_GuideEnrollmentLifetime", 1);

                if (CitizenshipLevel == 2 || CitizenshipLevel == 3)
                    OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(mClient, "citizenship");
                else if (HelperLevel == 3 || HelperLevel == 4 || HelperLevel == 5 || HelperLevel == 6 || HelperLevel == 7)
                    OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(mClient, "helper");

                this.DailyRespectPoints = 3;
                this.DailyPetRespectPoints = 3;
                this.LastFollowingLogin = DateTime.Now;
            }

            UserAchievement ach_friendlistsize = this.GetAchievementData("ACH_FriendListSize");

            if(ach_friendlistsize != null)
                ach_friendlistsize.Progress = FriendsCount;
        }

        internal void UpdateHabboClubAchievement()
        {
            UserAchievement ach_basicclub = this.GetAchievementData("ACH_BasicClub");
            if (ach_basicclub != null)
            {
                if ((DateTime.Now - this.Created).TotalDays > ((ach_basicclub.Progress * 31)))
                {
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Id, "ACH_BasicClub", 1);
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Id, "ACH_VipHC", 1);
                }
            }
        }

        internal void UpdateRooms()
        {
            UsersRooms = new List<uint>();

            DataTable dbTable;
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id FROM rooms WHERE owner = @name");
                dbClient.addParameter("name", Username);
                dbTable = dbClient.getTable();
            }

            foreach (DataRow Row in dbTable.Rows)
            {
                UsersRooms.Add(Convert.ToUInt32(Row["id"]));
            }
        }

        internal void LoadMuteUsers()
        {
            this.MutedUsers = new List<string>();

            DataTable dbTable;
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT ignore_id FROM user_ignores WHERE user_id = '" + Id + "'");
                dbTable = dbClient.getTable();
            }

            foreach(DataRow Row in dbTable.Rows)
            {
                string IgnoreId = (string)Row["ignore_id"];

                if (!MutedUsers.Contains(IgnoreId))
                    MutedUsers.Add(IgnoreId);
            }
        }

        internal void SerializeQuests(ref QueuedServerMessage response)
        {
            OtanixEnvironment.GetGame().GetQuestManager().GetList(mClient, null);
        }

        internal bool HasFuse(string Fuse)
        {
            if (OtanixEnvironment.GetGame().GetRoleManager().RankHasRight(Rank, Fuse))
            {
                return true;
            }

            return false;
        }

        internal void OnDisconnect()
        {
            try
            {
                if (Disconnected)
                    return;

                Disconnected = true;

                if (Messenger != null)
                {
                    Messenger.AppearOffline = true;
                    Messenger.Destroy();
                    Messenger = null;
                }

                if (IsPremium())
                {
                    GetPremiumManager().Destroy();
                }

                saveWardrobe();
                saveBadges();
                //OtanixEnvironment.GetGame().GetMuteManager().RemoveUserMute(Id);

                var pollParticipation = "";
                if (this.PollParticipation.Count > 0)
                {
                    foreach (UInt32 value in this.PollParticipation)
                    {
                        pollParticipation += value + ";";
                    }
                    pollParticipation = pollParticipation.Remove(pollParticipation.Length - 1);
                }

                var votedRooms = "";
                if (this.RatedRooms.Count > 0)
                {
                    foreach (UInt32 value in this.RatedRooms)
                    {
                        votedRooms += value + ";";
                    }
                    votedRooms = votedRooms.Remove(votedRooms.Length - 1);
                }

                var actrewards = "";
                if (this.WiredRewards.Count > 0)
                {
                    foreach (WiredActReward wrd in this.WiredRewards.Values)
                    {
                        actrewards += wrd.ItemId + "," + wrd.LastUpdate + "," + wrd.ActualRewards + "," + wrd.OriginalInt + ";";
                    }
                    actrewards = actrewards.Remove(actrewards.Length - 1);
                }

                var navilogs = "";
                if (this.navigatorLogs.Count > 0)
                {
                    foreach (NaviLogs navi in this.navigatorLogs.Values)
                    {
                        navilogs += navi.Id + "," + navi.Value1 + "," + navi.Value2 + ";";
                    }
                    navilogs = navilogs.Remove(navilogs.Length - 1);
                }

                var targetedoffers = "";
                if(this.TargetedOffers.Count > 0)
                {
                    foreach(KeyValuePair<uint, uint> k in this.TargetedOffers)
                    {
                        targetedoffers += k.Key + "-" + k.Value + ";";
                    }
                    targetedoffers = targetedoffers.Remove(targetedoffers.Length - 1);
                }

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("UPDATE users SET namechanges = '" + NameChanges + "', respect = '" + Respect +
                                      "', daily_respect_points = '" + DailyRespectPoints +
                                      "', daily_pet_respect_points = '" + DailyPetRespectPoints + "'," +
                                      " diamonds = '" + Diamonds + "',  machine_last = @machineLast, currentquestid = '" + CurrentQuestId +
                                      "', block_trade = '" + ((BlockTrade) ? "1" : "0") + "'," +
                                      " block_newfriends = '" + ((HasFriendRequestsDisabled) ? "1" : "0") +
                                      "', look = @look, motto = @motto, gender = @gender, last_online = '" + LastOnline.ToString() + "'," +
                                      " achievement_points = '" + AchievementPoints + "', home_room = '" + HomeRoom + "', volumenSystem = '" +
                                      volumenSystem + "', prefer_old_chat = '" + ((preferOldChat) ? "1" : "0") + "'," +
                                      " last_purchase = '" + LastPurchase + "'," +
                                      " poll_participation = '" + pollParticipation + "', voted_rooms = '" + votedRooms + "'," +
                                      " lastfollowinglogin = '" + LastFollowingLogin.ToString().Replace(".", "/") + "', ignoreRoomInvitations = '" + ((IgnoreRoomInvitations) ? "1" : "0") + "'," +
                                      " citizenship_level = '" + CitizenshipLevel + "', helper_level = '" + HelperLevel + "', actrewards = '" + actrewards + "', dontfocususers = '" + ((DontFocusUser) ? "1" : "0") + "'," +
                                      " navilogs = @navilogs, targeted_offers = @targetedoffers, alertasAtivados = '" + ((alertasAtivados) ? "1" : "0") + "', frankJaApareceu = '" + ((frankJaApareceu) ? "1" : "0") + "', FavoriteGroup = '" + FavoriteGroup + "', moedas = '" + Moedas + "', corAtual = @coratual, coresjaTenho  = '" + coresjaTenho + "', new_identity = '" + NewIdentity + "', new_bot = '" + NewBot + "', coins_purchased = '" + CoinsPurchased + "' WHERE id = " + Id);
                    dbClient.addParameter("look", Look);
                    dbClient.addParameter("coratual", corAtual);
                    dbClient.addParameter("motto", Motto);
                    dbClient.addParameter("gender", Gender);
                    dbClient.addParameter("machineLast", MachineId);
                    dbClient.addParameter("navilogs", navilogs);
                    dbClient.addParameter("targetedoffers", targetedoffers);
                    dbClient.runQuery();

                    dbClient.runFastQuery("DELETE FROM users_online WHERE user_id = '" + Id + "'");
                }

                if (this.AlfaServiceEnabled)
                {
                    if (this.AlfaGuideEnabled)
                    {
                        OtanixEnvironment.GetGame().GetAlfaManager().GetTourManager().RemoveAlfa(this.Id);
                        this.AlfaGuideEnabled = false;
                    }
                    if (this.AlfaHelperEnabled)
                    {
                        Help help = null;
                        if (OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps.ContainsKey(AlfaServiceId))
                            help = OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().Helps[AlfaServiceId];

                        if (help != null)
                        {
                            if (help.helpState == HelpState.TALKING)
                            {
                                help.helpState = HelpState.FINISHED;
                            }
                            else if (help.helpState == HelpState.SEARCHING_USER)
                            {
                                help.NeedUpdate = true;
                            }
                        }

                        OtanixEnvironment.GetGame().GetAlfaManager().GetHelpManager().RemoveAlfa(this.Id);
                        this.AlfaHelperEnabled = false;
                    }
                    if (this.AlfaGuardianEnabled)
                    {
                        Bully bully = null;
                        if (OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().Bullies.ContainsKey(AlfaServiceId))
                            bully = OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().Bullies[AlfaServiceId];

                        if (bully != null)
                        {
                            if (bully.bullyState == BullyState.WAITING_RESPONSE)
                            {
                                bully.bullySolution = BullySolution.EXIT;
                                bully.bullyState = BullyState.FINISHED;
                            }
                            else if (bully.bullyState == BullyState.SEARCHING_USER)
                            {
                                bully.NeedUpdate = true;
                            }
                        }

                        OtanixEnvironment.GetGame().GetAlfaManager().GetBullyManager().RemoveGuardian(this.Id);
                        this.AlfaGuardianEnabled = false;
                    }
                }

                if (InRoom && CurrentRoom != null && CurrentRoom.GetRoomUserManager() != null)
                {
                    CurrentRoom.GetRoomUserManager().RemoveUserFromRoom(mClient, false, false, false);
                }

                if (AvatarEffectsInventoryComponent != null)
                {
                    AvatarEffectsInventoryComponent.Dispose();
                    AvatarEffectsInventoryComponent = null;
                }

                if (InventoryComponent != null)
                {
                    InventoryComponent.SetIdleState();
                    InventoryComponent.RunDBUpdate();
                    InventoryComponent.Destroy();
                    InventoryComponent = null;
                }

                if (BadgeComponent != null)
                {
                    BadgeComponentLoaded = false;
                    BadgeComponent.Destroy();
                    BadgeComponent = null;
                }

                if (RelationshipComposer != null)
                {
                    RelationsLoaded = false;
                    RelationshipComposer.Destroy();
                    RelationshipComposer = null;
                }

                if (Achievements != null && Achievements.Count > 0)
                {
                    AchievementsLoaded = false;
                    Achievements.Clear();
                    Achievements = null;
                }

                if (quests != null && quests.Count > 0)
                {
                    QuestsLoaded = false;
                    quests.Clear();
                    quests = null;
                }

                if (wardrobes != null && wardrobes.Count > 0)
                {
                    WardrobeLoaded = false;
                    wardrobes.Clear();
                    wardrobes = null;
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Disconnecting user " + e);
            }
            finally
            {
                OtanixEnvironment.GetGame().GetClientManager().UnregisterClient(Id, Username);
                Logging.WriteLine(Username + " has logged out.");
            }
        }

        internal void InitMessenger()
        {
            var Client = GetClient();
            if (Client == null || Messenger == null || Messenger.requests == null)
                return;

            OtanixEnvironment.GetGame().GetClientManager().QueueConsoleUpdate(Client);
            friendsCount = Messenger.myFriends;

            Client.SendMessage(Messenger.SerializeFriendsCategories(Client.GetHabbo()));
            Client.SendMessage(Messenger.SerializeFriends(Client.GetHabbo()));
            Client.SendMessage(Messenger.SerializeRequests(Client));
            Messenger.SerializeOfflineMessages(Client);
        }

        internal void UpdateCreditsBalance()
        {
            if (mClient == null || mClient.GetMessageHandler() == null || mClient.GetHabbo() == null)
                return;

            if (Moedas < 0)
                Moedas = 0;

            if (Moedas > int.MaxValue)
                Moedas = int.MaxValue;

            mClient.GetMessageHandler().GetResponse().Init(Outgoing.CreditsBalance);
            mClient.GetMessageHandler().GetResponse().AppendString((EmuSettings.HOTEL_LUCRATIVO) ? Convert.ToString(mClient.GetHabbo().Moedas) : "99999.0"); // amount
            mClient.GetMessageHandler().SendResponse();
        }

        internal void UpdateExtraMoneyBalance()
        {
            if (mClient == null || mClient.GetMessageHandler() == null)
                return;

            if (Diamonds < 0)
                Diamonds = 0;

            if (Diamonds > uint.MaxValue)
                Diamonds = uint.MaxValue;

            if (CoinsPurchased < 0)
                CoinsPurchased = 0;

            if (CoinsPurchased > uint.MaxValue)
                CoinsPurchased = uint.MaxValue;


            mClient.GetMessageHandler().GetResponse().Init(Outgoing.ActivityPointsMessageParser);
            mClient.GetMessageHandler().GetResponse().AppendInt32(4); // count
            mClient.GetMessageHandler().GetResponse().AppendInt32(0); // duckets
            mClient.GetMessageHandler().GetResponse().AppendUInt(99999); // amount

           
            mClient.GetMessageHandler().GetResponse().AppendInt32(5); // Diamonds
            mClient.GetMessageHandler().GetResponse().AppendUInt(Diamonds); // amount
            mClient.GetMessageHandler().GetResponse().AppendInt32(103); // piruletas ou seja la oq for
            mClient.GetMessageHandler().GetResponse().AppendUInt(CoinsPurchased); // quantidade
            mClient.GetMessageHandler().GetResponse().AppendInt32(105); // diamantes
            mClient.GetMessageHandler().GetResponse().AppendUInt(Diamonds); // quantidade
            

            mClient.GetMessageHandler().SendResponse();
        }

        internal GameClient GetClient()
        {
            return OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Id);
        }

        internal HabboMessenger GetMessenger()
        {
            return Messenger;
        }

        internal BadgeComponent GetBadgeComponent()
        {
            if (!BadgeComponentLoaded)
                _LoadBadgeComponent();

            return BadgeComponent;
        }

        internal RelationshipComposer GetRelationshipComposer()
        {
            if (!RelationsLoaded)
                _LoadRelationships();

            return RelationshipComposer;
        }

        internal InventoryComponent GetInventoryComponent()
        {
            return InventoryComponent;
        }

        internal AvatarEffectsInventoryComponent GetAvatarEffectsInventoryComponent()
        {
            return AvatarEffectsInventoryComponent;
        }

        internal ChatMessageManager GetChatMessageManager()
        {
            return chatMessageManager;
        }

        internal Premium GetPremiumManager()
        {
            return premiumManager;
        }

        internal UserClothing GetUserClothingManager()
        {
            return userclothingManager;
        }

        internal bool IsPremium()
        {
            return premiumManager != null;
        }

        internal void GiveUserPiruleta(int p)
        {
            int SumaDiamantes = (int)CoinsPurchased + p;
            if (SumaDiamantes < 0)
                CoinsPurchased = 0;
            else
                CoinsPurchased = (uint)SumaDiamantes;

            UpdateExtraMoneyBalance();
        }

        internal void GiveUserDiamonds(int p)
        {
            int SumaDiamantes = (int)Diamonds + p;
            if(SumaDiamantes < 0)
                Diamonds = 0;
            else
                Diamonds = (uint)SumaDiamantes;

            UpdateExtraMoneyBalance();
        }

        internal void darMoedas(int p)
        {
            int SumaDiamantes = (int)Moedas + p;
            if (SumaDiamantes < 0)
                Moedas = 0;
            else
                Moedas = (int)SumaDiamantes;

            UpdateCreditsBalance();
        }

        internal int GetQuestProgress(uint p)
        {
            if (!QuestsLoaded)
                _LoadQuests();

            var progress = 0;
            quests.TryGetValue(p, out progress);
            return progress;
        }

        internal UserAchievement GetAchievementData(string p)
        {
            if (!AchievementsLoaded)
                LoadAchievements();

            UserAchievement achievement = null;
            Achievements.TryGetValue(p, out achievement);
            return achievement;
        }
    }
}
