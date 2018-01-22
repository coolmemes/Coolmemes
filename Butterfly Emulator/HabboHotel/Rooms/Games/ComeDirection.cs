using Butterfly.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButterStorm.HabboHotel.Rooms
{
    enum ComeDirection
    {
        UP,
        UP_RIGHT,
        RIGHT,
        DOWN_RIGHT,
        DOWN,
        DOWN_LEFT,
        LEFT,
        UP_LEFT,
        NULL
    }

    public class _ComeDirection
    {
        internal static ComeDirection InverseDirections(Room room, ComeDirection comeWith, int x, int y)
        {
            try
            {
                if (comeWith == ComeDirection.UP)
                {
                    return ComeDirection.DOWN;
                }
                else if (comeWith == ComeDirection.UP_RIGHT)
                {
                    
                   return ComeDirection.DOWN_LEFT;
                }
                else if (comeWith == ComeDirection.RIGHT)
                {
                    return ComeDirection.LEFT;
                }
                else if (comeWith == ComeDirection.DOWN_RIGHT)
                {                  
                    return ComeDirection.UP_LEFT;
                }
                else if (comeWith == ComeDirection.DOWN)
                {
                    return ComeDirection.UP;
                }
                else if (comeWith == ComeDirection.DOWN_LEFT)
                {
                    return ComeDirection.UP_RIGHT;
                }
                else if (comeWith == ComeDirection.LEFT)
                {
                    return ComeDirection.RIGHT;
                }
                else if (comeWith == ComeDirection.UP_LEFT)
                {
                    return ComeDirection.DOWN_RIGHT;
                }
                return ComeDirection.NULL;
            }
            catch
            {
                return ComeDirection.NULL;
            }
        }
        internal static ComeDirection GetInverseDirectionEasy(ComeDirection comeWith)
        {
            try
            {
                if (comeWith == ComeDirection.UP)
                {
                    return ComeDirection.DOWN;
                }
                else if (comeWith == ComeDirection.UP_RIGHT)
                {
                    return ComeDirection.DOWN_LEFT;
                }
                else if (comeWith == ComeDirection.RIGHT)
                {
                    return ComeDirection.LEFT;
                }
                else if (comeWith == ComeDirection.DOWN_RIGHT)
                {
                    return ComeDirection.UP_LEFT;
                }
                else if (comeWith == ComeDirection.DOWN)
                {
                    return ComeDirection.UP;
                }
                else if (comeWith == ComeDirection.DOWN_LEFT)
                {
                    return ComeDirection.UP_RIGHT;
                }
                else if (comeWith == ComeDirection.LEFT)
                {
                    return ComeDirection.RIGHT;
                }
                else if (comeWith == ComeDirection.UP_LEFT)
                {
                    return ComeDirection.DOWN_RIGHT;
                }
                return ComeDirection.NULL;
            }
            catch
            {
                return ComeDirection.NULL;
            }
        }

        internal static void GetNewCoords(ComeDirection comeWith, ref  int newX, ref int newY)
        {
            try
            {
                if (comeWith == ComeDirection.UP)
                {
                    // newX = newX;
                    newY++;
                }
                else if (comeWith == ComeDirection.UP_RIGHT)
                {
                    newX--;
                    newY++;
                }
                else if (comeWith == ComeDirection.RIGHT)
                {
                    newX--;
                    // newY = newY;
                }
                else if (comeWith == ComeDirection.DOWN_RIGHT)
                {
                    newX--;
                    newY--;
                }
                else if (comeWith == ComeDirection.DOWN)
                {
                    // newX = newX;
                    newY--;
                }
                else if (comeWith == ComeDirection.DOWN_LEFT)
                {
                    newX++;
                    newY--;
                }
                else if (comeWith == ComeDirection.LEFT)
                {
                    newX++;
                    // newY = newY;
                }
                else if (comeWith == ComeDirection.UP_LEFT)
                {
                    newX++;
                    newY++;
                }
            }
            catch { }
        }

        internal static ComeDirection GetComeDirection(Point user, Point ball)
        {
            try
            {
                if (user.X == ball.X && user.Y - 1 == ball.Y)
                    return ComeDirection.DOWN;
                else if (user.X + 1 == ball.X && user.Y - 1 == ball.Y)
                    return ComeDirection.DOWN_LEFT;
                else if (user.X + 1 == ball.X && user.Y == ball.Y)
                    return ComeDirection.LEFT;
                else if (user.X + 1 == ball.X && user.Y + 1 == ball.Y)
                    return ComeDirection.UP_LEFT;
                else if (user.X == ball.X && user.Y + 1 == ball.Y)
                    return ComeDirection.UP;
                else if (user.X - 1 == ball.X && user.Y + 1 == ball.Y)
                    return ComeDirection.UP_RIGHT;
                else if (user.X - 1 == ball.X && user.Y == ball.Y)
                    return ComeDirection.RIGHT;
                else if (user.X - 1 == ball.X && user.Y - 1 == ball.Y)
                    return ComeDirection.DOWN_RIGHT;
                else
                    return ComeDirection.NULL;
            }
            catch
            {
                return ComeDirection.NULL;
            }
        }

        internal static ComeDirection GetUserComeDirection(RoomUser User)
        {
            if (User.X == User.SetX && User.Y < User.SetY)
                return ComeDirection.UP;
            else if (User.X > User.SetX && User.Y < User.SetY)
                return ComeDirection.UP_RIGHT;
            else if (User.X > User.SetX && User.Y == User.SetY)
                return ComeDirection.RIGHT;
            else if (User.X > User.SetX && User.Y > User.SetY)
                return ComeDirection.DOWN_RIGHT;
            else if (User.X == User.SetX && User.Y > User.SetY)
                return ComeDirection.DOWN;
            else if (User.X < User.SetX && User.Y > User.SetY)
                return ComeDirection.DOWN_LEFT;
            else if (User.X < User.SetX && User.Y == User.SetY)
                return ComeDirection.LEFT;
            else if (User.X <= User.SetX && User.Y < User.SetY)
                return ComeDirection.UP_LEFT;
            else
                return ComeDirection.NULL;
        }
    }
}
