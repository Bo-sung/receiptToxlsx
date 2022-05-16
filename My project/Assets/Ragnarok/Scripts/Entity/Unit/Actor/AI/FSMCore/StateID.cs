namespace Ragnarok.AI
{
    /// <summary>
    /// <see cref="CharacterAI"/>
    /// <see cref="MonsterAI"/>
    /// <seealso cref="Transition"/>
    /// </summary>
    public enum StateID
    {
        /// <summary>
        /// 전투 대기
        /// <see cref="ReadyState"/>
        /// </summary>
        Ready = 1,

        /// <summary>
        /// 기본 자세
        /// <see cref="IdleState"/>
        /// </summary>
        Idle,

        /// <summary>
        /// 순찰
        /// <see cref="PatrolState"/>
        /// </summary>
        Patrol,

        /// <summary>
        /// 근처 조사: 타겟을 놓쳤(죽었)을 경우 근처에 타겟이 있는지 둘러본다.
        /// <see cref="InvestigateState"/>
        /// </summary>
        Investigate,

        /// <summary>
        /// 추적: 타겟을 쫒아간다
        /// <see cref="ChaseState"/>
        /// </summary>
        Chase,

        /// <summary>
        /// 회귀: 원래 자리로 되돌아간다.
        /// <see cref="ReturnState"/>
        /// </summary>
        Return,

        /// <summary>
        /// 움직임: 특정 자리로 움직인다.
        /// <see cref="MoveState"/>
        /// </summary>
        Move,

        /// <summary>
        /// 도망: 피격받은 반대방향으로 피신한다
        /// <see cref="EvadeState"/>
        /// </summary>
        Evade,

        /// <summary>
        /// 스킬
        /// <see cref="SkillState"/>
        /// </summary>
        Skill,

        /// <summary>
        /// 피격
        /// <see cref="AttackerTargetingState"/>
        /// </summary>
        Hit,

        /// <summary>
        /// 죽음
        /// <see cref="DieState"/>
        /// </summary>
        Die,

        /// <summary>
        /// 부활
        /// <see cref="RebirthState"/>
        /// </summary>
        Rebirth,

        /// <summary>
        /// 승리
        /// <see cref="VictoryState"/>
        /// </summary>
        Victory,

        /// <summary>
        /// 바인딩 된 타겟 쫓아다님
        /// <see cref="FollowBindingTargetState"/>
        /// </summary>
        FollowBindingTarget,

        /// <summary>
        /// 리스폰 시간 대기
        /// <see cref="CupetRespawnState"/>
        /// </summary>
        Respawn,

        /// <summary>
        /// 타겟에 부딪힘
        /// <see cref="CollisionTargetState"/>
        /// </summary>
        CollisionTarget,

        /// <summary>
        /// 전투 매칭
        /// </summary>
        Match,

        /// <summary>
        /// 고정
        /// <see cref="PassiveHoldState"/>
        /// </summary>
        PassiveHold,

        /// <summary>
        /// 무방비 상태
        /// </summary>
        Defenseless,

        /// <summary>
        /// 분노 연출
        /// <see cref="AngerState">
        /// </summary>
        Anger,

        /// <summary>
        /// 종료
        /// <see cref="EndState"/>
        /// </summary>
        End,

#if UNITY_EDITOR
        /// <summary>
        /// 테스트
        /// </summary>
        Test,
#endif
    }
}