namespace Ragnarok
{
    public enum UnitEntityType
    {
        Player,
        MultiPlayer,
        GhostPlayer,

        PlayerCupet,
        MultiCupet,
        GhostCupet,

        BossMonster,
        NormalMonster,
        WorldBoss,
        GhostMonster,
        MazeMonster,
        MvpMonster,
        MonsterBot,

        Guardian,
        GuardianDestroyer,

        Nexus,

        NPC,

        Turret,
        TurretBoss,

        UI,
    }

    public static class UnitEntityTypeExtensions
    {
        public static bool IsHideHpMonster(this UnitEntityType type)
        {
            switch (type)
            {
                case UnitEntityType.BossMonster:
                case UnitEntityType.WorldBoss:
                case UnitEntityType.MvpMonster:
                case UnitEntityType.TurretBoss:
                    return true;
            }
            return false;
        }

        public static bool IsHideNameMonster(this UnitEntityType type)
        {
            switch (type)
            {
                case UnitEntityType.BossMonster:
                case UnitEntityType.WorldBoss:
                case UnitEntityType.MvpMonster:
                case UnitEntityType.TurretBoss:
                    return true;
            }
            return false;
        }
    }
}