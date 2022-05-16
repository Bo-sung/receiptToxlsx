using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    public class MonsterModel : UnitModel<MonsterEntity>
    {
        private readonly MonsterDataManager monsterDataRepo;
        private readonly SkillDataManager skillDataRepo;
        private readonly BetterList<SkillInfo> skillList;
        private readonly SkillInfo angrySkill;
        private readonly Buffer<SkillInfo> skillBuffer;

        private ObscuredInt monsterId;
        private ObscuredInt monsterLevel;

        public int MonsterID => monsterId;
        public int MonsterLevel => monsterLevel;
        public bool CanBeAngry => GetCanBeAngry();
        public float AngryHPRate => GetAngryHPRate();

        private MonsterData data;

        public MonsterModel()
        {
            monsterDataRepo = MonsterDataManager.Instance;
            skillDataRepo = SkillDataManager.Instance;

            skillList = new BetterList<SkillInfo>();
            angrySkill = new PassiveSkill();
            skillBuffer = new Buffer<SkillInfo>();
        }

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        /// <summary>
        /// 몬스터 세팅
        /// </summary>
        public void Initialize(ISpawnMonster spawnMonster)
        {
            Initialize(spawnMonster.Id, spawnMonster.Level);
        }

        /// <summary>
        /// 몬스터 세팅
        /// </summary>
        public void Initialize(int monsterId, int monsterLevel)
        {
            bool isDirtyMonsterId = SetMonsterId(monsterId);
            bool isDirtyMonsterLevel = SetMonsterLevel(monsterLevel);

            if (isDirtyMonsterId || isDirtyMonsterLevel)
            {
                Entity.ReloadStatus(); // 스탯 다시로드
            }
        }

        /// <summary>
        /// 몬스터 데이터 반환
        /// </summary>
        public MonsterData GetMonsterData()
        {
            return data;
        }

        /// <summary>
        /// 유효한 스킬 목록
        /// </summary>
        public SkillInfo[] GetValidSkillList(bool isAngry)
        {
            for (int i = 0; i < skillList.size; i++)
            {
                skillBuffer.Add(skillList[i]);
            }

            if (isAngry && !angrySkill.IsInvalidData)
                skillBuffer.Add(angrySkill);

            return skillBuffer.GetBuffer(isAutoRelease: true);
        }

        private bool SetMonsterId(int id)
        {
            bool isDirty = monsterId.Replace(id);

            if (isDirty)
            {
                ResetData();
                ReloadData(id);
            }

            return isDirty;
        }

        private bool SetMonsterLevel(int level)
        {
            return monsterLevel.Replace(level);
        }

        public override void ResetData()
        {
            base.ResetData();

            data = null;
            skillList.Release();
            angrySkill.ResetData();
            skillBuffer.Release();
        }

        private void ReloadData(int id)
        {
            // 초기화
            data = monsterDataRepo.Get(id);
            if (data == null)
                return;

            for (int i = 0; i < Constants.Size.MONSTER_SKILL_SIZE; i++)
            {
                int skillId = data.GetSkillID(i);

                if (skillId == 0)
                    continue;

                int skillLevel = 1;
                int skillRate = data.GetSkillRate(i);

                SkillData skillData = skillDataRepo.Get(skillId, skillLevel);

                if (skillData == null)
                {
                    Debug.LogError($"스킬 세팅 실패: {nameof(skillId)} = {skillId}, {nameof(skillLevel)} = {skillLevel}");
                    continue;
                }

                SkillInfo info;

                SkillType skillType = skillData.skill_type.ToEnum<SkillType>();
                switch (skillType)
                {
                    case SkillType.Plagiarism:
                    case SkillType.Reproduce:
                    case SkillType.SummonBall:
                    case SkillType.Passive:
                    case SkillType.RuneMastery:
                        info = new PassiveSkill();
                        break;

                    case SkillType.Active:
                    case SkillType.BasicActiveSkill:
                        info = new ActiveSkill();
                        break;

                    default:
                        Debug.LogError($"설정되지 않은 타입: skillType = {skillType}");
                        continue;
                }

                info.SetData(skillData);
                info.SetSkillRate(skillRate);

                skillList.Add(info);
            }

            if (CanBeAngry)
            {
                int skillId = data.pattern_value2;
                int skillLevel = 1;
                SkillData skillData = skillDataRepo.Get(skillId, skillLevel);
                if (skillData == null)
                    return;

                SkillType skillType = skillData.skill_type.ToEnum<SkillType>();
                if (skillType != SkillType.Passive)
                {
                    Debug.LogError($"몬스터 분노 상태 패시브의 지정된 스킬 (id:{skillId})이 패시브 스킬이 아닙니다.");
                    return;
                }

                angrySkill.SetData(skillData);
            }
        }

        public bool GetCanBeAngry()
        {
            if (data == null)
                return false;

            return data.pattern_type != 0 && data.pattern_value1 > 0;
        }

        public float GetAngryHPRate()
        {
            if (data == null)
                return 0f;

            return MathUtils.ToPermyriadValue(data.pattern_value1);
        }
    }
}