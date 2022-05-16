namespace Ragnarok
{
    public struct BattleStatus
    {
        public readonly int value;
        public readonly int perValue;

        public BattleStatus(int value, int perValue)
        {
            this.value = value;
            this.perValue = perValue;
        }

        public int GetStatusValue()
        {
            return GetStatusValue(basicValue: 0);
        }

        public int GetStatusValue(int basicValue, bool isLong = false)
        {
            // MaxHp의 경우 int.MaxValue값을 넘어선다
            if (isLong)
            {
                return (int)MathUtils.ToLong((basicValue + value) * (1 + MathUtils.ToPermyriadValue(perValue)));
            }

            return MathUtils.ToInt((basicValue + value) * (1 + MathUtils.ToPermyriadValue(perValue)));
        }        

#if UNITY_EDITOR
        public string GetStatusValueText(int basicValue = 0)
        {
            return $"({basicValue} + {value}) x (1 + {perValue} x 0.0001)";
        }

        public override string ToString()
        {
#if SHOW_PERCENTAGE_LOG
            return $"{value}, {MathUtils.ToPermyriadText(perValue)}";
#else
            return $"{value}, {MathUtils.ToPermyriadValue(perValue)}";
#endif
        }
#endif

        public static BattleStatus operator +(BattleStatus a, BattleStatus b)
        {
            int value = a.value + b.value;
            int perValue = a.perValue + b.perValue;
            return new BattleStatus(value, perValue);
        }
    }
}