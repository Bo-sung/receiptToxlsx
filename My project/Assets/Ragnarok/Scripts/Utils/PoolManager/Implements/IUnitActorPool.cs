namespace Ragnarok
{
    /// <summary>
    /// <see cref="UnitActorPoolManager"/>
    /// </summary>
    public interface IUnitActorPool
    {
        PlayerActor SpawnPlayer();
        CharacterActor SpawnMazePlayer();
        CharacterActor SpawnCharacter();
        CharacterActor SpawnDummyCharacter();
        CharacterActor SpawnBookDummyCharacter();
        GhostPlayerActor SpawnGhostPlayer();

        PlayerCupetActor SpawnPlayerCupet();
        CupetActor SpawnMazeCupet();
        CupetActor SpawnCupet();
        CupetActor SpawnGhostCupet();

        MonsterActor SpawnMonster();
        BossMonsterActor SpawnBossMonster();
        GuardianActor SpawnGuardian();
        GuardianDestroyerActor SpawnGuardianDestroyer();
        MonsterActor SpawnWorldBoss();
        MonsterActor SpawnMazeMonster();
        PlayerActor SpawnGVGPlayer();
        CharacterActor SpawnGVGMultiPlayer();
        NexusActor SpawnNexus();

        CharacterActor SpawnPlayerBot();
        MonsterActor SpawnMonsterBot();

        NpcActor SpawnNPC();
        MonsterActor SpawnTurret();
    }
}