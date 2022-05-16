using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    public class CupetSkillData : IData
    {
        ObscuredInt[] skill_id1;
        ObscuredInt[] skill_id2;
        ObscuredInt[] skill_id3;
        ObscuredInt[] skill_id4;
        ObscuredInt[] skill_rate1;
        ObscuredInt[] skill_rate2;
        ObscuredInt[] skill_rate3;
        ObscuredInt[] skill_rate4;

        /// <summary>
        /// 모든 랭크의 
        /// </summary>
        /// <param name="cupetID"></param>
        public void Initialize(int cupetID)
        {
            int maxRank = BasisType.CUPET_MAX_RANK.GetInt();

            skill_id1       = new ObscuredInt[maxRank];
            skill_id2       = new ObscuredInt[maxRank];
            skill_id3       = new ObscuredInt[maxRank];
            skill_id4       = new ObscuredInt[maxRank];
            skill_rate1     = new ObscuredInt[maxRank];
            skill_rate2     = new ObscuredInt[maxRank];
            skill_rate3     = new ObscuredInt[maxRank];
            skill_rate4     = new ObscuredInt[maxRank];

            for (int i = 0; i < maxRank; ++i)
            {
                CupetData cupetData = CupetDataManager.Instance.Get(cupetID, rating: i + 1);

                skill_id1[i]    = cupetData.GetSkillID(0);
                skill_id2[i]    = cupetData.GetSkillID(1);
                skill_id3[i]    = cupetData.GetSkillID(2);
                skill_id4[i]    = cupetData.GetSkillID(3);
                skill_rate1[i]  = cupetData.GetSkillRate(0);
                skill_rate2[i]  = cupetData.GetSkillRate(1);
                skill_rate3[i]  = cupetData.GetSkillRate(2);
                skill_rate4[i]  = cupetData.GetSkillRate(3);
            }
        }

        public int GetSkillID(int index, int rank = 1)
        {
            if (rank < 1)
                return 0;

            switch (index)
            {
                case 0: return skill_id1[rank-1];
                case 1: return skill_id2[rank-1];
                case 2: return skill_id3[rank-1];
                case 3: return skill_id4[rank-1];

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(index)}] {nameof(index)} = {index}");
                    break;
            }

            return 0;
        }

        public int GetSkillRate(int index, int rank = 1)
        {
            if (rank < 1)
                return 0;

            switch (index)
            {
                case 0: return skill_rate1[rank-1];
                case 1: return skill_rate2[rank-1];
                case 2: return skill_rate3[rank-1];
                case 3: return skill_rate4[rank-1];

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(index)}] {nameof(index)} = {index}");
                    break;
            }

            return 0;
        }
    }
}