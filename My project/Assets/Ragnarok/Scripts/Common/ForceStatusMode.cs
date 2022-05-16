namespace Ragnarok
{
    [System.Flags]
    public enum ForceStatusMode
    {
        /// <summary>
        /// 언데드 모드
        /// </summary>
        UndeadMode = 1 << 0,
        /// <summary>
        /// 이속 감소 모드
        /// </summary>
        MazeMoveSpdDown = 1 << 1,
        /// <summary>
        /// 이속 증가 모드
        /// </summary>
        MazeMoveSpdUp = 1 << 2,
        /// <summary>
        /// 버프 옵션 스킵 모드
        /// </summary>
        UseBuffItemOptions = 1 << 3,
        /// <summary>
        /// 회피 모드
        /// </summary>
        Flee = 1 << 4,
        /// <summary>
        /// 참조 스킬 사용 안하기 모드
        /// </summary>
        RefSkillOff = 1 << 5,
        /// <summary>
        /// 평타 스킬 사용 한하기 모드
        /// </summary>
        BasicAttackSkillOff = 1 << 6,
        /// <summary>
        /// 기본 모드 사용 (동등한 스탯)
        /// </summary>
        BasicStatusModeOn = 1 << 7,
        /// <summary>
        /// 축복 버프 사용 안하기 모드
        /// </summary>
        BlssModeOff = 1 << 8
    }
}