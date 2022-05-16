namespace Ragnarok
{
    /// <summary>
    /// 캐릭터 <see cref="JobData"/>
    /// 몬스터 <see cref="MonsterData"/>
    /// 큐펫 <see cref="CupetData"/>
    /// </summary>
    public interface IStatusCoefficient
    {
        int HpCoefficient { get; }
        int AtkCoefficient { get; }
        int MatkCoefficient { get; }
        int DefCoefficient { get; }
        int MdefCoefficient { get; }
    }
}