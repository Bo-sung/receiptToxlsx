using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class AgentSlotInfoPacket : IPacket<Response>
    {
        public long SlotNo { get; private set; }
        public int AgentID { get; private set; }
        public int OrderID { get; private set; }

        public bool IsUsingSlot { get { return AgentID > 0; } }

        public void Initialize(Response t)
        {
            SlotNo = t.GetLong("1");
            AgentID = t.GetInt("2");
            OrderID = t.GetInt("3");
        }
    }
}
