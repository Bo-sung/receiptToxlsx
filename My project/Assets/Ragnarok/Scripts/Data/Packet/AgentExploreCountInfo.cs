using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class AgentExploreCountInfo : IPacket<Response>
    {
        public int StageID { get; private set; }
        public int remainTradeCount;
        public bool IsViewAd { get; private set; } // 0 : 오늘 광고 안봄, 1 : 오늘 공고 봄

        public void Initialize(Response t)
        {
            StageID = t.GetInt("1");
            remainTradeCount = t.GetInt("2");
            IsViewAd = t.GetInt("3") == 1;
        }

        public AgentExploreCountInfo() { }

        public AgentExploreCountInfo(int stageID, int remainTradeCount)
        {
            StageID = stageID;
            this.remainTradeCount = remainTradeCount;
        }

        public void SetIsViewAd(bool isViewAd)
        {
            IsViewAd = isViewAd;
        }

        public override string ToString()
        {
            return $"ExploreCountInfo StageID : {StageID}, RemainTradeCount : {remainTradeCount}, IsViewAd : {IsViewAd}";
        }
    }
}
