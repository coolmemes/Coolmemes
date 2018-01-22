using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Pathfinding;
using Butterfly.HabboHotel.Pets;
using Butterfly.HabboHotel.Quests;
using Butterfly.HabboHotel.RoomBots;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.Util;
using HabboEvents;
using Butterfly.HabboHotel.Navigators.RoomQueue;
using System.Collections.Concurrent;

namespace Butterfly.HabboHotel.Rooms
{
    class RoomUserManager
    {
        private Room room;

        internal Hashtable usersByUsername;
        internal Hashtable usersByUserID;
        internal event RoomEventDelegate OnUserEnter;
        internal event RoomEventDelegate OnBotTakeUser;

        private Hashtable pets;
        private Hashtable bots;

        private int petCount;
        private int botCount;
        private int userCount;

        // public bool UpdateUsersPath;

        private int primaryPrivateUserID;
        private int secondaryPrivateUserID;

        internal int PetCount
        {
            get
            {
                return petCount;
            }
        }

        internal int BotCount
        {
            get
            {
                return botCount;
            }
        }

        internal List<RoomUser> GetBots
        {
            get
            {
                return bots.Values.OfType<RoomUser>().ToList();
            }
        }

        internal ConcurrentDictionary<int, RoomUser> UserList
        {
            get;
            private set;
        }

        internal RoomUser getFirstUserOnRoom()
        {
            return UserList.Values.First();
        }

        internal int GetRoomUserCount()
        {
            if (UserList == null)
                return 0;

            return UserList.Count - bots.Count - pets.Count;
        }

        public RoomUserManager(Room room)
        {
            this.room = room;
            UserList = new ConcurrentDictionary<int, RoomUser>();
            pets = new Hashtable();
            bots = new Hashtable();

            usersByUsername = new Hashtable();
            usersByUserID = new Hashtable();
            primaryPrivateUserID = 0;
            secondaryPrivateUserID = 0;
            ToRemove = new List<RoomUser>((int)room.RoomData.UsersMax);

            petCount = 0;
            botCount = 0;
            userCount = 0;
        }

        internal RoomUser DeployBot(RoomBot Bot, Pet PetData)
        {
            var BotUser = new RoomUser(Bot.BotId, room.RoomId, primaryPrivateUserID++, room, false);
            var PersonalID = secondaryPrivateUserID++;
            BotUser.InternalRoomID = PersonalID;
            UserList.TryAdd(PersonalID, BotUser);
            OnUserAdd(BotUser);

            BotUser.BotData = Bot;
            BotUser.BotAI = Bot.GenerateBotAI(BotUser.VirtualId);

            var Model = room.GetGameMap().Model;

            if (BotUser.IsPet)
            {
                BotUser.BotAI.Init((int)Bot.BotId, BotUser.VirtualId, room.RoomId, BotUser, room);
                BotUser.PetData = PetData;
                BotUser.PetData.VirtualId = BotUser.VirtualId;
            }
            else
            {
                BotUser.BotAI.Init((int)Bot.BotId, BotUser.VirtualId, room.RoomId, BotUser, room);
            }

            if ((Bot.X > 0 && Bot.Y > 0) && Bot.X < Model.MapSizeX && Bot.Y < Model.MapSizeY)
            {
                BotUser.SetPos(Bot.X, Bot.Y, Bot.Z);
                BotUser.SetRot(Bot.Rot, false);
            }
            else
            {
                Bot.X = Model.DoorX;
                Bot.Y = Model.DoorY;

                BotUser.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                BotUser.SetRot(Model.DoorOrientation, false);
            }

            UpdateUserStatus(BotUser, false);
            BotUser.UpdateNeeded = true;

            var EnterMessage = new ServerMessage(Outgoing.UsersMessageParser);
            EnterMessage.AppendInt32(1);
            BotUser.Serialize(EnterMessage);
            room.SendMessage(EnterMessage);

            BotUser.BotAI.OnSelfEnterRoom();

            if (BotUser.IsPet)
            {
                if (pets.ContainsKey(BotUser.PetData.PetId)) //Pet allready placed
                    pets[BotUser.PetData.PetId] = BotUser;
                else
                    pets.Add(BotUser.PetData.PetId, BotUser);

                petCount++;
            }
            else
            {
                if (bots.ContainsKey(BotUser.BotData.BotId)) //Bot allready placed
                    bots[BotUser.BotData.BotId] = BotUser;
                else
                    bots.Add(BotUser.BotData.BotId, BotUser);

                botCount++;
            }

            room.GetGameMap().AddUserToMap(BotUser, new Point(BotUser.Coordinate.X, BotUser.Coordinate.Y));

            return BotUser;
        }

        internal void RemoveBot(int VirtualId, bool Kicked)
        {
            var User = GetRoomUserByVirtualId(VirtualId);

            if (User == null || !User.IsBot)
            {
                return;
            }

            RemoveRoomUser(User);

            if (User.IsPet)
            {
                pets.Remove(User.PetData.PetId);
                petCount--;
            }

            if (User.IsBot)
            {
                bots.Remove(User.BotData.BotId);
                botCount--;
            }

            User.BotAI.OnSelfLeaveRoom(Kicked);

            var LeaveMessage = new ServerMessage(Outgoing.UserLeftRoom);
            LeaveMessage.AppendString(User.VirtualId.ToString());
            room.SendMessage(LeaveMessage);

            RoomUser roomUser;
            UserList.TryRemove(User.InternalRoomID, out roomUser);
        }

        private void UpdateUserEffect(RoomUser User, int x, int y)
        {
            try
            {
                if (User.IsBot)
                    return;

                var NewCurrentUserItemEffect = room.GetGameMap().EffectMap[x, y];
                if (NewCurrentUserItemEffect > 0)
                {
                    var Type = ByteToItemEffectEnum.Parse(NewCurrentUserItemEffect);
                    if (Type != User.CurrentItemEffect)
                    {
                        switch (Type)
                        {
                            case ItemEffectType.Iceskates:
                                {
                                    if (User.GetClient().GetHabbo().Gender.ToLower() == "m")
                                        User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(38);
                                    else
                                        User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(39);
                                    User.CurrentItemEffect = ItemEffectType.Iceskates;

                                    if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
                                    {
                                        User.skatingTimerSecond = !User.skatingTimerSecond;

                                        if (User.skatingTimerSecond == false)
                                        {
                                            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(User.GetClient().GetHabbo().Id, "ACH_TagC", 1);
                                        }
                                    }

                                    break;
                                }

                            case ItemEffectType.Normalskates:
                                {
                                    if (User.GetClient().GetHabbo().Gender.ToLower() == "m")
                                    {
                                        User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(55);
                                    }
                                    else
                                    {
                                        User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(56);
                                    }
                                    //56=girls
                                    //55=
                                    User.CurrentItemEffect = Type;

                                    if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
                                    {
                                        User.normalSkateTimerSecond = !User.normalSkateTimerSecond;

                                        if (User.normalSkateTimerSecond == false)
                                        {
                                            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(User.GetClient().GetHabbo().Id, "ACH_RbTagC", 1);
                                        }
                                    }

                                    break;
                                }
                            case ItemEffectType.Swim:
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(29);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimLow:
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(30);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimHalloween:
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(37);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.None:
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(0);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.PublicPool:
                                {
                                    User.AddStatus("swim", string.Empty);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimHalloween15:
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(185);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.HorseJump:
                                {
                                    if (User.montandoBol == false)
                                        break;

                                    RoomUser Pet = GetRoomUserByVirtualId(User.montandoID);
                                    if (Pet == null)
                                        break;

                                    if (Pet.Statusses.ContainsKey("jmp"))
                                    {
                                        Pet.RemoveStatus("jmp");
                                        break;
                                    }

                                    if (ByteToItemEffectEnum.Parse(room.GetGameMap().EffectMap[User.SetX, User.SetY]) != ItemEffectType.HorseJump)
                                        break;

                                    Pet.AddStatus("jmp", "1.1");
                                    Pet.UpdateNeeded = true;

                                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(User.GetClient().GetHabbo().Id, "ACH_HorseJumping", 1);

                                    break;
                                }
                            case ItemEffectType.Snowboard:
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(97);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.Trampoline:
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(193);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.Treadmill:
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(194);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.Crosstrainer:
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(195);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                        }
                    }
                }
                else if (User.CurrentItemEffect != ItemEffectType.None && NewCurrentUserItemEffect == 0)
                {
                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyEffect(-1);
                    User.CurrentItemEffect = ItemEffectType.None;
                    User.RemoveStatus("swim");
                }
            }
            catch { }
        }

        internal RoomUser GetUserForSquare(int x, int y)
        {
            return room.GetGameMap().GetRoomUsers(new Point(x, y)).FirstOrDefault();
        }

        internal void AddUserToRoom(GameClient Session, bool Spectator)
        {
            if (Session == null || Session.GetHabbo() == null || room == null)
                return;

            var User = new RoomUser(Session.GetHabbo().Id, room.RoomId, primaryPrivateUserID++, room, Spectator);
            if (User == null)
                return;

            if (usersByUsername.ContainsKey(Session.GetHabbo().Username.ToLower()))
                usersByUsername.Remove(Session.GetHabbo().Username.ToLower());

            if (usersByUserID.ContainsKey(User.HabboId))
                usersByUserID.Remove(User.HabboId);

            usersByUsername.Add(Session.GetHabbo().Username.ToLower(), User);
            usersByUserID.Add(Session.GetHabbo().Id, User);

            var PersonalID = secondaryPrivateUserID++;
            User.InternalRoomID = PersonalID;
            Session.GetHabbo().CurrentRoomId = room.RoomId;

            UserList.TryAdd(PersonalID, User);
            OnUserAdd(User);
        }

        private void OnUserAdd(RoomUser user)
        {
            try
            {
                if (room == null || room.GetGameMap() == null)
                    return;

                if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                    return;

                if (!user.IsSpectator)
                {
                    var Model = room.GetGameMap().Model;

                    user.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                    user.SetRot(Model.DoorOrientation, false);

                    int roomRank = room.GetRightsLevel(user.GetClient());
                    if (roomRank > 0)
                    {
                        user.AddStatus("flatctrl " + roomRank, "");
                    }

                    user.CurrentItemEffect = ItemEffectType.None;

                    if (!user.IsBot && user.GetClient().GetHabbo().IsTeleporting)
                    {
                        var Item = room.GetRoomItemHandler().GetItem(user.GetClient().GetHabbo().TeleporterId);

                        if (Item != null)
                        {
                            user.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                            user.SetRot(Item.Rot, false);

                            Item.InteractingUser2 = user.GetClient().GetHabbo().Id;
                            Item.ExtraData = "2";
                            Item.UpdateState(false, true);
                        }
                    }

                    user.GetClient().GetHabbo().IsTeleporting = false;
                    user.GetClient().GetHabbo().TeleporterId = 0;

                    var EnterMessage = new ServerMessage(Outgoing.UsersMessageParser);
                    EnterMessage.AppendInt32(1);
                    user.Serialize(EnterMessage);
                    room.SendMessage(EnterMessage);


                    if (room.RoomData.Owner != user.GetClient().GetHabbo().Username && !room.IsPublic)
                    {
                        OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(user.GetClient(), QuestType.SOCIAL_VISIT);
                    }
                }

                if (user.GetClient().GetHabbo().GetMessenger() != null && user.GetClient().GetHabbo().notifyOnRoomEnter)
                {
                    OtanixEnvironment.GetGame().GetClientManager().QueueConsoleUpdate(user.GetClient());
                }

                if (room.RoomMuted && !room.CheckRights(user.GetClient(), true))
                {
                    room.AddMute(user.GetClient().GetHabbo().Id, 900000);
                }

                user.GetClient().GetMessageHandler().OnRoomUserAdd();

                

                OnUserEnter?.Invoke(user, null);

                if (room.GotMusicController())
                    room.GetRoomMusicController().OnNewUserEnter(user);

                if (!user.IsSpectator)
                {
                    foreach (var roomUser in UserList.Values)
                    {
                        if (roomUser.IsBot || roomUser.IsPet)
                            roomUser.BotAI.OnUserEnterRoom(user);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException(e.ToString());
            }
        }

        internal void RemoveUserFromRoom(GameClient Session, Boolean NotifyClient, Boolean NotifyKick, Boolean notifyConsole = true)
        {
            try
            {
                if (Session == null)
                    return;

                if (Session.GetHabbo() == null || Session.GetHabbo().GetAvatarEffectsInventoryComponent() == null)
                    return;

                Session.GetHabbo().GetAvatarEffectsInventoryComponent().OnRoomExit();

                if (NotifyClient)
                {
                    if (NotifyKick)
                    {
                        Session.GetMessageHandler().GetResponse().Init(Outgoing.RoomError);
                        Session.GetMessageHandler().GetResponse().AppendInt32(4008);
                        Session.GetMessageHandler().SendResponse();
                    }

                    Session.GetMessageHandler().GetResponse().Init(Outgoing.OutOfRoom);
                    Session.GetMessageHandler().SendResponse();
                }

                var User = GetRoomUserByHabbo(Session.GetHabbo().Id);

                if (User != null)
                {
                    Session.GetHabbo().onlineTimeInRooms += Convert.ToUInt32(User.enteredStopwatch.ElapsedMilliseconds / 1000 / 60);
                    if (User.team != Team.none)
                    {
                        room.GetTeamManager().OnUserLeave(User);
                    }
                    if (User.montandoBol == true)
                    {
                        User.montandoBol = false;
                        var usuarioVinculado = GetRoomUserByVirtualId(User.montandoID);
                        if (usuarioVinculado != null)
                        {
                            usuarioVinculado.montandoBol = false;
                            usuarioVinculado.montandoID = 0;
                        }
                    }

                    if (User.sentadoBol == true || User.acostadoBol == true)
                    {
                        User.sentadoBol = false;
                        User.acostadoBol = false;
                    }

                    RemoveRoomUser(User);

                    if (Session.GetHabbo().FavoriteGroup > 0)
                    {
                        room.QueueRemoveGroupUser(Session.GetHabbo().FavoriteGroup);
                    }

                    if (!User.IsSpectator)
                    {
                        if (User.CurrentItemEffect != ItemEffectType.None)
                        {
                            User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect = -1;
                        }

                        if (Session.GetHabbo() != null)
                        {
                            if (room.HasActiveTrade(Session.GetHabbo().Id))
                                room.TryStopTrade(Session.GetHabbo().Id);

                            Session.GetHabbo().CurrentRoomId = 0;

                            if (Session.GetHabbo().GetMessenger() != null && notifyConsole == true)
                            {
                                OtanixEnvironment.GetGame().GetClientManager().QueueConsoleUpdate(Session);
                            }
                        }
                    }

                    if (room.RoomData.Type == "public")
                    {
                        RoomQueue rQueue = OtanixEnvironment.GetGame().GetRoomQueueManager().GetRoomQueue(room.Id);
                        if (rQueue != null)
                        {
                            rQueue.FirstUserOnRoom();
                        }
                    }

                    usersByUserID.Remove(User.HabboId);
                    if (Session.GetHabbo() != null)
                        usersByUsername.Remove(Session.GetHabbo().Username.ToLower());

                    User.Dispose();
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Error during removing user from room:" + e);
            }
        }

        private void onRemove(RoomUser user)
        {
            try
            {
                var session = user.GetClient();
                var Bots = new List<RoomUser>();

                foreach (var roomUser in UserList.Values)
                {
                    if (roomUser.IsBot)
                        Bots.Add(roomUser);
                }

                var PetsToRemove = new List<RoomUser>();
                foreach (var Bot in Bots)
                {
                    Bot.BotAI.OnUserLeaveRoom(session);

                    if (Bot.IsPet && Bot.PetData.OwnerId == user.HabboId && !room.CheckRights(session, true))
                    {
                        PetsToRemove.Add(Bot);
                    }
                }

                foreach (var toRemove in PetsToRemove)
                {
                    if (user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().GetInventoryComponent() == null)
                        continue;

                    user.GetClient().GetHabbo().GetInventoryComponent().AddPet(toRemove.PetData);
                    RemoveBot(toRemove.VirtualId, false);
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException(e.ToString());
            }
        }

        private void RemoveRoomUser(RoomUser user)
        {
            // Eliminamos al usuario de la lista UserList.
            RoomUser junk;
            if (!UserList.TryRemove(user.InternalRoomID, out junk)) return;

            // Obtenemos la baldosa en la que el usuario se situa.
            int X = user.IsWalking ? user.SetX : user.X;
            int Y = user.IsWalking ? user.SetY : user.Y;

            // Actualizamos sus variables.
            user.InternalRoomID = -1;
            user.isKicking = false;

            // Limpiamos los estados de las baldosas.
            if (!user.IsWalking)
                room.GetGameMap().GameMap[X, Y] = user.SqState;

            room.GetGameMap().RemoveUserFromMap(user, new Point(X, Y));
            onRemove(junk);

            ServerMessage LeaveMessage = new ServerMessage(Outgoing.UserLeftRoom);
            LeaveMessage.AppendString(user.VirtualId + String.Empty);
            room.SendMessage(LeaveMessage);
        }

        internal RoomUser GetPet(uint PetId)
        {
            if (pets.ContainsKey(PetId))
                return (RoomUser)pets[PetId];

            return null;
        }

        internal void UpdateUserCount(int count)
        {
            userCount = count;
            room.RoomData.UsersNow = count;
        }

        internal RoomUser GetRoomUserByVirtualId(int VirtualId)
        {
            return UserList.ContainsKey(VirtualId) ? UserList[VirtualId] : null;
        }

        internal RoomUser GetRoomUserByHabbo(uint pId)
        {
            if (usersByUserID == null)
                return null;

            if (usersByUserID.ContainsKey(pId))
                return (RoomUser)usersByUserID[pId];

            return null;
        }

        internal List<RoomUser> GetRoomUsers()
        {
            var users = UserList.ToList();

            var returnList = new List<RoomUser>();
            foreach (var pair in users)
            {
                if (!pair.Value.IsBot)
                    returnList.Add(pair.Value);
            }

            return returnList;
        }

        internal List<RoomUser> GetRoomUserByFuse(string Fuse)
        {
            var returnList = new List<RoomUser>();
            foreach (var user in UserList.Values)
            {
                if (!user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null && user.GetClient().GetHabbo().HasFuse(Fuse))
                    returnList.Add(user);
            }

            return returnList;
        }

        internal RoomUser GetRoomUserByHabbo(string pName)
        {
            if (usersByUsername.ContainsKey(pName.ToLower()))
                return (RoomUser)usersByUsername[pName.ToLower()];

            return null;
        }

        internal void SavePets(IQueryAdapter dbClient)
        {
            try
            {
                if (GetPets().Count > 0)
                {
                    AppendPetsUpdateString(dbClient);
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Error during saving furniture for room " + room.RoomId + ". Stack: " + e);
            }
        }

        internal void AppendPetsUpdateString(IQueryAdapter dbClient)
        {
            var inserts = new QueryChunk("INSERT INTO user_pets (id,user_id,room_id,name,type,race,color,expirience,energy,createstamp,nutrition,respect,z,y,z) VALUES ");
            var updates = new QueryChunk();

            var petsSaved = new List<uint>();
            foreach (var pet in GetPets())
            {
                if (petsSaved.Contains(pet.PetId))
                    continue;

                petsSaved.Add(pet.PetId);
                if (pet.DBState == DatabaseUpdateState.NeedsInsert)
                {
                    inserts.AddParameter(pet.PetId + "name", pet.Name);
                    inserts.AddParameter(pet.PetId + "race", pet.Race);
                    inserts.AddParameter(pet.PetId + "color", pet.Color);
                    inserts.AddQuery("(" + pet.PetId + "," + pet.OwnerId + "," + pet.RoomId + ",@" + pet.PetId + "name," + pet.Type + ",@" + pet.PetId + "race,@" + pet.PetId + "color,0,100,'" + pet.CreationStamp + "',0,0,0,0,0)");
                }
                else if (pet.DBState == DatabaseUpdateState.NeedsUpdate)
                {
                    updates.AddParameter(pet.PetId + "name", pet.Name);
                    updates.AddParameter(pet.PetId + "race", pet.Race);
                    updates.AddParameter(pet.PetId + "color", pet.Color);
                    updates.AddQuery("UPDATE user_pets SET room_id = " + pet.RoomId + ", name = @" + pet.PetId + "name, race = @" + pet.PetId + "race, color = @" + pet.PetId + "color, type = " + pet.Type + ", expirience = " + pet.Expirience + ", " +
                        "energy = " + pet.Energy + ", nutrition = " + pet.Nutrition + ", respect = " + pet.Respect + ", createstamp = '" + pet.CreationStamp + "', x = " + pet.X + ", Y = " + pet.Y + ", Z = " + pet.Z + " WHERE id = " + pet.PetId);
                }

                pet.DBState = DatabaseUpdateState.Updated;
            }

            inserts.Execute(dbClient);
            updates.Execute(dbClient);

            inserts.Dispose();
            updates.Dispose();

            inserts = null;
            updates = null;
        }

        internal List<Pet> GetPets()
        {
            var users = UserList.ToList();

            var results = new List<Pet>();
            foreach (var pair in users)
            {
                var user = pair.Value;
                if (user.IsPet)
                    results.Add(user.PetData);
            }

            return results;
        }

        internal ServerMessage SerializeStatusUpdates(Boolean All)
        {
            var Users = new List<RoomUser>();

            foreach (var User in UserList.Values)
            {
                if (!All)
                {
                    if (!User.UpdateNeeded)
                        continue;

                    if (User.UpdateNeededCounter > 0)
                    {
                        User.UpdateNeededCounter--;
                        continue;
                    }

                    User.UpdateNeeded = false;
                }

                if (User.IsSpectator)
                    continue;

                Users.Add(User);
            }

            if (Users.Count == 0)
                return null;

            var Message = new ServerMessage(Outgoing.UserUpdateMessageParser);
            Message.AppendInt32(Users.Count);
            foreach (var User in Users)
                User.SerializeStatus(Message);

            return Message;
        }

        internal void OnUserUpdateStatus()
        {
            foreach (var user in UserList.Values)
            {
                UpdateUserStatus(user, false);
            }
        }

        internal bool isValid(RoomUser user)
        {
            if (user == null)
                return false;
            if (user.IsBot)
                return true;
            if (user.GetClient() == null)
                return false;
            if (user.GetClient().GetHabbo() == null)
                return false;
            if (user.GetClient().GetHabbo().CurrentRoomId != room.RoomId)
                return false;

            return true;
        }

        internal RoomUser GetBot(uint BotId)
        {
            if (bots.ContainsKey(BotId))
                return (RoomUser)bots[BotId];

            return null;
        }

        internal void UpdateUserStatus(RoomUser User, bool cyclegameitems)
        {
            try
            {
                if (User == null)
                    return;

                if (User.IsBot)
                    cyclegameitems = false;

                if ((User.Statusses.ContainsKey("lay") && !User.acostadoBol) || (User.Statusses.ContainsKey("sit") && !User.sentadoBol))
                {
                    User.Statusses.Remove("lay");
                    User.Statusses.Remove("sit");
                    User.UpdateNeeded = true;
                }

                if (User.Statusses.ContainsKey("sign"))
                {
                    User.Statusses.Remove("sign");
                    User.UpdateNeeded = true;
                }

                var ItemsOnSquare = room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                double newZ = room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare) + ((User.montandoBol == true && User.IsPet == false) ? 1 : 0);

                if (newZ != User.Z)
                {
                    User.Z = newZ;
                    if (User.isFlying)
                        User.Z += 4 + (0.5 * Math.Sin(0.7 * User.flyk));
                    User.UpdateNeeded = true;
                }

                foreach (var Item in ItemsOnSquare)
                {
                    if (cyclegameitems)
                    {
                        User.wiredItemIdTrigger = Item.Id;
                        Item.UserWalksOnFurni(User);
                    }

                    if (User.IsBot)
                    {
                        Item.BotWalksOnFurni(User);
                    }

                    if (Item.GetBaseItem().IsSeat)
                    {
                        if (!User.Statusses.ContainsKey("sit"))
                        {
                            User.Statusses.Add("sit", Item.GetBaseItem().MultiHeight.Count > 0 ? TextHandling.GetString(newZ-0.5) : TextHandling.GetString(Item.GetBaseItem().Height));
                        }

                        User.Z = Item.GetZ;
                        if (User.isFlying)
                            User.Z += 4 + (0.5 * Math.Sin(0.7 * User.flyk));

                        User.RotHead = Item.Rot;
                        User.RotBody = Item.Rot;
                        User.UpdateNeeded = true;
                    }

                    switch (Item.GetBaseItem().InteractionType)
                    {
                        case InteractionType.bed:
                        case InteractionType.tent:
                        case InteractionType.wobench:
                            {
                                Point bedCoord = new Point(Item.GetX, Item.GetY);
                                if (Item.GetBaseItem().Width == 2)
                                {
                                    if (Item.Rot == 0)
                                    {
                                        if (bedCoord.X != User.X)
                                            bedCoord.X = User.X;
                                    }
                                    else if (Item.Rot == 2)
                                    {
                                        if (bedCoord.Y != User.Y)
                                            bedCoord.Y = User.Y;
                                    }
                                }

                                room.GetGameMap().UpdateUserMovement(User.Coordinate, bedCoord, User);
                                User.SetPos(bedCoord.X, bedCoord.Y, Item.GetZ);
                                User.SetRot(Item.Rot, false);

                                if (!User.Statusses.ContainsKey("lay"))
                                {
                                    User.Statusses.Add("lay", TextHandling.GetString(Item.GetBaseItem().Height) + " null");
                                }

                                User.UpdateNeeded = true;

                                if (Item.GetBaseItem().InteractionType == InteractionType.tent)
                                {
                                    User.tentId = Item.Id;
                                }
                                else if (Item.GetBaseItem().InteractionType == InteractionType.wobench)
                                {
                                    Item.ExtraData = "1";
                                    Item.UpdateState();
                                }

                                break;
                            }

                        case InteractionType.bigtent:
                            {
                                User.tentId = Item.Id;
                                break;
                            }

                        case InteractionType.shower:
                            {
                                Item.ExtraData = "1";
                                Item.UpdateState();

                                if (Item.GetBaseItem().Name == "hblooza14_planepadb")
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(176);
                                else if (Item.GetBaseItem().Name == "hblooza14_planepadr")
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(175);

                                break;
                            }
                        case InteractionType.preassureplate:
                            {
                                Item.ExtraData = "1";
                                Item.UpdateState(false, true);
                                break;
                            }
                        case InteractionType.fbgate:
                            {
                                if (User.GetClient().GetHabbo().LastMovFGate && User.GetClient().GetHabbo().BackupGender == User.GetClient().GetHabbo().Gender)
                                {
                                    User.GetClient().GetHabbo().LastMovFGate = false;
                                    User.GetClient().GetHabbo().Look = User.GetClient().GetHabbo().BackupLook;
                                }
                                else
                                {
                                    // mini Fix
                                    string _gateLook = ((User.GetClient().GetHabbo().Gender.ToUpper() == "M") ? Item.ExtraData.Split(',')[0] : Item.ExtraData.Split(',')[1]);
                                    string gateLook = "";
                                    foreach (string part in _gateLook.Split('.'))
                                    {
                                        if (part.StartsWith("hd"))
                                            continue;
                                        gateLook += part + ".";
                                    }
                                    gateLook = gateLook.Substring(0, gateLook.Length - 1);

                                    // Generating New Look.
                                    var Parts = User.GetClient().GetHabbo().Look.Split('.');
                                    var NewLook = "";
                                    foreach (var Part in Parts)
                                    {
                                        if (/*Part.StartsWith("hd") || */Part.StartsWith("sh") || Part.StartsWith("cp") || Part.StartsWith("cc") || Part.StartsWith("ch") || Part.StartsWith("lg") || Part.StartsWith("ca") || Part.StartsWith("wa"))
                                            continue;
                                        NewLook += Part + ".";
                                    }

                                    /*if (!OtanixEnvironment.GetGame().GetUserLookManager().IsValidLook(User.GetClient().GetHabbo(), NewLook))
                                    {
                                        User.WhisperComposer("El look que intentas ponerte no es válido para tu usuario.");
                                        break;
                                    }*/

                                    NewLook += gateLook;

                                    User.GetClient().GetHabbo().BackupLook = User.GetClient().GetHabbo().Look;
                                    User.GetClient().GetHabbo().BackupGender = User.GetClient().GetHabbo().Gender;
                                    User.GetClient().GetHabbo().Look = NewLook;
                                    User.GetClient().GetHabbo().LastMovFGate = true;
                                }

                                var UpdateLook = new ServerMessage(Outgoing.UpdateUserInformation);
                                UpdateLook.AppendInt32(-1);
                                UpdateLook.AppendString(User.GetClient().GetHabbo().Look);
                                UpdateLook.AppendString(User.GetClient().GetHabbo().Gender.ToLower());
                                UpdateLook.AppendString(User.GetClient().GetHabbo().Motto);
                                UpdateLook.AppendUInt(User.GetClient().GetHabbo().AchievementPoints);
                                User.GetClient().SendMessage(UpdateLook);

                                if (User.GetClient().GetHabbo().InRoom)
                                {
                                    var UpdateLookRoom = new ServerMessage(Outgoing.UpdateUserInformation);
                                    UpdateLookRoom.AppendInt32(User.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(User.GetClient().GetHabbo().Id).VirtualId);
                                    UpdateLookRoom.AppendString(User.GetClient().GetHabbo().Look);
                                    UpdateLookRoom.AppendString(User.GetClient().GetHabbo().Gender.ToLower());
                                    UpdateLookRoom.AppendString(User.GetClient().GetHabbo().Motto);
                                    UpdateLookRoom.AppendUInt(User.GetClient().GetHabbo().AchievementPoints);
                                    User.GetClient().GetHabbo().CurrentRoom.SendMessage(UpdateLookRoom);
                                }

                                break;
                            }

                        //33: Red
                        //34: Green
                        //35: Blue
                        //36: Yellow

                        case InteractionType.banzaigategreen:
                        case InteractionType.banzaigateblue:
                        case InteractionType.banzaigatered:
                        case InteractionType.banzaigateyellow:
                        case InteractionType.freezeyellowgate:
                        case InteractionType.freezeredgate:
                        case InteractionType.freezegreengate:
                        case InteractionType.freezebluegate:
                            {
                                if (cyclegameitems && !room.GetGameManager().IsGameStarted() && !room.GetGameManager().IsGamePaused() && !User.montandoBol)
                                {
                                    var effectID = (int)Item.team + (Item.GetBaseItem().Name.StartsWith("bb_") ? 32 : 39);
                                    var t = room.GetTeamManager();
                                    var efectmanager = User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent();

                                    if (User.team != Item.team)
                                    {
                                        if (User.team != Team.none)
                                        {
                                            t.OnUserLeave(User);
                                            efectmanager.ApplyCustomEffect(0);
                                            User.team = Team.none;
                                            return;
                                        }

                                        if (t.CanEnterOnTeam(Item.team))
                                        {
                                            User.team = Item.team;
                                            t.AddUser(User);

                                            if (efectmanager.CurrentEffect != effectID && efectmanager.CurrentEffect != 4)
                                            {
                                                efectmanager.ApplyCustomEffect(effectID);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        t.OnUserLeave(User);
                                        efectmanager.ApplyCustomEffect(0);
                                        User.team = Team.none;
                                    }
                                }
                                break;
                            }

                        case InteractionType.banzaitele:
                            {
                                double tempo = OtanixEnvironment.GetUnixTimestampInMili();
                                 if (User.IsWalking && !User.IsTeleporting && ((tempo - User.lastTeleBanzai) > 700)) 
                                    room.GetGameItemHandler().onTeleportRoomUserEnter(User, Item);
                                break;
                            }

                        case InteractionType.piñata:
                            {
                                if (User.IsWalking)
                                {
                                    if (Item.ExtraData.Length > 0)
                                    {
                                        var golpesdados = int.Parse(Item.ExtraData);
                                        if (golpesdados < Item.GetBaseItem().VendingIds[0])
                                        {
                                            if (User.CurrentEffect == 158) // el efecto del bastón o Regadera!
                                            {
                                                golpesdados++;
                                                Item.ExtraData = golpesdados.ToString();
                                                Item.UpdateState();

                                                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(User.GetClient().GetHabbo().Id, "ACH_PinataWhacker", 1);

                                                if (golpesdados == Item.GetBaseItem().VendingIds[0]) // regalito! rompemos piñata!
                                                {
                                                    OtanixEnvironment.GetGame().GetPiñataManager().DeliverPiñataRandomItem(User, room, Item);
                                                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(User.GetClient().GetHabbo().Id, "ACH_PinataBreaker", 1);
                                                }
                                            }
                                        }
                                    }
                                }

                                break;
                            }

                        case InteractionType.treadmill:
                        case InteractionType.crosstrainer:
                            {
                                User.SetRot(Item.Rot, false, true);

                                Item.ExtraData = "1";
                                Item.UpdateState();

                                break;
                            }

                        case InteractionType.fxprovider:
                            {
                                if (Item.ExtraData.Length > 0 && Item.ExtraData.Contains(";"))
                                {
                                    int EffectId = 0;
                                    int.TryParse(Item.ExtraData.Split(';')[1], out EffectId);

                                    if (EffectId > 0)
                                    {

                                        if (EffectId != User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect)
                                            User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(EffectId);
                                        else
                                            User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(0);

                                        Item.updateInteractionCount(Item, true);
                                    }
                                }
                                break;
                            }
                        case InteractionType.colortile:
                            {
                                Item.updateInteractionCount(Item);
                                break;
                            }
                    }
                }

                if (cyclegameitems)
                {
                    if (room.GetGameManager() != null)
                    {
                        room.GetGameManager().GetBanzai().OnUserWalk(User);
                        room.GetGameManager().GetFreeze().OnUserWalk(User);
                    }
                }
            }
            catch { }
        }

        private readonly List<RoomUser> ToRemove;

        internal void OnCycle(ref int idleCount)
        {
            // Vaciamos los usuarios a eliminar.
            ToRemove.Clear();

            // Reiniciamos el countador actual de usuarios en sala.
            int userCounter = 0;

            foreach (RoomUser User in UserList.Values)
            {
                // Si no es un usuario válido (Se ha desconectado, lleva mucho rato ausente...) lo echamos de sala.
                if (!isValid(User))
                {
                    if (User.GetClient() != null)
                        RemoveUserFromRoom(User.GetClient(), false, false);
                    else
                        RemoveRoomUser(User);
                }

                // Incrementamos el tiempo ausente.
                User.IdleTime++;

                // Si no está durmiendo y lleva 600 ciclos ausente y además no es bot ni mascota, lo ponemos como durmiendo.
                if (!User.IsBot && !User.IsAsleep && User.IdleTime >= 600 && isValid(User))
                {
                    User.IsAsleep = true;
                    User.GetClient().GetHabbo().ExitAlfaState();

                    ServerMessage FallAsleep = new ServerMessage(Outgoing.IdleStatus);
                    FallAsleep.AppendInt32(User.VirtualId);
                    FallAsleep.AppendBoolean(true);
                    room.SendMessage(FallAsleep);
                }

                // En caso de llevar más de 10 minutos ausente, echamos de sala.
                // if (User.NeedsAutokick && !ToRemove.Contains(User))
                // {
                //     ToRemove.Add(User);
                //     continue;
                // }

                // Actualizamos el estado de item en mano (café, agua...)
                if (User.CarryItemID > 0)
                {
                    User.CarryTimer--;
                    if (User.CarryTimer <= 0)
                        User.CarryItem(0);
                }

                // Actualizamos Freeze y Shield del juego Freeze.
                if (room.GetGameManager() != null && room.GetGameManager().GetFreeze() != null)
                    room.GetGameManager().GetFreeze().CycleUser(User);

                // Si el comando :fly está activado, da el efecto de volar.
                if (User.isFlying)
                    User.OnFly();

                // Si hay una baldosa a continuación:
                if (User.SetStep)
                {
                    // Si el usuario está sobre un item determinado, tal vez se tenga que activar un efecto.
                    UpdateUserEffect(User, User.SetX, User.SetY);

                    // Actualizamos la variable.
                    User.tentId = 0;

                    // La baldosa actual vuelve a obtener el valor que tenía y la nueva baldosa pasa a estado ocupada.
                    room.GetGameMap().GameMap[User.X, User.Y] = User.SqState;
                    User.SqState = room.GetGameMap().GameMap[User.SetX, User.SetY];

                    // Wired: Vete Ya!
                    List<RoomItem> items = room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                    for (int i = 0; i < items.Count; i++)
                    {
                        RoomItem item = items[i];
                        if (item == null)
                            continue;

                        User.wiredItemIdTrigger = item.Id;
                        item.UserWalksOffFurni(User);

                        if (item.GetBaseItem().InteractionType == InteractionType.shower || item.GetBaseItem().InteractionType == InteractionType.guildgate || item.GetBaseItem().InteractionType == InteractionType.treadmill || item.GetBaseItem().InteractionType == InteractionType.crosstrainer || item.GetBaseItem().InteractionType == InteractionType.preassureplate)
                        {
                            item.ExtraData = "0";
                            item.UpdateState();
                        }
                        else if(item.GetBaseItem().InteractionType == InteractionType.wobench)
                        {
                            item.ExtraData = "0";
                            item.UpdateState();

                            User.ApplyEffect(114);
                        }
                    }

                    // Por fin, actualizamos las coordenadas del usuario a la nueva baldosa.
                    User.X = User.SetX;
                    User.Y = User.SetY;
                    User.Z = User.SetZ;
                    if (User.isFlying)
                        User.Z += 4 + 0.5 * Math.Sin(0.7 * User.flyk);

                    // Si estamos subiendo a una mascota, en llegar a ella:
                    if (User.walkingToPet != null)
                    {
                        if (User.X == User.walkingToPet.X && User.Y == User.walkingToPet.Y)
                        {
                            User.montandoBol = true;
                            User.walkingToPet.Freezed = false;
                            User.walkingToPet.montandoBol = true;
                            User.walkingToPet.montandoID = User.VirtualId;
                            User.montandoID = User.walkingToPet.VirtualId;
                            User.RotHead = User.walkingToPet.RotHead;
                            User.RotBody = User.walkingToPet.RotBody;
                            if (User.walkingToPet.PetData.HaveSaddle == 1)
                                User.ApplyEffect(77);
                            else if (User.walkingToPet.PetData.HaveSaddle == 2)
                                User.ApplyEffect(103);

                            User.walkingToPet = null;
                        }
                    }

                    // ¡Actualizamos! Si pisamos furni, etc.
                    UpdateUserStatus(User, true);

                    // Si pisamos la puerta, salimos de sala.
                    if (User.X == room.GetGameMap().Model.DoorX && User.Y == room.GetGameMap().Model.DoorY && !ToRemove.Contains(User) && !User.IsBot)
                    {
                        ToRemove.Add(User);
                        continue;
                    }

                    // Volvemos a poner que no hay baldosa futura.
                    User.SetStep = false;
                }

                needrepath:
                GenerateNewPath(User);

                // Si el usuario está andando y no está freezeado:
                if (User.IsWalking && !User.Freezed)
                {
                    // Si no podemos pisar la baldosa o hemos llegado a nuestro destino:
                    if ((User.PathStep >= User.Path.Count) || (User.GoalX == User.X && User.GoalY == User.Y))
                    {
                        // Dejamos de andar y borramos el estado de movimiento.
                        User.ClearMovement(false);

                        // Breeding:
                        if (User.IsPet && (User.PetData.Type == 0 || User.PetData.Type == 1 || User.PetData.Type == 2 || User.PetData.Type == 3 || User.PetData.Type == 4) && User.PetData.waitingForBreading > 0 && (User.PetData.breadingTile.X == User.X && User.PetData.breadingTile.Y == User.Y))
                        {
                            User.Freezed = true;
                            room.GetGameMap().RemoveUserFromMap(User, User.Coordinate);

                            Dictionary<uint, RoomItem> breedingPet = room.GetRoomItemHandler().breedingPet;

                            if (breedingPet != null)
                            {
                                if (breedingPet[User.PetData.waitingForBreading].havepetscount.Count == 2)
                                {
                                    GameClient owner = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(User.PetData.OwnerId);
                                    if (owner != null)
                                        owner.SendMessage(PetBreeding.GetMessage(User.PetData.waitingForBreading, breedingPet[User.PetData.waitingForBreading].havepetscount[0], breedingPet[User.PetData.waitingForBreading].havepetscount[1]));
                                }
                            }
                        }
                        else if (User.IsPet && (User.PetData.Type == 0 || User.PetData.Type == 1 || User.PetData.Type == 2 || User.PetData.Type == 3 || User.PetData.Type == 4) && User.PetData.waitingForBreading > 0)
                        {
                            User.Freezed = false;
                            User.PetData.waitingForBreading = 0;
                            User.PetData.breadingTile = new Point();
                        }

                        // ¡Actualizamos! Si pisamos furni, etc.
                        UpdateUserStatus(User, false);
                        User.handelingBallStatus = 0;

                        // Bot Take User:
                        if (User.IsBot)
                        {
                            List<RoomUser> usersArround = room.GetGameMap().SquareHasUsersNear(User.X, User.Y);
                            RoomUser[] userArray = new RoomUser[2];
                            userArray[0] = User;

                            foreach (RoomUser myUser in usersArround)
                            {
                                userArray[1] = myUser;
                                if (OnBotTakeUser != null)
                                    OnBotTakeUser(userArray, null);
                            }

                            Array.Clear(userArray, 0, userArray.Length);
                            userArray = null;
                            usersArround.Clear();
                            usersArround = null;
                        }

                        // Caballo:
                        if (User.montandoBol == true && User.IsPet == false)
                        {
                            var mascotaVinculada = GetRoomUserByVirtualId(User.montandoID);
                            if (mascotaVinculada != null)
                            {
                                mascotaVinculada.IsWalking = false;
                                mascotaVinculada.RemoveStatus("mv");

                                var mess = new ServerMessage(Outgoing.UserUpdateMessageParser);
                                mess.AppendInt32(1);
                                mascotaVinculada.SerializeStatus(mess, "");
                                User.GetClient().GetHabbo().CurrentRoom.SendMessage(mess);
                            }
                        }

                        // Si nos han kickeado, nos salimos de sala.
                        if (User.isKicking)
                        {
                            ToRemove.Add(User);
                            continue;
                        }
                    }
                    else
                    {
                        // Recuperamos cual es la siguiente baldosa que vamos a pisar.
                        int s = (User.Path.Count - User.PathStep) - 1;
                        Vector2D NextStep = User.Path[s];
                        User.PathStep++;

                        if(User.fastWalk && User.PathStep < User.Path.Count)
                        {
                            s = (User.Path.Count - User.PathStep) - 1;
                            NextStep = User.Path[s];
                            User.PathStep++;
                        }
                        
                        // Estas son las coordenadas de la siguiente baldosa.
                        int nextX = NextStep.X;
                        int nextY = NextStep.Y;

                        // Eliminamos el efecto andar que tiene el usuario.
                        User.RemoveStatus("mv");

                        // Si la siguiente baldosa contiene un salto de caballo y no estamos montados, finaliza el usuario de andar.
                        if (ByteToItemEffectEnum.Parse(room.GetGameMap().EffectMap[nextX, nextY]) == ItemEffectType.HorseJump && User.montandoBol == false)
                        {
                            User.UpdateNeeded = true;
                            User.IsWalking = false;
                            continue;
                        }

                        // Comprueba que podamos movernos a la baldosa.
                        if (room.GetGameMap().IsValidStep(new Vector2D(User.X, User.Y), new Vector2D(nextX, nextY), (User.GoalX == nextX && User.GoalY == nextY), User, false, false) && !User.comandoFreeze)
                        {
                            // Obtiene la altura de la nueva baldosa.
                            double nextZ = room.GetGameMap().SqAbsoluteHeight(nextX, nextY);

                            // Nos levantamos en caso de estar sentados o acostados.
                            if (User.Statusses.ContainsKey("lay"))
                            {
                                User.Statusses.Remove("lay");
                                User.acostadoBol = false;
                                User.UpdateNeeded = true;
                            }
                            else if (User.Statusses.ContainsKey("sit"))
                            {
                                User.Statusses.Remove("sit");
                                User.sentadoBol = false;
                                User.UpdateNeeded = true;
                            }

                            // Si el usuario tiene/no tiene el comando :fly activado, activa el efecto de andar.
                            if (!User.isFlying)
                            {
                                if (User.montandoBol == true && User.IsPet == false)
                                {
                                    var mascotaVinculada = GetRoomUserByVirtualId(User.montandoID);
                                    User.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ + 1));
                                    mascotaVinculada.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                }
                                else
                                {
                                    User.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                }
                            }
                            else
                            {
                                User.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ + 4 + (0.5 * Math.Sin(0.7 * User.flyk))));
                            }

                            // Rotación de usuario.
                            int newRot = Rotation.Calculate(User.X, User.Y, nextX, nextY, User.moonwalkEnabled);
                            User.RotBody = newRot;
                            User.RotHead = newRot;

                            // Establecemos que hay una baldosa a la que moverse y su futura coordenada.
                            User.SetStep = true;
                            User.SetX = nextX;
                            User.SetY = nextY;
                            User.SetZ = nextZ;

                            // Actualizamos lista de usuarios sobre baldosa. Borramos la anterior y metemos al usuario en la nueva balodsa.
                            room.GetGameMap().UpdateUserMovement(new Point(User.X, User.Y), new Point(User.SetX, User.SetY), User);

                            // Actualización de pelota para chutarla.
                            if (room.GotSoccer())
                                room.GetSoccer().OnUserWalk(User);

                            // Actualizamos los estados de los caballos.
                            if (User.montandoBol == true && User.IsPet == false)
                            {
                                RoomUser mascotaVinculada = GetRoomUserByVirtualId(User.montandoID);
                                if (mascotaVinculada != null)
                                {
                                    mascotaVinculada.RotHead = User.RotHead;
                                    mascotaVinculada.RotBody = User.RotBody;
                                    mascotaVinculada.SetStep = true;
                                    mascotaVinculada.SetX = nextX;
                                    mascotaVinculada.SetY = nextY;
                                    mascotaVinculada.SetZ = nextZ;

                                    var mess = new ServerMessage(Outgoing.UserUpdateMessageParser);
                                    mess.AppendInt32(2);
                                    mascotaVinculada.SerializeStatus(mess);
                                    User.SerializeStatus(mess);
                                    room.SendMessage(mess);
                                }
                            }
                        }

                        if (User.PathRecalcNeeded && !User.SetStep)
                            goto needrepath;
                    }

                    // Si no está montado en un caballo, actualizamos el estado.
                    if (!User.montandoBol)
                        User.UpdateNeeded = true;
                }
                else
                {
                    // En el caso de no estar andando, eliminamos el efecto.
                    if (User.Statusses.ContainsKey("mv"))
                    {
                        User.RemoveStatus("mv");
                        User.UpdateNeeded = true;
                    }
                }

                if (User.IsBot)
                {
                    // En caso de ser un bot, accedemos al Tick para realizar acciones.
                    User.BotAI.OnTimerTick();
                }
                else
                {
                    // Si es un usuario normal, sumamos el contador de usuarios en sala para ver si hace falta actualizar el contador.
                    userCounter++;
                }
            }

            // Si no hay usuarios en sala, sumamos al timer para ver cuándo desloguear la sala.
            if (userCounter == 0)
                idleCount++;

            // Procedemos a eliminar a los usuarios que se han marchado de sala o deban salir.
            foreach (RoomUser toRemove in ToRemove)
            {
                GameClient client = toRemove.GetClient();
                if (client != null)
                    RemoveUserFromRoom(client, true, false);
                else
                    RemoveRoomUser(toRemove);
            }

            // Si hay distintos usuarios en sala a la última actualización, actualizamos el contador.
            if (userCount != userCounter)
            {
                UpdateUserCount(userCounter);
            }

            // Desactivamos el updatePath
            // UpdateUsersPath = false;
        }

        public void GenerateNewPath(RoomUser User)
        {
            // Si hace falta volver a generar un nuevo path:
            //if (User.PathRecalcNeeded) // || (UpdateUsersPath && User.IsWalking))
            if(true)
            {
                // Limpiamos el actual y generamos el nuevo.
                User.Path.Clear();
                User.Path = PathFinder.FindPath(room.RoomData.AllowDiagonalEnabled, this.room.GetGameMap(), new Vector2D(User.X, User.Y), new Vector2D(User.GoalX, User.GoalY), User);

                // En caso de que haya encontrado uno, empezamos de nuevo.
                if (User.Path.Count > 1)
                {
                    User.PathStep = 1;
                    User.IsWalking = true;
                    User.PathRecalcNeeded = false;
                }
                else
                {
                    User.PathRecalcNeeded = false;
                    User.Path.Clear();
                }
            }
        }

        internal void Destroy()
        {
            room = null;
            usersByUsername.Clear();
            usersByUsername = null;
            usersByUserID.Clear();
            usersByUserID = null;
            OnUserEnter = null;
            OnBotTakeUser = null;
            pets.Clear();
            pets = null;
            bots.Clear();
            bots = null;
            UserList.Clear();
            UserList = null;
        }
    }
}
