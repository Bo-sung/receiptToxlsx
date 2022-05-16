namespace Ragnarok
{
    /// <summary>
    /// <see cref="UseItemInfo"/>
    /// <see cref="BuffSkillInfo"/>
    /// <see cref="BuffItemInfo"/>
    /// <see cref="CrowdControlInfo"/>
    /// </summary>
    public interface IBuff : IInfo
    {
        string IconName { get; }
        float GetProgress();
        bool IsValid();
    }
}