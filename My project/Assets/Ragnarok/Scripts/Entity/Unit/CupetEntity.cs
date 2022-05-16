using System.Collections.Generic;

namespace Ragnarok
{
    public abstract class CupetEntity : UnitEntity, IUIData
    {
        public CupetModel Cupet { get; private set; }

        /// <summary>
        /// 레이어 값
        /// </summary>
        public override int Layer => IsUI ? Ragnarok.Layer.UI_3D : Ragnarok.Layer.CUPET;

        private float respawnDelay;
        /// <summary>
        /// 리스폰 시간
        /// </summary>
        public float RespawnDelay => respawnDelay;

        private bool isCupetRespawnable;
        /// <summary>
        /// 큐펫 부활 여부
        /// </summary>
        public bool IsCupetRespawnable => isCupetRespawnable;

        /// <summary>
        /// Actor 생성
        /// </summary>
        protected override UnitActor SpawnEntityActor()
        {
            return unitActorPool.SpawnCupet();
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Cupet, Cupet.CupetID, Cupet.Level);
        }

        /// <summary>
        /// 유닛 스탯 세팅값 생성
        /// </summary>
        public override UnitEntitySettings CreateUnitSettings()
        {
            CupetData cupetData = Cupet.GetCupetData();

            if (cupetData == null)
                return null;

            return new UnitEntitySettings
            {
                unitSettings = new BattleUnitInfo.Settings
                {
                    id = cupetData.id, // 유닛 고유 아이디: CupetData 아이디
                    level = Cupet.Level, // 레벨: Level
                    unitElementType = cupetData.element_type.ToEnum<ElementType>(), // 유닛 고유 속성타입: 데이터
                    unitElementLevel = 0, // 유닛 고유 속성레벨: 0
                    unitSizeType = cupetData.cost.ToUnitSizeType(), // 유닛 사이즈타입: 데이터
                    unitMonsterType = MonsterType.None, // 유닛 몬스터타입: 없음
                    cognizanceDistance = 0f, // 타겟 인지거리: 없음 (플레이어의 타겟킹 = 큐펫의 타겟팅)
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
                    basicActiveSkillId = cupetData.basic_active_skill_id, // 평타 스킬id: 데이터
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
                    basicStr = Cupet.BasicStr,
                    basicAgi = Cupet.BasicAgi,
                    basicVit = Cupet.BasicVit,
                    basicInt = Cupet.BasicInt,
                    basicDex = Cupet.BasicDex,
                    basicLuk = Cupet.BasicLuk,

                    basicMoveSpd = cupetData.move_spd, // 기본이속: 데이터
                    basicAtkSpd = cupetData.atk_spd, // 기본공속: 데이터
                    basicAtkRange = cupetData.atk_range, // 기본사정거리: 데이터

                    hpCoefficient = cupetData.hp_coefficient, // 전체체력 계수: 데이터
                    atkCoefficient = cupetData.atk_coefficient, // 물리공격력 계수: 데이터
                    matkCoefficient = cupetData.matk_coefficient, // 마법공격력 계수: 데이터
                    defCoefficient = cupetData.def_coefficient, // 물리방어력 계수: 데이터
                    mdefCoefficient = cupetData.mdef_coefficient, // 마법방어력 계수: 데이터

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
            return Cupet.GetValidSkillList(); // 유효한 스킬 목록
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
            return Cupet.NameId;
        }

        /// <summary>
        /// 이름 반환
        /// </summary>
        public override string GetName()
        {
            return Cupet.Name;
        }

        /// <summary>
        /// 프리팹 이름 반환
        /// </summary>
        public override string GetPrefabName()
        {
            return Cupet.PrefabName;
        }

        /// <summary>
        /// 썸네일 아이콘 이름
        /// </summary>
        public override string GetProfileName()
        {
            return Cupet.ThumbnailName;
        }

        public override string GetThumbnailName()
        {
            return string.Empty; // 원형 썸네일 음슴
        }

        /// <summary>
        /// 큐펫 리스폰 가능 여부 설정
        /// </summary>
        public void SetCupetRespawnable(bool isRespawnable)
        {
            isCupetRespawnable = isRespawnable;
        }

        /// <summary>
        /// 특정 장비를 장착했을 때의 가상 전투력
        /// </summary>
        public int GetVirtualAttackPower(BattleStatusInfo status, EquipmentItemInfo equipment)
        {
            int AP = UnityEngine.Mathf.Max(status.MeleeAtk, status.RangedAtk) * 2 + status.Def + status.MDef + status.Hit + status.Flee - 200;
            return AP;
        }

        /// <summary>
        /// 큐펫 부활 딜레이 설정
        /// </summary>
        /// <param name="delay">(단위: 밀리초)</param>
        public void SetRespawnDelay(int delay)
        {
            respawnDelay = MathUtils.MillisecToSec(delay);
        }

        public class Factory
        {
            public static PlayerCupetEntity CreatePlayerCupet(int cupetID)
            {
                PlayerCupetEntity entity = new PlayerCupetEntity();
                entity.Cupet = UnitModel<CupetEntity>.Create<CupetModel>(entity);
                entity.SetRespawnDelay(BasisType.CUPET_COOL_TIME.GetInt());

                entity.Cupet.Initialize(cupetID, rank: 0, exp: 0, count: 0); // 큐펫 기본 세팅

                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 기본 정보만 가진 더미 큐펫 생성
            /// </summary>
            public static CupetEntity CreateDummyCupet(int cupetID, int rank, int level)
            {
                DummyCupetEntity entity = new DummyCupetEntity();
                entity.Cupet = UnitModel<CupetEntity>.Create<CupetModel>(entity);
                entity.Cupet.Initialize(cupetID, rank, level);

                return entity;
            }

            /// <summary>
            /// 멀티 전투 큐펫 생성
            /// </summary>
            public static MultiCupetEntity CreateMultiBattleCupet(int cupetID)
            {
                MultiCupetEntity entity = new MultiCupetEntity();
                entity.Cupet = UnitModel<CupetEntity>.Create<CupetModel>(entity);

                entity.Cupet.Initialize(cupetID, rank: 0, exp: 0, count: 0); // 큐펫 기본 세팅

                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 고스트 큐펫 생성
            /// </summary>
            public static GhostCupetEntity CreateGhostCupet()
            {
                GhostCupetEntity entity = new GhostCupetEntity();
                entity.Cupet = UnitModel<CupetEntity>.Create<CupetModel>(entity);

                entity.Initialize();

                return entity;
            }
        }
    }
}