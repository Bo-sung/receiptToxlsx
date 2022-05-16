namespace Ragnarok
{
    public interface IElementDamage
    {
        /// <summary>
        /// 속성댐 배율
        /// </summary>
        int Get(ElementType attackerType, int attackerLevel, ElementType defenderType, int defenderLevel);
    }
}