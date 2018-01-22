using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Pathfinding;
using Butterfly.Messages;
using ButterStorm.HabboHotel.Rooms;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Games.BolasMovimento
{
    class Rebug : IMovimentos
    {
        private Room room;
        private Soccer soccer;

        public Rebug(Room room, Soccer soccer)
        {
            this.room = room;
            this.soccer = soccer;
        }
        public Rebug()
        {

        }

        public void onUserwalk(RoomUser User, RoomItem ball)
        {
            if (User == null)
                return;

            if (ball == null)
                return;

            if (User.SetX == ball.GetX && User.SetY == ball.GetY && (ball._iBallValue == 5 || ball._iBallValue == 6)) // start the second game ball... inversing:
            {

            }

            else if (User.SetX == ball.GetX && User.SetY == ball.GetY && User.GoalX == ball.GetX && User.GoalY == ball.GetY && User.handelingBallStatus == 0) // super chute.
            {
                Point userPoint = new Point(User.X, User.Y);
                ball.ExtraData = "55";
                ball.ballIsMoving = true;
                ball._iBallValue = 1;
                ball.ballMover = User;
                MoveBall(ball, User.GetClient(), userPoint);
            }
            else if (User.SetX == ball.GetX && User.SetY == ball.GetY && User.GoalX == ball.GetX && User.GoalY == ball.GetY && User.handelingBallStatus == 1) // super chute quando para de andar
            {
                User.handelingBallStatus = 0;
                ComeDirection _comeDirection = _ComeDirection.GetComeDirection(new Point(User.X, User.Y), ball.Coordinate);
                if (_comeDirection != ComeDirection.NULL)
                {
                    int NewX = User.SetX;
                    int NewY = User.SetY;

                    _ComeDirection.GetNewCoords(_comeDirection, ref NewX, ref NewY);
                    if (ball.GetRoom().GetGameMap().ValidTile(NewX, NewY))
                    {
                        Point userPoint = new Point(User.X, User.Y);
                        ball.ExtraData = "55";
                        ball.ballIsMoving = true;
                        ball._iBallValue = 1;
                        ball.ballMover = User;
                        MoveBall(ball, User.GetClient(), userPoint);
                    }
                }
            }
            else if (User.X == ball.GetX && User.Y == ball.GetY && User.handelingBallStatus == 0)
            {
                Point userPoint = new Point(User.SetX, User.SetY);
                ball.ExtraData = "55";
                ball.ballIsMoving = true;
                ball._iBallValue = 1;
                ball.ballMover = User;
                MoveBall(ball, User.GetClient(), userPoint);
            }
            else
            {
                if (User.handelingBallStatus == 0 && User.GoalX == ball.GetX && User.GoalY == ball.GetY)
                    return;

                if (User.SetX == ball.GetX && User.SetY == ball.GetY && User.IsWalking && (User.X != User.GoalX || User.Y != User.GoalY))
                {
                    User.handelingBallStatus = 1;
                    ComeDirection _comeDirection = _ComeDirection.GetComeDirection(new Point(User.X, User.Y), ball.Coordinate);
                    if (_comeDirection != ComeDirection.NULL)
                    {
                        int NewX = User.SetX;
                        int NewY = User.SetY;

                        _ComeDirection.GetNewCoords(_comeDirection, ref NewX, ref NewY);

                        if (!room.GetGameMap().itemCanBePlacedHere(NewX, NewY) && User.handelingBallStatus == 1)
                        {
                            ball.comeDirection = _ComeDirection.InverseDirections(room, ball.comeDirection, NewX, NewY);
                            NewX = User.X;
                            NewY = User.Y;
                            ball.ExtraData = "11";
                            MoveBall(ball, User, NewX, NewY);
                        }
                        else if (room.GetGameMap().SquareHasUsers(NewX, NewY) && User.handelingBallStatus == 1)
                        {
                            ball.comeDirection = _ComeDirection.InverseDirections(room, ball.comeDirection, NewX, NewY);
                            NewX = User.X;
                            NewY = User.Y;
                            ball.ExtraData = "11";
                            MoveBall(ball, User, NewX, NewY);
                        }
                        else if (ball.GetRoom().GetGameMap().ValidTile(NewX, NewY, true))
                        {
                            ball.ExtraData = "11";
                            MoveBall(ball, User, NewX, NewY);
                        }
                        else
                        {
                            ball.comeDirection = _ComeDirection.InverseDirections(room, ball.comeDirection, NewX, NewY);
                            NewX = User.X;
                            NewY = User.Y;
                            ball.ExtraData = "11";
                            MoveBall(ball, User, NewX, NewY);
                        }
                    }
                }
            }
        }

        public bool MoveBall(RoomItem item, RoomUser mover, int newX, int newY)
        {
            if (item == null || item.GetBaseItem() == null /*|| mover == null || mover.GetHabbo() == null*/)
                return false;

            if (!room.GetGameMap().itemCanBePlacedHere(newX, newY))
                return false;

            if (mover != null && mover.handelingBallStatus == 1)
            {
                if (room.GetGameMap().SquareHasUsers(newX, newY) && item._iBallValue > 1)
                    return false;
            }

            Point oldRoomCoord = item.Coordinate;
            Double NewZ = room.GetGameMap().Model.SqFloorHeight[newX, newY];

             ServerMessage mMessage2 = new ServerMessage(Outgoing.BallUpdate); // Cf
             mMessage2.AppendUInt(item.Id);
             mMessage2.AppendInt32(item.GetBaseItem().SpriteId);
             mMessage2.AppendInt32(newX);
             mMessage2.AppendInt32(newY);
             mMessage2.AppendInt32(4); // rot;
             mMessage2.AppendString((String.Format("{0:0.00}", TextHandling.GetString(item.GetZ))));
             mMessage2.AppendString((String.Format("{0:0.00}", TextHandling.GetString(item.GetZ))));
             mMessage2.AppendUInt(0);
             mMessage2.AppendUInt(0);
             mMessage2.AppendString(item.ExtraData);
             mMessage2.AppendInt32(-1);
             mMessage2.AppendUInt(0);
             mMessage2.AppendUInt(item.OwnerId);
             room.SendFastMessage(mMessage2);

            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
                return false;

            item.SetState(newX, newY, item.GetZ, Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, item.Rot));

            if (mover != null)
                return soccer.HandleFootballGameItems(new Point(newX, newY), mover);

            return false;
        }

        public void MoveBall(RoomItem item, GameClient client, Point user)
        {
            try
            {
                item.comeDirection = _ComeDirection.GetComeDirection(user, item.Coordinate);

                if (item.comeDirection != ComeDirection.NULL)
                {
                    MoveBallProcess(item);
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public void MoveBallProcess(RoomItem item)
        {
            int tryes = 0;
            int newX = item.Coordinate.X;
            int newY = item.Coordinate.Y;
            int resetX;
            int resetY;

            while (tryes < 3)
            {
                if (room == null)
                    return;

                if (room.GetGameMap() == null)
                {
                    room.FixGameMap();
                    if (room.GetGameMap() == null)
                        return;
                }

                if (item.comeDirection == ComeDirection.NULL)
                {
                    item.ballIsMoving = false;
                    break;
                }

                resetX = newX;
                resetY = newY;

                _ComeDirection.GetNewCoords(item.comeDirection, ref newX, ref newY);

                if (room.GetGameMap().SquareHasUsers(newX, newY) && tryes == 1 && (item._iBallValue == 2 || item._iBallValue == 3)) // se chuta ela no quadrado da frente, e ela volta, ela atravessa o usuario
                {
                }
                else if (!room.GetGameMap().itemCanBePlacedHere(newX, newY) || room.GetGameMap().SquareHasUsers(newX, newY)) // start the second game ball... inversing:
                {

                    item.comeDirection = _ComeDirection.InverseDirections(room, item.comeDirection, newX, newY);
                    newX = resetX;
                    newY = resetY;
                    tryes++;
                    if (tryes > 2)
                        item.ballIsMoving = false;
                    continue;
                }

                if (MoveBall(item, item.ballMover, newX, newY))
                {
                    item.ballIsMoving = false;
                    break;
                }

                int Number = 11;
                int.TryParse(item.ExtraData, out Number);
                if (Number > 11)
                    item.ExtraData = (int.Parse(item.ExtraData) - 11).ToString();
                item._iBallValue++;

                if (item._iBallValue > 6)
                {
                    item.ballIsMoving = false;
                    item._iBallValue = 1;
                    item.ballMover = null;
                }
                break;
            }
        }
    }

    class Push : IMovimentos
    {
        private Room room;
        private Soccer soccer;
        public Push(Room room, Soccer soccer)
        {
            this.room = room;
            this.soccer = soccer;
        }
        public Push() {
        }
        public void onUserwalk(RoomUser User, RoomItem ball)
        {
            if (User == null)
                return;

            if (ball == null)
                return;

            if (User.SetX == ball.GetX && User.SetY == ball.GetY && User.GoalX == ball.GetX && User.GoalY == ball.GetY && User.handelingBallStatus == 0) // super chute.
            {
                Point userPoint = new Point(User.X, User.Y);
                ball.ExtraData = "55";
                ball.ballIsMoving = true;
                ball._iBallValue = 1;
                ball.ballMover = User;
                MoveBall(ball, User.GetClient(), userPoint);
            }
            else if (User.X == ball.GetX && User.Y == ball.GetY && User.handelingBallStatus == 0)
            {
                Point userPoint = new Point(User.SetX, User.SetY);
                ball.ExtraData = "55";
                ball.ballIsMoving = true;
                ball._iBallValue = 1;
                ball.ballMover = User;
                MoveBall(ball, User.GetClient(), userPoint);
            }
            else
            {
                if (User.handelingBallStatus == 0 && User.GoalX == ball.GetX && User.GoalY == ball.GetY)
                    return;

                if (User.SetX == ball.GetX && User.SetY == ball.GetY && User.IsWalking && (User.X != User.GoalX || User.Y != User.GoalY))
                {
                    User.handelingBallStatus = 1;
                    ComeDirection _comeDirection = _ComeDirection.GetComeDirection(new Point(User.X, User.Y), ball.Coordinate);
                    if (_comeDirection != ComeDirection.NULL)
                    {
                        int NewX = User.SetX;
                        int NewY = User.SetY;

                        _ComeDirection.GetNewCoords(_comeDirection, ref NewX, ref NewY);
                        if (ball.GetRoom().GetGameMap().ValidTile(NewX, NewY))
                        {
                            ball.ExtraData = "11";
                            MoveBall(ball, User, NewX, NewY);
                        }
                    }
                }
            }
        }

        public bool MoveBall(RoomItem item, RoomUser mover, int newX, int newY)
        {
            if (item == null || item.GetBaseItem() == null /*|| mover == null || mover.GetHabbo() == null*/)
                return false;

            if (!room.GetGameMap().itemCanBePlacedHere(newX, newY))
                return false;

            if (mover != null && mover.handelingBallStatus == 1)
            {
                if (room.GetGameMap().SquareHasUsers(newX, newY))
                    return false;
            }

            Point oldRoomCoord = item.Coordinate;
            Double NewZ = room.GetGameMap().Model.SqFloorHeight[newX, newY];

            ServerMessage mMessage2 = new ServerMessage(Outgoing.BallUpdate); // Cf
            mMessage2.AppendUInt(item.Id);
            mMessage2.AppendInt32(item.GetBaseItem().SpriteId);
            mMessage2.AppendInt32(newX);
            mMessage2.AppendInt32(newY);
            mMessage2.AppendInt32(4); // rot;
            mMessage2.AppendString((String.Format("{0:0.00}", TextHandling.GetString(item.GetZ))));
            mMessage2.AppendString((String.Format("{0:0.00}", TextHandling.GetString(item.GetZ))));
            mMessage2.AppendUInt(0);
            mMessage2.AppendUInt(0);
            mMessage2.AppendString(item.ExtraData);
            mMessage2.AppendInt32(-1);
            mMessage2.AppendUInt(0);
            mMessage2.AppendUInt(item.OwnerId);
            room.SendFastMessage(mMessage2);

            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
                return false;

            item.SetState(newX, newY, item.GetZ, Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, item.Rot));

            if (mover != null)
                return soccer.HandleFootballGameItems(new Point(newX, newY), mover);

            return false;
        }

        public void MoveBall(RoomItem item, GameClient client, Point user)
        {
            try
            {
                item.comeDirection = _ComeDirection.GetComeDirection(user, item.Coordinate);

                if (item.comeDirection != ComeDirection.NULL)
                {
                    MoveBallProcess(item);
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public void MoveBallProcess(RoomItem item)
        {
            int tryes = 0;
            int newX = item.Coordinate.X;
            int newY = item.Coordinate.Y;
            int resetX;
            int resetY;

            while (tryes < 3)
            {
                if (room == null)
                    return;

                if (room.GetGameMap() == null)
                {
                    room.FixGameMap();
                    if (room.GetGameMap() == null)
                        return;
                }

                if (item.comeDirection == ComeDirection.NULL)
                {
                    item.ballIsMoving = false;
                    break;
                }

                resetX = newX;
                resetY = newY;

                _ComeDirection.GetNewCoords(item.comeDirection, ref newX, ref newY);

                if (room.GetGameMap().SquareHasUsers(newX, newY)) // break 100 %, cannot return the ball
                {
                    if (!(item._iBallValue == 1 || item._iBallValue == 2)) // Si está a 1c o 2c, atraviesa al otro usuario.
                    {
                        RoomUser userTile = room.GetRoomUserManager().GetUserForSquare(newX, newY);
                        if (userTile != null && userTile.IsWalking)
                        {
                            ComeDirection cd = _ComeDirection.GetUserComeDirection(userTile);
                            if (_ComeDirection.GetInverseDirectionEasy(item.comeDirection) == cd || userTile.GoalX == newX && userTile.GoalY == newY)
                                item._iBallValue = 6;
                        }
                        else
                        {
                            item.ballIsMoving = false;
                            break;
                        }
                    }
                }

                if (!room.GetGameMap().itemCanBePlacedHere(newX, newY)) // start the second game ball... inversing:
                {
                    item.comeDirection = _ComeDirection.InverseDirections(room, item.comeDirection, newX, newY);
                    newX = resetX;
                    newY = resetY;
                    tryes++;
                    if (tryes > 2)
                        item.ballIsMoving = false;
                    continue;
                }

                if (MoveBall(item, item.ballMover, newX, newY))
                {
                    item.ballIsMoving = false;
                    break;
                }

                int Number = 11;
                int.TryParse(item.ExtraData, out Number);
                if (Number > 11)
                    item.ExtraData = (int.Parse(item.ExtraData) - 11).ToString();
                item._iBallValue++;

                if (item._iBallValue > 6)
                {
                    item.ballIsMoving = false;
                    item._iBallValue = 1;
                    item.ballMover = null;
                }
                break;
            }
        }
    }

    class Cloud : IMovimentos
    {
        private Room room;
        private Soccer soccer;
        public Cloud(Room room, Soccer soccer)
        {
            this.room = room;
            this.soccer = soccer;
        }
        public Cloud()
        {
        }

        public void onUserwalk(RoomUser User, RoomItem item)
        {
            int NewX = 0;
            int NewY = 0;
            int differenceX = User.X - item.GetX;
            int differenceY = User.Y - item.GetY;

            if (differenceX == 0 && differenceY == 0)
            {
                if (User.RotBody == 4)
                {
                    NewX = User.X;
                    NewY = User.Y + 2;
                    item.ExtraData = "55";
                    item.ballIsMoving = true;
                    item._iBallValue = 1;

                }
                else if (User.RotBody == 6)
                {
                    NewX = User.X - 2;
                    NewY = User.Y;
                    item.ExtraData = "55";
                    item.ballIsMoving = true;
                    item._iBallValue = 1;

                }
                else if (User.RotBody == 0)
                {
                    NewX = User.X;
                    NewY = User.Y - 2;
                    item.ExtraData = "55";
                    item.ballIsMoving = true;
                    item._iBallValue = 1;

                }
                else if (User.RotBody == 2)
                {
                    NewX = User.X + 2;
                    NewY = User.Y;
                    item.ExtraData = "55";
                    item.ballIsMoving = true;
                    item._iBallValue = 1;

                }
                else if (User.RotBody == 1)
                {
                    NewX = User.X + 2;
                    NewY = User.Y - 2;
                    item.ExtraData = "55";
                    item.ballIsMoving = true;
                    item._iBallValue = 1;

                }
                else if (User.RotBody == 7)
                {
                    NewX = User.X - 2;
                    NewY = User.Y - 2;
                    item.ExtraData = "55";
                    item.ballIsMoving = true;
                    item._iBallValue = 1;

                }
                else if (User.RotBody == 3)
                {
                    NewX = User.X + 2;
                    NewY = User.Y + 2;
                    item.ExtraData = "55";
                    item.ballIsMoving = true;
                    item._iBallValue = 1;

                }
                else if (User.RotBody == 5)
                {
                    NewX = User.X - 2;
                    NewY = User.Y + 2;
                    item.ExtraData = "55";
                    item.ballIsMoving = true;
                    item._iBallValue = 1;
                }

                if (!room.GetRoomItemHandler().CheckPosItem(User.GetClient(), item, NewX, NewY, item.Rot, false, false))
                {
                    if (User.RotBody == 0)
                    {
                        NewX = User.X;
                        NewY = User.Y + 1;
                    }
                    else if (User.RotBody == 2)
                    {
                        NewX = User.X - 1;
                        NewY = User.Y;
                    }
                    else if (User.RotBody == 4)
                    {
                        NewX = User.X;
                        NewY = User.Y - 1;
                    }
                    else if (User.RotBody == 6)
                    {
                        NewX = User.X + 1;
                        NewY = User.Y;
                    }
                    else if (User.RotBody == 5)
                    {
                        NewX = User.X + 1;
                        NewY = User.Y - 1;
                    }
                    else if (User.RotBody == 3)
                    {
                        NewX = User.X - 1;
                        NewY = User.Y - 1;
                    }
                    else if (User.RotBody == 7)
                    {
                        NewX = User.X + 1;
                        NewY = User.Y + 1;
                    }
                    else if (User.RotBody == 1)
                    {
                        NewX = User.X - 1;
                        NewY = User.Y + 1;
                    }
                }
            }
            else if (differenceX <= 1 && differenceX >= -1 && differenceY <= 1 && differenceY >= -1 && VerifyBall(User, item.Coordinate.X, item.Coordinate.Y))//VERYFIC BALL CHECAR SI ESTA EN DIRECCION ASIA LA PELOTA
            {
                NewX = differenceX * -1;
                NewY = differenceY * -1;

                NewX = NewX + item.GetX;
                NewY = NewY + item.GetY;
            }

            if (item.GetRoom().GetGameMap().ValidTile(NewX, NewY))
            {
                MoveBall(item, User, NewX, NewY);
            }
        }
        private bool VerifyBall(RoomUser user, int actualx, int actualy)
        {
            return Rotation.Calculate(user.X, user.Y, actualx, actualy) == user.RotBody;
        }
        public bool MoveBall(RoomItem item, RoomUser mover, int newX, int newY)
        {
            if (item == null || item.GetBaseItem() == null /*|| mover == null || mover.GetHabbo() == null*/)
                return false;

            if (!room.GetGameMap().itemCanBePlacedHere(newX, newY))
                return false;

            if (mover != null && mover.handelingBallStatus == 1)
            {
                if (room.GetGameMap().SquareHasUsers(newX, newY))
                    return false;
            }

            Point oldRoomCoord = item.Coordinate;
            Double NewZ = room.GetGameMap().Model.SqFloorHeight[newX, newY];

            ServerMessage mMessage2 = new ServerMessage(Outgoing.BallUpdate); // Cf
            mMessage2.AppendUInt(item.Id);
            mMessage2.AppendInt32(item.GetBaseItem().SpriteId);
            mMessage2.AppendInt32(newX);
            mMessage2.AppendInt32(newY);
            mMessage2.AppendInt32(4); // rot;
            mMessage2.AppendString((String.Format("{0:0.00}", TextHandling.GetString(item.GetZ))));
            mMessage2.AppendString((String.Format("{0:0.00}", TextHandling.GetString(item.GetZ))));
            mMessage2.AppendUInt(0);
            mMessage2.AppendUInt(0);
            mMessage2.AppendString(item.ExtraData);
            mMessage2.AppendInt32(-1);
            mMessage2.AppendUInt(0);
            mMessage2.AppendUInt(item.OwnerId);
            room.SendFastMessage(mMessage2);

            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
                return false;

            item.SetState(newX, newY, item.GetZ, Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, item.Rot));

            if (mover != null)
                return soccer.HandleFootballGameItems(new Point(newX, newY), mover);

            return false;
        }

        public void MoveBall(RoomItem item, GameClient client, Point user)
        {
            try
            {
                item.comeDirection = _ComeDirection.GetComeDirection(user, item.Coordinate);

                if (item.comeDirection != ComeDirection.NULL)
                {
                    MoveBallProcess(item, client);
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public void MoveBallProcess(RoomItem item)
        {
        }

        public void MoveBallProcess(RoomItem item, GameClient client)
        {
            var tryes = 0;
            var newX = item.Coordinate.X;
            var newY = item.Coordinate.Y;

            //while (tryes < 3)
            {
                if (room == null || room.GetGameMap() == null)
                    return;

                var total = item.ExtraData == "55" ? 6 : 1;
                for (var i = 0; i != total; i++)
                {
                    if (item.comeDirection == ComeDirection.NULL)
                    {
                        item.ballIsMoving = false;
                        break;
                    }

                    var resetX = newX;
                    var resetY = newY;

                    _ComeDirection.GetNewCoords(item.comeDirection, ref newX, ref newY);

                    var ignoreUsers = false;

                    if (room.GetGameMap().SquareHasUsers(newX, newY))
                    {
                        if (item.ExtraData != "55" && item.ExtraData != "44")
                        {
                            item.ballIsMoving = false;
                            break;
                        }
                        ignoreUsers = true;
                    }

                    if (ignoreUsers == false)
                        if (!room.GetGameMap().itemCanBePlacedHere(newX, newY))
                        {
                            item.comeDirection = _ComeDirection.InverseDirections(room, item.comeDirection, newX, newY);
                            newX = resetX;
                            newY = resetY;
                            tryes++;
                            if (tryes > 2)
                                item.ballIsMoving = false;
                            continue;
                        }
                    RoomUser roomuserTest = room.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
                    if (roomuserTest == null)
                        break;

                    if (MoveBall(item, roomuserTest, newX, newY))
                    {
                        item.ballIsMoving = false;
                        break;
                    }

                    int number;
                    int.TryParse(item.ExtraData, out number);
                    if (number > 11)
                        item.ExtraData = (int.Parse(item.ExtraData) - 11).ToString();

                }

                item._iBallValue++;

                if (item._iBallValue <= 6)
                    return;
                item.ballIsMoving = false;
                item._iBallValue = 1;
                //break;
            }
        }
    }
}
