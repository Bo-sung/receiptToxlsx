namespace Ragnarok
{
    /// <summary>
    /// 캐릭터 <see cref="StatusModel"/>
    /// 몬스터 <see cref="MonsterData"/>
    /// 큐펫 <see cref="CupetData"/>
    /// </summary>
    public interface IBasicStatus
    {
        int BasicStr { get; }
        int BasicAgi { get; }
        int BasicVit { get; }
        int BasicInt { get; }
        int BasicDex { get; }
        int BasicLuk { get; }
    }
}