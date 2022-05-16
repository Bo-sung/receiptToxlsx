using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class BattleUnitInfo : BattleUnitInfo.IValue, BattleUnitInfo.ITargetValue
    {
        public struct Settings
        {
            public int id; // 데이터 아이디 (캐릭터: JobData_Id, 큐펫: CupetData_Id, 몬스터: MonsterData_Id)
            public int level;
            public ElementType unitElementType;
            public int unitElementLevel;
            public UnitSizeType unitSizeType;
            public MonsterType unitMonsterType;
            public float cognizanceDistance;
        }

        public interface IValue
        {
            int Level { get; }
        }

        public interface ITargetValue
        {
            int Id { get; }
            int Level { get; }
            ElementType UnitElementType { get; }
            int UnitElementLevel { get; }
            UnitSizeType UnitSizeType { get; }
            MonsterType UnitMonsterType { get; }
        }

        private ObscuredInt id;
        private ObscuredInt level;
        private ObscuredInt unitElementType;
        private ObscuredInt unitElementLevel;
        private ObscuredInt unitSizeType;
        private ObscuredInt unitMonsterType;
        private ObscuredFloat cognizanceDistance;

        /// <summary>
        /// 데이터 아이디 (캐릭터: JobData_Id, 큐펫: CupetData_Id, 몬스터: MonsterData_Id)
        /// 서버로 아이템 드랍 프로토콜을 보내기 위해 필요
        /// </summary>
        public int Id => id;

        /// <summary>
        /// 레벨
        /// </summary>
        public int Level => level;

        /// <summary>
        /// 유닛 ElementType
        /// </summary>
        public ElementType UnitElementType => unitElementType.ToEnum<ElementType>();

        /// <summary>
        /// 유닛 속성 레벨
        /// </summary>
        public int UnitElementLevel => unitElementLevel;

        /// <summary>
        /// 유닛 SizeType
        /// </summary>
        public UnitSizeType UnitSizeType => unitSizeType.ToEnum<UnitSizeType>();

        /// <summary>
        /// 유닛 MonsterType
        /// </summary>
        public MonsterType UnitMonsterType => unitMonsterType.ToEnum<MonsterType>();

        /// <summary>
        /// 타겟 인지 거리
        /// </summary>
        public float CognizanceDistance => cognizanceDistance;

        /// <summary>
        /// 정보 리셋
        /// </summary>
        public void Clear()
        {
            id = 0;
            level = 0;
            SetUnitElementType(default);
            SetUnitElementLevel(0);
            SetUnitSizeType(default);
            SetUnitMonsterType(default);
            cognizanceDistance = 0f;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Settings settings)
        {
            Clear(); // 리셋

            id = settings.id; // 유닛 데이터 고유 아이디
            level = settings.level; // 레벨
            SetUnitElementType(settings.unitElementType); // 유닛 ElementType
            SetUnitElementLevel(settings.unitElementLevel); // 유닛 속성 레벨
            SetUnitSizeType(settings.unitSizeType); // 유닛 SizeType
            SetUnitMonsterType(settings.unitMonsterType); // 유닛 MonsterType
            cognizanceDistance = settings.cognizanceDistance; // 타겟 인지 거리
        }

        private void SetUnitElementType(ElementType unitElementType)
        {
            this.unitElementType = (int)unitElementType;
        }

        private void SetUnitElementLevel(int unitElementLevel)
        {
            this.unitElementLevel = unitElementLevel;
        }

        private void SetUnitSizeType(UnitSizeType unitSizeType)
        {
            this.unitSizeType = (int)unitSizeType;
        }

        private void SetUnitMonsterType(MonsterType unitMonsterType)
        {
            this.unitMonsterType = (int)unitMonsterType;
        }
    }
}