using Ragnarok.View;
using Ragnarok.View.Skill;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UISkill"/>
    /// </summary>
    public sealed class SkillPresenter : ViewPresenter
    {
        private readonly ISkillCanvas canvas;
        private readonly GoodsModel goodsModel;
        private readonly CharacterModel characterModel;
        private readonly SkillModel skillModel;
        private readonly QuestModel questModel;

        private readonly JobDataManager jobDataRepo;
        private readonly SkillDataManager skillDataRepo;
        private readonly Buffer<JobData> jobBuffer;
        private readonly int skillLevelNeedPoint;
        private readonly int needSkillInitCatCoin;
        /******************** Pool ********************/
        private readonly Dictionary<int, JobSkillInfo> jobSkillInfoDic;
        private readonly Dictionary<int, SkillGroupInfo> skillGropuInfoDic;
        private readonly SkillEquipInfo[] skillEquipInfos;

        public SkillPresenter(ISkillCanvas canvas)
        {
            this.canvas = canvas;
            goodsModel = Entity.player.Goods;
            characterModel = Entity.player.Character;
            skillModel = Entity.player.Skill;
            questModel = Entity.player.Quest;

            jobDataRepo = JobDataManager.Instance;
            skillDataRepo = SkillDataManager.Instance;
            jobBuffer = new Buffer<JobData>();
            skillLevelNeedPoint = BasisType.SKILL_LEVEL_NEED_POINT.GetInt();
            needSkillInitCatCoin = BasisType.PRICE_SKILL_INIT.GetInt();

            jobSkillInfoDic = new Dictionary<int, JobSkillInfo>(IntEqualityComparer.Default);
            skillGropuInfoDic = new Dictionary<int, SkillGroupInfo>(IntEqualityComparer.Default);
            int maxCharacterSkillSlot = BasisType.MAX_CHAR_SKILL_SLOT.GetInt();
            skillEquipInfos = new SkillEquipInfo[maxCharacterSkillSlot];

            // Initialize SkillEquipInfo
            for (int i = 0; i < skillEquipInfos.Length; i++)
            {
                skillEquipInfos[i] = new SkillEquipInfo(i, skillModel, skillDataRepo);
            }
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += canvas.UpdateZeny;
            goodsModel.OnUpdateCatCoin += canvas.UpdateCatCoin;
            characterModel.OnChangedJob += OnChangedJob;
            characterModel.OnUpdateJobLevel += canvas.UpdateJobLevel;
            skillModel.OnUpdateSkillPoint += canvas.Refresh;
            skillModel.OnUpdateSkillSlot += canvas.Refresh;
            skillModel.OnUpdateSkill += canvas.Refresh;
            skillModel.OnUpdateHasNewSkillPoint += canvas.UpdateHasNewSkillPoint;
            skillModel.OnSkillInit += RefreshWithClearJobSkills;
            skillModel.OnSkillChange += RefreshWithClearJobSkills;
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= canvas.UpdateZeny;
            goodsModel.OnUpdateCatCoin -= canvas.UpdateCatCoin;
            characterModel.OnChangedJob -= OnChangedJob;
            characterModel.OnUpdateJobLevel -= canvas.UpdateJobLevel;
            skillModel.OnUpdateSkillPoint -= canvas.Refresh;
            skillModel.OnUpdateSkillSlot -= canvas.Refresh;
            skillModel.OnUpdateSkill -= canvas.Refresh;
            skillModel.OnUpdateHasNewSkillPoint -= canvas.UpdateHasNewSkillPoint;
            skillModel.OnSkillInit -= RefreshWithClearJobSkills;
            skillModel.OnSkillChange -= RefreshWithClearJobSkills;

            jobSkillInfoDic.Clear();
            skillGropuInfoDic.Clear();
        }

        void OnChangedJob(bool isInit)
        {
            canvas.Refresh();
        }

        /// <summary>
        /// 제니 반환
        /// </summary>
        public long GetZeny()
        {
            return goodsModel.Zeny;
        }

        /// <summary>
        /// 캣코인 반환
        /// </summary>
        public long GetCatCoin()
        {
            return goodsModel.CatCoin;
        }

        /// <summary>
        /// 직업 아이콘 반환
        /// </summary>
        public string GetJobIcon()
        {
            return characterModel.Job.GetJobIcon();
        }

        /// <summary>
        /// 직업 이름 반환
        /// </summary>
        public string GetJobName()
        {
            return characterModel.Job.GetJobName();
        }

        /// <summary>
        /// 직업 레벨 반환
        /// </summary>
        public int GetJobLevel()
        {
            return characterModel.JobLevel;
        }

        /// <summary>
        /// 스킬포인트 반환
        /// </summary>
        public int GetSkillPoint()
        {
            return skillModel.SkillPoint;
        }

        /// <summary>
        /// 차수 별 스킬 목록
        /// </summary>
        public SkillListView.IInfo[] GetSkillInfos()
        {
            JobData[] arrJobData = GetArrayJobData((int)characterModel.Job);
            JobSkillInfo[] output = new JobSkillInfo[arrJobData.Length];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = GetJobSkillInfo(arrJobData[i]);
            }

            return output;
        }

        /// <summary>
        /// 특정 스킬 정보
        /// </summary>
        public ISkillViewInfo GetSkillInfo(int skillId)
        {
            return GetSkillGroupInfo(skillId);
        }

        /// <summary>
        /// 스킬 장착 정보 목록
        /// </summary>
        public UISkillPreset.IInfo[] GetSkillEquipInfos()
        {
            return skillEquipInfos;
        }

        /// <summary>
        /// 레벨업 한 스킬 존재 여부
        /// </summary>
        public bool HasLevelUpSkill()
        {
            return skillModel.HasInPossessionSkill();
        }

        /// <summary>
        /// 새로운 스킬포인트 존재 여부
        /// </summary>
        public bool HasNewSkillPoint()
        {
            return skillModel.HasNewSkillPoint;
        }

        /// <summary>
        /// 장착 가능한 스킬 존재 여부
        /// </summary>
        public bool HasEquipSkill()
        {
            return skillModel.HasEquipSkill();
        }

        /// <summary>
        /// 스킬 선택 타입 반환
        /// </summary>
        public UISkill.SkillSelectType GetSelectType(int skillId)
        {
            UISkillInfoSelect.IInfo skillData = skillDataRepo.Get(skillId, level: 1);

            if (skillData == null)
                return default;

            return GetSkillSelectType(skillData);
        }

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContent_Skill()
        {
            questModel.RemoveNewOpenContent(ContentType.Skill); // 신규 컨텐츠 플래그 제거 (스킬)
        }

        /// <summary>
        /// 새로운 스킬 포인트 존재 여부 플래그 제거
        /// </summary>
        public void RemoveHasNewSkillPoint()
        {
            skillModel.SetHasNewSkillPoint(hasNewSkillPoint: false);
        }

        /// <summary>
        /// 스킬 초기화 서버 호출
        /// </summary>
        public void RequestSkillInitialize()
        {
            RequestSkillInitializeAsync().WrapNetworkErrors();
        }

        /// <summary>
        /// 스킬 레벨업 서버 호출
        /// </summary>
        public void RequestSkillLevelUp(int skillId, int plusLevel)
        {
            RequestSkillLevelUpAsync(skillId, plusLevel).WrapNetworkErrors();
        }

        /// <summary>
        /// 스킬 슬롯 구매 서버 호출
        /// </summary>
        public void RequestBuySkillSlot()
        {
            RequestBuySkillSlotAsync().WrapNetworkErrors();
        }

        /// <summary>
        /// 스킬 슬롯 장착 서버 호출
        /// </summary>
        public void RequestEquipSkillSlot(int slotIndex, int skillId)
        {
            RequestEquipSkillSlotAsync(slotIndex, skillId).WrapNetworkErrors();
        }

        /// <summary>
        /// 스킬 슬롯 장착해제 서버 호출
        /// </summary>
        public void RequestUnequipSkillSlot(int slotIndex)
        {
            RequestUnequipSkillSlotAsync(slotIndex).WrapNetworkErrors();
        }

        /// <summary>
        /// 스킬 변경 (훔쳐 배우기, 서몬 볼)
        /// </summary>
        public void RequestChangeSkill(int skillId, int changeSkillId)
        {
            RequestChangeSkillAsync(skillId, changeSkillId).WrapNetworkErrors();
        }

        /// <summary>
        /// 직업 정보 초기화 후 Canvas Refresh
        /// </summary>
        private void RefreshWithClearJobSkills()
        {
            jobSkillInfoDic.Clear(); // 훔쳐 배운 스킬에 대한 정보를 초기화 시켜야 함
            canvas.UnSelectSkill(); // 선택한 스킬 해제
        }

        private JobData[] GetArrayJobData(int id)
        {
            JobData jobData = jobDataRepo.Get(id);
            if (jobData == null)
            {
                jobBuffer.Sort((a, b) => a.id.CompareTo(b.id));
                return jobBuffer.GetBuffer(isAutoRelease: true);
            }

            jobBuffer.Add(jobData);
            return GetArrayJobData(jobData.previous_index);
        }

        private JobSkillInfo GetJobSkillInfo(JobData jobData)
        {
            int id = jobData.id;
            if (!jobSkillInfoDic.ContainsKey(id))
            {
                JobSkillInfo.SkillInfo[] skillInfos = new JobSkillInfo.SkillInfo[JobData.MAX_SKILL_COUNT];
                for (int i = 0; i < skillInfos.Length; i++)
                {
                    int skillId = GetSkillId(jobData.GetSkillId(i));
                    if (skillId == 0)
                        continue;

                    skillInfos[i] = new JobSkillInfo.SkillInfo(skillId, skillModel, skillDataRepo);
                }

                jobSkillInfoDic.Add(id, new JobSkillInfo(jobData, skillInfos));
            }

            return jobSkillInfoDic[id];
        }

        private int GetSkillId(int skillId)
        {
            SkillData skillData = skillDataRepo.Get(skillId, level: 1);
            if (skillData == null)
                return skillId;

            UISkill.SkillSelectType selectType = GetSkillSelectType(skillData);
            if (selectType == default)
                return skillId;

            // 변경된 아이디  없음
            int changedSkillId = skillModel.GetChangedSkillId(skillId);
            if (changedSkillId == 0)
                return skillId;

            // 변경된 아이디에 해당하는 스킬 데이터 반환
            return GetSkillId(changedSkillId);
        }

        private UISkill.SkillSelectType GetSkillSelectType(UISkillInfoSelect.IInfo info)
        {
            switch (info.SkillType)
            {
                case SkillType.Plagiarism:
                case SkillType.Reproduce:
                    return UISkill.SkillSelectType.Steal;

                case SkillType.SummonBall:
                case SkillType.RuneMastery:
                    return UISkill.SkillSelectType.SummonBall;
            }

            return default;
        }

        private SkillGroupInfo GetSkillGroupInfo(int skillId)
        {
            if (skillId == 0)
                return null;

            if (!skillGropuInfoDic.ContainsKey(skillId))
                skillGropuInfoDic.Add(skillId, new SkillGroupInfo(skillId, skillModel, skillDataRepo, skillLevelNeedPoint));

            return skillGropuInfoDic[skillId];
        }

        private async Task RequestSkillInitializeAsync()
        {
            string title = LocalizeKey._39001.ToText(); // 스킬 초기화
            string description = LocalizeKey._39002.ToText(); // 스킬 초기화를 진행하시겠습니까?

            int needCatCoin = skillModel.IsFreeSkillReset ? 0 : needSkillInitCatCoin;

            if (!await UI.CostPopup(CoinType.CatCoin, needCatCoin, title, description))
                return;

            await skillModel.RequestSkillInit();
        }

        private async Task RequestSkillLevelUpAsync(int skillId, int plusLevel)
        {
            long skillNo = skillModel.GetSkillNo(skillId);

            if (skillNo == 0L)
                return;

            await skillModel.RequestSkillLevelUp(skillNo, plusLevel);
            SoundManager.Instance.PlayUISfx(Sfx.UI.ChangeCard);
        }

        private async Task RequestBuySkillSlotAsync()
        {
            await skillModel.RequestBuySkillSlot();
        }

        private async Task RequestEquipSkillSlotAsync(int slotIndex, int skillId)
        {
            SkillModel.ISlotValue slotData = skillModel.GetSlotInfo(slotIndex - 1);
            long skillNo = skillModel.GetSkillNo(skillId);
            await skillModel.RequestUpdateSkillSlot(slotData.SlotNo, skillNo);
        }

        private async Task RequestUnequipSkillSlotAsync(int slotIndex)
        {
            SkillModel.ISlotValue slotData = skillModel.GetSlotInfo(slotIndex - 1);
            await skillModel.RequestUpdateSkillSlot(slotData.SlotNo, 0L);
        }

        public int GetSkillIDInSlot(int slotIndex)
        {
            return skillModel.GetSkillId(slotIndex - 1);
        }

        /// <summary>
        /// 컨텐츠 오픈 여부
        /// </summary>
        public bool IsOpenContent(bool isShowPopup)
        {
            return characterModel.IsCheckJobGrade(Constants.OpenCondition.NEED_SKILL_JOB_GRADE, isShowPopup);
        }

        private async Task RequestChangeSkillAsync(int skillId, int changeSkillId)
        {
            long skillNo = skillModel.GetSkillNo(skillId);

            if (skillNo == 0L)
                return;

            SkillData skillData = skillDataRepo.Get(skillId, level: 1);
            if (skillData == null)
                return;

            SkillData changeSkillData = skillDataRepo.Get(changeSkillId, level: 1);
            if (changeSkillData == null)
                return;

            string title = StringBuilderPool.Get()
                .Append(LocalizeKey._39100.ToText()) // 스킬 선택
                .Append(" (").Append(skillData.name_id.ToText()).Append(")")
                .Release();

            string description = StringBuilderPool.Get()
                .Append("[").Append(changeSkillData.name_id.ToText()).Append("]")
                .AppendLine()
                .AppendLine()
                .Append(LocalizeKey._39105.ToText()) // 해당 스킬을 선택하시겠습니까?
                .Release();

            if (!await UI.SelectPopup(title, description))
                return;

            await skillModel.RequestChangeSkill(skillNo, changeSkillId);

            UI.Close<UIStealSkillSelect>();
            UI.Close<UISkillSelect>();
        }

        private class JobSkillInfo : SkillListView.IInfo
        {
            public class SkillInfo : UISkillInfoToggle.IInfo
            {
                private readonly int skillId;
                private readonly SkillModel.ISkillModelImpl skillModelImpl;
                private readonly SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl;

                public int SkillId => skillId;
                public SkillType SkillType => GetSkillType();
                public string SkillIcon => GetSkillIcon();

                public SkillInfo(int skillId, SkillModel.ISkillModelImpl skillModelImpl, SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl)
                {
                    this.skillId = skillId;
                    this.skillModelImpl = skillModelImpl;
                    this.skillDataManagerImpl = skillDataManagerImpl;
                }

                public int GetSkillLevel()
                {
                    return skillModelImpl.GetSkillLevel(skillId);
                }

                public bool IsAvailableWeapon(EquipmentClassType weaponType)
                {
                    UISkillInfo.IInfo info = skillDataManagerImpl.Get(GetBattleSkillId(), level: 1);
                    return info.IsAvailableWeapon(weaponType);
                }

                public bool IsOverrideSkill()
                {
                    return skillId != GetBattleSkillId();
                }

                private int GetBattleSkillId()
                {
                    return skillModelImpl.GetBattleSkillId(skillId);
                }

                private SkillType GetSkillType()
                {
                    UISkillInfo.IInfo info = skillDataManagerImpl.Get(GetBattleSkillId(), level: 1);
                    return info.SkillType;
                }

                private string GetSkillIcon()
                {
                    UISkillInfo.IInfo info = skillDataManagerImpl.Get(GetBattleSkillId(), level: 1);
                    return info.SkillIcon;
                }
            }

            private readonly string jobIcon;
            private readonly int jobNameId;
            private readonly SkillInfo[] skillInfos;

            public JobSkillInfo(JobData jobData, SkillInfo[] skillInfos)
            {
                jobIcon = jobData.id.ToEnum<Job>().GetJobIcon();
                jobNameId = jobData.name_id;
                this.skillInfos = skillInfos;
            }

            public string GetJobIcon()
            {
                return jobIcon;
            }

            public string GetJobName()
            {
                return jobNameId.ToText();
            }

            public UISkillInfoToggle.IInfo[] GetSkills()
            {
                return skillInfos;
            }
        }

        private class SkillGroupInfo : ISkillViewInfo
        {
            private readonly int skillId;
            private readonly SkillModel.ISkillModelImpl skillModelImpl;
            private readonly SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl;
            private readonly int skillLevelNeedPoint;

            public SkillGroupInfo(int skillId, SkillModel.ISkillModelImpl skillModelImpl, SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl, int skillLevelNeedPoint)
            {
                this.skillId = skillId;
                this.skillModelImpl = skillModelImpl;
                this.skillDataManagerImpl = skillDataManagerImpl;
                this.skillLevelNeedPoint = skillLevelNeedPoint;
            }

            public int GetSkillId()
            {
                return skillId;
            }

            public int GetSkillLevel()
            {
                return skillModelImpl.GetSkillLevel(skillId);
            }

            public bool HasSkill(int level)
            {
                return skillDataManagerImpl.Get(GetBattleSkillId(), level) != null;
            }

            public SkillData.ISkillData GetSkillData(int level)
            {
                return skillDataManagerImpl.Get(GetBattleSkillId(), level);
            }

            public int GetSkillLevelNeedPoint(int plusLevel)
            {
                return skillLevelNeedPoint * plusLevel;
            }

            // 큐펫 전용
            public int GetSkillLevelUpNeedRank()
            {
                return default;
            }

            public bool IsOverrideSkill()
            {
                return GetSkillId() != GetBattleSkillId();
            }

            private int GetBattleSkillId()
            {
                return skillModelImpl.GetBattleSkillId(skillId);
            }
        }

        private class SkillEquipInfo : UISkillPreset.IInfo
        {
            private readonly int slotRealIndex; // 0부터 시작
            private readonly SkillModel.ISkillModelImpl skillModelImpl;
            private readonly SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl;

            public UISkillInfo.IInfo SkillInfo
            {
                get
                {
                    if (!HasSkill())
                        return null;

                    int skillId = skillModelImpl.GetSkillId(slotRealIndex); // 실제 skillId
                    int battleSkillId = skillModelImpl.GetBattleSkillId(skillId); // 전투 skillId
                    int skillLevel = skillModelImpl.GetSkillLevel(skillId);
                    return skillDataManagerImpl.Get(battleSkillId, skillLevel);
                }
            }

            public SkillEquipInfo(int slotRealIndex, SkillModel.ISkillModelImpl skillModelImpl, SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl)
            {
                this.slotRealIndex = slotRealIndex;
                this.skillModelImpl = skillModelImpl;
                this.skillDataManagerImpl = skillDataManagerImpl;
            }

            public bool IsUnlock()
            {
                return slotRealIndex < skillModelImpl.SkillSlotCount;
            }

            public bool HasSkill()
            {
                int skillId = skillModelImpl.GetSkillId(slotRealIndex);
                return skillId > 0;
            }

            public bool HasEquipSkillSlot()
            {
                return skillModelImpl.HasEquipSkillSlot();
            }

            public bool IsOverrideSkill()
            {
                int skillId = skillModelImpl.GetSkillId(slotRealIndex); // 실제 skillId
                int battleSkillId = skillModelImpl.GetBattleSkillId(skillId); // 전투 skillId
                return skillId != battleSkillId;
            }
        }
    }
}