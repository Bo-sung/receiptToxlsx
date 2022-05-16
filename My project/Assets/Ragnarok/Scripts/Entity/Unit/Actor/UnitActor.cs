#define TEST_BUILD

using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    [SelectionBase]
    public abstract class UnitActor : EntityActor<UnitEntity>, ITargetable
    {
        private class Parameter
        {
            public SkillType skillType;
            public UnitActor target;
            public SkillInfo skillInfo;
            public SkillSetting skillSetting;
            public float attackSpeedRate;
            public ISkillArea skillArea;
            public bool queueIdleMotion;
            public bool isItemSkill;
        }

        private ISkillSettingContainer skillSettingContainer;
        private IProjectileSettingContainer projectileSettingContainer;
        protected BattleManager battleManager;

        /// <summary>
        /// 인공지능 관리자
        /// </summary>
        public UnitAI AI { get; private set; }

        /// <summary>
        /// 애니메이션 관리자
        /// </summary>
        public UnitAnimator Animator { get; private set; }

        /// <summary>
        /// 외형 관리자
        /// </summary>
        public UnitAppearance Appearance { get; private set; }

        /// <summary>
        /// 움직임 관리자
        /// </summary>
        public UnitMovement Movement { get; private set; }

        /// <summary>
        /// 이펙트 플레이어
        /// </summary>
        public UnitEffectPlayer EffectPlayer { get; private set; }

        /// <summary>
        /// 유닛감지 관리자
        /// </summary>
        public UnitRadar Radar { get; private set; }

        /// <summary>
        /// 현재 Actor가 소속된 Entity
        /// </summary>
        public UnitEntity Entity { get; private set; }

        /// <summary>
        /// 피격 시 틴트 칼라 이펙트 플레이어
        /// </summary>
        public UnitHitEffect HitTintPlayer { get; private set; }

        /// <summary>
        /// 사용중인 스킬의 애니메이션 시간
        /// </summary>
        private RelativeRemainTime useSkillAnimDuration;

        protected LifeCycle lifeCycle = new LifeCycle();

        private Dictionary<int, int> skillCountDic;

        protected override void Awake()
        {
            base.Awake();

            useSkillAnimDuration = new RelativeRemainTime();
            skillSettingContainer = AssetManager.Instance;
            projectileSettingContainer = AssetManager.Instance;
            battleManager = BattleManager.Instance;
            skillCountDic = new Dictionary<int, int>(IntEqualityComparer.Default);
        }

        protected virtual void LateUpdate()
        {
            if (skillCountDic.Count > 0)
            {
                skillCountDic.Clear();
            }
        }

        public override void OnCreate(IPooledDespawner despawner, string poolID)
        {
            base.OnCreate(despawner, poolID);

            AI = GetComponent<UnitAI>();
            Animator = GetComponent<UnitAnimator>();
            Appearance = GetComponent<UnitAppearance>();
            Movement = GetComponent<UnitMovement>();
            EffectPlayer = GetComponent<UnitEffectPlayer>();
            Radar = GetComponent<UnitRadar>();
        }

        public override void OnReady(UnitEntity entity)
        {
            Entity = entity;

            if (AI)
            {
                AI.enabled = !Entity.IsUI; // 인공지능 설정
            }

            if (Movement)
            {
                Movement.enabled = !Entity.IsUI; // 움직임 설정
            }
        }

        public override void OnRelease()
        {
            Timing.KillCoroutines(GetInstanceID());
            lifeCycle.Dispose();
        }

        public override void AddEvent()
        {
            if (Entity)
            {
                Entity.OnChangeCrowdControl += OnChangeCrowdControl;
                Entity.OnDamaged += OnDamaged;
                Entity.OnAttacked += OnAttacked;
            }

            if (Movement)
            {
                Movement.OnMoveStart += Animator.PlayRun; // 이동 시작
                Movement.OnMoveStop += OnMoveStop; // 이동 정지
            }
        }

        public override void RemoveEvent()
        {
            if (Entity)
            {
                Entity.OnChangeCrowdControl -= OnChangeCrowdControl;
                Entity.OnDamaged -= OnDamaged;
                Entity.OnAttacked -= OnAttacked;
            }

            if (Movement)
            {
                Movement.OnMoveStart -= Animator.PlayRun; // 이동 시작
                Movement.OnMoveStop -= OnMoveStop; // 이동 정지
            }
        }

        /// <summary>
        /// 타겟 타입 해당 여부 반환
        /// </summary>
        public virtual bool IsLookTarget(UnitActor other, TargetType targetType)
        {
            // 타겟 무시 타입의 경우 타겟이 될 수 없다.
            if (other.Entity.IsIgnoreTarget)
                return false;

            if (!IsCheckTarget(targetType, other.Entity.IsEnemy))
                return false;

            return other.CanBeLookTarget(targetType);
        }

        /// <summary>
        /// 타겟 타입 스킬 영향 여부
        /// </summary>
        public virtual bool IsSkillTarget(UnitActor other, TargetType targetType)
        {
            // 타겟 무시 타입의 경우 타겟이 될 수 없다.
            if (other.Entity.IsIgnoreTarget)
                return false;

            if (!IsCheckTarget(targetType, other.Entity.IsEnemy))
                return false;

            return other.CanBeSkillTarget(targetType);
        }

        /// <summary>
        /// 타겟팅 가능 여부
        /// </summary>
        protected abstract bool CanBeLookTarget(TargetType targetType);

        /// <summary>
        /// 스킬 가능 여부 (Default: 타겟팅 가능 여부)
        /// </summary>
        protected virtual bool CanBeSkillTarget(TargetType targetType)
        {
            return CanBeLookTarget(targetType);
        }

        /// <summary>
        /// 타겟 체크
        /// </summary>
        private bool IsCheckTarget(TargetType targetType, bool isEnemy)
        {
            switch (targetType)
            {
                case TargetType.Allies:
                case TargetType.AlliesCharacter:
                case TargetType.AlliesCupet:
                    return Entity.IsEnemy == isEnemy; // 같을 경우

                case TargetType.Enemy:
                case TargetType.EnemyCharacter:
                case TargetType.EnemyCupet:
                    return Entity.IsEnemy != isEnemy; // 다를 경우
            }

            return false;
        }

        /// <summary>
        /// 해당 Target 과 Distance 안에 있을 경우에 True
        /// </summary>
        public virtual bool IsCheckDistance(UnitActor other, float distance)
        {
            // -1 일 경우에는 무조건 OK
            if (distance == -1f)
                return true;

            float dist = GetDistance(other);
            float totalRadius = Appearance.GetRadius() + other.Appearance.GetRadius();
            dist -= totalRadius * totalRadius; // 반지름 값 고려

            // 찾고자 하는 거리보다 멀다
            if (dist > distance * distance)
                return false;

            return true;
        }

        public float GetDistance(UnitActor other)
        {
            return (Entity.LastPosition - other.Entity.LastPosition).sqrMagnitude;
        }

        /// <summary>
        /// 서버 스킬 쿨타임 체크 유무
        /// </summary>
        public virtual bool IsRequestSkillCooltime()
        {
            return false;
        }

        /// <summary>
        /// 스킬 쿨타임 체크
        /// </summary>
        public virtual void RequestSkillCooltimeCheck(SkillInfo info)
        {
            if (BattleManager.isUseSkillPoint)
            {
                if (info.SlotNo == 0L)
                {
                    // 참조 스킬이 스킬 초월의 경우
                    if (info.RefBattleOption == BattleOptionType.SkillOverride)
                    {
                        int remainMp = Entity.CurMp - info.MpCost;
                        Entity.SetCurrentMp(remainMp);
                    }
                }
            }
        }

        /// <summary>
        /// 몸통 연결
        /// </summary>
        public void LinkBody(GameObject body)
        {
            Animator.SetBody(body);
            HitTintPlayer = body.GetComponent<UnitHitEffect>();
            Link(body);

            if (HitTintPlayer)
            {
                if (IsShowDark())
                {
                    HitTintPlayer.SetDark();
                }
            }
        }

        /// <summary>
        /// 자식 연결
        /// </summary>
        private void Link(GameObject child)
        {
            if (child == null)
                return;

            int layer = CachedGameObject.layer;
            Transform tfChild = child.transform;

            // 레이어 세팅
            if (layer != child.layer)
            {
                child.layer = layer;
                tfChild.SetChildLayer(layer);
            }

            tfChild.SetParent(CachedTransform, worldPositionStays: false);
            tfChild.localPosition = Vector3.zero;
            tfChild.localRotation = Quaternion.identity;
            //tfChild.localScale = Vector3.one;
        }

        private void OnChangeCrowdControl(CrowdControlType type, bool isGroggy, int overapCount)
        {
            bool isDie = Entity != null && Entity.IsDie;
            if (!isDie && overapCount > 0)
            {
                if (isGroggy)
                    Animator.PlayDebuff(); // Debuff 애니메이션
            }
        }

        private void OnDamaged(UnitEntity attacker, SkillType skillType, ElementType elementType, bool hasDamage, bool isChainableSkill)
        {
            // 연계로 인해 발동된 스킬의 경우 아무것도 하지 않는다
            if (isChainableSkill)
                return;

            // 공격자 없음
            if (attacker.IsDie || attacker.GetActor() == null)
                return;

            // 피격 스킬 연계
            TargetUnit targetUnit = battleManager.unitList.FindSkillTarget(this, attacker.GetActor(), ActiveSkill.Type.Attack, Entity.battleSkillInfo.GetBeAttackedActiveSkills(elementType));
            if (!targetUnit.IsInvalid())
                UseSkill(targetUnit.target, targetUnit.selectedSkill, isChainableSkill: true);
        }

        private void OnAttacked(UnitEntity target, SkillType skillType, int skillId, ElementType elementType, bool hasDamage, bool isChainableSkill)
        {
            // 연계로 인해 발동된 스킬의 경우 아무것도 하지 않는다
            if (isChainableSkill)
                return;

            // 피격자 없음
            if (target.IsDie || target.GetActor() == null)
                return;

            // 추가 스킬 연계
            UseChainAttackSkill(target, elementType, skillId);
        }

        /// <summary>
        /// 연계 스킬 사용 (공격)
        /// </summary>
        public void UseChainAttackSkill(UnitEntity target, ElementType elementType, int skillId)
        {
            TargetUnit targetUnit;

            // Attack (특정속성연계)
            targetUnit = battleManager.unitList.FindSkillTarget(this, target.GetActor(), ActiveSkill.Type.Attack, Entity.battleSkillInfo.GetBlowActiveSkills(elementType));
            if (!targetUnit.IsInvalid())
                UseSkill(targetUnit.target, targetUnit.selectedSkill, isChainableSkill: true);

            // Attack (특정스킬연계)
            targetUnit = battleManager.unitList.FindSkillTarget(this, target.GetActor(), ActiveSkill.Type.Attack, Entity.battleSkillInfo.GetBlowActiveSkills(skillId));
            if (!targetUnit.IsInvalid())
                UseSkill(targetUnit.target, targetUnit.selectedSkill, isChainableSkill: true);

            // Buff (특정속성연계)
            targetUnit = battleManager.unitList.FindSkillTarget(this, target.GetActor(), ActiveSkill.Type.Buff, Entity.battleSkillInfo.GetBlowActiveSkills(elementType));
            if (!targetUnit.IsInvalid())
                UseSkill(targetUnit.target, targetUnit.selectedSkill, isChainableSkill: true);

            // Buff (특정스킬연계)
            targetUnit = battleManager.unitList.FindSkillTarget(this, target.GetActor(), ActiveSkill.Type.Buff, Entity.battleSkillInfo.GetBlowActiveSkills(skillId));
            if (!targetUnit.IsInvalid())
                UseSkill(targetUnit.target, targetUnit.selectedSkill, isChainableSkill: true);

            // Recovery (특정속성연계)
            targetUnit = battleManager.unitList.FindSkillTarget(this, target.GetActor(), ActiveSkill.Type.RecoveryHp, Entity.battleSkillInfo.GetBlowActiveSkills(elementType));
            if (!targetUnit.IsInvalid())
                UseSkill(targetUnit.target, targetUnit.selectedSkill, isChainableSkill: true);

            // Recovery (특정스킬연계)
            targetUnit = battleManager.unitList.FindSkillTarget(this, target.GetActor(), ActiveSkill.Type.RecoveryHp, Entity.battleSkillInfo.GetBlowActiveSkills(skillId));
            if (!targetUnit.IsInvalid())
                UseSkill(targetUnit.target, targetUnit.selectedSkill, isChainableSkill: true);
        }

        public SkillSetting GetSkillSetting(SkillInfo skillInfo)
        {
            return skillSettingContainer.Get(skillInfo.EffectID); // 스킬 세팅
        }

        public bool GetSkillRelatedInfos(int skillID, out SkillInfo skillInfo, out SkillSetting skillSetting)
        {
            var activeSkills = Entity.battleSkillInfo.GetActiveSkills();
            skillInfo = null;
            skillSetting = null;

            foreach (var each in activeSkills)
            {
                if (each.SkillId == skillID)
                {
                    skillInfo = each;
                    break;
                }
            }

            if (skillInfo == null)
            {
                if (Entity.battleSkillInfo.basicActiveSkill.SkillId == skillID)
                    skillInfo = Entity.battleSkillInfo.basicActiveSkill;
            }

            if (skillInfo == null)
            {
#if UNITY_EDITOR
                Debug.LogError("몬스터에 스킬이 존재하지 않습니다.");
#endif
                return false;
            }

            skillSetting = GetSkillSetting(skillInfo);
            if (skillSetting == null)
            {
#if UNITY_EDITOR
                Debug.LogError("몬스터에 스킬 세팅이 존재하지 않습니다.");
#endif
                return false;
            }

            return true;
        }

        public void CancelSkill()
        {
            Timing.KillCoroutines(GetInstanceID());
            lifeCycle.Dispose();
        }

        /// <summary>
        /// 액티브스킬 모션 플레이 상태 반환
        /// </summary>
        public bool IsPlayingActiveSkill()
        {
            if (AI.UsedSkill == null || AI.UsedSkill.IsBasicActiveSkill)
                return false;

            return useSkillAnimDuration.GetRemainTime() > 0f;
        }

        /// <summary>
        /// 평타스킬 모션 플레이 상태 반환
        /// </summary>
        public bool IsPlayingBasicActiveSkill()
        {
            if (AI.UsedSkill == null || !AI.UsedSkill.IsBasicActiveSkill)
                return false;

            return useSkillAnimDuration.GetRemainTime() > 0f;
        }

        /// <summary>
        /// 어둡게 보이게 할 지 여부
        /// </summary>
        protected virtual bool IsShowDark()
        {
            return false;
        }

        public (float duration, string aniName) UseSkill(UnitActor target, SkillInfo skillInfo, bool isChainableSkill, bool isRequestServer = true, bool queueIdleMotion = false)
        {
            if (skillCountDic.ContainsKey(skillInfo.SkillId))
            {
                if (skillCountDic[skillInfo.SkillId] == BasisType.SKILL_CHAIN_MAX_COUNT.GetInt())
                {
                    Debug.Log($"스킬 최대 횟수 초과 {skillInfo.SkillId} ({skillInfo.SkillName})");
                    return (0.0f, string.Empty);
                }

                ++skillCountDic[skillInfo.SkillId]; // 스킬 카운트 추가
            }
            else
            {
                skillCountDic.Add(skillInfo.SkillId, 1);
            }

            SkillSetting skillSetting = GetSkillSetting(skillInfo); // 스킬 세팅
            if (skillSetting == null) // 스틸 세팅 값이 존재하지 않음
                return (0.0f, string.Empty);

            SkillType skillType = skillInfo.SkillType;
            float attackSpeedRate = 1f; // 공속 비율
            if (skillType == SkillType.BasicActiveSkill)
                attackSpeedRate = 1f / Entity.AttackSpeedRate;

            // 스킬 애니메이션 시간 기록
            float attackAnimDelay = skillInfo.ChasingTime + skillInfo.CastingTime;
            float skillAnimDuration = Animator.GetClipLength(skillSetting.aniName) ?? default;
            float totalDuration = attackAnimDelay + skillAnimDuration * attackSpeedRate;
            useSkillAnimDuration = totalDuration;

            DetectCrowdControlState(skillType, time: useSkillAnimDuration);

            // 공격 전에 돌아보기
            if (!AI.IsLookFixed)
                CachedTransform.LookAt(target.Entity.LastPosition);

            Parameter parameter = new Parameter()
            {
                skillType = skillType,
                target = target,
                skillInfo = skillInfo,
                skillSetting = skillSetting,
                attackSpeedRate = attackSpeedRate,
                skillArea = null,
                queueIdleMotion = queueIdleMotion,
                isItemSkill = false,
            };

            if (skillInfo.ChasingTime > 0)
                ChaseTarget(parameter);
            else
                CastSkill(parameter);

            // 돌진기
            if (skillInfo.IsRush && !BattleManager.isAntiChaseSkill)
            {
                //Movement.Rush(target.LastPosition, skillInfo.GetSkillRange(Entity.AtkRangeRate));
                Movement.Rush(target.Entity.LastPosition, Constants.Battle.RushTime);
            }
            else
            {
                Movement.Stop();
            }

            if (Entity.State != UnitEntity.UnitState.GVGMultiPlayer
                && Entity.State != UnitEntity.UnitState.TempStagePlayerOther
                && !Entity.IsFreeFightSkillCoool) // GVGMultiPlayer는 쿨타임을 무시한다.
            {
                // 서버 호출 - 쿨타임 적용
                if (isRequestServer && IsRequestSkillCooltime())
                {
                    RequestSkillCooltimeCheck(skillInfo);
                }
                else
                {
                    ForceStartCooldown(skillInfo); // 강제 쿨타임 적용
                }
            }

            // 스킬 사용 이벤트 발생
            Entity.InvokeUseSkill(target.Entity, skillInfo);

            if (attackAnimDelay > 0)
            {
                if (skillSetting.castingAniName == null || skillSetting.castingAniName.Length == 0)
                    Entity.GetActor().Animator.PlayIdle();
                else
                    Entity.GetActor().Animator.Play(skillSetting.castingAniName, skillInfo.SkillType == SkillType.BasicActiveSkill ? UnitAnimator.AniType.BasicAttack : default);
            }
            else
            {
                Entity.GetActor().Animator.Play(skillSetting.aniName, skillInfo.SkillType == SkillType.BasicActiveSkill ? UnitAnimator.AniType.BasicAttack : default);
                if (queueIdleMotion && Entity.GetActor().Animator is CharacterAnimator)
                    (Entity.GetActor().Animator as CharacterAnimator).QueueIdle();
            }

            return (totalDuration, skillSetting.aniName);
        }

        public void UseItemSkill(SkillInfo skillInfo)
        {
            SkillSetting skillSetting = GetSkillSetting(skillInfo); // 스킬 세팅
            if (skillSetting == null) // 스틸 세팅 값이 존재하지 않음
                return;

            SkillType skillType = skillInfo.SkillType;
            float attackSpeedRate = 1f; // 공속 비율
            if (skillType == SkillType.BasicActiveSkill)
                attackSpeedRate = 1f / Entity.AttackSpeedRate;

            Parameter parameter = new Parameter()
            {
                skillType = skillType,
                target = this,
                skillInfo = skillInfo,
                skillSetting = skillSetting,
                attackSpeedRate = attackSpeedRate,
                skillArea = null,
                queueIdleMotion = false,
                isItemSkill = true,
            };

            CastSkill(parameter);
            ForceStartCooldown(skillInfo); // 강제 쿨타임 적용

            // 스킬 사용 이벤트 발생
            Entity.InvokeUseSkill(Entity, skillInfo);
        }

        /// <summary>
        /// 강제 쿨타임 적용
        /// </summary>
        protected virtual void ForceStartCooldown(SkillInfo skillInfo)
        {
            Entity.ForceStartCooldown(skillInfo); // 쿨타임 적용 (클라 전용)
        }

        /// <summary>
        /// 화면 밖으로 유닛을 날림
        /// </summary>
        [System.Obsolete]
        public void HideOnSky()
        {
            CachedTransform.position = Vector3.one * 50000f;
        }

        /// <summary>
        /// 상태이상으로 인해 스킬 캔슬 시, 스킬 사용 취소 처리
        /// </summary>
        private void DetectCrowdControlState(SkillType skillType, float time)
        {
            Timing.RunCoroutine(YieldDetectCrowdControlState(skillType, time), GetInstanceID());
        }

        private void ChaseTarget(Parameter p)
        {
            Timing.RunCoroutine(YieldChaseTarget(p), GetInstanceID());
        }

        private void CastSkill(Parameter p)
        {
            if (p.skillInfo.CastingTime > 0)
                Timing.RunCoroutine(YieldCastSkill(p), GetInstanceID());
            else
                DischargeSkill(p);
        }

        private void DischargeSkill(Parameter p)
        {
            if (p.isItemSkill)
            {
                // Do Nothing (아이템 스킬의 경우)
            }
            else
            {
                // 공격 애니메이션이 지연됬을 경우에는 이 메서드가 호출되는 시점에 애니메이션 실행
                if (p.skillInfo.ChasingTime > 0 || p.skillInfo.CastingTime > 0)
                {
                    Entity.GetActor().Animator.Play(p.skillSetting.aniName, p.skillInfo.SkillType == SkillType.BasicActiveSkill ? UnitAnimator.AniType.BasicAttack : default);
                    if (p.queueIdleMotion && Entity.GetActor().Animator is CharacterAnimator)
                        (Entity.GetActor().Animator as CharacterAnimator).QueueIdle();
                }
            }

            Damage(p);
            ShowEffect(p);
            PlaySound(p);
            LaunchProjectile(p);
        }

        public void ShowEffectAndProjectile(UnitActor target, SkillInfo skillInfo, bool queueIdleMotion = false)
        {
            if (skillInfo == null)
                return;

            SkillSetting skillSetting = GetSkillSetting(skillInfo); // 스킬 세팅
            if (skillSetting == null) // 스틸 세팅 값이 존재하지 않음
                return;

            SkillType skillType = skillInfo.SkillType;
            float attackSpeedRate = 1f; // 공속 비율
            if (skillType == SkillType.BasicActiveSkill)
                attackSpeedRate = 1f / Entity.AttackSpeedRate;

            Parameter parameter = new Parameter()
            {
                skillType = skillType,
                target = target,
                skillInfo = skillInfo,
                skillSetting = skillSetting,
                attackSpeedRate = attackSpeedRate,
                skillArea = null,
                queueIdleMotion = queueIdleMotion,
                isItemSkill = false,
            };

            ShowEffect(parameter);
            LaunchProjectile(parameter);
        }

        private void Damage(Parameter p)
        {
            Timing.RunCoroutine(YieldDamage(p), GetInstanceID());
        }

        private void ShowEffect(Parameter p)
        {
            if (p.skillSetting.arrVfx == null)
                return;

            foreach (var item in p.skillSetting.arrVfx)
            {
                float time = (item.time * 0.01f) * p.attackSpeedRate;
                Timing.RunCoroutine(YieldShowEffect(p, item, time), GetInstanceID());
            }
        }

        private void PlaySound(Parameter p)
        {
            if (p.skillSetting.arrSound == null)
                return;

            foreach (var item in p.skillSetting.arrSound)
            {
                float time = (item.time * 0.01f) * p.attackSpeedRate;
                Timing.RunCoroutine(YieldPlaySound(p.skillType, item, time, p.isItemSkill), GetInstanceID());
            }
        }

        private void LaunchProjectile(Parameter p)
        {
            if (p.skillSetting.arrProjectile == null)
                return;

            foreach (var item in p.skillSetting.arrProjectile)
            {
                ProjectileSetting projectileSetting = projectileSettingContainer.Get(item.name);

                if (projectileSetting == null)
                {
                    Debug.LogError($"존재하지 않는 발사체 입니다: name = {item.name}");
                    continue;
                }

                float time = (item.time * 0.01f) * p.attackSpeedRate;
                Timing.RunCoroutine(YieldLaunchProjectile(p.skillType, p.target, item, projectileSetting, time, p.isItemSkill), GetInstanceID());
            }
        }

        IEnumerator<float> YieldChaseTarget(Parameter p)
        {
            p.skillArea = ShowSkillAreaCircle(p.skillInfo, p.target);

            float castingAnimTime;
            if (p.skillInfo.CastingTime > 0)
                castingAnimTime = p.skillInfo.CastingTime + p.skillSetting.hitTime * 0.01f * p.attackSpeedRate;
            else
                castingAnimTime = 0;

            p.skillArea.StartAnim(p.skillInfo.ChasingTime, castingAnimTime);

            float timer = p.skillInfo.ChasingTime;

            while (timer > 0)
            {
                p.skillArea.SetPos(Entity.LastPosition, p.target.Entity.LastPosition);
                if (!AI.IsLookFixed)
                    CachedTransform.LookAt(p.target.Entity.LastPosition); // 캐스팅 중에 타겟을 쳐다보도록 처리

                timer -= Timing.DeltaTime;

                if (p.isItemSkill)
                {
                    // Do Nothing (아이템 스킬의 경우)
                }
                else
                {
                    // 스킬 도중 상태이상
                    if (Entity.battleCrowdControlInfo.GetCannotUseSkill() && p.skillType == SkillType.Active)
                    {
                        p.skillArea.StartHide();
                        yield break;
                    }
                }

                yield return Timing.WaitForOneFrame;
            }

            CastSkill(p);
        }

        IEnumerator<float> YieldCastSkill(Parameter p)
        {
            if (p.skillArea == null)
            {
                p.skillArea = ShowSkillAreaCircle(p.skillInfo, p.target);
                p.skillArea.SetPos(Entity.LastPosition, p.target.Entity.LastPosition);
                p.skillArea.StartAnim(0, p.skillInfo.CastingTime + p.skillSetting.hitTime * 0.01f * p.attackSpeedRate);
            }

            float timer = p.skillInfo.CastingTime;

            while (timer > 0)
            {
                timer -= Timing.DeltaTime;

                if (p.isItemSkill)
                {
                    // Do Nothing (아이템 스킬의 경우)
                }
                else
                {
                    // 스킬 도중 상태이상
                    if (Entity.battleCrowdControlInfo.GetCannotUseSkill() && p.skillType == SkillType.Active)
                    {
                        p.skillArea.StartHide();
                        yield break;
                    }
                }

                yield return Timing.WaitForOneFrame;
            }

            DischargeSkill(p);
        }

        IEnumerator<float> YieldDetectCrowdControlState(SkillType skillType, float time)
        {
            RelativeRemainTime leftTime = time;
            while (leftTime.GetRemainTime() > 0f)
            {
                // 평타 불가 상태이상
                if (skillType == SkillType.BasicActiveSkill && Entity.battleCrowdControlInfo.GetCannotUseBasicAttack())
                {
                    useSkillAnimDuration = 0f; // 이동 불가 시간 초기화
                    yield break;
                }

                // 액티브스킬 불가 상태이상
                if (skillType == SkillType.Active && Entity.battleCrowdControlInfo.GetCannotUseSkill())
                {
                    useSkillAnimDuration = 0f; // 이동 불가 시간 초기화
                    yield break;
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        protected virtual ISkillArea ShowSkillAreaCircle(SkillInfo skillInfo, UnitActor target)
        {
            return new EmptySkillArea(skillInfo, Entity.LastPosition, target.Entity.LastPosition);
        }

        IEnumerator<float> YieldDamage(Parameter p)
        {
            float time = (p.skillSetting.hitTime * 0.01f) * p.attackSpeedRate;

            if (!p.skillInfo.IsBasicActiveSkill)
                EffectPlayer.ShowSkillBalloon(p.skillInfo.SkillName, p.skillInfo.IconName);

            if (Entity.State == UnitEntity.UnitState.GVGMultiPlayer || Entity.State == UnitEntity.UnitState.TempStagePlayerOther) // GVGMultiPlayer는 모션만 재생한다.
                yield break;

            if (time > 0)
                yield return Timing.WaitForSeconds(time);

            if (p.isItemSkill)
            {
                // Do Nothing (아이템 스킬의 경우)
            }
            else
            {
                // 스킬 도중 상태이상
                if (Entity.battleCrowdControlInfo.GetCannotUseBasicAttack() && p.skillType == SkillType.BasicActiveSkill)
                    yield break;

                // 스킬 도중 상태이상
                if (Entity.battleCrowdControlInfo.GetCannotUseSkill() && p.skillType == SkillType.Active)
                    yield break;
            }

            if (p.skillArea == null)
            {
                p.skillArea = ShowSkillAreaCircle(p.skillInfo, p.target);
                p.skillArea.SetPos(Entity.LastPosition, p.target.Entity.LastPosition);
                p.skillArea.StartAnim(0, 0);
            }

            UnitActor pointTarget = p.skillInfo.PointType == EffectPointType.Target ? p.target : this;
            // 해당 스킬의 영향을 받는 유닛 리스트
            UnitActor[] units = battleManager.unitList.GetSkillAffectableUnits(this, pointTarget, p.skillInfo.TargetType, p.skillArea);

            // 대미지 독립 처리 여부 체크
            if (Entity.State == UnitEntity.UnitState.GVG || Entity.State == UnitEntity.UnitState.TempStagePlayer)
            {
                var targetEntities = (from each in units select each.Entity).ToArray();
                Entity.InvokeApplySkill(targetEntities, p.skillInfo);
                yield break;
            }

            // 대미지 적용
            foreach (var item in units.OrEmptyIfNull())
            {
                // 대미지 타입의 스킬의 경우
                UnitEntity devotedUnit = p.skillInfo.ActiveOptions.HasDamageValue ? battleManager.unitList.FindMinDevotedUnit(item) : null;
                item.Entity.Apply(Entity, p.skillInfo, devotedUnit); // 스킬 효과 적용

                if (p.isItemSkill)
                {
                    // Do Nothing (아이템 스킬의 경우)
                }
                else
                {
                    if (p.skillInfo.IsRush && !BattleManager.isAntiChaseSkill)
                    {
                        Vector3 offset = pointTarget.Entity.LastPosition - item.Entity.LastPosition;
                        if (offset.Equals(Vector3.zero))
                            offset = item.Entity.LastPosition - Entity.LastPosition;

                        item.Movement.KnockBack(offset.normalized, Constants.Battle.RushKnockBackPower);
                    }
                }
            }
        }

        IEnumerator<float> YieldShowEffect(Parameter p, SkillSetting.Vfx vfx, float time)
        {
            if (time > 0)
                yield return Timing.WaitForSeconds(time);

            if (p.isItemSkill)
            {
                // Do Nothing (아이템 스킬의 경우)
            }
            else
            {
                // 스킬 도중 상태이상
                if (Entity.battleCrowdControlInfo.GetCannotUseBasicAttack() && p.skillType == SkillType.BasicActiveSkill)
                    yield break;

                // 스킬 도중 상태이상
                if (Entity.battleCrowdControlInfo.GetCannotUseSkill() && p.skillType == SkillType.Active)
                    yield break;
            }

            UnitActor unit = vfx.toTarget ? p.target : this; // ToTarget
            SkillEffect effect = null;

            // 죽었을 경우
            if (unit.Entity.IsDie)
            {
                effect = EffectPlayer.ShowSkillEffect(vfx.name, unit.Entity.LastPosition, vfx.duration);
            }
            else
            {
                effect = EffectPlayer.ShowSkillEffect(vfx.name, unit.CachedTransform, vfx.node, vfx.offset, vfx.rotate, vfx.isAttach, vfx.duration);
            }

            if (unit == this && !vfx.isAttach && effect != null)
            {
                Matrix4x4 casterLocalToWorld = CachedTransform.localToWorldMatrix;
                effect.CachedTransform.position = casterLocalToWorld.MultiplyPoint3x4(vfx.offset);
                effect.CachedTransform.rotation = casterLocalToWorld.rotation * Quaternion.Euler(vfx.rotate);
            }
        }

        IEnumerator<float> YieldPlaySound(SkillType skillType, SkillSetting.Sound sound, float time, bool isItemSkill)
        {
            if (time > 0)
                yield return Timing.WaitForSeconds(time);

            if (isItemSkill)
            {
                // Do Nothing (아이템 스킬의 경우)
            }
            else
            {
                // 스킬 도중 상태이상
                if (Entity.battleCrowdControlInfo.GetCannotUseBasicAttack() && skillType == SkillType.BasicActiveSkill)
                    yield break;

                // 스킬 도중 상태이상
                if (Entity.battleCrowdControlInfo.GetCannotUseSkill() && skillType == SkillType.Active)
                    yield break;
            }

            EffectPlayer.PlaySound(sound.name, sound.duration > 0 ? sound.duration * 0.01f : 0f);
        }

        IEnumerator<float> YieldLaunchProjectile(SkillType skillType, UnitActor target, SkillSetting.Projectile projectile, ProjectileSetting projectileSetting, float time, bool isItemSkill)
        {
            if (time > 0)
                yield return Timing.WaitForSeconds(time);

            if (isItemSkill)
            {
                // Do Nothing (아이템 스킬의 경우)
            }
            else
            {
                // 스킬 도중 상태이상
                if (Entity.battleCrowdControlInfo.GetCannotUseBasicAttack() && skillType == SkillType.BasicActiveSkill)
                    yield break;

                // 스킬 도중 상태이상
                if (Entity.battleCrowdControlInfo.GetCannotUseSkill() && skillType == SkillType.Active)
                    yield break;
            }

            if (projectileSetting.start != null)
            {
                UnitActor unit = target; // ToTarget

                // 죽었을 경우
                if (unit.Entity.IsDie)
                {
                    EffectPlayer.ShowSkillEffect(projectileSetting.start.name, unit.Entity.LastPosition, duration: 0);
                }
                else
                {
                    EffectPlayer.ShowSkillEffect(projectileSetting.start.name, unit.CachedTransform, projectile.node, projectile.offset, projectile.rotate, isAttach: false, duration: 0);
                }
            }

            if (projectileSetting.loop != null)
            {
                UnitActor unit = target; // ToTarget
                SkillEffect loop;

                Transform node = GetNode(CachedTransform, projectileSetting.loop.node);

                // 죽었을 경우
                if (unit.Entity.IsDie)
                {
                    loop = EffectPlayer.ShowSkillEffect(projectileSetting.loop.name, node == null ? CachedTransform.position : node.position, duration: 0);
                }
                else
                {
                    loop = EffectPlayer.ShowSkillEffect(projectileSetting.loop.name, node == null ? CachedTransform : node, projectile.node, projectile.offset, projectile.rotate, isAttach: false, duration: projectile.duration + projectileSetting.loop.delayDestory);

                    if (loop != null)
                    {
                        loop.SetDestination(unit.CachedTransform.position, projectileSetting.loop.heightCurve, projectileSetting.loop.moveCurve, projectileSetting.loop.sideDirCurve);
                    }
                }

                if (projectileSetting.end != null && !string.IsNullOrEmpty(projectileSetting.end.name))
                {
                    int delay = projectile.duration - projectileSetting.end.overlapTime;

                    if (delay > 0)
                        yield return Timing.WaitForSeconds(delay * 0.01f);

                    if (loop != null)
                    {
                        EffectPlayer.ShowSkillEffect(projectileSetting.end.name, loop.CachedTransform.position, duration: 0);
                    }
                }
            }
        }

        /// <summary>
        /// Find Recursive
        /// </summary>
        private Transform GetNode(Transform tf, string name)
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

        void OnMoveStop(UnitEntity unitEntity)
        {
            Animator.PlayIdle();
        }

        public void SetParent(Transform parent, bool worldPositionStays)
        {
            CachedTransform.SetParent(parent, worldPositionStays);
            Appearance.OnSetParent();
        }
    }
}