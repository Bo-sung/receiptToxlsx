using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 큐펫이 지니고 있는 스킬들에 대한 정보
    /// </summary>
    public class CupetSkillInfo : DataInfo<CupetSkillData>
    {
        public int cupetID;

        public CupetSkillInfo(int cupetID)
        {
            SetData(cupetID);
        }

        public void SetData(int cupetID)
        {
            this.cupetID = cupetID;
            CupetSkillData cupetSkillData = new CupetSkillData();
            cupetSkillData.Initialize(this.cupetID);
            SetData(cupetSkillData);
        }

        /// <summary>
        /// 해당 큐펫의 스킬 리스트를 반환.
        /// </summary>
        /// <param name="isPreview">보유하지 않은 스킬은 1레벨 스킬로 만들어서 반환</param>
        public List<SkillInfo> GetCupetSkillList(int nowRank, bool isPreview = false)
        {
            List<SkillInfo> retSkillInfoList = new List<SkillInfo>();
            for (int i = 0; i < CupetData.CUPET_SKILL_SIZE; ++i)
            {
                // 보유하지 않은 스킬인지 판단.
                int skillID = data.GetSkillID(i, nowRank);

                if (skillID == 0)
                {
                    if (!isPreview) // 프리뷰 모드가 아니면 그냥 스킬없는채로 넘어감.
                    {
                        retSkillInfoList.Add(null);
                        continue;
                    }

                    // 프리뷰 모드면 1레벨의 스킬을 찾아서 추가.
                    int firstLevelRank = GetUnlockRank(i);
                    skillID = data.GetSkillID(i, firstLevelRank);
                }

                SkillData skillData = SkillDataManager.Instance.Get(skillID, 1);
                if (skillData == null)
                {
                    retSkillInfoList.Add(null);
                    Debug.LogError("SkillData 없음.");
                    continue;
                }

                SkillType skillType = skillData.skill_type.ToEnum<SkillType>();
                SkillInfo skillInfo = null;
                switch (skillType)
                {
                    case SkillType.Passive:
                        skillInfo = new PassiveSkill();
                        break;
                    case SkillType.Active:
                        skillInfo = new ActiveSkill();
                        break;
                    default:
                        Debug.LogError($"{skillID}의 스킬타입 지정 불가.");
                        break;
                }

                if (skillInfo == null)
                {
                    retSkillInfoList.Add(null);
                    Debug.LogError("SkillType 없음.");
                    continue;
                }

                skillInfo.SetData(skillData);
                retSkillInfoList.Add(skillInfo);
            }

            return retSkillInfoList;
        }

        /// <summary>
        /// 해당 스킬이 처음으로 습득되는 랭크를 반환.
        /// </summary>
        public int GetUnlockRank(int skillIndex)
        {
            int maxRank = BasisType.CUPET_MAX_RANK.GetInt();
            for (int i = 0; i < maxRank; ++i)
            {
                int skillID = data.GetSkillID(skillIndex, i + 1);
                if (skillID != 0)
                {
                    return i + 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// 해당 스킬이 해금된 상태인지 반환
        /// </summary>
        public bool HasSkill(int skillIndex, int nowRank)
        {
            return (GetUnlockRank(skillIndex) <= nowRank);
        }

        /// <summary>
        /// 해당 스킬에 변화가 생기는(=레벨업 되는) 다음 랭크를 반환.
        /// </summary>
        public int GetNextLevelRank(int skillIndex, int nowRank)
        {
            int nowSkillID = data.GetSkillID(skillIndex, nowRank);
            int nowSkillRate = data.GetSkillRate(skillIndex, nowRank);

            int maxRank = BasisType.CUPET_MAX_RANK.GetInt();
            for (int i = nowRank; i <= maxRank; ++i)
            {
                int skillID = data.GetSkillID(skillIndex, i);
                if (skillID != nowSkillID)
                {
                    return i;
                }

                int skillRate = data.GetSkillRate(skillIndex, i);
                if (skillRate != nowSkillRate)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// 해당 스킬이 몇 레벨인지 반환. (큐펫스킬에는 레벨데이터가 없음.)
        /// </summary>
        public int GetSkillLevel(int skillIndex, int rank)
        {
            int level = 0;
            int lastSkillID = 0;
            int lastSkillRate = 0;

            int maxRank = BasisType.CUPET_MAX_RANK.GetInt();
            for (int i = 1; i <= rank; ++i)
            {
                int skillID = data.GetSkillID(skillIndex, i);
                int skillRate = data.GetSkillRate(skillIndex, i);
                if (skillID != lastSkillID || skillRate != lastSkillRate)
                {
                    ++level;
                    lastSkillID = skillID;
                    lastSkillRate = skillRate;
                }
            }

            return level;

        }
    }
}