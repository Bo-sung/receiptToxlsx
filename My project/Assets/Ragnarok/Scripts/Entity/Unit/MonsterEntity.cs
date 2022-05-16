using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public abstract class MonsterEntity : UnitEntity
    {
        public MonsterModel Monster { get; private set; }

        /// <summary>
        /// 레이어 값
        /// </summary>
		public override int Layer => IsUI ? Ragnarok.Layer.UI_3D : Ragnarok.Layer.ENEMY;

        /// <summary>
        /// 스케일
        /// </summary>
        protected float scale;

        /// <summary>
        /// [미로] 미로용 몬스터 입장 스테이지
        /// </summary>
        public int stageId { get; private set; }

        /// <summary>
        /// [미로] 몬스터 배틀 타입
        /// </summary>
        public MazeBattleType battleType;

        /// <summary>
        /// 보스가 소환한 몬스터인가?
        /// </summary>
        public bool IsBossSpawnMonster { get; private set; }

        /// <summary>
        /// 중앙실험실 몬스터 고유 아이디
        /// </summary>
        public int CentralLabMonId { get; private set; }

        /// <summary>
        /// Actor 생성
        /// </summary>
        protected override UnitActor SpawnEntityActor()
        {
            UnitActor actor = unitActorPool.SpawnMonster();
            actor.CachedTransform.localScale = Vector3.one * scale; // 스케일 적용
            return actor;
        }

        /// <summary>
        /// 유닛 스탯 세팅값 생성
        /// </summary>
        public override UnitEntitySettings CreateUnitSettings()
        {
            MonsterData monsterData = Monster.GetMonsterData();
            if (monsterData == null)
                return null;

            float sensing = monsterData.sensing > 0f ? monsterData.sensing * 0.01f : monsterData.sensing; // 타겟 인지거리: 데이터

            return new UnitEntitySettings
            {
                unitSettings = new BattleUnitInfo.Settings
                {
                    id = monsterData.id, // 유닛 고유 아이디: MonsterData 아이디
                    level = Monster.MonsterLevel, // 레벨: StageData 또는 DungeonData 참조
                    unitElementType = monsterData.element_type.ToEnum<ElementType>(), // 유닛 고유 속성타입: 데이터
                    unitElementLevel = 0, // 유닛 고유 속성레벨: 0
                    unitSizeType = UnitSizeTypeExtensions.ToUnitSizeType(monsterData.cost), // 유닛 사이즈타입: 데이터
                    unitMonsterType = GetMonsterType(), // 유닛 몬스터타입: 데이터
                    cognizanceDistance = sensing, // 타겟 인지거리: 데이터
                },

                itemSettings = new BattleItemInfo.Settings
                {
                    weaponType = default, // 현재 장착중인 무기 인덱스: 없음
                    weaponElementType = ElementType.Neutral, // 무기 속성 : 무속성
                    weaponElementLevel = 0, // 무기 속성레벨: 0
                    equippedItems = GetEquippedItems(), // 현재 장착중인 장비 목록
                    serverTotalItemAtk = GetServerTotalItemAtk(), // 서버에서 받은 장착아이템으로 인한 Atk
                    serverTotalItemMatk = GetServerTotalItemMatk(), // 서버에서 받은 장착아이템으로 인한 Matk
                    serverTotalItemDef = GetServerTotalItemDef(), // 서버에서 받은 장착아이템으로 인한 Def
                    serverTotalItemMdef = GetServerTotalItemMdef(), // 서버에서 받은 장착아이템으로 인한 Mdef
                },

                skillSettings = new BattleSkillInfo.Settings
                {
                    basicActiveSkillId = monsterData.basic_active_skill_id, // 평타 스킬id: 데이터
                    basicPassiveSkillId = 0, // 직업패시브 스킬id: 없음
                    skills = GetValidSkills(), // 유효한 스킬 목록
                    isAntiSkillAuto = IsAntiSkillAuto(), // 스킬 수동 여부
                },

                guildSkillSettings = new BattleGuildSkillInfo.Settings
                {
                    guildSkills = GetValidGuildSkills(), // 유요한 길드 스킬 목록
                },

                statusInput = new Battle.StatusInput
                {
                    basicStr = monsterData.base_str, // 기본Str: 데이터
                    basicAgi = monsterData.base_agi, // 기본Agi: 데이터
                    basicVit = monsterData.base_vit, // 기본Vit: 데이터
                    basicInt = monsterData.base_int, // 기본Int: 데이터
                    basicDex = monsterData.base_dex, // 기본Dex: 데이터
                    basicLuk = monsterData.base_luk, // 기본Luk: 데이터

                    basicMoveSpd = monsterData.move_spd, // 기본이속: 데이터
                    basicAtkSpd = monsterData.atk_spd, // 기본공속: 데이터
                    basicAtkRange = monsterData.atk_range, // 기본사정거리: 데이터

                    hpCoefficient = monsterData.hp_coefficient, // 전체체력 계수: 데이터
                    atkCoefficient = monsterData.atk_coefficient, // 물리공격력 계수: 데이터
                    matkCoefficient = monsterData.matk_coefficient, // 마법공격력 계수: 데이터
                    defCoefficient = monsterData.def_coefficient, // 물리방어력 계수: 데이터
                    mdefCoefficient = monsterData.mdef_coefficient, // 마법방어력 계수: 데이터

                    attackSpeedPenalty = 0, // 공속패널티: 없음
                },

                agentSettings = new BattleAgentOptionInfo.Settings
                {
                    agents = GetAllAgents(),
                },

                agentBookSettings = new BattleAgentBookOptionInfo.Settings
                {
                    enabledBooks = GetEnabledBookStates(),
                },

                shareForceSettings = new BattleShareForceStatusOptionInfo.Settings
                {
                    shareForces = GetShareForceData(),
                },

                serverBattleOptions = GetServerBattleOptions(), // 서버에서 받은 전옵타
                serverGuildBattleOptions = GetServerGuildBattleOptions(), // 서버에서 받은 전옵타 (길드)
            };
        }

        protected virtual MonsterType GetMonsterType()
        {
            return MonsterType.Normal;
        }

        /// <summary>
        /// 현재 장착중인 장비 아이템 리스트
        /// </summary>
        protected override ItemInfo[] GetEquippedItems()
        {
            return null; // 없음
        }

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Atk
        /// </summary>
        protected override int GetServerTotalItemAtk()
        {
            return 0;
        }

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Matk
        /// </summary>
        protected override int GetServerTotalItemMatk()
        {
            return 0;
        }

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Def
        /// </summary>
        protected override int GetServerTotalItemDef()
        {
            return 0;
        }

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Mdef
        /// </summary>
        protected override int GetServerTotalItemMdef()
        {
            return 0;
        }

        /// <summary>
        /// 유효한 스킬 목록
        /// </summary>
        protected override SkillInfo[] GetValidSkills()
        {
            return Monster.GetValidSkillList(IsAngry); // 유효한 스킬 목록
        }

        /// <summary>
        /// 길드 스킬 목록
        /// </summary>
        public override SkillInfo[] GetValidGuildSkills()
        {
            return null;
        }

        /// <summary>
        /// 현재 받고 있는 버프 효과
        /// </summary>
        protected override BattleBuffItemInfo.ISettings[] GetArrayBuffItemSettings()
        {
            return null;
        }

        protected override EventBuffInfo[] GetArrayEventBuffInfos()
        {
            return null;
        }

        protected override IEnumerable<BlessBuffItemInfo> GetBlssBuffItems()
        {
            return null;
        }

        /// <summary>
        /// 동료 보유 효과
        /// </summary>
        protected override IEnumerable<IAgent> GetAllAgents()
        {
            return null;
        }

        /// <summary>
        /// 동료 도감 효과
        /// </summary>
        protected override IEnumerable<AgentBookState> GetEnabledBookStates()
        {
            return null;
        }

        /// <summary>
        /// 쉐어포스 스탯 효과
        /// </summary>
        protected override IEnumerable<ShareStatBuildUpData> GetShareForceData()
        {
            return null;
        }

        /// <summary>
        /// 서버에서 받은 전옵타
        /// </summary>
        protected override BattleOption[] GetServerBattleOptions()
        {
            return null;
        }

        /// <summary>
        /// 서버에서 받은 전옵타 (길드)
        /// </summary>
        protected override BattleOption[] GetServerGuildBattleOptions()
        {
            return null;
        }

        /// <summary>
        /// 이름 id 반환
        /// </summary>
        public override int GetNameId()
        {
            MonsterData data = Monster.GetMonsterData();
            if (data == null)
                return 0;

            return data.name_id;
        }

        /// <summary>
        /// 이름 반환
        /// </summary>
        public override string GetName()
        {
            MonsterData data = Monster.GetMonsterData();
            if (data == null)
                return string.Empty;

            return data.name_id.ToText();
        }

        /// <summary>
        /// 프리팹 이름 반환
        /// </summary>
        public override string GetPrefabName()
        {
            MonsterData data = Monster.GetMonsterData();

            string prefabName = data.prefab_name;
            if (string.IsNullOrEmpty(prefabName))
            {
                Debug.LogError($"프리팹 이름 음슴: id = {data.id}");
                return string.Empty;
            }

            return prefabName;
        }

        public override string GetProfileName()
        {
            MonsterData data = Monster.GetMonsterData();

            string thumbnailName = data.icon_name;
            if (string.IsNullOrEmpty(thumbnailName))
            {
                Debug.LogError($"썸네일 이름 음슴: id = {data.id}");
                return string.Empty;
            }

            return thumbnailName;
        }

        public override string GetThumbnailName()
        {
            return string.Empty; // 원형 썸네일 음슴
        }

        public Vector3 GetHudOffset()
        {
            MonsterData data = Monster.GetMonsterData();
            float yOffset = data == null ? 0f : data.GetHudOffset();
            return Vector3.up * yOffset;
        }

        public void SetStateId(int id)
        {
            stageId = id;
        }

        public void SetIsBossSpawnMonster(bool value)
        {
            IsBossSpawnMonster = value;
        }

        public void SetCentralLabId(int centralLabMonId)
        {
            CentralLabMonId = centralLabMonId;
        }

        public new class Factory
        {
            /// <summary>
            /// 몬스터 생성
            /// </summary>
            public static MonsterEntity CreateMonster(ISpawnMonster spawnMonster)
            {
                return CreateMonster(spawnMonster.Type, spawnMonster.Id, spawnMonster.Level, spawnMonster.Scale);
            }

            /// <summary>
            /// 몬스터 생성
            /// </summary>
            public static MonsterEntity CreateMonster(MonsterType monsterType, int monsterId, int monsterLevel, float scale)
            {
                MonsterEntity entity = null;

                if (monsterType == MonsterType.Normal)
                {
                    entity = new NormalMonsterEntity();
                }
                else if (monsterType == MonsterType.Boss)
                {
                    entity = new BossMonsterEntity();
                }

                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Monster.Initialize(monsterId, monsterLevel); // 몬스터 세팅
                entity.scale = scale;
                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 몬스터 생성
            /// </summary>
            public static MonsterEntity CreateMvpMonster(ISpawnMonster spawnMonster)
            {
                MonsterEntity entity = new MvpMonsterEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);
                entity.Monster.Initialize(spawnMonster.Id, spawnMonster.Level); // 몬스터 세팅
                entity.scale = spawnMonster.Scale;
                entity.Initialize();
                return entity;
            }

            /// <summary>
            /// 수호자 생성
            /// </summary>
            public static GuardianEntity CreateGuardian(int monsterId, int monsterLevel)
            {
                GuardianEntity entity = new GuardianEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Monster.Initialize(monsterId, monsterLevel); // 몬스터 세팅
                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 수호파괴자 생성
            /// </summary>
            public static GuardianDestroyerEntity CreateGuardianDestroyer(int monsterId, int monsterLevel)
            {
                GuardianDestroyerEntity entity = new GuardianDestroyerEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Monster.Initialize(monsterId, monsterLevel); // 몬스터 세팅
                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 월드보스 생성
            /// </summary>
            public static WorldBossEntity CreateWorldBoss(ISpawnMonster spawnMonster)
            {
                WorldBossEntity entity = new WorldBossEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Monster.Initialize(spawnMonster); // 몬스터 세팅
                entity.scale = spawnMonster.Scale;
                entity.Initialize();

                return entity;
            }

            public static MazeMonsterEntity CreateMazeMonster(int stageId, IBossMonsterSpawnData spawnMonster)
            {
                MazeMonsterEntity entity = new MazeMonsterEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Monster.Initialize(spawnMonster.BossMonsterId, spawnMonster.Level); // 몬스터 세팅
                entity.stageId = stageId;
                entity.Initialize();

                return entity;
            }

            public static MazeMonsterEntity CreateMazeMonster(int monsterId, int monsterLevel)
            {
                MazeMonsterEntity entity = new MazeMonsterEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Monster.Initialize(monsterId, monsterLevel); // 몬스터 세팅                
                entity.Initialize();

                return entity;
            }

            public static BonusMazeMonsterEntity CreateBonusMazeMonster(int monsterId, int monsterLevel)
            {
                BonusMazeMonsterEntity entity = new BonusMazeMonsterEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Monster.Initialize(monsterId, monsterLevel); // 몬스터 세팅
                entity.Initialize();

                return entity;
            }

            [System.Obsolete("미로던전용 임시")]
            public static MazeMonsterEntity CreateMazeMonster(DungeonType dungeonType)
            {
                const int MONSTER_ID = 50001;
                const int MONSTER_LEVEL = 10;

                MazeMonsterEntity entity = new MazeMonsterEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);
                entity.Monster.Initialize(MONSTER_ID, MONSTER_LEVEL);
                entity.Initialize();

                return entity;
            }

            public static MazeMonsterEntity CreateMazeMonster()
            {
                MazeMonsterEntity entity = new MazeMonsterEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Initialize();

                return entity;
            }

            public static NexusEntity CreateNexus(int teamIndex, int maxHp)
            {
                NexusEntity entity = new NexusEntity();
                entity.SetState(UnitState.GVG);
                entity.TeamIndex = teamIndex;
                entity.FixedMaxHp = maxHp;

                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Monster.Initialize(Constants.Monster.GuildMaze.NEXUS_MONSTER_ID, 1); // 몬스터 세팅
                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 멀티 몬스터 생성
            /// </summary>
            public static MonsterBotEntity CreateMonsterBot()
            {
                MonsterBotEntity entity = new MonsterBotEntity();

                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 포탑 생성
            /// </summary>
            public static TurretEntity CreateTurret(int id, int level)
            {
                TurretEntity entity = new TurretEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Monster.Initialize(id, level); // 몬스터 세팅
                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 포탑 생성
            /// </summary>
            public static TurretBossEntity CreateTurretBoss(int id, int level)
            {
                TurretBossEntity entity = new TurretBossEntity();
                entity.Monster = UnitModel<MonsterEntity>.Create<MonsterModel>(entity);

                entity.Monster.Initialize(id, level); // 몬스터 세팅
                entity.Initialize();

                return entity;
            }
        }
    }
}