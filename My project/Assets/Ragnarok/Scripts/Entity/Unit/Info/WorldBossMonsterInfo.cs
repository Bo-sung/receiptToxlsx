namespace Ragnarok
{
    public class WorldBossMonsterInfo : MonsterInfo
    {
        public override string IconName => string.Concat("Boss_", base.IconName);

        public WorldBossMonsterInfo() : base(isBoss: true)
        {
        }
    }
}