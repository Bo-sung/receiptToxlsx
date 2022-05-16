using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class BattleUnitList : BetterList<UnitEntity>
    {
        private struct TargetSkill
        {
            /// <summary>
            /// 선택된 스킬
            /// </summary>
            public readonly SkillInfo skillInfo;

            /// <summary>
            /// 스킬 타겟팅 가능한 유닛 목록
            /// </summary>
            public readonly UnitActor[] targetableUnits;

            public TargetSkill(SkillInfo skillInfo, UnitActor[] targetableUnits)
            {
                this.skillInfo = skillInfo;
                this.targetableUnits = targetableUnits;
            }

            public TargetUnit SelectRandomTargetUnit()
            {
                if (targetableUnits == null || targetableUnits.Length == 0)
                    return default;

                int randNum = Random.Range(0, targetableUnits.Length); // 타겟 랜덤 인덱스
                return new TargetUnit(targetableUnits[randNum], skillInfo);
            }
        }

        private readonly Buffer<TargetSkill> targetSkillBuffer;
        private readonly Buffer<UnitActor> targetableUnitBuffer;
        private readonly Buffer<SkillInfo> skillBuffer;
        private readonly Buffer<UnitActor> unitBuffer;

        public BattleUnitList()
        {
            targetSkillBuffer = new Buffer<TargetSkill>();
            targetableUnitBuffer = new Buffer<UnitActor>();
            skillBuffer = new Buffer<SkillInfo>();
            unitBuffer = new Buffer<UnitActor>();
        }

        /// <summary>
        /// 아군 등록
        /// </summary>
        public void AddAllies(IEnumerable<UnitEntity> allies)
        {
            foreach (var item in allies)
            {
                if (item == null)
                    continue;

                item.SetEnemy(isEnemy: false);
                Add(item);
            }
        }

        /// <summary>
        /// 적군 등록
        /// </summary>
        public void AddEnemies(IEnumerable<UnitEntity> enemies)
        {
            foreach (var item in enemies)
            {
                if (item == null)
                    continue;

                item.SetEnemy(isEnemy: true);
                Add(item);
            }
        }

        /// <summary>
        /// 가장 가까이에 있는 타겟 반환
        /// </summary>
        public UnitActor FindMinTarget(UnitActor actor, TargetType targetType)
        {
            Vector3 pos = actor.Entity.LastPosition;
            float cognizanceDistance = BattleManager.isAntiChaseSkill ? -1f : actor.Entity.battleUnitInfo.CognizanceDistance;
            float sqrDistance = cognizanceDistance == -1f ? -1f : (cognizanceDistance * cognizanceDistance); // 거리 제곱

            float minDist = float.MaxValue;
            UnitActor minTarget = null;

            for (int i = 0; i < size; i++)
            {
                // 죽어있음
                if (base[i].IsDie)
                    continue;

                if (base[i].GetActor() == null)
                    continue;

                // 타겟 타입에 해당되지 않음
                if (!actor.IsLookTarget(base[i].GetActor(), targetType))
                    continue;

                float dist = (pos - base[i].LastPosition).sqrMagnitude;

                // 찾고자 하는 거리보다 멀다
                if ((sqrDistance != -1) && (dist > sqrDistance))
                    continue;

                // 가장 가까운 것은 아니다
                if (dist > minDist)
                    continue;

                minDist = dist;
                minTarget = base[i].GetActor();
            }

            return minTarget;
        }

        /// <summary>
        /// 가장 가까이에 있는 타겟 반환
        /// </summary>
        public UnitActor FindMinTarget(UnitActor actor, TargetType targetType, UnitEntityType type)
        {
            Vector3 pos = actor.Entity.LastPosition;
            float cognizanceDistance = actor.Entity.battleUnitInfo.CognizanceDistance;
            float sqrDistance = cognizanceDistance == -1f ? -1f : (cognizanceDistance * cognizanceDistance); // 거리 제곱            

            float minDist = float.MaxValue;
            UnitActor minTarget = null;

            for (int i = 0; i < size; i++)
            {
                // 죽어있음
                if (base[i].IsDie)
                    continue;

                if (base[i].GetActor() == null)
                    continue;

                if (base[i].type != type)
                    continue;

                // 타겟 타입에 해당되지 않음
                if (!actor.IsLookTarget(base[i].GetActor(), targetType))
                    continue;

                float dist = (pos - base[i].LastPosition).sqrMagnitude;

                // 찾고자 하는 거리보다 멀다
                if ((sqrDistance != -1) && (dist > sqrDistance))
                    continue;

                // 가장 가까운 것은 아니다
                if (dist > minDist)
                    continue;

                minDist = dist;
                minTarget = base[i].GetActor();
            }

            return minTarget;
        }

        /// <summary>
        /// 가장 가까이에 있는 타겟 반환
        /// </summary>
        public UnitActor FindMinTarget(UnitActor actor, TargetType targetType, float cognizanceDistance)
        {
            Vector3 pos = actor.Entity.LastPosition;
            float sqrDistance = cognizanceDistance == -1f ? -1f : (cognizanceDistance * cognizanceDistance); // 거리 제곱            

            float minDist = float.MaxValue;
            UnitActor minTarget = null;

            for (int i = 0; i < size; i++)
            {
                // 죽어있음
                if (base[i].IsDie)
                    continue;

                if (base[i].GetActor() == null)
                    continue;

                // 타겟 타입에 해당되지 않음
                if (!actor.IsLookTarget(base[i].GetActor(), targetType))
                    continue;

                float dist = (pos - base[i].LastPosition).sqrMagnitude;

                // 찾고자 하는 거리보다 멀다
                if ((sqrDistance != -1) && (dist > sqrDistance))
                    continue;

                // 가장 가까운 것은 아니다
                if (dist > minDist)
                    continue;

                minDist = dist;
                minTarget = base[i].GetActor();
            }

            return minTarget;
        }

        /// <summary>
        /// 가장 가까이에 있는 타겟 반환
        /// </summary>
        public UnitActor FindMinTarget(UnitActor actor, TargetType targetType, UnitEntityType type, float cognizanceDistance)
        {
            Vector3 pos = actor.Entity.LastPosition;
            float sqrDistance = cognizanceDistance == -1f ? -1f : (cognizanceDistance * cognizanceDistance); // 거리 제곱            

            float minDist = float.MaxValue;
            UnitActor minTarget = null;

            for (int i = 0; i < size; i++)
            {
                // 죽어있음
                if (base[i].IsDie)
                    continue;

                if (base[i].GetActor() == null)
                    continue;

                if (base[i].type != type)
                    continue;

                // 타겟 타입에 해당되지 않음
                if (!actor.IsLookTarget(base[i].GetActor(), targetType))
                    continue;

                float dist = (pos - base[i].LastPosition).sqrMagnitude;

                // 찾고자 하는 거리보다 멀다
                if ((sqrDistance != -1) && (dist > sqrDistance))
                    continue;

                // 가장 가까운 것은 아니다
                if (dist > minDist)
                    continue;

                minDist = dist;
                minTarget = base[i].GetActor();
            }

            return minTarget;
        }

        /// <summary>
        /// 가장 가까이에 있는 헌신 유닛
        /// </summary>
        public UnitEntity FindMinDevotedUnit(UnitActor actor)
        {
            Vector3 pos = actor.Entity.LastPosition;
            float cognizanceDistance = actor.Entity.battleUnitInfo.CognizanceDistance;
            float sqrDistance = cognizanceDistance == -1f ? -1f : (cognizanceDistance * cognizanceDistance); // 거리 제곱            

            float minDist = float.MaxValue;
            UnitEntity minTarget = null;

            for (int i = 0; i < size; i++)
            {
                // 죽어있음
                if (base[i].IsDie)
                    continue;

                // 자기 자신은 헌신 대상이 아님
                if (base[i].Equals(actor.Entity))
                    continue;

                if (base[i].GetActor() == null)
                    continue;

                // 헌신 옵션이 음슴
                if (base[i].battleStatusInfo.DevoteRate == 0)
                    continue;

                // 아군이 아님
                if (!actor.IsLookTarget(base[i].GetActor(), TargetType.Allies))
                    continue;

                float dist = (pos - base[i].LastPosition).sqrMagnitude;

                // 찾고자 하는 거리보다 멀다
                if ((sqrDistance != -1) && (dist > sqrDistance))
                    continue;

                // 가장 가까운 것은 아니다
                if (dist > minDist)
                    continue;

                minDist = dist;
                minTarget = base[i];
            }

            return minTarget;
        }

        public UnitActor FindMaxTarget(UnitActor actor, TargetType targetType)
        {
            Vector3 pos = actor.Entity.LastPosition;
            float cognizanceDistance = BattleManager.isAntiChaseSkill ? -1f : actor.Entity.battleUnitInfo.CognizanceDistance;
            float sqrDistance = cognizanceDistance == -1f ? -1f : (cognizanceDistance * cognizanceDistance); // 거리 제곱            

            float maxDist = float.MinValue;
            UnitActor maxTarget = null;

            for (int i = 0; i < size; i++)
            {
                // 죽어있음
                if (base[i].IsDie)
                    continue;

                if (base[i].GetActor() == null)
                    continue;

                // 타겟 타입에 해당되지 않음
                if (!actor.IsLookTarget(base[i].GetActor(), targetType))
                    continue;

                float dist = (pos - base[i].LastPosition).sqrMagnitude;

                // 찾고자 하는 거리보다 멀다
                if ((sqrDistance != -1) && (dist > sqrDistance))
                    continue;

                // 가장 가까운 것은 아니다
                if (dist < maxDist)
                    continue;

                maxDist = dist;
                maxTarget = base[i].GetActor();
            }

            return maxTarget;
        }

        public UnitActor FindRandomUnitInRangeGroup(UnitActor actor, TargetType targetType, AttackType attackType)
        {
            for (int i = 0; i < size; i++)
            {
                // 죽어있음
                if (base[i].IsDie)
                    continue;

                if (base[i].GetActor() == null)
                    continue;

                // 타겟 타입에 해당되지 않음
                if (!actor.IsLookTarget(base[i].GetActor(), targetType))
                    continue;

                if (base[i].battleSkillInfo.basicActiveSkill.AttackType == attackType)
                    unitBuffer.Add(base[i].GetActor());
            }

            if (unitBuffer.size == 0)
                return null;

            int randIndex = Random.Range(0, unitBuffer.size);
            UnitActor ret = unitBuffer[randIndex];
            unitBuffer.Release();

            return ret;
        }

        public UnitActor FindWeakestUnit(UnitActor actor, TargetType targetType)
        {
            float minHPRate = float.MaxValue;
            UnitActor minHPTarget = null;

            for (int i = 0; i < size; i++)
            {
                // 죽어있음
                if (base[i].IsDie)
                    continue;

                if (base[i].GetActor() == null)
                    continue;

                // 타겟 타입에 해당되지 않음
                if (!actor.IsLookTarget(base[i].GetActor(), targetType))
                    continue;

                float hpRate = base[i].CurHP / (float)base[i].MaxHP;
                if (hpRate >= minHPRate)
                    continue;

                minHPRate = hpRate;
                minHPTarget = base[i].GetActor();
            }

            return minHPTarget;
        }

        /// <summary>
        /// 가장 가까이에 있는 NPC
        /// </summary>
        public UnitActor FindMinNPC(UnitActor actor)
        {
            Vector3 pos = actor.Entity.LastPosition;

            float minDist = float.MaxValue;
            UnitActor minTarget = null;

            for (int i = 0; i < size; i++)
            {
                if (base[i].type != UnitEntityType.NPC)
                    continue;

                UnitActor targetActor = base[i].GetActor();
                if (targetActor == null)
                    continue;

                float cognizanceDistance = targetActor.AI.GetFindDistance();
                float sqrDistance = cognizanceDistance == -1f ? -1f : (cognizanceDistance * cognizanceDistance); // 거리 제곱      

                float dist = (pos - base[i].LastPosition).sqrMagnitude;

                // 찾고자 하는 거리보다 멀다
                if ((sqrDistance != -1) && (dist > sqrDistance))
                    continue;

                // 가장 가까운 것은 아니다
                if (dist > minDist)
                    continue;

                minDist = dist;
                minTarget = targetActor;
            }

            return minTarget;
        }

        /// <summary>
        /// 조건에 맞는 타겟 반환
        /// </summary>
        public TargetUnit FindSkillTarget(UnitActor actor, UnitActor target, SkillInfo skillInfo, bool isInputSkill)
        {
            if (skillInfo == null || skillInfo.IsInvalidData)
                return default;

            return FindSkillTarget(actor, target, skillInfo.ActiveSkillType, skillInfo, skills: null, isInputSkill);
        }

        /// <summary>
        /// 조건에 맞는 타겟 반환
        /// </summary>
        public TargetUnit FindSkillTarget(UnitActor actor, UnitActor target, ActiveSkill.Type condition, IEnumerable<SkillInfo> skills)
        {
            if (skills == null)
                return default;

            return FindSkillTarget(actor, target, condition, singleSkill: null, skills, isInputSkill: false);
        }

        /// <summary>
        /// 스킬 적용이 되는 타겟 리스트 반환
        /// </summary>
        public UnitActor[] GetSkillAffectableUnits(UnitActor caster, UnitActor target, TargetType targetType, ISkillArea skillArea)
        {
            if (skillArea.SkillArea == 0f)
            {
                targetableUnitBuffer.Add(target); // 단일 타겟
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    // 죽어있음
                    if (base[i].IsDie)
                        continue;

                    if (base[i].GetActor() == null)
                        continue;

                    // 타겟 타입에 해당되지 않음
                    if (!caster.IsSkillTarget(base[i].GetActor(), targetType))
                        continue;

                    // 타겟 거리 체크 (distValue 가 -1일 경우에는 거리 체크 없이 무조건 통과)
                    if (!skillArea.CheckCollision(base[i].LastPosition, base[i].GetActor().Appearance.GetRadius()))
                        continue;

                    targetableUnitBuffer.Add(base[i].GetActor());
                }
            }

            return targetableUnitBuffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 공격형 스킬 사용 가능한 랜덤한 대상 반환
        /// </summary>
        private TargetUnit FindSkillTarget(UnitActor actor, UnitActor target, ActiveSkill.Type condition, SkillInfo singleSkill, IEnumerable<SkillInfo> skills, bool isInputSkill)
        {
            if (target)
            {
                unitBuffer.Add(target);
            }
            else
            {
                foreach (var item in this)
                {
                    if (item.GetActor() == null)
                        continue;

                    unitBuffer.Add(item.GetActor());
                }
            }

            if (singleSkill)
            {
                skillBuffer.Add(singleSkill); // null 이면서 유효하지 않은 스킬은 절대 오지 않음
            }
            else
            {
                if (skills != null)
                {
                    foreach (var item in skills)
                    {
                        if (item == null || item.IsInvalidData)
                            continue;

                        skillBuffer.Add(item);
                    }
                }
            }

            EquipmentClassType weaponType = actor.Entity.battleItemInfo.WeaponType;

            foreach (var skill in skillBuffer)
            {
                // 스킬 조건에 맞지 않는다
                if (condition != skill.ActiveSkillType)
                    continue;

                // 스킬 쿨타임 남아있음
                if (skill.HasRemainCooldownTime())
                    continue;

                // 외부 입력 스킬이 아니면서 스킬 발동 확률이 성립하지 않았을 때
                if (!isInputSkill && !skill.IsCheckSkillRate())
                    continue;

                // 유효한 스킬 여부
                if (!skill.IsValid(weaponType))
                    continue;

                foreach (var unit in unitBuffer)
                {
                    // (타겟이 존재하지 않음) or (타겟이 죽어있음)
                    if (unit == null || unit.Entity.IsDie)
                        continue;

                    // 타겟 타입에 해당되지 않음
                    if (!actor.IsSkillTarget(unit, skill.TargetType))
                        continue;

                    // 타겟 거리 체크 (distValue 가 -1일 경우에는 거리 체크 없이 무조건 통과)
                    bool isIgnoreSkillRange = BattleManager.isAntiChaseSkill && skill.ActiveSkillType == ActiveSkill.Type.Attack; // 추적금지 상태이면서 공격형 스킬
                    if (!unit.IsCheckDistance(actor, isIgnoreSkillRange ? -1f : skill.GetSkillRange(actor.Entity.AtkRangeRate)))
                        continue;

                    // 회복형 스킬의 경우 만피의 경우 해당되지 않음
                    if (skill.ActiveSkillType == ActiveSkill.Type.RecoveryHp && unit.Entity.IsMaxHp)
                        continue;

                    targetableUnitBuffer.Add(unit); // 타겟팅 가능한 유닛에 등록
                }

                // 스킬 타겟팅 가능한 유닛 목록
                UnitActor[] targetableUnits = targetableUnitBuffer.GetBuffer(isAutoRelease: true);
                if (targetableUnits == null)
                    continue;

                targetSkillBuffer.Add(new TargetSkill(skill, targetableUnits)); // 선택 가능한 스킬에 등록
            }

            unitBuffer.Release();
            skillBuffer.Release();

            TargetSkill targetSkill = targetSkillBuffer.Dequeue(isAutoRelease: true); // 스킬 사용할 타겟
            return targetSkill.SelectRandomTargetUnit(); // 랜덤한 타겟 선택
        }

        /// <summary>
        /// 기본스킬이 없는 경우 (포탑)
        /// </summary>
        public UnitActor[] GetValidTargets(UnitActor actor, IEnumerable<SkillInfo> skills)
        {
            skillBuffer.AddRange(skills);

            foreach (var item in this)
            {
                if (item.GetActor() == null)
                    continue;

                unitBuffer.Add(item.GetActor());
            }

            EquipmentClassType weaponType = actor.Entity.battleItemInfo.WeaponType;

            foreach (var skill in skillBuffer)
            {
                // 스킬 조건에 맞지 않는다
                if (ActiveSkill.Type.Attack != skill.ActiveSkillType)
                    continue;

                // 유효한 스킬 여부
                if (!skill.IsValid(weaponType))
                    continue;

                foreach (var unit in unitBuffer)
                {
                    // (타겟이 존재하지 않음) or (타겟이 죽어있음)
                    if (unit == null || unit.Entity.IsDie)
                        continue;

                    // 타겟 타입에 해당되지 않음
                    if (!actor.IsSkillTarget(unit, skill.TargetType))
                        continue;

                    // 타겟 거리 체크 (distValue 가 -1일 경우에는 거리 체크 없이 무조건 통과)
                    bool isIgnoreSkillRange = BattleManager.isAntiChaseSkill && skill.ActiveSkillType == ActiveSkill.Type.Attack; // 추적금지 상태이면서 공격형 스킬
                    if (!unit.IsCheckDistance(actor, isIgnoreSkillRange ? -1f : skill.GetSkillRange(actor.Entity.AtkRangeRate)))
                        continue;

                    //// 회복형 스킬의 경우 만피의 경우 해당되지 않음
                    //if (skill.ActiveSkillType == ActiveSkill.Type.RecoveryHp && unit.Entity.IsMaxHp)
                    //    continue;

                    targetableUnitBuffer.Add(unit); // 타겟팅 가능한 유닛에 등록
                }

                // 스킬 타겟팅 가능한 유닛 목록
                UnitActor[] targetableUnits = targetableUnitBuffer.GetBuffer(isAutoRelease: true);
                if (targetableUnits == null)
                    continue;

                targetSkillBuffer.Add(new TargetSkill(skill, targetableUnits)); // 선택 가능한 스킬에 등록
            }

            // 타겟이 될 수 있는 유닛목록 뽑기
            List<UnitActor> targetActorList = new List<UnitActor>();
            foreach (var skill in targetSkillBuffer.GetBuffer(isAutoRelease: true))
            {
                targetActorList.AddRange(skill.targetableUnits);
            }

            unitBuffer.Release();
            skillBuffer.Release();

            UnitActor[] targetActors = targetActorList.Distinct().ToArray();
            return targetActors;
        }

        public UnitActor[] GetValidTargets(UnitActor actor, SkillInfo basicAttackSkill, IEnumerable<SkillInfo> skills)
        {
            skillBuffer.Add(basicAttackSkill);
            skillBuffer.AddRange(skills);

            foreach (var item in this)
            {
                if (item.GetActor() == null)
                    continue;

                unitBuffer.Add(item.GetActor());
            }

            EquipmentClassType weaponType = actor.Entity.battleItemInfo.WeaponType;

            foreach (var skill in skillBuffer)
            {
                // 스킬 조건에 맞지 않는다
                if (ActiveSkill.Type.Attack != skill.ActiveSkillType)
                    continue;

                // 유효한 스킬 여부
                if (!skill.IsValid(weaponType))
                    continue;

                foreach (var unit in unitBuffer)
                {
                    // (타겟이 존재하지 않음) or (타겟이 죽어있음)
                    if (unit == null || unit.Entity.IsDie)
                        continue;

                    // 타겟 타입에 해당되지 않음
                    if (!actor.IsSkillTarget(unit, skill.TargetType))
                        continue;

                    // 타겟 거리 체크 (distValue 가 -1일 경우에는 거리 체크 없이 무조건 통과)
                    bool isIgnoreSkillRange = BattleManager.isAntiChaseSkill && skill.ActiveSkillType == ActiveSkill.Type.Attack; // 추적금지 상태이면서 공격형 스킬
                    if (!unit.IsCheckDistance(actor, isIgnoreSkillRange ? -1f : skill.GetSkillRange(actor.Entity.AtkRangeRate)))
                        continue;

                    //// 회복형 스킬의 경우 만피의 경우 해당되지 않음
                    //if (skill.ActiveSkillType == ActiveSkill.Type.RecoveryHp && unit.Entity.IsMaxHp)
                    //    continue;

                    targetableUnitBuffer.Add(unit); // 타겟팅 가능한 유닛에 등록
                }

                // 스킬 타겟팅 가능한 유닛 목록
                UnitActor[] targetableUnits = targetableUnitBuffer.GetBuffer(isAutoRelease: true);
                if (targetableUnits == null)
                    continue;

                targetSkillBuffer.Add(new TargetSkill(skill, targetableUnits)); // 선택 가능한 스킬에 등록
            }

            // 타겟이 될 수 있는 유닛목록 뽑기
            List<UnitActor> targetActorList = new List<UnitActor>();
            foreach (var skill in targetSkillBuffer.GetBuffer(isAutoRelease: true))
            {
                targetActorList.AddRange(skill.targetableUnits);
            }

            unitBuffer.Release();
            skillBuffer.Release();

            UnitActor[] targetActors = targetActorList.Distinct().ToArray();
            return targetActors;
        }

        public bool IsValidTarget(UnitActor actor, UnitActor target, SkillInfo basicAttackSkill, IEnumerable<SkillInfo> skills)
        {
            skillBuffer.Add(basicAttackSkill);
            skillBuffer.AddRange(skills);

            EquipmentClassType weaponType = actor.Entity.battleItemInfo.WeaponType;

            foreach (var skill in skillBuffer)
            {
                // 스킬 조건에 맞지 않는다
                if (ActiveSkill.Type.Attack != skill.ActiveSkillType)
                    continue;

                // 유효한 스킬 여부
                if (!skill.IsValid(weaponType))
                    continue;

                // (타겟이 존재하지 않음) or (타겟이 죽어있음)
                if (target == null || target.Entity.IsDie)
                    continue;

                // 타겟 타입에 해당되지 않음
                if (!actor.IsSkillTarget(target, skill.TargetType))
                    continue;

                // 타겟 거리 체크 (distValue 가 -1일 경우에는 거리 체크 없이 무조건 통과)
                bool isIgnoreSkillRange = BattleManager.isAntiChaseSkill && skill.ActiveSkillType == ActiveSkill.Type.Attack; // 추적금지 상태이면서 공격형 스킬
                if (!target.IsCheckDistance(actor, isIgnoreSkillRange ? -1f : skill.GetSkillRange(actor.Entity.AtkRangeRate)))
                    continue;

                //// 회복형 스킬의 경우 만피의 경우 해당되지 않음
                //if (skill.ActiveSkillType == ActiveSkill.Type.RecoveryHp && unit.Entity.IsMaxHp)
                //    continue;

                skillBuffer.Release();
                return true;
            }

            skillBuffer.Release();
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 전투중인 모든 유닛 목록(디버그 용)
        /// </summary>
        public UnitEntity[] GetUnits()
        {
            return ToArray();
        }
#endif
    }
}