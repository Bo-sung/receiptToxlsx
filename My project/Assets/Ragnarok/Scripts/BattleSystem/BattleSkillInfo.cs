using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class BattleSkillInfo : List<BattleOption>
    {
        public struct Settings
        {
            public int basicActiveSkillId;
            public int basicPassiveSkillId;
            public SkillInfo[] skills;
            public bool isAntiSkillAuto;
        }

        private readonly SkillDataManager skillDataRepo;

        /// <summary>
        /// 적용 가능한 액티브 스킬 리스트
        /// </summary>
        private readonly List<SkillInfo> activeSkillList;

        /// <summary>
        /// 평타 스킬
        /// </summary>
        public readonly SkillInfo basicActiveSkill;

        /// <summary>
        /// 특정 몬스터 동행 확률 세팅
        /// key: monsterId, value: 소환 확률
        /// </summary>
        private readonly Dictionary<ObscuredInt, ObscuredInt> colleagueRateDic;

        /// <summary>
        /// 화속성 공격 시 발동되는 스킬 리스트
        /// </summary>
        private readonly List<SkillInfo> fireActiveSkillList;

        /// <summary>
        /// 평타 시 발동되는 스킬 리스트
        /// </summary>
        private readonly List<SkillInfo> extraBasicActiveSkillList;

        /// <summary>
        /// 공격 시 발동되는 스킬 리스트
        /// </summary>
        private readonly List<SkillInfo> extraActiveSkillList;

        /// <summary>
        /// 스킬 초월
        /// key: 기존SkillId, value: 변경SkillId
        /// </summary>
        private readonly Dictionary<ObscuredInt, ObscuredInt> skillOverrideDic;

        /// <summary>
        /// 스킬 연계
        /// key: SkillId, value: 스킬리스트
        /// </summary>
        private readonly Dictionary<ObscuredInt, List<SkillInfo>> skillChainDic;

        /// <summary>
        /// 참조 스킬 남은시간 관리
        /// </summary>
        private readonly Dictionary<int, RemainTime> refSkillRemainTimeDic;

        /// <summary>
        /// 참조 스킬 쿨타임 대기 상태 관리
        /// </summary>
        private readonly HashSet<int> refSkillResponseCooldownCheckStateHashSet;

        /// <summary>
        /// 스킬 초월 패시브 스킬 리스트
        /// </summary>
        private readonly List<SkillInfo> skillOverridePassiveSkillList;

        public BattleSkillInfo()
        {
            skillDataRepo = SkillDataManager.Instance;

            activeSkillList = new List<SkillInfo>();
            basicActiveSkill = new ActiveSkill();

            colleagueRateDic = new Dictionary<ObscuredInt, ObscuredInt>(ObscuredIntEqualityComparer.Default);

            fireActiveSkillList = new List<SkillInfo>();
            extraBasicActiveSkillList = new List<SkillInfo>();
            extraActiveSkillList = new List<SkillInfo>();
            skillOverrideDic = new Dictionary<ObscuredInt, ObscuredInt>(ObscuredIntEqualityComparer.Default);
            skillChainDic = new Dictionary<ObscuredInt, List<SkillInfo>>(ObscuredIntEqualityComparer.Default);

            refSkillRemainTimeDic = new Dictionary<int, RemainTime>(IntEqualityComparer.Default);
            refSkillResponseCooldownCheckStateHashSet = new HashSet<int>(IntEqualityComparer.Default);

            skillOverridePassiveSkillList = new List<SkillInfo>();
        }

        /// <summary>
        /// 쿨타임 정보 없애기
        /// </summary>
        public void ClearCooltime()
        {
            refSkillRemainTimeDic.Clear();
            refSkillResponseCooldownCheckStateHashSet.Clear();
        }

        /// <summary>
        /// 쿨타임 초기화
        /// </summary>
        public void ResetCooldown()
        {
            ClearCooltime();

            // 평타 시 발동
            foreach (var item in extraBasicActiveSkillList)
            {
                item.StartCooldown(0L); // 쿨타임 초기화
            }

            // 공격 시 발동
            foreach (var item in extraActiveSkillList)
            {
                item.StartCooldown(0L); // 쿨타임 초기화
            }

            // 화속성 공격 시 발동
            foreach (var item in fireActiveSkillList)
            {
                item.StartCooldown(0L); // 쿨타임 초기화
            }

            // 스킬 연계
            foreach (var skillList in skillChainDic.Values)
            {
                foreach (var item in skillList)
                {
                    item.StartCooldown(0L); // 쿨타임 초기화
                }
            }
        }

        /// <summary>
        /// 정보 리셋
        /// </summary>
        public new void Clear()
        {
            base.Clear();

            activeSkillList.Clear();
            basicActiveSkill.ResetData();

            colleagueRateDic.Clear();

            fireActiveSkillList.Clear();
            extraBasicActiveSkillList.Clear();
            extraActiveSkillList.Clear();
            skillOverrideDic.Clear();
            skillChainDic.Clear();

            skillOverridePassiveSkillList.Clear();
            // 쿨타임 초기화는 하지 않는다
        }

        public void Initialize(Settings settings, Dictionary<int, int> overrideDic)
        {
            Clear(); // 리셋

            if (overrideDic != null)
            {
                foreach (var item in overrideDic)
                {
                    skillOverrideDic.Add(item.Key, item.Value);
                }
            }

            SetBasicActiveSkill(settings.basicActiveSkillId); // 평타 스킬 세팅
            SetBasicPassiveSkill(settings.basicPassiveSkillId, 1); // 직업 패시브스킬 세팅 (직업 패시브 스킬은 무조건 레벨이 1이다)

            bool isAntiSkillAuto = settings.isAntiSkillAuto; // 스킬 수동 여부
            if (settings.skills != null)
            {
                for (int i = 0; i < settings.skills.Length; i++)
                {
                    if (settings.skills[i].IsInvalidData)
                        continue;

                    int skillId = settings.skills[i].SkillId;
                    switch (settings.skills[i].SkillType)
                    {
                        case SkillType.Plagiarism:
                        case SkillType.Reproduce:
                        case SkillType.SummonBall:
                        case SkillType.Passive:
                        case SkillType.RuneMastery:
                            if (skillOverrideDic.ContainsKey(skillId))
                            {
                                skillId = overrideDic[skillId];
                                SetBasicPassiveSkill(skillId, settings.skills[i].SkillLevel); // 패시브스킬 세팅

                                SkillData data = skillDataRepo.Get(skillId, settings.skills[i].SkillLevel);
                                if (data == null)
                                {
                                    Debug.LogError($"스킬 세팅 실패: {nameof(skillId)} = {skillId}");
                                    continue;
                                }

                                if (data.skill_type.ToEnum<SkillType>() != SkillType.Passive)
                                {
                                    Debug.LogError($"패시브 스킬이 아님 skill = {skillId}");
                                    continue;
                                }

                                SkillInfo info = new PassiveSkill();
                                info.SetRefBattleOption(settings.skills[i].SlotNo, BattleOptionType.SkillOverride); // 스킬초월
                                info.SetData(data);
                                skillOverridePassiveSkillList.Add(info);
                            }
                            else
                            {
                                AddRange(settings.skills[i]); // 전투옵션 세팅
                            }

                            break;

                        case SkillType.Active:
                        case SkillType.BasicActiveSkill:
                            if (skillOverrideDic.ContainsKey(skillId))
                            {
                                skillId = overrideDic[skillId];
                                SkillData data = skillDataRepo.Get(skillId, settings.skills[i].SkillLevel);
                                if (data == null)
                                {
                                    Debug.LogError($"스킬 세팅 실패: {nameof(skillId)} = {skillId}");
                                    continue;
                                }

                                if (data.skill_type.ToEnum<SkillType>() != SkillType.Active && data.skill_type.ToEnum<SkillType>() != SkillType.BasicActiveSkill)
                                {
                                    Debug.LogError($"[화속성 공격 시 발동되는 스킬 세팅 실패] 공격 타입이 아님: skill = {skillId}");
                                    continue;
                                }

                                SkillInfo info = new ActiveSkill();
                                info.SetSkillRate(isAntiSkillAuto ? 0 : 10000); // 스킬 발동확률: 100%
                                info.SetChainableSkill(true); // 연계로 인해 발동되는 스킬 여부: true
                                info.SetRefBattleOption(settings.skills[i].SlotNo, BattleOptionType.SkillOverride); // 스킬초월
                                info.SetData(data);

                                // 쿨타임 대기 상태로 변경
                                if (refSkillResponseCooldownCheckStateHashSet.Contains(skillId))
                                {
                                    info.SetResponseCooldownCheckState();
                                }

                                // 참조 쿨타임 적용
                                if (refSkillRemainTimeDic.ContainsKey(skillId))
                                {
                                    info.StartCooldown((long)refSkillRemainTimeDic[skillId]);
                                }

                                activeSkillList.Add(info);
                            }
                            else
                            {
                                activeSkillList.Add(settings.skills[i]);
                            }
                            break;

                        default:
                            Debug.LogError($"[올바르지 않은 SkillInfo] = {settings.skills[i].SkillType}");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 특정 몬스터 동행 확률 세팅
        /// </summary>
        public void SetColleagueRate(Dictionary<int, int> dic)
        {
            colleagueRateDic.Clear();

            if (dic == null)
                return;

            foreach (var item in dic)
            {
                colleagueRateDic.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 화속성 공격 시 발동되는 스킬 세팅
        /// </summary>
        public void SetFireActiveSkillRate(Dictionary<int, int> dic)
        {
            fireActiveSkillList.Clear();

            if (dic == null)
                return;

            foreach (var item in dic)
            {
                int skillId = item.Key;
                if (skillId == 0)
                    continue;

                SkillData data = skillDataRepo.Get(skillId, level: 1); // 참조하는 스킬 레벨은 무조건 1
                if (data == null)
                {
                    Debug.LogError($"스킬 세팅 실패: {nameof(skillId)} = {skillId}");
                    continue;
                }

                if (data.skill_type.ToEnum<SkillType>() != SkillType.Active && data.skill_type.ToEnum<SkillType>() != SkillType.BasicActiveSkill)
                {
                    Debug.LogError($"[화속성 공격 시 발동되는 스킬 세팅 실패] 공격 타입이 아님: skill = {skillId}");
                    continue;
                }

                SkillInfo info = new ActiveSkill();
                info.SetSkillRate(item.Value); // 스킬 발동확률: value
                info.SetChainableSkill(true); // 연계로 인해 발동되는 스킬 여부: true
                info.SetRefBattleOption(0L, BattleOptionType.FirePillar); // 화속성 공격 시 발동되는 스킬 세팅
                info.SetData(data);

                // 쿨타임 대기 상태로 변경
                if (refSkillResponseCooldownCheckStateHashSet.Contains(skillId))
                {
                    info.SetResponseCooldownCheckState();
                }

                // 참조 쿨타임 적용
                if (refSkillRemainTimeDic.ContainsKey(skillId))
                {
                    info.StartCooldown((long)refSkillRemainTimeDic[skillId]);
                }

                fireActiveSkillList.Add(info);
            }
        }

        /// <summary>
        /// 평타 시 발동되는 스킬 세팅
        /// </summary>
        public void SetBasicActiveSkillRate(Dictionary<int, int> dic)
        {
            extraBasicActiveSkillList.Clear();

            if (dic == null)
                return;

            foreach (var item in dic)
            {
                int skillId = item.Key;
                SkillData data = skillDataRepo.Get(skillId, level: 1); // 참조하는 스킬 레벨은 무조건 1
                if (data == null)
                {
                    Debug.LogError($"스킬 세팅 실패: {nameof(skillId)} = {skillId}");
                    continue;
                }

                if (data.skill_type.ToEnum<SkillType>() != SkillType.Active && data.skill_type.ToEnum<SkillType>() != SkillType.BasicActiveSkill)
                {
                    Debug.LogError($"[평타 시 발동되는 스킬 세팅 실패] 공격 타입이 아님: skill = {skillId}");
                    continue;
                }

                SkillInfo info = new ActiveSkill();
                info.SetSkillRate(item.Value); // 스킬 발동확률: value
                info.SetRefBattleOption(0L, BattleOptionType.BasicActiveSkillRate); // 평타 시 발동되는 스킬 세팅
                info.SetData(data);

                // 쿨타임 대기 상태로 변경
                if (refSkillResponseCooldownCheckStateHashSet.Contains(skillId))
                {
                    info.SetResponseCooldownCheckState();
                }

                // 참조 쿨타임 적용
                if (refSkillRemainTimeDic.ContainsKey(skillId))
                {
                    info.StartCooldown((long)refSkillRemainTimeDic[skillId]);
                }

                extraBasicActiveSkillList.Add(info);
            }
        }

        /// <summary>
        /// 공격 시 발동되는 스킬 세팅 (추가 세팅)
        /// </summary>
        public void SetExtraActiveSkillRate(Dictionary<int, int> dic)
        {
            extraActiveSkillList.Clear();

            if (dic == null)
                return;

            foreach (var item in dic)
            {
                int skillId = item.Key;
                SkillData data = skillDataRepo.Get(skillId, level: 1); // 참조하는 스킬 레벨은 무조건 1
                if (data == null)
                {
                    Debug.LogError($"스킬 세팅 실패: {nameof(skillId)} = {skillId}");
                    continue;
                }

                if (data.skill_type.ToEnum<SkillType>() != SkillType.Active && data.skill_type.ToEnum<SkillType>() != SkillType.BasicActiveSkill)
                {
                    Debug.LogError($"[공격 시 발동되는 스킬 세팅 실패] 공격 타입이 아님: skill = {skillId}");
                    continue;
                }

                SkillInfo info = new ActiveSkill();
                info.SetSkillRate(item.Value); // 스킬 발동확률: value
                info.SetRefBattleOption(0L, BattleOptionType.ActiveSkill); // 스킬 참조 후 자동 사용 확률
                info.SetData(data);

                // 쿨타임 대기 상태로 변경
                if (refSkillResponseCooldownCheckStateHashSet.Contains(skillId))
                {
                    info.SetResponseCooldownCheckState();
                }

                // 참조 쿨타임 적용
                if (refSkillRemainTimeDic.ContainsKey(skillId))
                {
                    info.StartCooldown((long)refSkillRemainTimeDic[skillId]);
                }

                extraActiveSkillList.Add(info);
            }
        }

        /// <summary>
        /// 스킬 연계 세팅
        /// </summary>
        public void SetSkillChain(Dictionary<int, List<int>> dic)
        {
            skillChainDic.Clear();

            if (dic == null)
                return;

            foreach (var pair in dic)
            {
                if (!skillChainDic.ContainsKey(pair.Key))
                    skillChainDic.Add(pair.Key, new List<SkillInfo>());

                for (int i = 0; i < pair.Value.Count; i++)
                {
                    int skillId = pair.Value[i];
                    SkillData data = skillDataRepo.Get(skillId, level: 1); // 참조하는 스킬 레벨은 무조건 1
                    if (data == null)
                    {
                        Debug.LogError($"스킬 세팅 실패: {nameof(skillId)} = {skillId}");
                        continue;
                    }

                    if (data.skill_type.ToEnum<SkillType>() != SkillType.Active && data.skill_type.ToEnum<SkillType>() != SkillType.BasicActiveSkill)
                    {
                        Debug.LogError($"[공격 시 발동되는 스킬 세팅 실패] 공격 타입이 아님: skill = {skillId}");
                        continue;
                    }

                    SkillInfo info = new ActiveSkill();
                    info.SetSkillRate(10000); // 스킬 발동확률: value
                    info.SetChainableSkill(true); // 연계로 인해 발동되는 스킬 여부: true
                    info.SetRefBattleOption(0L, BattleOptionType.SkillChain); // 스킬 연계
                    info.SetData(data);

                    // 쿨타임 대기 상태로 변경
                    if (refSkillResponseCooldownCheckStateHashSet.Contains(skillId))
                    {
                        info.SetResponseCooldownCheckState();
                    }

                    // 참조 쿨타임 적용
                    if (refSkillRemainTimeDic.ContainsKey(skillId))
                    {
                        info.StartCooldown((long)refSkillRemainTimeDic[skillId]);
                    }

                    skillChainDic[pair.Key].Add(info);
                }
            }
        }

        /// <summary>
        /// 평타 스킬 세팅
        /// </summary>
        private bool SetBasicActiveSkill(int skillId)
        {
            if (skillId == 0)
                return false;

            // 바뀐 값이 없을 경우
            if (!basicActiveSkill.IsInvalidData && basicActiveSkill.SkillId == skillId)
                return false;

            basicActiveSkill.ResetData(); // 현재 세팅 초기화

            SkillData data = skillDataRepo.Get(skillId, level: 1); // 평타 스킬은 무조건 레벨이 1이다
            if (data == null)
            {
                Debug.LogError($"[평타 스킬 추가 실패] 데이터가 없음: {nameof(skillId)} = {skillId}");
                return false;
            }

            if (data.skill_type.ToEnum<SkillType>() != SkillType.BasicActiveSkill)
            {
                Debug.LogError($"[평타 스킬 추가 실패] 평타 타입이 아님: skill = {skillId}");
                return false;
            }

            basicActiveSkill.SetSkillRate(10000); // 스킬 발동확률: 10000
            basicActiveSkill.SetData(data);

            return true;
        }

        /// <summary>
        /// 직업 패시브 스킬 세팅
        /// </summary>
        private void SetBasicPassiveSkill(int skillId, int skillLevel)
        {
            if (skillId == 0)
                return;

            if (skillOverrideDic.ContainsKey(skillId))
                skillId = skillOverrideDic[skillId];

            SkillData data = skillDataRepo.Get(skillId, skillLevel);
            if (data == null)
            {
                Debug.LogError($"[직업 패시브 스킬 추가 실패] 데이터가 없음: {nameof(skillId)} = {skillId}");
                return;
            }

            if (data.skill_type.ToEnum<SkillType>() != SkillType.Passive)
            {
                Debug.LogError($"[직업 패시브 스킬 추가 실패] 패시브 타입이 아님: skill = {skillId}");
                return;
            }

            Add(new BattleOption(data.battle_option_type_1, data.value1_b1, data.value2_b1));
            Add(new BattleOption(data.battle_option_type_2, data.value1_b2, data.value2_b2));
            Add(new BattleOption(data.battle_option_type_3, data.value1_b3, data.value2_b3));
            Add(new BattleOption(data.battle_option_type_4, data.value1_b4, data.value2_b4));
        }

        /// <summary>
        /// 참조 평타 스킬 반환
        /// </summary>
        public IEnumerable<SkillInfo> GetExtraBasicActiveSkills()
        {
            for (int i = 0; i < extraBasicActiveSkillList.Count; i++)
            {
                yield return extraBasicActiveSkillList[i];
            }
        }

        /// <summary>
        /// 액티브 스킬 반환
        /// </summary>
        public IEnumerable<SkillInfo> GetActiveSkills()
        {
            for (int i = 0; i < activeSkillList.Count; i++)
            {
                yield return activeSkillList[i];
            }

            // 참조 액티브 스킬
            for (int i = 0; i < extraActiveSkillList.Count; i++)
            {
                yield return extraActiveSkillList[i];
            }
        }

        /// <summary>
        /// 공격 시 추가 스킬 반환
        /// </summary>
        public IEnumerable<SkillInfo> GetBlowActiveSkills(ElementType elementType)
        {
            if (elementType == ElementType.Fire)
            {
                for (int i = 0; i < fireActiveSkillList.Count; i++)
                {
                    yield return fireActiveSkillList[i];
                }
            }
        }

        /// <summary>
        /// 공격 시 추가 스킬 반환
        /// </summary>
        public IEnumerable<SkillInfo> GetBlowActiveSkills(int skillId)
        {
            if (skillChainDic.ContainsKey(skillId))
            {
                foreach (var item in skillChainDic[skillId])
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// 피격 시 추가 스킬 반환
        /// </summary>
        public IEnumerable<SkillInfo> GetBeAttackedActiveSkills(ElementType elementType)
        {
            yield break;
        }

        /// <summary>
        /// 평타 스킬의 공격 타입 반환
        /// (근거리/원거리 에 따라서 ATK 값을 변경해서 보여줘야 하기 때문에)
        /// </summary>
        public AttackType GetBasicActiveAttackType()
        {
            return basicActiveSkill.AttackType;
        }

        /// <summary>
        /// 참조 스킬 쿨타임 대기 상태로 변경
        /// </summary>
        public void SetRefSkillResponseCooldownCheckState(int skillId)
        {
            refSkillResponseCooldownCheckStateHashSet.Add(skillId);

            // 평타 시 발동
            foreach (var item in extraBasicActiveSkillList)
            {
                if (item.SkillId == skillId)
                {
                    item.SetResponseCooldownCheckState();
                }
            }

            // 공격 시 발동
            foreach (var item in extraActiveSkillList)
            {
                if (item.SkillId == skillId)
                {
                    item.SetResponseCooldownCheckState();
                }
            }

            // 화속성 공격 시 발동
            foreach (var item in fireActiveSkillList)
            {
                if (item.SkillId == skillId)
                {
                    item.SetResponseCooldownCheckState();
                }
            }

            // 스킬 초월
            foreach (var item in activeSkillList)
            {
                if (item.SkillId == skillId)
                {
                    item.SetResponseCooldownCheckState();
                }
            }

            // 스킬 연계
            foreach (var skillList in skillChainDic.Values)
            {
                foreach (var item in skillList)
                {
                    if (item.SkillId == skillId)
                    {
                        item.SetResponseCooldownCheckState();
                    }
                }
            }
        }

        /// <summary>
        /// 참조 스킬 쿨타임 따로 관리
        /// </summary>
        public void SetRefCooldown(int refSkillId, long remainCooldown)
        {
            if (!refSkillRemainTimeDic.ContainsKey(refSkillId))
            {
                refSkillRemainTimeDic.Add(refSkillId, remainCooldown);
            }
            else
            {
                refSkillRemainTimeDic[refSkillId] = remainCooldown;
            }

            // 평타 시 발동
            foreach (var item in extraBasicActiveSkillList)
            {
                if (item.SkillId == refSkillId)
                {
                    item.StartCooldown(remainCooldown);
                }
            }

            // 공격 시 발동
            foreach (var item in extraActiveSkillList)
            {
                if (item.SkillId == refSkillId)
                {
                    item.StartCooldown(remainCooldown);
                }
            }

            // 화속성 공격 시 발동
            foreach (var item in fireActiveSkillList)
            {
                if (item.SkillId == refSkillId)
                {
                    item.StartCooldown(remainCooldown);
                }
            }

            // 스킬 초월
            foreach (var item in activeSkillList)
            {
                if (item.SkillId == refSkillId)
                {
                    item.StartCooldown(remainCooldown);
                }
            }

            // 스킬 연계
            foreach (var skillList in skillChainDic.Values)
            {
                foreach (var item in skillList)
                {
                    if (item.SkillId == refSkillId)
                    {
                        item.StartCooldown(remainCooldown);
                    }
                }
            }
        }

        /// <summary>
        /// 실제 사용하는 전투스킬 반환 (초월스킬이 있다면 초월스킬로 반환)
        /// </summary>
        public SkillInfo GetBattleSkill(int skillId)
        {
            if (skillOverrideDic.ContainsKey(skillId))
            {
                skillId = skillOverrideDic[skillId];
            }

            foreach (var item in activeSkillList)
            {
                if (item.SkillId == skillId)
                    return item;
            }

            foreach (var item in skillOverridePassiveSkillList)
            {
                if (item.SkillId == skillId)
                    return item;
            }

            return null;
        }
    }
}