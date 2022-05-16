using System.Collections.Generic;

namespace Ragnarok
{
    public abstract class CharacterEntity : UnitEntity
    {
        /******************** 공통 ********************/
        public UserModel User { get; protected set; }
        public CharacterModel Character { get; protected set; }
        public StatusModel Status { get; protected set; }
        public BuffItemListModel BuffItemList { get; protected set; }
        public SkillModel Skill { get; protected set; }
        public GuildModel Guild { get; protected set; }

        /******************** 플레이어 전용 ********************/
        public EventBuffModel EventBuff { get; protected set; }
        public GoodsModel Goods { get; protected set; }
        public CharacterListModel CharacterList { get; protected set; }
        public InventoryModel Inventory { get; protected set; }
        public CupetListModel CupetList { get; protected set; }
        public AchievementModel Achievement { get; protected set; }
        public DailyModel Daily { get; protected set; }
        public DungeonModel Dungeon { get; protected set; }
        public QuestModel Quest { get; protected set; }
        public FriendModel Friend { get; protected set; }
        public MailModel Mail { get; protected set; }
        public VIPModel VIP { get; protected set; }
        public MakeModel Make { get; protected set; }
        public AlarmModel AlarmModel { get; protected set; }
        public ShopModel ShopModel { get; protected set; }
        public ChatModel ChatModel { get; protected set; }
        public RankModel RankModel { get; protected set; }
        public TradeModel Trade { get; protected set; }
        public EventModel Event { get; protected set; }
        public LeagueModel League { get; protected set; }
        public AgentModel Agent { get; protected set; }
        public TutorialModel Tutorial { get; protected set; }
        public SharingModel Sharing { get; protected set; }
        public DuelModel Duel { get; protected set; }
        public BookModel Book { get; protected set; }
        public BingoModel Bingo { get; protected set; }

        /// <summary>
        /// 프론테라 축복 버프아이템
        /// </summary>
        protected static readonly BlessBuffItemInfo pronteraBlessBuff = new BlessBuffItemInfo();

        /// <summary>
        /// 레이어 값
        /// </summary>
        public override int Layer => IsUI ? Ragnarok.Layer.UI_3D : Ragnarok.Layer.PLAYER;

        public event System.Action OnUpdateSlotItem;

        /// <summary>
        /// 전투력 관련 정보
        /// </summary>
        public AttackPowerInfo attackPowerInfo;

        public CharacterEntity()
        {
            attackPowerInfo = new AttackPowerInfo(this);
        }

        public override void Initialize()
        {
            base.Initialize();

            if (Character != null)
            {
                Character.OnUpdateJobLevel += OnUpdateJobLevel;
                Character.OnChangedJob += OnChangedJob;
                Character.OnRebirth += ReloadStatus;
                Character.OnUpdateShareForceStatus += ReloadStatus;
            }

            if (Skill != null)
            {
                Skill.OnSkillInit += ReloadStatus;
                Skill.OnUpdateSkill += ReloadStatus;
                Skill.OnUpdateSkillSlot += ReloadStatus;
            }

            if (Inventory != null)
            {
                Inventory.OnUpdateItem += ReloadStatus;
            }

            if (Status != null)
            {
                Status.OnUpdateBasicStatus += ReloadStatus;
            }

            if (BuffItemList != null)
            {
                BuffItemList.OnUpdateBuff += ReloadStatus;
            }

            if (Guild != null)
            {
                Guild.OnUpdateGuildSkill += ReloadStatus;
            }

            if (Agent != null)
            {
                Agent.OnStatusReloadRequired += ReloadStatus;
            }

            if (Book != null)
            {
                Book.OnStatusReloadRequired += ReloadStatus;
            }

            DungeonModel.OnUpdateStageChapter += ReloadStatus;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (Character != null)
            {
                Character.OnUpdateJobLevel -= OnUpdateJobLevel;
                Character.OnChangedJob -= OnChangedJob;
                Character.OnRebirth -= ReloadStatus;
                Character.OnUpdateShareForceStatus -= ReloadStatus;
            }

            if (Skill != null)
            {
                Skill.OnSkillInit -= ReloadStatus;
                Skill.OnUpdateSkill -= ReloadStatus;
                Skill.OnUpdateSkillSlot -= ReloadStatus;
            }

            if (Inventory != null)
            {
                Inventory.OnUpdateItem -= ReloadStatus;
            }

            if (Status != null)
            {
                Status.OnUpdateBasicStatus -= ReloadStatus;
            }

            if (BuffItemList != null)
            {
                BuffItemList.OnUpdateBuff -= ReloadStatus;
            }

            if (Guild != null)
            {
                Guild.OnUpdateGuildSkill -= ReloadStatus;
            }

            if (Agent != null)
            {
                Agent.OnStatusReloadRequired -= ReloadStatus;
            }

            if (Book != null)
            {
                Book.OnStatusReloadRequired -= ReloadStatus;
            }

            DungeonModel.OnUpdateStageChapter -= ReloadStatus;
        }

        /// <summary>
        /// 레벨
        /// </summary>
        void OnUpdateJobLevel(int jobLevel)
        {
            ReloadStatus();
            SetCurrentHp(MaxHP);
        }

        /// <summary>
        /// 전직
        /// </summary>
        void OnChangedJob(bool isInit)
        {
            ReloadStatus();
        }

        /// <summary>
        /// 유닛 스탯 세팅값 생성
        /// </summary>
        public override UnitEntitySettings CreateUnitSettings()
        {
            if (Character == null)
                return null;

            if (Status == null)
                return null;

            JobData jobData = Character.GetJobData();
            if (jobData == null)
                return null;

            EquipmentClassType weaponType = GetWeaponType();
            ElementType weaponElementType = GetWeaponElementType();
            int weaponElementLevel = GetWeaponElementLevel();
            ElementType armorElementType = GetArmorElementType();
            int armorElementLevel = GetArmorElementLevel();

            return new UnitEntitySettings
            {
                unitSettings = new BattleUnitInfo.Settings
                {
                    id = jobData.id, // 유닛 고유 아이디: JobData 아이디
                    level = Character.JobLevel, // 레벨: JobLevel
                    unitElementType = armorElementType, // 유닛 고유 속성타입
                    unitElementLevel = armorElementLevel, // 유닛 고유 속성레벨: 
                    unitSizeType = default, // 유닛 사이즈타입: 없음
                    unitMonsterType = MonsterType.None, // 유닛 몬스터타입: 없음
                    cognizanceDistance = -1f, // 타겟 인지거리: 전체(-1f)
                },

                itemSettings = new BattleItemInfo.Settings
                {
                    weaponType = weaponType, // 현재 장착중인 무기 인덱스: WeaponIndex
                    weaponElementType = weaponElementType,
                    weaponElementLevel = weaponElementLevel,
                    equippedItems = GetEquippedItems(), // 현재 장착중인 장비 목록
                    serverTotalItemAtk = GetServerTotalItemAtk(), // 서버에서 받은 장착아이템으로 인한 Atk
                    serverTotalItemMatk = GetServerTotalItemMatk(), // 서버에서 받은 장착아이템으로 인한 Matk
                    serverTotalItemDef = GetServerTotalItemDef(), // 서버에서 받은 장착아이템으로 인한 Def
                    serverTotalItemMdef = GetServerTotalItemMdef(), // 서버에서 받은 장착아이템으로 인한 Mdef
                },

                skillSettings = new BattleSkillInfo.Settings
                {
                    basicActiveSkillId = GetBasicActiveSkillID(weaponType), // 평타 스킬id: 기초데이터 (무기에 따라 변경)
                    basicPassiveSkillId = 0, // 직업패시브 스킬id: 없음
                    skills = GetValidSkills(), // 유효한 스킬 목록
                    isAntiSkillAuto = IsAntiSkillAuto(), // 스킬 수동 여부
                },

                guildSkillSettings = new BattleGuildSkillInfo.Settings
                {
                    guildSkills = GetValidGuildSkills(), // 유효한 길드 스킬 목록
                },

                statusInput = new Battle.StatusInput
                {
                    basicStr = Status.BasicStr, // 기본Str: Str
                    basicAgi = Status.BasicAgi, // 기본Agi: Agi
                    basicVit = Status.BasicVit, // 기본Vit: Vit
                    basicInt = Status.BasicInt, // 기본Int: Int
                    basicDex = Status.BasicDex, // 기본Dex: Dex
                    basicLuk = Status.BasicLuk, // 기본Luk: Luk

                    basicMoveSpd = 10000, // 기본이속: 데이터
                    basicAtkSpd = 10000, // 기본공속: 데이터
                    basicAtkRange = 10000, // 기본사정거리: 데이터

                    hpCoefficient = jobData.hp_coefficient, // 전체체력 계수: 데이터
                    atkCoefficient = 1, // 물리공격력 계수: 1
                    matkCoefficient = 1, // 마법공격력 계수: 1
                    defCoefficient = 1, // 물리방어력 계수: 1
                    mdefCoefficient = 1, // 마법방어력 계수: 1

                    attackSpeedPenalty = jobData.GetAtkSpdPenalty(weaponType), // 공속패널티: 데이터 (무기에 따라 변경)
                },

                agentSettings = new BattleAgentOptionInfo.Settings
                {
                    agents = GetAllAgents(),
                },

                agentBookSettings = new BattleAgentBookOptionInfo.Settings
                {
                    enabledBooks = GetEnabledBookStates(),
                },

                bookSettings = new BattleBookOptionInfo.Settings
                {
                    battleOptions = GetBookOptions(),
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
        /// 전투력 반환
        /// </summary>
        /// <returns></returns>
        public int GetTotalAttackPower()
        {
            return attackPowerInfo.GetTotalAttackPower();
        }

        /// <summary>
        /// 이름
        /// </summary>
        public override string GetName()
        {
            return Character.Name;
        }

        /// <summary>
        /// 프리팹
        /// </summary>
        public override string GetPrefabName()
        {
            string jobName = Character.Job.ToString();
            return jobName.AddPostfix(Character.Gender);
        }

        /// <summary>
        /// 프로필 (네모)
        /// </summary>
        public override string GetProfileName()
        {
            return Character.GetProfileName();
        }

        /// <summary>
        /// 썸네일 (원형)
        /// </summary>
        public override string GetThumbnailName()
        {
            return Character.GetThumbnailName();
        }

        /// <summary>
        /// 현재 장착한 무기 타입
        /// </summary>
        public EquipmentClassType GetWeaponType()
        {
            if (Character is DummyCharacterModel dummyCharacter)
                return dummyCharacter.WeaponType;

            if (Inventory == null)
                return EquipmentClassType.OneHandedSword;

            // 현재 장착한 무기
            ItemInfo weapon = Inventory.GetItemInfo(ItemEquipmentSlotType.Weapon);

            // 캐릭터의 경우에는 장착한 무기가 없을 경우 한손검으로
            if (weapon == null)
                return EquipmentClassType.OneHandedSword;

            return weapon.ClassType;
        }

        private ElementType GetWeaponElementType()
        {
            if (Inventory == null)
                return ElementType.Neutral;

            ItemInfo weapon = Inventory.GetItemInfo(ItemEquipmentSlotType.Weapon);
            if (weapon == null)
                return ElementType.Neutral;

            return weapon.ElementType;
        }

        private int GetWeaponElementLevel()
        {
            if (Inventory == null)
                return 0;

            ItemInfo weapon = Inventory.GetItemInfo(ItemEquipmentSlotType.Weapon);
            if (weapon == null)
                return 0;

            return weapon.ElementLevel;
        }

        private ElementType GetArmorElementType()
        {
            if (Inventory == null)
                return ElementType.Neutral;

            ItemInfo armor = Inventory.GetItemInfo(ItemEquipmentSlotType.Armor);
            if (armor == null)
                return ElementType.Neutral;

            return armor.ElementType;
        }

        private int GetArmorElementLevel()
        {
            if (Inventory == null)
                return 0;

            ItemInfo armor = Inventory.GetItemInfo(ItemEquipmentSlotType.Armor);
            if (armor == null)
                return 0;

            return armor.ElementLevel;
        }

        /// <summary>
        /// 현재 장착중인 장비 아이템 리스트
        /// </summary>
        protected override ItemInfo[] GetEquippedItems()
        {
            if (Inventory == null)
                return null; // 없음

            // 장착아이템 옵션 포함 예외
            if (Status.IsExceptEquippedItems)
                return null; // 장착한 무기 아이템의 옵션이 들어가는 것을 방지한다

            return Inventory.GetEquippedItems();
        }

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Atk
        /// </summary>
        protected override int GetServerTotalItemAtk()
        {
            if (Inventory == null)
                return 0;

            // 장착아이템 옵션 포함 예외
            if (Status.IsExceptEquippedItems)
                return Inventory.ServerTotalItemAtk; // 서버에서 받은 값으로 처리

            return 0;
        }

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Matk
        /// </summary>
        protected override int GetServerTotalItemMatk()
        {
            if (Inventory == null)
                return 0;

            // 장착아이템 옵션 포함 예외
            if (Status.IsExceptEquippedItems)
                return Inventory.ServerTotalItemMatk; // 서버에서 받은 값으로 처리

            return 0;
        }

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Def
        /// </summary>
        protected override int GetServerTotalItemDef()
        {
            if (Inventory == null)
                return 0;

            // 장착아이템 옵션 포함 예외
            if (Status.IsExceptEquippedItems)
                return Inventory.ServerTotalItemDef; // 서버에서 받은 값으로 처리

            return 0;
        }

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Mdef
        /// </summary>
        protected override int GetServerTotalItemMdef()
        {
            if (Inventory == null)
                return 0;

            // 장착아이템 옵션 포함 예외
            if (Status.IsExceptEquippedItems)
                return Inventory.ServerTotalItemMdef; // 서버에서 받은 값으로 처리

            return 0;
        }

        /// <summary>
        /// 장착한 무기에 유효한 스킬 리스트
        /// </summary>
        protected override SkillInfo[] GetValidSkills()
        {
            EquipmentClassType weaponType = GetWeaponType();
            return Skill?.GetValidSkills(weaponType);
        }

        /// <summary>
        /// 스킬 수동 여부
        /// </summary>
        protected override bool IsAntiSkillAuto()
        {
            return Skill == null ? false : Skill.IsAntiSkillAuto;
        }

        /// <summary>
        /// 길드 스킬 목록
        /// </summary>
        public override SkillInfo[] GetValidGuildSkills()
        {
            EquipmentClassType weaponType = GetWeaponType();
            return Guild?.GetValidSkills(weaponType);
        }

        /// <summary>
        /// 현재 받고 있는 버프 효과
        /// </summary>
        protected override BattleBuffItemInfo.ISettings[] GetArrayBuffItemSettings()
        {
            return BuffItemList?.GetBuffItemInfos();
        }

        protected override EventBuffInfo[] GetArrayEventBuffInfos()
        {
            return EventBuff?.GetEventBuffList();
        }

        protected override IEnumerable<BlessBuffItemInfo> GetBlssBuffItems()
        {
            if (DungeonModel.StageChapter == 1)
                yield return pronteraBlessBuff;
        }

        /// <summary>
        /// 동료 보유 효과
        /// </summary>
        protected override IEnumerable<IAgent> GetAllAgents()
        {
            return Agent?.GetAllAgents();
        }

        /// <summary>
        /// 동료 도감 효과
        /// </summary>
        protected override IEnumerable<AgentBookState> GetEnabledBookStates()
        {
            return Agent?.GetEnabledBookStates();
        }

        protected override IEnumerable<BattleOption> GetBookOptions()
        {
            return Book?.BattleOptions;
        }

        /// <summary>
        /// 쉐어포스 효과
        /// </summary>
        protected override IEnumerable<ShareStatBuildUpData> GetShareForceData()
        {
            return Character?.GetShareForceData();
        }

        /// <summary>
        /// 서버에서 받은 전옵타
        /// </summary>
        protected override BattleOption[] GetServerBattleOptions()
        {
            return Status.GetServerBattleOptions();
        }

        /// <summary>
        /// 서버에서 받은 전옵타 (길드)
        /// </summary>
        protected override BattleOption[] GetServerGuildBattleOptions()
        {
            return Status.GetServerGuildBattleOptions();
        }

        /// <summary>
        /// 무기 프리팹 이름
        /// </summary>
        public string GetWeaponPrefabName()
        {
            if (Character is DummyCharacterModel dummyCharacter)
                return dummyCharacter.WeaponPrefabName;

            if (Inventory == null)
                return Constants.Appearance.EMPTY_WEAPON_PREFAB_NAME;

            // 현재 장착한 무기
            ItemInfo weapon = Inventory.GetItemInfo(ItemEquipmentSlotType.Weapon);

            // 캐릭터의 경우에는 장착한 무기가 없을 경우 한손검으로
            if (weapon == null)
                return Constants.Appearance.EMPTY_WEAPON_PREFAB_NAME;

            return weapon.PrefabName;
        }

        /// <summary>
        /// 코스튬 프리팹 이름
        /// </summary>
        /// <returns></returns>
        public string GetCostumePrefabName(ItemEquipmentSlotType slotType)
        {
            if (Character is DummyCharacterModel dummyCharacter)
            {
                switch (slotType)
                {
                    case ItemEquipmentSlotType.CostumeWeapon:
                        return dummyCharacter.CostumeWeaponPrefabName;

                    case ItemEquipmentSlotType.CostumeHat:
                        return dummyCharacter.CostumeHatPrefabName;

                    case ItemEquipmentSlotType.CostumeFace:
                        return dummyCharacter.CostumeFacePrefabName;

                    case ItemEquipmentSlotType.CostumeCape:
                        return dummyCharacter.CostumeCapePrefabName;

                    case ItemEquipmentSlotType.CostumePet:
                        return dummyCharacter.CostumePetPrefabName;

                    case ItemEquipmentSlotType.CostumeBody:
                        return dummyCharacter.CostumeBodyPrefabName;
                }
                return string.Empty;
            }

            if (Inventory == null)
                return string.Empty;

            // 현재 장착한 코스튬
            ItemInfo item = Inventory.GetItemInfo(slotType);

            // 캐릭터의 경우에는 장착한 코스튬이 없을 경우
            if (item == null)
                return string.Empty;

            return item.PrefabName;
        }

        /// <summary>
        /// 장착중인 모자 코스튬 오프셋 위치 데이터
        /// </summary>
        /// <returns></returns>
        public CostumeData GetCostumeData()
        {
            if (Character is DummyCharacterModel dummyCharacter)
                return dummyCharacter.GetCostumeData();

            // 현재 장착한 코스튬
            ItemInfo item = Inventory.GetItemInfo(ItemEquipmentSlotType.CostumeHat);

            // 캐릭터의 경우에는 장착한 코스튬이 없을 경우
            if (item == null)
                return null;

            return Inventory.GetCostumeData(item.CostumeDataId);
        }

        /// <summary>
        /// 장착한 무기 코스튬 타입
        /// </summary>
        /// <returns></returns>
        public CostumeType GetCostumeWeaponType()
        {
            if (Character is DummyCharacterModel dummyCharacter)
                return dummyCharacter.CostumeWeaponType;

            // 현재 장착한 코스튬
            ItemInfo item = Inventory.GetItemInfo(ItemEquipmentSlotType.CostumeWeapon);

            // 캐릭터의 경우에는 장착한 코스튬이 없을 경우
            if (item == null)
                return CostumeType.OneHandedSword;

            return item.CostumeType;
        }

        /// <summary>
        /// 착용한 아이템이 이펙트 이름
        /// </summary>
        /// <param name="slotType"></param>
        /// <returns></returns>
        public string GetEffectName(ItemEquipmentSlotType slotType)
        {
            // 더미 캐릭터 (UI에서만 사용)
            // UI 에서는 이펙트를 생성하지 않는다
            if (Character is DummyCharacterModel)
                return string.Empty;

            // 현재 장착한 아이템
            ItemInfo item = Inventory.GetItemInfo(slotType);

            // 캐릭터의 경우에는 장착한 아이템이 없을 경우
            if (item == null)
                return string.Empty;

            return item.EffectName.Equals("0") ? string.Empty : item.EffectName;
        }

        /// <summary>
        /// 평타 스킬
        /// </summary>
        private int GetBasicActiveSkillID(EquipmentClassType weaponType)
        {
            BattleItemIndex index = weaponType.ToBattleItemIndex();
            return BasisType.REF_BASIS_ACTIVE_SKILL_ID.GetInt((int)index);
        }

        protected override bool IsNeedSaveDamagePacket()
        {
            return true;
        }

        /// <summary>
        /// 서버로부터 BattleScore 받음
        /// </summary>
        public virtual void UpdateBattleScore(int? battleScore)
        {
        }

        /// <summary>
        /// Actor 생성
        /// </summary>
        protected override UnitActor SpawnEntityActor()
        {
            return unitActorPool.SpawnCharacter();
        }

        public override void SetState(UnitState state)
        {
            base.SetState(state);

            if (CupetList)
                CupetList.SetState(state); // 보유하고 있는 큐펫까지 스테이트를 변경시켜준다
        }

        public override void ResetSkillCooldown()
        {
            base.ResetSkillCooldown();

            Skill?.ResetCooldown();
        }

        /// <summary>
        /// 착용중인 칭호 이름
        /// </summary>
        /// <returns></returns>
        public string GetTitleName()
        {
            if (Character is DummyCharacterModel dummyCharacter)
            {
                if (dummyCharacter.CostumeTitleId == 0)
                    return string.Empty;

                return dummyCharacter.CostumeTitleId.ToText();
            }

            if (Inventory == null)
                return string.Empty;

            // 현재 장착한 코스튬
            ItemInfo item = Inventory.GetItemInfo(ItemEquipmentSlotType.CostumeTitle);

            // 캐릭터의 경우에는 장착한 코스튬이 없을 경우
            if (item == null)
                return string.Empty;

            if (item.CostumeTitleID == 0)
                return string.Empty;

            return item.CostumeTitleID.ToText();
        }

        /// <summary>
        /// 캐릭터 성별
        /// </summary>
        public Gender GetGender()
        {
            if (Character is DummyCharacterModel dummyCharacter)
                return dummyCharacter.Gender;

            return Character.Gender;
        }

        public CostumeBodyType GetCostumeBodyType()
        {
            if (Character is DummyCharacterModel dummyCharacter)
                return dummyCharacter.CostumeBodyType;

            // 현재 장착한 코스튬
            ItemInfo item = Inventory.GetItemInfo(ItemEquipmentSlotType.CostumeBody);

            // 캐릭터의 경우에는 장착한 코스튬이 없을 경우
            if (item == null)
                return CostumeBodyType.Unisex;

            return item.CostumeBodyType;
        }

        protected T Create<T>()
            where T : CharacterEntityModel, new()
        {
            T info = new T();
            info.Initialize(this);
            return info;
        }

        public class Factory
        {
            /// <summary>
            /// 멀티 플레이어 생성
            /// </summary>
            public static MultiPlayerEntity CreateMultiPlayer()
            {
                MultiPlayerEntity entity = new MultiPlayerEntity();
                entity.SetState(UnitState.Stage);
                entity.Character = CharacterEntityModel.Create<DummyCharacterModel>(entity);
                entity.Character.SetJob(Job.Novice); // ReloadStatus 하면서 Job이 0이라서 에러가 난다.. 
                entity.Status = CharacterEntityModel.Create<StatusModel>(entity);
                entity.Trade = CharacterEntityModel.Create<TradeModel>(entity);
                entity.Guild = CharacterEntityModel.Create<GuildModel>(entity); // Lobby 전용 (길드표시)

                entity.Initialize();

                entity.ReloadStatus(); // 이동속도 지정을 위해 

                return entity;
            }

            /// <summary>
            /// 플레이어 클론 생성
            /// </summary>
            public static PlayerCloneEntity CreatePlayerClone(UnitState unitState = UnitState.Stage)
            {
                PlayerCloneEntity entity = new PlayerCloneEntity();
                entity.SetState(unitState);

                entity.Character = CharacterEntityModel.Create<CharacterModel>(entity);
                entity.Status = CharacterEntityModel.Create<StatusModel>(entity);
                entity.Skill = CharacterEntityModel.Create<SkillModel>(entity);
                entity.Inventory = CharacterEntityModel.Create<InventoryModel>(entity);

                if (unitState == UnitState.Stage)
                    entity.CupetList = CharacterEntityModel.Create<CupetListModel>(entity);

                entity.Guild = CharacterEntityModel.Create<GuildModel>(entity);
                entity.Trade = CharacterEntityModel.Create<TradeModel>(entity);

                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 멀티 전투 플레이어 생성
            /// </summary>
            public static MultiPlayerEntity CreateMultiBattlePlayer(UnitState unitState = UnitState.Stage)
            {
                MultiPlayerEntity entity = new MultiPlayerEntity();
                entity.SetState(unitState);

                entity.Character = CharacterEntityModel.Create<CharacterModel>(entity);
                entity.Status = CharacterEntityModel.Create<StatusModel>(entity);
                entity.Skill = CharacterEntityModel.Create<SkillModel>(entity);
                entity.Inventory = CharacterEntityModel.Create<InventoryModel>(entity);

                if (unitState == UnitState.Stage)
                    entity.CupetList = CharacterEntityModel.Create<CupetListModel>(entity);

                entity.Guild = CharacterEntityModel.Create<GuildModel>(entity);
                entity.Trade = CharacterEntityModel.Create<TradeModel>(entity);

                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 고스트 플레이어 생성
            /// </summary>
            public static GhostPlayerEntity CreateGhostPlayer()
            {
                GhostPlayerEntity entity = new GhostPlayerEntity();
                entity.SetState(UnitState.Stage);
                entity.Character = CharacterEntityModel.Create<CharacterModel>(entity);
                entity.Status = CharacterEntityModel.Create<StatusModel>(entity);
                entity.Skill = CharacterEntityModel.Create<SkillModel>(entity);
                entity.Inventory = CharacterEntityModel.Create<InventoryModel>(entity);
                entity.Guild = CharacterEntityModel.Create<GuildModel>(entity);
                entity.Trade = CharacterEntityModel.Create<TradeModel>(entity);

                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 멀티 전투 플레이어 생성
            /// </summary>
            [System.Obsolete]
            public static DummyMultiPlayerEntity CreateDummyMultiBattlePlayer()
            {
                DummyMultiPlayerEntity entity = new DummyMultiPlayerEntity();
                entity.SetState(UnitState.Stage);
                entity.Character = CharacterEntityModel.Create<CharacterModel>(entity);
                entity.Status = CharacterEntityModel.Create<StatusModel>(entity);
                entity.Skill = CharacterEntityModel.Create<SkillModel>(entity);
                entity.Inventory = CharacterEntityModel.Create<InventoryModel>(entity);
                entity.CupetList = CharacterEntityModel.Create<CupetListModel>(entity);
                entity.Guild = CharacterEntityModel.Create<GuildModel>(entity);
                entity.Trade = CharacterEntityModel.Create<TradeModel>(entity);

                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 특정 정보의 UI 플레이어 생성
            /// </summary>
            /// <param name="isBookPreview">도감 미리보기 유무(도감 전용의 경우에는 코스튬이 무기에 관계없이 그대로 보여주어야 한다.)</param>
            /// <returns></returns>
            public static CharacterEntity CreateDummyUIPlayer(bool isBookPreview)
            {
                // 도감 전용의 경우에는 코스튬이 무기에 관계없이 그대로 보여주어야 한다.
                CharacterEntity entity = isBookPreview ? new BookDummyCharacterEntity() : new DummyCharacterEntity();
                entity.Character = CharacterEntityModel.Create<DummyCharacterModel>(entity);

                entity.Initialize();

                return entity;
            }

            /// <summary>
            /// 플레이어 봇 생성
            /// </summary>
            public static PlayerBotEntity CreatePlayerBot()
            {
                PlayerBotEntity entity = new PlayerBotEntity();
                entity.SetState(UnitState.Stage);
                entity.Character = CharacterEntityModel.Create<CharacterModel>(entity);
                entity.Status = CharacterEntityModel.Create<StatusModel>(entity);
                entity.Skill = CharacterEntityModel.Create<SkillModel>(entity);
                entity.Inventory = CharacterEntityModel.Create<InventoryModel>(entity);
                entity.Guild = CharacterEntityModel.Create<GuildModel>(entity);
                entity.Trade = CharacterEntityModel.Create<TradeModel>(entity);

                entity.Initialize();

                return entity;
            }
        }
    }
}