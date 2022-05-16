using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;

namespace Ragnarok
{
    public class ExtraBattleOptions : IEqualityComparer<ExtraBattleOptionType>
    {
        private readonly Dictionary<ExtraBattleOptionType, ObscuredInt> dic;

        public ExtraBattleOptions()
        {
            dic = new Dictionary<ExtraBattleOptionType, ObscuredInt>(this);
        }

        public void Clear()
        {
            dic.Clear();
        }

        public bool HasType(ExtraBattleOptionType type)
        {
            return dic.ContainsKey(type);
        }

        public void Set(ExtraBattleOptionType type, int value)
        {
            if (HasType(type))
            {
                dic[type] = value;
            }
            else
            {
                dic.Add(type, value);
            }
        }

        public int Get(ExtraBattleOptionType type)
        {
            if (HasType(type))
                return dic[type];

            return 0;
        }

        public bool Reset(ExtraBattleOptionType type)
        {
            return dic.Remove(type);
        }

        bool IEqualityComparer<ExtraBattleOptionType>.Equals(ExtraBattleOptionType x, ExtraBattleOptionType y)
        {
            return x == y;
        }

        int IEqualityComparer<ExtraBattleOptionType>.GetHashCode(ExtraBattleOptionType obj)
        {
            return obj.GetHashCode();
        }
    }
}