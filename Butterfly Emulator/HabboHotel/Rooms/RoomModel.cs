using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms
{
    class RoomModel
    {
        public int DoorX;
        public int DoorY;
        public double DoorZ;
        public int DoorOrientation;

        public int MapSizeX;
        public int MapSizeY;

        public SquareState[,] SqState;
        public short[,] SqFloorHeight;

        public Point SquareInFrontDoor
        {
            get
            {
                var Sq = new Point(DoorX, DoorY);

                if (DoorOrientation == 0)
                {
                    Sq.Y--;
                }
                else if (DoorOrientation == 2)
                {
                    Sq.X++;
                }
                else if (DoorOrientation == 4)
                {
                    Sq.Y++;
                }
                else if (DoorOrientation == 6)
                {
                    Sq.X--;
                }

                return Sq;
            }
        }

        public RoomModel(int DoorX, int DoorY, double DoorZ, int DoorOrientation, string Heightmap)
        {
            this.DoorX = DoorX;
            this.DoorY = DoorY;
            this.DoorZ = DoorZ;
            this.DoorOrientation = DoorOrientation;

            Heightmap = Heightmap.Replace("\n", "");
            string[] mapLevels = Heightmap.Split('\r');

            this.MapSizeX = mapLevels[0].Length;
            this.MapSizeY = mapLevels.Length;

            this.SqState = new SquareState[MapSizeX, MapSizeY];
            this.SqFloorHeight = new short[MapSizeX, MapSizeY];

            for (int y = 0; y < MapSizeY; y++)
            {
                char[] lineArray = mapLevels[y].ToCharArray();

                for (int x = 0; x < MapSizeX; x++)
                {
                    if (DoorX == x && DoorY == y)
                    {
                        SqState[x, y] = SquareState.OPEN;
                        SqFloorHeight[x, y] = (short)DoorZ;
                    }
                    else if (lineArray[x] == 'x')
                    {
                        SqState[x, y] = SquareState.BLOCKED;
                    }
                    else
                    {
                        SqState[x, y] = SquareState.OPEN;
                        SqFloorHeight[x, y] = parse(lineArray[x]);
                    }
                }
            }
        }

        public static string getRandomSquare(string Heightmap)
        {

            Heightmap = Heightmap.Replace("\n", "");
            string[] mapLevels = Heightmap.Split('\r');

            var MapSizeX = mapLevels[0].Length;
            var MapSizeY = mapLevels.Length;

            for (int y = 0; y < MapSizeY; y++)
            {
                char[] lineArray = mapLevels[y].ToCharArray();

                for (int x = 0; x < MapSizeX; x++)
                {
                    if (lineArray[x] != 'x')
                    {
                        return x + "/" + y;
                    }
                }
            }
            return "";
        }

        public string SerializeStringHeightmap()
        {
            StringBuilder thatMessage = new StringBuilder();
            for (int y = 0; y < MapSizeY; y++)
            {
                for (int x = 0; x < MapSizeX; x++)
                {
                    if (x == DoorX && y == DoorY)
                        thatMessage.Append(GetValue((int)DoorZ));
                    else if (SqState[x, y] == SquareState.BLOCKED)
                        thatMessage.Append("x");
                    else
                        thatMessage.Append(GetValue(SqFloorHeight[x, y]));
                }

                thatMessage.Append(Convert.ToChar(13).ToString());
            }

            return thatMessage.ToString();
        }

        public ServerMessage SerializeRelativeHeightmap(int WallHeight)
        {
            StringBuilder thatMessage = new StringBuilder();

            ServerMessage Message = new ServerMessage(Outgoing.FloorHeightMapMessageParser);
            Message.AppendBoolean(false); // ??
            Message.AppendInt32(WallHeight);

            for (int y = 0; y < MapSizeY; y++)
            {
                for (int x = 0; x < MapSizeX; x++)
                {
                    if (x == DoorX && y == DoorY)
                        thatMessage.Append(GetValue((int)DoorZ));
                    else if (SqState[x, y] == SquareState.BLOCKED)
                        thatMessage.Append("x");
                    else
                        thatMessage.Append(GetValue(SqFloorHeight[x, y]));
                }

                thatMessage.Append(Convert.ToChar(13).ToString());
            }

            Message.AppendString(thatMessage.ToString());

            thatMessage.Clear();
            thatMessage = null;

            return Message;
        }

        public ServerMessage SerializeHeightmap(Gamemap map)
        {
            ServerMessage Message = new ServerMessage(Outgoing.HeightMapMessageParser);
            Message.AppendInt32(MapSizeX);
            Message.AppendInt32(MapSizeX * MapSizeY);
            for (int y = 0; y < MapSizeY; y++)
            {
                for (int x = 0; x < MapSizeX; x++)
                {
                    if (SqState[x, y] != SquareState.BLOCKED || (DoorX == x && DoorY == y))
                    {
                        Message.AppendShort((int)(map.SqAbsoluteHeightGameMap(x, y) * 256));
                    }
                    else
                    {
                        Message.AppendShort(0x4000);
                    }
                }
            }

            return Message;
        }

        public static short parse(char input, bool ePorta = false, GameClient Session = null)
        {
            switch (input)
            {
                case '0':
                    return 0;
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                case '5':
                    return 5;
                case '6':
                    return 6;
                case '7':
                    return 7;
                case '8':
                    return 8;
                case '9':
                    return 9;
                case 'a':
                    return 10;
                case 'b':
                    return 11;
                case 'c':
                    return 12;
                case 'd':
                    return 13;
                case 'e':
                    return 14;
                case 'f':
                    return 15;
                case 'g':
                    return 16;
                case 'h':
                    return 17;
                case 'i':
                    return 18;
                case 'j':
                    return 19;
                case 'k':
                    return 20;
                case 'l':
                    return 21;
                case 'm':
                    return 22;
                case 'n':
                    return 23;
                case 'o':
                    return 24;
                case 'p':
                    return 25;
                case 'q':
                    return 26;
                case 'r':
                    return 27;
                case 's':
                    return 28;
                case 't':
                    return 29;
                case 'u':
                    return 30;
                case 'v':
                    return 31;
                case 'w':
                    return 32;
                default:
                    if (ePorta)
                         return -1;
                     throw new FormatException("O floor só pode ter caracteres de 0 a 9 ou a porta está em um lugar inválido. [" + input + "]");
            }
        }

        private string GetValue(int val)
        {
            if (val < 10)
                return val.ToString();
            else
                return ((char)(87 + val)).ToString();
        }

        public void Destroy()
        {
            Array.Clear(SqState, 0, SqState.Length);
            Array.Clear(SqFloorHeight, 0, SqFloorHeight.Length);

            SqState = null;
            SqFloorHeight = null;
        }
    }

    public enum SquareState
    {
        OPEN = 0,
        BLOCKED = 1
    }
}