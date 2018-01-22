using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Pathfinding
{
    class PathFinder
    {
        public static List<Vector2D> FindPath(bool Diag, Gamemap Map, Vector2D Start, Vector2D End, RoomUser User)
        {
            List<Vector2D> Path = new List<Vector2D>();

            PathFinderNode Nodes = FindPathReversed(Diag, Map, Start, End, User);

            if (Nodes != null)
            {
                Path.Add(End);

                while (Nodes.Next != null)
                {
                    Path.Add(Nodes.Next.Position);
                    Nodes = Nodes.Next;
                }
            }

            return Path;
        }

        internal static int CalculateRotation(int X1, int Y1, int X2, int Y2)
        {
            int dX = X2 - X1;
            int dY = Y2 - Y1;

            double d = Math.Atan2(dY, dX) * 180 / Math.PI;
            return ((int)d + 90) / 45;
        }

        public static PathFinderNode FindPathReversed(bool Diag, Gamemap Map, Vector2D Start, Vector2D End, RoomUser User)
        {
            MinHeap<PathFinderNode> OpenList = new MinHeap<PathFinderNode>();

            PathFinderNode[,] PfMap = new PathFinderNode[Map.Model.MapSizeX, Map.Model.MapSizeY];
            PathFinderNode Node;
            Vector2D Tmp;
            int Cost;
            int Diff;

            PathFinderNode Current = new PathFinderNode(Start);
            Current.Cost = 0;

            PathFinderNode Finish = new PathFinderNode(End);

            PfMap[Current.Position.X, Current.Position.Y] = Current;
            OpenList.Add(Current);

            while (OpenList.Count > 0)
            {
                Current = OpenList.ExtractFirst();
                Current.InClosed = true;

                for (int i = 0; Diag ? i < DiagMovePoints.Length : i < NoDiagMovePoints.Length; i++)
                {
                    Tmp = Current.Position + (Diag ? DiagMovePoints[i] : NoDiagMovePoints[i]);
                    bool IsFinalMove = (Tmp.X == End.X && Tmp.Y == End.Y);
                    bool DiagMove = (i == 0 || i == 2 || i == 4 || i == 6);

                    if (Map.IsValidStep(new Vector2D(Current.Position.X, Current.Position.Y), Tmp, IsFinalMove, User, true, DiagMove))
                    {
                        if (PfMap[Tmp.X, Tmp.Y] == null)
                        {
                            Node = new PathFinderNode(Tmp);
                            PfMap[Tmp.X, Tmp.Y] = Node;
                        }
                        else
                        {
                            Node = PfMap[Tmp.X, Tmp.Y];
                        }

                        if (!Node.InClosed)
                        {
                            Diff = 0;

                            if (Current.Position.X != Node.Position.X)
                            {
                                Diff += 1;
                            }

                            if (Current.Position.Y != Node.Position.Y)
                            {
                                Diff += 1;
                            }

                            Cost = Current.Cost + Diff + Node.Position.GetDistanceSquared(End);

                            if (Cost < Node.Cost)
                            {
                                Node.Cost = Cost;
                                Node.Next = Current;
                            }

                            if (!Node.InOpen)
                            {
                                if (Node.Equals(Finish))
                                {
                                    Node.Next = Current;
                                    return Node;
                                }

                                Node.InOpen = true;
                                OpenList.Add(Node);
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static Vector2D[] DiagMovePoints = new Vector2D[]
        {
            new Vector2D(-1, -1),
            new Vector2D(0, -1),
            new Vector2D(1, -1),
            new Vector2D(1, 0),
            new Vector2D(1, 1),
            new Vector2D(0, 1),
            new Vector2D(-1, 1),
            new Vector2D(-1, 0)
        };

        public static Point[] TilesArround = new Point[]
        {
            new Point(-1,-1),
            new Point(0, -1),
            new Point(1, -1),
            new Point(1, 0),
            new Point(1, 1),
            new Point(0, 1),
            new Point(-1, 1),
            new Point(-1, 0)
        };

        public static Vector2D[] NoDiagMovePoints = new Vector2D[]
        {
            new Vector2D(0, -1),
            new Vector2D(1, 0),
            new Vector2D(0, 1),
            new Vector2D(-1, 0)
        };
    }
}
