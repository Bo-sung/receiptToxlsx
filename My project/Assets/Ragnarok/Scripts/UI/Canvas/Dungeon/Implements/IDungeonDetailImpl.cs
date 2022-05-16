namespace Ragnarok
{
    public interface IDungeonDetailImpl : IDungeonImpl
    {
        /// <summary>
        /// 던전 오픈 여부
        /// </summary>
        bool IsOpend(DungeonType dungeonType, int id, bool isShowNotice);

        /// <summary>
        /// 던전 클리어 여부 (소탕을 하려면 클리어 여부를 알아야 한다)
        /// </summary>
        bool IsCleared(DungeonType dungeonType, int id);

        /// <summary>
        /// 클리어한 던전 난이도
        /// </summary>
        int GetClearedDifficulty(DungeonType dungeonType);

        /// <summary>
        /// 던전 입장 가능 여부
        /// </summary>
        bool CanEnter(DungeonType dungeonType, int id, bool isShowPopup);

        /// <summary>
        /// 소탕권 보유 수
        /// </summary>
        int GetClearTicketCount(DungeonType dungeonType);

        /// <summary>
        /// 몬스터 정보
        /// </summary>
        UIMonsterIcon.IInput[] GetMonsterInfos(DungeonType dungeonType, (int monsterId, MonsterType type, int monsterLevel)[] monsterInfos);
    }
}