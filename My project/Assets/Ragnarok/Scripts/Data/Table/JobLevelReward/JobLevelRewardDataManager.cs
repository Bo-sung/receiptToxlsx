using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public sealed class JobLevelRewardDataManager : Singleton<JobLevelRewardDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, JobLevelRewardData> dataDic;
        
        public ResourceType DataType => ResourceType.JobLevelRewardDataDB;

        public JobLevelRewardDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, JobLevelRewardData>(ObscuredIntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    JobLevelRewardData data = new JobLevelRewardData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public JobLevelRewardData Get(int id)
        {
            return dataDic.ContainsKey(id) ? dataDic[id] : null;
        }

        /// <summary>
        /// 스킬포인트 획득 그룹의 잡레벨 배열
        /// </summary>
        public int[] GetRewardJobLevelsByGroup(int group)
        {
            List<int> jobLevels = new List<int>();

            foreach(var v in dataDic.Values)
            {
                if (v.group_id == group)
                    jobLevels.Add(v.job_level);
            }

            // 오름차순 정렬
            jobLevels.Sort(delegate (int a, int b)
            {
                if (a > b) return 1;
                else if (a < b) return -1;
                return 0;
            });

            return jobLevels.ToArray();
        }

        /// <summary>
        /// 해당 그룹의 보상 스킬포인트
        /// </summary>
        public int GetRewardSkillPoint(int group)
        {
            foreach (var v in dataDic.Values)
            {
                if (v.group_id == group)
                    return v.reward_value;
            }

            return default; // 0
        }

        /// <summary>
        /// 잡레벨에 해당하는 획득가능한 보상 타입이 있는지
        /// </summary>
        public bool HasRewardByJobLevel(int jobLevel, RewardType rewardType = RewardType.SkillPoint)
        {
            foreach (var v in dataDic.Values)
            {
                if (v.job_level == jobLevel && v.reward_type.ToEnum<RewardType>() == rewardType)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            foreach (var item in dataDic.Values)
            {
                if(item.reward_type.ToEnum<RewardType>() == RewardType.Item)
                {
                    if (ItemDataManager.Instance.Get(item.reward_value) == null)
                        throw new System.Exception($"67.레벨보상 테이블 오류 ID={item.id}, 없는 아이템={item.reward_value}");
                }
            }
#endif
        }
    }
}