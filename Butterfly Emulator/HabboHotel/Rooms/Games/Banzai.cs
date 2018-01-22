using Butterfly.HabboHotel.GameClients;
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
    class Banzai
    {
        private int TilesToEnd;
        private List<RoomItem> banzaiTiles;
        private Dictionary<uint, RoomItem> pucks;
        private Room room;
        public Banzai(Room room)
        {
            this.room = room;
            banzaiTiles = new List<RoomItem>();
            pucks = new Dictionary<uint, RoomItem>();
        }

        public void AddTile(RoomItem Item)
        {
            banzaiTiles.Add(Item);
        }

        public void RemoveTile(RoomItem Item)
        {
            banzaiTiles.Remove(Item);
        }

        public void PrepareGame()
        {
            TilesToEnd = banzaiTiles.Count;

            foreach (RoomItem Item in banzaiTiles)
            {
                Item.ExtraData = "1";
                Item.value = 0;
                Item.team = Team.none;
                Item.UpdateState();
            }
        }
        public void EndGame(Team winners, Room Room)
        {
            foreach (RoomItem Item in banzaiTiles)
            {
                if (Item.team == Team.none)
                {
                    Item.ExtraData = "0";
                    Item.UpdateState();
                }
                else if(Item.team == winners)
                {
                    Item.interactionCount = 0;
                    Item.interactionCountHelper = 0;
                    Item.UpdateNeeded = true;
                }
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
                            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(user.GetClient().GetHabbo().Id, "ACH_GamePlayerExperience", user.BanzaiPoints);
                            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(user.GetClient().GetHabbo().Id, "ACH_BattleBallWinner", Room.GetGameManager().GetTeamPoints((int)winners));
                        }
                    }

                    if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null)
                    {
                        OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(user.GetClient().GetHabbo().Id, "ACH_BattleBallTilesLocked", user.BanzaiPoints);
                        OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(user.GetClient().GetHabbo().Id, "ACH_BattleBallPlayer", 1);
                    }

                    user.classPoints = 0;
                    user.BanzaiPoints = 0;
                }
            }
        }
        internal void AddPuck(RoomItem item)
        {
            if (!pucks.ContainsKey(item.Id))
                pucks.Add(item.Id, item);
        }

        internal void RemovePuck(uint itemID)
        {
            pucks.Remove(itemID);
        }
        public void OnUserWalk(RoomUser User)
        {
            if (User == null)
                return;

            foreach (RoomItem item in pucks.Values)
            {
                int differenceX = User.X - item.GetX;
                int differenceY = User.Y - item.GetY;

                if (differenceX <= 1 && differenceX >= -1 && differenceY <= 1 && differenceY >= -1)
                {
                    int NewX = differenceX * -1;
                    int NewY = differenceY * -1;

                    NewX = NewX + item.GetX;
                    NewY = NewY + item.GetY;

                    if (item.interactingBallUser == User.HabboId && room.GetGameMap().ValidTile(NewX, NewY))
                    {
                        item.interactingBallUser = 0;

                        MovePuck(item, User.GetClient(), User.Coordinate, item.Coordinate, 6, User.team);
                    }
                    else if (room.GetGameMap().ValidTile(NewX, NewY))
                    {
                        MovePuck(item, User.GetClient(), NewX, NewY, User.team);
                    }
                }
            }

            if(room.GetGameManager().IsGameStarted() && !room.GetGameManager().IsGamePaused())
            {
                HandleBanzaiTiles(User.Coordinate, User.team, User);
            }
        }

        internal void MovePuck(RoomItem item, GameClient mover, int newX, int newY, Team team)
        {
            RoomUser user = mover.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(mover.GetHabbo().Id);

            if (!room.GetGameMap().itemCanBePlacedHere(newX, newY))
                return;

            Point oldRoomCoord = item.Coordinate;

            Double NewZ = room.GetGameMap().Model.SqFloorHeight[newX, newY];

            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
                return;
            
           // item.ExtraData = ((int)team).ToString();
           // item.UpdateNeeded = true;
           // item.UpdateState();

            var mMessage = new ServerMessage(Outgoing.ObjectOnRoller); // Cf
            mMessage.AppendInt32(item.GetX);
            mMessage.AppendInt32(item.GetY);
            mMessage.AppendInt32(newX);
            mMessage.AppendInt32(newY);
            mMessage.AppendInt32(1);
            mMessage.AppendUInt(item.Id);
            mMessage.AppendString(TextHandling.GetString(item.GetZ));
            mMessage.AppendString(TextHandling.GetString(item.GetZ));
            mMessage.AppendUInt(item.Id);
            room.SendMessage(mMessage);

            room.GetRoomItemHandler().SetFloorItem(mover, item, newX, newY, item.Rot, false, false, false, false);

            if (mover == null || mover.GetHabbo() == null)
                return;

            if (room.GetGameManager().IsGameStarted() && !room.GetGameManager().IsGamePaused())
            {
                HandleBanzaiTiles(new Point(newX, newY), team, user);
            }
        }
        internal void MovePuck(RoomItem item, GameClient client, Point user, Point ball, int length, Team team)
        {
            int differenceX = user.X - ball.X;
            int differenceY = user.Y - ball.Y;

            if (differenceX <= 1 && differenceX >= -1 && differenceY <= 1 && differenceY >= -1)
            {
                List<Point> affectedTiles = new List<Point>();
                int newX = ball.X;
                int newY = ball.Y;
                for (int i = 1; i < length; i++)
                {
                    newX = differenceX * -i;
                    newY = differenceY * -i;

                    newX = newX + item.GetX;
                    newY = newY + item.GetY;

                    if (!room.GetGameMap().itemCanBePlacedHere(newX, newY))
                    {
                        if (i == 1)
                            break;

                        if (i != length)
                            affectedTiles.Add(new Point(newX, newY));
                        i = i - 1;
                        newX = differenceX * -i;
                        newY = differenceY * -i;

                        newX = newX + item.GetX;
                        newY = newY + item.GetY;
                        break;
                    }
                    else
                    {
                        if (i != length)
                            affectedTiles.Add(new Point(newX, newY));
                    }
                }
                if (client == null || client.GetHabbo() == null)
                    return;

                RoomUser _user = client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);

                foreach (Point coord in affectedTiles)
                {
                    HandleBanzaiTiles(coord, team, _user);
                }

                if (newX != ball.X || newY != ball.Y)
                    MovePuck(item, client, newX, newY, team);
            }
        }

        public void HandleBanzaiTiles(Point coord, Team team, RoomUser user)
        {
            if (team == Team.none)
                return;

            List<RoomItem> items = room.GetGameMap().GetCoordinatedItems(coord);
            foreach (RoomItem _item in items)
            {
                if (_item.GetBaseItem().InteractionType != InteractionType.banzaifloor)
                    continue;

                if (_item.GetX != coord.X || _item.GetY != coord.Y)
                    continue;

                SetTile(_item, team, user, room);
                _item.UpdateState(false, true);

                if (TilesToEnd <= 0)
                    room.GetGameManager().EndGame();
            }


            /*RoomItem item = room.GetGameMap().getMaxHeightItem(coord);

            if (item.GetBaseItem().InteractionType != InteractionType.banzaifloor || item.value >= 3)
                return;

            SetTile(item, team, user, room);
            item.UpdateState(false, true);

            if (TilesToEnd <= 0)
                room.GetGameManager().EndGame();*/
        }

        private void SetTile(RoomItem item, Team team, RoomUser user, Room Room)
        {
            if (item.team == team)
            {
                if (item.value < 3)
                {
                    item.value++;
                    if (item.value == 3)
                    {
                        Room.GetGameManager().AddPointsToTeam(item.team, 1, user);
                        TilesToEnd--;
                        user.BanzaiPoints++;
                    }
                }
            }
            else
            {
                if (item.value < 3)
                {
                    item.team = team;
                    item.value = 1;
                }
            }

            var newColor = item.value + ((int)item.team * 3) - 1;
            item.ExtraData = newColor.ToString();
        }

        public void Destroy()
        {
            banzaiTiles.Clear();
            banzaiTiles = null;
        }
    }
}
