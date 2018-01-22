using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Games
{
    class Freeze
    {
        private RoomItem exitTile;

        private List<RoomItem> freezeTiles;
        private List<RoomItem> freezeBlocks;

        public Freeze()
        {
            freezeTiles = new List<RoomItem>();
            freezeBlocks = new List<RoomItem>();
        }

        public void SetExitTile(RoomItem Item)
        {
            exitTile = Item;
        }

        public void AddFreezeTile(RoomItem Item)
        {
            freezeTiles.Add(Item);
        }

        public void AddFreezeBlock(RoomItem Item)
        {
            freezeBlocks.Add(Item);
        }

        public void RemoveFreezeTile(RoomItem Item)
        {
            freezeTiles.Remove(Item);
        }

        public void RemoveFreezeBlock(RoomItem Item)
        {
            freezeBlocks.Remove(Item);
        }

        public bool FreezeEnable()
        {
            return exitTile != null;
        }

        public void UpdateExitTile(string Extradata)
        {
            if (FreezeEnable())
            {
                exitTile.ExtraData = Extradata;
                exitTile.UpdateState();
            }
        }

        public void PrepareGame(Room Room)
        {
            foreach (RoomItem Item in freezeBlocks)
            {
                Item.ExtraData = "";
                Item.UpdateState(false, true);

                Room.GetGameMap().AddItemToMap(Item, false);
            }

            foreach (RoomUser User in Room.GetRoomUserManager().UserList.Values)
            {
                if (User.team != Team.none)
                {
                    if (User.CurrentEffect < 40 || User.CurrentEffect > 43) // not
                        continue;

                    List<RoomItem> items = GetItemsForSquare(User.Coordinate, Room);
                    if (SquareGotFreezeTile(items))
                        continue;

                    Room.GetTeamManager().OnUserLeave(User);
                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(0);
                    User.team = Team.none;
                }
                else
                {
                    List<RoomItem> items = GetItemsForSquare(User.Coordinate, Room);
                    if (SquareGotFreezeTile(items))
                    {
                        if (FreezeEnable())
                            Room.GetGameMap().TeleportToItem(User, exitTile);
                    }
                }
            }
        }

        public void EndGame(Team winners, Room Room)
        {
            int totalPoints = 0;

            foreach (var user in Room.GetRoomUserManager().UserList.Values)
            {
                if (user.team == winners)
                    totalPoints += user.FreezePoints;
            }

            if (winners != Team.none)
            {
                foreach (RoomUser user in Room.GetRoomUserManager().GetRoomUsers())
                {
                    if (user == null || user.team == Team.none || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() == null)
                        continue;

                    if (winners == user.team)
                    {
                        var Action = new ServerMessage(Outgoing.Action);
                        Action.AppendInt32(user.VirtualId);
                        Action.AppendInt32(1);
                        Room.SendMessage(Action);

                        if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null)
                        {
                            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(user.GetClient().GetHabbo().Id, "ACH_FreezeWinner", totalPoints);
                        }
                    }

                    if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null)
                    {
                        OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(user.GetClient().GetHabbo().Id, "ACH_EsA", user.FreezePoints);
                        OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(user.GetClient().GetHabbo().Id, "ACH_FreezePlayer", 1);
                    }

                    user.classPoints = 0;
                    user.FreezePoints = 0;
                }
            }
        }

        public void OnUserWalk(RoomUser User)
        {
            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(User.RoomId);
            if (Room == null)
                return;

            if (!Room.GetGameManager().IsGameStarted() || Room.GetGameManager().IsGamePaused() || User.team == Team.none)
                return;

            if (User.X == User.GoalX && User.GoalY == User.Y && User.throwBallAtGoal)
            {
                foreach (RoomItem item in freezeTiles)
                {
                    if (item.interactionCountHelper == 0)
                    {
                        if (item.GetX == User.X && item.GetY == User.Y)
                        {
                            item.interactionCountHelper = 1;
                            item.ExtraData = "1000";
                            item.UpdateState();
                            item.InteractingUser = User.HabboId;
                            item.freezePowerUp = User.banzaiPowerUp;
                            item.ReqUpdate(4, true);

                            switch (User.banzaiPowerUp)
                            {
                                case FreezePowerUp.GreenArrow:
                                case FreezePowerUp.OrangeSnowball:
                                    {
                                        User.banzaiPowerUp = FreezePowerUp.None;
                                        break;
                                    }
                            }
                            break;
                        }
                    }
                }
            }

            foreach (RoomItem item in freezeBlocks)
            {
                if (User.X == item.GetX && User.Y == item.GetY)
                {
                    if (item.freezePowerUp != FreezePowerUp.None)
                    {
                        PickUpPowerUp(item, User);
                    }
                }
            }
        }

        public void onFreezeTiles(RoomItem item, FreezePowerUp powerUp, uint userID)
        {
            Room Room = item.GetRoom();
            if (Room == null)
                return;

            RoomUser user = Room.GetRoomUserManager().GetRoomUserByHabbo(userID);
            if (user == null)
                return;

            List<RoomItem> items;

            switch (powerUp)
            {
                case FreezePowerUp.BlueArrow:
                    {
                        items = GetVerticalItems(item.GetX, item.GetY, 5, user);
                        break;
                    }
                case FreezePowerUp.GreenArrow:
                    {
                        items = GetDiagonalItems(item.GetX, item.GetY, 5, user);
                        break;
                    }
                case FreezePowerUp.OrangeSnowball:
                    {
                        items = GetVerticalItems(item.GetX, item.GetY, 5, user);
                        items.AddRange(GetDiagonalItems(item.GetX, item.GetY, 5, user));
                        break;
                    }
                default:
                    {
                        items = GetVerticalItems(item.GetX, item.GetY, 3, user);
                        break;
                    }
            }

            HandleBanzaiFreezeItems(items);
            
            items.Clear();
            items = null;
        }

        public void CycleUser(RoomUser User)
        {
            if (User.Freezed)
            {
                User.FreezeCounter++;

                if (User.FreezeCounter > 10)
                {
                    User.Freezed = false;
                    User.FreezeCounter = 0;

                    ActivateShield(User);
                }
            }

            if (User.shieldActive)
            {
                User.shieldCounter++;

                if (User.shieldCounter > 10)
                {
                    User.shieldActive = false;
                    User.shieldCounter = 10;

                    User.ApplyEffect((int)User.team + 39);
                }
            }
        }

        private void PickUpPowerUp(RoomItem item, RoomUser user)
        {
            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(user.RoomId);
            if (Room == null)
                return;

            switch (item.freezePowerUp)
            {
                case FreezePowerUp.Heart:
                    {
                        if (user.FreezeLives < 3)
                        {
                            user.FreezeLives++;
                            Room.GetGameManager().AddPointsToTeam(user.team, 10, user);
                        }

                        ServerMessage message = new ServerMessage(Outgoing.UpdateFreezeLives);
                        message.AppendInt32(user.InternalRoomID);
                        message.AppendInt32(user.FreezeLives);
                        user.GetClient().SendMessage(message);

                        break;
                    }
                case FreezePowerUp.Shield:
                    {
                        ActivateShield(user);
                        break;
                    }
                case FreezePowerUp.BlueArrow:
                case FreezePowerUp.GreenArrow:
                case FreezePowerUp.OrangeSnowball:
                    {
                        user.banzaiPowerUp = item.freezePowerUp;
                        break;
                    }
            }

            item.freezePowerUp = FreezePowerUp.None;
            item.ExtraData = "1" + item.ExtraData;
            item.UpdateState(false, true);

            if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null)
                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(user.GetClient().GetHabbo().Id, "ACH_FreezePowerUp", 1);
        }

        private List<RoomItem> GetVerticalItems(int x, int y, int length, RoomUser user)
        {
            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(user.RoomId);
            if (Room == null)
                return null;

            var totalItems = new List<RoomItem>();

            for (var i = 0; i < length; i++)
            {
                var point = new Point(x + i, y);

                var items = GetItemsForSquare(point, Room);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point, user);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 1; i < length; i++)
            {
                var point = new Point(x, y + i);

                var items = GetItemsForSquare(point, Room);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point, user);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 1; i < length; i++)
            {
                var point = new Point(x - i, y);
                var items = GetItemsForSquare(point, Room);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point, user);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 1; i < length; i++)
            {
                var point = new Point(x, y - i);
                var items = GetItemsForSquare(point, Room);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point, user);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            return totalItems;
        }

        private List<RoomItem> GetDiagonalItems(int x, int y, int length, RoomUser user)
        {
            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(user.RoomId);
            if (Room == null)
                return null;

            var totalItems = new List<RoomItem>();

            for (var i = 0; i < length; i++)
            {
                var point = new Point(x + i, y + i);

                var items = GetItemsForSquare(point, Room);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point, user);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 0; i < length; i++)
            {
                var point = new Point(x - i, y - i);
                var items = GetItemsForSquare(point, Room);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point, user);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 0; i < length; i++)
            {
                var point = new Point(x - i, y + i);
                var items = GetItemsForSquare(point, Room);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point, user);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 0; i < length; i++)
            {
                var point = new Point(x + i, y - i);
                var items = GetItemsForSquare(point, Room);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point, user);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            return totalItems;
        }

        private void HandleUserFreeze(Point point, RoomUser userThrow)
        {
            Room Room = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(userThrow.RoomId);
            if (Room == null)
                return;

            var user = Room.GetGameMap().GetRoomUsers(point).FirstOrDefault();

            if (user != null)
            {
                if (user.IsWalking && user.SetX != point.X && user.SetY != point.Y)
                    return;
                
                FreezeUser(user, userThrow, Room);
            }
        }

        private void FreezeUser(RoomUser user, RoomUser userThrow, Room room)
        {
            if (user.IsBot || user.shieldActive || user.team == Team.none || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (user.Freezed)
            {
                //user.Freezed = false;
                //user.ApplyEffect((int)user.team + 39);
                return;
            }

            user.Freezed = true;
            user.FreezeCounter = 0;
            user.FreezeLives--;

            userThrow.FreezePoints++;

            if (user.FreezeLives <= 0 && FreezeEnable())
            {
                var message2 = new ServerMessage(Outgoing.UpdateFreezeLives);
                message2.AppendInt32(user.InternalRoomID);
                message2.AppendInt32(user.FreezeLives);
                user.GetClient().SendMessage(message2);

                user.ApplyEffect(0);
                room.GetGameManager().AddPointsToTeam(user.team, -20, user);
                var t = room.GetTeamManager();
                t.OnUserLeave(user);
                user.team = Team.none;
                if (FreezeEnable())
                    room.GetGameMap().TeleportToItem(user, exitTile);

                user.Freezed = false;
                user.SetStep = false;
                user.IsWalking = false;
                user.UpdateNeeded = true;

                Team winner = Team.none;
                for (int i = 1; i < 5; i++ )
                {
                    if (t.EmptyTeam(i))
                        continue;

                    if (winner != Team.none)
                    {
                        winner = Team.none;
                        break;
                    }

                    winner = (Team)i;
                }

                if (winner != Team.none)
                    room.GetGameManager().EndGame();

                return;
            }

            room.GetGameManager().AddPointsToTeam(user.team, -10, user);
            user.ApplyEffect(12);

            var message = new ServerMessage(Outgoing.UpdateFreezeLives);
            message.AppendInt32(user.InternalRoomID);
            message.AppendInt32(user.FreezeLives);
            user.GetClient().SendMessage(message);
        }

        private void HandleBanzaiFreezeItems(List<RoomItem> items)
        {
            foreach (var item in items)
            {
                switch (item.GetBaseItem().InteractionType)
                {
                    case InteractionType.freezetile:
                        {
                            item.ExtraData = "11000";
                            item.UpdateState(false, true);
                            continue;
                        }

                    case InteractionType.freezetileblock:
                        {
                            SetRandomPowerUp(item);
                            item.UpdateState(false, true);
                            continue;
                        }
                }
            }
        }

        private void SetRandomPowerUp(RoomItem item)
        {
            if (!string.IsNullOrEmpty(item.ExtraData))
                return;

            var next = new Random().Next(1, 14);

            switch (next)
            {
                case 2:
                    {
                        item.ExtraData = "2000";
                        item.freezePowerUp = FreezePowerUp.BlueArrow;
                        break;
                    }
                case 3:
                    {
                        item.ExtraData = "3000";
                        item.freezePowerUp = FreezePowerUp.Snowballs;
                        break;
                    }
                case 4:
                    {
                        item.ExtraData = "4000";
                        item.freezePowerUp = FreezePowerUp.GreenArrow;
                        break;
                    }
                case 5:
                    {
                        item.ExtraData = "5000";
                        item.freezePowerUp = FreezePowerUp.OrangeSnowball;
                        break;
                    }
                case 6:
                    {
                        item.ExtraData = "6000";
                        item.freezePowerUp = FreezePowerUp.Heart;
                        break;
                    }
                case 7:
                    {
                        item.ExtraData = "7000";
                        item.freezePowerUp = FreezePowerUp.Shield;
                        break;
                    }
                default:
                    {
                        item.ExtraData = "1000";
                        item.freezePowerUp = FreezePowerUp.None;
                        break;
                    }
            }

            item.GetRoom().GetGameMap().RemoveFromMap(item, false);
            item.UpdateState(false, true);
        }

        private List<RoomItem> GetItemsForSquare(Point point, Room room)
        {
            return room.GetGameMap().GetCoordinatedItems(point);
        }

        private bool SquareGotFreezeTile(List<RoomItem> items)
        {
            foreach (RoomItem item in items)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.freezetile)
                    return true;
            }

            return false;
        }

        private bool SquareGotFreezeBlock(List<RoomItem> items)
        {
            foreach (var item in items)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.freezetileblock)
                    return true;
            }

            return false;
        }

        private void ActivateShield(RoomUser user)
        {
            user.ApplyEffect((int)user.team + 48);
            user.shieldActive = true;
            user.shieldCounter = 0;
        }

        public void Destroy()
        {
            freezeTiles.Clear();
            freezeTiles = null;

            freezeBlocks.Clear();
            freezeBlocks = null;

            exitTile = null;
        }
    }

    enum FreezePowerUp
    {
        None = 0,
        BlueArrow = 1,
        GreenArrow = 2,
        Shield = 3,
        Heart = 4,
        OrangeSnowball = 5,
        Snowballs = 6
    }
}