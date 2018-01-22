using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Games.BolasMovimento
{
    interface IMovimentos
    {
        void onUserwalk(RoomUser User, RoomItem ball);
        bool MoveBall(RoomItem item, RoomUser mover, int newX, int newY);
        void MoveBall(RoomItem item, GameClient client, Point user);
        void MoveBallProcess(RoomItem item);
    }
}
