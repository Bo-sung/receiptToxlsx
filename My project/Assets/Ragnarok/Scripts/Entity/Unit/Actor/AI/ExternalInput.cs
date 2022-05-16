namespace Ragnarok
{
    /// <summary>
    /// 외부입력
    /// </summary>
    public class ExternalInput
    {
        /// <summary>
        /// 외부입력-스킬 초기화 시간
        /// 외부입력은 특정 시간이 지나면 자동 사라지도록 처리
        /// (애니메이션이 긴 동작이나, 오랫동안 움직일 수 없는 상태 등을 고려)
        /// </summary>
        public static readonly float RESET_INPUT_SKILL_TIME = 2f;

        SkillInfo skill;
        bool isController;

        RelativeRemainTime remainSkill;

        public delegate void SetSkillEvent(SkillInfo skillInfo);

        public event SetSkillEvent OnSetSkill;

        public void Reset()
        {
            remainSkill = 0f;
            skill = null;

            OnSetSkill?.Invoke(null);
        }

        public void SetSkill(SkillInfo input)
        {
            remainSkill = RESET_INPUT_SKILL_TIME; // 스킬
            skill = input;

            OnSetSkill?.Invoke(input);
        }

        public void SetMove(bool isController)
        {
            this.isController = isController;
        }

        public SkillInfo GetSelectedSkill()
        {
            return remainSkill.GetRemainTime() > 0f ? skill : null;
        }

        public bool GetIsController()
        {
            return isController;
        }
    }
}