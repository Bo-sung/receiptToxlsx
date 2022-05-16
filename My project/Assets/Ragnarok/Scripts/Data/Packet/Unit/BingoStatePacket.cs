using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class BingoStatePacket : IPacket<Response>
    {
        public string bingoState;
        public int questClearCount;
        public int curQuestNumber;
        public int curQuestProgress;

        public bool GetIsChecked(int x, int y)
        {
            return bingoState[x + y * 5] == '1';
        }

        public bool GetIsRewarded(int x, int y)
        {
            if (y > 0)
                return bingoState[26 + 6 + y] == '1';
            else
                return bingoState[26 + x] == '1';
        }

        public void Initialize(Response t)
        {
            bingoState = t.GetUtfString("1");
            questClearCount = t.GetByte("2");
            curQuestNumber = t.GetInt("3");
            curQuestProgress = t.GetInt("4");
        }
    }
}
