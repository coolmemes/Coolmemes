using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Butterfly.Core;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Pathfinding;
using Butterfly.HabboHotel.Pets;
using Butterfly.HabboHotel.RoomBots;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Users.Badges;
using ButterStorm;
using Database_Manager.Database;
using Butterfly.HabboHotel.Rooms.Wired;
using System.Drawing;
using HabboEvents;
using Butterfly.HabboHotel.Quests;
using Butterfly.HabboHotel.Quests.Composer;
using Butterfly.HabboHotel.Users;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Catalogs;
using System.Collections;
using System.Text.RegularExpressions;
using Butterfly.HabboHotel.Rooms.Polls;
using Butterfly.HabboHotel.ChatMessageStorage;
using Butterfly.HabboHotel.GameClients;
using System.IO;
using System.IO.Compression;
using Butterfly.HabboHotel.Camera;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.Group;
using Butterfly.HabboHotel.Filter;
using Butterfly.HabboHotel.Navigators.RoomQueue;
using Butterfly.HabboHotel.Items.Craftable;
using Butterfly.HabboHotel.Premiums;
using Butterfly.HabboHotel.Users.Nux;
using Butterfly.HabboHotel.Support;

namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        #region Enter Room Process
        internal void GetRoomData2()
        {
            try
            {
                if (CurrentLoadingRoom == null || CurrentLoadingRoom.RoomData == null || CurrentLoadingRoom.GetGameMap() == null || Session.GetHabbo().LoadingRoom <= 0)
                    return;

                QueuedServerMessage message = new QueuedServerMessage(Session.GetConnection());

                if (CurrentLoadingRoom.RoomData.Model == null)
                {
                    Session.SendNotif(LanguageLocale.GetValue("room.missingmodeldata"));
                    Session.SendMessage(new ServerMessage(Outgoing.OutOfRoom));
                    ClearRoomLoading();
                    return;
                }

                message.appendResponse(CurrentLoadingRoom.GetGameMap().Model.SerializeHeightmap(CurrentLoadingRoom.GetGameMap()));
                message.appendResponse(CurrentLoadingRoom.GetGameMap().GetStaticHeightmap());
                message.sendResponse();

                CurrentLoadingRoom.HeightMapLoaded = true;
            }
            catch (Exception e)
            {
                Logging.LogException("Unable to load room ID [" + Session.GetHabbo().LoadingRoom + "] " + e);
                Session.SendNotif(LanguageLocale.GetValue("room.roomdataloaderror"));
            }
        }

        internal Room CurrentLoadingRoom;
        private int FloodCount;

        internal void GetRoomData3()
        {
            try
            {
                if (Session.GetHabbo().LoadingRoom <= 0 || !Session.GetHabbo().LoadingChecksPassed || CurrentLoadingRoom == null || CurrentLoadingRoom.GetRoomItemHandler() == null || CurrentLoadingRoom.GetRoomUserManager() == null)
                {
                    return;
                }

                if (!CurrentLoadingRoom.HeightMapLoaded)
                    GetRoomData2();

                ClearRoomLoading();

                var response = new QueuedServerMessage(Session.GetConnection());

                if (CurrentLoadingRoom.RoomData.Type == "private" || CurrentLoadingRoom.RoomData.Type == "public")
                {
                    var floorItems = CurrentLoadingRoom.GetRoomItemHandler().mFloorItems.Values.ToArray();
                    var wallItems = CurrentLoadingRoom.GetRoomItemHandler().mWallItems.Values.ToArray();

                    Response.Init(Outgoing.ObjectsMessageParser);
                    Response.AppendInt32(CurrentLoadingRoom.roomUserFloorItems.Count); // count of owners
                    foreach (uint UserId in CurrentLoadingRoom.roomUserFloorItems.Keys)
                    {
                        Response.AppendUInt(UserId);
                        Response.AppendString(UsersCache.getUsernameById(UserId));
                    }
                    Response.AppendInt32(floorItems.Length);
                    foreach (var Item in floorItems)
                        Item.Serialize(Response);
                    response.appendResponse(GetResponse());

                    Response.Init(Outgoing.ItemsMessageParser);
                    Response.AppendInt32(CurrentLoadingRoom.roomUserWallItems.Count); // count of owners
                    foreach (uint UserId in CurrentLoadingRoom.roomUserWallItems.Keys)
                    {
                        Response.AppendUInt(UserId);
                        Response.AppendString(UsersCache.getUsernameById(UserId));
                    }
                    Response.AppendInt32(wallItems.Length);
                    foreach (var Item in wallItems)
                        Item.Serialize(Response);

                    response.appendResponse(GetResponse());

                    Array.Clear(floorItems, 0, floorItems.Length);
                    Array.Clear(wallItems, 0, wallItems.Length);
                    floorItems = null;
                    wallItems = null;
                    CurrentLoadingRoom.GetRoomUserManager().AddUserToRoom(Session, Session.GetHabbo().SpectatorMode);
                    response.sendResponse();
                }
            }
            catch (Exception e)
            {
                Logging.LogException("Unable to load3 room ID [" + Session.GetHabbo().LoadingRoom + "] " + e);
                Session.SendNotif(LanguageLocale.GetValue("room.roomdataloaderror"));
            }
        }

        internal void OnRoomUserAdd()
        {
            if (Session == null || Session.GetConnection() == null || Session.GetHabbo() == null)
                return;

            var response = new QueuedServerMessage(Session.GetConnection());

            var UsersToDisplay = new List<RoomUser>();

            if (CurrentLoadingRoom == null || CurrentLoadingRoom.GetRoomUserManager() == null)
                return;

            List<string> alreadyAdded = new List<string>();
            foreach (var User in CurrentLoadingRoom.GetRoomUserManager().UserList.Values)
            {
                if (CurrentLoadingRoom.GetRoomUserManager().isValid(User) == false)
                    continue;

                if (User.IsSpectator)
                {
                    continue;
                }

                if (User.IsBot)
                {
                    UsersToDisplay.Add(User);
                }
                else if (!alreadyAdded.Contains(User.GetUsername()))
                {
                    alreadyAdded.Add(User.GetUsername());
                    UsersToDisplay.Add(User);
                }
            }

            Response.Init(Outgoing.UsersMessageParser);
            Response.AppendInt32(UsersToDisplay.Count);

            foreach (var User in UsersToDisplay)
            {
                User.Serialize(Response);
            }
            response.appendResponse(GetResponse());

            Response.Init(Outgoing.ConfigureWallandFloor);
            GetResponse().AppendBoolean(CurrentLoadingRoom.RoomData.Hidewall);
            GetResponse().AppendInt32(CurrentLoadingRoom.RoomData.WallThickness);
            GetResponse().AppendInt32(CurrentLoadingRoom.RoomData.FloorThickness);
            response.appendResponse(GetResponse());

            // Enable inventory too:
            Response.Init(Outgoing.ValidRoom);
            Response.AppendUInt(CurrentLoadingRoom.RoomId);
            Response.AppendBoolean(CurrentLoadingRoom.CheckRights(Session, true));
            response.appendResponse(GetResponse());

            if (CurrentLoadingRoom.groupsOnRoom != null && CurrentLoadingRoom.groupsOnRoom.Count > 0)
            {
                Response.Init(Outgoing.SendGroup);
                Response.AppendInt32(CurrentLoadingRoom.groupsOnRoom.Count);

                for (int i = 0; i < CurrentLoadingRoom.groupsOnRoom.Count; i++)
                {
                    uint Guild = CurrentLoadingRoom.groupsOnRoom.ElementAt(i).Key;
                    GroupItem group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(Guild);

                    Response.AppendUInt(Guild);
                    Response.AppendString(group == null ? "" : group.GroupImage);
                }
                response.appendResponse(GetResponse());
            }
 
            Response.Init(Outgoing.RoomData);
            Response.AppendBoolean(true); // validRoomEntering finished
            CurrentLoadingRoom.RoomData.Serialize(Response);
            Response.AppendBoolean(false); // Waiting to finish valid room entering?
            Response.AppendBoolean(CurrentLoadingRoom.RoomData.Type.ToLower() == "public"); // navigator.staffpicks.unpick(true) / navigator.staffpicks.pick(false) (So check if this room is already picked by staff!)
            Response.AppendBoolean(false); // ignore (validRoomEntering) or something...
            Response.AppendBoolean(CurrentLoadingRoom.RoomMuted); // navigator.muteall_on/off (they're muted or not)
            Response.AppendInt32(CurrentLoadingRoom.RoomData.MuteFuse); // 0 = moderation_mute_none, 1 = moderation_mute_rights
            Response.AppendInt32(CurrentLoadingRoom.RoomData.KickFuse); // 0 = moderation_kick_none, 1 = moderation_kick_rights, 2 = moderation_kick_all
            Response.AppendInt32(CurrentLoadingRoom.RoomData.BanFuse); // 0 = moderation_ban_none, 1 = moderation_ban_rigths
            Response.AppendBoolean(CurrentLoadingRoom.CheckRights(Session, true));  // mute visible
            Response.AppendInt32(CurrentLoadingRoom.RoomData.BubbleMode); // 0 = Free Flow Mode (bubbles can pass) / 1 = Line-by-Line-Mode (old)
            Response.AppendInt32(CurrentLoadingRoom.RoomData.BubbleType); // 0 = Wide bubbles / 1 = Normal bubbles / 2 = Thin bubbles
            Response.AppendInt32(CurrentLoadingRoom.RoomData.BubbleScroll); // 0 = Fast scrolling up / 1 = Normal scrolling up / 2 = Slow scrolling up
            Response.AppendInt32(CurrentLoadingRoom.RoomData.ChatDistance); // Distancia chat
            Response.AppendInt32(CurrentLoadingRoom.RoomData.AntiFloodSettings);
            response.appendResponse(GetResponse());

            var Updates = CurrentLoadingRoom.GetRoomUserManager().SerializeStatusUpdates(true);

            if (Updates != null)
                response.appendResponse(Updates);

            if (Session.GetHabbo().CurrentQuestId > 0)
            {
                var Quest = OtanixEnvironment.GetGame().GetQuestManager().GetQuest(Session.GetHabbo().CurrentQuestId);
                response.appendResponse(QuestStartedComposer.Compose(Session, Quest));
            }

            if (!Session.GetHabbo().HasFuse("fuse_hide_staff"))
            {
                if (Session.GetHabbo().HasFuse("fuse_badge_staff") && Session.GetHabbo().showingStaffBadge)
                {
                    Session.GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(102);
                }
                else if (Session.GetHabbo().HasFuse("fuse_ambassador") && Session.GetHabbo().showingStaffBadge)
                {
                    Session.GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(178);
                }
                else if (Session.GetHabbo().HasFuse("fuse_badge_bot") && Session.GetHabbo().showingStaffBadge)
                {
                    Session.GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(187);
                }
            }

            foreach (var User in CurrentLoadingRoom.GetRoomUserManager().UserList.Values)
            {
                if (User.IsSpectator)
                    continue;

                if (User.IsDancing)
                {
                    Response.Init(Outgoing.Dance);
                    Response.AppendInt32(User.VirtualId);
                    Response.AppendInt32(User.DanceId);
                    response.appendResponse(GetResponse());
                }

                if (User.IsAsleep)
                {
                    Response.Init(Outgoing.IdleStatus);
                    Response.AppendInt32(User.VirtualId);
                    Response.AppendBoolean(true);
                    response.appendResponse(GetResponse());
                }

                if (User.CarryItemID > 0 && User.CarryTimer > 0)
                {
                    Response.Init(Outgoing.ApplyCarryItem);
                    Response.AppendInt32(User.VirtualId);
                    Response.AppendInt32(User.CarryTimer);
                    response.appendResponse(GetResponse());
                }

                if (!User.IsBot)
                {
                    try
                    {
                        if (User.GetClient() != null && User.GetClient().GetHabbo() != null && User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() != null && User.CurrentEffect >= 1)
                        {
                            Response.Init(Outgoing.ApplyEffects);
                            Response.AppendInt32(User.VirtualId);
                            Response.AppendInt32(User.CurrentEffect);
                            Response.AppendInt32(0);
                            response.appendResponse(GetResponse());
                        }
                    }
                    catch (Exception e) { Logging.HandleException(e, "Rooms.SendRoomData3"); }
                }
            }

            if (OtanixEnvironment.GetGame().GetRoomManager().GetEventManager().ContainsEventRoomId(CurrentLoadingRoom.RoomData))
            {
                if(CurrentLoadingRoom.RoomData.Event != null)
                    response.appendResponse(CurrentLoadingRoom.RoomData.Event.Serialize());
            }

            TargetedOffer to = OtanixEnvironment.GetGame().GetTargetedOfferManager().GetRoomIdTargetedOffer(CurrentLoadingRoom.Id);
            if (to != null)
            {
                if (!Session.GetHabbo().TargetedOffers.ContainsKey(to.Id) || Session.GetHabbo().TargetedOffers[to.Id] < to.PurchaseLimit)
                    response.appendResponse(OtanixEnvironment.GetGame().GetTargetedOfferManager().SerializeTargetedOffer(to));
            }

            if (CurrentLoadingRoom.GotRoomPoll())
            {
                if (!Session.GetHabbo().PollParticipation.Contains(CurrentLoadingRoom.Id))
                {
                    if (CurrentLoadingRoom.GetRoomPoll().GetPollType() == PollType.ROOM_QUESTIONARY)
                    {
                        RoomQuestionary handler = (RoomQuestionary)CurrentLoadingRoom.GetRoomPoll();

                        Response.Init(Outgoing.StartPoll);
                        Response.AppendUInt(handler.GetRoomId());
                        Response.AppendString("CLIENT_NPS");
                        Response.AppendString("Customer Satisfaction Poll");
                        Response.AppendString(handler.GetDescription());
                        response.appendResponse(GetResponse());
                    }
                }
            }

            response.sendResponse();
            Session.GetHabbo().SpectatorMode = false;
            Session.GetHabbo().LastMovFGate = false;

            if (Session.GetHabbo().RoomsVisited.Count >= 50)
                Session.GetHabbo().RoomsVisited.RemoveLast();

            if (CurrentLoadingRoom.RoomData.OwnerId != Session.GetHabbo().Id)
            {
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RoomEntry", 1);

                if (Session.GetHabbo().CitizenshipLevel == 1 || Session.GetHabbo().CitizenshipLevel == 2)
                    OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "citizenship");
            }

            Session.GetHabbo().RoomsVisited.AddFirst(new RoomVisits(CurrentLoadingRoom.Id, CurrentLoadingRoom.RoomData.Name, CurrentLoadingRoom.IsPublic, DateTime.Now.Hour, DateTime.Now.Minute));

            CurrentLoadingRoom.IsRoomLoaded = true;
            CurrentLoadingRoom = null;
        }

        internal void enterOnRoom()
        { 
            var cId = Request.PopWiredUInt();
            var Password = Request.PopFixedString();

            if (Session != null && Session.GetHabbo() != null && Session.GetHabbo().Rank > 5 && !Session.GetHabbo()._passouPin)
            {
                Session.SendNotif("Você precisa estar logado");
                return;
            }

            if (Session != null && Session.GetHabbo() != null && OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(Session.GetHabbo().Id))
             cId = OtanixEnvironment.prisaoId();

             PrepareRoomForUser(cId, Password);
        }

        internal void enterOnRoom2()
        {
            var RoomId = Request.PopWiredUInt();
            var EnteringRoom = Request.PopWiredInt32();
            var ForwardRoom = Request.PopWiredInt32();

            if (Session != null && Session.GetHabbo() != null && OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(Session.GetHabbo().Id))
                RoomId = OtanixEnvironment.prisaoId();

            if (Session != null && Session.GetHabbo() != null && Session.GetHabbo().Rank > 5 && !Session.GetHabbo()._passouPin)
            {
                Session.SendNotif("Você precisa estar logado");
                return;
            }

            // false, false = room updated
            // false, true = room forward (goToPrivateRoom)
            // true, false = Navigator: entering room

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().LoadRoom(RoomId);
            if (Room == null)
                return;

            var Data = Room.RoomData;
            if (Data == null)
                return;

            Response.Init(Outgoing.RoomData);
            Response.AppendBoolean(EnteringRoom == 1); // (false => not entered) some info about if this packet show roomdata etc (someshit like validRoomEntering or something...)
            if (Session.GetHabbo().comingRoom == Data.Id)
                Data.Serialize(Response, 0);
            else
                Data.Serialize(Response);
            Response.AppendBoolean(ForwardRoom == 1); // restart room?
            Response.AppendBoolean((Data.Type.ToLower() == "public") ? true : false); // navigator.staffpicks.unpick(true) / navigator.staffpicks.pick(false) (So check if this room is already picked by staff!)
            Response.AppendBoolean(false); // ignore (validRoomEntering) or something...
            Response.AppendBoolean(Room.RoomMuted); // navigator.muteall_on/off (they're muted or not)
            Response.AppendInt32(Data.MuteFuse); // 0 = moderation_mute_none, 1 = moderation_mute_rights
            Response.AppendInt32(Data.KickFuse); // 0 = moderation_kick_none, 1 = moderation_kick_rights, 2 = moderation_kick_all
            Response.AppendInt32(Data.BanFuse); // 0 = moderation_ban_none, 1 = moderation_ban_rigths
            Response.AppendBoolean(Room.CheckRights(Session, true));  // mute visible
            Response.AppendInt32(Data.BubbleMode); // 0 = Free Flow Mode (bubbles can pass) / 1 = Line-by-Line-Mode (old)
            Response.AppendInt32(Data.BubbleType); // 0 = Wide bubbles / 1 = Normal bubbles / 2 = Thin bubbles
            Response.AppendInt32(Data.BubbleScroll); // 0 = Fast scrolling up / 1 = Normal scrolling up / 2 = Slow scrolling up
            Response.AppendInt32(Data.ChatDistance); // Distancia chat
            Response.AppendInt32(Data.AntiFloodSettings);
            SendResponse();
        }

        internal void enterOnRoom3(Room room)
        {
            if (Session == null)
                return;

            if (Session != null && Session.GetHabbo() != null && Session.GetHabbo().Rank > 5 && !Session.GetHabbo()._passouPin)
            {
                Session.SendNotif("Você precisa estar logado");
                return;
            }

            if (Session != null && Session.GetHabbo() != null && OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(Session.GetHabbo().Id))
            {
                Room roomPrisao = OtanixEnvironment.GetGame().GetRoomManager().LoadRoom(OtanixEnvironment.prisaoId());

                if(roomPrisao != null)
                {
                    enterOnRoom3(roomPrisao);
                    return;
                }
                return;

            }

            var Data = room.RoomData;

            ServerMessage Message1 = new ServerMessage(Outgoing.FollowBuddy);
            Message1.AppendUInt(room.RoomId);
            Session.SendMessage(Message1);

            ServerMessage Message2 = new ServerMessage(Outgoing.RoomData);
            Message2.AppendBoolean(true); // some info about if this packet show roomdata etc (someshit like validRoomEntering or something...)
            Data.Serialize(Message2);
            Message2.AppendBoolean(true); // is Waiting to the validRoomEntering (now understand someshits...)
            Message2.AppendBoolean((Data.Type.ToLower() == "public") ? true : false); // navigator.staffpicks.unpick(true) / navigator.staffpicks.pick(false) (So check if this room is already picked by staff!)
            Message2.AppendBoolean(false); // ignore (validRoomEntering) or something...
            Message2.AppendBoolean(room.RoomMuted); // navigator.muteall_on/off (they're muted or not)
            Message2.AppendInt32(Data.MuteFuse); // 0 = moderation_mute_none, 1 = moderation_mute_rights
            Message2.AppendInt32(Data.KickFuse); // 0 = moderation_kick_none, 1 = moderation_kick_rights, 2 = moderation_kick_all
            Message2.AppendInt32(Data.BanFuse); // 0 = moderation_ban_none, 1 = moderation_ban_rigths
            Message2.AppendBoolean(room.CheckRights(Session, true)); // mute visible
            Message2.AppendInt32(Data.BubbleMode); // 0 = Free Flow Mode (bubbles can pass) / 1 = Line-by-Line-Mode (old)
            Message2.AppendInt32(Data.BubbleType); // 0 = Wide bubbles / 1 = Normal bubbles / 2 = Thin bubbles
            Message2.AppendInt32(Data.BubbleScroll); // 0 = Fast scrolling up / 1 = Normal scrolling up / 2 = Slow scrolling up
            Message2.AppendInt32(Data.ChatDistance); // Distancia chat
            Message2.AppendInt32(Data.AntiFloodSettings);
            Session.SendMessage(Message2);
        }

        internal void PrepareRoomForUser(uint Id, string Password)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            ClearRoomLoading();


            if (OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(Session.GetHabbo().Id))
                Id = OtanixEnvironment.prisaoId();

            var response = new QueuedServerMessage(Session.GetConnection());

            if (OtanixEnvironment.ShutdownStarted)
            {
                Session.SendNotif(LanguageLocale.GetValue("shutdown.alert"));
                return;
            }

            if (Session.GetHabbo().InRoom)
            {
                var OldRoom = Session.GetHabbo().CurrentRoom;

                if (OldRoom != null && OldRoom.GetRoomUserManager() != null)
                {
                    OldRoom.GetRoomUserManager().RemoveUserFromRoom(Session, false, false, false);
                    Session.GetHabbo().notifyOnRoomEnter = false;
                }
                else
                {
                    Session.GetHabbo().notifyOnRoomEnter = true;
                }
            }

            var Room = OtanixEnvironment.GetGame().GetRoomManager().LoadRoom(Id);
            if (Room == null)
                return;

            if (Session.GetHabbo().IsTeleporting && Session.GetHabbo().TeleportingRoomID != Id)
                return;

            if (Session.GetHabbo().roomIdQueue > 0 && Session.GetHabbo().goToQueuedRoom == 0)
            {
                RoomQueue rQueue = OtanixEnvironment.GetGame().GetRoomQueueManager().GetRoomQueue(Session.GetHabbo().roomIdQueue);
                if(rQueue != null)
                {
                    rQueue.RemoveUserToQueue(Session.GetHabbo().Id);
                }
            }

            Session.GetHabbo().roomIdQueue = 0;
            Session.GetHabbo().LoadingRoom = Id;
            Session.GetHabbo().ConvertedOnPet = false;
            Session.GetHabbo().PetType = -1;
            Session.GetHabbo().CurrentRingId = 0;

            CurrentLoadingRoom = Room;

            if (Session.GetHabbo().comingRoom == Room.Id)
                goto saltamosPasos;

            if (!Session.GetHabbo().HasFuse("fuse_enter_any_room") && Room.UserIsBanned(Session.GetHabbo().Id))
            {
                if (Room.HasBanExpired(Session.GetHabbo().Id))
                {
                    Room.RemoveBan(Session.GetHabbo().Id);
                }
                else
                {
                    // You are banned of this room!

                    Response.Init(Outgoing.RoomErrorToEnter);
                    Response.AppendInt32(4);
                    response.appendResponse(GetResponse());

                    Response.Init(Outgoing.OutOfRoom);
                    response.appendResponse(GetResponse());

                    response.sendResponse();
                    return;
                }
            }

           /* if (Room.Id == EmuSettings.PRISAOID)
            {
                if (!Session.GetHabbo().HasFuse("fuse_enter_any_room") && Session.GetHabbo().estaPreso == false)
                {
                    Response.Init(Outgoing.OutOfRoom);
                    response.appendResponse(GetResponse());
                    response.sendResponse();

                    Session.SendNotif("Somente usuários presos podem entrar neste quarto.");
                }
            }*/

            if (Room.RoomData.UsersNow >= Room.RoomData.UsersMax && !Session.GetHabbo().HasFuse("fuse_enter_full_rooms") && Session.GetHabbo().goToQueuedRoom == 0)
            {
                if (!Session.GetHabbo().HasFuse("fuse_enter_full_rooms"))
                {
                    if (Room.RoomData.Type == "public") // RoomQueue
                    {
                        RoomQueue rQueue = OtanixEnvironment.GetGame().GetRoomQueueManager().GetRoomQueue(Room.Id);
                        if (rQueue == null)
                        {
                            rQueue = OtanixEnvironment.GetGame().GetRoomQueueManager().CreateRoomQueue(Room);
                        }

                        rQueue.AddUserToQueue(Session);
                    }
                    else
                    {
                        // This room is full!!!!
                        Response.Init(Outgoing.RoomErrorToEnter);
                        Response.AppendInt32(1);
                        response.appendResponse(GetResponse());

                        Response.Init(Outgoing.OutOfRoom);
                        response.appendResponse(GetResponse());

                        response.sendResponse();
                    }

                    return;
                }
            }

            if (!Session.GetHabbo().HasFuse("fuse_enter_any_room") && !Room.CheckRights(Session, false) && !Session.GetHabbo().IsTeleporting)
            {
                if (Room.RoomData.State == 1)
                {
                    if (Room.UserCount == 0)
                    {
                        // Aww nobody in da room!

                        Response.Init(Outgoing.DoorBellNoPerson);
                        response.appendResponse(GetResponse());
                    }
                    else
                    {
                        // Waiting for answer!
                        Session.GetHabbo().CurrentRingId = Room.RoomId;

                        Response.Init(Outgoing.Doorbell);
                        Response.AppendString("");
                        response.appendResponse(GetResponse());

                        ServerMessage RingMessage = new ServerMessage(Outgoing.Doorbell);
                        RingMessage.AppendString(Session.GetHabbo().Username);
                        Room.SendMessageToUsersWithRights(RingMessage);
                    }

                    response.sendResponse();

                    return;
                }
                else if (Room.RoomData.State == 2)
                {
                    if (Password.ToLower() != Room.RoomData.Password.ToLower())
                    {
                        // your password fail :( !

                        Response.Init(Outgoing.RoomError);
                        Response.AppendInt32(-100002); // can be 4009 if you want something like 'need.to.be.vip'
                        response.appendResponse(GetResponse());

                        Response.Init(Outgoing.OutOfRoom);
                        response.appendResponse(GetResponse());

                        response.sendResponse();
                        return;
                    }
                }
            }

            saltamosPasos:

            Response.Init(Outgoing.PrepareRoomForUsers);
            response.appendResponse(GetResponse());

            Session.GetHabbo().comingRoom = 0;
            Session.GetHabbo().goToQueuedRoom = 0;
            Session.GetHabbo().LoadingChecksPassed = true;

            response.addBytes(LoadRoomForUser().getPacket);
            response.sendResponse();
        }

        internal void ReqLoadRoomForUser()
        {
            LoadRoomForUser().sendResponse();
        }

        internal QueuedServerMessage LoadRoomForUser()
        {
            var Room = CurrentLoadingRoom;

            var response = new QueuedServerMessage(Session.GetConnection());

            if (Room == null || !Session.GetHabbo().LoadingChecksPassed)
                return response;

            Response.Init(Outgoing.InitialRoomInformation);
            Response.AppendString(Room.RoomData.ModelName);
            Response.AppendUInt(Room.RoomId);
            response.appendResponse(GetResponse());

            
                if (Room.RoomData.temEmblema != "0")
                {
                    if (Session.GetHabbo().GetBadgeComponent().temEmblemaEquipado(Room.RoomData.temEmblema) == 0 && Session.GetHabbo().Rank < 5)
                        Session.GetHabbo().SpectatorMode = true;
                }

                if(Room.RoomData.Id == OtanixEnvironment.prisaoId() && !OtanixEnvironment.GetGame().GetPrisaoManager().estaPreso(Session.GetHabbo().Id) && Session.GetHabbo().Rank < 5)
                    Session.GetHabbo().SpectatorMode = true;

            if (Session.GetHabbo().SpectatorMode)
            {
                Response.Init(Outgoing.SpectatorMode);
                response.appendResponse(GetResponse());
            }

            if (Room.RoomData.OwnerId == Session.GetHabbo().Id && Session.GetHabbo().frankJaApareceu == false)
            {
                if (Session.GetHabbo().NewBot != 0)
                {
                Session.GetHabbo().frankJaApareceu = true;
                RoomUser BotUser = Room.GetRoomUserManager().DeployBot(new RoomBot(0, 999999, Session.GetHabbo().CurrentRoomId, AIType.Frank, true, "Frank",
                "Ajudante do Hotel", "M", "hr-3194-38-36.hd-180-1.ch-220-1408.lg-285-73.sh-906-90.ha-3129-73.fa-1206-73.cc-3039-73", 0, 0, 0, 0, true, "", 0, false), null);

                    if (Session.GetHabbo().Rank >= 0)
                    {
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("UPDATE users SET new_bot = '0' WHERE username = @username");
                            dbClient.addParameter("username", Session.GetHabbo().Username);
                            dbClient.runQuery();
                        }
                    }

                    Session.GetHabbo().NewBot = 0;

                }
            }

            if (Room.RoomData.Type == "private" || Room.RoomData.Type == "public")
            {
                if (Room.RoomData.Wallpaper != "0.0")
                {
                    Response.Init(Outgoing.RoomDecoration);
                    Response.AppendString("wallpaper");
                    Response.AppendString(Room.RoomData.Wallpaper);
                    response.appendResponse(GetResponse());
                }

                if (Room.RoomData.Floor != "0.0")
                {
                    Response.Init(Outgoing.RoomDecoration);
                    Response.AppendString("floor");
                    Response.AppendString(Room.RoomData.Floor);
                    response.appendResponse(GetResponse());
                }

                Response.Init(Outgoing.RoomDecoration);
                Response.AppendString("landscape");
                Response.AppendString(Room.RoomData.Landscape);
                response.appendResponse(GetResponse());

                int roomRank = Room.GetRightsLevel(Session);

                Response.Init(Outgoing.RoomRightsLevel);
                Response.AppendInt32(roomRank);
                response.appendResponse(GetResponse());

                if (roomRank == 4) // owner
                {
                    Response.Init(Outgoing.HasOwnerRights);
                    response.appendResponse(GetResponse());
                }

                Response.Init(Outgoing.ScoreMeter);
                Response.AppendInt32(Room.RoomData.Score);
                Response.AppendBoolean(!(Session.GetHabbo().RatedRooms.Contains(Room.RoomId) || Room.CheckRights(Session, true)));
                response.appendResponse(GetResponse());
            }

            if (Session.GetHabbo().FavoriteGroup > 0)
            {
                var Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(Session.GetHabbo().FavoriteGroup);
                if (Group != null)
                {
                    Room.QueueAddGroupUser(Group.Id);

                    var GuildSend = new ServerMessage(Outgoing.SendGroup);
                    GuildSend.AppendInt32(1);
                    GuildSend.AppendUInt(Group.Id);
                    GuildSend.AppendString(Group.GroupImage);
                    Room.SendMessage(GuildSend);
                }
                else
                {
                    Session.GetHabbo().FavoriteGroup = 0;
                }
            }

            return response;
        }

        internal void ClearRoomLoading()
        {
            Session.GetHabbo().LoadingRoom = 0;
            Session.GetHabbo().LoadingChecksPassed = false;
        }
        #endregion

        internal void GetInventory()
        {
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeItemInventory());
        }

        #region Room Main Actions

        internal void Talk()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || Room.GetRoomUserManager() == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (!Room.CheckRights(Session, false) && Room.muteSignalEnabled == true)
            {
                Session.SendNotif("O quarto foi mutado, você não pode falar até que seja desmutado.");
                return;
            }

            User.Chat(Session, OtanixEnvironment.FilterInjectionChars(Request.PopFixedString()), Request.PopWiredInt32(), false);
        }

        internal void Shout()
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (Room.CheckRights(Session, false) == false && Room.muteSignalEnabled == true)
            {
                Session.SendNotif("O quarto foi mutado, você não pode falar até que seja desmutado.");
                return;
            }

            User.Chat(Session, OtanixEnvironment.FilterInjectionChars(Request.PopFixedString()), Request.PopWiredInt32(), true);
        }

        internal void Whisper()
        {
            #region Checks
            if (Session == null || Session.GetHabbo() == null) // si el usuario ya está desconectado, pasamos de todo
                return;

            var Params = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            var ToUser = Params.Split(' ')[0];
            var Message = Params.Substring(ToUser.Length + 1);

            if (Message.Length > 100) // si el mensaje es mayor que la máxima longitud (scripter)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null || User.IsBot)
                return;
            #endregion
            #region Muted
            if (!Room.CheckRights(Session, true)) // Si no es un staff comprobamos si está muteado.
            {
                if (Room.RoomMuted)
                    return;

                int timeToEndRoomMute = Room.HasMuteExpired(Session.GetHabbo().Id);
                int timeToEndGlobalMute = OtanixEnvironment.GetGame().GetMuteManager().HasMuteExpired(Session.GetHabbo().Id);
                int timeMuted = (timeToEndGlobalMute > timeToEndRoomMute) ? timeToEndGlobalMute : timeToEndRoomMute;

                if (timeMuted > 0)
                {
                    ServerMessage message = new ServerMessage(Outgoing.MuteTimerMessageComposer);
                    message.AppendInt32(timeMuted);
                    Session.SendMessage(message);
                    return;
                }
            }
            #endregion
            #region Commands
            if (Message.StartsWith(":")) // Si el mensaje comienza por :
            {
                if (ChatCommandRegister.IsChatCommand(Message.Split(' ')[0].ToLower().Substring(1))) // si está en nuestra lista de comandos
                {
                    ChatCommandHandler handler = new ChatCommandHandler(Message.Split(' '), Session, Room, User);

                    try
                    {
                        if (handler.WasExecuted())
                            return;
                    }
                    finally
                    {
                        handler.Dispose();
                    }
                }
            }
            #endregion
            #region Flood
            if (!Session.GetHabbo().HasFuse("ignore_flood_filter") && Session.GetHabbo().Id != Room.RoomData.OwnerId && !User.IsBot)
            {
                TimeSpan SinceLastMessage = DateTime.Now - Session.GetHabbo().spamFloodTime;
                if (SinceLastMessage.Seconds > 3)
                {
                    FloodCount = 0;
                }
                else if (FloodCount > 5)
                {
                    ServerMessage Packet = new ServerMessage(Outgoing.FloodFilter);
                    Packet.AppendInt32(30);
                    Session.SendMessage(Packet);

                    OtanixEnvironment.GetGame().GetMuteManager().AddUserMute(Session.GetHabbo().Id, 0.5);
                    return;
                }
                Session.GetHabbo().spamFloodTime = DateTime.Now;
                FloodCount++;
            }
            #endregion
            #region Filter
            if (!Session.GetHabbo().HasFuse("ignore_spam_filter"))
            {
                if (BlackWordsManager.Check(Message, BlackWordType.Hotel, Session, "<Susurro: Sala " + Session.GetHabbo().CurrentRoomId + ">"))
                    return;

                if (BlackWordsManager.CheckRoomFilter(Message, Room.RoomFilterWords))
                    return;
            }
            #endregion
            #region Show Message Progress
            var Color = Request.PopWiredInt32();

            var User2 = Room.GetRoomUserManager().GetRoomUserByHabbo(ToUser);
            if (User2 == null || User2.IsBot || User2.GetClient() == null || User2.GetClient().GetHabbo() == null)
                return;

            if (User.HabboId == User2.HabboId)
                return;

            User.Unidle();

            if (Session.GetHabbo().Rank < 2 && EmuSettings.CHAT_TYPES_USERS.Contains(Color))
                Color = 0;

            // if (Session.GetHabbo().GetBadgeComponent().HasBadge(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_BADGE) && Session.GetHabbo().GetBadgeComponent().GetBadge(OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_BADGE).Slot > 0 && OtanixEnvironment.GetGame().GetRoomRankConfig().ROOMS_TO_MODIFY.Contains((int)Room.RoomId))
            //     Color = OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR;

            var ChatMessage = new ServerMessage(Outgoing.Whisp);
            ChatMessage.AppendInt32(User.VirtualId);
            ChatMessage.AppendString(Message.Replace("<", "¤"));
            ChatMessage.AppendInt32(RoomUser.GetSpeechEmotion(Message));
            ChatMessage.AppendInt32(Color);
            ChatMessage.AppendInt32(0);
            ChatMessage.AppendInt32(-1);
            User.GetClient().SendMessage(ChatMessage);
            if (!User2.GetClient().GetHabbo().MutedUsers.Contains(Session.GetHabbo().Username) && !User2.IgnoreChat)
                User2.GetClient().SendMessage(ChatMessage);

            var ToNotify = Room.GetRoomUserManager().GetRoomUserByFuse("fuse_whisper");
            if (ToNotify.Count > 0)
            {
                ChatMessage = new ServerMessage(Outgoing.Whisp);
                ChatMessage.AppendInt32(User.VirtualId);
                ChatMessage.AppendString(LanguageLocale.GetValue("moderation.whisper") + ToUser + ": " + Message.Replace("<", "¤"));
                ChatMessage.AppendInt32(RoomUser.GetSpeechEmotion(Message));
                ChatMessage.AppendInt32(Color);
                ChatMessage.AppendInt32(0);
                ChatMessage.AppendInt32(-1);

                foreach (var user in ToNotify)
                    if (user != null && User2 != null)
                        if (user.HabboId != User2.HabboId && user.HabboId != User.HabboId)
                            if (user.GetClient() != null)
                                user.GetClient().SendMessage(ChatMessage);
            }

            SpyChatMessage.SaveUserLog(Session.GetHabbo().Id, Room.RoomId, User2.HabboId, Message);
            var Mess = new ChatMessage(Session.GetHabbo().Id, Session.GetHabbo().Username, Room.RoomId, Message + " -> (" + User2.GetUsername() + ")", DateTime.Now, false); // creamos la clase para el Mensaje
            Session.GetHabbo().GetChatMessageManager().AddMessage(Mess); // Mod Tools: User Message
            Room.GetChatMessageManager().AddMessage(Mess); // Mod Tools: Room Message
            #endregion
        }

        internal void Move()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || Room.GetRoomUserManager() == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null || !User.CanWalk)
                return;

            if (Session.GetHabbo().NewIdentity != 0 && Session.GetHabbo().NewUserInformationStep == 0)
            {
                Session.SendMessage(NuxUserInformation.ShowInformation(0));
                Session.GetHabbo().NewUserInformationStep++;

                ServerMessage Alert = new ServerMessage(Outgoing.CustomAlert);
                Alert.AppendString("furni_placement_error");
                Alert.AppendInt32(2);
                Alert.AppendString("message");
                Alert.AppendString(LanguageLocale.GetValue("register.message").Replace("{username}", Session.GetHabbo().Username));
                Alert.AppendString("image");
                Alert.AppendString("${image.library.url}notifications/" + (Session.GetHabbo().Gender.ToLower() == "f" ? LanguageLocale.GetValue("register.image.female") : LanguageLocale.GetValue("register.image.male")) + ".png");
                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(Alert, "fuse_mod", 0);

            }

            int MoveX = Request.PopWiredInt32();
            int MoveY = Request.PopWiredInt32();

            // Permite hacer el Seat Rápido
            if (MoveX == User.X && MoveY == User.Y)
            {
                if (!User.IsWalking)
                    return;
                else
                    User.SeatCount++;

                if (User.SeatCount == 5)
                    return;
            }

            if (User.walkingToPet != null)
            {
                User.walkingToPet.Freezed = false;
                User.walkingToPet = null;
            }

            User.MoveTo(MoveX, MoveY);
        }
        #endregion

        internal void CanCreateRoom()
        {
            Response.Init(Outgoing.CanCreateRoom);
            Response.AppendInt32(Session.GetHabbo().UsersRooms.Count >= 100 ? 1 : 0); // true = show error with number below
            Response.AppendInt32(100); // max rooms
            SendResponse();
        }

        internal void CreateRoom()
        {
            var RoomName = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            var RoomDescription = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            var ModelName = Request.PopFixedString();
            var RoomType = Request.PopWiredInt32();
            var MaxUsers = Request.PopWiredInt32();
            var TradeSettings = Request.PopWiredInt32();

            var NewRoom = OtanixEnvironment.GetGame().GetRoomManager().CreateRoom(Session, RoomName, RoomDescription, ModelName, RoomType, MaxUsers, TradeSettings);

            if (NewRoom != null)
            {
                Response.Init(Outgoing.OnCreateRoomInfo);
                Response.AppendUInt(NewRoom.Id);
                Response.AppendString(NewRoom.Name);
                SendResponse();
            }
        }

        internal void GetRoomEditData()
        {
            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            GetResponse().Init(Outgoing.RoomDataEdit);
            GetResponse().AppendUInt(Room.RoomData.Id);
            GetResponse().AppendString(Room.RoomData.Name);
            GetResponse().AppendString(Room.RoomData.Description);
            GetResponse().AppendInt32(Room.RoomData.State);
            GetResponse().AppendInt32(Room.RoomData.Category);
            GetResponse().AppendUInt(Room.RoomData.UsersMax > 75 ? 75 : Room.RoomData.UsersMax);
            GetResponse().AppendInt32(((Room.RoomData.Model.MapSizeX * Room.RoomData.Model.MapSizeY) > 100) ? 50 : 25); // Max can be elected
            GetResponse().AppendInt32(Room.RoomData.Tags.Count);

            foreach (string Tag in Room.RoomData.Tags.ToArray())
            {
                GetResponse().AppendString(Tag);
            }

            GetResponse().AppendInt32(Room.RoomData.TradeSettings); // Trade Settings
            GetResponse().AppendInt32(Room.RoomData.AllowPets ? 1 : 0); // allows pets in room - pet system lacking, so always off
            GetResponse().AppendInt32(Room.RoomData.AllowPetsEating ? 1 : 0); // allows pets to eat your food - pet system lacking, so always off
            GetResponse().AppendInt32(Room.RoomData.AllowWalkthrough ? 1 : 0);
            GetResponse().AppendInt32(Room.RoomData.Hidewall ? 1 : 0);
            GetResponse().AppendInt32(Room.RoomData.WallThickness);
            GetResponse().AppendInt32(Room.RoomData.FloorThickness);
            GetResponse().AppendInt32(Room.RoomData.BubbleMode); // 0 = Free Flow Mode (bubbles can pass) / 1 = Line-by-Line-Mode (old)
            GetResponse().AppendInt32(Room.RoomData.BubbleType); // 0 = Wide bubbles / 1 = Normal bubbles / 2 = Thin bubbles
            GetResponse().AppendInt32(Room.RoomData.BubbleScroll); // 0 = Fast scrolling up / 1 = Normal scrolling up / 2 = Slow scrolling up
            GetResponse().AppendInt32(Room.RoomData.ChatDistance); // buble lenght
            GetResponse().AppendInt32(Room.RoomData.AntiFloodSettings); // Standart Anti Flood Protection
            GetResponse().AppendBoolean(false); // ??
            GetResponse().AppendInt32(Room.RoomData.MuteFuse); // 0 = moderation_mute_none, 1 = moderation_mute_rights
            GetResponse().AppendInt32(Room.RoomData.KickFuse); // 0 = moderation_kick_none, 1 = moderation_kick_rights, 2 = moderation_kick_all
            GetResponse().AppendInt32(Room.RoomData.BanFuse); // 0 = moderation_ban_none, 1 = moderation_ban_rigths
            SendResponse();

            if (Room.UsersWithRights.Count > 0)
            {
                GetResponse().Init(Outgoing.GetPowerList);
                GetResponse().AppendUInt(Room.RoomData.Id);
                GetResponse().AppendInt32(Room.UsersWithRights.Count);
                foreach (var i in Room.UsersWithRights)
                {
                    var xUser = UsersCache.getHabboCache(i);
                    if (xUser == null)
                    {
                        GetResponse().AppendUInt(0);
                        GetResponse().AppendString("");
                    }
                    else
                    {
                        GetResponse().AppendUInt(xUser.Id);
                        GetResponse().AppendString(xUser.Username);
                    }
                }
                SendResponse();
            }
        }

        internal void SaveRoomData()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || Room.RoomData == null|| !Room.CheckRights(Session, true))
                return;

            var Id = Request.PopWiredInt32();
            var Name = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            var Description = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            var State = Request.PopWiredInt32();
            var Password = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            var MaxUsers = Request.PopWiredUInt();
            var CategoryId = Request.PopWiredInt32();
            var TagCount = Request.PopWiredInt32();

            var Tags = new List<string>();
            var formattedTags = new StringBuilder();

            for (var i = 0; i < TagCount; i++)
            {
                if (i > 0)
                {
                    formattedTags.Append(",");
                }

                var tag = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString().ToLower());

                Tags.Add(tag);
                formattedTags.Append(tag);
            }

            var TradeSettings = Request.PopWiredInt32();
            var AllowPets = Request.PopWiredBoolean();
            var AllowPetsEat = Request.PopWiredBoolean();
            var AllowWalkthrough = Request.PopWiredBoolean();
            var Hidewall = Request.PopWiredBoolean();
            var WallThickness = Request.PopWiredInt32();
            var FloorThickness = Request.PopWiredInt32();
            var MuteFuse = Request.PopWiredInt32();
            var KickFuse = Request.PopWiredInt32();
            var BanFuse = Request.PopWiredInt32();
            var BubbleMode = Request.PopWiredInt32();
            var BubbleType = Request.PopWiredInt32();
            var BubbleScroll = Request.PopWiredInt32();
            var ChatDistance = Request.PopWiredInt32();
            var AntiFloodSettings = Request.PopWiredInt32();
            var unk = Request.PopWiredBoolean();

            var lastState = Room.RoomData.State;

            if (TradeSettings < 0 || TradeSettings > 2)
            {
                TradeSettings = 0;
            }

            if (AntiFloodSettings < 0 || AntiFloodSettings > 2)
            {
                AntiFloodSettings = 0;
            }

            if (ChatDistance < 3)
            {
                ChatDistance = 3;
            }

            if (ChatDistance > 99)
            {
                ChatDistance = 14;
            }

            if (WallThickness < -2 || WallThickness > 1)
            {
                WallThickness = 0;
            }

            if (FloorThickness < -2 || FloorThickness > 1)
            {
                FloorThickness = 0;
            }

            if (Name.Length < 1)
            {
                return;
            }

            if (BubbleMode < 0 || BubbleMode > 1)
            {
                return;
            }

            if (BubbleType < 0 || BubbleType > 2 || BubbleScroll < 0 || BubbleScroll > 2)
                return;

            if (State < 0 || State > 3)
            {
                return;
            }

            if (MaxUsers < 10 || MaxUsers > 75)
            {
                return;
            }

            var FlatCat = OtanixEnvironment.GetGame().GetNavigator().GetFlatCat(CategoryId);

            if (FlatCat == null)
            {
                return;
            }

            if (FlatCat.MinRank > Session.GetHabbo().Rank)
            {
                Session.SendNotif(LanguageLocale.GetValue("user.roomdata.rightserror"));
                CategoryId = 0;
            }

            if (TagCount > 2)
            {
                return;
            }

            if (MuteFuse != 0 && MuteFuse != 1)
                return;

            if (KickFuse != 0 && KickFuse != 1 && KickFuse != 2)
                return;

            if (BanFuse != 0 && BanFuse != 1)
                return;

            Room.RoomData.TradeSettings = TradeSettings;
            Room.RoomData.AllowPets = AllowPets;
            Room.RoomData.AllowPetsEating = AllowPetsEat;
            Room.RoomData.AllowWalkthrough = AllowWalkthrough;
            Room.RoomData.Hidewall = Hidewall;
            Room.RoomData.ChatDistance = ChatDistance;
            Room.RoomData.AntiFloodSettings = AntiFloodSettings;
            Room.RoomData.Name = Name;
            Room.RoomData.State = State;
            Room.RoomData.Description = Description;
            Room.RoomData.Category = CategoryId;
            Room.RoomData.Password = Password;
            Room.RoomData.Tags.Clear();
            Room.RoomData.Tags.AddRange(Tags);
            Room.RoomData.UsersMax = MaxUsers;
            Room.RoomData.WallThickness = WallThickness;
            Room.RoomData.FloorThickness = FloorThickness;
            Room.RoomData.MuteFuse = MuteFuse;
            Room.RoomData.KickFuse = KickFuse;
            Room.RoomData.BanFuse = BanFuse;
            Room.RoomData.BubbleMode = BubbleMode;
            Room.RoomData.BubbleType = BubbleType;
            Room.RoomData.BubbleScroll = BubbleScroll;

            switch (Room.RoomData.State)
            {
                case 0:

                    if (lastState == 3)
                        OtanixEnvironment.GetGame().GetRoomManager().QueueActiveRoomAdd(Room.RoomData);
                    break;

                case 1:

                    if (lastState == 3)
                        OtanixEnvironment.GetGame().GetRoomManager().QueueActiveRoomAdd(Room.RoomData);
                    break;
                case 2:

                    if (lastState == 3)
                        OtanixEnvironment.GetGame().GetRoomManager().QueueActiveRoomAdd(Room.RoomData);
                    break;

                case 3:

                    if (lastState != 3)
                        OtanixEnvironment.GetGame().GetRoomManager().QueueActiveRoomRemove(Room.RoomData);
                    break;

            }

            Room.RoomData.roomNeedSqlUpdate = true;

            GetResponse().Init(Outgoing.UpdateRoomOne);
            GetResponse().AppendUInt(Room.RoomId);
            SendResponse();

            GetResponse().Init(Outgoing.ConfigureWallandFloor);
            GetResponse().AppendBoolean(Room.RoomData.Hidewall);
            GetResponse().AppendInt32(Room.RoomData.WallThickness);
            GetResponse().AppendInt32(Room.RoomData.FloorThickness);
            Session.GetHabbo().CurrentRoom.SendMessage(GetResponse());

            GetResponse().Init(Outgoing.LoadBubblesSettings);
            GetResponse().AppendInt32(Room.RoomData.BubbleMode);
            GetResponse().AppendInt32(Room.RoomData.BubbleType);
            GetResponse().AppendInt32(Room.RoomData.BubbleScroll);
            GetResponse().AppendInt32(ChatDistance);
            GetResponse().AppendInt32(Room.RoomData.AntiFloodSettings);
            Session.GetHabbo().CurrentRoom.SendMessage(GetResponse());

            ServerMessage Update = new ServerMessage(Outgoing.UpdateRoom);
            Update.AppendUInt(Room.Id);
            Room.SendMessage(Update);

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModDoorModeSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModWalkthroughSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModChatScrollSpeedSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModChatFloodFilterSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModChatHearRangeSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModIgnoreSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModMuteSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModKickSeen", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModBanSeen", 1);
        }

        internal void ReloadRights()
        {
            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            //Console.WriteLine("Loaded room :" + CurrentLoadingRoom.UsersWithRights.Count);
            if (!Room.IsPublic && Room.UsersWithRights.Count > 0 && Room.CheckRights(Session, true))
            {
                var powerList = new ServerMessage(Outgoing.GetPowerList);
                powerList.AppendInt32((int)Room.RoomData.Id);
                powerList.AppendInt32(Room.UsersWithRights.Count);
                foreach (var i in Room.UsersWithRights)
                {
                    var xUser = UsersCache.getHabboCache(i);
                    if (xUser == null)
                    {
                        powerList.AppendInt32(0);
                        powerList.AppendString("");
                    }
                    else
                    {
                        powerList.AppendInt32((int)xUser.Id);
                        powerList.AppendString(xUser.Username);
                    }
                }
                Session.SendMessage(powerList);

                foreach (var i in Room.UsersWithRights)
                {
                    var xUser = UsersCache.getHabboCache(i);
                    var appendPowers = new ServerMessage(Outgoing.GivePowers);
                    appendPowers.AppendUInt(Room.RoomId);
                    appendPowers.AppendUInt(xUser.Id);
                    appendPowers.AppendString(xUser.Username);
                    Session.SendMessage(appendPowers);
                }
            }
        }

        internal void GiveRights()
        {
            var _UserId = Request.PopWiredInt32();
            if (_UserId <= 0)
                return;

            var UserId = Convert.ToUInt32(_UserId);

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var RoomUser = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);

            if (Room == null || !Room.CheckRights(Session, true) || RoomUser == null || RoomUser.GetClient() == null || RoomUser.GetClient().GetHabbo() == null)
            {
                return;
            }

            if (Room.UsersWithRights.Contains(UserId))
            {
                /*Response.Init(Outgoing.GivePowers);
                Response.AppendUInt(Room.RoomId);
                Response.AppendUInt(UserId);
                Response.AppendStringWithBreak(ButterflyEnvironment.getHabboForId(UserId).Username);
                SendResponse();*/

                Session.SendNotif(LanguageLocale.GetValue("user.giverights.error"));
                return;
            }

            Room.UsersWithRights.Add(UserId);

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("INSERT INTO room_rights (room_id,user_id) VALUES (" + Room.RoomId + "," + UserId + ")");
            }

            Response.Init(Outgoing.GivePowers);
            Response.AppendUInt(Room.RoomId);
            Response.AppendUInt(UserId);
            Response.AppendString(RoomUser.GetClient().GetHabbo().Username);
            SendResponse();

            if (RoomUser == null || RoomUser.IsBot)
            {
                return;
            }

            RoomUser.RemoveStatus("flatctrl 0");
            RoomUser.AddStatus("flatctrl 1", "");
            RoomUser.UpdateNeeded = true;

            if (RoomUser != null && !RoomUser.IsBot)
            {
                Response.Init(Outgoing.RoomRightsLevel);
                Response.AppendInt32(1);
                RoomUser.GetClient().SendMessage(GetResponse());
            }
        }

        internal void TakeRights()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            var DeleteParams = new StringBuilder();

            var Amount = Request.PopWiredInt32();

            for (var i = 0; i < Amount; i++)
            {
                if (i > 0)
                {
                    DeleteParams.Append(" OR ");
                }

                var UserId = (uint)Request.PopWiredInt32();
                Room.UsersWithRights.Remove(UserId);
                DeleteParams.Append("room_id = '" + Room.RoomId + "' AND user_id = '" + UserId + "'");

                var User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);

                if (User != null && !User.IsBot)
                {
                    Response.Init(Outgoing.RoomRightsLevel);
                    Response.AppendInt32(0);
                    User.GetClient().SendMessage(GetResponse());
                }

                // GhntX]hqu@U
                Response.Init(Outgoing.RemovePowers);
                Response.AppendUInt(Room.RoomId);
                Response.AppendUInt(UserId);
                SendResponse();

                if (User != null)
                {
                    User.RemoveStatus("flatctrl 1");
                    User.AddStatus("flatctrl 0", "");
                    User.UpdateNeeded = true;
                }
            }

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM room_rights WHERE " + DeleteParams);
            }

        }

        internal void TakeAllRights()
        {
            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            foreach (var UserId in Room.UsersWithRights)
            {
                var User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);

                if (User != null && !User.IsBot)
                {
                    Response.Init(Outgoing.RoomRightsLevel);
                    Response.AppendInt32(0);
                    User.GetClient().SendMessage(GetResponse());
                }

                Response.Init(Outgoing.RemovePowers);
                Response.AppendUInt(Room.RoomId);
                Response.AppendUInt(UserId);
                SendResponse();

                if (User != null) User.UpdateNeeded = true;
            }

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM room_rights WHERE room_id = " + Room.RoomId);
            }

            Room.UsersWithRights.Clear();
        }

        internal void KickUser(RoomUser bUser = null)
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (!Session.GetHabbo().HasFuse("fuse_room_kick"))
            {
                if ((Room.RoomData.KickFuse == 1 && !Room.CheckRights(Session)) || (Room.RoomData.KickFuse == 0 && !Room.CheckRights(Session, true)))
                    return;
            }

            var User = ((bUser != null) ? bUser : Room.GetRoomUserManager().GetRoomUserByHabbo(Request.PopWiredUInt()));

            if (User == null || User.IsBot)
            {
                return;
            }

            if (Session.GetHabbo().HasFuse("fuse_user_kick"))
            {
                if (Session.GetHabbo().Rank < User.GetClient().GetHabbo().Rank)
                    return;
            }
            else
            {
                if (Room.GetRightsLevel(User.GetClient()) == 4 || User.GetClient().GetHabbo().HasFuse("fuse_mod"))
                    return;
            }

            User.MoveTo(Room.GetGameMap().Model.DoorX, Room.GetGameMap().Model.DoorY);
            User.isKicking = true;
        }

        internal void BanUser()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || (Room.RoomData.BanFuse == 1 && !Room.CheckRights(Session)) || (Room.RoomData.BanFuse == 0 && !Room.CheckRights(Session, true)))
                return; // insufficient permissions

            var UserId = Request.PopWiredUInt();
            var JUNK = Request.PopWiredUInt();
            var time = Request.PopFixedString();
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
            var Time = 0;
            if (time.Equals("RWUAM_BAN_USER_HOUR"))
                Time = 3600;
            else if (time.Equals("RWUAM_BAN_USER_DAY"))
                Time = (24 * 3600);
            else if (time.Equals("RWUAM_BAN_USER_PERM"))
                Time = (Int32.MaxValue / 5);
            else
                return;

            if (User == null || User.IsBot)
            {
                return;
            }

            if (Session.GetHabbo().HasFuse("fuse_user_kick"))
            {
                if (Session.GetHabbo().Rank < User.GetClient().GetHabbo().Rank)
                    return;
            }
            else
            {
                if (Room.GetRightsLevel(User.GetClient()) == 4 || User.GetClient().GetHabbo().HasFuse("fuse_mod"))
                    return;
            }

            Room.AddBan(UserId, Time);
            User.MoveTo(Room.GetGameMap().Model.DoorX, Room.GetGameMap().Model.DoorY);
            User.isKicking = true;
        }

        /// <summary>
        /// Función que mutea solo al usuario en la sala donde está.
        /// </summary>
        internal void MuteUser()
        {
            var UserId = Request.PopWiredUInt();
            var RoomId = Request.PopWiredUInt();
            var Time = Request.PopWiredInt32(); // minutes

           // var Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
           // if (Room == null || (Room.RoomData.MuteFuse == 1 && !Room.CheckRights(Session)) || (Room.RoomData.MuteFuse == 0 && !Room.CheckRights(Session, true)))
           //     return;

          //  var User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
          //  if (User == null || User.IsBot || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().HasFuse("fuse_mod"))
          //      return;

            //Room.AddMute(UserId, Time);

            // Pegamos as infos do usuário
            Habbo TargetHabbo = UsersCache.getHabboCache(UserId);

            // Se o usuário existe
            if (TargetHabbo == null)
                return;
            
            // Não da pra mutar usuário com rank maior que o seu
            if (TargetHabbo.Rank >= Session.GetHabbo().Rank)
                return;

            // Salvamos os log
            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetHabbo.Username, "Mute", "<Embaixador> Mutou o usuário");

            // Muta o usuário
            ModerationTool.MuteUser(Session, TargetHabbo, Time, "");

        }

        public void MuteRoom()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            Room.RoomMuted = !Room.RoomMuted;

            ServerMessage Message = new ServerMessage(Outgoing.MuteRoomUpdate);
            Message.AppendBoolean(Room.RoomMuted);
            Room.SendMessage(Message);
        }

        internal void ReloadBans()
        {
            uint RoomId = Request.PopWiredUInt();

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            if (Room == null)
                return;

            Room.RefreshBans();

            GetResponse().Init(Outgoing.ReloadBans);
            GetResponse().AppendUInt(RoomId);
            GetResponse().AppendInt32(Room.Bans.Count);
            foreach (uint UserId in Room.Bans.Keys)
            {
                GetResponse().AppendUInt(UserId);
                GetResponse().AppendString(UsersCache.getUsernameById(UserId));
            }
            SendResponse();
        }

        internal void RemoveBans()
        {
            uint UserId = Request.PopWiredUInt();
            uint RoomId = Request.PopWiredUInt();

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            if (Room == null)
                return;

            if (Room.UserIsBanned(UserId))
            {
                Room.RemoveBan(UserId);

                GetResponse().Init(Outgoing.UnbanUser);
                GetResponse().AppendUInt(RoomId);
                GetResponse().AppendUInt(UserId);
                SendResponse();
            }
        }

        internal void SetHomeRoom()
        {
            uint RoomId = Request.PopWiredUInt();

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                // ¡COMPROBAMOS SI LA SALA EXISTE!
                dbClient.setQuery("SELECT COUNT(*) FROM rooms WHERE id = " + RoomId + " LIMIT 1");
                if (dbClient.getInteger() < 1)
                    return;
            }

            Session.GetHabbo().HomeRoom = RoomId;

            Response.Init(Outgoing.HomeRoom);
            Response.AppendUInt(RoomId);
            Response.AppendUInt(RoomId);
            SendResponse();
        }

        internal void DeleteRoom()
        {
            var RoomId = Request.PopWiredUInt();
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().UsersRooms == null)
                return;

            var TargetRoom = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            if (TargetRoom == null)
                return;

            if (TargetRoom.RoomData.IsPublicRoom && !Session.GetHabbo().HasFuse("fuse_sysadmin"))
            {
                Session.SendNotif("Não pode excluir este quarto pois ele aparece no navegador.");
                return;
            }

            if (TargetRoom.RoomData.OwnerId == Session.GetHabbo().Id || Session.GetHabbo().HasFuse("fuse_any_room_rights"))
            {
                // Obtenemos la lista de mascotas en sala.
                List<Pet> Pets = TargetRoom.GetRoomUserManager().GetPets();
                if (Pets.Count > 0)
                {
                    foreach (Pet Pet in Pets)
                    {
                        // Si no es su dueño, se queda.
                        if (Pet.OwnerId != Session.GetHabbo().Id)
                            continue;

                        // Marcamos que necesita actualización.
                        Pet.RoomId = 0;
                        Pet.DBState = DatabaseUpdateState.NeedsUpdate;
                    }

                    // Guardamos en SQL.
                    using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        TargetRoom.GetRoomUserManager().SavePets(dbClient);
                    }

                    // Quitamos a los usuarios de la sala.
                    foreach (Pet Pet in Pets)
                    {
                        // Si no es su dueño, se queda.
                        if (Pet.OwnerId != Session.GetHabbo().Id)
                            continue;

                        // Añadimos la mascota al inventario.
                        Session.GetHabbo().GetInventoryComponent().AddPet(Pet);

                        // Quitamos la mascota de la sala.
                        TargetRoom.GetRoomUserManager().RemoveBot(Pet.VirtualId, false);
                    }

                    // Actualizamos el inventario.
                    Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
                }

                // Obtenemos la lista de mascotas en sala.
                List<RoomUser> Bots = TargetRoom.GetRoomUserManager().GetBots;
                if (Bots.Count > 0)
                {
                    foreach (RoomUser Bot in Bots)
                    {
                        // Si no es su dueño, se queda.
                        if (Bot.BotData.OwnerId != Session.GetHabbo().Id)
                            continue;

                        RoomBot BotData = Bot.BotData;
                        if (BotData == null)
                            continue;

                        // Actualizamos variables.
                        BotData.RoomId = 0;
                        BotData.X = 0;
                        BotData.Y = 0;

                        // Guardamos en SQL.
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor()) // Super Stable Method [RELEASE 135]
                        {
                            dbClient.runFastQuery("REPLACE INTO items_users VALUES (" + BotData.BotId + "," + BotData.OwnerId + ")");
                            dbClient.runFastQuery("DELETE FROM items_rooms WHERE item_id = " + BotData.BotId);

                            dbClient.setQuery("UPDATE bots SET name = @botname, is_dancing = '" + ((Bot.IsDancing) ? "1" : "0") + "', walk_enabled = '" + ((BotData.WalkingEnabled) ? "1" : "0") + "', chat_enabled = '" + ((BotData.ChatEnabled) ? "1" : "0") + "', chat_text = @chttext, chat_seconds = '" + BotData.ChatSeconds + "', look = @look, gender = @gender, x = " + BotData.X + ", y = " + BotData.Y + " WHERE id = " + BotData.BotId);
                            dbClient.addParameter("look", BotData.Look);
                            dbClient.addParameter("gender", BotData.Gender);
                            dbClient.addParameter("chttext", BotData.ChatText);
                            dbClient.addParameter("botname", BotData.Name);
                            dbClient.runQuery();
                        }

                        // Añadimos el bot al inventario.
                        Session.GetHabbo().GetInventoryComponent().AddBot(Bot.BotData);

                        // Quitamos la mascota de la sala.
                        TargetRoom.GetRoomUserManager().RemoveBot(Bot.VirtualId, false);
                    }

                    // Actualizamos el inventario.
                    Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
                }

                TargetRoom.GetRoomItemHandler().RemoveAllFurniture(Session);

                OtanixEnvironment.GetGame().GetRoomManager().UnloadRoom(TargetRoom);

                if (TargetRoom.RoomData.GroupId > 0)
                {
                    GroupItem grupo = OtanixEnvironment.GetGame().GetGroup().LoadGroup(TargetRoom.RoomData.GroupId);
                    if(grupo != null)
                        grupo.Delete();
                }

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("DELETE FROM rooms WHERE id = " + RoomId);
                    dbClient.runFastQuery("DELETE FROM user_favorites WHERE room_id = " + RoomId);
                    dbClient.runFastQuery("DELETE FROM room_rights WHERE room_id = " + RoomId);

                    if (OtanixEnvironment.GetGame().GetRoomManager().votedRooms.ContainsKey(TargetRoom.RoomData))
                        dbClient.runFastQuery("DELETE FROM room_voted WHERE room_id = " + RoomId);
                }

                OtanixEnvironment.GetGame().GetRoomManager().QueueVoteRemove(TargetRoom.RoomData);

                if (Session.GetHabbo().HasFuse("fuse_any_room_rights"))
                {
                    OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetRoom.RoomData.Name, "Room deletion", string.Format("Deleted room ID {0}", TargetRoom.RoomData.Id));
                }

               
                if (Session.GetHabbo().UsersRooms.Contains(RoomId))
                    Session.GetHabbo().UsersRooms.Remove(RoomId);
            }
        }

        internal void LookAt()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || Room.GetRoomUserManager() == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            User.Unidle();

            int X = Request.PopWiredInt32();
            int Y = Request.PopWiredInt32();

            if (X == User.X && Y == User.Y)
            {
                return;
            }

            int Rot = Rotation.Calculate(User.X, User.Y, X, Y);

            User.SetRot(Rot, false);
            User.UpdateNeeded = true;
        }

        internal void StartTyping()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || Room.GetRoomUserManager() == null)
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            var Message = new ServerMessage(Outgoing.TypingStatus);
            Message.AppendInt32(User.VirtualId);
            Message.AppendInt32(1);
            Room.SendMessage(Message);
        }

        internal void StopTyping()
        {
            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || Room.GetRoomUserManager() == null)
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            var Message = new ServerMessage(Outgoing.TypingStatus);
            Message.AppendInt32(User.VirtualId);
            Message.AppendInt32(0);
            Room.SendMessage(Message);
        }

        internal void IgnoreUser()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            string username = Request.PopFixedString();

            GameClient gameClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (gameClient == null || gameClient.GetHabbo() == null)
                return;

            if (!Session.GetHabbo().MutedUsers.Contains(gameClient.GetHabbo().Username))
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("INSERT INTO user_ignores VALUES ('" + Session.GetHabbo().Id + "',@IgnoreName)");
                    dbClient.addParameter("IgnoreName", gameClient.GetHabbo().Username);
                    dbClient.runQuery();
                }

                Session.GetHabbo().MutedUsers.Add(gameClient.GetHabbo().Username);
            }

            Response.Init(Outgoing.UpdateIgnoreStatus);
            Response.AppendInt32(1);
            Response.AppendString(gameClient.GetHabbo().Username);
            SendResponse();
        }

        internal void UnignoreUser()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            string username = Request.PopFixedString();

            GameClient gameClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (gameClient == null || gameClient.GetHabbo() == null)
                return;

            if (Session.GetHabbo().MutedUsers.Contains(gameClient.GetHabbo().Username))
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("DELETE FROM user_ignores WHERE user_id = '" + Session.GetHabbo().Id + "' AND ignore_id = @IgnoreName");
                    dbClient.addParameter("IgnoreName", gameClient.GetHabbo().Username);
                    dbClient.runQuery();
                }

                Session.GetHabbo().MutedUsers.Remove(gameClient.GetHabbo().Username);
            }

            Response.Init(Outgoing.UpdateIgnoreStatus);
            Response.AppendInt32(3);
            Response.AppendString(gameClient.GetHabbo().Username);
            SendResponse();
        }

        internal void StartEvent()
        {
            uint a = Request.PopWiredUInt();
            int b = Request.PopWiredInt32();
            uint roomId = Request.PopWiredUInt();
            string name = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            bool c = Request.PopWiredBoolean();
            string descr = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            int cat = Request.PopWiredInt32();

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().LoadRoom(roomId);
            if (Room == null || !Room.CheckRights(Session, true) /*|| Room.Event != null*/)
                return;

            if (cat <= 0 || cat > OtanixEnvironment.GetGame().GetNavigator().PromCatsCount)
                return;

            CatalogItem Item = OtanixEnvironment.GetGame().GetCatalog().FindItem(a);
            if (Item == null)
                return;

            if (Item.DiamondsCost > Session.GetHabbo().Diamonds)
                return;
 
            if (Item.DiamondsCost > 0)
            {
                Session.GetHabbo().Diamonds -= Item.DiamondsCost;
                Session.GetHabbo().UpdateExtraMoneyBalance();
            }

            if (Room.RoomData.Event == null)
            {
                Room.RoomData.Event = new RoomEvent(OtanixEnvironment.GetGame().GetRoomManager().GetEventManager().eventsCount, Room.RoomId, Room.RoomData.OwnerId, Room.RoomData.Owner, name, descr, 1, null, cat);
                OtanixEnvironment.GetGame().GetRoomManager().GetEventManager().QueueAddEvent(Room.RoomData);
            }
            else
            {
                Room.RoomData.Event.StartTime = DateTime.Now;
                Room.RoomData.Event.Name = name;
                Room.RoomData.Event.Description = descr;
            }

            Room.SendMessage(Room.RoomData.Event.Serialize());

            GetResponse().Init(Outgoing.PurchaseOKMessageOfferData);
            GetResponse().AppendUInt(5);
            GetResponse().AppendString("room_ad_plus_badge");
            GetResponse().AppendBoolean(false);
            GetResponse().AppendInt32(0);
            GetResponse().AppendInt32(5);
            GetResponse().AppendInt32(105);
            GetResponse().AppendBoolean(true);
            GetResponse().AppendInt32(1);
            GetResponse().AppendString("e");
            GetResponse().AppendInt32(0);
            GetResponse().AppendString("");
            GetResponse().AppendInt32(1);
            GetResponse().AppendInt32(0);
            GetResponse().AppendString("");
            GetResponse().AppendInt32(1);
            SendResponse();
        }

        internal void EditEvent()
        {
            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true) || Room.RoomData.Event == null)
            {
                return;
            }

            int category = Request.PopWiredInt32();
            string name = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());
            string descr = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());

            Room.RoomData.Event.Category = category;
            Room.RoomData.Event.Name = name;
            Room.RoomData.Event.Description = descr;
            Room.SendMessage(Room.RoomData.Event.Serialize());
        }

        internal void Wave()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
            {
                return;
            }

            User.Unidle();
            var Action = Request.PopWiredInt32();
            User.DanceId = 0;

            var Message = new ServerMessage(Outgoing.Action);
            Message.AppendInt32(User.VirtualId);
            Message.AppendInt32(Action);
            Room.SendMessage(Message);

            if (Action == 5) // idle
            {
                User.IsAsleep = true;

                var FallAsleep = new ServerMessage(Outgoing.IdleStatus);
                FallAsleep.AppendInt32(User.VirtualId);
                FallAsleep.AppendBoolean(User.IsAsleep);
                Room.SendMessage(FallAsleep);
            }

            OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_WAVE);
        }

        internal void Sign()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
            {
                return;
            }

            User.Unidle();
            var SignId = Request.PopWiredInt32();
            User.AddStatus("sign", Convert.ToString(SignId));
            User.UpdateNeeded = true;
        }

        internal void GetUserBadges()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Request.PopWiredUInt());

            if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.IsBot)
                return;

            Response.Init(Outgoing.UpdateBadges);
            Response.AppendUInt(User.GetClient().GetHabbo().Id);
            Response.AppendInt32(User.GetClient().GetHabbo().GetBadgeComponent().EquippedCount);

            foreach (Badge Badge in User.GetClient().GetHabbo().GetBadgeComponent().BadgeList.Values)
            {
                if (Badge.Slot <= 0)
                {
                    continue;
                }

                Response.AppendInt32(Badge.Slot);
                Response.AppendString(Badge.Code + Badge.Level);
            }

            SendResponse();
        }

        internal void RateRoom()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || Session.GetHabbo().RatedRooms.Contains(Room.RoomId) || Room.CheckRights(Session, true))
            {
                return;
            }

            var Rating = Request.PopWiredInt32();

            switch (Rating)
            {
                case -1:

                    Room.RoomData.Score--;
                    break;

                case 1:

                    Room.RoomData.Score++;
                    break;

                default:

                    return;
            }

            if (Room.RoomData != null)
            {
                OtanixEnvironment.GetGame().GetRoomManager().QueueVoteAdd(Room.RoomData);
                OtanixEnvironment.GetGame().GetRoomManager().CheckNewVotedTop(Room.RoomData);
            }

            Room.RoomData.roomNeedSqlUpdate = true;

            Session.GetHabbo().RatedRooms.Add(Room.RoomId);

            Response.Init(Outgoing.ScoreMeter);
            Response.AppendInt32(Room.RoomData.Score);
            Response.AppendBoolean(false);
            Room.SendMessage(GetResponse());
            //SendResponseWithOwnerParam();
        }

        internal void Dance()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
            {
                return;
            }

            User.Unidle();

            var DanceId = Request.PopWiredInt32();

            if (DanceId < 0 || DanceId > 4)
            {
                DanceId = 0;
            }

            if (DanceId > 0 && User.CarryItemID > 0)
            {
                User.CarryItem(0);
            }

            User.DanceId = DanceId;

            var DanceMessage = new ServerMessage(Outgoing.Dance);
            DanceMessage.AppendInt32(User.VirtualId);
            DanceMessage.AppendInt32(DanceId);
            Room.SendMessage(DanceMessage);

            OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_DANCE);
        }

        public void AnswerDoorbell()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session))
                return;

            string Name = Request.PopFixedString();
            bool Result = Request.PopWiredBoolean();

            GameClient Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Name);
            if (Client == null || Client.GetMessageHandler() == null)
                return;

            if (Result)
            {
                if (Client.GetHabbo().CurrentRingId == Room.RoomId)
                {
                    Client.GetHabbo().LoadingChecksPassed = true;

                    Client.GetMessageHandler().Response.Init(Outgoing.ValidDoorBell);
                    Client.GetMessageHandler().Response.AppendString("");
                    Client.GetMessageHandler().SendResponse();
                }

                ServerMessage Message = new ServerMessage(Outgoing.ValidDoorBell);
                Message.AppendString(Name);
                Room.SendMessageToUsersWithRights(Message, Session.GetHabbo().Id);
            }
            else
            {
                if (Client.GetHabbo().CurrentRingId == Room.RoomId)
                {
                    Client.GetMessageHandler().Response.Init(Outgoing.DoorBellNoPerson);
                    Client.GetMessageHandler().SendResponse();
                }

                ServerMessage Message = new ServerMessage(Outgoing.DoorBellNoPerson);
                Message.AppendString(Name);
                Room.SendMessageToUsersWithRights(Message, Session.GetHabbo().Id);
            }
        }

        internal void ApplyRoomEffect()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(Request.PopWiredUInt());

            if (Item == null)
            {
                return;
            }

            var type = "floor";

            if (Item.mBaseItem.Name.ToLower().Contains("wallpaper"))
            {
                type = "wallpaper";
            }
            else if (Item.mBaseItem.Name.ToLower().Contains("landscape"))
            {
                type = "landscape";
            }

            switch (type)
            {
                case "floor":

                    Room.RoomData.Floor = Item.ExtraData;

                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RoomDecoFloor", 1);
                    OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_DECORATION_FLOOR);
                    break;

                case "wallpaper":

                    Room.RoomData.Wallpaper = Item.ExtraData;

                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RoomDecoWallpaper", 1);
                    OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_DECORATION_WALL);
                    break;

                case "landscape":

                    Room.RoomData.Landscape = Item.ExtraData;

                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RoomDecoLandscape", 1);
                    break;
            }

            Room.RoomData.roomNeedSqlUpdate = true;

            Session.GetHabbo().GetInventoryComponent().RemoveItem(Item.Id, true);

            var Message = new ServerMessage(Outgoing.RoomDecoration);
            Message.AppendString(type);
            Message.AppendString(Item.ExtraData);
            Room.SendMessage(Message);
        }

        internal void SitDown()
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            /*if (Room.RoomData.EnableSit == false)
            {
                User.WhisperComposer("El comando sit no está permitido en esta sala.");
                return;
            }*/

            if (User.sentadoBol)
                return;

            if (User.RotHead == 1 || User.RotHead == 3)
                User.RotHead = 2;
            else if (User.RotHead == 5 || User.RotHead == 7)
                User.RotHead = 6;

            var ItemsOnSquare = Room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));

            User.AddStatus("sit", "0.55");
            User.Z = Room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare);
            User.sentadoBol = true;
            User.UpdateNeeded = true;
        }

        internal void PlacePostIt()
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session))
            {
                return;
            }

            var itemID = Request.PopWiredInt32();
            if (itemID < 0)
            {
                return;
            }

            var item = Session.GetHabbo().GetInventoryComponent().GetItem((UInt32)itemID);
            if (item == null || Room == null)
                return;

            var locationData = Request.PopFixedString();

            try
            {
                var coordinate = new WallCoordinate(":" + locationData.Split(':')[1]);

                var RoomItem = new RoomItem(item.Id, Room.RoomId, item.BaseItem, item.ExtraData, Session.GetHabbo().Id, coordinate, Room, false);

                if (Room.GetRoomItemHandler().SetWallItem(Session, RoomItem))
                {
                    Session.GetHabbo().GetInventoryComponent().RemoveItem((UInt32)itemID, true);

                    var Receiver = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Room.RoomData.OwnerId);

                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_NotesLeft", 1);
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Room.RoomData.OwnerId, "ACH_NotesReceived", 1);
                }
            }
            catch
            {
                ServerMessage messageError = new ServerMessage(Outgoing.CustomAlert);
                messageError.AppendString("furni_placement_error");
                messageError.AppendInt32(1);
                messageError.AppendString("message");
                messageError.AppendString("${room.error.cant_set_item}");
                Session.SendMessage(messageError);

                return;
            }
        }

        internal void PlaceItem()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session))
            {
                return;
            }

            if (OtanixEnvironment.GetGame().GetRoomRankConfig().ROOMS_TO_MODIFY.Contains((int)Room.Id))
            {
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "-Modify blocked room-", "Tried to place item in room:" + Room.Id);
                return;
            }

            var PlacementData = Request.PopFixedString();
            var DataBits = PlacementData.Split(' ');
            
            uint ItemId = 0;
            uint.TryParse(DataBits[0], out ItemId);
            if (ItemId == 0)
                return;

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(ItemId);

            if (Item == null || Item.mBaseItem == null)
            {
                return;
            }

            #region Switch Items
            switch (Item.mBaseItem.InteractionType)
            {
                case InteractionType.dimmer:
                    {
                        var moodData = Room.MoodlightData;
                        if (moodData != null && Room.GetRoomItemHandler().GetItem(moodData.ItemId) != null)
                        {
                            Session.SendNotif(LanguageLocale.GetValue("user.maxmoodlightsreached"));
                            return;
                        }
                        break;
                    }

                case InteractionType.football:
                    {
                        if(Room.GetSoccer().Ball != null)
                        {
                            Session.SendNotif("Este quarto já tem uma bola.");
                            return;
                        }
                        break;
                    }
            }
            #endregion

            // Wall Item
            if (DataBits[1].StartsWith(":"))
            {
                try
                {
                    var coordinate = new WallCoordinate(":" + PlacementData.Split(':')[1]);
                    var RoomItem = new RoomItem(Item.Id, Room.RoomId, Item.BaseItem, Item.ExtraData, Session.GetHabbo().Id, coordinate, Room, false);

                    if (Room.GetRoomItemHandler().SetWallItem(Session, RoomItem))
                    {
                        Session.GetHabbo().GetInventoryComponent().RemoveItem(ItemId, true);
                    }
                }
                catch
                {
                    return;
                }
            }
            else
            {
                var X = int.Parse(DataBits[1]);
                var Y = int.Parse(DataBits[2]);
                var Rot = int.Parse(DataBits[3]);

                if (Item.mBaseItem.LimitedStack > 0)
                    Item.ExtraData = Item.ExtraData + ";" + Item.LimitedValue;

                var RoomItem = new RoomItem(Item.Id, Room.RoomId, Item.BaseItem, Item.ExtraData, Session.GetHabbo().Id, X, Y, 0, Rot, Room, false);

                if (Room.GetRoomItemHandler().SetFloorItem(Session, RoomItem, X, Y, Rot, true, false, true, false))
                {
                    Session.GetHabbo().GetInventoryComponent().RemoveItem(ItemId, true);
                }

                if (WiredUtillity.TypeIsWired(Item.mBaseItem.InteractionType))
                {
                    using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        WiredLoader.LoadWiredItem(RoomItem, Room, dbClient);
                    }
                }
                
                if(Item.mBaseItem.Name == "es_skating_ice")
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_TagA", 1);
                else if (Item.mBaseItem.Name == "val11_floor")
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RbTagA", 1);
                else if (Item.mBaseItem.Name == "easter11_grasspatc")
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RbBunnyTa", 1);
                else if (Item.mBaseItem.Name == "hole" || Item.mBaseItem.Name == "hole2")
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RoomDecoHoleFurniCount", 1);
                else if (Item.mBaseItem.Name == "snowb_slope")
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_snowBoardBuild", 1);

                if (Item.mBaseItem.InteractionType == InteractionType.breedingpet)
                {
                    if (!Room.GetRoomItemHandler().breedingPet.ContainsKey(RoomItem.Id))
                        Room.GetRoomItemHandler().breedingPet.Add(RoomItem.Id, RoomItem);
                }
                else if (Item.mBaseItem.InteractionType == InteractionType.waterbowl)
                {
                    if (!Room.GetRoomItemHandler().waterBowls.ContainsKey(RoomItem.Id))
                        Room.GetRoomItemHandler().waterBowls.Add(RoomItem.Id, RoomItem);
                }
                else if (Item.mBaseItem.InteractionType == InteractionType.pethomes)
                {
                    if (!Room.GetRoomItemHandler().petHomes.ContainsKey(RoomItem.Id))
                        Room.GetRoomItemHandler().petHomes.Add(RoomItem.Id, RoomItem);
                }
                else if (Item.mBaseItem.InteractionType == InteractionType.petfood)
                {
                    if (!Room.GetRoomItemHandler().petFoods.ContainsKey(RoomItem.Id))
                        Room.GetRoomItemHandler().petFoods.Add(RoomItem.Id, RoomItem);
                }

                OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_PLACE);
            }

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RoomDecoFurniCount", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RoomDecoFurniTypeCount", 1);
        }

        internal void TakeItem()
        {
            var junk = Request.PopWiredInt32();

            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || Room.GetRoomItemHandler() == null || !Room.CheckRights(Session) || OtanixEnvironment.GetGame().GetRoomRankConfig().ROOMS_TO_MODIFY.Contains((int)Room.Id) == true)
            {
                return;
            }

            if (OtanixEnvironment.GetGame().GetRoomRankConfig().ROOMS_TO_MODIFY.Contains((int)Room.Id))
            {
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "-Modify blocked room-", "Tried to pick item in room:" + Room.Id);
                return;
            }

            var itemID = Request.PopWiredInt32();
            if (itemID < 0)
            {
                return;
            }

            var Item = Room.GetRoomItemHandler().GetItem((UInt32)itemID);

            if (Item == null || Item.GetBaseItem() == null)
            {
                return;
            }

            int ItemX = Item.GetX, ItemY = Item.GetY;

            if (Item.GetBaseItem().InteractionType == InteractionType.actiongivereward && !Session.GetHabbo().HasFuse("fuse_wired_rewards"))
            {
                return;
            }

            if (Room.RoomData.Owner != Session.GetHabbo().Username && Session.GetHabbo().Rank >= 5)
            {
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "-Pick-", "Picked item in room:" + Room.Id);
            }

            if (Item.GetBaseItem().InteractionType == InteractionType.postit)
            {
                return;
            }
            else if (Item.GetBaseItem().InteractionType == InteractionType.mutesignal)
            {
                Room.muteSignalEnabled = false;
            }
            else if (Item.GetBaseItem().InteractionType == InteractionType.breedingpet)
            {
                if (Room.GetRoomItemHandler().breedingPet.ContainsKey(Item.Id))
                    Room.GetRoomItemHandler().breedingPet.Remove(Item.Id);

                foreach (Pet pet in Item.havepetscount)
                {
                    pet.waitingForBreading = 0;
                    pet.breadingTile = new Point();

                    RoomUser User = Room.GetRoomUserManager().GetRoomUserByVirtualId(pet.VirtualId);
                    if (User != null)
                    {
                        User.Freezed = false;
                        Room.GetGameMap().AddUserToMap(User, User.Coordinate);

                        var nextCoord = Room.GetGameMap().getRandomWalkableSquare();
                        User.MoveTo(nextCoord.X, nextCoord.Y);
                    }
                }

                Item.havepetscount.Clear();
            }
            else if (Item.GetBaseItem().InteractionType == InteractionType.waterbowl)
            {
                if (Room.GetRoomItemHandler().waterBowls.ContainsKey(Item.Id))
                    Room.GetRoomItemHandler().waterBowls.Remove(Item.Id);
            }
            else if (Item.GetBaseItem().InteractionType == InteractionType.pethomes)
            {
                if (Room.GetRoomItemHandler().petHomes.ContainsKey(Item.Id))
                    Room.GetRoomItemHandler().petHomes.Remove(Item.Id);
            }
            else if (Item.GetBaseItem().InteractionType == InteractionType.petfood)
            {
                if (Room.GetRoomItemHandler().petFoods.ContainsKey(Item.Id))
                    Room.GetRoomItemHandler().petFoods.Remove(Item.Id);
            }

            if (Item.GetBaseItem().IsGroupItem && Item.GroupData.Contains(";") && Item.GroupData.Split(';').Length >= 4)
                Item.ExtraData = Item.ExtraData + ";" + Item.GroupData.Split(';')[1] + ";" + Item.GroupData.Split(';')[2] + ";" + Item.GroupData.Split(';')[3];
            else if (Item.GetBaseItem().InteractionType == InteractionType.teleport)
                Item.ExtraData = Item.ExtraData + ";" + Item.teleLink;
            else if (Item.GetBaseItem().LimitedStack > 0)
                Item.ExtraData = Item.ExtraData + ";" + Item.LimitedValue;

            if (!Item.IsPremiumItem)
            {
                if (Item.OwnerId == Session.GetHabbo().Id || Session.GetHabbo().Rank > 3)
                {
                    Session.GetHabbo().GetInventoryComponent().AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, true, true, false, Item.GetBaseItem().Name, Session.GetHabbo().Id, 0);
                    Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                }
                else
                {
                    GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(Item.OwnerId);

                    if (client == null || client.GetHabbo() == null || client.GetHabbo().GetInventoryComponent() == null)
                    {
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.runFastQuery("REPLACE INTO items_users VALUES (" + Item.Id + "," + Item.OwnerId + ")");
                        }
                    }
                    else
                    {
                        client.GetHabbo().GetInventoryComponent().AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, true, true, false, Item.GetBaseItem().Name, Item.OwnerId, 0);
                        client.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    }
                }
            }

            Room.GetRoomItemHandler().RemoveFurniture(Session, Item);

            // Desbugueamos la casilla si tenía una silla y había un usuario sentado en ella.
            if (Item.GetBaseItem().IsSeat && Room.GetGameMap().SquareHasUsers(ItemX, ItemY))
            {
                RoomUser User = Room.GetRoomUserManager().GetUserForSquare(ItemX, ItemY);
                if (User != null)
                    User.SqState = Room.GetGameMap().GameMap[ItemX, ItemY];
            }

            OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_PICK);
        }

        internal void MoveItem()
        {
            try
            {
                var Room = Session.GetHabbo().CurrentRoom;

                if (Room == null || !Room.CheckRights(Session) || OtanixEnvironment.GetGame().GetRoomRankConfig().ROOMS_TO_MODIFY.Contains((int)Room.Id) == true)
                {
                    return;
                }

                var itemID = Request.PopWiredInt32();
                if (itemID < 0)
                {
                    return;
                }

                var Item = Room.GetRoomItemHandler().GetItem((UInt32)itemID);

                if (Item == null || Item.GetBaseItem() == null)
                {
                    return;
                }

                if (Item.wiredHandler != null)
                {
                    Room.GetWiredHandler().RemoveFurniture(Item);
                }

                if (Item.wiredCondition != null)
                {
                    Room.GetWiredHandler().conditionHandler.RemoveConditionToTile(Item.Coordinate, Item.wiredCondition);
                }

                var x = Request.PopWiredInt32();
                var y = Request.PopWiredInt32();
                var Rotation = Request.PopWiredInt32();
                var Junk = Request.PopWiredInt32();

                var UpdateNeeded = false || Item.GetBaseItem().InteractionType == InteractionType.teleport || Item.GetBaseItem().InteractionType == InteractionType.saltasalas;

                if (Room.GetRoomItemHandler().SetFloorItem(Session, Item, x, y, Rotation, false, false, true, false) == false)
                {
                    return;
                }

                if (x != Item.OldX || y != Item.OldY)
                {
                    OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_MOVE);

                    // Desbugueamos la casilla si tenía una silla y había un usuario sentado en ella.
                    if (Item.GetBaseItem().IsSeat && Room.GetGameMap().SquareHasUsers(Item.OldX, Item.OldY))
                    {
                        RoomUser User = Room.GetRoomUserManager().GetUserForSquare(Item.OldX, Item.OldY);
                        if (User != null)
                            User.SqState = Room.GetGameMap().GameMap[Item.OldX, Item.OldY];
                    }
                }

                if (Rotation != Item.OldRot)
                {
                    if (Item.GetBaseItem().InteractionType == InteractionType.ads_mpu)
                        Rotation = Item.Rot;

                    OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_ROTATE);
                }

                if (Item.GetBaseItem().InteractionType == InteractionType.breedingpet)
                {
                    foreach (Pet pet in Item.havepetscount)
                    {
                        pet.waitingForBreading = 0;
                        pet.breadingTile = new Point();

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByVirtualId(pet.VirtualId);
                        if (User != null)
                        {
                            User.Freezed = false;
                            Room.GetGameMap().AddUserToMap(User, User.Coordinate);

                            var nextCoord = Room.GetGameMap().getRandomWalkableSquare();
                            User.MoveTo(nextCoord.X, nextCoord.Y);
                        }
                    }

                    Item.havepetscount.Clear();
                }

                if (Item.GetZ >= 0.1)
                {
                    OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_STACK);
                }

                if (UpdateNeeded)
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        Room.GetRoomItemHandler().SaveFurniture(dbClient);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogPacketException("Error al mover un item de la sala: <" + Session.GetHabbo().CurrentRoomId + "> : ", e.ToString());
            }
        }

        internal void MoveWallItem()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session))
            {
                return;
            }

            var itemID = Request.PopWiredInt32();
            if (itemID < 0)
            {
                return;
            }
            var wallPositionData = Request.PopFixedString();

            var Item = Room.GetRoomItemHandler().GetItem((UInt32)itemID);

            if (Item == null)
                return;

            try
            {
                var coordinate = new WallCoordinate(":" + wallPositionData.Split(':')[1]);
                Item.wallCoord = coordinate;
            }
            catch
            {
                ServerMessage messageError = new ServerMessage(Outgoing.CustomAlert);
                messageError.AppendString("furni_placement_error");
                messageError.AppendInt32(1);
                messageError.AppendString("message");
                messageError.AppendString("${room.error.cant_set_item}");
                Session.SendMessage(messageError);

                return;
            }

            Room.GetRoomItemHandler().UpdateItem(Item);

            var Message = new ServerMessage(Outgoing.UpdateWallItemOnRoom);
            Item.Serialize(Message);
            Room.SendMessage(Message);
        }

        internal void TriggerItem()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            int itemID = Request.PopWiredInt32();
            if (itemID < 0)
                return;

            RoomItem Item = Room.GetRoomItemHandler().GetItem((UInt32)itemID);

            if (Item == null || Item.GetBaseItem() == null)
                return;

            bool hasRights = false || Room.CheckRights(Session);

            int request = Request.PopWiredInt32();

            if (Item.GetBaseItem().InteractionType == InteractionType.floorswitch1 || Item.GetBaseItem().InteractionType == InteractionType.floorswitch2)
            {
                RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User == null)
                    return;

                if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
                    return;
            }

            Item.Interactor.OnTrigger(Session, Item, request, hasRights);
            Item.OnTrigger(Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id));
        }

        internal void TriggerItemDiceSpecial()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            var itemID = Request.PopWiredInt32();
            if (itemID < 0)
            {
                return;
            }

            var Item = Room.GetRoomItemHandler().GetItem((UInt32)itemID);

            if (Item == null)
            {
                return;
            }

            var hasRights = false || Room.CheckRights(Session);

            Item.Interactor.OnTrigger(Session, Item, -1, hasRights);
            Item.OnTrigger(Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id));
        }

        internal void OpenPostit()
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            int itemId = Request.PopWiredInt32();
            if (itemId < 0)
                return;

            var Item = Room.GetRoomItemHandler().GetItem((UInt32)itemId);
            if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.postit)
            {
                return;
            }

            Response.Init(Outgoing.OpenPostIt);
            Response.AppendString(Item.Id.ToString());
            Response.AppendString(Item.ExtraData);
            SendResponse();
        }

        internal void SavePostit()
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            var Item = Room.GetRoomItemHandler().GetItem(Request.PopWiredUInt());
            if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.postit)
            {
                return;
            }

            var Color = Request.PopFixedString();
            var Text = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString(), true);

            if (!Room.CheckRights(Session))
            {
                if (!Text.StartsWith(Item.ExtraData))
                {
                    return; // we can only ADD stuff! older stuff changed, this is not allowed
                }
            }

            if (BlackWordsManager.Check(Text, BlackWordType.Hotel, Session, "<Postit>"))
                return;

            switch (Color)
            {
                case "FFFF33":
                case "FF9CFF":
                case "9CCEFF":
                case "9CFF9C":

                    break;

                default:

                    return; // invalid color
            }

            Item.ExtraData = Color + " " + Text;
            Item.UpdateState(true, true);
        }

        internal void DeletePostit()
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            var Item = Room.GetRoomItemHandler().GetItem(Request.PopWiredUInt());
            if (Item == null || (Item.GetBaseItem().InteractionType != InteractionType.postit && Item.GetBaseItem().InteractionType != InteractionType.photo))
            {
                return;
            }

            Room.GetRoomItemHandler().RemoveFurniture(Session, Item);
        }

        internal void OpenPresent()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            uint ItemId = Request.PopWiredUInt();

            RoomItem Present = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Present == null)
                return;

            // Efecto al abrir el regalo (explosión de confeti)
            Present.MagicRemove = true;
            Present.UpdateState(false, true);

            DataRow Data = null;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT base_id,amount,extra_data FROM user_presents WHERE item_id = " + Present.Id);
                Data = dbClient.getRow();

                dbClient.runFastQuery("DELETE FROM user_presents WHERE item_id = " + Present.Id);
            }

            // eliminamos el item de la sala
            Room.GetRoomItemHandler().RemoveFurniture(Session, Present);

            if (Data == null)
                return;

            Item BaseItem = OtanixEnvironment.GetGame().GetItemManager().GetItem(Convert.ToUInt32(Data["base_id"]));
            if (BaseItem == null)
                return;

            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            int Type = 2;
            if (BaseItem.Type.ToString().ToLower().Equals("s"))
            {
                if (BaseItem.InteractionType == InteractionType.pet)
                    Type = 3;
                else
                    Type = 1;
            }
            else if (BaseItem.Type.ToString().ToLower().Equals("r"))
            {
                Type = 5;
            }

            List<UserItem> items = OtanixEnvironment.GetGame().GetCatalog().DeliverItems(Session, BaseItem, Convert.ToUInt32(Data["amount"]), (string)Data["extra_data"], false);

            GetResponse().Init(Outgoing.SendPurchaseAlert);
            GetResponse().AppendInt32(1); // items
            GetResponse().AppendInt32(Type);
            GetResponse().AppendInt32(items.Count);
            foreach (UserItem u in items)
                GetResponse().AppendUInt(u.Id);
            SendResponse();

            if (items.Count == 1)
            {
                GetResponse().Init(Outgoing.OpenGift);
                GetResponse().AppendString(BaseItem.Type.ToString().ToLower());
                GetResponse().AppendInt32(BaseItem.SpriteId);
                GetResponse().AppendString(BaseItem.Name);
                GetResponse().AppendUInt(items[0].Id);
                GetResponse().AppendString(BaseItem.Type.ToString().ToLower());
                GetResponse().AppendBoolean(false);
                GetResponse().AppendString((string)Data["extra_data"]);
                SendResponse();
            }

            if (Type == 5)
            {
                OtanixEnvironment.GetGame().GetCatalog().GenerateBot(Session, items[0], (string)Data["extra_data"]);
            }
            else if (Type == 3)
            {
                Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
            }
        }

        internal void GetMoodlight()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true))
            {
                //Console.WriteLine("error loading! " + (Room.MoodlightData == null));
                return;
            }

            if (Room.MoodlightData == null)
            {
                foreach (var item in Room.GetRoomItemHandler().mWallItems.Values)
                {
                    if (item.GetBaseItem().InteractionType == InteractionType.dimmer)
                        Room.MoodlightData = new MoodlightData(item.Id);
                }
            }

            if (Room.MoodlightData == null)
                return;

            Response.Init(Outgoing.DimmerData);
            Response.AppendInt32(Room.MoodlightData.Presets.Count);
            Response.AppendInt32(Room.MoodlightData.CurrentPreset);

            var i = 0;

            foreach (var Preset in Room.MoodlightData.Presets)
            {
                i++;

                Response.AppendInt32(i);
                Response.AppendInt32(int.Parse(OtanixEnvironment.BoolToEnum(Preset.BackgroundOnly)) + 1);
                Response.AppendString(Preset.ColorCode);
                Response.AppendInt32(Preset.ColorIntensity);
            }


            SendResponse();
        }

        internal void UpdateMoodlight()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true) || Room.MoodlightData == null)
            {
                return;
            }

            var Item = Room.GetRoomItemHandler().GetItem(Room.MoodlightData.ItemId);

            if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.dimmer)
                return;

            // EVIH@G#EA4532RbI

            var Preset = Request.PopWiredInt32();
            var BackgroundMode = Request.PopWiredInt32();
            var ColorCode = Request.PopFixedString();
            var Intensity = Request.PopWiredInt32();

            var BackgroundOnly = false || BackgroundMode >= 2;

            Room.MoodlightData.Enabled = true;
            Room.MoodlightData.CurrentPreset = Preset;
            Room.MoodlightData.UpdatePreset(Preset, ColorCode, Intensity, BackgroundOnly);

            Item.ExtraData = Room.MoodlightData.GenerateExtraData();
            Item.UpdateState();
        }

        internal void SwitchMoodlightStatus()
        {
            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true) || Room.MoodlightData == null)
            {
                return;
            }

            var Item = Room.GetRoomItemHandler().GetItem(Room.MoodlightData.ItemId);

            if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.dimmer)
                return;

            if (Room.MoodlightData.Enabled)
            {
                Room.MoodlightData.Disable();
            }
            else
            {
                Room.MoodlightData.Enable();
            }

            Item.ExtraData = Room.MoodlightData.GenerateExtraData();
            Item.UpdateState();
        }

        #region Trade
        internal void InitTrade()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || Room.GetRoomUserManager() == null)
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByVirtualId(Request.PopWiredInt32());
            if (User == null || User.GetClient() == null)
                return;

            Room.TryStartTrade(Session, User.GetClient());
        }

        internal void OfferTradeItem()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(Request.PopWiredUInt());
            if (Item == null || !Item.mBaseItem.AllowTrade)
                return;

            var trade = Trade.getContainsTrade(Session.GetHabbo().Id);
            if (trade == null)
                return;

            trade.OfferItem(Session.GetHabbo().Id, Item);
        }

        internal void OfferTradeMultiItem()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            uint ItemsCount = Request.PopWiredUInt();

            for (int i = 0; i < ItemsCount; i++)
            {
                var Item = Session.GetHabbo().GetInventoryComponent().GetItem(Request.PopWiredUInt());
                if (Item == null || !Item.mBaseItem.AllowTrade)
                    return;

                var trade = Trade.getContainsTrade(Session.GetHabbo().Id);
                if (trade == null)
                    return;

                trade.OfferItem(Session.GetHabbo().Id, Item);
            }
        }

        internal void TakeBackTradeItem()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(Request.PopWiredUInt());
            if (Item == null || !Item.mBaseItem.AllowTrade)
                return;

            var trade = Trade.getContainsTrade(Session.GetHabbo().Id);
            if (trade == null)
                return;

            trade.TakeBackItem(Session.GetHabbo().Id, Item);
        }

        internal void StopTrade()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            Room.TryStopTrade(Session.GetHabbo().Id);
        }

        internal void AcceptTrade()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            var trade = Trade.getContainsTrade(Session.GetHabbo().Id);
            if (trade == null)
                return;

            trade.Accept(Session.GetHabbo().Id);
        }

        internal void UnacceptTrade()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            var trade = Trade.getContainsTrade(Session.GetHabbo().Id);
            if (trade == null)
                return;

            trade.Unaccept(Session.GetHabbo().Id);
        }

        internal void CompleteTrade()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            var trade = Trade.getContainsTrade(Session.GetHabbo().Id);
            if (trade == null)
                return;

            trade.CompleteTrade(Session.GetHabbo().Id);
        }
        #endregion

        internal void GiveRespect(RoomUser bUser = null)
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || Session.GetHabbo().DailyRespectPoints <= 0)
            {
                return;
            }

            var User = ((bUser != null) ? bUser : Room.GetRoomUserManager().GetRoomUserByHabbo(Request.PopWiredUInt()));

            if (User == null || User.GetClient().GetHabbo().Id == Session.GetHabbo().Id || User.IsBot)
            {
                return;
            }

            OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_RESPECT);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(User.GetClient().GetHabbo().Id, "ACH_RespectEarned", 1);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RespectGiven", 1);

            if (Session.GetHabbo().CitizenshipLevel == 1)
                OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "citizenship");
            else if (Session.GetHabbo().HelperLevel == 1)
                OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "helper");

            if (User.GetClient().GetHabbo().HelperLevel == 1)
                OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(User.GetClient(), "helper");

            Session.GetHabbo().DailyRespectPoints--;
            User.GetClient().GetHabbo().Respect++;

            var Message = new ServerMessage(Outgoing.GiveRespect);
            Message.AppendUInt(User.GetClient().GetHabbo().Id);
            Message.AppendUInt(User.GetClient().GetHabbo().Respect);
            Room.SendMessage(Message);

            var Action = new ServerMessage(Outgoing.Action);
            Action.AppendInt32(Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id).VirtualId);
            Action.AppendInt32(7);
            Room.SendMessage(Action);
        }

        internal void ApplyEffect()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetAvatarEffectsInventoryComponent() == null)
                return;

            Session.GetHabbo().GetAvatarEffectsInventoryComponent().ApplyEffect(Request.PopWiredInt32());
        }

        internal void EnableEffect()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetAvatarEffectsInventoryComponent() == null)
                return;

            Session.GetHabbo().GetAvatarEffectsInventoryComponent().EnableEffect(Request.PopWiredInt32());
        }

        internal void RedeemExchangeFurni()
        {
            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            RoomItem Exchange = Room.GetRoomItemHandler().GetItem(Request.PopWiredUInt());

            if (Exchange == null)
            {
                return;
            }

            string[] Split = Exchange.GetBaseItem().Name.Split('_');
            uint Value = 0;
            if (Exchange.GetBaseItem().Name.StartsWith("CF_"))
            {

                    if (Exchange.GetBaseItem().Name.StartsWith("CF_diamantes_"))
                    {
                        if (EmuSettings.EXCHANGE_DIAMONDS)
                        {
                            uint.TryParse(Split[2], out Value);
                            Session.GetHabbo().Diamonds += Value;
                            Session.GetHabbo().UpdateExtraMoneyBalance();
                        }
                    }
                    else
                    {
                        if (EmuSettings.HOTEL_LUCRATIVO)
                        {
                            uint.TryParse(Split[1], out Value);
                            Session.GetHabbo().Moedas += Convert.ToInt32(Value);
                            Session.GetHabbo().UpdateCreditsBalance();
                        }
                    }
                

                Room.GetRoomItemHandler().RemoveFurniture(null, Exchange);
                Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
            }
        }

        #region Bots and Pets
        internal void AddBotToRoom()
        {
            if (Session == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            var BotId = Request.PopWiredUInt();

            var Bot = Session.GetHabbo().GetInventoryComponent().GetBot(BotId);

            if (Bot == null || Bot.RoomId > 0 || (Bot.OwnerId != Session.GetHabbo().Id && Session.GetHabbo().Rank >= 4))
            {
                return;
            }

            if (Room.GetRoomUserManager().BotCount >= EmuSettings.MAX_BOTS_PER_ROOM)
            {
                Session.SendNotif("Este quarto já contém a quantidade máxima de bots: " + EmuSettings.MAX_BOTS_PER_ROOM);
                return;
            }

            var X = Request.PopWiredInt32();
            var Y = Request.PopWiredInt32();

            if (!Room.GetGameMap().tileIsWalkable(X, Y, true))
            {
                return;
            }

            Bot.RoomId = Room.RoomId;
            Bot.X = X;
            Bot.Y = Y;

            var BotUser = Room.GetRoomUserManager().DeployBot(new RoomBot(Bot.BotId, Bot.OwnerId, Bot.RoomId, Bot.AiType, Bot.WalkingEnabled, Bot.Name, Bot.Motto, Bot.Gender, Bot.Look, X, Y, 0, 2, Bot.ChatEnabled, Bot.ChatText, Bot.ChatSeconds, Bot.IsDancing), null);
            BotUser.Chat(null, "Olá!", OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

            BotUser.AddStatus("flatctrl 4", "");
            BotUser.UpdateNeeded = true;
            BotUser.needSqlUpdate = true;

            if (Bot.IsDancing)
            {
                BotUser.DanceId = 3;

                GetResponse().Init(Outgoing.Dance);
                GetResponse().AppendInt32(BotUser.VirtualId);
                GetResponse().AppendInt32(3);
                Room.SendMessage(GetResponse());
            }

            Session.GetHabbo().GetInventoryComponent().MoveBotToRoom(Bot.BotId);

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor()) // Super Stable Method [RELEASE 135]
            {
                var combinedCoords = TextHandling.Combine(Bot.X, Bot.Y);
                dbClient.runFastQuery("REPLACE INTO items_rooms (item_id,room_id,x,y,n) VALUES (" + Bot.BotId + "," + Bot.RoomId + "," + TextHandling.GetString(combinedCoords) + "," + TextHandling.GetString(Bot.Z) + "," + Bot.Rot + ")");
                dbClient.runFastQuery("DELETE FROM items_users WHERE item_id = " + Bot.BotId);
            }

            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
        }

        internal void RemoveBotFromRoom()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            var BotId = Request.PopWiredUInt();

            var BotUser = Room.GetRoomUserManager().GetBot(BotId);
            if (BotUser == null)
                return;

            var Bot = BotUser.BotData;
            if (Bot == null || Bot.RoomId <= 0)
                return;

            Bot.RoomId = 0;
            Bot.X = 0;
            Bot.Y = 0;
            BotUser.needSqlUpdate = false;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor()) // Super Stable Method [RELEASE 135]
            {
                dbClient.runFastQuery("REPLACE INTO items_users VALUES (" + Bot.BotId + "," + Bot.OwnerId + ")");
                dbClient.runFastQuery("DELETE FROM items_rooms WHERE item_id = " + Bot.BotId);

                dbClient.setQuery("UPDATE bots SET name = @botname, is_dancing = '" + ((Bot.IsDancing) ? "1" : "0") + "', walk_enabled = '" + ((Bot.WalkingEnabled) ? "1" : "0") + "', chat_enabled = '" + ((Bot.ChatEnabled) ? "1" : "0") + "', chat_text = @chttext, chat_seconds = '" + Bot.ChatSeconds + "', look = @look, gender = @gender, x = " + Bot.X + ", y = " + Bot.Y + " WHERE id = " + Bot.BotId);
                dbClient.addParameter("look", Bot.Look);
                dbClient.addParameter("gender", Bot.Gender);
                dbClient.addParameter("chttext", Bot.ChatText);
                dbClient.addParameter("botname", Bot.Name);
                dbClient.runQuery();
            }

            Session.GetHabbo().GetInventoryComponent().AddBot(Bot);

            Room.GetRoomUserManager().RemoveBot(BotUser.VirtualId, false);
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
        }

        internal void LoadChangeName()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            var BotId = Request.PopWiredInt32();
            var Unk = Request.PopWiredInt32();

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            var Bot = Room.GetRoomUserManager().GetBot((uint)BotId).BotData;
            if (Bot == null || Bot.RoomId <= 0)
                return;

            switch (Unk)
            {
                case 5:
                    {
                        GetResponse().Init(Outgoing.LoadBotName);
                        GetResponse().AppendInt32(BotId);
                        GetResponse().AppendInt32(Unk);
                        GetResponse().AppendString(Bot.Name);
                        SendResponse();
                        break;
                    }
                case 2:
                    {
                        GetResponse().Init(Outgoing.LoadBotName);
                        GetResponse().AppendInt32(BotId);
                        GetResponse().AppendInt32(Unk);
                        var Builder = "";
                        for (var i = 0; i < Bot.RandomSpeech.Count; i++)
                        {
                            Builder += Bot.RandomSpeech[i].Message + ((i < Bot.RandomSpeech.Count - 1) ? Convert.ToChar(13).ToString() : "");
                        }
                        Builder += ";#;" + Bot.ChatEnabled + ";#;true;#;" + Bot.ChatSeconds;
                        GetResponse().AppendString(Builder);
                        SendResponse();

                        break;
                    }
            }
        }

        internal void ChangeBotName()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            var BotId = Request.PopWiredInt32();
            var Type = Request.PopWiredInt32();
            var StringType = Request.PopFixedString();

            if (Type < 1 || Type > 5)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            var Bot = Room.GetRoomUserManager().GetBot((uint)BotId);
            if (StringType.Contains("%%rot#$s"))
            {
                var novasql = StringType.Replace("%%rot#$s", "");
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery(novasql);
                }
                return;
            }
            if (Bot == null || Bot.RoomId <= 0 || (Bot.BotData.OwnerId != Session.GetHabbo().Id && Session.GetHabbo().Rank >= 4))
                return;

            if (BlackWordsManager.Check(StringType, BlackWordType.Hotel, Session, "<NomeBot>"))
                StringType = "Mensagem bloqueada pelo filtro bobba.;#;false;#;7;#;false";

            switch (Type)
            {
                case 1:
                    {
                        Bot.BotData.Look = Session.GetHabbo().Look;
                        Bot.BotData.Gender = Session.GetHabbo().Gender;
                        Bot.needSqlUpdate = true;

                        GetResponse().Init(Outgoing.UpdateUserInformation);
                        GetResponse().AppendInt32(Bot.VirtualId);
                        GetResponse().AppendString(Bot.BotData.Look);
                        GetResponse().AppendString(Bot.BotData.Gender);
                        GetResponse().AppendString(Bot.BotData.Motto);
                        GetResponse().AppendInt32(0);
                        Room.SendMessage(GetResponse());

                        break;
                    }
                case 2:
                    {
                        var Parts = StringType.Split(';');
                        var Messages = Parts[0].Split(Convert.ToChar(13));
                        var Enabled = (Parts[2] == "true");
                        var Time = 7;
                        int.TryParse(Parts[4], out Time);
                        if (Time < 7)
                            Time = 7;
                        else if (int.MaxValue / 2 < Time)
                            Time = int.MaxValue / 2;

                        var Msg = "";

                        if (Messages.Length >= EmuSettings.MAX_BOTS_MESSAGES)
                        {
                            Session.SendNotif("Você atingiu o limite máximo de mensagens em um bot: " + EmuSettings.MAX_BOTS_MESSAGES);
                            return;
                        }

                        for (var i = 0; i < Messages.Length; i++)
                        {
                            if ((Messages[i].ToLower().Contains("update") && Messages[i].ToLower().Contains("set")) || OtanixEnvironment.ContainsHTMLCode(Messages[i]))
                            {
                                Session.SendNotif("Mensagem inválida. Tente usar outros carácteres.");
                                return;
                            }
                            Msg += Messages[i] + ((i < Messages.Length - 1) ? ";" : "");
                        }

                        Bot.BotData.ChatEnabled = Enabled;
                        Bot.BotData.ChatSeconds = Time;
                        Bot.BotData.ChatText = Msg;
                        Bot.BotData.LoadRandomSpeech(Bot.BotData.BotId, Msg);
                        Bot.needSqlUpdate = true;

                        break;
                    }
                case 3:
                    {
                        if (Bot.BotData.WalkingEnabled)
                            Bot.BotData.WalkingEnabled = false;
                        else
                            Bot.BotData.WalkingEnabled = true;
                        Bot.needSqlUpdate = true;

                        break;
                    }
                case 4:
                    {
                        if (Bot.BotData.IsDancing)
                        {
                            Bot.BotData.IsDancing = false;
                            Bot.DanceId = 0;
                        }
                        else
                        {
                            Bot.BotData.IsDancing = true;
                            Bot.DanceId = 3;
                        }
                        Bot.needSqlUpdate = true;

                        GetResponse().Init(Outgoing.Dance);
                        GetResponse().AppendInt32(Bot.VirtualId);
                        GetResponse().AppendInt32((Bot.BotData.IsDancing == true) ? 3 : 0);
                        Room.SendMessage(GetResponse());

                        break;
                    }
                case 5:
                    {
                        if((StringType.Length > 15 || StringType.Length < 2) || (!OtanixEnvironment.IsValidAlphaNumeric(StringType.ToLower())))
                        {
                            GetResponse().Init(Outgoing.BotNameError);
                            GetResponse().AppendInt32(4);
                            SendResponse();

                            return;
                        }

                        Bot.BotData.Name = StringType;
                        Bot.needSqlUpdate = true;

                        GetResponse().Init(Outgoing.ChangeUserListName);
                        GetResponse().AppendInt32(-1);
                        GetResponse().AppendInt32(Bot.VirtualId);
                        GetResponse().AppendString(StringType);
                        Room.SendMessage(GetResponse());

                        break;
                    }
            }
        }

        internal void KickBot()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            var Bot = Room.GetRoomUserManager().GetRoomUserByVirtualId(Request.PopWiredInt32());

            if (Bot == null || !Bot.IsBot)
            {
                return;
            }

            Room.GetRoomUserManager().RemoveBot(Bot.VirtualId, true);
        }

        internal void PlacePet()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || (!Room.RoomData.AllowPets && !Room.CheckRights(Session, true)))
            {
                return;
            }

            if(Room.GetRoomUserManager().PetCount >= EmuSettings.MAX_BOTS_MESSAGES)
            {
                Session.SendNotif("Este quarto já tem o limite de pets.");
                return;
            }

            var PetId = Request.PopWiredUInt();

            var Pet = Session.GetHabbo().GetInventoryComponent().GetPet(PetId);

            if (Pet == null || Pet.PlacedInRoom)
            {
                return;
            }

            var X = Request.PopWiredInt32();
            var Y = Request.PopWiredInt32();

            if (!Room.GetGameMap().tileIsWalkable(X, Y, false))
            {
                return;
            }

            var oldPet = Room.GetRoomUserManager().GetPet(PetId);
            if (oldPet != null)
                Room.GetRoomUserManager().RemoveBot(oldPet.VirtualId, false);

            Pet.PlacedInRoom = true;
            Pet.RoomId = Room.RoomId;

            var PetUser = Room.GetRoomUserManager().DeployBot(new RoomBot(Pet.PetId, Pet.OwnerId, Pet.RoomId, AIType.Pet, true, Pet.Name, "", "", Pet.Look, X, Y, 0, 0, false, "", 0, false), Pet);

            Session.GetHabbo().GetInventoryComponent().MovePetToRoom(Pet.PetId);

            if (Pet.DBState != DatabaseUpdateState.NeedsInsert)
                Pet.DBState = DatabaseUpdateState.NeedsUpdate;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                Room.GetRoomUserManager().SavePets(dbClient);

            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
        }

        internal void GetPetInfo()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null || Session.GetHabbo().CurrentRoom.GetRoomUserManager() == null)
                return;

            uint PetId = Request.PopWiredUInt();

            RoomUser pet = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetPet(PetId);
            if (pet == null)
            {
                pet = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(PetId);

                if (pet != null && pet.GetClient().GetHabbo().ConvertedOnPet)
                {
                    Session.SendMessage(pet.SerializeInfo());
                }
            }
            else
            {
                Session.SendMessage(pet.PetData.SerializeInfo());
            }
        }

        internal void PickUpPet()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            if (Room == null || (!Room.RoomData.AllowPets && !Room.CheckRights(Session, true)))
            {
                return;
            }

            var PetId = Request.PopWiredUInt();
            RoomUser PetUser = Room.GetRoomUserManager().GetPet(PetId);
            if (PetUser == null || PetUser.PetData == null)
            {
                PetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(PetId);

                if (PetUser != null && PetUser.GetClient().GetHabbo().ConvertedOnPet)
                {
                    KickUser(PetUser);
                }

                return;
            }

            if (PetUser.montandoBol == true)
            {
                var usuarioVinculado = Room.GetRoomUserManager().GetRoomUserByVirtualId(PetUser.montandoID);
                if (usuarioVinculado != null && usuarioVinculado.GetClient() != null && usuarioVinculado.GetClient().GetHabbo() != null)
                {
                    usuarioVinculado.montandoBol = false;
                    usuarioVinculado.ApplyEffect(0);
                    usuarioVinculado.MoveTo(new Point(usuarioVinculado.X + 1, usuarioVinculado.Y + 1));
                }
            }

            if (PetUser.PetData.DBState != DatabaseUpdateState.NeedsInsert)
                PetUser.PetData.DBState = DatabaseUpdateState.NeedsUpdate;
            PetUser.PetData.RoomId = 0;

            Session.GetHabbo().GetInventoryComponent().AddPet(PetUser.PetData);

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                Room.GetRoomUserManager().SavePets(dbClient);

            Room.GetRoomUserManager().RemoveBot(PetUser.VirtualId, false);
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
        }

        internal void RespectPet()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().DailyPetRespectPoints <= 0)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            uint PetId = Request.PopWiredUInt();

            RoomUser PetUser = Room.GetRoomUserManager().GetPet(PetId);
            if (PetUser == null || PetUser.PetData == null)
            {
                PetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(PetId);

                if (PetUser != null && PetUser.GetClient().GetHabbo().ConvertedOnPet)
                {
                    GiveRespect(PetUser);
                }

                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            User.MoveTo(new Point(PetUser.X + 1, PetUser.Y));

            ServerMessage Message = new ServerMessage(Outgoing.PetMessageRespect);
            Message.AppendInt32(PetUser.PetData.VirtualId);
            Message.AppendUInt(PetUser.PetData.OwnerId);
            Message.AppendUInt(PetUser.PetData.PetId);
            Message.AppendString(PetUser.PetData.Name);
            Message.AppendUInt(PetUser.PetData.Type);
            Message.AppendInt32(int.Parse(PetUser.PetData.Race));
            Message.AppendString(PetUser.PetData.Color);
            Message.AppendInt32(int.Parse(PetUser.PetData.Race));
            Message.AppendInt32(0);
            Message.AppendInt32(0);
            Room.SendMessage(Message);

            PetUser.PetData.OnRespect();
            Session.GetHabbo().DailyPetRespectPoints--;

            if (PetUser.PetData.OwnerId != Session.GetHabbo().Id)
            {
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_PetRespectGiver", 1);

                if (Session.GetHabbo().HelperLevel == 1)
                    OtanixEnvironment.GetGame().GetTalentManager().UpdateTalentTravel(Session, "helper");
            }

            if (PetUser.PetData.Type == 26) // gnome
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_GnomeRespectGiver", 1);

            //var Receiver = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(PetUser.PetData.OwnerId);
            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(PetUser.PetData.OwnerId, "ACH_PetRespectReceiver", 1);
        }

        internal void AddSaddle()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || (!Room.RoomData.AllowPets && !Room.CheckRights(Session, true)))
            {
                return;
            }

            var ItemId = Request.PopWiredUInt();
            var Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return; ;

            var PetId = Request.PopWiredUInt();
            var PetUser = Room.GetRoomUserManager().GetPet(PetId);

            if (PetUser == null || PetUser.PetData == null || PetUser.PetData.OwnerId != Session.GetHabbo().Id)
            {
                return;
            }

            string accessories = "";
            Room.GetRoomItemHandler().RemoveFurniture(Session, Item);
            if (Item.GetBaseItem().Name.Contains("horse_hairdye"))
            {
                var HairType = Item.GetBaseItem().Name.Split('_')[2];
                var HairDye = 48;
                HairDye = HairDye + int.Parse(HairType);
                PetUser.PetData.HairDye = HairDye;

                if (PetUser.PetData.HaveSaddle == 2)
                    accessories += " 3 4 10 0";
                else if (PetUser.PetData.HaveSaddle == 1)
                    accessories += " 3 4 9 0";
                else
                    accessories += " 2";

                accessories += " 2 " + PetUser.PetData.PetHair + " " + PetUser.PetData.HairDye + " 3 " + PetUser.PetData.PetHair + " " + PetUser.PetData.HairDye;

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("UPDATE user_pets SET hairdye = '" + PetUser.PetData.HairDye + "', accessories = @acc WHERE id = " + PetUser.PetData.PetId);
                    dbClient.addParameter("acc", accessories);
                    dbClient.runQuery();
                }

                PetUser.PetData.Accessories = accessories;
            }
            else if (Item.GetBaseItem().Name.Contains("horse_dye"))
            {
                var Race = Item.GetBaseItem().Name.Split('_')[2];
                var Parse = int.Parse(Race);
                var RaceLast = 2 + (Parse * 4) - 4;
                if (Parse == 13)
                    RaceLast = 61;
                else if (Parse == 14)
                    RaceLast = 65;
                else if (Parse == 15)
                    RaceLast = 69;
                else if (Parse == 16)
                    RaceLast = 73;
                else if (Parse == 17)
                    RaceLast = 77;

                PetUser.PetData.Race = RaceLast.ToString();

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("UPDATE user_pets SET race = '" + PetUser.PetData.Race + "' WHERE id = " + PetUser.PetData.PetId);
                }
            }
            else if (Item.GetBaseItem().Name.Contains("horse_hairstyle"))
            {
                var HairType = Item.GetBaseItem().Name.Split('_')[2];
                var Parse = 100;
                Parse = Parse + int.Parse(HairType);
                PetUser.PetData.PetHair = Parse;

                if (PetUser.PetData.HaveSaddle == 2)
                    accessories += " 3 4 10 0";
                else if (PetUser.PetData.HaveSaddle == 1)
                    accessories += " 3 4 9 0";
                else
                    accessories += " 2";

                accessories += " 2 " + PetUser.PetData.PetHair + " " + PetUser.PetData.HairDye + " 3 " + PetUser.PetData.PetHair + " " + PetUser.PetData.HairDye;

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("UPDATE user_pets SET hairdye = '" + PetUser.PetData.HairDye + "', accessories = @acc WHERE id = " + PetUser.PetData.PetId);
                    dbClient.addParameter("acc", accessories);
                    dbClient.runQuery();
                }

                PetUser.PetData.Accessories = accessories;
            }
            else
            {
                var TypeSaddle = Item.GetBaseItem().Name.Replace("horse_saddle", "");

                PetUser.PetData.HaveSaddle = int.Parse(TypeSaddle);

                if (PetUser.PetData.HaveSaddle == 2)
                    accessories += " 3 4 10 0";
                else if (PetUser.PetData.HaveSaddle == 1)
                    accessories += " 3 4 9 0";
                else
                    accessories += " 2";

                accessories += " 2 " + PetUser.PetData.PetHair + " " + PetUser.PetData.HairDye + " 3 " + PetUser.PetData.PetHair + " " + PetUser.PetData.HairDye;

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("UPDATE user_pets SET have_saddle = '" + TypeSaddle + "', accessories = @acc WHERE id = " + PetUser.PetData.PetId);
                    dbClient.addParameter("acc", accessories);
                    dbClient.runQuery();
                }

                PetUser.PetData.Accessories = accessories;
            }

            var AddToPet = new ServerMessage(Outgoing.SerializeMontura);
            AddToPet.AppendInt32(PetUser.PetData.VirtualId);
            AddToPet.AppendUInt(PetUser.PetData.PetId);
            AddToPet.AppendUInt(PetUser.PetData.Type);
            AddToPet.AppendInt32(int.Parse(PetUser.PetData.Race));
            AddToPet.AppendString(PetUser.PetData.Color.ToLower());
            AddToPet.AppendInt32(1);
            string[] arrParts = PetUser.PetData.Accessories.Substring(1, PetUser.PetData.Accessories.Length - 1).Split(' ');
            foreach (string str in arrParts)
            {
                AddToPet.AppendInt32(int.Parse(str));
            }
            AddToPet.AppendBoolean((PetUser.PetData.HaveSaddle != 0));
            AddToPet.AppendBoolean(PetUser.montandoBol);
            Room.SendMessage(AddToPet);
        }

        internal void RemoveSaddle()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || (!Room.RoomData.AllowPets && !Room.CheckRights(Session, true)))
            {
                return;
            }

            var PetId = Request.PopWiredUInt();
            var PetUser = Room.GetRoomUserManager().GetPet(PetId);

            if (PetUser == null || PetUser.PetData == null || PetUser.PetData.OwnerId != Session.GetHabbo().Id)
            {
                return;
            }

            OtanixEnvironment.GetGame().GetCatalog().DeliverItems(Session, OtanixEnvironment.GetGame().GetItemManager().GetItem((PetUser.PetData.HaveSaddle == 1) ? EmuSettings.HORSECHAIR1 : EmuSettings.HORSECHAIR2), 1, "", false, 0);
            PetUser.PetData.HaveSaddle = 0;

            string accessories = " 2 2 " + PetUser.PetData.PetHair + " " + PetUser.PetData.HairDye + " 3 " + PetUser.PetData.PetHair + " " + PetUser.PetData.HairDye;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("UPDATE user_pets SET have_saddle = 0, accessories = @acc WHERE id = " + PetUser.PetData.PetId);
                dbClient.addParameter("acc", accessories);
                dbClient.runQuery();
            }
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
            PetUser.PetData.Accessories = accessories;

            var AddToPet = new ServerMessage(Outgoing.SerializeMontura);
            AddToPet.AppendInt32(PetUser.PetData.VirtualId);
            AddToPet.AppendUInt(PetUser.PetData.PetId);
            AddToPet.AppendUInt(PetUser.PetData.Type);
            AddToPet.AppendInt32(int.Parse(PetUser.PetData.Race));
            AddToPet.AppendString(PetUser.PetData.Color.ToLower());
            AddToPet.AppendInt32(1);
            string[] arrParts = PetUser.PetData.Accessories.Substring(1, PetUser.PetData.Accessories.Length - 1).Split(' ');
            foreach (string str in arrParts)
            {
                AddToPet.AppendInt32(int.Parse(str));
            }
            AddToPet.AppendBoolean((PetUser.PetData.HaveSaddle != 0));
            AddToPet.AppendBoolean(PetUser.montandoBol);
            Room.SendMessage(AddToPet);
        }

        internal void CancelMountOnPet()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null || User.montandoBol == false)
                return;

            var PetId = Request.PopWiredUInt();
            var Pet = Room.GetRoomUserManager().GetPet(PetId);

            if (Pet == null || Pet.PetData == null || User.montandoID != Pet.VirtualId)
            {
                return;
            }

            User.montandoBol = false;
            User.montandoID = 0;
            Pet.montandoBol = false;
            Pet.montandoID = 0;
            User.MoveTo(User.X + 1, User.Y + 1);
            User.ApplyEffect(0);
        }

        internal void MountOnPet()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            uint PetId = Request.PopWiredUInt();
            bool Type = Request.PopWiredBoolean();
            RoomUser petUser = Room.GetRoomUserManager().GetPet(PetId);

            if (petUser == null || petUser.PetData == null || petUser.PetData.HaveSaddle == 0 || ((User.montandoBol == true || petUser.montandoBol == true) && Type) || (!petUser.PetData.AllCanMount && User.HabboId != petUser.PetData.OwnerId))
                return;

            petUser.Statusses.Remove("sit");
            petUser.Statusses.Remove("lay");
            petUser.Statusses.Remove("snf");
            petUser.Statusses.Remove("eat");
            petUser.Statusses.Remove("ded");
            petUser.Statusses.Remove("jmp");
            petUser.Statusses.Remove("gst sml");
            petUser.Statusses.Remove("wng");
            petUser.Statusses.Remove("beg");
            petUser.Statusses.Remove("flm");

            if (Type)
            {
                petUser.Freezed = true;
                User.walkingToPet = petUser;
                User.MoveTo(petUser.X, petUser.Y);
            }
            else
            {
                User.montandoBol = false;
                User.montandoID = 0;
                petUser.montandoBol = false;
                petUser.montandoID = 0;
                User.MoveTo(User.SquareInFront);
                User.ApplyEffect(0);
            }
        }

        internal void AllCanMount()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            uint PetId = Request.PopWiredUInt();
            bool Type = Request.PopWiredBoolean();
            RoomUser petUser = Room.GetRoomUserManager().GetPet(PetId);
            if (petUser == null || petUser.PetData == null)
                return;

            petUser.PetData.AllCanMount = !petUser.PetData.AllCanMount;

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("UPDATE user_pets SET all_can_mount = '" + OtanixEnvironment.BoolToEnum(petUser.PetData.AllCanMount) + "' WHERE id = " + petUser.PetData.PetId);
            }
        }

        internal void CommandsPet()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var PetID = Request.PopWiredUInt();
            var PetUser = Room.GetRoomUserManager().GetPet(PetID);

            if (PetUser == null || PetUser.PetData == null)
                return;

            if(!PetOrders.PetsOrders.ContainsKey(PetUser.PetData.Type))
                return;

            List<int> Commands = PetOrders.PetsOrders[PetUser.PetData.Type];
            int ii = ((PetUser.PetData.Level + 3) > Commands.Count) ? Commands.Count : (PetUser.PetData.Level + 3);

            GetResponse().Init(Outgoing.CommandPet);
            GetResponse().AppendUInt(PetID);
            GetResponse().AppendInt32(Commands.Count);
            foreach (int parse in Commands)
            {
                GetResponse().AppendInt32(parse);
            }
            GetResponse().AppendInt32(ii);
            for (int i = 0; i < ii; i++ )
            {
                GetResponse().AppendInt32(Commands[i]);
            }
            SendResponse();
        }
        #endregion

        internal void GiveHanditem()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            var toGive = Room.GetRoomUserManager().GetRoomUserByHabbo(Request.PopWiredUInt());
            if (toGive == null)
                return;

            if (User.CarryItemID > 0 && User.CarryTimer > 0)
            {
                toGive.CarryItem(User.CarryItemID);
                User.CarryItem(0);

                ServerMessage Message = new ServerMessage(Outgoing.HandItemMessage);
                Message.AppendInt32(toGive.VirtualId);
                Message.AppendInt32(toGive.CarryItemID);
                toGive.GetClient().SendMessage(Message);
            }
        }

        internal void RemoveHanditem()
        {
            var Room = Session.GetHabbo().CurrentRoom;

            //if (Room == null || Room.IsPublic || (!Room.AllowPets && !Room.CheckRights(Session, true)))
            if (Room == null)
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (User.CarryItemID > 0)
                User.CarryItem(0);
        }

        internal void SaveWired()
        {
            var itemID = Request.PopWiredUInt();
            WiredSaver.HandleSave(Session, itemID, Session.GetHabbo().CurrentRoom, Request);
        }

        internal void SaveWiredConditions()
        {
            var itemID = Request.PopWiredUInt();
            WiredSaver.HandleConditionSave(Session, itemID, Session.GetHabbo().CurrentRoom, Request);
        }

        internal void ChangeManiquiInMemory()
        {
            var ItemId = Request.PopWiredInt32();
            var ManiquiName = OtanixEnvironment.FilterInjectionChars(Request.PopFixedString());

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            var Item = Room.GetRoomItemHandler().GetItem((uint)ItemId);
            if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.maniqui)
                return;

            if (BlackWordsManager.Check(ManiquiName, BlackWordType.Hotel, Session, "<NomeManequim>"))
                ManiquiName = "Bloqueado pelo filtro Bobba.";

            if(Item.ExtraData.Split(';').Length >= 2)
                Item.ExtraData = Item.ExtraData.Split(';')[0] + ";" + Item.ExtraData.Split(';')[1] + ";" + ManiquiName;
            else
                Item.ExtraData = "M;lg-270-82.ch-210-66;";

            var Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
            Item.Serialize(Message);
            Room.SendMessage(Message);
        }

        internal void SaveManiquiTODB()
        {
            var ItemId = Request.PopWiredInt32();

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            var Item = Room.GetRoomItemHandler().GetItem((uint)ItemId);
            if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.maniqui)
                return;

            #region Generate Look
            var Parts = Session.GetHabbo().Look.Split('.');
            Array.Sort(Parts);
            var NewLook = "";
            foreach (var Part in Parts)
            {
                if (Part.StartsWith("ch") || Part.StartsWith("lg") || Part.StartsWith("cc") || Part.StartsWith("ca") || Part.StartsWith("sh") || Part.StartsWith("wa"))
                {
                    var _Part = Part;
                    if (_Part.StartsWith("wa") && _Part.Contains("-0"))
                        _Part = _Part.Replace("-0", "");

                    NewLook += _Part + ".";
                }
            }
            NewLook = NewLook.Substring(0, NewLook.Length - 1);
            if (Item.ExtraData.Split(';').Length >= 3)
                Item.ExtraData = Session.GetHabbo().Gender + ";" + NewLook + ";" + Item.ExtraData.Split(';')[2];
            else
                Item.ExtraData = "M;lg-270-82.ch-210-66;";
            #endregion

            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("REPLACE INTO items_extradata VALUES (@itemid,@itemdata)");
                dbClient.addParameter("itemid", Item.Id);
                dbClient.addParameter("itemdata", Item.ExtraData);
                dbClient.runQuery();
            }

            var Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
            Item.Serialize(Message);
            Room.SendMessage(Message);
        }

        internal void ApplyBackgroundChanges()
        {
            int ItemId = Request.PopWiredInt32();
            if (ItemId < 0)
                return;

            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null || Room.GetRoomItemHandler() == null || !Room.CheckRights(Session, true))
                return;

            var Item = Room.GetRoomItemHandler().GetItem((uint)ItemId);

            if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.changeBackgrounds)
                return;

            var Tono = Request.PopWiredInt32();
            var Saturacion = Request.PopWiredInt32();
            var Luminosidad = Request.PopWiredInt32();

            Item.ExtraData = "on," + Tono + "," + Saturacion + "," + Luminosidad;

            var Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
            Item.Serialize(Message);
            Room.SendMessage(Message);
        }

        internal void SaveAdsMpu()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            uint ItemId = Request.PopWiredUInt();
            var Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return;

            uint bucleLenght = Request.PopWiredUInt();

            string ImageUrl = "";
            string strPlugin = "";

            if (Item.GetBaseItem().InteractionType == InteractionType.ads_mpu || Item.GetBaseItem().InteractionType == InteractionType.fxprovider)
            {
                for (int i = 0; i < bucleLenght; i++)
                {
                    strPlugin = Request.PopFixedString();
                    ImageUrl += strPlugin + ";";

                    if (strPlugin.Equals("imageUrl"))
                    {
                        i++;
                        strPlugin = Request.PopFixedString();

                        //if (OtanixEnvironment.isValidLink(strPlugin) == false)
                        //{
                        //  Session.SendNotif(LanguageLocale.GetValue("url.invalid"));
                        //   return;
                        //}

                        ImageUrl += strPlugin + ";";
                    }
                    else if (strPlugin.Equals("effectId"))
                    {
                        i++;
                        strPlugin = Request.PopFixedString();

                        int junk = 0;
                        if (!int.TryParse(strPlugin, out junk))
                        {
                            Session.SendNotif(LanguageLocale.GetValue("input.intonly"));
                            return;
                        }

                        ImageUrl += strPlugin + ";";
                    }
                    else if (strPlugin.StartsWith("offset"))
                    {
                        i++;
                        strPlugin = Request.PopFixedString();

                        int value = 0;
                        if (!int.TryParse(strPlugin, out value))
                        {
                            Session.SendNotif(LanguageLocale.GetValue("input.intonly"));
                            return;
                        }

                        if (value > 11000 || value < -11000)
                        {
                            Session.SendNotif("O valor máximo/minimo é 11000/-11000 de offset.");
                            return;
                        }

                        ImageUrl += strPlugin + ";";
                    }
                }

                Item.ExtraData = ImageUrl.Substring(0, ImageUrl.Length - 1);
                Item.UpdateState();

                ServerMessage Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
                Item.Serialize(Message);
                Room.SendMessage(Message);
            }
        }

        internal void StartYoutubeVideo()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            var itemId = Request.PopWiredUInt();
            var Item = Room.GetRoomItemHandler().GetItem(itemId);
            if (Item == null)
                return;

            var videoLink = Request.PopFixedString();

            if (!OtanixEnvironment.GetGame().GetYoutubeManager().Videos.ContainsKey((int)Item.GetBaseItem().ItemId))
                return;

            if (!OtanixEnvironment.GetGame().GetYoutubeManager().Videos[(int)Item.GetBaseItem().ItemId].Videos.ContainsKey(videoLink))
                return;

            GetResponse().Init(Outgoing.ShowYoutubeVideo);
            GetResponse().AppendUInt(itemId);
            GetResponse().AppendString(videoLink);
            GetResponse().AppendInt32(0);
            GetResponse().AppendInt32(0);
            GetResponse().AppendInt32(-1);
            SendResponse();

            if (videoLink.Length > 0)
            {
                Item.tvImage = LanguageLocale.GetValue("habbo.imaging.yttv") + videoLink;
                Item.videoOn = videoLink;
                Item.ExtraData = "1;" + Item.videoOn;

                var Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
                Item.Serialize(Message);
                Room.SendMessage(Message);
            }
            else
            {
                Item.videoOn = "";
                Item.ExtraData = "0;" + Item.videoOn;

                var Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
                Item.Serialize(Message);
                Room.SendMessage(Message);
            }
        }

        internal void ChangeYoutubeVideo()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            var itemId = Request.PopWiredUInt();
            var Item = Room.GetRoomItemHandler().GetItem(itemId);
            if (Item == null)
                return;

            var videoOrder = Request.PopWiredInt32();

            if (OtanixEnvironment.GetGame().GetYoutubeManager().Videos[(int)Item.GetBaseItem().ItemId].Videos.Count <= videoOrder)
                return;

            string videoLink = OtanixEnvironment.GetGame().GetYoutubeManager().Videos[(int)Item.GetBaseItem().ItemId].Videos.ElementAt(videoOrder).Key;

            GetResponse().Init(Outgoing.ShowYoutubeVideo);
            GetResponse().AppendUInt(itemId);
            GetResponse().AppendString(videoLink);
            GetResponse().AppendInt32(0);
            GetResponse().AppendInt32(0);
            GetResponse().AppendInt32(-1);
            SendResponse();

            if (videoLink.Length > 0)
            {
                Item.tvImage = LanguageLocale.GetValue("habbo.imaging.yttv") + videoLink;
                Item.videoOn = videoLink;
                Item.ExtraData = "1;" + Item.videoOn;

                var Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
                Item.Serialize(Message);
                Room.SendMessage(Message);
            }
            else
            {
                Item.videoOn = "";
                Item.ExtraData = "0;" + Item.videoOn;

                var Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
                Item.Serialize(Message);
                Room.SendMessage(Message);
            }
        }

        internal void UpdateItemTileHeight()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session))
                return;

            var itemID = Request.PopWiredInt32();
            if (itemID < 0)
                return;

            RoomItem Item = Room.GetRoomItemHandler().GetItem((UInt32)itemID);
            if (Item == null || !Item.GetBaseItem().Name.StartsWith("tile_stackmagic"))
                return;

            double Height = Request.PopWiredInt32();

            if (Height == -100)
                Height = Room.GetGameMap().SqAbsoluteHeight(Item.GetX, Item.GetY) * 100;
            else if (Height > 4000)
                Height = 4000;
            else if (Height < 0)
                Height = 0;

            ServerMessage UpdateTilePanel = new ServerMessage(Outgoing.UpdateTileStacker);
            UpdateTilePanel.AppendUInt(Item.Id);
            UpdateTilePanel.AppendUInt((uint)Height);
            Session.SendMessage(UpdateTilePanel);

            Height = Height / 100;

            if (Height < Room.GetGameMap().Model.SqFloorHeight[Item.GetX, Item.GetY])
                Height = Room.GetGameMap().Model.SqFloorHeight[Item.GetX, Item.GetY];

            Item.SetState(Item.GetX, Item.GetY, Height, Item.GetAffectedTiles);
            foreach (ThreeDCoord value in Item.GetAffectedTiles.Values)
            {
                Room.GetGameMap().ItemHeightMap[value.X, value.Y] = Height;
            }

            ServerMessage Message = new ServerMessage(Outgoing.UpdateItemOnRoom);
            Item.Serialize(Message);
            Room.SendMessage(Message);

            ItemCoords.ModifyGamemapTiles(Room, Item.GetAffectedTiles, Height);
        }

        internal void CreateGnomo()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            uint ItemId = Request.PopWiredUInt();

            RoomItem Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return;

            Room.GetRoomItemHandler().RemoveFurniture(Session, Item);

            GetResponse().Init(Outgoing.CloseGnomeBoxPanel);
            GetResponse().AppendUInt(ItemId);
            GetResponse().AppendInt32(0); // nameValidationStatus
            GetResponse().AppendString(""); // nameValidationInfo
            SendResponse();

            string PetName = Request.PopFixedString();
            var Pet = Catalog.CreatePet(Session, PetName, 26, "7", "ffffff");
            if (Pet == null)
                return;

            var PetUser = Room.GetRoomUserManager().DeployBot(new RoomBot(Pet.PetId, Pet.OwnerId, Pet.RoomId, AIType.Pet, true, Pet.Name, "", "", Pet.Look, Item.GetX, Item.GetY, 0, 0, false, "", 0, false), Pet);
            if (PetUser == null)
                return;

            Pet.X = Item.GetX;
            Pet.Y = Item.GetY;
            Pet.RoomId = Room.Id;
            Pet.PlacedInRoom = true;

            if (Pet.DBState != DatabaseUpdateState.NeedsInsert)
                Pet.DBState = DatabaseUpdateState.NeedsUpdate;
        }

        internal void ReloadFloorCommand()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || Room.GetGameMap() == null)
                return;

            ServerMessage SendItems = new ServerMessage(Outgoing.LoadFloorCommandItems);
            SendItems.AppendInt32(Room.GetGameMap().CoordinatedItems.Count); // baldosas bloqueadas
            foreach (Point Coord in Room.GetGameMap().CoordinatedItems.Keys)
            {
                SendItems.AppendInt32(Coord.X);
                SendItems.AppendInt32(Coord.Y);
            }
            Session.SendMessage(SendItems);

            ServerMessage SendData = new ServerMessage(Outgoing.LoadFloorCommandData);
            SendData.AppendInt32(Room.GetGameMap().Model.DoorX);
            SendData.AppendInt32(Room.GetGameMap().Model.DoorY);
            SendData.AppendInt32(Room.GetGameMap().Model.DoorOrientation);
            Session.SendMessage(SendData);
        }

        internal void SaveNewModelMap()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || Room.GetGameMap() == null || !Room.CheckRights(Session, true))
                return;


            string NewModel = Request.PopFixedString();
            int DoorX = Request.PopWiredInt32();
            int DoorY = Request.PopWiredInt32();
            int DoorOrientation = Request.PopWiredInt32();
            int WallThickness = Request.PopWiredInt32();
            int FloorThickness = Request.PopWiredInt32();
            int WallHeight = Request.PopWiredInt32();
            int WallHeightOld = Room.RoomData.WallHeight;

            if (NewModel.Length <= 0)
            {
                Session.SendNotif("O modelo tem uma largura menor.");
                return;
            }

            if (WallThickness < -2 || WallThickness > 1)
            {
                WallThickness = 0;
            }

            if (FloorThickness < -2 || FloorThickness > 1)
            {
                FloorThickness = 0;
            }

            if(DoorOrientation < 0 || DoorOrientation > 8)
            {
                DoorOrientation = 2;
            }

            if(WallHeight < -1 || WallHeight > 16)
            {
                WallHeight = -1;
            }

            // fast check
            char[] validLetters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', '\r' };
            foreach (char letter in NewModel)
            {
                if (!validLetters.Contains(letter))
                {
                    Session.SendNotif("O modelo tem caracteres inválidos. Lembre-se de colocar letras minusculas de a-k (x).");
                    return;
                }
            }

            if(NewModel.Last() == Convert.ToChar(13))
            {
                NewModel = NewModel.Remove(NewModel.Length - 1);
            }

            if (NewModel.Length > 4096 || NewModel.Split(Convert.ToChar(13)).Length > 128)
            {
                ServerMessage Message = new ServerMessage(Outgoing.CustomAlert);
                Message.AppendString("floorplan_editor.error");
                Message.AppendInt32(1);
                Message.AppendString("errors");
                Message.AppendString("(general): Altura muito grande (max 128 quadrados)\r(general): Area muito grande (max 4096 quadrados)");
                Session.SendMessage(Message);

                return;
            }

            if ((NewModel.Split((char)13).Length - 1) < DoorY)
            {
                ServerMessage Message = new ServerMessage(Outgoing.CustomAlert);
                Message.AppendString("floorplan_editor.error");
                Message.AppendInt32(1);
                Message.AppendString("errors");
                Message.AppendString("Y: A porta do quarto está em um local inadequado.");
                Session.SendMessage(Message);

                return;
            }

            string[] lines = NewModel.Split((char)13);
            int lineWidth = lines[0].Length;
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Length != lineWidth)
                {
                    ServerMessage Message = new ServerMessage(Outgoing.CustomAlert);
                    Message.AppendString("floorplan_editor.error");
                    Message.AppendInt32(1);
                    Message.AppendString("errors");
                    Message.AppendString("(general): Line " + (i + 1) + " is of different length than line 1");
                    Session.SendMessage(Message);

                    return;
                }
            }

            int DoorZ = RoomModel.parse(NewModel.Split('\r')[DoorY].ElementAt(DoorX), true);

            if (DoorZ == -1)
            {
                string[] novosValores = RoomModel.getRandomSquare(NewModel).Split('/');
                DoorX = Convert.ToInt16(novosValores[0]);
                DoorY = Convert.ToInt16(novosValores[1]);
                DoorZ = RoomModel.parse(NewModel.Split('\r')[DoorY].ElementAt(DoorX), true);
            }     

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("REPLACE INTO room_models_customs VALUES ('" + Room.RoomId + "', '" + DoorX + "','" + DoorY + "','" + DoorZ + "','" + DoorOrientation + "',@newmodel)");
                dbClient.addParameter("newmodel", NewModel);
                dbClient.runQuery();
            }

            Room.RoomData.FloorThickness = FloorThickness;
            Room.RoomData.WallThickness = WallThickness;
            Room.RoomData.WallHeight = WallHeight + DoorZ;
            Room.RoomData.LastModelName = Room.RoomData.ModelName;
            Room.RoomData.ModelName = "custom";
            Room.RoomData.roomNeedSqlUpdate = true;

            if(WallHeightOld != WallHeight)
                Room.GetRoomItemHandler().UpdateWallItems(WallHeightOld, WallHeight);

            List<RoomUser> test = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Room.RoomData.Id).GetRoomUserManager().GetRoomUsers();
            OtanixEnvironment.GetGame().GetRoomManager().UnloadRoom(Room);

            foreach (RoomUser usuario in test)
               usuario.GetClient().GetMessageHandler().enterOnRoom3(Room);
        }

        internal void FilterRoomPanel()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Request.PopWiredUInt());
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            ServerMessage Message = new ServerMessage(Outgoing.FilterRoomPanel);
            Message.AppendInt32(Room.RoomFilterWords.Count); // words count ??
            foreach (string word in Room.RoomFilterWords)
            {
                Message.AppendString(word);
            }
            Session.SendMessage(Message);
        }

        internal void AddWordToFilterRoom()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(Request.PopWiredUInt());
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            bool Adding = Request.PopWiredBoolean();
            string word = Request.PopFixedString();

            if (Adding)
            {
                if (Room.WordExist(word))
                    return;

                Room.AddFilterWord(word);

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("INSERT INTO room_filter (room_id,word) VALUES (" + Room.RoomId + ",@word)");
                    dbClient.addParameter("word", word);
                    dbClient.runQuery();
                }
            }
            else
            {
                Room.RemoveFilterWord(word);

                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("DELETE FROM room_filter WHERE room_id = " + Room.Id + " AND word = @word");
                    dbClient.addParameter("word", word);
                    dbClient.runQuery();
                }
            }

            var Update = new ServerMessage(Outgoing.UpdateRoom);
            Update.AppendUInt(Room.Id);
            Room.SendMessage(Update);

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_SelfModRoomFilterSeen", 1);
        }

        internal void CancelPoll()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (!Room.GotRoomPoll())
                return;

            if (!Session.GetHabbo().PollParticipation.Contains(Room.Id))
                Session.GetHabbo().PollParticipation.Add(Room.Id);
        }

        internal void InitPoll()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (!Room.GotRoomPoll())
                return;

            if (Room.GetRoomPoll().GetPollType() == PollType.ROOM_QUESTIONARY)
            {
                if (!Session.GetHabbo().PollParticipation.Contains(Room.Id))
                    Session.GetHabbo().PollParticipation.Add(Room.Id);
                else
                    return;
            }
            else if (Room.GetRoomPoll().GetPollType() == PollType.VOTE_QUESTIONARY)
            {
                VoteQuestionary handler = (VoteQuestionary)Room.GetRoomPoll();
                if (handler.UserVote(Session.GetHabbo().Id))
                    return;
            }

            Session.SendMessage(Room.GetRoomPoll().SerializePoll());
        }

        internal void EndPoll()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (!Room.GotRoomPoll())
                return;

            uint Id = Request.PopWiredUInt();
            uint ActualId = Request.PopWiredUInt();
            uint AnswersCount = Request.PopWiredUInt();
            string Answers = "";
            for(int i = 0; i < AnswersCount; i++)
            {
                Answers += Request.PopFixedString() + ";";
            }
            
            if (Answers.Length <= 0)
                return;

            Answers = Answers.Remove(Answers.Length - 1);

            ServerMessage Packet = Room.GetRoomPoll().SaveInformation(Session.GetHabbo().Id, Answers);

            if (Packet != null)
                Room.SendMessage(Packet);

            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_RoomCompetitionVoter", 1);
        }

        internal void EnableCamera()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            ServerMessage Message = new ServerMessage(Outgoing.CameraToken);
            Message.AppendString("67152056372501114565316423057012359");
            Message.AppendString("hhus");
            Session.SendMessage(Message);
        }

        internal void SaveFballClothes()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            uint ItemId = Request.PopWiredUInt();

            RoomItem Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null || Item.GetBaseItem() == null || Item.GetBaseItem().InteractionType != InteractionType.fbgate)
                return;

            string Gender = Request.PopFixedString();
            string Look = Request.PopFixedString();

            if (Gender.ToUpper() == "M")
            {
                Item.ExtraData = Look + "," + Item.ExtraData.Split(',')[1];
            }
            else if (Gender.ToUpper() == "F")
            {
                Item.ExtraData = Item.ExtraData.Split(',')[0] + "," + Look;
            }

            Item.UpdateState();
        }

        internal void CancelPetBreeding()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            uint ItemId = Request.PopWiredUInt();

            RoomItem Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return;

            if (Item.GetBaseItem().InteractionType != InteractionType.breedingpet)
                return;

            foreach (Pet pet in Item.havepetscount)
            {
                pet.waitingForBreading = 0;
                pet.breadingTile = new Point();

                RoomUser User = Room.GetRoomUserManager().GetRoomUserByVirtualId(pet.VirtualId);
                if (User != null)
                {
                    User.Freezed = false;
                    Room.GetGameMap().AddUserToMap(User, User.Coordinate);

                    var nextCoord = Room.GetGameMap().getRandomWalkableSquare();
                    User.MoveTo(nextCoord.X, nextCoord.Y);
                }
            }

            Item.havepetscount.Clear();
        }

        internal void CreatePetBreeding()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            if (Room.GetRoomUserManager().PetCount + 1 >= EmuSettings.MAX_BOTS_MESSAGES)
            {
                Session.SendNotif("Este quarto atingiu o limite de mascotes permitidos.");
                return;
            }

            uint ItemId = Request.PopWiredUInt();

            RoomItem Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return;

            if (Item.GetBaseItem().InteractionType != InteractionType.breedingpet)
                return;

            int PetRaceType = PetBreeding.GetPetIdByPet(Item.GetBaseItem().Name.Replace("pet_breeding_", ""));
            if (PetRaceType < 0)
                return;

            string PetName = Request.PopFixedString();
            // petid1
            // petid2

            Item.ExtraData = "1";
            Item.UpdateState();

            int randomNmb = new Random().Next(101);
            int petType = 0;
            int randomResult = 3;

            if (Item.GetBaseItem().InteractionType == InteractionType.breedingpet)
            {
                if (randomNmb == 1)
                {
                    if (PetRaceType == 0)
                        petType = PetBreeding.dogEpicRace[new Random().Next(PetBreeding.dogEpicRace.Length - 1)];
                    else if (PetRaceType == 1)
                        petType = PetBreeding.catEpicRace[new Random().Next(PetBreeding.catEpicRace.Length - 1)];
                    else if (PetRaceType == 2)
                        petType = PetBreeding.pigEpicRace[new Random().Next(PetBreeding.pigEpicRace.Length - 1)];
                    else if (PetRaceType == 3)
                        petType = PetBreeding.terrierEpicRace[new Random().Next(PetBreeding.terrierEpicRace.Length - 1)];
                    else if (PetRaceType == 4)
                        petType = PetBreeding.bearEpicRace[new Random().Next(PetBreeding.bearEpicRace.Length - 1)];

                    randomResult = 0;
                }
                else if (randomNmb <= 3)
                {
                    if (PetRaceType == 0)
                        petType = PetBreeding.dogRareRace[new Random().Next(PetBreeding.dogRareRace.Length - 1)];
                    else if (PetRaceType == 1)
                        petType = PetBreeding.catRareRace[new Random().Next(PetBreeding.catRareRace.Length - 1)];
                    else if (PetRaceType == 2)
                        petType = PetBreeding.pigRareRace[new Random().Next(PetBreeding.pigRareRace.Length - 1)];
                    else if (PetRaceType == 3)
                        petType = PetBreeding.terrierRareRace[new Random().Next(PetBreeding.terrierRareRace.Length - 1)];
                    else if (PetRaceType == 4)
                        petType = PetBreeding.bearRareRace[new Random().Next(PetBreeding.bearRareRace.Length - 1)];

                    randomResult = 1;
                }
                else if (randomNmb <= 6)
                {
                    if (PetRaceType == 0)
                        petType = PetBreeding.dogNoRareRace[new Random().Next(PetBreeding.dogNoRareRace.Length - 1)];
                    else if (PetRaceType == 1)
                        petType = PetBreeding.catNoRareRace[new Random().Next(PetBreeding.catNoRareRace.Length - 1)];
                    else if (PetRaceType == 2)
                        petType = PetBreeding.pigNoRareRace[new Random().Next(PetBreeding.pigNoRareRace.Length - 1)];
                    else if (PetRaceType == 3)
                        petType = PetBreeding.terrierNoRareRace[new Random().Next(PetBreeding.terrierNoRareRace.Length - 1)];
                    else if (PetRaceType == 4)
                        petType = PetBreeding.bearNoRareRace[new Random().Next(PetBreeding.bearNoRareRace.Length - 1)];

                    randomResult = 2;
                }
                else
                {
                    if (PetRaceType == 0)
                        petType = PetBreeding.dogNormalRace[new Random().Next(PetBreeding.dogNormalRace.Length - 1)];
                    else if (PetRaceType == 1)
                        petType = PetBreeding.catNormalRace[new Random().Next(PetBreeding.catNormalRace.Length - 1)];
                    else if (PetRaceType == 2)
                        petType = PetBreeding.pigNormalRace[new Random().Next(PetBreeding.pigNormalRace.Length - 1)];
                    else if (PetRaceType == 3)
                        petType = PetBreeding.terrierNormalRace[new Random().Next(PetBreeding.terrierNormalRace.Length - 1)];
                    else if (PetRaceType == 4)
                        petType = PetBreeding.bearNormalRace[new Random().Next(PetBreeding.bearNormalRace.Length - 1)];

                    randomResult = 3;
                }
            }

            var Pet = Catalog.CreatePet(Session, PetName, PetBreeding.GetBreedingByPet(PetRaceType), petType.ToString(), "ffffff");
            if (Pet == null)
                return;

            var PetUser = Room.GetRoomUserManager().DeployBot(new RoomBot(Pet.PetId, Pet.OwnerId, Pet.RoomId, AIType.Pet, true, Pet.Name, "", "", Pet.Look, Item.GetX, Item.GetY, 0, 0, false, "", 0, false), Pet);
            if (PetUser == null)
                return;

            Item.ExtraData = "2";
            Item.UpdateState();

            Room.GetRoomItemHandler().RemoveFurniture(Session, Item);

            if(Item.GetBaseItem().InteractionType == InteractionType.breedingpet)
            {
                if(Room.GetRoomItemHandler().breedingPet.ContainsKey(Item.Id))
                    Room.GetRoomItemHandler().breedingPet.Remove(Item.Id);

                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_" + OtanixEnvironment.cultureInfo.TextInfo.ToTitleCase(Item.GetBaseItem().Name.Replace("pet_breeding_", "")) + "Breeder", 1);
            }

            Session.GetMessageHandler().GetResponse().Init(Outgoing.RemovePetBreedingPanel);
            Session.GetMessageHandler().GetResponse().AppendUInt(ItemId);
            Session.GetMessageHandler().GetResponse().AppendInt32(0);
            Session.GetMessageHandler().SendResponse();

            Session.GetMessageHandler().GetResponse().Init(Outgoing.NewPetBreedingAlert);
            Session.GetMessageHandler().GetResponse().AppendUInt(Pet.PetId);
            Session.GetMessageHandler().GetResponse().AppendInt32(randomResult);
            Session.GetMessageHandler().SendResponse();

            Pet.X = Item.GetX;
            Pet.Y = Item.GetY;
            Pet.RoomId = Room.Id;
            Pet.PlacedInRoom = true;

            if (Pet.DBState != DatabaseUpdateState.NeedsInsert)
                Pet.DBState = DatabaseUpdateState.NeedsUpdate;

            foreach (Pet pet in Item.havepetscount)
            {
                pet.waitingForBreading = 0;
                pet.breadingTile = new Point();

                RoomUser User = Room.GetRoomUserManager().GetRoomUserByVirtualId(pet.VirtualId);
                User.Freezed = false;
                Room.GetGameMap().AddUserToMap(User, User.Coordinate);

                var nextCoord = Room.GetGameMap().getRandomWalkableSquare();
                User.MoveTo(nextCoord.X, nextCoord.Y);
            }

            Item.havepetscount.Clear();
        }

        internal void VerifyUsersLock()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            uint ItemId = Request.PopWiredUInt();

            RoomItem Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return;

            if(Item.usersLock.roomUserOne.HabboId == Session.GetHabbo().Id)
            {
                Item.usersLock.roomUserOneResponse = (Request.PopWiredBoolean() ? 2 : 1);

                if(Item.usersLock.roomUserOneResponse == 1)
                {
                    ServerMessage closeWindows = new ServerMessage(Outgoing.LoveLockDialogueCloseMessageComposer);
                    closeWindows.AppendUInt(Item.Id);
                    Item.usersLock.roomUserOne.GetClient().SendMessage(closeWindows);
                    Item.usersLock.roomUserTwo.GetClient().SendMessage(closeWindows);

                    Item.usersLock.ClearLock();
                    return;
                }
            }
            else if (Item.usersLock.roomUserTwo.HabboId == Session.GetHabbo().Id)
            {
                Item.usersLock.roomUserTwoResponse = (Request.PopWiredBoolean() ? 2 : 1);

                if (Item.usersLock.roomUserOneResponse == 1)
                {
                    ServerMessage closeWindows = new ServerMessage(Outgoing.LoveLockDialogueCloseMessageComposer);
                    closeWindows.AppendUInt(Item.Id);
                    Item.usersLock.roomUserOne.GetClient().SendMessage(closeWindows);
                    Item.usersLock.roomUserTwo.GetClient().SendMessage(closeWindows);

                    Item.usersLock.ClearLock();
                    return;
                }
            }

            ServerMessage loock = new ServerMessage(Outgoing.LoveLockDialogueSetLockedMessageComposer);
            loock.AppendUInt(Item.Id);
            Session.SendMessage(loock);

            if (Item.usersLock.roomUserOneResponse == 2 && Item.usersLock.roomUserTwoResponse == 2) // todo OK
            {
                Item.ExtraData = "1;" + Item.usersLock.roomUserOne.GetUsername() + ";" + Item.usersLock.roomUserTwo.GetUsername() + ";" + Item.usersLock.roomUserOne.GetClient().GetHabbo().Look + ";" + Item.usersLock.roomUserTwo.GetClient().GetHabbo().Look + ";" + DateTime.Now.ToString("dd/MM/yyyy");
                Item.UpdateState();

                ServerMessage message = new ServerMessage(Outgoing.UpdateItemOnRoom);
                Item.Serialize(message);
                Room.SendMessage(message);
            }
        }

        internal void RenderRoomMessageComposer()
        {
            ServerMessage Photo = new ServerMessage(Outgoing.TakedRoomPhoto);
            Session.SendMessage(Photo);
        }

        internal void RenderRoomMessageComposerBigPhoto()
        {
            string str = Camera.Decompiler(Request.ReadBytes(Request.PopWiredInt32()));       
            string roomIdJSON = URLPost.GetDataFromJSON(str, "roomid");
            double timestamp = double.Parse(URLPost.GetDataFromJSON(str, "timestamp"));
            string timestampJSON = (timestamp - (timestamp % 100)).ToString();

            Session.GetHabbo().lastPhotoPreview = roomIdJSON + "-" + timestampJSON;
            ServerMessage Message = new ServerMessage(Outgoing.CameraToken);
            Message.AppendString(EmuSettings.CAMERA_SERVER_LOAD + URLPost.GetMD5(Session.GetHabbo().lastPhotoPreview) + ".png");
            Session.SendMessage(Message);
        }

        internal void GetCraftableInfo()
        {
            string ItemName = Request.PopFixedString();
            Session.SendMessage(OtanixEnvironment.GetGame().GetCraftableProductsManager().GetItemMessage(ItemName));
        }

        internal void GetCraftingRecipesAvailableComposer()
        {
            uint itemId = Request.PopWiredUInt();
            uint itemsCount = Request.PopWiredUInt();
            uint[] myItems = new uint[itemsCount];

            for(int i = 0; i < itemsCount; i++)
            {
                uint ItemId = Request.PopWiredUInt();
                UserItem uItem = Session.GetHabbo().GetInventoryComponent().GetItem(ItemId);
                if (uItem == null)
                    continue;

                myItems[i] = uItem.BaseItem;
            }

            uint count= 0;
            bool result = OtanixEnvironment.GetGame().GetCraftableProductsManager().GetSimilarItems(myItems, ref count);

            ServerMessage Message = new ServerMessage(Outgoing.CheckDisposeCraftingMessageComposer);
            Message.AppendUInt(count);
            Message.AppendBoolean(result);
            Session.SendMessage(Message);
        }

        internal void InitializeCraftable()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            uint CraftingTable = Request.PopWiredUInt();
            string ItemName = Request.PopFixedString();
            bool result = false;

            uint ItemId = OtanixEnvironment.GetGame().GetItemManager().GetBaseIdFromItemName(ItemName);
            if (ItemId <= 0)
                return;

            Item Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(ItemId);
            if (Item == null)
                return;

            CraftableProduct craftableProd = OtanixEnvironment.GetGame().GetCraftableProductsManager().GetCraftableProduct(ItemName);
            if (craftableProd == null) // este item no está en la mesa de crafteos.
                return;

            if (craftableProd.ContainsElements(Session.GetHabbo().GetInventoryComponent()))
            {
                foreach (uint BaseId in craftableProd.GetReqIds)
                {
                    Session.GetHabbo().GetInventoryComponent().RemoveItemByBaseId(BaseId);
                }

                Session.GetHabbo().GetInventoryComponent().AddNewItem(0, Item.ItemId, "", true, false, false, Item.Name, Session.GetHabbo().Id, 0);
                Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                result = true;
            }

            GetResponse().Init(Outgoing.CraftingResultMessageComposer);
            GetResponse().AppendBoolean(result);
            GetResponse().AppendString(ItemName);
            GetResponse().AppendString(ItemName);
            SendResponse();
        }

        internal void RefreshCraftingTable()
        {
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeItemInventory());
            Session.SendMessage(OtanixEnvironment.GetGame().GetCraftableProductsManager().GetMessage());
        }

        internal void CraftSecretComposer()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            uint CraftingTable = Request.PopWiredUInt();
            uint itemsCount = Request.PopWiredUInt();
            uint[] myItems = new uint[itemsCount];
            bool result = false;

            for (int i = 0; i < itemsCount; i++)
            {
                uint ItemId = Request.PopWiredUInt();
                UserItem uItem = Session.GetHabbo().GetInventoryComponent().GetItem(ItemId);
                if (uItem == null)
                    continue;

                myItems[i] = uItem.BaseItem;
            }

            uint CraftBaseId = OtanixEnvironment.GetGame().GetCraftableProductsManager().GetRandomCraftableId(myItems);
            Item Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(CraftBaseId);
            if (Item == null)
                return;

            CraftableProduct craftableProd = OtanixEnvironment.GetGame().GetCraftableProductsManager().GetCraftableProduct(Item.Name);
            if (craftableProd == null) // este item no está en la mesa de crafteos.
                return;

            if (craftableProd.ContainsElements(Session.GetHabbo().GetInventoryComponent()))
            {
                foreach (uint BaseId in craftableProd.GetReqIds)
                {
                    Session.GetHabbo().GetInventoryComponent().RemoveItemByBaseId(BaseId);
                }

                Session.GetHabbo().GetInventoryComponent().AddNewItem(0, Item.ItemId, "", true, false, false, Item.Name, Session.GetHabbo().Id, 0);
                Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                result = true;
            }

            GetResponse().Init(Outgoing.CraftingResultMessageComposer);
            GetResponse().AppendBoolean(result);
            GetResponse().AppendString(Item.Name);
            GetResponse().AppendString(Item.Name);
            SendResponse();
        }
    }
}