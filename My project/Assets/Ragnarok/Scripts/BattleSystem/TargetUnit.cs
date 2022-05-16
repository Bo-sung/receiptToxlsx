namespace Ragnarok
{
    public struct TargetUnit
    {
        /// <summary>
        /// 타겟
        /// </summary>
        public readonly UnitActor target;

        /// <summary>
        /// 선택된 스킬
        /// </summary>
        public readonly SkillInfo selectedSkill;

        public TargetUnit(UnitActor target, SkillInfo selectedSkill)
        {
            this.target = target;
            this.selectedSkill = selectedSkill;
        }

        /// <summary>
        /// 유효하지 않음
        /// </summary>
        public bool IsInvalid()
        {
            return target == null || selectedSkill == null;
        }
    }
}