namespace Ragnarok
{
    /// <summary>
    /// 전투 랜덤 값을 위해 필요
    /// <see cref="RandomTableDataManager"/>
    /// </summary>
    public interface IRandomDamage
    {
        /// <summary>
        /// 랜덤 시퀀스 값을 가져온다
        /// </summary>
        int GetRandomSeq();

        /// <summary>
        /// 랜덤 범위 값
        /// </summary>
        int GetRandomRange(int seq, int min, int max);
    }
}