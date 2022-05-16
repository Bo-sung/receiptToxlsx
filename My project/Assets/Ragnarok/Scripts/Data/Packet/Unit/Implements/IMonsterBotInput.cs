namespace Ragnarok
{
    public interface IMonsterBotInput : ISpawnMonster
    {
        /// <summary>
        /// 몬스터 고유 인덱스 (서버)
        /// </summary>
        int Index { get; }

        /// <summary>
        /// 몬스터 상태
        /// <see cref="MonsterBotState"/>
        /// </summary>
        byte State { get; }

        float PosX { get; }
        float PosY { get; }
        float PosZ { get; }

        bool HasTargetPos { get; }
        /// <summary>
        /// HasTargetPos 가 존재할 경우에만 사용 (Pos ~ TargetPos 값을 사이의 값을 구하기 위해 필요)
        /// </summary>
        float SavedTargetingTime { get; }
        float TargetPosX { get; }
        float TargetPosY { get; }
        float TargetPosZ { get; }

        bool HasMaxHp { get; }
        int MaxHp { get; }
        bool HasCurHp { get; }
        int CurHp { get; }

        CrowdControlType CrowdControl { get; }
        float? MoveSpeed { get; }
    }
}