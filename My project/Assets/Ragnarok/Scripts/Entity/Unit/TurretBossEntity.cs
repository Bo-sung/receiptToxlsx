namespace Ragnarok
{
    public sealed class TurretBossEntity : TurretEntity
    {
        public override UnitEntityType type => UnitEntityType.TurretBoss;

        protected override void ApplyHP(int value, int blowCount, bool isNotDie)
        {
        }
    }
}