using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="BattlePoolManager"/>
    /// </summary>
    public interface IBattlePool
    {
        DroppedItem SpawnGold(Vector3 position);

        BounceDroppedItem[] SpawnBounceGolds(Vector3 position, int count);

        DroppedClickItem SpawnGold_ClickItem(Vector3 position);

        UnitCircle SpawnCircle(Transform parent, UnitEntityType entityType);

        MazeUnitCircle SpawnMazeUnitCircle(Transform parent);

        PoolObject SpawnShadow(Transform parent);

        CupetCube SpawnCupetCube(Transform parent);

        [System.Obsolete("조이스틱으로 변경되어 사용하지 않음")]
        PickingEffect SpawnClick(Vector3 position);

        TargetingLine SpawnTargetingLine(Transform parent);

        TargetingArrow SpawnArrow();

        MiddleBossTargetingArrow SpawnMvpTargetingArrow(Transform parent);

        MiddleBossTargetingArrow SpawnBossTargetingArrow(Transform parent);

        MiddleBossTargetingArrow SpawnNpcTargetingArrow(Transform parent, bool isDeviruchi);

        PoolObject SpawnCrowdControlEffect(Transform parent, CrowdControlType type);

        AutoReleasePoolObject SpawnAutoGuard(Transform parent);

        SkillAreaCircle SpawnSkillAreaCircle(Vector3 position);

        ImpulseSourceObject SpawnImpulseSource(Transform parent, BattlePoolManager.ImpulseType type);

        PanelBuffEffect SpawnPanelBuff(Transform parent);

        IMazeDropItem SpawnMazeDropItem(Vector3 position, MazeRewardType type, MazeBattleType battleType = MazeBattleType.None);

        AutoReleasePoolObject SpawnUnitAppear(Vector3 position);

        UnitSoul SpawnUnitSoul(Vector3 position);

        PoolObject SpawnDieMark(Vector3 position);

        PoolObject SpawnMonsterCube(Vector3 position, MazeBattleType type, bool isBoss);

        DropCube SpawnDropCube(Vector3 position, UIWidget target);

        MazeGold SpawnMazeZeny(Vector3 pos);

        MazeGold SpawnMazeExp(Vector3 pos);

        MazeGold SpawnMazeZenyYellow(Vector3 pos);

        PoolObject SpawnControllerAssist(Transform parent);

        PoolObject SpawnGrass(Vector3 pos, string grassPrefabName);

        NavPoolObjectWithReady SpawnGuildMazeRock();

        NavPoolObject SpawnGuildMazeItem();

        NavPoolObjectWithReady SpawnBoxTrap(BattleTrapType trapType);

        NavPoolObjectWithReady SpawnPowerUpPotion();

        PoolObject SpawnPowerUpEffect(Transform parent);

        PoolObject SpawnShieldEffect(Transform parent);

        PoolObject SpawnFrozen(Transform parent);

        PoolObject SpawnSleeping(Transform parent);

        PoolObject SpawnCubeLock(Transform parent);

        PoolObject SpawnBubble(Transform parent);

        UnitAura SpawnUnitAura(Transform parent);

        void SpawnUnitTeleport(Vector3 pos);

        PoolObject SpawnQuestionMark_Yellow(Transform parent);

        PoolObject SpawnQuestionMark_Gray(Transform parent);

        PoolObject SpawnExclamationMark(Transform parent);

        PoolObject SpawnHpHudTarget(Transform parent);

        AutoReleasePoolObject SpawnRebirth(Vector3 pos);

        DropCube SpawnDropMana(Vector3 position, UIWidget target);

        AutoReleasePoolObject SpawnHeal(Transform parent);

        PoolObject SpawnRageEffect(Transform parent);

        PoolObject SpawnSnowEffect(Transform parent);

        PoolObject SpawnAuraEffect(Transform parent, UnitAuraType type);

        PoolObject SpawnTimeSaveZone(Transform parent);
    }
}