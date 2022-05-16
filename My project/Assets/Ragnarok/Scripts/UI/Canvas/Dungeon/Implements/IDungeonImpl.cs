namespace Ragnarok
{
    public interface IDungeonImpl
    {
        /// <summary>
        /// 던전 오픈 여부 (대문)
        /// </summary>
        bool IsOpend(DungeonType dungeonType);

        /// <summary>
        /// 던전 오픈 조건 텍스트 반환
        /// </summary>
        string GetOpenConditionalSimpleText(IOpenConditional openConditional);

        /// <summary>
        /// 무료 입장 횟수
        /// </summary>
        int GetFreeEntryCount(DungeonType dungeonType);

        /// <summary>
        /// 무료 입장 최대 횟수
        /// </summary>
        int GetFreeEntryMaxCount(DungeonType dungeonType);

        /// <summary>
        /// 현재 던전 플레이 진행 여부
        /// </summary>
        bool IsCurrentDungeonPlaying();

        /// <summary>
        /// 던전 무료아이템 수령가능 여부
        /// </summary>
        bool PossibleFreeReward(DungeonType dungeonType);
    }
}