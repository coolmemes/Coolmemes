using System;
using System.Collections.Generic;
using System.Linq;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using System.Drawing;
using HabboEvents;
using ButterStorm.HabboHotel.Rooms;
using Butterfly.HabboHotel.Rooms.Games.BolasMovimento;
using Butterfly.HabboHotel.Pathfinding;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;

namespace Butterfly.HabboHotel.Rooms.Games
{
    class Soccer : Rebug
    {
        private Room room;
        private RoomItem ball;
        private List<RoomItem> goals;
        private Rebug rebugMove;
        private Push pushMove;
        private Cloud cloudMove;

        public Soccer(Room room)
        {
            this.room = room;
            this.ball = null;
            this.rebugMove = null;
            this.pushMove = null;
            this.cloudMove = null;
            this.goals = new List<RoomItem>();
        }

        internal RoomItem Ball
        {
            get { return ball; }
        }

        internal void OnCycle()
        {
            if (ball != null)
            {
                if (ball.ballIsMoving)
                {
                    if (ball.GetBaseItem().Name == "futebol_rebug")
                    {
                        rebugMove.MoveBallProcess(ball);
                    }else if(ball.GetBaseItem().Name == "futebol_push")
                    {
                        pushMove.MoveBallProcess(ball);
                    }else if (ball.GetBaseItem().Name == "futebol_cloud")
                    {
                        cloudMove.MoveBallProcess(ball);
                    }

                }
            }
        }

        internal void AddBall(RoomItem item)
        {
            if (ball == null)
            {
                ball = item;
                OtanixEnvironment.GetGame().GetRoomManager().QueueBallAdd(room);

                if(ball.GetBaseItem().Name == "futebol_rebug")
                {
                    rebugMove = new Rebug(room, this);
                }
                else if (ball.GetBaseItem().Name == "futebol_push")
                {
                    pushMove = new Push(room, this);
                }
                else if (ball.GetBaseItem().Name == "futebol_cloud")
                {
                    cloudMove = new Cloud(room, this);
                }
            }
        }

        internal void RemoveBall()
        {
            if (ball != null)
            {
                ball = null;
                OtanixEnvironment.GetGame().GetRoomManager().QueueBallRemove(room);
            }
        }

        internal void AddGoal(RoomItem item)
        {
            if (!this.goals.Contains(item))
                this.goals.Add(item);
        }

        internal void onGateRemove(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.footballgoalred:
                case InteractionType.footballgoalgreen:
                case InteractionType.footballgoalblue:
                case InteractionType.footballgoalyellow:
                case InteractionType.footballcounterred:
                case InteractionType.footballcountergreen:
                case InteractionType.footballcounterblue:
                case InteractionType.footballcounteryellow:
                    {
                        if (item.GetBaseItem().InteractionType == InteractionType.footballgoalred || item.GetBaseItem().InteractionType == InteractionType.footballgoalgreen || item.GetBaseItem().InteractionType == InteractionType.footballgoalblue || item.GetBaseItem().InteractionType == InteractionType.footballgoalyellow)
                            this.goals.Remove(item);

                        room.GetGameManager().RemoveTeamItem(item, item.team);
                        break;
                    }
            }
        }

        internal bool HandleFootballGameItems(Point ballItemCoord, RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().CurrentRoom == null)
                return false;

            foreach (RoomItem item in this.goals)
            {
                foreach (ThreeDCoord tile in item.GetAffectedTiles.Values)
                {
                    if (tile.X == ballItemCoord.X && tile.Y == ballItemCoord.Y)
                    {
                        room.GetGameManager().AddPointsToTeam(item.team, 1, user);

                        ServerMessage Action = new ServerMessage(Outgoing.Action);
                        Action.AppendInt32(user.VirtualId);
                        Action.AppendInt32(1);
                        user.GetClient().GetHabbo().CurrentRoom.SendMessage(Action);

                        if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null)
                        {
                            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(user.GetClient().GetHabbo().Id, "ACH_FootballGoalScored", 1);

                            var Receiver = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(room.RoomData.OwnerId);
                            OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(room.RoomData.OwnerId, "ACH_FootballGoalScoredInRoom", 1);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        internal void OnUserWalk(RoomUser User)
        {
            if (User == null)
                return;

            if (ball == null)
                return;

            if(ball.GetBaseItem().Name == "futebol_rebug")
            {
                rebugMove.onUserwalk(User, ball);
            }
            else if (ball.GetBaseItem().Name == "futebol_push")
            {
                pushMove.onUserwalk(User, ball);
            }
            else if (ball.GetBaseItem().Name == "futebol_cloud")
            {
                cloudMove.onUserwalk(User, ball);
            }
        }

        private static bool isSoccerGoal(InteractionType type)
        {
            return (type == InteractionType.footballgoalblue || type == InteractionType.footballgoalgreen || type == InteractionType.footballgoalred || type == InteractionType.footballgoalyellow);
        }

        private bool TileContainsGoal(Point P)
        {
            List<RoomItem> items = room.GetGameMap().GetCoordinatedItems(P);
            if (items.Count > 0)
            {
                foreach (RoomItem item in items)
                {
                    if (isSoccerGoal(item.GetBaseItem().InteractionType))
                        return true;
                }
            }

            return false;
        }

        internal void Destroy()
        {
            room = null;
            ball = null;
        }

        /*
         *             
         * if (TileContainsGoal(new Point(item.GetX, item.GetY)))
            {
                item.ballIsMoving = false;
                return false;
            }
         * 
         */
    }
}
