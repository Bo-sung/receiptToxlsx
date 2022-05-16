using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public sealed class JobDataManager : Singleton<JobDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, JobData> dataDic;
        private ObscuredInt maxJobClass;

        public ResourceType DataType => ResourceType.JobDataDB;

        public JobDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, JobData>(ObscuredIntEqualityComparer.Default);
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
            maxJobClass = 0;
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    JobData data = new JobData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);

                    int grade = data.grade;
                    if (maxJobClass < grade)
                        maxJobClass = grade;
                }
            }
        }

        public JobData Get(int id)
        {
            return dataDic.ContainsKey(id) ? dataDic[id] : null;
        }

        /// <summary>
        /// 다음 전직가능한 목록
        /// </summary>
        public JobData[] GetNextJobs(Job job)
        {
            return dataDic.Values.Where(x => x.previous_index == (byte)job).ToArray();
        }

        /// <summary>
        /// 2차 이후의 특정 등급의 직업을 검색.
        /// </summary>
        /// <param name="job">1차 직업의 경우 두개의 분기를 각각 처리</param>
        /// <param name="grade">타겟 등급</param>
        public JobData GetNextJob(Job job, int grade)
        {
            // 현재 직업
            JobData myJobData = Get((int)job);
            if (myJobData.grade == grade) return myJobData;

            // 다음차수 직업
            JobData jobData = dataDic.Values.FirstOrDefault(x => x.previous_index == (byte)job);
            if (jobData.grade == grade) return jobData;
            else return GetNextJob(jobData.id.ToEnum<Job>(), grade);
        }

        /// <summary>
        /// 특정 직업 차수의 스킬 목록 반환 (스킬 훔쳐배우기에 사용)
        /// </summary>
        public JobData[] GetSkillIds(int grade, Job withoutJob)
        {
            List<JobData> list = new List<JobData>();

            int withoutJobId = GetWithoutJobId(grade, (int)withoutJob);
            foreach (var item in dataDic.Values)
            {
                if (item.id == withoutJobId)
                    continue;

                if (item.grade == grade)
                    list.Add(item);
            }

            return list.ToArray();
        }

        /// <summary>
        /// 최대 직업 차수 (데이터 기반)
        /// </summary>
        public int GetMaxJobClass()
        {
            return maxJobClass;
        }

        /// <summary>
        /// 해당 차수에 해당하는 직업 데이터 반환
        /// </summary>
        public JobData[] GetJobs(int jobGrade)
        {
            List<JobData> list = new List<JobData>();

            foreach (var item in dataDic.Values)
            {
                if (item.grade == jobGrade)
                    list.Add(item);
            }

            return list.ToArray();
        }

        private int GetWithoutJobId(int grade, int withoutJobIndex)
        {
            JobData jobData = Get(withoutJobIndex);
            if (jobData == null)
                return 0;

            if (jobData.grade == grade)
                return jobData.id;

            return GetWithoutJobId(grade, jobData.previous_index); // 그 이전 차수
        }

        /// <summary>
        /// 현재 직업까지 배울 수 있는 모든 스킬 반환
        /// </summary>
        public int[] GetPreJobSkills(int jobGrade)
        {
            List<int> list = new List<int>();

            foreach (var item in dataDic.Values)
            {
                if (item.grade > jobGrade)
                    continue;

                for (int i = 0; i < JobData.MAX_SKILL_COUNT; i++)
                {
                    int skillId = item.GetSkillId(i);
                    if (skillId == 0)
                        continue;

                    if (list.Contains(skillId))
                        continue;

                    list.Add(skillId);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// 직업 그룹(1차 직업), 직업 없으면 노비스
        /// </summary>
        /// <param name="grade">2차 이후의 직업일 경우, 두번째 그룹(2차 직업)도 필요함..</param>
        public JobData GetJobGroup(int job, int grade = 1)
        {
            JobData jobData = Get(job);
            if (jobData == null)
                return Get((int)Job.Novice); // 노비스

            if (jobData.grade == grade)
                return jobData;

            return GetJobGroup(jobData.previous_index); // 그 이전 차수
        }

        public Job[] GetJobGradeCharacters(Job myJob, int targetJobGrade)
        {
            JobData myJobData = Get((int)myJob);

            // 노비스, 직업 등급이 0
            if (myJob == Job.Novice || myJobData.grade < 1) return new Job[] { Job.Novice };
            
            if(myJobData.grade > 1) // 직업 분기 이후, 1개만 표시
            {
                if (targetJobGrade == 1) // 1차 직업
                {
                    return new Job[] { GetJobGroup((int)myJob).id.ToEnum<Job>() };
                }
                else // 2차 이후의 직업
                {
                    JobData secondJobData = GetJobGroup((int)myJob, 2); // 2차직업
                    return new Job[] { GetNextJob(secondJobData.id.ToEnum<Job>(), targetJobGrade).id.ToEnum<Job>() };
                }
            }
            else // 직업 분기 이전, 1차 캐릭터
            {
                if(targetJobGrade == 1) // 1차 캐릭터
                {
                    return new Job[] { myJob };
                }
                else // 2차 등급 이후로는 2개씩 있음..
                {
                    JobData[] jobs = GetNextJobs(myJob); // 1차로 2채캐릭 두개 가져옴.
                    return new Job[] { GetNextJob(jobs[0].id.ToEnum<Job>(), targetJobGrade).id.ToEnum<Job>(),
                        GetNextJob(jobs[1].id.ToEnum<Job>(), targetJobGrade).id.ToEnum<Job>() };
                }
            }
        }

        public JobData[] GetJobGradeWithPreviousIndex(int jobGrade, int previousIndex)
        {
            List<JobData> list = new List<JobData>();

            foreach (var item in dataDic.Values)
            {
                if (item.grade == jobGrade && item.previous_index == previousIndex)
                    list.Add(item);
            }

            return list.ToArray();
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

#endif
        }
    }
}