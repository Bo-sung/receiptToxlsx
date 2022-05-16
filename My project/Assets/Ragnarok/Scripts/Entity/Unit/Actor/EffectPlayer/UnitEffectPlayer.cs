using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public abstract class UnitEffectPlayer : MonoBehaviour, IEntityActorElement<UnitEntity>
        , IEqualityComparer<CrowdControlType>
        , IEqualityComparer<BattlePoolManager.ImpulseType>
        , IEqualityComparer<UnitAuraType>
    {
        private const string EFFECT_ICE_SOUND_FX_NAME = "EF_Ice";
        private const string EFFECT_STUN_LOCK_SOUND_FX_NAME = "EF_Stun";
        private const string EFFECT_CUBE_LOCK_SOUND_FX_NAME = "EF_CubeLock";
        private const string EFFECT_BUBBLE_SOUND_FX_NAME = "poring_die";

        protected UnitActor actor; // Actor
        protected SkillEffectPoolManager skillEffectPool;
        protected IHUDPool hudPool;
        protected IBattlePool battlePool;
        protected SoundManager soundManager;
        protected BattleManager battleManager;

        protected GameObject CachedGameObject { get; private set; }
        protected Transform CachedTransform { get; private set; }
        protected Camera mainCamera;

        private List<PoolObject> poolObjectList;
        private Dictionary<CrowdControlType, PoolObject> crowdControlEffectDic;
        private Dictionary<BattlePoolManager.ImpulseType, ImpulseSourceObject> impulseSourceDic;
        private Dictionary<UnitAuraType, PoolObject> auraEffectDic;
        private HpGaugeBar hpBar;
        private UnitCircle unitCircle;
        private MazeUnitCircle mazeUnitCircle;
        private PoolObject shadow;
        private TargetingLine targetingLine;
        private TargetingArrow targetingArrow;
        private MiddleBossTargetingArrow bossTargetingArrow;
        private MiddleBossTargetingArrow npcTargetingArrow;
        private ChatBalloon chatBalloon;
        private SkillBalloon skillBalloon;
        private LobbyChatBallon lobbyChatBalloon;
        private Transform unitShadowTransform;
        private HudMazeItem hudMazeItem;
        private PoolObject frozen;
        private PoolObject sleeping;
        private PoolObject cubeLock;
        private PoolObject bubble;
        private PoolObject hpHudPostion;
        private UnitAura unitAura;
        private PoolObject powerUpEffect;
        private PoolObject shieldEffect;
        private float fallShadowResizeTime;
        private float fallShadowResizeElapsedTime;

        public System.Action<UnitActor> OnInfo;
        public System.Action<UnitActor> OnChat;
        public System.Action OnBattleClick;
        public System.Action<UnitActor> OnStore; // 개인상점 보기

        UnitEntity entity;
        UnitEntity targetBoss;
        Transform targetNpc;

        protected abstract bool IsCharacter { get; }

        protected bool isNonSkillEffectState;

        protected virtual void Awake()
        {
            actor = GetComponent<UnitActor>();

            CachedGameObject = gameObject;
            CachedTransform = transform;
            mainCamera = Camera.main;

            skillEffectPool = SkillEffectPoolManager.Instance;
            hudPool = HUDPoolManager.Instance;
            battlePool = BattlePoolManager.Instance;
            soundManager = SoundManager.Instance;
            battleManager = BattleManager.Instance;

            poolObjectList = new List<PoolObject>();
            crowdControlEffectDic = new Dictionary<CrowdControlType, PoolObject>(this);
            impulseSourceDic = new Dictionary<BattlePoolManager.ImpulseType, ImpulseSourceObject>(this);
            auraEffectDic = new Dictionary<UnitAuraType, PoolObject>(this);
        }

        protected virtual void OnDestroy()
        {
            Timing.KillCoroutines(GetInstanceID());
        }

        protected virtual void Update()
        {
            UpdateTargetingLine();
            UpdateTargetingArrow();
            UpdateFallingShadowResize();
        }

        protected virtual void LateUpdate()
        {
            UpdateTargetingArrow(bossTargetingArrow, targetBoss);
            UpdateTargetingArrow(npcTargetingArrow, targetNpc);
        }

        public virtual void OnReady(UnitEntity entity)
        {
            this.entity = entity;
        }

        public virtual void OnRelease()
        {
            Timing.KillCoroutines(GetInstanceID());

            if (poolObjectList.Count > 0)
            {
                for (int i = 0; i < poolObjectList.Count; i++)
                {
                    poolObjectList[i].Release();
                }

                poolObjectList.Clear();
            }

            ReleaseCrowdControlEffects();

            if (impulseSourceDic.Count > 0)
            {
                foreach (var item in impulseSourceDic.Values)
                {
                    item.Release();
                }

                impulseSourceDic.Clear();
            }

            if (hpBar != null)
            {
                hpBar.Release();
                hpBar = null;
            }

            if (unitCircle != null)
            {
                unitCircle.Release();
                unitCircle = null;
            }

            if (mazeUnitCircle != null)
            {
                mazeUnitCircle.Release();
                mazeUnitCircle = null;
            }

            if (shadow != null)
            {
                shadow.Release();
                shadow = null;
            }

            if (targetingLine != null)
            {
                targetingLine.Release();
                targetingLine = null;
            }

            if (targetingArrow != null)
            {
                targetingArrow.Release();
                targetingArrow = null;
            }

            ReleaseBossTargetingArrow();
            ReleaseNpcTargetingArrow();

            if (chatBalloon != null)
            {
                chatBalloon.Release();
                chatBalloon = null;
            }

            if (skillBalloon != null)
            {
                skillBalloon.Release();
                skillBalloon = null;
            }

            if (lobbyChatBalloon != null)
            {
                lobbyChatBalloon.Release();
                lobbyChatBalloon = null;
            }

            if (hudMazeItem != null)
            {
                hudMazeItem.Release();
                hudMazeItem = null;
            }

            if (frozen != null)
            {
                frozen.Release();
                frozen = null;
            }

            if (sleeping != null)
            {
                sleeping.Release();
                sleeping = null;
            }

            if (cubeLock != null)
            {
                cubeLock.Release();
                cubeLock = null;
            }

            if (bubble != null)
            {
                bubble.Release();
                bubble = null;
            }

            if (hpHudPostion != null)
            {
                hpHudPostion.Release();
                hpHudPostion = null;
            }

            ReleaseUnitAura();
            ReleasePowerUpEffect();
            ReleaseShieldEffect();

            ReleaseAuraEffects();
        }

        public virtual void AddEvent()
        {
            if (entity)
            {
                entity.OnDie += OnDie;
                entity.OnHit += OnHit;
                entity.OnAttacked += OnAttacked;
                entity.OnDotDamage += OnDotDamage;
                entity.OnRecoveryHp += OnRecoveryHp;
                entity.OnFlee += OnFlee;
                entity.OnChangeHP += OnChangeHP;
                entity.OnChangeCrowdControl += OnChangeCrowdControl;
                entity.OnAutoGuard += OnAutoGuard;
                entity.OnRebirth += OnRebirth;
            }

            AdvancedMovement.OnFallStart += OnFallStart;
            AdvancedMovement.OnFallStop += OnFallStop;
        }

        public virtual void RemoveEvent()
        {
            if (entity)
            {
                entity.OnDie -= OnDie;
                entity.OnHit -= OnHit;
                entity.OnAttacked -= OnAttacked;
                entity.OnDotDamage -= OnDotDamage;
                entity.OnRecoveryHp -= OnRecoveryHp;
                entity.OnFlee -= OnFlee;
                entity.OnChangeHP -= OnChangeHP;
                entity.OnChangeCrowdControl -= OnChangeCrowdControl;
                entity.OnAutoGuard -= OnAutoGuard;
                entity.OnRebirth -= OnRebirth;
            }

            AdvancedMovement.OnFallStart -= OnFallStart;
            AdvancedMovement.OnFallStop -= OnFallStop;
        }

        void OnFallStart(Transform pos)
        {
            if (entity == null)
                return;

            PlayExtraOnFallStart();
        }

        protected virtual void PlayExtraOnFallStart()
        {
            if (!entity.IsEnemy && !entity.IsDie)
            {
                ShowSurprise();
            }
        }

        void OnFallStop(Transform pos)
        {
            if (entity == null)
                return;

            PlayExtraOnFallStop(pos);
        }

        protected virtual void PlayExtraOnFallStop(Transform pos) { }

        /// <summary>
        /// 느낌표 
        /// </summary>
        public void ShowSurprise()
        {
            hudPool.SpawnSurprise(CachedTransform);
        }

        /// <summary>
        /// 유닛 써클 표시
        /// </summary>
        public void ShowUnitCircle(bool isIgnoreDie = false)
        {
            if (!isIgnoreDie && entity.IsDie)
                return;

            if (unitCircle == null)
                unitCircle = battlePool.SpawnCircle(CachedTransform, entity.type);

            unitCircle.CachedGameObject.layer = entity.Layer;

            unitCircle.Show();

            if (mazeUnitCircle)
                mazeUnitCircle.Show();
        }

        public void HideUnitCircle()
        {
            if (unitCircle)
                unitCircle.Hide();

            if (mazeUnitCircle)
                mazeUnitCircle.Hide();
        }

        public void ShowMazeCircle(MazeBattleType battleType)
        {
            if (mazeUnitCircle == null)
                mazeUnitCircle = battlePool.SpawnMazeUnitCircle(CachedTransform);

            mazeUnitCircle.CachedGameObject.layer = entity.Layer;
            mazeUnitCircle.Initialize(battleType);
            mazeUnitCircle.Show();
        }

        public void ShowShadow()
        {
            if (shadow == null)
                shadow = battlePool.SpawnShadow(CachedTransform);

            shadow.Show();
        }

        public void HideShadow()
        {
            if (shadow)
                shadow.Hide();
        }

        /// <summary>
        /// 타겟팅 라인 표시
        /// </summary>
        public void ShowTargetingLine()
        {
            if (targetingLine == null)
                targetingLine = battlePool.SpawnTargetingLine(CachedTransform);
        }

        /// <summary>
        /// 타겟팅 화살표 표시
        /// </summary>
        public void ShowTargetingArrow()
        {
            if (targetingArrow == null)
                targetingArrow = battlePool.SpawnArrow();
        }

        /// <summary>
        /// 보스 타겟팅 화살 표시
        /// </summary>
        public void ShowBossTargetingArrow(UnitEntity targetBoss)
        {
            ReleaseBossTargetingArrow();

            if (targetBoss == null)
                return;

            if (targetBoss is MonsterBotEntity monsterBotEntity)
            {
                if (monsterBotEntity.MonsterType == MonsterType.Boss)
                {
                    this.targetBoss = targetBoss;
                    bossTargetingArrow = battlePool.SpawnBossTargetingArrow(CachedTransform);
                    return;
                }
            }

            switch (targetBoss.type)
            {
                case UnitEntityType.BossMonster:
                    this.targetBoss = targetBoss;
                    bossTargetingArrow = battlePool.SpawnBossTargetingArrow(CachedTransform);
                    break;
                case UnitEntityType.MvpMonster:
                    this.targetBoss = targetBoss;
                    bossTargetingArrow = battlePool.SpawnMvpTargetingArrow(CachedTransform);
                    break;
                case UnitEntityType.NPC:
                    this.targetBoss = targetBoss;
                    bool isDeviruchi = (targetBoss is NpcEntity npcEntity) && npcEntity.GetNpcType() == NpcType.Deviruchi;
                    bossTargetingArrow = battlePool.SpawnNpcTargetingArrow(CachedTransform, isDeviruchi);
                    break;
            }
        }

        /// <summary>
        /// 보스 타겟킹 회수
        /// </summary>
        public void ReleaseBossTargetingArrow()
        {
            targetBoss = null;

            if (bossTargetingArrow != null)
            {
                bossTargetingArrow.Release();
                bossTargetingArrow = null;
            }
        }

        public void ShowNpcTargetingArrow(Transform target)
        {
            ReleaseNpcTargetingArrow();

            if (target == null)
                return;

            targetNpc = target;
            npcTargetingArrow = battlePool.SpawnNpcTargetingArrow(CachedTransform, isDeviruchi: false);
        }

        public void ReleaseNpcTargetingArrow()
        {
            targetNpc = null;

            if (npcTargetingArrow != null)
            {
                npcTargetingArrow.Release();
                npcTargetingArrow = null;
            }
        }

        /// <summary>
        /// 스킬 이펙트
        /// </summary>
        public SkillEffect ShowSkillEffect(string effectName, Vector3 lastPos, int duration)
        {
            if (isNonSkillEffectState)
                return null;

            if (string.IsNullOrEmpty(effectName))
                return null;

            SkillEffect skillEffect = skillEffectPool.Spawn(effectName) as SkillEffect;

            if (skillEffect == null)
                return null;

            skillEffect.CachedTransform.position = lastPos; // 마지막 위치한 곳으로
            skillEffect.SetDuration(duration);

            AddPoolObject(skillEffect); // 추가
            return skillEffect;
        }

        /// <summary>
        /// 스킬 이펙트
        /// </summary>
        public SkillEffect ShowSkillEffect(string effectName, Transform parent, string nodeName, Vector3 offset, Vector3 rotate, bool isAttach, int duration)
        {
            if (isNonSkillEffectState)
                return null;

            if (string.IsNullOrEmpty(effectName))
                return null;

            SkillEffect skillEffect = skillEffectPool.Spawn(effectName) as SkillEffect;

            if (skillEffect == null)
                return null;

            Transform node = GetNode(parent, nodeName); // Node
            skillEffect.CachedTransform.SetParent(node ?? parent, worldPositionStays: false); // Attach
            skillEffect.CachedTransform.localPosition = offset; // Offset
            skillEffect.CachedTransform.localRotation = Quaternion.Euler(rotate); // Rotate

            // 다시 빼주는 작업
            if (!isAttach)
            {
                skillEffect.CachedTransform.SetParent(null, worldPositionStays: true);
                skillEffect.CachedTransform.localScale = Vector3.one;
            }

            skillEffect.SetDuration(duration);

            AddPoolObject(skillEffect); // 추가
            return skillEffect;
        }

        /// <summary>
        /// 스킬 범위 표시
        /// </summary>
        public SkillAreaCircle ShowSkillAreaCircle(LifeCycle lifeCycle, SkillInfo skillInfo, Vector3 casterPos, Vector3 targetPos)
        {
            SkillAreaCircle skillAreaCircle = battlePool.SpawnSkillAreaCircle(Vector3.zero);
            skillAreaCircle.Initialize(lifeCycle, skillInfo, casterPos, targetPos);
            return skillAreaCircle;
        }

        /// <summary>
        /// 말풍선 표시
        /// </summary>
        public void ShowChatBalloon(string text)
        {
            if (chatBalloon == null)
            {
                chatBalloon = hudPool.SpawnChatBalloon(CachedTransform, text, duration: 2);
                chatBalloon.OnRelease += OnDespawnChatBalloon;
            }
            else
            {
                chatBalloon.Initialize(text, duration: 2);
            }
        }

        public void ShowSkillBalloon(string text, string iconName)
        {
            bool isBoss = entity == null ? false : (entity.type == UnitEntityType.MvpMonster || entity.type == UnitEntityType.BossMonster);
            bool isPlayer = entity == null ? false : entity.type == UnitEntityType.Player;
            SkillBalloon.Mode mode = isBoss ? SkillBalloon.Mode.BOSS : SkillBalloon.Mode.DEFAULT;

            if (skillBalloon == null)
            {
                Transform anchor = FindAnchor(mode);
                if (anchor is null)
                {
                    Debug.LogError($"스킬 말풍선 Anchor를 찾지 못함");
                    return;
                }

                skillBalloon = isPlayer ? hudPool.SpawnSkillImageBalloon(anchor) : hudPool.SpawnSkillBalloon(anchor);
                skillBalloon.OnRelease += OnDespawnSkillBalloon;
            }

            skillBalloon.Initialize(text, duration: 2);
            skillBalloon.Set(mode, iconName);
        }

        Transform FindAnchor(SkillBalloon.Mode mode)
        {
            switch (mode)
            {
                case SkillBalloon.Mode.BOSS: // 보스몹인 경우 UIBattleBossHP를 대상으로 띄움
                    return UI.GetUI<UIBattleBossHp>()?.GetHpHudTarget();
            }

            return CachedTransform;
        }

        /// <summary>
        /// 마을 말풍선 표시
        /// </summary>
        /// <param name="text"></param>
        public void ShowLobbyChatBallon(string text)
        {
            if (lobbyChatBalloon == null)
            {
                lobbyChatBalloon = hudPool.SpawnLobbyChatBalloon(CachedTransform, text, duration: 2);
                lobbyChatBalloon.OnRelease += OnDespawnLobbyChatBalloon;
            }
            else
            {
                lobbyChatBalloon.Initialize(text, duration: 2);
            }
        }

        /// <summary>
        /// 미로 아이템 표시 (연출용 Loop)
        /// </summary>
        public void ShowMazeItemLoop()
        {
            if (hudMazeItem == null)
            {
                hudMazeItem = hudPool.SpawnMazeItem(CachedTransform);
                hudMazeItem.OnRelease += OnReleaseHudMazeItem;
            }
        }

        /// <summary>
        /// 미로 아이템 완료
        /// </summary>
        public void ShowMazeItemFinish(int itemType)
        {
            if (hudMazeItem == null)
                ShowMazeItemLoop();

            hudMazeItem.Finish(itemType);
        }

        public abstract void ShowName();

        public virtual void HideName() { }

        public void ShowRandomGroggy(bool isPlaySfx)
        {
            int randNum = Random.Range(0, 3);
            switch (randNum)
            {
                case 0:
                    ShowFrozen(isPlaySfx);
                    break;

                case 1:
                    ShowSleeping(isPlaySfx);
                    break;

                case 2:
                    ShowCubeLock(isPlaySfx);
                    break;
            }
        }

        public void HideGroggy()
        {
            HideFrozen();
            HideSleeping();
            HideCubeLock();
        }

        /// <summary>
        /// 얼음 이펙트
        /// </summary>
        public void ShowFrozen(bool isPlaySfx)
        {
            if (frozen == null)
                frozen = battlePool.SpawnFrozen(CachedTransform);

            if (isPlaySfx)
                PlaySound(EFFECT_ICE_SOUND_FX_NAME, 0f);

            frozen.Show();
        }

        public void HideFrozen()
        {
            if (frozen)
                frozen.Hide();
        }

        /// <summary>
        /// 수면 이펙트
        /// </summary>
        public void ShowSleeping(bool isPlaySfx)
        {
            if (sleeping == null)
                sleeping = battlePool.SpawnSleeping(CachedTransform);

            if (isPlaySfx)
                PlaySound(EFFECT_STUN_LOCK_SOUND_FX_NAME, 0f);

            sleeping.Show();
        }

        public void HideSleeping()
        {
            if (sleeping)
                sleeping.Hide();
        }

        /// <summary>
        /// 큐브 이펙트
        /// </summary>
        public void ShowCubeLock(bool isPlaySfx)
        {
            if (cubeLock == null)
                cubeLock = battlePool.SpawnCubeLock(CachedTransform);

            if (isPlaySfx)
                PlaySound(EFFECT_CUBE_LOCK_SOUND_FX_NAME, 0f);

            cubeLock.Show();
        }

        public void HideCubeLock()
        {
            if (cubeLock)
                cubeLock.Hide();
        }

        /// <summary>
        /// 물거품 이펙트
        /// </summary>
        public void ShowBubble(bool isPlaySfx)
        {
            if (bubble == null)
                bubble = battlePool.SpawnBubble(CachedTransform);

            if (isPlaySfx)
                PlaySound(EFFECT_BUBBLE_SOUND_FX_NAME, 0f);

            bubble.Show();
        }

        public void HideBubble()
        {
            if (bubble)
                bubble.Hide();
        }

        /// <summary>
        /// 특정 아우라 세팅
        /// </summary>
        public void ShowUnitAura(params UnitAura.AuraType[] auraTypes)
        {
            if (unitAura == null)
                unitAura = battlePool.SpawnUnitAura(CachedTransform);

            unitAura.Initialize(auraTypes);
        }

        /// <summary>
        /// 특정 아우라 키기
        /// </summary>
        public void ShowUnitAura(UnitAura.AuraType auraType)
        {
            if (unitAura == null)
                unitAura = battlePool.SpawnUnitAura(CachedTransform);

            unitAura.Show(auraType);
        }

        /// <summary>
        /// 특정 아우라 끄기
        /// </summary>
        public void HideUnitAura(UnitAura.AuraType auraType)
        {
            if (unitAura == null)
                return;

            unitAura.Hide(auraType);
        }

        /// <summary>
        /// 아우라 끄기
        /// </summary>
        public void ReleaseUnitAura()
        {
            if (unitAura == null)
                return;

            unitAura.Release();
            unitAura = null;
        }

        public void SpawnUnitTeleport()
        {
            battlePool.SpawnUnitTeleport(CachedTransform.position);
        }

        public PoolObject SetHpHudTarget(Vector3 pos)
        {
            if (hpHudPostion == null)
                hpHudPostion = battlePool.SpawnHpHudTarget(CachedTransform);

            hpHudPostion.CachedTransform.localPosition = pos;

            return hpHudPostion;
        }

        /// <summary>
        /// 사운드 재생
        /// </summary>
        public void PlaySound(string name, float duration)
        {
            soundManager.PlaySfx(name, duration);
        }

        /// <summary>
        /// 충돌 발생
        /// </summary>
        public void GenerateImpulse(BattlePoolManager.ImpulseType type)
        {
            ImpulseSourceObject impulseSource;

            if (impulseSourceDic.ContainsKey(type))
            {
                impulseSource = impulseSourceDic[type];
            }
            else
            {
                impulseSource = battlePool.SpawnImpulseSource(CachedTransform, type);

                if (impulseSource == null)
                    return;

                impulseSourceDic.Add(type, impulseSource);
            }

            impulseSource.Fire();
        }

        /// <see cref="UpdateFallingShadowResize"/>
        public void PlayFallShadowResize(string shadowName, float fallTime)
        {
            unitShadowTransform = GetNode(CachedTransform, shadowName);
            if (unitShadowTransform == null)
                return;

            fallShadowResizeTime = fallTime;
            fallShadowResizeElapsedTime = 0f;
        }

        /// <summary>
        /// 동전 생성
        /// </summary>
        public void SpawnGold(Vector3 position)
        {
            DroppedItem droppedItem = battlePool.SpawnGold(position);
            AddPoolObject(droppedItem);
        }

        /// <summary>
        /// 튀는 동전 여러개 생성
        /// </summary>
        public void SpawnBounceGolds(Vector3 position, int count)
        {
            BounceDroppedItem[] bounceDroppedItem = battlePool.SpawnBounceGolds(position, count);
            foreach (var item in bounceDroppedItem)
                AddPoolObject(item);
        }

        /// <summary>
        /// 유닛 소울 생성
        /// </summary>
        protected void SpawnUnitSoul(Vector3 start)
        {
            UnitSoul unitSoul = battlePool.SpawnUnitSoul(start);
            unitSoul.Initialize(CachedTransform);
            AddPoolObject(unitSoul);
        }

        /// <summary>
        /// 승천하며 사라지는 HUD 텍스트 생성
        /// </summary>
        public PlainText SpawnPlainText()
        {
            PlainText plainText = hudPool.SpawnPlainText(actor.CachedTransform);
            AddPoolObject(plainText);

            return plainText;
        }

        /// <summary>
        /// 잠시 사라지는 대화
        /// </summary>
        public void SpawnSpeechBalloon(SpeechBalloon.BalloonType type, string text)
        {
            SpeechBalloon plainText = hudPool.SpawnSpeechBalloon(actor.CachedTransform, type, text);
            AddPoolObject(plainText);
        }

        /// <summary>
        /// 패널 버프 이펙트 출력
        /// </summary>
        public PanelBuffEffect PlayPanelBuffEffect()
        {
            PanelBuffEffect panelBuffEffect = battlePool.SpawnPanelBuff(actor.CachedTransform) as PanelBuffEffect;
            AddPoolObject(panelBuffEffect);

            return panelBuffEffect;
        }

        /// <summary>
        /// 전직 이펙트 출력
        /// </summary>
        public void PlayJobChangeEffect()
        {
            SkillEffect effect = skillEffectPool.SpawnJobChangeEffect(actor.CachedTransform);
            AddPoolObject(effect);
        }

        /// <summary>
        /// 레벨업 이펙트 출력
        /// </summary>
        protected void PlayLevelUpEffect()
        {
            SkillEffect effect = skillEffectPool.SpawnLevelUpEffect(CachedTransform);
            AddPoolObject(effect);

            soundManager.PlayLevelUpSfx();
        }

        /// <summary>
        /// 전직 이펙트 출력
        /// </summary>
        protected void PlayJobLevelUpEffect()
        {
            SkillEffect effect = skillEffectPool.SpawnJobLevelUpEffect(CachedTransform);
            AddPoolObject(effect);

            soundManager.PlayLevelUpSfx();
        }

        /// <summary>
        /// 돌진 이펙트 출력
        /// </summary>
        public void PlayRushEffect()
        {
            (SkillEffect frontEffect, SkillEffect backEffect) = skillEffectPool.SpawnRushEffect(actor.CachedTransform);
            AddPoolObject(frontEffect);
            AddPoolObject(backEffect);
        }

        /// <summary>
        /// [미로맵] 타입별 큐브 생성
        /// </summary>
        public virtual void SpawnMonsterCube(MazeBattleType battleType, bool isBoss)
        {
        }

        /// <summary>
        /// [스테이지] 큐브 생성
        /// </summary>
        /// <param name="position"></param>
        public DropCube SpawnDropCube(Vector3 position, UIWidget target)
        {
            DropCube cube = battlePool.SpawnDropCube(position, target);
            AddPoolObject(cube);
            return cube;
        }

        /// <summary>
        /// [스테이지] 마나 생성
        /// </summary>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public DropCube SpawnDropMana(Vector3 position, UIWidget target)
        {
            DropCube mana = battlePool.SpawnDropMana(position, target);
            AddPoolObject(mana);
            return mana;
        }

        /// <summary>
        /// 파워 업 이펙트 생성
        /// </summary>
        public void ShowPowerUpEffect()
        {
            if (powerUpEffect == null)
            {
                powerUpEffect = battlePool.SpawnPowerUpEffect(CachedTransform.parent); // 부모에 생성해주고 위치를 갱신해주는 것으로..
                Timing.RunCoroutineSingleton(YieldPowerUpEffectReposition().CancelWith(CachedGameObject), CachedGameObject, SingletonBehavior.Overwrite);
            }
        }

        IEnumerator<float> YieldPowerUpEffectReposition()
        {
            while (powerUpEffect != null && !powerUpEffect.IsPooled)
            {
                powerUpEffect.CachedTransform.position = CachedTransform.position;
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 파워 업 이펙트 제거
        /// </summary>
        public void ReleasePowerUpEffect()
        {
            if (powerUpEffect == null)
                return;

            powerUpEffect.Release();
            powerUpEffect = null;
        }

        /// <summary>
        /// 실드 이펙트 생성
        /// </summary>
        public void ShowShieldEffect()
        {
            if (shieldEffect == null)
            {
                shieldEffect = battlePool.SpawnShieldEffect(CachedTransform.parent); // 부모에 생성해주고 위치를 갱신해주는 것으로..
                Timing.RunCoroutineSingleton(YieldShieldEffectReposition().CancelWith(CachedGameObject), CachedGameObject, SingletonBehavior.Overwrite);
            }
        }

        IEnumerator<float> YieldShieldEffectReposition()
        {
            while (shieldEffect != null && !shieldEffect.IsPooled)
            {
                shieldEffect.CachedTransform.position = CachedTransform.position;
                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 실드 이펙트 제거
        /// </summary>
        public void ReleaseShieldEffect()
        {
            if (shieldEffect == null)
                return;

            shieldEffect.Release();
            shieldEffect = null;
        }

        /// <summary>
        /// 아우라 이펙트 생성
        /// </summary>
        public void ShowAura(UnitAuraType type)
        {
            if (!auraEffectDic.ContainsKey(type))
                auraEffectDic.Add(type, battlePool.SpawnAuraEffect(CachedTransform, type));

            auraEffectDic[type].Show();
        }

        /// <summary>
        /// 아우라 이펙트 제거
        /// </summary>
        public void ReleaseAura(UnitAuraType type)
        {
            if (!auraEffectDic.ContainsKey(type))
                return;

            auraEffectDic[type].Release();
            auraEffectDic.Remove(type);
        }

        protected virtual void OnDie(UnitEntity unit, UnitEntity attacker)
        {
            ReleaseCrowdControlEffects();
        }

        protected virtual void OnHit(UnitEntity unit, UnitEntity attacker, int value, int count, bool isCritical, bool isBasicActiveSkill, ElementType elementType, int elementFactor)
        {
            if (isCritical)
                GenerateImpulse(BattlePoolManager.ImpulseType.CriticalHit);

            Vector3 unitPos = entity.LastPosition;

            // Hit Effect
            if (isBasicActiveSkill)
            {
                if (elementType == ElementType.None)
                {
                    // 속성이 존재하지 않음
                }
                else
                {
                    SkillEffect hitEffect = skillEffectPool.Spawn(elementType, isCritical);
                    Transform node = GetNode(actor.CachedTransform, "HitPos"); // Node
                    if (node)
                    {
                        hitEffect.CachedTransform.position = node.position;
                    }
                    else
                    {
                        hitEffect.CachedTransform.position = unitPos;
                    }
                }
            }

            // 대미지
            //int elementFactor = elementType.GetElementFactor(unit.battleUnitInfo.UnitElementType);

            int fontSize = Damage.DEFAULT_HIT_FONT_SIZE;
            bool isShowCritical = false;
            Vector3 distance = Vector3.zero;
            if (attacker)
            {
                distance = mainCamera.WorldToViewportPoint(unitPos) - mainCamera.WorldToViewportPoint(attacker.LastPosition);
                distance.z = 0f;
                fontSize = GetDamageFontSize(attacker);
                isShowCritical = IsShowCritical(isCritical); // 플레이어가 맞았을 경우에는 Critical 표시를 보여주지 않는다
            }
            Timing.RunCoroutine(ShowDamage(unit.IsEnemy, value, count, isShowCritical, elementType, distance, fontSize, elementFactor), GetInstanceID());
        }

        protected virtual void OnAttacked(UnitEntity target, SkillType skillType, int skillId, ElementType elementType, bool hasDamage, bool isChainableSkill)
        {
        }

        protected virtual bool IsShowCritical(bool isCritical)
        {
            return isCritical;
        }

        protected int GetDamageFontSize(UnitEntity attacker)
        {
            return attacker.GetDamageFontSize();
        }

        protected virtual void OnDotDamage(UnitEntity unit, CrowdControlType type, int dotDamage)
        {
            Timing.RunCoroutine(ShowDamage(unit.IsEnemy, dotDamage, 1, false, default, Vector3.zero, Damage.DEFAULT_HIT_FONT_SIZE, default), GetInstanceID());
        }

        protected virtual void OnRecoveryHp(int value, int count)
        {
            Heal heal = hudPool.SpawnHeal(CachedTransform);
            heal.Initialize(value * count, Damage.DEFAULT_HIT_FONT_SIZE);
        }

        protected virtual void OnFlee(UnitEntity attacker)
        {
            //float attackerPosZ = (attacker == null) ? 0f : attacker.LastPosition.z;
            //bool isLeft = entity.LastPosition.z < attackerPosZ;
            //bool isPlayerAttacker = attacker == null ? false : attacker.type == UnitEntityType.Player;
            //int fontSize = isPlayerAttacker ? Damage.PLAYER_HIT_FONT_SIZE : Damage.DEFAULT_HIT_FONT_SIZE;
            Damage damage = hudPool.SpawnDamage(CachedTransform);
            damage.InitializeFlee();
        }

        protected virtual void OnChangeHP(int current, int max)
        {
            // Hp가 보이지 않아야 할 때 (큐브 몬스터)
            if (IsHideHp())
                return;

            bool isFullHPInvisible = IsHideFullHP();

            // 현재 체력이 0이거나, 몬스터이면서 피가 가득 찼을 경우에
            if (entity.IsDie || (isFullHPInvisible && entity.IsMaxHp))
            {
                if (hpBar != null)
                {
                    hpBar.Release();
                    hpBar = null;
                }
            }
            else
            {
                if (hpBar == null)
                {
                    hpBar = hudPool.SpawnHPBar(CachedTransform);
                    hpBar.Initialize(entity.IsEnemy);
                }

                hpBar.SetProgress(current, max);
            }
        }

        protected virtual bool IsHideFullHP()
        {
            return true;
        }

        protected virtual bool IsHideHp()
        {
            return false;
        }

        private void OnChangeCrowdControl(CrowdControlType type, bool isGroggy, int overapCount)
        {
            if (overapCount > 0)
            {
                if (!crowdControlEffectDic.ContainsKey(type))
                {
                    PoolObject poolObject = battlePool.SpawnCrowdControlEffect(CachedTransform, type);

                    if (poolObject == null)
                    {
                        Debug.LogError($"상태이상 이펙트가 존재하지 않습니다: {nameof(type)} = {type}");
                        return;
                    }

                    crowdControlEffectDic.Add(type, poolObject); // Dictionay 추가
                }
            }
            else
            {
                if (crowdControlEffectDic.ContainsKey(type))
                {
                    PoolObject poolObject = crowdControlEffectDic[type];
                    poolObject.Release(); // 해제
                    crowdControlEffectDic.Remove(type); // Dictionay 제거
                }
            }
        }

        private void OnAutoGuard()
        {
            PoolObject poolObject = battlePool.SpawnAutoGuard(CachedTransform);

            if (poolObject == null)
            {
                Debug.LogError("오토카드 이펙트가 존재하지 않습니다");
                return;
            }
        }

        private void OnRebirth()
        {
            ShowRebirthEffect();
        }

        private void UpdateTargetingLine()
        {
            if (targetingLine == null)
                return;

            // 내가 죽어있거나, 타겟이 없거나, 타겟이 죽었거나
            UnitActor target = actor.AI.FixedTarget;
            if (entity.IsDie || target == null || target.Entity.IsDie)
            {
                targetingLine.Hide();
            }
            else
            {
                targetingLine.SetPosition(entity.LastPosition, target.Entity.LastPosition);
            }
        }

        private void UpdateTargetingArrow()
        {
            if (targetingArrow == null)
                return;

            // 내가 죽어있거나, 타겟이 없거나, 타겟이 죽었거나
            UnitActor target = actor.AI.Target;
            if (entity.IsDie || target == null || target.Entity.IsDie)
            {
                targetingArrow.Hide();
            }
            else
            {
                targetingArrow.SetPosition(target.Entity.LastPosition, Vector3.up * 2.5f);
            }
        }

        private void UpdateTargetingArrow(MiddleBossTargetingArrow arrow, UnitEntity target)
        {
            if (arrow == null)
                return;

            if (entity.IsDie || target == null || target.IsDie || target.GetActor() == null)
            {
                arrow.Hide();
            }
            else
            {
                arrow.SetPosition(entity.LastPosition, target.LastPosition);
            }
        }

        private void UpdateTargetingArrow(MiddleBossTargetingArrow arrow, Transform target)
        {
            if (arrow == null)
                return;

            if (entity.IsDie || target == null)
            {
                arrow.Hide();
            }
            else
            {
                arrow.SetPosition(entity.LastPosition, target.position);
            }
        }

        /// <see cref="PlayFallShadowResize(string, float)">
        private void UpdateFallingShadowResize()
        {
            if (unitShadowTransform == null)
                return;

            // 위치 세팅
            Vector3 shadowPosition = unitShadowTransform.position;
            shadowPosition.y = Constants.Map.POSITION_Y;
            unitShadowTransform.position = shadowPosition;

            //	크기 세팅
            float t = Mathf.Clamp01(fallShadowResizeElapsedTime / fallShadowResizeTime);
            float shadowSize = Mathf.Lerp(Constants.Battle.FALL_SHADOW_MAX_SIZE, 1f, t);
            unitShadowTransform.localScale = Vector3.one * shadowSize;

            // 리사이징 완료하면 더이상 업데이트 시키지 않도록 ..
            if (t == 1f)
                unitShadowTransform = null;

            fallShadowResizeElapsedTime += Time.deltaTime;
        }

        /// <summary>
        /// Find Recursive
        /// </summary>
        protected Transform GetNode(Transform tf, string name)
        {
            if (tf == null)
                return null;

            if (string.IsNullOrEmpty(name))
                return null;

            if (tf.name.Equals(name))
                return tf;

            // 재귀함수를 통하여 모든 Transform 의 name 을 찾음
            for (int i = 0; i < tf.childCount; ++i)
            {
                Transform child = GetNode(tf.GetChild(i), name);

                if (child)
                    return child;
            }

            return null;
        }

        private void AddPoolObject(PoolObject obj)
        {
            obj.OnRelease += RemovePoolObject;
            poolObjectList.Add(obj);
        }

        private void RemovePoolObject(PoolObject obj)
        {
            poolObjectList.Remove(obj);
            obj.OnRelease -= RemovePoolObject;
        }

        void OnDespawnChatBalloon(PoolObject obj)
        {
            chatBalloon.OnRelease -= OnDespawnChatBalloon;
            chatBalloon = null;
        }

        void OnDespawnSkillBalloon(PoolObject obj)
        {
            skillBalloon.OnRelease -= OnDespawnSkillBalloon;
            skillBalloon = null;
        }

        void OnDespawnLobbyChatBalloon(PoolObject obj)
        {
            lobbyChatBalloon.OnRelease -= OnDespawnLobbyChatBalloon;
            lobbyChatBalloon = null;
        }

        void OnReleaseHudMazeItem(PoolObject obj)
        {
            hudMazeItem.OnRelease -= OnReleaseHudMazeItem;
            hudMazeItem = null;
        }

        IEnumerator<float> ShowDamage(bool isEnemy, int value, int count, bool isShowCritical, ElementType elementType, Vector3 distance, int fontSize, int elementFactor)
        {
            // 속성 증감율
            float elementRate = (elementFactor - 10000) / 10000f;

            if (count > 1)
            {
                TotalDamage totalDamage = hudPool.SpawnTotalDamage(CachedTransform);

                // 속성 대미지 프리팹
                if (Mathf.Abs(elementRate) >= 0.01f) // 1%보다 작은 변동치라면 무시
                {
                    ElementalDamageBonus elementalDamageBonus = hudPool.SpawnElementalDamageBonus(CachedTransform);
                    elementalDamageBonus.Initialize(elementType, elementRate);
                }

                int totalValue = 0;
                for (int i = 0; i < count; i++)
                {
                    Damage damage = hudPool.SpawnDamage(CachedTransform);
                    damage.Initialize(value, isShowCritical, isEnemy, distance, fontSize);

                    totalValue += value;
                    totalDamage.Initialize(totalValue, Damage.DEFAULT_HIT_FONT_SIZE);
                    yield return Timing.WaitForSeconds(0.1f);
                }
            }
            else
            {
                Damage damage = hudPool.SpawnDamage(CachedTransform);
                damage.Initialize(value, isShowCritical, isEnemy, distance, fontSize);

                // 속성 대미지 프리팹
                if (Mathf.Abs(elementRate) >= 0.01f) // 1%보다 작은 변동치라면 무시
                {
                    ElementalDamageBonus elementalDamageBonus = hudPool.SpawnElementalDamageBonus(CachedTransform);
                    elementalDamageBonus.Initialize(elementType, elementRate);
                }
            }
        }

        public virtual void ShowMazeMonsterInfo() { }

        public virtual void ShowBattleHUD() { }
        public virtual void HideBattleHUD() { }
        public virtual void SetUnitInfo() { }
        public virtual void SetStoreBalloon() { }
        public virtual void ShowRebirthEffect() { }

        public void ShowHealEffect()
        {
            battlePool.SpawnHeal(CachedTransform);
        }

        private void ReleaseCrowdControlEffects()
        {
            if (crowdControlEffectDic.Count == 0)
                return;

            foreach (var item in crowdControlEffectDic.Values)
            {
                item.Release();
            }

            crowdControlEffectDic.Clear();
        }

        private void ReleaseAuraEffects()
        {
            if (auraEffectDic.Count == 0)
                return;

            foreach (var item in auraEffectDic.Values)
            {
                item.Release();
            }

            auraEffectDic.Clear();
        }

        bool IEqualityComparer<CrowdControlType>.Equals(CrowdControlType x, CrowdControlType y)
        {
            return x == y;
        }

        int IEqualityComparer<CrowdControlType>.GetHashCode(CrowdControlType obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer<BattlePoolManager.ImpulseType>.Equals(BattlePoolManager.ImpulseType x, BattlePoolManager.ImpulseType y)
        {
            return x == y;
        }

        int IEqualityComparer<BattlePoolManager.ImpulseType>.GetHashCode(BattlePoolManager.ImpulseType obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer<UnitAuraType>.Equals(UnitAuraType x, UnitAuraType y)
        {
            return x == y;
        }

        int IEqualityComparer<UnitAuraType>.GetHashCode(UnitAuraType obj)
        {
            return obj.GetHashCode();
        }
    }
}