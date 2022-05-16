namespace Ragnarok
{
    public class MonsterInfo : DataInfo<MonsterData>, UIMonsterIcon.IInput
    {
        /// <summary>
        /// 몬스터 이름
        /// </summary>
        public string Name => data.name_id.ToText();

        /// <summary>
        /// 몬스터 아이콘 이름
        /// </summary>
        public virtual string IconName => data.icon_name;

        /// <summary>
        /// 속성 아이콘 이름
        /// </summary>
        public string ElementIconName => data.element_type.ToEnum<ElementType>().GetIconName();

        /// <summary>
        /// 몬스터 크기
        /// </summary>
        public UnitSizeType UnitSizeType => data.cost.ToUnitSizeType();

        public int ID => data.id;
        public MonsterData Data => data;

        // 미로 몬스터용 데이터
        public bool IsBoss { get; private set; }
        public int Level { get; private set; }

        public MonsterInfo(bool isBoss)
        {
            IsBoss = isBoss;
        }

        public void SetLevel(int level)
        {
            Level = level;
        }
    }
}