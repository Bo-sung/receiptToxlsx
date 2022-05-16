using UnityEngine;

namespace Ragnarok
{
    public sealed class BattlePoolManager : PoolManager<BattlePoolManager>, IBattlePool
    {
        public enum ImpulseType
        {
            /// <summary>
            /// 크리티컬 피격
            /// </summary>
            CriticalHit,

            /// <summary>
            /// 보스 등장
            /// </summary>
            BossAppearance,

            /// <summary>
            /// 일반 공격
            /// </summary>
            NormalAttack,

            /// <summary>
            /// 클릭 공격
            /// </summary>
            ClickAttack,
        }

        IEffectContainer effectContainer;

        protected override void Awake()
        {
            base.Awake();

            effectContainer = AssetManager.Instance;
        }

        protected override Transform GetOriginal(string key)
        {
            GameObject go = effectContainer.Get(key);

            if (go == null)
                throw new System.ArgumentException($"존재하지 않는 GameObject 입니다: {nameof(key)} = {key}");

            return go.transform;
        }

        protected override PoolObject Create(string key)
        {
            GameObject go = effectContainer.Get(key);

            if (go == null)
                throw new System.ArgumentException($"존재하지 않는 GameObject 입니다: {nameof(key)} = {key}");

            return Instantiate(go).AddMissingComponent<PoolObject>();
        }

        DroppedItem IBattlePool.SpawnGold(Vector3 position)
        {
            return Spawn("Gold_01", position) as DroppedItem;
        }

        BounceDroppedItem[] IBattlePool.SpawnBounceGolds(Vector3 position, int count)
        {
            BetterList<BounceDroppedItem> list = new BetterList<BounceDroppedItem>();
            for (int i = 0; i < count; i++)
            {
                BounceDroppedItem coin = Spawn("Bounce_Gold", position) as BounceDroppedItem;
                list.Add(coin);
            }
            return list.ToArray();
        }

        DroppedClickItem IBattlePool.SpawnGold_ClickItem(Vector3 position)
        {
            return Spawn("Gold_01_ClickItem", position) as DroppedClickItem;
        }

        UnitCircle IBattlePool.SpawnCircle(Transform parent, UnitEntityType type)
        {
            return Spawn(GetCircleName(type), parent, worldPositionStays: false) as UnitCircle;
        }

        MazeUnitCircle IBattlePool.SpawnMazeUnitCircle(Transform parent)
        {
            return Spawn("MazeUnitCircle", parent, worldPositionStays: false) as MazeUnitCircle;
        }

        [System.Obsolete("조이스틱으로 변경되어 사용하지 않음")]
        PickingEffect IBattlePool.SpawnClick(Vector3 position)
        {
            return Spawn("Click", position) as PickingEffect;
        }

        TargetingLine IBattlePool.SpawnTargetingLine(Transform parent)
        {
            return Spawn("Targeting Line", parent, worldPositionStays: false) as TargetingLine;
        }

        TargetingArrow IBattlePool.SpawnArrow()
        {
            return Spawn("Arrow") as TargetingArrow;
        }

        MiddleBossTargetingArrow IBattlePool.SpawnMvpTargetingArrow(Transform parent)
        {
            return Spawn("MiddleBossTargetingArrow", parent, worldPositionStays: false) as MiddleBossTargetingArrow;
        }

        MiddleBossTargetingArrow IBattlePool.SpawnBossTargetingArrow(Transform parent)
        {
            return Spawn("BossTargetingArrow", parent, worldPositionStays: false) as MiddleBossTargetingArrow;
        }

        MiddleBossTargetingArrow IBattlePool.SpawnNpcTargetingArrow(Transform parent, bool isDeviruchi)
        {
            return Spawn(isDeviruchi ? "DeviTargetingArrow" : "NpcTargetingArrow", parent, worldPositionStays: false) as MiddleBossTargetingArrow;
        }

        PoolObject IBattlePool.SpawnCrowdControlEffect(Transform parent, CrowdControlType type)
        {
            return Spawn(GetEffectName(type), parent, worldPositionStays: false);
        }

        AutoReleasePoolObject IBattlePool.SpawnAutoGuard(Transform parent)
        {
            return Spawn("AutoGuard", parent, worldPositionStays: false) as AutoReleasePoolObject;
        }

        SkillAreaCircle IBattlePool.SpawnSkillAreaCircle(Vector3 position)
        {
            return Spawn("SkillAreaCircle", position) as SkillAreaCircle;
        }

        ImpulseSourceObject IBattlePool.SpawnImpulseSource(Transform parent, ImpulseType type)
        {
            return Spawn(GetImpulseSourceName(type), parent, worldPositionStays: false) as ImpulseSourceObject;
        }

        PanelBuffEffect IBattlePool.SpawnPanelBuff(Transform parent)
        {
            return Spawn("PanelBuff", parent.position) as PanelBuffEffect;
        }

        IMazeDropItem IBattlePool.SpawnMazeDropItem(Vector3 position, MazeRewardType type, MazeBattleType battleType)
        {
            return Spawn(GetMazeRewardSourceName(type, battleType), position) as IMazeDropItem;
        }

        AutoReleasePoolObject IBattlePool.SpawnUnitAppear(Vector3 position)
        {
            return Spawn("UnitAppear", position) as AutoReleasePoolObject;
        }

        UnitSoul IBattlePool.SpawnUnitSoul(Vector3 position)
        {
            return Spawn("UnitSoul", position) as UnitSoul;
        }

        PoolObject IBattlePool.SpawnDieMark(Vector3 position)
        {
            return Spawn("DieMark", position);
        }

        PoolObject IBattlePool.SpawnMonsterCube(Vector3 position, MazeBattleType type, bool isBoss)
        {
            if (isBoss)
            {
                return Spawn("MazeMonsterCubeBoss", position);
            }
            return Spawn(GetMazeMonsterCubeName(type), position);
        }

        DropCube IBattlePool.SpawnDropCube(Vector3 position, UIWidget target)
        {
            DropCube dropCube = Spawn("DropCube", position) as DropCube;
            dropCube.Initialize(target);
            return dropCube;
        }

        DropCube IBattlePool.SpawnDropMana(Vector3 position, UIWidget target)
        {
            DropCube dropMana = Spawn("DropMana", position) as DropCube;
            dropMana.Initialize(target);
            return dropMana;
        }

        PoolObject IBattlePool.SpawnShadow(Transform parent)
        {
            return Spawn("Shadow150", parent, worldPositionStays: false);
        }

        CupetCube IBattlePool.SpawnCupetCube(Transform parent)
        {
            CupetCube cupetCube = Spawn("CupetCube", parent.position) as CupetCube;
            return cupetCube;
        }

        MazeGold IBattlePool.SpawnMazeZeny(Vector3 pos)
        {
            return Spawn("MazeGold", pos) as MazeGold;
        }

        MazeGold IBattlePool.SpawnMazeZenyYellow(Vector3 pos)
        {
            return Spawn("MazeGold_New", pos) as MazeGold;
        }

        MazeGold IBattlePool.SpawnMazeExp(Vector3 pos)
        {
            return Spawn("MazeExp", pos) as MazeGold;
        }

        PoolObject IBattlePool.SpawnControllerAssist(Transform parent)
        {
            return Spawn("ControllerAssist", parent, worldPositionStays: false);
        }

        PoolObject IBattlePool.SpawnGrass(Vector3 pos, string grassPrefabName)
        {
            return Spawn(grassPrefabName, pos);
        }

        NavPoolObjectWithReady IBattlePool.SpawnGuildMazeRock()
        {
            return Spawn("GuildMazeRock") as NavPoolObjectWithReady;
        }

        NavPoolObject IBattlePool.SpawnGuildMazeItem()
        {
            return Spawn("GuildMazeItem") as NavPoolObject;
        }

        NavPoolObjectWithReady IBattlePool.SpawnBoxTrap(BattleTrapType trapType)
        {
            return Spawn(GetTrapName(trapType)) as NavPoolObjectWithReady;
        }

        NavPoolObjectWithReady IBattlePool.SpawnPowerUpPotion()
        {
            return Spawn("PowerUpPotion") as NavPoolObjectWithReady;
        }

        PoolObject IBattlePool.SpawnPowerUpEffect(Transform parent)
        {
            return Spawn("PowerUp", parent, worldPositionStays: false);
        }

        PoolObject IBattlePool.SpawnShieldEffect(Transform parent)
        {
            return Spawn("Shield", parent, worldPositionStays: false);
        }

        PoolObject IBattlePool.SpawnFrozen(Transform parent)
        {
            return Spawn("Ice", parent, worldPositionStays: false);
        }

        PoolObject IBattlePool.SpawnSleeping(Transform parent)
        {
            return Spawn("Sleep_FX", parent, worldPositionStays: false);
        }

        PoolObject IBattlePool.SpawnCubeLock(Transform parent)
        {
            return Spawn("Maze_Cube_1", parent, worldPositionStays: false);
        }

        PoolObject IBattlePool.SpawnBubble(Transform parent)
        {
            return Spawn("Bubble", parent, worldPositionStays: false);
        }

        UnitAura IBattlePool.SpawnUnitAura(Transform parent)
        {
            return Spawn("UnitAura", parent, worldPositionStays: false) as UnitAura;
        }

        void IBattlePool.SpawnUnitTeleport(Vector3 pos)
        {
            Spawn("UnitTeleport", pos);
        }

        PoolObject IBattlePool.SpawnQuestionMark_Yellow(Transform parent)
        {
            return Spawn("Question_FX_Yellow", parent, worldPositionStays: false);
        }

        PoolObject IBattlePool.SpawnQuestionMark_Gray(Transform parent)
        {
            return Spawn("Question_FX_Gray", parent, worldPositionStays: false);
        }

        PoolObject IBattlePool.SpawnExclamationMark(Transform parent)
        {
            return Spawn("Exclamation_FX", parent, worldPositionStays: false);
        }

        PoolObject IBattlePool.SpawnHpHudTarget(Transform parent)
        {
            return Spawn("HpHudTarget", parent, worldPositionStays: false);
        }

        AutoReleasePoolObject IBattlePool.SpawnRebirth(Vector3 pos)
        {
            return Spawn("UnitRebirth", pos) as AutoReleasePoolObject;
        }

        AutoReleasePoolObject IBattlePool.SpawnHeal(Transform parent)
        {
            return Spawn("UnitHeal", parent, worldPositionStays: false) as AutoReleasePoolObject;
        }

        PoolObject IBattlePool.SpawnAuraEffect(Transform parent, UnitAuraType type)
        {
            return Spawn(GetAuraName(type), parent, worldPositionStays: false);
        }

        private string GetEffectName(CrowdControlType type)
        {
            switch (type)
            {
                case CrowdControlType.Stun:
                    return "VFX_Statu_Stun";

                case CrowdControlType.Silence:
                    return "VFX_Statu_Darkness";

                case CrowdControlType.Sleep:
                    return "VFX_Statu_Sleep";

                case CrowdControlType.Hallucination:
                    return "VFX_Statu_Confusion";

                case CrowdControlType.Bleeding:
                    return "VFX_Statu_Bleed";

                case CrowdControlType.Burning:
                    return "VFX_Statu_Burning";

                case CrowdControlType.Poison:
                    return "VFX_Statu_Infection";

                case CrowdControlType.Curse:
                    return "VFX_Statu_Curse";

                case CrowdControlType.Freezing:
                    return "VFX_Statu_Freezing";

                case CrowdControlType.Frozen:
                    return "VFX_Statu_Frozen";
            }

            return string.Empty;
        }

        private string GetImpulseSourceName(ImpulseType type)
        {
            switch (type)
            {
                case ImpulseType.CriticalHit:
                    return "ImpulseSource_CriticalHit";

                case ImpulseType.BossAppearance:
                    return "ImpulseSource_BossAppearance";

                case ImpulseType.NormalAttack:
                    return "ImpulseSource_NormalAttack";

                case ImpulseType.ClickAttack:
                    return "ImpulseSource_ClickAttack";
            }

            return string.Empty;
        }

        private string GetMazeRewardSourceName(MazeRewardType type, MazeBattleType battleType)
        {
            switch (type)
            {
                case MazeRewardType.Zeny:
                    return "MazeGold";

                case MazeRewardType.NormalQuest:
                    return "MazeQuest";

                case MazeRewardType.RandomBox:
                    return "MazeBox";

                case MazeRewardType.CubePiece:
                    return GetMazeRewardCubeSourceName(battleType);

                case MazeRewardType.Treasure:
                    return "MazeTreasure";

                case MazeRewardType.BombCamera:
                case MazeRewardType.BombHP:
                case MazeRewardType.BombControl:
                    return "MazeBomb";
                case MazeRewardType.HindranceCamera:
                case MazeRewardType.HindranceControl:
                    return "MazeBomb2";

                case MazeRewardType.MultiMazeCube:
                    return "MazeCoin";

                case MazeRewardType.SpeedItem:
                    return "MazePotion";

                case MazeRewardType.Snowball:
                    return "SnowBall";

                case MazeRewardType.Rudolph:
                    return "MiniRudolf";

                case MazeRewardType.PowerUpPotion:
                    return "PowerUpPotion_Maze";

                case MazeRewardType.HpPotion:
                    return "MazeRedPotion";

                case MazeRewardType.Emperium:
                    return "MazeEmperium";
            }

            return string.Empty;
        }

        private string GetMazeRewardCubeSourceName(MazeBattleType type)
        {
            switch (type)
            {
                case MazeBattleType.None:
                    return "MazeCube0";

                case MazeBattleType.Simple:
                    return "MazeCube1";

                case MazeBattleType.Clicker:
                    return "MazeCube2";

                case MazeBattleType.ClearAction:
                    return "MazeCube3";

                case MazeBattleType.Action:
                    return "MazeCube4";

                case MazeBattleType.IntoBattle:
                    return "MazeCube5";

                case MazeBattleType.AutoIntoBattle:
                    return "MazeCube6";
            }

            return string.Empty;
        }

        private string GetMazeMonsterCubeName(MazeBattleType type)
        {
            switch (type)
            {
                case MazeBattleType.Simple:
                    return "MazeMonsterCube1";

                case MazeBattleType.Clicker:
                    return "MazeMonsterCube2";

                case MazeBattleType.ClearAction:
                    return "MazeMonsterCube3";

                case MazeBattleType.Action:
                    return "MazeMonsterCube4";

                case MazeBattleType.IntoBattle:
                    return "MazeMonsterCube5";

                case MazeBattleType.AutoIntoBattle:
                    return "MazeMonsterCube6";
            }

            return string.Empty;
        }

        private string GetCircleName(UnitEntityType type)
        {
            switch (type)
            {
                case UnitEntityType.Player:
                case UnitEntityType.MultiPlayer:
                case UnitEntityType.GhostPlayer:
                    return "CharacterCircle";

                case UnitEntityType.MvpMonster:
                    return "MiddleBossCircle";

                case UnitEntityType.MazeMonster:
                    return "UnitSquare";
            }

            return "UnitCircle";
        }

        private string GetAuraName(UnitAuraType type)
        {
            switch (type)
            {
                case UnitAuraType.HardModeNormalMonster:
                    return "Arousal_NomalMonster";

                case UnitAuraType.HardModeBossMonster:
                    return "Arousal_BossMonster";

                case UnitAuraType.MazeBossMonsterProtect:
                    return "Scenario_BossFX";

                case UnitAuraType.ForestMazeBossMonsterShield:
                    return "Baphomet_Shield_FX";

                case UnitAuraType.ForestMazeMiddleBossMonster:
                    return "M_Boss_Red_FX";
            }

            return string.Empty;
        }

        public PoolObject SpawnRageEffect(Transform parent)
        {
            return Spawn("Rage_FX", parent, false);
        }

        public PoolObject SpawnSnowEffect(Transform parent)
        {
            return Spawn("Snow", parent, false);
        }

        private string GetTrapName(BattleTrapType type)
        {
            switch (type)
            {
                case BattleTrapType.FreeFight:
                    return "BoxTrap";

                case BattleTrapType.GuildAttack:
                    return "GuildAttackTrap";

                case BattleTrapType.ChristmasFreeFight:
                    return "SnowBallTrap";

                case BattleTrapType.WaterBombFreeFight:
                    return "WaterBombTrap";
            }

            return "BoxTrap";
        }

        PoolObject IBattlePool.SpawnTimeSaveZone(Transform parent)
        {
            return Spawn("Time_Savezone", parent, worldPositionStays: false);
        }
    }
}