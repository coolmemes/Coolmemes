using System;
using System.Drawing;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class MovementManagement
    {
        private readonly Random rnd;

        public MovementManagement()
        {
            rnd = new Random();
        }

        private static void HandleMovement(ref Point coordinate, MovementState state)
        {
            switch (state)
            {
                case MovementState.down:
                    {
                        coordinate.Y++;
                        break;
                    }

                case MovementState.up:
                    {
                        coordinate.Y--;
                        break;
                    }

                case MovementState.left:
                    {
                        coordinate.X--;
                        break;
                    }

                case MovementState.right:
                    {
                        coordinate.X++;
                        break;
                    }
            }
        }

        private static void HandleMovementDir(ref Point coordinate, MovementDirection state)
        {
            switch (state)
            {
                case MovementDirection.down:
                    {
                        coordinate.Y++;
                        break;
                    }

                case MovementDirection.up:
                    {
                        coordinate.Y--;
                        break;
                    }

                case MovementDirection.left:
                    {
                        coordinate.X--;
                        break;
                    }

                case MovementDirection.right:
                    {
                        coordinate.X++;
                        break;
                    }

                case MovementDirection.downright:
                    {
                        coordinate.X++;
                        coordinate.Y++;
                        break;
                    }

                case MovementDirection.downleft:
                    {
                        coordinate.X--;
                        coordinate.Y++;
                        break;
                    }

                case MovementDirection.upright:
                    {
                        coordinate.X++;
                        coordinate.Y--;
                        break;
                    }

                case MovementDirection.upleft:
                    {
                        coordinate.X--;
                        coordinate.Y--;
                        break;
                    }
            }
        }

        protected Point HandleMovement(Point newCoordinate, MovementState state, int newRotation)
        {
            var newPoint = new Point(newCoordinate.X, newCoordinate.Y);

            switch (state)
            {
                case MovementState.up:
                case MovementState.down:
                case MovementState.left:
                case MovementState.right:
                    {
                        HandleMovement(ref newPoint, state);
                        break;
                    }

                case MovementState.leftright:
                    {
                        if (rnd.Next(0, 2) == 1)
                        {
                            HandleMovement(ref newPoint, MovementState.left);
                        }
                        else
                        {
                            HandleMovement(ref newPoint, MovementState.right);
                        }
                        break;
                    }

                case MovementState.updown:
                    {
                        if (rnd.Next(0, 2) == 1)
                        {
                            HandleMovement(ref newPoint, MovementState.up);
                        }
                        else
                        {
                            HandleMovement(ref newPoint, MovementState.down);
                        }
                        break;
                    }

                case MovementState.random:
                    {
                        switch (rnd.Next(1, 5))
                        {
                            case 1:
                                {
                                    HandleMovement(ref newPoint, MovementState.up);
                                    break;
                                }
                            case 2:
                                {
                                    HandleMovement(ref newPoint, MovementState.down);
                                    break;
                                }

                            case 3:
                                {
                                    HandleMovement(ref newPoint, MovementState.left);
                                    break;
                                }
                            case 4:
                                {
                                    HandleMovement(ref newPoint, MovementState.right);
                                    break;
                                }
                        }
                        break;
                    }
            }

            return newPoint;
        }

        protected Point HandleMovementDir(Point newCoordinate, MovementDirection state, int newRotation)
        {
            var newPoint = new Point(newCoordinate.X, newCoordinate.Y);

            switch (state)
            {
                case MovementDirection.up:
                case MovementDirection.down:
                case MovementDirection.left:
                case MovementDirection.right:
                case MovementDirection.downright:
                case MovementDirection.downleft:
                case MovementDirection.upright:
                case MovementDirection.upleft:
                    {
                        HandleMovementDir(ref newPoint, state);
                        break;
                    }

                case MovementDirection.random:
                    {
                        switch (rnd.Next(1, 5))
                        {
                            case 1:
                                {
                                    HandleMovementDir(ref newPoint, MovementDirection.up);
                                    break;
                                }
                            case 2:
                                {
                                    HandleMovementDir(ref newPoint, MovementDirection.down);
                                    break;
                                }

                            case 3:
                                {
                                    HandleMovementDir(ref newPoint, MovementDirection.left);
                                    break;
                                }
                            case 4:
                                {
                                    HandleMovementDir(ref newPoint, MovementDirection.right);
                                    break;
                                }
                        }
                        break;
                    }
            }

            return newPoint;
        }

        protected int HandleRotation(int oldRotation, RotationState state)
        {
            var rotation = oldRotation;
            switch (state)
            {
                case RotationState.clocwise:
                    {
                        HandleClockwiseRotation(ref rotation);
                        return rotation;
                    }

                case RotationState.counterClockwise:
                    {
                        HandleCounterClockwiseRotation(ref rotation);
                        return rotation;
                    }

                case RotationState.random:
                    {
                        if (rnd.Next(0, 3) == 1)
                        {
                            HandleClockwiseRotation(ref rotation);
                        }
                        else
                        {
                            HandleCounterClockwiseRotation(ref rotation);
                        }
                        return rotation;
                    }
            }

            return rotation;
        }

        private static void HandleClockwiseRotation(ref int rotation)
        {
            rotation = rotation + 2;
            if (rotation > 6)
                rotation = 0;
        }

        private static void HandleCounterClockwiseRotation(ref int rotation)
        {
            rotation = rotation - 2;
            if (rotation < 0)
                rotation = 6;
        }
    }
}
