namespace Ragnarok
{
    public enum EffectPointType
    {
        /// <summary>
        /// 타겟 기준 (스킬에 대상이 되는 타겟 기준으로 부터 스킬 효과 발동)
        /// </summary>
        Target = 1,

        /// <summary>
        /// 시전자 기준 (스킬을 사용한 시전자 기준으로 부터 스킬 효과 발동)
        /// </summary>
        Executer = 2,
    }
}