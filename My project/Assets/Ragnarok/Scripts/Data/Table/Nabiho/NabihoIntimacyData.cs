using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="NabihoIntimacyDataManager"/>
    /// </summary>
    public class NabihoIntimacyData :IData
    {
        public int Id { get; }
        public int IntimacyLevel { get; }
        public int TotalNeedExp { get; private set; }
        public int ReduceMinute { get; }

        /// <summary>
        /// 이전 필요 레벨 (클라 전용)
        /// </summary>
        public int PreTotalNeedExp { get; private set; }

        public NabihoIntimacyData(IList<MessagePackObject> data)
        {
            byte index = 0;
            Id            = data[index++].AsInt32();
            IntimacyLevel = data[index++].AsInt32();
            TotalNeedExp  = data[index++].AsInt32();
            ReduceMinute  = data[index++].AsInt32();
        }

        public void SetMaxLevel()
        {
            TotalNeedExp = int.MaxValue;
        }

        public void SetPreTotalNeedExp(int preTotalNeedExp)
        {
            PreTotalNeedExp = preTotalNeedExp;
        }
    }
}