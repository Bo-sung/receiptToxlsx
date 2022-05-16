using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UnitActorPoolManager : PoolManager<UnitActorPoolManager>, IUnitActorPool
    {
        private const string PLAYER = "Player";
        private const string MAZE_PLAYER = "MazePlayer";
        private const string CHARACTER = "Character";
        private const string DUMMY_CHARACTER = "DummyCharacter";
        private const string BOOK_DUMMY_CHARACTER = "BookDummyCharacter";
        private const string GHOST_PLAYER = "GhostPlayer";
        private const string PLAYER_CUPET = "PlayerCupet";
        private const string MAZE_CUPET = "MazeCupet";
        private const string CUPET = "Cupet";
        private const string GHOST_CUPET = "GhostCupet";
        private const string MONSTER = "Monster";
        private const string BOSS_MONSTER = "BossMonster";
        private const string GUARDIAN = "Guardian";
        private const string GUARDIAN_DESTROYER = "GuardianDestroyer";
        private const string WORLD_BOSS = "WorldBoss";
        private const string MAZE_MONSTER = "MazeMonster";
        private const string GVG_PLAYER = "GVGPlayer";
        private const string GVG_MULTI_PLAYER = "GVGMultiPlayer";
        private const string NEXUS = "Nexus";
        private const string PLAYER_BOT = "PlayerBot";
        private const string MONSTER_BOT = "MonsterBot";
        private const string NPC = "NPC";
        private const string TURRET = "Turret";

        private Dictionary<string, UnitActor> dic;

        void Start()
        {
            dic = new Dictionary<string, UnitActor>(System.StringComparer.Ordinal)
            {
                { PLAYER, CreatePlayer() },
                { MAZE_PLAYER, CreateMazePlayer() },
                { CHARACTER, CreateCharacter() },
                { DUMMY_CHARACTER, CreateDummyCharacter() },
                { BOOK_DUMMY_CHARACTER, CreateBookDummyCharacter() },
                { GHOST_PLAYER, CreateGhostPlayer() },
                { CUPET, CreateCupet() },
                { GHOST_CUPET, CreateGhostCupet() },
                { PLAYER_CUPET, CreatePlayerCupet() },
                { MAZE_CUPET, CreateMazeCupet() },
                { MONSTER, CreateMonster() },
                { BOSS_MONSTER, CreateBossMonster() },
                { GUARDIAN, CreateGuardian() },
                { GUARDIAN_DESTROYER, CreateGuardianDestroyer() },
                { WORLD_BOSS, CreateWorldBoss() },
                { MAZE_MONSTER, CreateMazeMonster() },
                { GVG_PLAYER, CreateGVGPlayer() },
                { GVG_MULTI_PLAYER, CreateGVGMultiPlayer() },
                { NEXUS, CreateNexus() },
                { PLAYER_BOT, CreatePlayerBot() },
                { MONSTER_BOT, CreateMonsterBot() },
                { NPC, CreateNPC() },
                { TURRET, CreateTurret() },
            };
        }

        protected override Transform GetOriginal(string key)
        {
            UnitActor unitActor = dic.ContainsKey(key) ? dic[key] : null;

            if (unitActor == null)
                throw new System.ArgumentException($"존재하지 않는 UnitActor 입니다: {nameof(key)} = {key}");

            return unitActor.CachedTransform;
        }

        protected override PoolObject Create(string key)
        {
            UnitActor unitActor = dic.ContainsKey(key) ? dic[key] : null;

            if (unitActor == null)
                throw new System.ArgumentException($"존재하지 않는 UnitActor 입니다: {nameof(key)} = {key}");

            return Instantiate(unitActor);
        }

        PlayerActor IUnitActorPool.SpawnPlayer()
        {
            return Spawn(PLAYER) as PlayerActor;
        }

        CharacterActor IUnitActorPool.SpawnMazePlayer()
        {
            return Spawn(MAZE_PLAYER) as CharacterActor;
        }

        CharacterActor IUnitActorPool.SpawnCharacter()
        {
            return Spawn(CHARACTER) as CharacterActor;
        }

        CharacterActor IUnitActorPool.SpawnDummyCharacter()
        {
            return Spawn(DUMMY_CHARACTER) as CharacterActor;
        }

        CharacterActor IUnitActorPool.SpawnBookDummyCharacter()
        {
            return Spawn(BOOK_DUMMY_CHARACTER) as CharacterActor;
        }

        GhostPlayerActor IUnitActorPool.SpawnGhostPlayer()
        {
            return Spawn(GHOST_PLAYER) as GhostPlayerActor;
        }

        PlayerCupetActor IUnitActorPool.SpawnPlayerCupet()
        {
            return Spawn(PLAYER_CUPET) as PlayerCupetActor;
        }

        CupetActor IUnitActorPool.SpawnMazeCupet()
        {
            return Spawn(MAZE_CUPET) as CupetActor;
        }

        CupetActor IUnitActorPool.SpawnCupet()
        {
            return Spawn(CUPET) as CupetActor;
        }

        CupetActor IUnitActorPool.SpawnGhostCupet()
        {
            return Spawn(GHOST_CUPET) as CupetActor;
        }

        MonsterActor IUnitActorPool.SpawnMonster()
        {
            return Spawn(MONSTER) as MonsterActor;
        }

        BossMonsterActor IUnitActorPool.SpawnBossMonster()
        {
            return Spawn(BOSS_MONSTER) as BossMonsterActor;
        }

        GuardianActor IUnitActorPool.SpawnGuardian()
        {
            return Spawn(GUARDIAN) as GuardianActor;
        }

        GuardianDestroyerActor IUnitActorPool.SpawnGuardianDestroyer()
        {
            return Spawn(GUARDIAN_DESTROYER) as GuardianDestroyerActor;
        }

        MonsterActor IUnitActorPool.SpawnWorldBoss()
        {
            return Spawn(WORLD_BOSS) as MonsterActor;
        }

        MonsterActor IUnitActorPool.SpawnMazeMonster()
        {
            return Spawn(MAZE_MONSTER) as MonsterActor;
        }

        PlayerActor IUnitActorPool.SpawnGVGPlayer()
        {
            return Spawn(GVG_PLAYER) as PlayerActor;
        }

        CharacterActor IUnitActorPool.SpawnGVGMultiPlayer()
        {
            return Spawn(GVG_MULTI_PLAYER) as CharacterActor;
        }

        NexusActor IUnitActorPool.SpawnNexus()
        {
            return Spawn(NEXUS) as NexusActor;
        }

        CharacterActor IUnitActorPool.SpawnPlayerBot()
        {
            return Spawn(PLAYER_BOT) as CharacterActor;
        }

        MonsterActor IUnitActorPool.SpawnMonsterBot()
        {
            return Spawn(MONSTER_BOT) as MonsterActor;
        }

        NpcActor IUnitActorPool.SpawnNPC()
        {
            return Spawn(NPC) as NpcActor;
        }

        MonsterActor IUnitActorPool.SpawnTurret()
        {
            return Spawn(TURRET) as MonsterActor;
        }

        private PlayerActor CreatePlayer()
        {
            GameObject go = new GameObject(PLAYER) { hideFlags = HideFlags.HideAndDontSave };

            PlayerActor actor = go.AddComponent<PlayerActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CharacterAnimator>(); // 애니메이션 관리자
            go.AddComponent<CharacterAppearance>(); // 외형 관리자
            go.AddComponent<PlayerAI>(); // 인공지능 관리자
            go.AddComponent<CharacterMovement>(); // 움직임 관리자
            go.AddComponent<ObjectPicking>(); // Picking 관리자
            go.AddComponent<PlayerEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private CharacterActor CreateMazePlayer()
        {
            GameObject go = new GameObject(MAZE_PLAYER) { hideFlags = HideFlags.HideAndDontSave };

            CharacterActor actor = go.AddComponent<CharacterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CharacterAnimator>(); // 애니메이션 관리자
            go.AddComponent<CharacterAppearance>(); // 외형 관리자
            go.AddComponent<MazePlayerAI>(); // 인공지능 관리자
            go.AddComponent<MazeUnitMovement>(); // 움직임 관리자
            go.AddComponent<ObjectPicking>(); // Picking 관리자
            go.AddComponent<MazePlayerEffectPlayer>(); // 이펙트 관리자
            go.AddComponent<UnitRadar>(); // 유닛감지 관리자

            return actor;
        }

        private PlayerActor CreateGVGPlayer()
        {
            GameObject go = new GameObject(GVG_PLAYER) { hideFlags = HideFlags.HideAndDontSave };

            PlayerActor actor = go.AddComponent<PlayerActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CharacterAnimator>(); // 애니메이션 관리자
            go.AddComponent<CharacterAppearance>(); // 외형 관리자
            go.AddComponent<GVGPlayerAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<ObjectPicking>(); // Picking 관리자
            go.AddComponent<CharacterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private CharacterActor CreateGVGMultiPlayer()
        {
            GameObject go = new GameObject(GVG_MULTI_PLAYER) { hideFlags = HideFlags.HideAndDontSave };

            PlayerActor actor = go.AddComponent<PlayerActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CharacterAnimator>(); // 애니메이션 관리자
            go.AddComponent<CharacterAppearance>(); // 외형 관리자
            go.AddComponent<GVGMultiPlayerAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<ObjectPicking>(); // Picking 관리자
            go.AddComponent<GVGMultiPlayerEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private CharacterActor CreateCharacter()
        {
            GameObject go = new GameObject(CHARACTER) { hideFlags = HideFlags.HideAndDontSave };

            CharacterActor actor = go.AddComponent<CharacterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CharacterAnimator>(); // 애니메이션 관리자
            go.AddComponent<CharacterAppearance>(); // 외형 관리자
            go.AddComponent<CharacterAI>(); // 인공지능 관리자
            go.AddComponent<CharacterMovement>(); // 움직임 관리자
            go.AddComponent<ObjectPicking>(); // Picking 관리자
            go.AddComponent<CharacterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private CharacterActor CreateDummyCharacter()
        {
            GameObject go = new GameObject(DUMMY_CHARACTER) { hideFlags = HideFlags.HideAndDontSave };

            CharacterActor actor = go.AddComponent<CharacterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CharacterAnimator>(); // 애니메이션 관리자
            go.AddComponent<CharacterAppearance>(); // 외형 관리자
            go.AddComponent<DummyCharacterAI>(); // 인공지능 관리자
            go.AddComponent<CharacterMovement>(); // 움직임 관리자
            go.AddComponent<ObjectPicking>(); // Picking 관리자
            go.AddComponent<CharacterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private CharacterActor CreateBookDummyCharacter()
        {
            GameObject go = new GameObject(BOOK_DUMMY_CHARACTER) { hideFlags = HideFlags.HideAndDontSave };

            CharacterActor actor = go.AddComponent<BookDummyCharacterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CharacterAnimator>(); // 애니메이션 관리자
            go.AddComponent<CharacterAppearance>(); // 외형 관리자
            go.AddComponent<DummyCharacterAI>(); // 인공지능 관리자
            go.AddComponent<CharacterMovement>(); // 움직임 관리자
            go.AddComponent<ObjectPicking>(); // Picking 관리자
            go.AddComponent<CharacterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private GhostPlayerActor CreateGhostPlayer()
        {
            GameObject go = new GameObject(GHOST_PLAYER) { hideFlags = HideFlags.HideAndDontSave };

            GhostPlayerActor actor = go.AddComponent<GhostPlayerActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CharacterAnimator>(); // 애니메이션 관리자
            go.AddComponent<CharacterAppearance>(); // 외형 관리자
            go.AddComponent<GhostPlayerAI>(); // 인공지능 관리자
            go.AddComponent<CharacterMovement>(); // 움직임 관리자
            go.AddComponent<ObjectPicking>(); // Picking 관리자
            go.AddComponent<CharacterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private CupetActor CreateCupet()
        {
            GameObject go = new GameObject(CUPET) { hideFlags = HideFlags.HideAndDontSave };

            CupetActor actor = go.AddComponent<CupetActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CupetAnimator>(); // 애니메이션 관리자
            go.AddComponent<CupetAppearance>(); // 외형 관리자
            go.AddComponent<CupetAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<CupetEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private PlayerCupetActor CreatePlayerCupet()
        {
            GameObject go = new GameObject(PLAYER_CUPET) { hideFlags = HideFlags.HideAndDontSave };

            PlayerCupetActor actor = go.AddComponent<PlayerCupetActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CupetAnimator>(); // 애니메이션 관리자
            go.AddComponent<CupetAppearance>(); // 외형 관리자
            go.AddComponent<PlayerCupetAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<CupetEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private CupetActor CreateMazeCupet()
        {
            GameObject go = new GameObject(MAZE_CUPET) { hideFlags = HideFlags.HideAndDontSave };

            CupetActor actor = go.AddComponent<CupetActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CupetAnimator>(); // 애니메이션 관리자
            go.AddComponent<CupetAppearance>(); // 외형 관리자
            go.AddComponent<MazeCupetAI>(); // 인공지능 관리자
            go.AddComponent<MazeCupetUnitMovement>(); // 움직임 관리자
            go.AddComponent<CupetEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private CupetActor CreateGhostCupet()
        {
            GameObject go = new GameObject(GHOST_CUPET) { hideFlags = HideFlags.HideAndDontSave };

            GhostCupetActor actor = go.AddComponent<GhostCupetActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CupetAnimator>(); // 애니메이션 관리자
            go.AddComponent<CupetAppearance>(); // 외형 관리자
            go.AddComponent<GhostCupetAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<CupetEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private NormalMonsterActor CreateMonster()
        {
            GameObject go = new GameObject(MONSTER) { hideFlags = HideFlags.HideAndDontSave };

            NormalMonsterActor actor = go.AddComponent<NormalMonsterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<MonsterAnimator>(); // 애니메이션 관리자
            go.AddComponent<MonsterAppearance>(); // 외형 관리자
            go.AddComponent<NormalMonsterAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<NormalMonsterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private BossMonsterActor CreateBossMonster()
        {
            GameObject go = new GameObject(MONSTER) { hideFlags = HideFlags.HideAndDontSave };

            BossMonsterActor actor = go.AddComponent<BossMonsterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<MonsterAnimator>(); // 애니메이션 관리자
            go.AddComponent<MonsterAppearance>(); // 외형 관리자
            go.AddComponent<BossMonsterAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<BossMonsterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private GuardianActor CreateGuardian()
        {
            GameObject go = new GameObject(GUARDIAN) { hideFlags = HideFlags.HideAndDontSave };

            GuardianActor actor = go.AddComponent<GuardianActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<MonsterAnimator>(); // 애니메이션 관리자
            go.AddComponent<MonsterAppearance>(); // 외형 관리자
            go.AddComponent<GuardianAI>(); // 인공지능 관리자
            go.AddComponent<GuardianMovement>(); // 움직임 관리자
            go.AddComponent<MonsterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private GuardianDestroyerActor CreateGuardianDestroyer()
        {
            GameObject go = new GameObject(GUARDIAN_DESTROYER) { hideFlags = HideFlags.HideAndDontSave };

            GuardianDestroyerActor actor = go.AddComponent<GuardianDestroyerActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<MonsterAnimator>(); // 애니메이션 관리자
            go.AddComponent<MonsterAppearance>(); // 외형 관리자
            go.AddComponent<GuardianDestroyerAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<MonsterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private NormalMonsterActor CreateWorldBoss()
        {
            GameObject go = new GameObject(WORLD_BOSS) { hideFlags = HideFlags.HideAndDontSave };

            NormalMonsterActor actor = go.AddComponent<NormalMonsterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<MonsterAnimator>(); // 애니메이션 관리자
            go.AddComponent<MonsterAppearance>(); // 외형 관리자
            go.AddComponent<WorldBossAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<WorldBossEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private MonsterActor CreateMazeMonster()
        {
            GameObject go = new GameObject(MAZE_MONSTER) { hideFlags = HideFlags.HideAndDontSave };

            MonsterActor actor = go.AddComponent<MonsterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<MonsterAnimator>(); // 애니메이션 관리자
            go.AddComponent<MonsterAppearance>(); // 외형 관리자
            go.AddComponent<MazeMonsterAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<MazeMonsterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private NexusActor CreateNexus()
        {
            GameObject go = new GameObject(NEXUS) { hideFlags = HideFlags.HideAndDontSave };

            NexusActor actor = go.AddComponent<NexusActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<MonsterAnimator>(); // 애니메이션 관리자
            go.AddComponent<MonsterAppearance>(); // 외형 관리자
            go.AddComponent<NexusAI>(); // 인공지능 관리자
            go.AddComponent<NexusMovement>(); // 움직임 관리자
            go.AddComponent<NexusEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private CharacterActor CreatePlayerBot()
        {
            GameObject go = new GameObject(PLAYER_BOT) { hideFlags = HideFlags.HideAndDontSave };

            CharacterActor actor = go.AddComponent<CharacterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<CharacterAnimator>(); // 애니메이션 관리자
            go.AddComponent<CharacterAppearance>(); // 외형 관리자
            go.AddComponent<PlayerBotAI>(); // 인공지능 관리자
            go.AddComponent<CharacterMovement>(); // 움직임 관리자
            go.AddComponent<ObjectPicking>(); // Picking 관리자
            go.AddComponent<CharacterEffectPlayer>(); // 이펙트 관리자
            go.AddComponent<UnitRadar>(); // 유닛감지 관리자

            return actor;
        }

        private MonsterActor CreateMonsterBot()
        {
            GameObject go = new GameObject(MONSTER_BOT) { hideFlags = HideFlags.HideAndDontSave };

            MonsterActor actor = go.AddComponent<MonsterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<MonsterAnimator>(); // 애니메이션 관리자
            go.AddComponent<MonsterAppearance>(); // 외형 관리자
            go.AddComponent<MonsterBotAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<NormalMonsterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private NpcActor CreateNPC()
        {
            GameObject go = new GameObject(NPC) { hideFlags = HideFlags.HideAndDontSave };

            NpcActor actor = go.AddComponent<NpcActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<NpcAnimator>(); // 애니메이션 관리자
            go.AddComponent<NpcAppearance>(); // 외형 관리자
            go.AddComponent<NpcAI>(); // 인공지능 관리자
            go.AddComponent<UnitMovement>(); // 움직임 관리자
            go.AddComponent<NpcEffectPlayer>(); // 이펙트 관리자

            return actor;
        }

        private MonsterActor CreateTurret()
        {
            GameObject go = new GameObject(MONSTER_BOT) { hideFlags = HideFlags.HideAndDontSave };

            MonsterActor actor = go.AddComponent<MonsterActor>();
            DontDestroyOnLoad(actor); // 사라짐 방지

            go.AddComponent<MonsterAnimator>(); // 애니메이션 관리자
            go.AddComponent<MonsterAppearance>(); // 외형 관리자
            go.AddComponent<TurretAI>(); // 인공지능 관리자
            go.AddComponent<GuardianMovement>(); // 움직임 관리자
            go.AddComponent<MonsterEffectPlayer>(); // 이펙트 관리자

            return actor;
        }
    }
}