using System;

namespace Ragnarok
{
    [Flags]
    public enum NavMeshArea
    {
        All = -1,
        None = 0,

        Walkable = 1 << 0, // 1
        NotWalkable = 1 << 1, // 2
        Jump = 1 << 2, // 3
        Door =  1 << 3, // 4
    }
}
