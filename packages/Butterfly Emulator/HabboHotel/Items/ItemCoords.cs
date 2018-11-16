using Butterfly.HabboHotel.Pathfinding;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Items
{
    class ItemCoords
    {
        /*
         * Al meter el primer movimiento de furni, repite la estructura, no falla, pero bueno...
         */

        internal static void ModifyGamemapTiles(Room room, Dictionary<int, ThreeDCoord> AffectedTiles)
        {
            ServerMessage Message = new ServerMessage(Outgoing.SendCoordsOfItemPacket);
            Message.AppendByted(AffectedTiles.Count);
            foreach (ThreeDCoord tile in AffectedTiles.Values)
            {
                Message.AppendByted(tile.X);
                Message.AppendByted(tile.Y);
                Message.AppendShort((int)(room.GetGameMap().SqAbsoluteHeightGameMap(tile.X, tile.Y) * 256));
            }
            room.SendMessage(Message);
        }

        internal static void ModifyGamemapTiles(Room room, Dictionary<int, ThreeDCoord> AffectedTiles, double Height)
        {
            ServerMessage Message = new ServerMessage(Outgoing.SendCoordsOfItemPacket);
            Message.AppendByted(AffectedTiles.Count);
            foreach (ThreeDCoord tile in AffectedTiles.Values)
            {
                Message.AppendByted(tile.X);
                Message.AppendByted(tile.Y);
                Message.AppendShort((int)(Height * 256));
            }
            room.SendMessage(Message);
        }

        internal static void ModifyGamemapTiles(Room room, List<Point> AffectedTiles)
        {
            ServerMessage Message = new ServerMessage(Outgoing.SendCoordsOfItemPacket);
            Message.AppendByted(AffectedTiles.Count);
            foreach (Point tile in AffectedTiles)
            {
                Message.AppendByted(tile.X);
                Message.AppendByted(tile.Y);
                Message.AppendShort((int)(room.GetGameMap().SqAbsoluteHeightGameMap(tile.X, tile.Y) * 256));
            }
            room.SendMessage(Message);
        }

        internal static void ModifyGamemapTiles(Room room, Dictionary<int, ThreeDCoord> AffectedTiles, Dictionary<int, ThreeDCoord> OldAffectedTiles)
        {
            ServerMessage Message = new ServerMessage(Outgoing.SendCoordsOfItemPacket);
            Message.AppendByted(AffectedTiles.Count + OldAffectedTiles.Count);
            foreach (ThreeDCoord tile in OldAffectedTiles.Values)
            {
                Message.AppendByted(tile.X);
                Message.AppendByted(tile.Y);
                Message.AppendShort((int)(room.GetGameMap().SqAbsoluteHeightGameMap(tile.X, tile.Y) * 256));
            }
            foreach (ThreeDCoord tile in AffectedTiles.Values)
            {
                Message.AppendByted(tile.X);
                Message.AppendByted(tile.Y);
                Message.AppendShort((int)(room.GetGameMap().SqAbsoluteHeightGameMap(tile.X, tile.Y) * 256));
            }
            room.SendMessage(Message);
        }
    }
}
