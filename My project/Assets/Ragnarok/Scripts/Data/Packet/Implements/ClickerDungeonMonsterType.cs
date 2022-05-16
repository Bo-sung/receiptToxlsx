namespace Ragnarok
{
    ///<see cref="ClickerDungeonData"/>
    public enum ClickerDungeonMonsterType
    {
        /// <summary> 기본 </summary>
        NONE = 0,

        /// <summary> 큐브 파괴 시 몬스터 생성 (충돌 시 캐릭터 경직) </summary>
        FREEZE = 1,

        /// <summary> 큐브 파괴 시 몬스터 생성 (충돌 시 캐릭터 즉사) </summary>
        DIE = 2,
    }
}