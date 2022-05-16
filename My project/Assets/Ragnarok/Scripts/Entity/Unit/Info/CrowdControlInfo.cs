namespace Ragnarok
{
    /// <summary>
    /// 현재 적용중인 상태이상
    /// </summary>
    public class CrowdControlInfo : DataInfo<CrowdControlData>, IBuff
    {
        /// <summary>
        /// 끝나는 시간
        /// </summary>
        private RelativeRemainTime endTime;

        /// <summary>
        /// 상태이상 아이디
        /// </summary>
        public int CrowdControlId => data.id;

        /// <summary>
        /// 상태이상 타입
        /// </summary>
        public CrowdControlType Type => CrowdControlId.ToEnum<CrowdControlType>();

        /// <summary>
        /// 아이콘 이름
        /// </summary>
        public string IconName => data.icon_name;

        /// <summary>
        /// 이동 불가능 여부
        /// </summary>
        public bool CannotMove => data.cannot_move > 0;

        /// <summary>
        /// 평타 스킬 사용 불가능 여부
        /// </summary>
        public bool CannotUseBasicAttack => data.cannot_use_basic_attack > 0;

        /// <summary>
        /// 스킬 사용 불가능 여부
        /// </summary>
        public bool CannotUseSkill => data.cannot_use_skill > 0;

        /// <summary>
        /// 스킬 회피 불가능 여부
        /// </summary>
        public bool CannotFlee => data.cannot_flee > 0;

        /// <summary>
        /// 그로기 상태 여부
        /// </summary>
        public bool IsGroggy => CannotMove && CannotUseBasicAttack && CannotUseSkill;

        /// <summary>
        /// 지속시간
        /// </summary>
        public float Duration => GetDuration();

        /// <summary>
        /// 전체 체력 퍼센트 도트대미지
        /// </summary>
        public int DotDamageRate => data.dot_damage_rate;

        /// <summary>
        /// 물리방어력 감소율
        /// </summary>
        public int DefDecreaseRate => data.def_decrease_rate;

        /// <summary>
        /// 마법방어력 감소율
        /// </summary>
        public int MdefDecreaseRate => data.mdef_decrease_rate;

        /// <summary>
        /// 전체대미지 감소율
        /// </summary>
        public int TotalDmgDecreaseRate => data.total_dmg_decrease_rate;

        /// <summary>
        /// 크리티컬 확률 감소율
        /// </summary>
        public int CriRateDecreaseRate => data.cri_rate_decrease_rate;

        /// <summary>
        /// 이동속도 감소율
        /// </summary>
        public int MoveSpdDecreaseRate => data.move_spd_decrease_rate;

        /// <summary>
        /// 공격속도 감소율
        /// </summary>
        public int AtkSpdDecreaseRate => data.atk_spd_decreaase_rate;

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize()
        {
            endTime = GetDuration();
        }

        /// <summary>
        /// 유효성
        /// </summary>
        public bool IsValid()
        {
            return endTime.GetRemainTime() > 0f;
        }

        /// <summary>
        /// 진행도
        /// </summary>
        public float GetProgress()
        {
            float duration = GetDuration();
            if (duration == 0f)
                return 1f;

            float remainTime = endTime.GetRemainTime();
            if (remainTime == 0f)
                return 1f;

            return 1 - (remainTime / duration);
        }

        /// <summary>
        /// 지속시간 반환
        /// </summary>
        private float GetDuration()
        {
            if (data.duration == 0)
                return 0f;

            return data.duration * 0.001f;
        }
    }
}