#if UNITY_EDITOR
#define SHOW_DETAIL_LOG
#endif

using CodeStage.AntiCheat.ObscuredTypes;
using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="CharacterEntity"/>
    /// <see cref="MonsterEntity"/>
    /// <see cref="CupetEntity"/>
    /// </summary>
    public abstract class UnitEntity : Entity, IInfo
    {
        private const string TAG = nameof(BattleManager);

        private static int uniqueIndex;
        public readonly static Dictionary<int, UnitEntity> entityDic = new Dictionary<int, UnitEntity>(IntEqualityComparer.Default);
        public readonly int clientUID; // 클라이언트 고유 아이디

        public class UnitEntitySettings
        {
            public BattleUnitInfo.Settings unitSettings;
            public BattleItemInfo.Settings itemSettings;
            public BattleSkillInfo.Settings skillSettings;
            public BattleGuildSkillInfo.Settings guildSkillSettings;
            public BattleAgentOptionInfo.Settings agentSettings;
            public BattleAgentBookOptionInfo.Settings agentBookSettings;
            public BattleBookOptionInfo.Settings bookSettings;
            public BattleShareForceStatusOptionInfo.Settings shareForceSettings;
            public Battle.StatusInput statusInput;
            public BattleOption[] serverBattleOptions; // 전옵타 합산 - 길드 제외 (멀티플레이어 전용)
            public BattleOption[] serverGuildBattleOptions; // 전옵타 합산 = 길드 (멀티플레이어 전용)
        }

        public enum UnitState
        {
            Stage = 0,
            Maze = 1,
            //Lobby = 2,
            GVG = 3, // 길드미로 플레이어
            GVGMultiPlayer = 4, // 길드미로 다른 플레이어
            TempStagePlayer = 5, // TempStageEntry 에서 사용하는 임시 코드.. (플레이어)
            TempStagePlayerOther = 6 // TempStageEntry 에서 사용하는 임시 코드.. (다른 플레이어)
        }

        private const int MAZE_MONSTER_MOVE_SPEED_DOWN_RATE = 3000; // 미로 이동 속도 감소율
        private const int MAZE_MONSTER_MOVE_SPEED_UP_RATE = 6000; // 미로 이동 속도 증가율

        public delegate void HitEvent(UnitEntity unit, UnitEntity attacker, int value, int count, bool isCritical, bool isBasicActiveSkill, ElementType elementType, int elementFactor);
        public delegate void DamagedEvent(UnitEntity attacker, SkillType skillType, ElementType elementType, bool hasDamage, bool isChainableSkill);
        public delegate void AttackedEvent(UnitEntity target, SkillType skillType, int skillId, ElementType elementType, bool hasDamage, bool isChainableSkill);
        public delegate void DotDamageEvent(UnitEntity unit, CrowdControlType type, int dotDamage);
        public delegate void DieEvent(UnitEntity unit, UnitEntity attacker);
        public delegate void ChangeHPEvent(int current, int max);
        public delegate void ReloadStatusEvent();
        public delegate void ReadyToBattleEvent();
        public delegate void MoveSpeedRateEvent(float speed);
        public delegate void AttackSpeedRateEvent(float speed);
        public delegate void FleeEvent(UnitEntity attacker);
        public delegate void AutoGuardEvent();
        public delegate void RecoveryHpEvent(int value, int count);
        public delegate void HitComboEvent(int combo);
        public delegate void DamageValueEvent(UnitEntity unit, UnitEntity attacker, int damage);
        public delegate void UseSkillEvent(UnitEntity target, SkillInfo skillInfo); // 스킬 시작하는 시점에 호출
        public delegate void ApplySkillEvent(UnitEntity[] targets, SkillInfo skillInfo); // 스킬 대미지가 적용되는 시점에 호출
        public delegate void RebirthEvent();

        /// <summary>
        /// 듀얼 레벨 감소를 적용시킬 레벨
        /// </summary>
        protected static int duelDecreaseDamageLevel;
        /// <summary>
        /// 듀얼 레벨 감소율
        /// </summary>
        protected static float duelDecreaseDamagePer;

        public abstract UnitEntityType type { get; }

        public readonly BattleUnitInfo battleUnitInfo; // 전투 유닛 기본 정보
        public readonly BattleItemInfo battleItemInfo; // 전투 아이템 정보
        public readonly BattleSkillInfo battleSkillInfo; // 전투 스킬 정보
        public readonly BattleGuildSkillInfo battleGuildSkillInfo; // 전투 길드 스킬 정보
        public readonly BattleAgentOptionInfo battleAgentOptionInfo; // 동료 옵션 정보
        public readonly BattleAgentBookOptionInfo battleAgentBookOptionInfo; // 동료 옵션 도감 정보
        public readonly BattleBookOptionInfo battleBookOptionInfo; // 도감 옵션 정보
        public readonly BattlePassiveBuffSkillInfo battlePassiveBuffSkillInfo; // 전투 패시브 버프 스킬 정보
        public readonly BattleBuffItemInfo battleBuffItemInfo; // 전투 버프 아이템 정보
        public readonly BattleBuffSkillInfo battleBuffSkillInfo; // 전투 버프 스킬 정보
        public readonly BattleStatusInfo onlyBaseBattleStatusInfo; // 전투 스탯 정보 (기본 스탯 정보만)
        public readonly BattleStatusInfo preBattleStatusInfo; // 전투 스탯 정보 (버프X)
        public readonly BattleStatusInfo battleStatusInfo; // 전투 스탯 정보
        public readonly BattleCrowdControlInfo battleCrowdControlInfo; // 전투 상태이상 정보
        public readonly BattleShareForceStatusOptionInfo battleShareForceStatusOptionInfo; // 쉐어포스 스탯 정보
        private readonly IRandomDamage randomDamage; // 랜덤 지수
        private readonly IElementDamage elementDamage; // 속성댐 배율

        private readonly PassiveBattleOptionList preOptions; // 전투 옵션 리스트 (선체크)
        private readonly PassiveBattleOptionList options; // 전투 옵션 리스트
        private readonly ExtraBattleOptions extraOptions; // 추가 전투 옵션
        public readonly DamagePacket damagePacket; // 대미지 패킷
        private ForceStatusMode forceStatusMode; // 강제 스탯 옵션

        private Vector3 lastPosition; // 죽기 전 마지막 위치

        /// <summary>
        /// 위치
        /// </summary>
        public Vector3 LastPosition => (IsDie || actor == null) ? lastPosition : actor.CachedTransform.position;

        /// <summary>
        /// 적군 여부
        /// </summary>
        public bool IsEnemy { get; private set; }

        /// <summary>
        /// 타겟 무시 여부
        /// </summary>
        public bool IsIgnoreTarget { get; private set; }

        /// <summary>
        /// UI Type 여부
        /// </summary>
        public bool IsUI => type == UnitEntityType.UI;

        /// <summary>
        /// 레이어 값
        /// </summary>
        public abstract int Layer { get; }

        /// <summary>
        /// 현재 체력
        /// </summary>
        public ObscuredInt CurHP { get; private set; }

        /// <summary>
        /// 현재 마나
        /// </summary>
        public ObscuredInt CurMp { get; private set; }

        /// <summary>
        /// 만피 여부
        /// </summary>
        public bool IsMaxHp => CurHP == MaxHP;

        /// <summary>
        /// 최대마나 여부
        /// </summary>
        public bool IsMaxMp => CurMp == MaxMp;

        /// <summary>
        /// 죽음 여부
        /// </summary>
        public virtual bool IsDie => CurHP <= 0;

        /// <summary>
        /// 최대 체력
        /// </summary>
        public int MaxHP => battleStatusInfo.MaxHp;

        /// <summary>
        /// 최대 마나
        /// </summary>
        public int MaxMp => Constants.Battle.MAX_MAZE_MP;

        /// <summary>
        /// 이동 속도 배율 (Default: 1)
        /// </summary>
        public float MoveSpeedRate => GetMoveSpeedRate();

        /// <summary>
        /// 공격 속도 배율 (Default: 1)
        /// </summary>
        public float AttackSpeedRate => GetAttackSpeedRate();

        /// <summary>
        /// 스킬 사정거리 배율 (Default: 1)
        /// </summary>
        public float AtkRangeRate => GetAttackRangeRate();

        /// <summary>
        /// 분노 여부
        /// </summary>
        public bool IsAngry { get; private set; }

        bool IInfo.IsInvalidData => default;
        public event System.Action OnUpdateEvent;

        private int comboCount;
        private CoroutineHandle comboHandle;

        public event HitEvent OnHit;
        public event DamagedEvent OnDamaged; // 회피를 하더라도 호출 (가장 마지막에 호출되므로 Die 체크 필수)
        public event AttackedEvent OnAttacked; // 회피를 하더라도 호출 (가장 마지막에 호출되므로 Die 체크 필수)
        public event DotDamageEvent OnDotDamage;
        public event DieEvent OnDie, OnDieNext;
        public event ReloadStatusEvent OnReloadStatus;
        public event ReadyToBattleEvent OnReadyToBattle;
        public event ChangeHPEvent OnChangeHP, OnChangeMP;
        public event MoveSpeedRateEvent OnChangeMoveSpeedRate;
        public event AttackSpeedRateEvent OnChangeAttackSpeedRate;
        public event FleeEvent OnFlee;
        public event AutoGuardEvent OnAutoGuard;
        public event RecoveryHpEvent OnRecoveryHp;
        public event BattleCrowdControlInfo.CrowdControlEvent OnChangeCrowdControl;
        public event HitComboEvent OnHitCombo;
        public event DamageValueEvent OnDamageValue;
        public event UseSkillEvent OnUseSkill;
        public event ApplySkillEvent OnApplySkill;
        public event RebirthEvent OnRebirth;

        public UnitState State { get; private set; }

        public readonly List<IUnitModel> modelList;

        protected readonly IUnitActorPool unitActorPool;

        protected UnitActor actor;

        public event System.Action<UnitActor> OnSpawnActor;
        public event System.Action<UnitActor> OnDespawnActor;
        public event System.Action<UnitEntity> OnDespawnActorFinished;

        private CoroutineHandle maxRegenHandle;

        public bool IsIgnoreWeapon { get; private set; }

        public bool IsFreeFightSkillCoool { get; private set; }

        private bool isIgnoreApplyBattleOptions; // 전투 옵션 적용 무시

        public UnitEntity()
        {
            unitActorPool = UnitActorPoolManager.Instance;
            modelList = new List<IUnitModel>();

            clientUID = ++uniqueIndex;
            entityDic.Add(clientUID, this); // 추가

            battleUnitInfo = new BattleUnitInfo();
            battleItemInfo = new BattleItemInfo();
            battleSkillInfo = new BattleSkillInfo();
            battleGuildSkillInfo = new BattleGuildSkillInfo();
            battleAgentOptionInfo = new BattleAgentOptionInfo();
            battleAgentBookOptionInfo = new BattleAgentBookOptionInfo();
            battleBookOptionInfo = new BattleBookOptionInfo();
            battlePassiveBuffSkillInfo = new BattlePassiveBuffSkillInfo();
            battleBuffItemInfo = new BattleBuffItemInfo();
            battleBuffSkillInfo = new BattleBuffSkillInfo();
            battleStatusInfo = new BattleStatusInfo();
            onlyBaseBattleStatusInfo = new BattleStatusInfo();
            preBattleStatusInfo = new BattleStatusInfo();
            battleCrowdControlInfo = new BattleCrowdControlInfo();
            battleShareForceStatusOptionInfo = new BattleShareForceStatusOptionInfo();
            randomDamage = RandomTableDataManager.Instance;
            elementDamage = ElementDataManager.Instance;

            preOptions = new PassiveBattleOptionList();
            options = new PassiveBattleOptionList();
            extraOptions = new ExtraBattleOptions();
            damagePacket = new DamagePacket();
        }

        public virtual void Initialize()
        {
            for (int i = 0; i < modelList.Count; i++)
            {
                modelList[i].AddEvent(type);
            }

            battleStatusInfo.OnChangeMaxHp += OnChangeMaxHp;
            battleStatusInfo.OnChangeMoveSpd += OnChangeMoveSpd;
            battleStatusInfo.OnChangeAtkSpd += OnChangeAtkSpd;
            battleCrowdControlInfo.OnCrowdControl += OnCrowdControl;
            battleCrowdControlInfo.OnAddDotDamage += OnAddDotDamage;
        }

        public virtual void ResetData()
        {
            for (int i = 0; i < modelList.Count; i++)
            {
                modelList[i].ResetData();
            }

            extraOptions.Clear();
            battleSkillInfo.ClearCooltime();
            battlePassiveBuffSkillInfo.Clear();
            IsAngry = false;

            battleBuffItemInfo.Clear();
            battleBuffSkillInfo.Clear();
            isIgnoreApplyBattleOptions = false;
        }

        public virtual void Dispose()
        {
            StopAllCoroutines();

            battleStatusInfo.OnChangeMaxHp -= OnChangeMaxHp;
            battleStatusInfo.OnChangeMoveSpd -= OnChangeMoveSpd;
            battleStatusInfo.OnChangeAtkSpd -= OnChangeAtkSpd;
            battleCrowdControlInfo.OnCrowdControl -= OnCrowdControl;
            battleCrowdControlInfo.OnAddDotDamage -= OnAddDotDamage;

            for (int i = 0; i < modelList.Count; i++)
            {
                modelList[i].RemoveEvent(type);
            }
        }

        public virtual void Release()
        {
            for (int i = 0; i < modelList.Count; i++)
            {
                modelList[i].ResetData();
            }

            battlePassiveBuffSkillInfo.Clear();
        }

        public UnitActor GetActor()
        {
            return actor;
        }

        public UnitActor SpawnActor()
        {
            if (actor == null)
            {
                actor = SpawnEntityActor();
                actor.SetEntity(this);

                OnSpawnActor?.Invoke(actor);
            }

            return actor;
        }

        public void DespawnActor()
        {
            if (actor == null)
                return;

            OnDespawnActor?.Invoke(actor);
            actor.Release();
            actor = null;

            OnDespawnActorFinished?.Invoke(this);
        }

        protected abstract UnitActor SpawnEntityActor();

        /// <summary>
        /// 이름 id 반환
        /// </summary>
        public virtual int GetNameId()
        {
            return 0;
        }

        /// <summary>
        /// 이름
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// 프리팹 이름
        /// </summary>
        public abstract string GetPrefabName();

        /// <summary>
        /// 프로필 이름 (네모)
        /// </summary>
        public abstract string GetProfileName();

        /// <summary>
        /// 썸네일 이름 (원형)
        /// </summary>
        public abstract string GetThumbnailName();

        /// <summary>
        /// Enemy 타입 설정
        /// </summary>
        public void SetEnemy(bool isEnemy)
        {
            IsEnemy = isEnemy;
        }

        /// <summary>
        /// 타겟 무시 여부
        /// </summary>
        public void SetIgnoreTarget(bool isIgnoreTarget)
        {
            IsIgnoreTarget = isIgnoreTarget;
        }

        private Dictionary<int, int> GetSkillOverride(UnitEntitySettings settings)
        {
            if (settings == null)
                return null;

            if (extraOptions.HasType(ExtraBattleOptionType.PlusSensing))
                settings.unitSettings.cognizanceDistance += extraOptions.Get(ExtraBattleOptionType.PlusSensing);

            if (forceStatusMode.HasFlag(ForceStatusMode.BasicAttackSkillOff))
                settings.skillSettings.basicActiveSkillId = Constants.Battle.EMPTY_SKILL_ID;

            battleUnitInfo.Initialize(settings.unitSettings); // 기본 정보 세팅
            battleItemInfo.Initialize(settings.itemSettings); // 아이템 정보 세팅
            battleSkillInfo.Initialize(settings.skillSettings, null); // 스킬 정보 세팅
            battleGuildSkillInfo.Initialize(settings.guildSkillSettings); // 길드 스킬 정보 세팅
            battleAgentOptionInfo.Initialize(settings.agentSettings); // 동료 정보 세팅
            battleAgentBookOptionInfo.Initialize(settings.agentBookSettings); // 동료 도감 정보 세팅
            battleBookOptionInfo.Initialize(settings.bookSettings); // 도감 정보 세팅
            battleShareForceStatusOptionInfo.Initialize(settings.shareForceSettings); // 쉐어포스 세팅

            bool isIgnoreBlssBuff = forceStatusMode.HasFlag(ForceStatusMode.BlssModeOff);
            if (forceStatusMode.HasFlag(ForceStatusMode.UseBuffItemOptions))
            {
                battleBuffItemInfo.Initialize(GetArrayBuffItemSettings(), GetArrayEventBuffInfos(), isIgnoreBlssBuff ? null : GetBlssBuffItems()); // 아이템 버프 정보 세팅, 이벤트 버프 정보 세팅
            }
            else
            {
                // 플레이어 이외의 다른 캐릭터의 경우에도 축복 버프는 적용켜야 함
                battleBuffItemInfo.Initialize(null, null, isIgnoreBlssBuff ? null : GetBlssBuffItems()); // 아이템 버프 옵션 사용하지 않음
            }

            preOptions.AddRange(battleItemInfo); // 아이템 옵션 추가
            preOptions.AddRange(battleSkillInfo); // 스킬 옵션 추가
            preOptions.AddRange(battleGuildSkillInfo); // 길드 스킬 옵션 추가
            preOptions.AddRange(battleAgentOptionInfo); // 동료 옵션 추가
            preOptions.AddRange(battleAgentBookOptionInfo); // 동료 도감 옵션 추가
            preOptions.AddRange(battleBookOptionInfo); // 도감 옵션 추가
            preOptions.AddRange(battleShareForceStatusOptionInfo); // 쉐어포스 옵션 추가
            preOptions.AddRange(battlePassiveBuffSkillInfo); // 패시브 스킬 버프 옵션 추가
            preOptions.AddRange(battleBuffItemInfo); // 아이템 버프 옵션 추가
            preOptions.AddRange(battleBuffSkillInfo); // 스킬 버프 옵션 추가

            // 서버 전옵타 - 길드 제외 (멀티 플레이어 전용)
            if (settings.serverBattleOptions != null)
                preOptions.AddRange(settings.serverBattleOptions);

            // 서버 전옵타 - 길드 (멀티 플레이어 전용)
            if (settings.serverGuildBattleOptions != null)
                preOptions.AddRange(settings.serverGuildBattleOptions);

            return preOptions.GetSkillOverrideDic();
        }

        /// <summary>
        /// 스탯 다시 로드
        /// </summary>
        public void ReloadStatus()
        {
            UnitEntitySettings settings = CreateUnitSettings();

            if (settings == null)
                return;

            if (extraOptions.HasType(ExtraBattleOptionType.PlusSensing))
                settings.unitSettings.cognizanceDistance += extraOptions.Get(ExtraBattleOptionType.PlusSensing);

            if (forceStatusMode.HasFlag(ForceStatusMode.BasicAttackSkillOff))
                settings.skillSettings.basicActiveSkillId = Constants.Battle.EMPTY_SKILL_ID;

            battleItemInfo.Clear();
            Battle.StatusOutput onlyBaseResult = Battle.ReloadStatus(settings.statusInput, battleUnitInfo, battleItemInfo, options, this); // 기본 스탯 정보로만 스테이터스 저장

            battleUnitInfo.Initialize(settings.unitSettings); // 기본 정보 세팅
            battleItemInfo.Initialize(settings.itemSettings); // 아이템 정보 세팅
            battleSkillInfo.Initialize(settings.skillSettings, GetSkillOverride(settings)); // 스킬 정보 세팅
            battleGuildSkillInfo.Initialize(settings.guildSkillSettings); // 길드 스킬 정보 세팅
            battleAgentOptionInfo.Initialize(settings.agentSettings); // 동료 정보 세팅
            battleAgentBookOptionInfo.Initialize(settings.agentBookSettings); // 동료 도감 정보 세팅
            battleBookOptionInfo.Initialize(settings.bookSettings); // 도감 정보 세팅
            battleShareForceStatusOptionInfo.Initialize(settings.shareForceSettings); // 쉐어포스 세팅

            if (isIgnoreApplyBattleOptions)
            {
                // Do Nothing
            }
            else
            {
                bool isIgnoreBlssBuff = forceStatusMode.HasFlag(ForceStatusMode.BlssModeOff);
                if (forceStatusMode.HasFlag(ForceStatusMode.UseBuffItemOptions))
                {
                    battleBuffItemInfo.Initialize(GetArrayBuffItemSettings(), GetArrayEventBuffInfos(), isIgnoreBlssBuff ? null : GetBlssBuffItems()); // 아이템 버프 정보 세팅, 이벤트 버프 정보 세팅
                }
                else
                {
                    // 플레이어 이외의 다른 캐릭터의 경우에도 축복 버프는 적용켜야 함
                    battleBuffItemInfo.Initialize(null, null, isIgnoreBlssBuff ? null : GetBlssBuffItems()); // 아이템 버프 옵션 사용하지 않음
                }
            }
            //battleBuffSkillInfo.Initialize(); // 스킬 버프 정보 세팅

#if SHOW_DETAIL_LOG
            SetHeader($"서버와 {nameof(BattleOptionType)} 체크");

            #region BattleItemInfo
            AddLog($"==== 장착한 장비 아이템 (카드 포함)");
            foreach (var item in battleItemInfo)
            {
                AddLog($"[{item}]");
            }
            AddLog($"[= 총합 =]");
            options.AddRange(battleItemInfo);
            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (!options.HasValue(item))
                    continue;

                BattleStatus option = options.Get(item);
                AddLog($"[{item} ⇒ {nameof(option)}: {option}]");
            }
            options.Clear();
            #endregion

            #region BattleSkillInfo
            AddLog($"==== 배운 스킬 (only 패시브)");
            foreach (var item in battleSkillInfo)
            {
                AddLog($"[{item}]");
            }
            AddLog($"[= 총합 =]");
            options.AddRange(battleSkillInfo);
            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (!options.HasValue(item))
                    continue;

                BattleStatus option = options.Get(item);
                AddLog($"[{item} ⇒ {nameof(option)}: {option}]");
            }
            options.Clear();
            #endregion

            #region BattleGuildSkillInfo
            AddLog($"==== 배운 길드 스킬");
            foreach (var item in battleGuildSkillInfo)
            {
                AddLog($"[{item}]");
            }
            AddLog($"[= 총합 =]");
            options.AddRange(battleGuildSkillInfo);
            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (!options.HasValue(item))
                    continue;

                BattleStatus option = options.Get(item);
                AddLog($"[{item} ⇒ {nameof(option)}: {option}]");
            }
            options.Clear();
            #endregion

            #region BattleAgentOptionInfo
            AddLog($"==== 동료");
            foreach (var item in battleAgentOptionInfo)
            {
                AddLog($"[{item}]");
            }
            AddLog($"[= 총합 =]");
            options.AddRange(battleAgentOptionInfo);
            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (!options.HasValue(item))
                    continue;

                BattleStatus option = options.Get(item);
                AddLog($"[{item} ⇒ {nameof(option)}: {option}]");
            }
            options.Clear();
            #endregion

            #region BattleAgentBookOptionInfo
            AddLog($"==== 동료 도감");
            foreach (var item in battleAgentBookOptionInfo)
            {
                AddLog($"[{item}]");
            }

            AddLog($"[= 총합 =]");
            options.AddRange(battleAgentBookOptionInfo);
            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (!options.HasValue(item))
                    continue;

                BattleStatus option = options.Get(item);
                AddLog($"[{item} ⇒ {nameof(option)}: {option}]");
            }
            options.Clear();
            #endregion

            #region BattleBookOptionInfo
            AddLog($"==== 도감");
            foreach (var item in battleBookOptionInfo)
            {
                AddLog($"[{item}]");
            }

            AddLog($"[= 총합 =]");
            options.AddRange(battleBookOptionInfo);
            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (!options.HasValue(item))
                    continue;

                BattleStatus option = options.Get(item);
                AddLog($"[{item} ⇒ {nameof(option)}: {option}]");
            }
            options.Clear();
            #endregion

            #region BattleShareForceStatusOptionInfo
            AddLog($"==== 쉐어포스");
            foreach (var item in battleShareForceStatusOptionInfo)
            {
                AddLog($"[{item}]");
            }

            AddLog($"[= 총합 =]");
            options.AddRange(battleShareForceStatusOptionInfo);
            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (!options.HasValue(item))
                    continue;

                BattleStatus option = options.Get(item);
                AddLog($"[{item} ⇒ {nameof(option)}: {option}]");
            }
            options.Clear();
            #endregion

            #region BattlePassiveBuffSkillInfo
            AddLog($"==== 패시브 버프");
            foreach (var item in battlePassiveBuffSkillInfo)
            {
                AddLog($"[{item}]");
            }
            AddLog($"[= 총합 =]");
            options.AddRange(battlePassiveBuffSkillInfo);
            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (!options.HasValue(item))
                    continue;

                BattleStatus option = options.Get(item);
                AddLog($"[{item} ⇒ {nameof(option)}: {option}]");
            }
            options.Clear();
            #endregion

            #region BattleBuffItemInfo
            AddLog($"==== 아이템 버프");
            foreach (var item in battleBuffItemInfo)
            {
                AddLog($"[{item}]");
            }
            AddLog($"[= 총합 =]");
            options.AddRange(battleBuffItemInfo);
            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (!options.HasValue(item))
                    continue;

                BattleStatus option = options.Get(item);
                AddLog($"[{item} ⇒ {nameof(option)}: {option}]");
            }
            options.Clear();
            #endregion

            #region BattleBuffSkillInfo
            AddLog($"==== 스킬 버프");
            foreach (var item in battleBuffSkillInfo)
            {
                AddLog($"[{item}]");
            }
            AddLog($"[= 총합 =]");
            options.AddRange(battleBuffSkillInfo);
            foreach (BattleOptionType item in System.Enum.GetValues(typeof(BattleOptionType)))
            {
                if (!options.HasValue(item))
                    continue;

                BattleStatus option = options.Get(item);
                AddLog($"[{item} ⇒ {nameof(option)}: {option}]");
            }
            options.Clear();
            #endregion

            FinishLog();
#endif

            options.AddRange(battleItemInfo); // 아이템 옵션 추가
            options.AddRange(battleSkillInfo); // 스킬 옵션 추가
            options.AddRange(battleGuildSkillInfo); // 길드 스킬 옵션 추가
            options.AddRange(battleAgentOptionInfo); // 동료 옵션 추가
            options.AddRange(battleAgentBookOptionInfo); // 동료 도감 옵션 추가
            options.AddRange(battleBookOptionInfo); // 도감 옵션 추가
            options.AddRange(battleShareForceStatusOptionInfo); // 쉐어포스 옵션 추가
            options.AddRange(battlePassiveBuffSkillInfo); // 패시브 버프 옵션 추가

            Battle.StatusOutput preResult = Battle.ReloadStatus(settings.statusInput, battleUnitInfo, battleItemInfo, options, this); // 버프 받지 않은 스테이터스 저장

            options.AddRange(battleBuffItemInfo); // 아이템 버프 옵션 추가
            options.AddRange(battleBuffSkillInfo); // 스킬 버프 옵션 추가

            // 서버 전옵타 - 길드 제외 (멀티 플레이어 전용)
            if (settings.serverBattleOptions != null)
                options.AddRange(settings.serverBattleOptions);

            // 서버 전옵타 - 길드 (멀티 플레이어 전용)
            if (settings.serverGuildBattleOptions != null)
                options.AddRange(settings.serverGuildBattleOptions);

            Battle.StatusOutput result = Battle.ReloadStatus(settings.statusInput, battleUnitInfo, battleItemInfo, options, this);

            if (forceStatusMode.HasFlagEnum(ForceStatusMode.BasicStatusModeOn))
            {
                result.statusSettings = BattleStatusInfo.Settings.DEFAULT;
            }

            if (forceStatusMode.HasFlagEnum(ForceStatusMode.UndeadMode))
            {
                result.statusSettings.maxHp = 1_000_000_000;
                result.statusSettings.regenHp = 1_000_000_000;
            }

            if (forceStatusMode.HasFlagEnum(ForceStatusMode.Flee))
            {
                result.statusSettings.flee = 1_000_000;
            }

            if (extraOptions.HasType(ExtraBattleOptionType.AttackSpeed))
                result.statusSettings.atkSpd = extraOptions.Get(ExtraBattleOptionType.AttackSpeed);

            if (extraOptions.HasType(ExtraBattleOptionType.MoveSpeed))
                result.statusSettings.moveSpd = extraOptions.Get(ExtraBattleOptionType.MoveSpeed);

            if (extraOptions.HasType(ExtraBattleOptionType.Flee))
                result.statusSettings.flee = extraOptions.Get(ExtraBattleOptionType.Flee);

            if (extraOptions.HasType(ExtraBattleOptionType.AttackRange))
                result.statusSettings.atkRange = extraOptions.Get(ExtraBattleOptionType.AttackRange);

            if (extraOptions.HasType(ExtraBattleOptionType.CriDmgRate))
                result.statusSettings.criDmgRate = extraOptions.Get(ExtraBattleOptionType.CriDmgRate);

            if (extraOptions.HasType(ExtraBattleOptionType.PlusAttackSpd))
                result.statusSettings.atkSpd += extraOptions.Get(ExtraBattleOptionType.PlusAttackSpd);

            if (extraOptions.HasType(ExtraBattleOptionType.PlusMoveSpd))
                result.statusSettings.moveSpd += extraOptions.Get(ExtraBattleOptionType.PlusMoveSpd);

            if (extraOptions.HasType(ExtraBattleOptionType.MaxHp))
                result.statusSettings.maxHp = extraOptions.Get(ExtraBattleOptionType.MaxHp);

            if (extraOptions.HasType(ExtraBattleOptionType.PlusPvpHpRate))
                result.statusSettings.maxHp += MathUtils.ToInt(result.statusSettings.maxHp * MathUtils.ToPermyriadValue(extraOptions.Get(ExtraBattleOptionType.PlusPvpHpRate)));

            if (extraOptions.HasType(ExtraBattleOptionType.DmgRate))
                result.statusSettings.dmgRate += extraOptions.Get(ExtraBattleOptionType.DmgRate);

            if (extraOptions.HasType(ExtraBattleOptionType.DmgRateResist))
                result.statusSettings.dmgRateResist += extraOptions.Get(ExtraBattleOptionType.DmgRateResist);

            if (forceStatusMode.HasFlag(ForceStatusMode.RefSkillOff))
            {
                battleSkillInfo.SetFireActiveSkillRate(null); // 화속성 공격 시 발동되는 스킬 세팅
                battleSkillInfo.SetBasicActiveSkillRate(null); // 평타 스킬 특정 스킬로 변경 확률 세팅
                battleSkillInfo.SetExtraActiveSkillRate(null); // 특정 스킬 사용 확률 세팅
                battleSkillInfo.SetColleagueRate(null); // 특정 몬스터 동행 확률 세팅
                battleSkillInfo.SetSkillChain(null); // 스킬연계 세팅
            }
            else
            {
                battleSkillInfo.SetFireActiveSkillRate(options.GetFireActiveSkillRateDic()); // 화속성 공격 시 발동되는 스킬 세팅
                battleSkillInfo.SetBasicActiveSkillRate(options.GetBasicActiveSkillRateDic()); // 평타 스킬 특정 스킬로 변경 확률 세팅
                battleSkillInfo.SetExtraActiveSkillRate(options.GetActiveSkillRateDic()); // 특정 스킬 사용 확률 세팅
                battleSkillInfo.SetColleagueRate(options.GetColleagueRateDic()); // 특정 몬스터 동행 확률 세팅
                battleSkillInfo.SetSkillChain(options.GetSkillChainDic()); // 스킬연계 세팅
            }

            battleStatusInfo.SetSkillIdDmgRate(options.GetSkillIdDmgRateDic()); // 특정 스킬 증폭대미지 비율 세팅

            onlyBaseBattleStatusInfo.Initialize(onlyBaseResult.statusSettings); // 오직 기본 스탯으로만 정보 세팅
            preBattleStatusInfo.Initialize(preResult.statusSettings); // 버프 받지 않은 스탯 정보 저장
            battleStatusInfo.Initialize(result.statusSettings); // 스탯 정보 세팅
            options.Clear(); // 기존 옵션 초기화
            preOptions.Clear(); // 기존 옵션 초기화

            SetCurrentHp(Mathf.Min(CurHP, result.statusSettings.maxHp)); // 현재 hp는 maxhp 값을 넘을 수 음슴

            OnReloadStatus?.Invoke();
        }

        /// <summary>
        /// 유닛 스탯 세팅값 생성
        /// </summary>
        public abstract UnitEntitySettings CreateUnitSettings();

        /// <summary>
        /// 현재 장착중인 장비 아이템 리스트
        /// </summary>
        protected abstract ItemInfo[] GetEquippedItems();

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Atk
        /// </summary>
        protected abstract int GetServerTotalItemAtk();

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Matk
        /// </summary>
        protected abstract int GetServerTotalItemMatk();

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Def
        /// </summary>
        protected abstract int GetServerTotalItemDef();

        /// <summary>
        /// 서버에서 받은 장착아이템으로 인한 Mdef
        /// </summary>
        protected abstract int GetServerTotalItemMdef();

        /// <summary>
        /// 유효한 스킬 목록
        /// </summary>
        protected abstract SkillInfo[] GetValidSkills();

        /// <summary>
        /// 스킬 수동 여부
        /// </summary>
        protected virtual bool IsAntiSkillAuto()
        {
            return false; // 자동
        }

        /// <summary>
        /// 길드 스킬 목록
        /// </summary>
        public abstract SkillInfo[] GetValidGuildSkills();

        /// <summary>
        /// 아이템 버프 목록
        /// </summary>
        protected abstract BattleBuffItemInfo.ISettings[] GetArrayBuffItemSettings();

        /// <summary>
        /// 이벤트 버프 목록
        /// </summary>
        protected abstract EventBuffInfo[] GetArrayEventBuffInfos();

        /// <summary>
        /// 축복 아이템버프 목록
        /// </summary>
        protected abstract IEnumerable<BlessBuffItemInfo> GetBlssBuffItems();

        /// <summary>
        /// 동료 보유 효과
        /// </summary>
        protected abstract IEnumerable<IAgent> GetAllAgents();

        /// <summary>
        /// 동료 도감 효과
        /// </summary>
        protected abstract IEnumerable<AgentBookState> GetEnabledBookStates();

        protected virtual IEnumerable<BattleOption> GetBookOptions() { return null; }

        /// <summary>
        /// 쉐어포스 스탯 효과
        /// </summary>
        protected abstract IEnumerable<ShareStatBuildUpData> GetShareForceData();

        /// <summary>
        /// 서버에서 받은 전옵타
        /// </summary>
        protected abstract BattleOption[] GetServerBattleOptions();

        /// <summary>
        /// 서버에서 받은 전옵타 (길드)
        /// </summary>
        protected abstract BattleOption[] GetServerGuildBattleOptions();

        /// <summary>
        /// 전투 준비
        /// </summary>
        public void ReadyToBattle()
        {
            StopAllCoroutines();

            battleCrowdControlInfo.Clear(); // 기존의 상태이상 모두 제거

            SetCurrentHp(MaxHP); // 현재HP를 최대HP로 맞추어 줌
            SetCurrentMp(MaxMp);

            OnReadyToBattle?.Invoke();
        }

        public void Rebirth()
        {
            ReadyToBattle();
            OnRebirth?.Invoke();
        }

        /// <summary>
        /// 체력 재생
        /// </summary>
        public void RegenHp()
        {
            if (IsDie)
                return;

            if (State == UnitState.Maze || State == UnitState.GVG || State == UnitState.GVGMultiPlayer)
                return;

            int hp = CurHP + battleStatusInfo.RegenHp;
            SetCurrentHp(hp);
        }

        /// <summary>
        /// 마나 재생
        /// </summary>
        public void RegenMp()
        {
            if (IsDie)
                return;

            RecoveryMp(battleStatusInfo.RegenMp);
        }

        /// <summary>
        /// 웨이브 클리어로 인한 체력 회복
        /// </summary>
        public void RecoveryHpForWaveClear()
        {
            if (IsDie)
                return;

            int value = MathUtils.ToInt(battleStatusInfo.RegenHp * BasisType.WAVE_HP_INC_RATE.GetFloat());
            RecoveryHp(value, blowCount: 1);
        }

        /// <summary>
        /// 지속 효과 체크 (버프, 상태이상)
        /// </summary>
        public void CheckDurationEffect()
        {
            bool isDirtyBuffItem = battleBuffItemInfo.CheckDurationEffect();
            bool isDirtyBuffSkill = battleBuffSkillInfo.CheckDurationEffect();
            battleCrowdControlInfo.CheckDurationEffect(); // 상태이상의 경우 만료되어도 Status을 다시 로드할 필요 없음

            // 변경된 정보가 있을 경우 스탯 다시 로드
            if (isDirtyBuffItem || isDirtyBuffSkill)
            {
#if SHOW_DETAIL_LOG
                SetHeader("[버프 만료]");
                FinishLog();
#endif
                if (type == UnitEntityType.Player)
                    UIPowerUpdate.IsIgnoreOnce = true;
                ReloadStatus();
            }
        }

        public virtual int GetDamageFontSize()
        {
            return Ragnarok.Damage.DEFAULT_HIT_FONT_SIZE;
        }

        /// <summary>
        /// 스킬 효과 적용
        /// </summary>
        public void Apply(UnitEntity attacker, SkillInfo skillInfo, UnitEntity devotedUnit)
        {
            // 패시브 타입의 스킬은 효과를 적용할 수 없음
            SkillType skillType = skillInfo.SkillType;
            switch (skillType)
            {
                case SkillType.Passive:
                case SkillType.Plagiarism:
                case SkillType.Reproduce:
                case SkillType.SummonBall:
                case SkillType.RuneMastery:
                    return;
            }

            // 멀티 플레이어의 경우에는 대미지가 들어가지 않음
            if (IsDamageIgnore())
                return;

            // 시전자가 죽었을 경우 => 시전자의 스킬 적용 무시
            if (attacker.IsDie)
                return;

            if (IsDie)
                return;

            // 스킬이 평타이면서 속성이 존재하지 않을 경우 => 장착한 무기의 속성으로 대체
            bool useEquipmentElement = skillInfo.IsBasicActiveSkill && skillInfo.ElementType == default;
            ElementType skillElementType = useEquipmentElement ? attacker.battleItemInfo.WeaponElementType : skillInfo.ElementType;
            int skillElementLevel = useEquipmentElement ? attacker.battleItemInfo.WeaponElementLevel : 0;
            float dcecreaseDamageRatePer = GetDcecreaseDamageRatePer(attacker, this); // 전체댐감소율 (댐보정)

            bool isChainedSkill = skillInfo.IsChainedSkill(); // 연계스킬 여부

            if (skillInfo.ActiveSkillType == ActiveSkill.Type.Buff)
            {
                AddBattleBuff(skillInfo);
                attacker.OnAttacked?.Invoke(this, skillType, skillInfo.SkillId, skillElementType, hasDamage: false, isChainedSkill); // 공격 이벤트
            }
            else
            {
                Battle.SkillInput settings = new Battle.SkillInput
                {
                    unitTargetValue = battleUnitInfo,
                    statusTargetValue = battleStatusInfo,
                    crowdControlTargetValue = battleCrowdControlInfo,

                    itemAttackerValue = attacker.battleItemInfo,
                    statusAttackerValue = attacker.battleStatusInfo,
                    crowdControlAttackerValue = attacker.battleCrowdControlInfo,

                    skillValue = skillInfo,
                    skillElementType = skillElementType,
                    skillElementLevel = skillElementLevel,
                    randomDamage = randomDamage,
                    elementDamage = elementDamage,
                    dcecreaseDamageRatePer = dcecreaseDamageRatePer,
                };

                Battle.SkillOutput result = Battle.ApplyActiveSkill(settings, this);

                if (result.isAutoGuard)
                    OnAutoGuard?.Invoke();

                if (result.isFlee)
                    OnFlee?.Invoke(attacker);

                int blowCount = skillInfo.BlowCount;

                if (result.hasRecovery)
                {
                    RecoveryHp(result.totalRecovery, blowCount);
                }

                if (result.hasDamage)
                {
                    // 대미지 패킷 저장
                    bool isNeedSaveDamagePacket = attacker.IsNeedSaveDamagePacket();
                    if (isNeedSaveDamagePacket)
                    {
                        damagePacket.Set(GetDamageUnitKey(), attacker.GetDamageUnitKey(), settings, battleBuffSkillInfo.GetBuffSkills(), IsAngry, attacker.battleBuffSkillInfo.GetBuffSkills(), attacker.IsAngry, result);
                    }

                    if (devotedUnit == null || devotedUnit.IsDie)
                    {
                        Damage(attacker, result.totalDamage, blowCount, result.isCritical, skillType == SkillType.BasicActiveSkill, skillElementType, result.elementFactor, isNotDie: false);
                    }
                    else
                    {
                        int realTotalDamage = Mathf.Max(0, MathUtils.ToInt(result.totalDamage * (1 - MathUtils.ToPermyriadValue(devotedUnit.battleStatusInfo.DevoteRate)))); // 대미지는 0 이상
                        int devoteTotalDamage = result.totalDamage - realTotalDamage;

                        devotedUnit.Damage(attacker, devoteTotalDamage, blowCount, result.isCritical, skillType == SkillType.BasicActiveSkill, skillElementType, result.elementFactor, isNotDie: true); //  헌신 대미지 (헌신 피해 대미지로는 죽을 수 없다)
                        Damage(attacker, realTotalDamage, blowCount, result.isCritical, skillType == SkillType.BasicActiveSkill, skillElementType, result.elementFactor, isNotDie: false); // 일반 대미지
                    }

                    //attacker.PlusCombo(blowCount); // 콤보 증가
                }

                if (result.hasAbsorbRecovery)
                {
                    attacker.RecoveryHp(result.absorbRecovery, blowCount);
                }

#if SHOW_DETAIL_LOG
                if (skillType == SkillType.BasicActiveSkill)
                {
                    attacker.SetHeader($"스킬 사용 {attacker.GetName()} ⇒ {GetName()} 평타 ({skillInfo.SkillId})");
                }
                else
                {
                    attacker.SetHeader($"스킬 사용 {attacker.GetName()} ⇒ {GetName()} {skillInfo.SkillName} ({skillInfo.SkillId})");
                }
                attacker.AddLog($"[isAutoGuard: 타겟의 오토가드 성공여부] {result.isAutoGuard}");
                attacker.AddLog($"[isFlee: 타겟의 회피 성공여부] {result.isFlee}");
                attacker.AddLog($"[타겟의 대미지] {result.totalDamage}");
                attacker.FinishLog();
#endif

                battleCrowdControlInfo.Apply(result.crowdControlSettings); // 상태이상 적용 (상태이상은 나중에 적용할 것)

                OnDamaged?.Invoke(attacker, skillType, skillElementType, result.hasDamage, isChainedSkill); // 피격 이벤트
                attacker.OnAttacked?.Invoke(this, skillType, skillInfo.SkillId, skillElementType, result.hasDamage, isChainedSkill); // 공격 이벤트

                if (IsDie)
                    battleCrowdControlInfo.Clear(); // 기존의 상태이상 모두 제거
            }
        }

        protected abstract DamagePacket.UnitKey GetDamageUnitKey();

        /// <summary>
        /// 버프스킬 추가 적용
        /// </summary>
        public void AddBattleBuff(SkillInfo skillInfo)
        {
            battleBuffSkillInfo.Add(skillInfo);
            if (type == UnitEntityType.Player)
                UIPowerUpdate.IsIgnoreOnce = true;
            ReloadStatus();
        }

        /// <summary>
        /// 패시브 버프 초기화
        /// </summary>
        public void ResetPassiveBuffSkills()
        {
            SetPassiveBuffSkills(null);
        }

        /// <summary>
        /// 패시브 버프 세팅
        /// </summary>
        public void SetPassiveBuffSkills(ISkillDataKey[] buffSkills)
        {
            int length = buffSkills == null ? 0 : buffSkills.Length;
            // 버프 개수가 0일 때, 패시브 버프도 0이면 굳이 스탯을 Reload 하지 않는다.
            if (battlePassiveBuffSkillInfo.Count == 0 && length == 0)
                return;

            battlePassiveBuffSkillInfo.Initialize(buffSkills);
            ReloadStatus();
        }

        public void MakeWeak()
        {
            SetCurrentHp(1); // 현재HP를 1로 만들어 줌
            battleCrowdControlInfo.MakeWeak(); // 회피 금지
        }

        public void MakeHalfWeak()
        {
            SetCurrentHp(MaxHP / 2); // 현재HP를 반피로 만들어 줌
            battleCrowdControlInfo.MakeWeak(); // 회피 금지
        }

        /// <summary>
        /// 쿨타임 정보 초기화
        /// </summary>
        public virtual void ResetSkillCooldown()
        {
            battleSkillInfo.ResetCooldown(); // 쿨타임 정보 초기화
            battleBuffSkillInfo.Clear(); // 버프 스킬 정보 초기화
        }

        /// <summary>
        /// 데미지패킷 저장 필요 여부
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsNeedSaveDamagePacket()
        {
            return false;
        }

        /// <summary>
        /// 대미지 무시 여부
        /// </summary>
        protected virtual bool IsDamageIgnore()
        {
            return false;
        }

        private void OnCrowdControl(CrowdControlType ccType, bool isGroggy, int overapCount)
        {
            // 상태이상으로 이속 또는 공속이 바뀔 수 있다
            OnChangeMoveSpd();
            OnChangeAtkSpd();

            OnChangeCrowdControl?.Invoke(ccType, isGroggy, overapCount);
        }

        private void OnAddDotDamage(CrowdControlType ccType, float duration, int dotDamageRate)
        {
            Timing.RunCoroutine(YieldDotDamage(ccType, duration, MathUtils.ToPermyriadValue(dotDamageRate)), clientUID, TAG);
        }

        private void StopAllCoroutines()
        {
            Timing.KillCoroutines(clientUID, TAG);
        }

        public void RecoveryHp(int value, int blowCount)
        {
            if (IsDie)
                return;

            OnRecoveryHp?.Invoke(value, blowCount);

            int hp = CurHP + (value * blowCount);
            SetCurrentHp(hp);
        }

        public void RecoveryMp(int value)
        {
            int mp = CurMp + value;
            SetCurrentMp(mp);
        }

        public void ApplyDamage(UnitEntity attacker, int value, int blowCount, bool isCritical, bool isBasicActiveSkill, ElementType elementType, int totalDamage)
        {
            if (IsDie)
                return;

            if (totalDamage == 0 && value == 0)
            {
                OnFlee?.Invoke(attacker);
                return;
            }

            Damage(attacker, value, blowCount, isCritical, isBasicActiveSkill, elementType, elementFactor: 10000, isNotDie: false, totalDamage);
        }

        private void Damage(UnitEntity attacker, int value, int blowCount, bool isCritical, bool isBasicActiveSkill, ElementType elementType, int elementFactor, bool isNotDie, int totalDamage = 0)
        {
            if (IsDie)
                return;

            if (totalDamage == 0)
                totalDamage = value * blowCount;

            OnHit?.Invoke(this, attacker, value, blowCount, isCritical, isBasicActiveSkill, elementType, elementFactor);
            OnDamageValue?.Invoke(this, attacker, totalDamage);

            ApplyHP(totalDamage, 1, isNotDie);

            if (actor)
                actor.HitTintPlayer?.PlayEffect();

            if (IsDie)
            {
                if (actor)
                    lastPosition = actor.CachedTransform.position;

                StopAllCoroutines();
                OnDie?.Invoke(this, attacker);
                OnDieNext?.Invoke(this, attacker);
            }
        }

        /// <summary>
        /// 강제 사망처리
        /// </summary>
        public void Die(UnitEntity attacker)
        {
            SetCurrentHp(0);

            if (actor)
                lastPosition = actor.CachedTransform.position;

            StopAllCoroutines();
            OnDie?.Invoke(this, attacker);
            OnDieNext?.Invoke(this, attacker);
        }

        /// <summary>
        /// 분노 여부 (false 상태는 ResetData 에서 한다)
        /// </summary>
        public void SetAngryState(bool isAngry)
        {
            if (IsAngry == isAngry)
                return;

            IsAngry = isAngry;
            ReloadStatus();
        }

        protected virtual void ApplyHP(int value, int blowCount, bool isNotDie)
        {
            int hp;

            // 죽지 않아야 할 경우 (헌신) 최종 대미지가 1
            if (isNotDie)
            {
                hp = Mathf.Max(1, CurHP - (value * blowCount));
            }
            else
            {
                hp = CurHP - (value * blowCount);
            }

            SetCurrentHp(hp);
        }

        private void DotDamage(CrowdControlType ccType, int dotDamage)
        {
            // 죽었을 경우
            if (IsDie)
                return;

            OnDotDamage?.Invoke(this, ccType, dotDamage);

            int hp = Mathf.Max(1, CurHP - dotDamage); // 최종 대미지는 1이상
            SetCurrentHp(hp);
        }

        public void SetCurrentHp(int hp)
        {
            int currentHp = Mathf.Clamp(hp, 0, MaxHP);

            if (currentHp == CurHP)
                return;

            CurHP = currentHp;
            OnChangeHP?.Invoke(currentHp, MaxHP);
        }

        public void SetCurrentMp(int mp)
        {
            int currentMp = Mathf.Clamp(mp, 0, MaxMp);

            if (currentMp == CurMp)
                return;

            CurMp = currentMp;
            OnChangeMP?.Invoke(currentMp, MaxMp);
        }

        private void OnChangeMaxHp()
        {
            OnChangeHP?.Invoke(CurHP, MaxHP);
        }

        private void OnChangeMoveSpd()
        {
            OnChangeMoveSpeedRate?.Invoke(MoveSpeedRate);
        }

        private void OnChangeAtkSpd()
        {
            OnChangeAttackSpeedRate?.Invoke(AttackSpeedRate);
        }

        protected virtual float GetMoveSpeedRate()
        {
            float moveSpd = MathUtils.ToPermyriadValue(battleStatusInfo.MoveSpd);
            int moveSpdDecreaseRate = battleCrowdControlInfo.MoveSpdDecreaseRate;

            if (forceStatusMode.HasFlagEnum(ForceStatusMode.MazeMoveSpdDown))
                moveSpdDecreaseRate += MAZE_MONSTER_MOVE_SPEED_DOWN_RATE;

            if (forceStatusMode.HasFlagEnum(ForceStatusMode.MazeMoveSpdUp))
                moveSpdDecreaseRate -= MAZE_MONSTER_MOVE_SPEED_UP_RATE;

            if (moveSpdDecreaseRate == 0)
                return moveSpd;

            return moveSpd * (1 - MathUtils.ToPermyriadValue(moveSpdDecreaseRate)); // 상태이상 적용
        }

        private float GetAttackSpeedRate()
        {
            float atkSpd = MathUtils.ToPermyriadValue(battleStatusInfo.AtkSpd);
            int atkSpdDecreaseRate = battleCrowdControlInfo.AtkSpdDecreaseRate;

            if (atkSpdDecreaseRate == 0)
                return atkSpd;

            return atkSpd * (1 - MathUtils.ToPermyriadValue(atkSpdDecreaseRate)); // 상태이상 적용
        }

        private float GetAttackRangeRate()
        {
            return MathUtils.ToPermyriadValue(battleStatusInfo.AtkRange);
        }

        private void PlusCombo(int blowCount)
        {
            Timing.KillCoroutines(comboHandle);
            comboHandle = Timing.RunCoroutine(YieldHitCombo(blowCount), clientUID, TAG);
        }

        private IEnumerator<float> YieldDotDamage(CrowdControlType ccType, float duration, float dotDamageRate)
        {
            RelativeRemainTime remainTime = duration;

            while (!IsDie && remainTime.GetRemainTime() > 0f)
            {
                yield return Timing.WaitForSeconds(1f);

                int dotDamage = Mathf.Max(1, (int)(battleStatusInfo.MaxHp * dotDamageRate)); // 도트 대미지는 1 이상
                DotDamage(ccType, dotDamage);
            }
        }

        private IEnumerator<float> YieldHitCombo(int blowCount)
        {
            for (int i = 0; i < blowCount; i++)
            {
                SetComboCount(comboCount + 1);
                yield return Timing.WaitForSeconds(0.1f);
            }

            yield return Timing.WaitForSeconds(2f);
            SetComboCount(0); // ResetCombo
        }

        private void SetComboCount(int count)
        {
            if (comboCount == count)
                return;

            comboCount = count;
            OnHitCombo?.Invoke(comboCount);

            OnChangeMoveSpd(); // 콤보로 인하여 이속이 변경될 수 있다
            OnChangeAtkSpd(); // 콤보로 인하여 공속이 변경될 수 있다
        }

        public void InvokeUseSkill(UnitEntity target, SkillInfo skillInfo)
        {
            OnUseSkill?.Invoke(target, skillInfo);
        }

        public void InvokeApplySkill(UnitEntity[] targets, SkillInfo skillInfo)
        {
            OnApplySkill?.Invoke(targets, skillInfo);
        }

#if UNITY_EDITOR
        public void SetHp(int hp)
        {
            SetCurrentHp(hp);
        }
#endif
        public virtual void SetState(UnitState state)
        {
            State = state;
        }

        public void SetExtraOption(ExtraBattleOptionType type, int value)
        {
            extraOptions.Set(type, value);
            ReloadStatus();
        }

        public void ResetExtraOption(ExtraBattleOptionType key)
        {
            if (extraOptions.Reset(key))
                ReloadStatus();
        }

        public void SetForceStatus(ForceStatusType flag)
        {
            switch (flag)
            {
                case ForceStatusType.MaxRegenOn:
                    forceStatusMode.AddFlagEnum(ForceStatusMode.UndeadMode);
                    ReloadStatus();
                    SetCurrentHp(MaxHP);

                    Timing.KillCoroutines(maxRegenHandle);
                    maxRegenHandle = Timing.RunCoroutine(YieldMaxRegen(), clientUID, TAG);
                    break;

                case ForceStatusType.MaxRegenOff:
                    forceStatusMode.RemoveFlagEnum(ForceStatusMode.UndeadMode);
                    ReloadStatus();
                    SetCurrentHp(MaxHP);

                    Timing.KillCoroutines(maxRegenHandle);
                    break;

                case ForceStatusType.Stun:
                    battleCrowdControlInfo.Apply(new BattleCrowdControlInfo.ApplySettings { isStun = true });
                    break;

                case ForceStatusType.MazeMoveSpdDownOn:
                    forceStatusMode.AddFlagEnum(ForceStatusMode.MazeMoveSpdDown);
                    OnChangeMoveSpd();
                    break;

                case ForceStatusType.MazeMoveSpdDownOff:
                    forceStatusMode.RemoveFlagEnum(ForceStatusMode.MazeMoveSpdDown);
                    OnChangeMoveSpd();
                    break;

                case ForceStatusType.MazeMoveSpdUpOn:
                    forceStatusMode.AddFlagEnum(ForceStatusMode.MazeMoveSpdUp);
                    OnChangeMoveSpd();
                    break;

                case ForceStatusType.MazeMoveSpdUpOff:
                    forceStatusMode.RemoveFlagEnum(ForceStatusMode.MazeMoveSpdUp);
                    OnChangeMoveSpd();
                    break;

                case ForceStatusType.BuffItemOptionOn:
                    forceStatusMode.AddFlagEnum(ForceStatusMode.UseBuffItemOptions);
                    ReloadStatus();
                    break;

                case ForceStatusType.BuffItemOptionOff:
                    forceStatusMode.RemoveFlagEnum(ForceStatusMode.UseBuffItemOptions);
                    ReloadStatus();
                    break;

                case ForceStatusType.FleeOn:
                    forceStatusMode.AddFlagEnum(ForceStatusMode.Flee);
                    ReloadStatus();
                    break;

                case ForceStatusType.FleeOff:
                    forceStatusMode.RemoveFlagEnum(ForceStatusMode.Flee);
                    ReloadStatus();
                    break;

                case ForceStatusType.RefSkillOn:
                    forceStatusMode.RemoveFlagEnum(ForceStatusMode.RefSkillOff);
                    ReloadStatus();
                    break;

                case ForceStatusType.RefSkillOff:
                    forceStatusMode.AddFlagEnum(ForceStatusMode.RefSkillOff);
                    ReloadStatus();
                    break;
                case ForceStatusType.BasicAttackSkillOn:
                    forceStatusMode.RemoveFlagEnum(ForceStatusMode.BasicAttackSkillOff);
                    ReloadStatus();
                    break;

                case ForceStatusType.BasicAttackSkillOff:
                    forceStatusMode.AddFlagEnum(ForceStatusMode.BasicAttackSkillOff);
                    ReloadStatus();
                    break;

                case ForceStatusType.BasicStatusModeOn:
                    forceStatusMode.AddFlagEnum(ForceStatusMode.BasicStatusModeOn);
                    ReloadStatus();
                    break;

                case ForceStatusType.BasicStatusModeOff:
                    forceStatusMode.RemoveFlagEnum(ForceStatusMode.BasicStatusModeOn);
                    ReloadStatus();
                    break;

                case ForceStatusType.BlssBuffOn:
                    forceStatusMode.RemoveFlagEnum(ForceStatusMode.BlssModeOff);
                    ReloadStatus();
                    break;

                case ForceStatusType.BlssBuffOff:
                    forceStatusMode.AddFlagEnum(ForceStatusMode.BlssModeOff);
                    ReloadStatus();
                    break;
            }
        }

        /// <summary>
        /// 스킬 강제 쿨타임 적용
        /// </summary>
        public void ForceStartCooldown(SkillInfo skillInfo)
        {
            if (skillInfo == null)
                return;

            int cooldownRate = battleStatusInfo.CooldownRate; // 쿨타임 감소율
            int clientCoondownTime = skillInfo.GetRealCooldownTime(cooldownRate); // 클라가 계산한 쿨타임
            if (clientCoondownTime > 0)
            {
                if (skillInfo.SlotNo > 0L)
                {
                    // 사용해서 나간 액티브 스킬
                    skillInfo.StartCooldownWithCooldownRate(cooldownRate); // 쿨타임 적용 (클라 전용)
                }
                else
                {
                    battleSkillInfo.SetRefCooldown(skillInfo.SkillId, clientCoondownTime); // 참조스킬 쿨타임 적용
                }
            }
        }

        protected void InitializeExtraOptions(UnitEntity other)
        {
            // extraOptions 는 굳이 안해도 된다.
            forceStatusMode = other.forceStatusMode;
        }

        /// <summary>
        /// 전투 옵션 복사
        /// </summary>
        protected void CloneBattleOptions(UnitEntity other)
        {
            isIgnoreApplyBattleOptions = true;

            battleBuffItemInfo.Clear();
            battleBuffSkillInfo.Clear();

            battleBuffItemInfo.AddRange(other.battleBuffItemInfo);
            battleBuffSkillInfo.AddRange(other.battleBuffSkillInfo);
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        IEnumerator<float> YieldMaxRegen()
        {
            while (true)
            {
                yield return Timing.WaitForOneFrame;
                RegenHp();
            }
        }

        /// <summary>
        /// 대미지 감소율 (보정값)
        /// </summary>
        private float GetDcecreaseDamageRatePer(UnitEntity attacker, UnitEntity target)
        {
            //GetDamageUnitKey()
            // 공격자 및 피격자가 모두 캐릭터 타입
            if ((attacker is CharacterEntity) && (target is CharacterEntity))
            {
                // 보정 레벨에 도달할 경우
                if ((attacker.battleUnitInfo.Level >= duelDecreaseDamageLevel) || (target.battleUnitInfo.Level >= duelDecreaseDamageLevel))
                    return duelDecreaseDamagePer;
            }

            return 0f;
        }

        public void SetIgnoreWeapon(bool isIgnoreWeapon)
        {
            IsIgnoreWeapon = isIgnoreWeapon;
        }

        public void SetIsFreeFightSkillCool(bool isFreeFightSkillCool)
        {
            IsFreeFightSkillCoool = isFreeFightSkillCool;
        }

        /// <summary>
        /// 기본 스탯 반환 (8개)
        /// 체력, 사정거리, 이동속도, 공격속도, 물리공격, 물리방어, 마법공격, 마법방어
        /// </summary>
        public IEnumerable<BasicStatusOptionValue> GetBasicStatusOptions()
        {
            yield return new BasicStatusOptionValue(LocalizeKey._19012, MaxHP.ToString()); // 체력
            int basicAttackRange = battleSkillInfo.basicActiveSkill == null ? 100 : battleSkillInfo.basicActiveSkill.SkillRange;
            int atkRange = (int)MathUtils.ToPermyriadValue(basicAttackRange * battleStatusInfo.AtkRange);
            yield return new BasicStatusOptionValue(LocalizeKey._19013, atkRange.ToString()); // 사정거리
            yield return new BasicStatusOptionValue(LocalizeKey._19014, MathUtils.ToPermyriadText(battleStatusInfo.MoveSpd)); // 이동속도
            yield return new BasicStatusOptionValue(LocalizeKey._19015, MathUtils.ToPermyriadText(battleStatusInfo.AtkSpd)); // 공격속도
            int atk = Mathf.Max(battleStatusInfo.MeleeAtk, battleStatusInfo.RangedAtk);
            yield return new BasicStatusOptionValue(LocalizeKey._19016, atk.ToString()); // 물리공격
            yield return new BasicStatusOptionValue(LocalizeKey._19017, battleStatusInfo.Def.ToString()); // 물리방어
            yield return new BasicStatusOptionValue(LocalizeKey._19018, battleStatusInfo.MAtk.ToString()); // 마법공격
            yield return new BasicStatusOptionValue(LocalizeKey._19019, battleStatusInfo.MDef.ToString()); // 마법방어
        }

        /// <summary>
        /// 추가 스탯 반환 (6개)
        /// </summary>
        public IEnumerable<BasicStatusOptionValue> GetDetailStatusOptions()
        {
            yield return new BasicStatusOptionValue(LocalizeKey._19020, battleStatusInfo.Str.ToString()); // STR
            yield return new BasicStatusOptionValue(LocalizeKey._19021, battleStatusInfo.Vit.ToString()); // VIT
            yield return new BasicStatusOptionValue(LocalizeKey._19022, battleStatusInfo.Agi.ToString()); // AGI
            yield return new BasicStatusOptionValue(LocalizeKey._19023, battleStatusInfo.Dex.ToString()); // DEX
            yield return new BasicStatusOptionValue(LocalizeKey._19024, battleStatusInfo.Int.ToString()); // INT
            yield return new BasicStatusOptionValue(LocalizeKey._19025, battleStatusInfo.Luk.ToString()); // LUK
        }

        public static IEnumerable<BasicStatusOptionValue> GetDefaultBasicOptions()
        {
            yield return new BasicStatusOptionValue(LocalizeKey._19012, "-"); // 체력
            yield return new BasicStatusOptionValue(LocalizeKey._19013, "-"); // 사정거리
            yield return new BasicStatusOptionValue(LocalizeKey._19014, "-"); // 이동속도
            yield return new BasicStatusOptionValue(LocalizeKey._19015, "-"); // 공격속도
            yield return new BasicStatusOptionValue(LocalizeKey._19016, "-"); // 물리공격
            yield return new BasicStatusOptionValue(LocalizeKey._19017, "-"); // 물리방어
            yield return new BasicStatusOptionValue(LocalizeKey._19018, "-"); // 마법공격
            yield return new BasicStatusOptionValue(LocalizeKey._19019, "-"); // 마법방어
        }

        public static IEnumerable<BasicStatusOptionValue> GetDefaultDetailStatusOptions()
        {
            yield return new BasicStatusOptionValue(LocalizeKey._19020, "-"); // STR
            yield return new BasicStatusOptionValue(LocalizeKey._19021, "-"); // VIT
            yield return new BasicStatusOptionValue(LocalizeKey._19022, "-"); // AGI
            yield return new BasicStatusOptionValue(LocalizeKey._19023, "-"); // DEX
            yield return new BasicStatusOptionValue(LocalizeKey._19024, "-"); // INT
            yield return new BasicStatusOptionValue(LocalizeKey._19025, "-"); // LUK
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is UnitEntity)
                return Equals((UnitEntity)obj);

            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 29 + type.GetHashCode();
            hash = hash * 29 + battleUnitInfo.Id.GetHashCode();
            hash = hash * 29 + clientUID.GetHashCode();

            return hash;
        }

        public bool Equals(UnitEntity obj)
        {
            if (obj == null)
                return false;

            return type == obj.type && battleUnitInfo.Id == obj.battleUnitInfo.Id && clientUID == obj.clientUID;
        }

        public static implicit operator bool(UnitEntity entity)
        {
            return entity != null;
        }
    }
}