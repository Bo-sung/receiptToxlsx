#if UNITY_EDITOR

using System.Collections.Generic;

namespace Ragnarok
{
    public class EntityLog : List<string>
    {
        public readonly int tick;
        public string header;
        public bool isFinished;

        public EntityLog(int tick, string header)
        {
            this.tick = tick;
            this.header = header;
        }

        public void Finish()
        {
            isFinished = true;
        }
    }
}
#endif