using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 캐릭터 기본 정보
    /// </summary>
    public class CharacterModel : CharacterEntityModel, View.UICharacterIntroduction.IInput, CharacterModel.IInputValue
    {
        public interface IInputValue
        {
            /// <summary>
            /// 더미동료의 경우에는 동료Id의 마이너스 값
            /// </summary>
            int Cid { get; }
            string Name { get; }
            byte Job { get; }
            byte Gender { get; }
            int Level { get; }
            int LevelExp { get; }
            int JobLevel { get; }
            long JobLevelExp { get; }
            int RebirthCount { get; }
            int RebirthAccrueCount { get; }
            int NameChangeCount { get; }
            string CidHex { get; }
            int ProfileId { get; }
        }

        public interface IShareForceLevelValue
        {
            int Group { get; }
            int Level { get; }
        }

        private readonly JobDataManager jobDataRepo;
        private readonly StatDataManager statDataRepo;
        private readonly IGamePotImpl gamePotImpl;
        private readonly ConnectionManager connectionManager;
        private readonly JobLevelRewardDataManager jobLevelRewardRepo;
        private readonly AgentDataManager agentDataRepo;
        private readonly ProfileDataManager profileDataRepo;
        private readonly ShareStatBuildUpDataManager shareStatBuildUpDataRepo;

        private ObscuredInt cid;
        private ObscuredString name;
        private ObscuredByte job;
        private ObscuredByte gender;
        private ObscuredInt level;
        private ObscuredInt levelExp;
        private ObscuredInt jobLevel;
        private ObscuredLong jobLevelExp;
        private ObscuredInt rebirthCount; // 전승 횟수
        private ObscuredInt rebirthAccrueCount;
        private ObscuredInt nameChangeCount; // 이름 변경한 횟수
        private ObscuredString cidHex;

        public int Cid => cid;
        public string Name => GetName();
        byte IInputValue.Job => job;
        byte IInputValue.Gender => gender;
        public virtual Job Job => job.ToEnum<Job>();
        public virtual Gender Gender => gender.ToEnum<Gender>();
        public int Level => level;
        public int LevelExp => levelExp;
        public int JobLevel => jobLevel;
        public long JobLevelExp => jobLevelExp;
        public int RebirthCount => rebirthCount;
        public int RebirthAccrueCount => rebirthAccrueCount;
        public int NameChangeCount => nameChangeCount;
        public string CidHex => cidHex;
        public string JobTextureName => Job.GetJobSDName(Gender);
        public int ProfileId { get; private set; }

        /// <summary>전승 가능한 레벨인지 체크</summary>
        public bool CanRebirthLv { get { return level >= PossibleRebirthLv; } }
        /// <summary>획득 가능한 전승 스탯이 있는지 체크</summary>
        public bool HaveAddPoint { get { return RebirthAccrueCount < BasisType.MAX_BASE_REBIRTH_ACCRUE_STAT.GetInt(); } }

        /// <summary>남은 누적 스탯 포인트</summary>
        public int RemainAccruePoint
        {
            get { return Mathf.Max(0, BasisType.MAX_BASE_REBIRTH_ACCRUE_STAT.GetInt() - RebirthAccrueCount); }
        }

        /// <summary>전승 시 추가되는 AP</summary>
        public int AddRibirthPoint
        {
            get { return Mathf.Min(statDataRepo.Get(Level).transmission_stat, RemainAccruePoint); }
        }

        /// <summary>전승 가능 레벨</summary>
        public int PossibleRebirthLv
        {
            get { return statDataRepo.PossibleRebirthLv; }
        }

        private readonly Dictionary<ShareForceType, int> shareForceDic; // 합체 타임슈트 정보

        /// <summary>
        /// 쉐어포스 레벨 정보
        /// kee: group, value: level
        /// </summary>
        private readonly Dictionary<int, int> shareForceStatusLevelDic;

        /// <summary>
        /// 레벨 변경 시 호출
        /// </summary>
        public event Action<int> OnUpdateLevel;

        /// <summary>
        /// 레벨 경험치 변경 시 호출
        /// </summary>
        public event Action<int> OnUpdateLevelExp;

        /// <summary>
        /// 직업 레벨 변경 시 호출
        /// </summary>
        public event Action<int> OnUpdateJobLevel;

        /// <summary>
        /// 직업 레벨 경험치 변경 시 호출
        /// </summary>
        public event Action<long> OnUpdateJobExp;

        /// <summary>
        /// 전직 시 호출
        /// </summary>
        public event Action<bool> OnChangedJob; // bool: isInit

        /// <summary>
        /// 성별 변경 시 호출
        /// </summary>
        public event Action OnChangedGender;

        /// <summary>
        /// 캐릭터 이름 변경시 호출
        /// </summary>
        public event Action<string> OnChangedName;

        /// <summary>
        /// 전승 시 호출
        /// </summary>
        public event Action OnRebirth;

        /// <summary>
        /// 쉐어포스 레벨업 시 호출
        /// </summary>
        public event Action OnShareForceLevelUp;

        /// <summary>
        /// 프로필 업데이트
        /// </summary>
        public event Action OnUpdateProfile;

        /// <summary>
        /// 쉐어포스 스탯 레벨업
        /// </summary>
        public event Action OnUpdateShareForceStatus;

        public virtual void SetJob(Job job)
        {
            this.job = job.ToByteValue();
        }

        public virtual void SetGender(Gender gender)
        {
            this.gender = gender.ToByteValue();
            OnChangedGender?.Invoke();
        }

        public void SetCID(int CID)
        {
            this.cid = CID;
        }

        public void SetJobLevel(int level)
        {
            this.jobLevel = level;
        }

        public CharacterModel()
        {
            jobDataRepo = JobDataManager.Instance;
            statDataRepo = StatDataManager.Instance;
            gamePotImpl = GamePotSystem.Instance;
            connectionManager = ConnectionManager.Instance;
            jobLevelRewardRepo = JobLevelRewardDataManager.Instance;
            agentDataRepo = AgentDataManager.Instance;
            profileDataRepo = ProfileDataManager.Instance;
            shareStatBuildUpDataRepo = ShareStatBuildUpDataManager.Instance;
            shareForceDic = new Dictionary<ShareForceType, int>();
            shareForceStatusLevelDic = new Dictionary<int, int>(IntEqualityComparer.Default);
        }

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        public override void ResetData()
        {
            base.ResetData();

            cid = 0;
            name = null;
            job = 0;
            gender = 0;
            level = 0;
            levelExp = 0;
            jobLevel = 0;
            jobLevelExp = 0;
            rebirthCount = 0;
            rebirthAccrueCount = 0;
            nameChangeCount = 0;
            cidHex = null;
            shareForceDic.Clear();
            ProfileId = 0;
            shareForceStatusLevelDic.Clear();
        }

        internal void Initialize(IInputValue inputValue)
        {
            cid = inputValue.Cid;
            name = inputValue.Name;
            SetJob(inputValue.Job.ToEnum<Job>());
            SetGender(inputValue.Gender.ToEnum<Gender>());
            level = inputValue.Level;
            levelExp = inputValue.LevelExp;
            jobLevel = inputValue.JobLevel;
            jobLevelExp = inputValue.JobLevelExp;
            rebirthCount = inputValue.RebirthCount;
            rebirthAccrueCount = inputValue.RebirthAccrueCount;
            nameChangeCount = inputValue.NameChangeCount;
            cidHex = inputValue.CidHex;
            ProfileId = Mathf.Max(0, inputValue.ProfileId);

            OnChangedJob?.Invoke(true);
            OnChangedName?.Invoke(name);
            OnUpdateProfile?.Invoke();
        }

        /// <summary>
        /// 클론 캐릭터 전용
        /// </summary>
        internal void Initialize(IEnumerable<IShareForceLevelValue> shareForceLevelValues)
        {
            shareForceStatusLevelDic.Clear();
            foreach (IShareForceLevelValue item in shareForceLevelValues)
            {
                shareForceStatusLevelDic.Add(item.Group, item.Level);
            }
        }

        internal void UpdateData(short? inputLevel, int? inputLevelExp, short? inputJobLevel, long? inputJobExp, short? inputAccrueStatPoint)
        {
            int oldLevel = level;
            int oldJobLevel = jobLevel;

            if (level.Replace(inputLevel))
                OnUpdateLevel?.Invoke(level);

            if (levelExp.Replace(inputLevelExp))
                OnUpdateLevelExp?.Invoke(LevelExp);

            UpdateJobLevel(inputJobLevel);

            if (inputJobExp.HasValue)
            {
                var gap = inputJobExp - jobLevelExp;
                if (DebugUtils.IsLogExp)
                    Debug.Log($"직업경험치 : add = {gap}, total = {inputJobExp:N0}, level = {jobLevel}");
            }

            if (jobLevelExp.Replace(inputJobExp))
                OnUpdateJobExp?.Invoke(jobLevelExp);

            rebirthAccrueCount.Replace(inputAccrueStatPoint);

            if (Entity.type == UnitEntityType.Player)
            {
                // 퀘스트 처리
                bool isLevelUp = level > oldLevel;
                bool isJobLevelUp = jobLevel > oldJobLevel;

                if (isJobLevelUp)
                {
                    Analytics.TrackEvent($"Joblevel{jobLevel}");
                    Quest.QuestProgress(QuestType.CHARACTER_MAX_JOB_LEVEL, questValue: jobLevel); // 직업 레벨 도달
                }

                if (isLevelUp)
                {
                    Analytics.TrackEvent($"Baselevel{level}");
                    Quest.QuestProgress(QuestType.CHARACTER_MAX_BASIC_LEVEL, questValue: level); // 일반 레벨 도달
                    Quest.QuestProgress(QuestType.CHARACTER_BASIC_LEVEL_MAX, questValue: level - oldLevel); // 일반 레벨업 횟수
                }
            }
        }

        public void UpdateJobLevel(int? jobLevel)
        {
            if (this.jobLevel.Replace(jobLevel))
            {
                if (jobLevel == BasisType.GUILD_JOIN_NEED_JOB_LEVEL.GetInt())
                    Entity.Quest.AddNewOpenContent(ContentType.Guild);

                // 스킬포인트 획득 체크
                if (jobLevelRewardRepo.HasRewardByJobLevel(JobLevel))
                {
                    UI.ShowToastPopup(LocalizeKey._2026.ToText().Replace(ReplaceKey.LEVEL, JobLevel)); // JOB Lv {LEVEL} 달성을 축하합니다.우편함에서 보상을 수령해주세요.
                    NotifyAddAlarm(AlarmType.MailCharacter); // 빨콩표시
                }

                OnUpdateJobLevel?.Invoke(JobLevel);

                UI.Show<UIJobLvUp>().Set(jobLevel.Value);
            }
        }

        /// <summary>
        /// 직업 데이터 반환
        /// </summary>
        public JobData GetJobData()
        {
            return jobDataRepo.Get(job);
        }

        public int JobGrade()
        {
            JobData jobData = GetJobData();
            if (jobData == null)
                return 0;

            return jobData.grade;
        }

        public int JobNextGrade()
        {
            return JobGrade() + 1;
        }

        /// <summary>
        /// 전직 가능 여부
        /// </summary>
        public bool CanChangeJob()
        {
            // 전직 가능 레벨에 도달 && 다음 전직차수가 있어야함
            return JobLevel >= BasisType.JOB_MAX_LEVEL.GetInt(JobNextGrade()) && JobGrade() < jobDataRepo.GetMaxJobClass();
        }

        /// <summary>
        /// 직업 레벨 체크
        /// </summary>
        public bool IsCheckJobLevel(int needLevel, bool isShowPopup)
        {
            if (Cheat.All_OPEN_CONTENT)
                return true;

            // 직업레벨 부족
            if (JobLevel < needLevel)
            {
                if (isShowPopup)
                {
                    string description = LocalizeKey._90087.ToText() // 직업레벨 {LEVEL} 이후에 오픈됩니다.
                        .Replace(ReplaceKey.LEVEL, needLevel);

                    UI.ShowToastPopup(description);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 직업 차수 체크
        /// </summary>
        public bool IsCheckJobGrade(int needGrade, bool isShowPopup)
        {
            if (Cheat.All_OPEN_CONTENT)
                return true;

            // 직업레벨 부족
            if (JobGrade() < needGrade)
            {
                if (isShowPopup)
                {
                    string description = LocalizeKey._90170.ToText() // {VALUE}차 전직 후 이용 가능합니다.
                        .Replace(ReplaceKey.VALUE, needGrade)
                        .Replace(ReplaceKey.LEVEL, BasisType.JOB_MAX_LEVEL.GetInt(needGrade));

                    UI.ShowToastPopup(description);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 성별 변환
        /// </summary>
        public void ChangeGender()
        {
            Gender newGender = Gender != Gender.Male ? Gender.Male : Gender.Female;
            SetGender(newGender);
        }

        /// <summary>
        /// 캐릭터 전직
        /// </summary>
        public async Task<bool> RequestCharacterChangeJob(Job jobType)
        {
            string title = LocalizeKey._5.ToText(); // 알람
            string description = LocalizeKey._90039.ToText() // {JOB}으로 전직 하시겠습니까?
                .Replace("{JOB}", jobType.GetJobName());

            if (!await UI.SelectPopup(title, description))
                return false;

            // 튜토리얼이 아닐 경우에만
            if (Entity.IsDie && !Tutorial.isInProgress)
            {
                UI.ShowToastPopup(LocalizeKey._90049.ToText()); // 전직을 할 수 있는 상태가 아닙니다.
                return false;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)jobType); // 1. 전직하려는 직업 ID

            var response = await Protocol.CHAR_CHANGE_JOB.SendAsync(sfs);
            if (response.isSuccess)
            {
                // 1. 직업
                SetJob(response.GetByte("1").ToEnum<Job>());

                // 2. (2번 제거)

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                // 3. 동료 슬롯 정보
                if (response.ContainsKey("3"))
                {
                    var agentSlotInfoPackets = response.GetPacketArray<AgentSlotInfoPacket>("3");
                    Notify(agentSlotInfoPackets);
                }

                // 4. 캐릭터 스킬 정보
                CharacterSkillData[] skills = response.GetPacketArray<CharacterSkillData>("4");
                Notify(skills);

                //// 5. 캐릭터 스킬 슬롯 정보
                //CharacterSkillSlotData[] skillSlots = response.GetPacketArray<CharacterSkillSlotData>("5");

                int jobGrade = GetJobData().grade;

                switch (jobGrade)
                {
                    case 1:
                        Analytics.TrackEvent(TrackType.FirstJob);
                        break;

                    case 2:
                        Analytics.TrackEvent(TrackType.SecondJob);
                        break;

                    case 3:
                        Analytics.TrackEvent(TrackType.ThirdJob);
                        break;
                }

                Quest.QuestProgress(QuestType.JOB_DEGREE, conditionValue: jobGrade); // 해당 차수 전직 도달

                OnChangedJob?.Invoke(false); // 전직 완료 이벤트 호출
            }
            else
            {
                response.ShowResultCode();
            }

            return response.isSuccess;
        }

        /// <summary>
        /// 직업 변경
        /// </summary>
        public async Task<bool> RequestJobChangeTicket(Job jobType)
        {
            string title = LocalizeKey._5.ToText(); // 알람
            string description = LocalizeKey._22005.ToText() // {Name}으로 직업 변경하시겠습니까?
                .Replace(ReplaceKey.NAME, jobType.GetJobName());

            if (!await UI.SelectPopup(title, description))
                return false;

            //// 튜토리얼이 아닐 경우에만
            //if (Entity.IsDie && !Tutorial.isInProgress)
            //{
            //    UI.ShowToastPopup(LocalizeKey._22006.ToText()); // 직업 변경할 수 있는 상태가 아닙니다.
            //    return false;
            //}

            var sfs = Protocol.NewInstance();
            sfs.PutByte("1", (byte)jobType); // 1. 전직하려는 직업 ID
            var response = await Protocol.REQUEST_JOB_CHANGE_TICKET.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            // cud. 캐릭터 업데이트 데이터
            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;

            // 1. 캐릭터 스킬 정보
            CharacterSkillData[] skills = response.GetPacketArray<CharacterSkillData>("1");

            // 2. 캐릭터 스킬 슬롯 정보 (기존 슬롯정보에서 Skill 만 0 처리)
            List<SkillModel.ISlotValue> list = Entity.Skill.skillSlotList;
            CharacterSkillSlotData[] skillSlots = new CharacterSkillSlotData[list.Count];
            for (int i = 0; i < skillSlots.Length; i++)
            {
                skillSlots[i] = new CharacterSkillSlotData();
                skillSlots[i].Initialize(list[i].SlotNo, 0L, list[i].SlotIndex, list[i].IsAutoSkill);
            }

            Entity.Skill.ResetSkill(skills, skillSlots, charUpdateData);

            // 직업
            SetJob(jobType);
            OnChangedJob?.Invoke(false); // 전직 완료 이벤트 호출
            return true;
        }

        /// <summary>
        /// 캐릭터 전승
        /// </summary>
        public async Task RequestCharacterRebirth()
        {
            Response response = await Protocol.CHAR_REBIRTH.SendAsync();
            if (response.isSuccess)
            {
                ++rebirthCount;

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                OnRebirth?.Invoke();

                // 퀘스트 처리
                Quest.QuestProgress(QuestType.CHARACTER_RETURN); // 전승 횟수
            }
            else
            {
                response.ShowResultCode();
            }
        }

        // TODO: Initialize로 이름 설정할 수 있도록 ..
        public void SetName(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// 캐릭터 이름 변경
        /// </summary>
        public async Task RequestChangeName(string name)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", name); // 1. 변경하고자 하는 캐릭터 이름

            var response = await Protocol.REQUEST_CHANGE_NAME.SendAsync(sfs);
            if (response.isSuccess)
            {
                this.name = name;
                OnChangedName?.Invoke(name);

                // 1. 캐릭터 변경 횟수
                nameChangeCount = response.GetShort("1");

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 캐릭터 선택화면으로 이동하기 전 서버로 호출할 것
        /// </summary>
        public async Task RequestGotoCharList()
        {
            await Protocol.REQUEST_GOTO_CHAR_LIST.SendAsync();
        }

        /// <summary>
        /// 게임팟 유저 로그 전송
        /// </summary>
        public void SendLogCharacter()
        {
            GamePotSendLogCharacter characterLog = new GamePotSendLogCharacter();
            characterLog.put(GamePotSendLogCharacter.NAME, Name);
            characterLog.put(GamePotSendLogCharacter.PLAYER_ID, CidHex);
            characterLog.put(GamePotSendLogCharacter.LEVEL, JobLevel.ToString());
            characterLog.put(GamePotSendLogCharacter.SERVER_ID, connectionManager.GetSelectServerGroupId().ToString());
            gamePotImpl.SendLogCharacter(characterLog);
        }

        private string GetName()
        {
            if (Entity.type == UnitEntityType.Player)
            {
                string serverPosition = connectionManager.GetServerPosition();
                if (!serverPosition.Equals("Real", StringComparison.OrdinalIgnoreCase))
                    return $"{name}({serverPosition})";
            }

            return name;
        }

        public string GetAgentIconName(int id, AgentIconType agentIconType)
        {
            var data = agentDataRepo.Get(id);
            if (data == null)
                return string.Empty;

            return data.GetIconName(agentIconType);
        }

        /// <summary>
        /// 프로필 (원형)
        /// </summary>
        public string GetThumbnailName()
        {
            if (ProfileId > 0)
            {
                ProfileData profileData = profileDataRepo.Get(ProfileId);

                if (profileData != null)
                    return profileData.ThumbnailName;
            }

            return Job.GetThumbnailName(Gender);
        }

        /// <summary>
        /// 프로필 (네모)
        /// </summary>
        public string GetProfileName()
        {
            if (ProfileId > 0)
            {
                ProfileData profileData = profileDataRepo.Get(ProfileId);

                if (profileData != null)
                    return profileData.ProfileName;
            }

            return Job.GetJobProfile(Gender);
        }

        /// <summary>
        /// 프로필 (육각)
        /// </summary>
        public string GetAgentProfileName()
        {
            if (ProfileId > 0)
            {
                ProfileData profileData = profileDataRepo.Get(ProfileId);

                if (profileData != null)
                    return profileData.AgentProfileName;
            }

            return Job.GetAgentName(Gender);
        }

        #region 타임슈트

        internal void Initialize(TimeSuitPacket[] packets)
        {
            for (int i = 0; i < packets.Length; i++)
            {
                TimeSuitPacket packet = packets[i];
                ShareForceType type = packet.type.ToEnum<ShareForceType>();
                Debug.Log($"[쉐어포스] type={type}, level={packet.level}");
                SetShareForceLevel(type, packet.level);
            }
        }

        internal void InitializeShareForceStatus(string text)
        {
            shareForceStatusLevelDic.Clear();

            if (string.IsNullOrEmpty(text))
                return;

            string[] statusResults = StringUtils.Split(text, StringUtils.SplitType.Comma);
            if (statusResults == null)
                return;

            for (int i = 0; i < statusResults.Length; i++)
            {
                string temp = statusResults[i];
                string[] valueResults = StringUtils.Split(temp, StringUtils.SplitType.Clone);
                if (valueResults == null || valueResults.Length != 2)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Length 에러: {text} -> [{i}] {temp}");
#endif
                    continue;
                }

                if (!StringUtils.IsDigit(valueResults[0]) || !StringUtils.IsDigit(valueResults[1]))
                {
#if UNITY_EDITOR
                    Debug.LogError($"Digit 에러: {text} -> [{i}] {temp}");
#endif
                    continue;
                }

                // 쉐어 포스 스탯 추가
                AddShareForceStatus(int.Parse(valueResults[0]), int.Parse(valueResults[1]));
            }

            OnUpdateShareForceStatus?.Invoke();
        }

        public void SetShareForceLevel(ShareForceType type, int level)
        {
            if (HasShareForce(type))
            {
                shareForceDic[type] = level;
            }
            else
            {
                shareForceDic.Add(type, level);
            }
        }

        /// <summary>
        /// 쉐어포스 레벨
        /// </summary>
        public int GetShareForceLevel(ShareForceType type)
        {
            if (HasShareForce(type))
                return shareForceDic[type];

            return 0;
        }

        /// <summary>
        /// 쉐어포스 특정 파트 보유 여부
        /// </summary>
        public bool HasShareForce(ShareForceType type)
        {
            return shareForceDic.ContainsKey(type);
        }

        /// <summary>
        /// 쉐어포스 보유 여부
        /// </summary>
        public bool HasShareForce()
        {
            foreach (ShareForceType type in Enum.GetValues(typeof(ShareForceType)))
            {
                if (!HasShareForce(type))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 쉐어 포스 반환
        /// </summary>
        public int GetShareForce()
        {
            int force = 0;
            foreach (var level in shareForceDic.Values.OrEmptyIfNull())
            {
                if (level == 0)
                    continue;

                force += BasisType.TIME_SUIT_LEVEL_INC.GetInt(level);
            }

            // 사용한 포인트 만큼 빼준다.
            force -= GetUsedShareForceStatusPoint();

            return force;
        }

        /// <summary>
        /// 쉐어 포스 스탯 초기화 가능 여부
        /// </summary>
        public bool CanGetShareForceStatusReset()
        {
            return shareForceStatusLevelDic.Count > 0;
        }

        /// <summary>
        /// 쉐어 포스 스탯 레벨 반환
        /// </summary>
        public int GetShareForceStatusLevel(int group)
        {
            return shareForceStatusLevelDic.ContainsKey(group) ? shareForceStatusLevelDic[group] : 0;
        }

        /// <summary>
        /// 쉐어 포스 스탯 업그레이드 가능 여부
        /// </summary>
        public bool CanShareForceStatusUpgrade()
        {
            int point = GetShareForce();
            DataGroup<ShareStatBuildUpData>[] data = shareStatBuildUpDataRepo.GetData();
            foreach (var item in data)
            {
                ShareStatBuildUpData last = item.Last;
                if (last == null)
                    continue;

                int level = GetShareForceStatusLevel(last.group);
                int maxLevel = last.stat_lv;

                // 최대 레벨 상태
                if (level == maxLevel)
                    continue;

                // 다음 레벨
                ShareStatBuildUpData next = shareStatBuildUpDataRepo.Get(last.group, level + 1);
                if (point >= next.GetNeedPoint())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 쉐어포스 스탯 효과 반환
        /// </summary>
        public IEnumerable<ShareStatBuildUpData> GetShareForceData()
        {
            foreach (var item in shareForceStatusLevelDic)
            {
                yield return shareStatBuildUpDataRepo.Get(item.Key, item.Value);
            }
        }

        /// <summary>
        /// [쉐어포스] 쉐어포스 강화 요청
        /// </summary>
        public async Task RequestShareForceLevelUp(ShareForceType type)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", type.ToIntValue());

            Response response = await Protocol.REQUEST_TP_COSTUME_LEVELUP.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            SetShareForceLevel(type, GetShareForceLevel(type) + 1);

            OnShareForceLevelUp?.Invoke();
        }

        #endregion

        /// <summary>
        /// 프로필 변경
        /// </summary>
        public async Task RequestChangeProfile(int profileId)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", profileId);
            Response response = await Protocol.REQUEST_PROFILE_CHANGE.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            ProfileId = profileId;
            OnUpdateProfile?.Invoke();
        }

        /// <summary>
        /// 쉐어포스 스탯 강화
        /// </summary>
        public async Task RequestShareStatBuildUp(int group, int level)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", group);
            sfs.PutInt("2", level);
            Response response = await Protocol.REQUEST_SHARE_STAT_BUILD_UP.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            AddShareForceStatus(group, level);
            OnUpdateShareForceStatus?.Invoke();
        }

        /// <summary>
        /// 쉐어포스 스탯 초기화
        /// </summary>
        public async Task RequestShareStatReset()
        {
            Response response = await Protocol.REQUEST_SHARE_STAT_RESET.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            shareForceStatusLevelDic.Clear();
            OnUpdateShareForceStatus?.Invoke();
        }

        /// <summary>
        /// 쉐어 포스 스탯 강화
        /// </summary>
        private void AddShareForceStatus(int group, int level)
        {
            if (!shareForceStatusLevelDic.ContainsKey(group))
            {
                shareForceStatusLevelDic.Add(group, level);
            }
            else
            {
                shareForceStatusLevelDic[group] = level;
            }
        }

        /// <summary>
        /// 쉐어 포스 스탯 강화로 사용한 포인트 반환
        /// </summary>
        private int GetUsedShareForceStatusPoint()
        {
            int usedStatusPoint = 0;

            foreach (var item in shareForceStatusLevelDic)
            {
                ShareStatBuildUpData data = shareStatBuildUpDataRepo.Get(item.Key, item.Value);
                if (data == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"ShareStatBuildUpData 존재하지 않음: group = {item.Key}, level = {item.Value}");
#endif
                    continue;
                }

                usedStatusPoint += data.need_shareforce;
            }

            return usedStatusPoint;
        }

        public IEnumerable<IShareForceLevelValue> GetShareForceLevels()
        {
            foreach (var item in shareForceStatusLevelDic)
            {
                yield return new ShareForceLevelValue(item.Key, item.Value);
            }
        }

        private class ShareForceLevelValue : IShareForceLevelValue
        {
            public int Group { get; }
            public int Level { get; }

            public ShareForceLevelValue(int group, int level)
            {
                Group = group;
                Level = level;
            }
        }
    }
}