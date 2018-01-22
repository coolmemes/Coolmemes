using System;

namespace WiredSolver
{
    [Flags]
    public enum WireCurrentTransfer
    {
        NONE = 0,
        UP = 1,
        RIGHT = 2,
        DOWN = 4,
        LEFT = 8

    }
}
