namespace Ragnarok.AI
{
    /// <summary>
    /// <see cref="CharacterAI"/>
    /// <see cref="MonsterAI"/>
    /// <seealso cref="StateID"/>
    /// </summary>
    public enum Transition
    {
        /// <summary>
        /// 전투 시작
        /// </summary>
        StartBattle = 1,

        /// <summary>
        /// 전투 종료
        /// </summary>
        EndBattle,

        /// <summary>
        /// 현재 상태 종료
        /// </summary>
        Finished,

        /// <summary>
        /// 유닛 선택 (외부 입력)
        /// </summary>
        SelectedUnit,

        /// <summary>
        /// 큐브 선택 (외부 입력)
        /// </summary>
        SelectedCube,

        /// <summary>
        /// 스킬 선택 (외부 입력)
        /// </summary>
        SelectedSkill,

        /// <summary>
        /// 외부 입력 스킬 초기화
        /// </summary>
        ResetInputSkill,

        /// <summary>
        /// 타겟 발견
        /// </summary>
        SawTarget,

        /// <summary>
        /// 감지 당함
        /// </summary>
        BeDetected,

        /// <summary>
        /// 피격
        /// </summary>
        BeAttacked,

        /// <summary>
        /// 타겟 놓침 (죽었을 경우)
        /// </summary>
        LostTarget,

        /// <summary>
        /// 스킬 사용
        /// </summary>
        UseSkill,

        /// <summary>
        /// 타겟 없음
        /// </summary>
        EmptyTarget,

        /// <summary>
        /// 죽음
        /// </summary>
        Dead,

        /// <summary>
        /// 도망
        /// </summary>
        Evade,
        
        /// <summary>
        /// 부활
        /// </summary>
        Rebirth,

        /// <summary>
        /// 타겟 바인딩 변경
        /// </summary>
        ChangeBindingTarget,

        MoveAround, // 배회

        /// <summary>
        /// 바인딩 된 타겟이 움직임
        /// </summary>
        MovedBindingTarget,

        /// <summary>
        /// 리스폰 시간 대기
        /// </summary>
        Respawn,

        /// <summary>
        /// 전투 매치
        /// </summary>
        Match,

        /// <summary>
        /// 고정
        /// </summary>
        Hold,

        /// <summary>
        /// 전투불능 시작
        /// </summary>
        Groggy,

        /// <summary>
        /// 분노 상태 도달
        /// </summary>
        BeAngry,

        /// <summary>
        /// 관전 상태
        /// </summary>
        Observe,
    }
}