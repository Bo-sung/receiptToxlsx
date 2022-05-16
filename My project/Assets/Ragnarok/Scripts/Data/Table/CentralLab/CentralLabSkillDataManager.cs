using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class CentralLabSkillDataManager : Singleton<CentralLabSkillDataManager>
        , IDataManger, IEqualityComparer<CentralLabSkillType>, IEqualityComparer<Job>
    {
        private readonly Dictionary<CentralLabSkillType, BetterList<int>> commonSkillBuffer;
        private readonly Dictionary<Job, Dictionary<CentralLabSkillType, BetterList<int>>> skillBuffer;

        private readonly Dictionary<CentralLabSkillType, BetterList<SkillData>> commonSkillDic;
        private readonly Dictionary<Job, Dictionary<CentralLabSkillType, BetterList<SkillData>>> skillDic;

        private readonly Buffer<SkillData> buffer;

        public ResourceType DataType => ResourceType.CLabSkillDataDB;

        public CentralLabSkillDataManager()
        {
            commonSkillBuffer = new Dictionary<CentralLabSkillType, BetterList<int>>(this);
            skillBuffer = new Dictionary<Job, Dictionary<CentralLabSkillType, BetterList<int>>>(this);

            commonSkillDic = new Dictionary<CentralLabSkillType, BetterList<SkillData>>(this);
            skillDic = new Dictionary<Job, Dictionary<CentralLabSkillType, BetterList<SkillData>>>(this);

            buffer = new Buffer<SkillData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            ClearBuffers();

            commonSkillDic.Clear();
            skillDic.Clear();

            buffer.Release();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    CentralLabSkillData data = new CentralLabSkillData(mpo.AsList());

                    Job job = data.job_id.ToEnum<Job>();
                    int skillId = data.skill_id;
                    CentralLabSkillType skillType = data.skill_type.ToEnum<CentralLabSkillType>();

                    // 공통일 경우
                    if (job == default)
                    {
                        if (!commonSkillBuffer.ContainsKey(skillType))
                            commonSkillBuffer.Add(skillType, new Buffer<int>());

                        commonSkillBuffer[skillType].Add(skillId);
                    }
                    else
                    {
                        if (!skillBuffer.ContainsKey(job))
                            skillBuffer.Add(job, new Dictionary<CentralLabSkillType, BetterList<int>>(this));

                        if (!skillBuffer[job].ContainsKey(skillType))
                            skillBuffer[job].Add(skillType, new BetterList<int>());

                        skillBuffer[job][skillType].Add(skillId);
                    }
                }
            }
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            // 스킬데이터 반환
            SkillDataManager skillDataRepo = SkillDataManager.Instance;
            foreach (var item in commonSkillBuffer)
            {
                foreach (var skillId in item.Value)
                {
                    SkillData skillData = skillDataRepo.Get(skillId, level: 1);
                    if (skillData == null)
                    {
                        Debug.LogError($"존재하지 않는 스킬 id가 존재: {nameof(skillId)} = {skillId}");
                        continue;
                    }

                    if (!commonSkillDic.ContainsKey(item.Key))
                        commonSkillDic.Add(item.Key, new BetterList<SkillData>());

                    commonSkillDic[item.Key].Add(skillData);
                }
            }

            foreach (var item in skillBuffer)
            {
                foreach (var item2 in item.Value)
                {
                    foreach (var skillId in item2.Value)
                    {
                        SkillData skillData = skillDataRepo.Get(skillId, level: 1);
                        if (skillData == null)
                        {
                            Debug.LogError($"존재하지 않는 스킬 id가 존재: {nameof(skillId)} = {skillId}");
                            continue;
                        }

                        if (!skillDic.ContainsKey(item.Key))
                            skillDic.Add(item.Key, new Dictionary<CentralLabSkillType, BetterList<SkillData>>(this));

                        if (!skillDic[item.Key].ContainsKey(item2.Key))
                            skillDic[item.Key].Add(item2.Key, new BetterList<SkillData>());

                        skillDic[item.Key][item2.Key].Add(skillData);
                    }
                }
            }

            ClearBuffers();
        }

        private void ClearBuffers()
        {
            commonSkillBuffer.Clear();
            skillBuffer.Clear();
        }

        public SkillData[] GetSkills(Job job, CentralLabSkillType type)
        {
            // 공통 스킬
            if (commonSkillDic.ContainsKey(type))
                buffer.AddRange(commonSkillDic[type].ToArray());

            if (skillDic.ContainsKey(job) && skillDic[job].ContainsKey(type))
                buffer.AddRange(skillDic[job][type].ToArray());

            return buffer.GetBuffer(isAutoRelease: true);
        }

        public void VerifyData()
        {
#if UNITY_EDITOR

#endif
        }

        bool IEqualityComparer<CentralLabSkillType>.Equals(CentralLabSkillType x, CentralLabSkillType y)
        {
            return x == y;
        }

        int IEqualityComparer<CentralLabSkillType>.GetHashCode(CentralLabSkillType obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer<Job>.Equals(Job x, Job y)
        {
            return x == y;
        }

        int IEqualityComparer<Job>.GetHashCode(Job obj)
        {
            return obj.GetHashCode();
        }
    }
}