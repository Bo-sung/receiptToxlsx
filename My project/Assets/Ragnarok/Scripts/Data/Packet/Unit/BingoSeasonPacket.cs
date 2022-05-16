using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class BingoSeasonPacket : IPacket<Response>
    {
        public int group;
        public RemainTime seasonStartTime;
        public RemainTime seasonEndTime;

        public void Initialize(Response t)
        {
            group = t.GetInt("1");
            seasonStartTime = t.GetLong("2");
            seasonEndTime = t.GetLong("3");
        }
    }
}
