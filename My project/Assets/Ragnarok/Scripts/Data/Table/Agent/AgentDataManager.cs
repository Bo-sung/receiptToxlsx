using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public sealed class AgentDataManager : Singleton<AgentDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, AgentData> dataDic;
        private readonly List<AgentData> agentDataList;
        private readonly List<DummySharingCharacter> dummySharingCharacterList;

        public IEnumerable<AgentData> AgentDataList => agentDataList;

        public ResourceType DataType => ResourceType.AgentDataDB;

        public AgentDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, AgentData>(ObscuredIntEqualityComparer.Default);
            agentDataList = new List<AgentData>();
            dummySharingCharacterList = new List<DummySharingCharacter>();
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
            agentDataList.Clear();
            dummySharingCharacterList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    AgentData data = new AgentData(mpo.AsList());
                    agentDataList.Add(data);
                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public AgentData Get(int id)
        {
            return dataDic.ContainsKey(id) ? dataDic[id] : null;
        }

        public DummySharingCharacter[] GetRandomDummySharingAgents(int stageID, IEnumerable<int> exceptIds, int count)
        {
            var keys = BasisType.SHARE_HIRING_QUALIFICATION.GetKeyList();

            int maximumKey = int.MinValue;

            for (int i = 0; i < keys.Count; ++i)
                if (keys[i] <= stageID && maximumKey < keys[i])
                    maximumKey = keys[i];

            int limitAttackPower = MathUtils.ToInt(BasisType.SHARE_HIRING_QUALIFICATION.GetInt(maximumKey)); // 전투력 제한
            var allFinded = dummySharingCharacterList.FindAll(a => a.Power <= limitAttackPower);
            allFinded.RemoveAll(a => exceptIds.Contains(a.agentId));
            return allFinded.GetRandomPick(count);
        }

        public DummySharingCharacter GetDummySharingAgent(int agentId)
        {
            return dummySharingCharacterList.Find(a => a.agentId == agentId);
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            StatDataManager statDataRepo = StatDataManager.Instance;
            JobDataManager jobDataRepo = JobDataManager.Instance;
            SkillDataManager.ISkillDataRepoImpl impl = SkillDataManager.Instance;

            const int AUTO_SHARING_AGENT_FLAG = (int)AgentType.AutoSharingAgent;
            int maxStatus = BasisType.MAX_STAT.GetInt(); // 500
            for (int i = 0; i < agentDataList.Count; i++)
            {
                if (agentDataList[i].agent_type == AUTO_SHARING_AGENT_FLAG)
                {
                    int jobId = agentDataList[i].job_id;
                    int jobLevel = agentDataList[i].dummy_lv;
                    JobData agentJobData = jobDataRepo.Get(jobId);
                    if (agentJobData == null)
                    {
                        Debug.LogError($"JobData가 존재하지 않습니다: {nameof(jobId)} = {jobId}");
                        continue;
                    }

                    int totalStatPoint = statDataRepo.GetTotalPoint(jobLevel);
                    JobData.StatValue basicStat = new JobData.StatValue(0);
                    JobData.StatValue maxStat = new JobData.StatValue(maxStatus);
                    short[] plusGuidStats = agentJobData.GetAutoStatGuidePoints(totalStatPoint, basicStat, maxStat);
                    DummySharingCharacter info = new DummySharingCharacter(agentDataList[i]);
                    info.UpdateStatus(jobLevel, plusGuidStats[0], plusGuidStats[1], plusGuidStats[2], plusGuidStats[3], plusGuidStats[4], plusGuidStats[5]);
                    info.Initialize(impl);
                    dummySharingCharacterList.Add(info);
                }
            }
        }

        public void VerifyData()
        {
#if UNITY_EDITOR

#endif
        }
    }
}