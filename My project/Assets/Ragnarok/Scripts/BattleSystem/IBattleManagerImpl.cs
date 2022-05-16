namespace Ragnarok
{
    /// <summary>
    /// <see cref="BattleManager"/>
    /// </summary>
    public interface IBattleManagerImpl
    {
        /// <summary>
        /// 유닛 존재 여부
        /// </summary>
        bool IsExists(UnitEntity unit);

        /// <summary>
        /// 유닛 등록
        /// </summary>
        bool Add(UnitEntity unit, bool isEnemy);

        /// <summary>
        /// 유닛 해제
        /// </summary>
        bool Remove(UnitEntity unit);

        /// <summary>
        /// 모든 아군 죽음 여부
        /// </summary>
        bool IsAllAlliesDead();

        /// <summary>
        /// 모든 적 죽음 여부
        /// </summary>
        bool IsAllEnemyDead();

        /// <summary>
        /// 플레이어 죽음 여부
        /// </summary>
        bool IsPlayerDead();

        /// <summary>
        /// 특정 타입 죽음 여부
        /// </summary>
        bool IsDead(UnitEntityType type);

        /// <summary>
        /// 적군 유닛 리셋
        /// </summary>
        void ResetEnemyUnitList();

        /// <summary>
        /// 모든 유닛 리셋
        /// </summary>
        void ResetUnitList();

        /// <summary>
        /// 전투 모드 세팅
        /// </summary>
        void SetBattleEntry(BattleEntry battleEntry);

        void InvokePreStart();
        void InvokeStart();
        void InvokeReady();
        void InvokeEnd();

        void ReadyAllUnit();
        void PauseAllUnit();
        void EndAllUnit();

        /// <summary>
        /// 전투 시작
        /// </summary>
        void StartBattle(BattleMode mode, bool isStartFadeIn = true);

        /// <summary>
        /// 전투 시작
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="id"></param>
        void StartBattle(BattleMode mode, int id, bool isStartFadeIn = true);

        /// <summary>
        /// 전투 시작
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="id"></param>
        void StartBattle(BattleMode mode, IBattleInput battleInput, bool isStartFadeIn = true);

        /// <summary>
        /// 현재 전투 나가기
        /// </summary>
        void ExitBattle();

        BattleMode GetSaveBattleMode();

        bool IsPlayingFadeIn();
    }
}