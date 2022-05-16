using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildBattleReady"/>
    /// </summary>
    public class GuildBattleReadyPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GuildModel guildModel;
        private readonly CupetListModel cupetListModel;
        private readonly GoodsModel goodsModel;

        // <!-- Repositories --!>
        private readonly SkillDataManager skillDataRepo;

        // <!-- Data --!>
        private readonly Buffer<int> leftCupetIds, rightCupetIds;
        private readonly Buffer<CupetModel> cupetBuffer;
        private readonly Buffer<GuildBattleBuffSelectElement.IInput> buffElements;

        // <!-- Event --!>
        public event System.Action OnUpdateGuildBattleInfo;

        public event System.Action OnUpdateGuildBattleRequest
        {
            add { guildModel.OnUpdateGuildBattleRequest += value; }
            remove { guildModel.OnUpdateGuildBattleRequest -= value; }
        }

        public event System.Action OnUpdateCupetList
        {
            add { cupetListModel.OnUpdateCupetList += value; }
            remove { cupetListModel.OnUpdateCupetList -= value; }
        }

        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public event System.Action OnUpdateGuildBattleBuff
        {
            add { guildModel.OnUpdateGuildBattleBuff += value; }
            remove { guildModel.OnUpdateGuildBattleBuff -= value; }
        }

        public GuildBattleReadyPresenter()
        {
            guildModel = Entity.player.Guild;
            cupetListModel = Entity.player.CupetList;
            goodsModel = Entity.player.Goods;
            skillDataRepo = SkillDataManager.Instance;
            leftCupetIds = new Buffer<int>();
            rightCupetIds = new Buffer<int>();
            cupetBuffer = new Buffer<CupetModel>();
            buffElements = new Buffer<GuildBattleBuffSelectElement.IInput>();
            Initialize();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        private void Initialize()
        {
            int[] leftTurretCupets = guildModel.GetLeftTurretCupets();
            int[] rightTurretCupets = guildModel.GetRightTurretCupets();

            if (leftTurretCupets != null)
                leftCupetIds.AddRange(leftTurretCupets);

            if (rightTurretCupets != null)
                rightCupetIds.AddRange(rightTurretCupets);
        }

        /// <summary>
        /// 큐펫 정보 목록 요청
        /// </summary>
        public void RequestGuildCupetInfo()
        {
            cupetListModel.RequestGuildCupetInfo().WrapNetworkErrors();
        }

        public void RequestGuildBattleInfo()
        {
            OnUpdateGuildBattleInfo?.Invoke();
        }

        /// <summary>
        /// 길드전 신청
        /// </summary>
        public void ReqeustGuildBattle()
        {
            guildModel.RequestGuildBattleEntry(leftCupetIds.ToArray(), rightCupetIds.ToArray()).WrapNetworkErrors();
        }

        public CupetModel[] GetLeftCupetArray()
        {
            foreach (var item in leftCupetIds)
            {
                cupetBuffer.Add(cupetListModel.Get(item).Cupet);
            }

            return cupetBuffer.GetBuffer(isAutoRelease: true);
        }

        public CupetModel[] GetRightCupetArray()
        {
            foreach (var item in rightCupetIds)
            {
                cupetBuffer.Add(cupetListModel.Get(item).Cupet);
            }

            return cupetBuffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 길드전 신청 남은시간
        /// </summary>
        public RemainTime GetRemainTimeGuildBattleRequest()
        {
            return guildModel.GuildBattleSeasonRemainTime;
        }

        public void OnSelectLeftTurretCupet()
        {
            UI.Show<UICupetSelect>().Set(leftCupetIds.ToArray(), OnUpateLeftSelectIds, rightCupetIds.ToArray());
        }

        public void OnSelectRightTurretCupet()
        {
            UI.Show<UICupetSelect>().Set(rightCupetIds.ToArray(), OnUpdateRightSelectIds, leftCupetIds.ToArray());
        }

        private void OnUpateLeftSelectIds(int[] ids)
        {
            leftCupetIds.Release();
            leftCupetIds.AddRange(ids);

            OnUpdateGuildBattleInfo?.Invoke();
        }

        private void OnUpdateRightSelectIds(int[] ids)
        {
            rightCupetIds.Release();
            rightCupetIds.AddRange(ids);

            OnUpdateGuildBattleInfo?.Invoke();
        }

        /// <summary>
        /// 길드전 버프 목록 요청
        /// </summary>
        public void RequestGuildBattleBuffInfo()
        {
            guildModel.RequestGuildBattleBuffInfo().WrapNetworkErrors();
        }

        /// <summary>
        /// 길드전 신청 버튼 상태
        /// </summary>
        public bool GetBtnRequestState()
        {
            return guildModel.GuildPosition == GuildPosition.Master || guildModel.GuildPosition == GuildPosition.PartMaster;
        }

        /// <summary>
        /// 길드전 신청 상태
        /// </summary>
        public bool IsReqeustState()
        {
            return guildModel.IsGuildBattleRequest;
        }

        public GuildBattleBuffSelectElement.IInput[] GetBuffArray()
        {
            int skillId1 = BasisGuildWarInfo.BuffSkill1.GetInt();
            int skillId2 = BasisGuildWarInfo.BuffSkill2.GetInt();
            int skillId3 = BasisGuildWarInfo.BuffSkill3.GetInt();
            int skillId4 = BasisGuildWarInfo.BuffSkill4.GetInt();
            int skillId5 = BasisGuildWarInfo.BuffSkill5.GetInt();
            buffElements.Add(new GuildBattleBuffElementInfo(skillDataRepo, skillId1, guildModel.GetGuildBuffExp(skillId1)));
            buffElements.Add(new GuildBattleBuffElementInfo(skillDataRepo, skillId2, guildModel.GetGuildBuffExp(skillId2)));
            buffElements.Add(new GuildBattleBuffElementInfo(skillDataRepo, skillId3, guildModel.GetGuildBuffExp(skillId3)));
            buffElements.Add(new GuildBattleBuffElementInfo(skillDataRepo, skillId4, guildModel.GetGuildBuffExp(skillId4)));
            buffElements.Add(new GuildBattleBuffElementInfo(skillDataRepo, skillId5, guildModel.GetGuildBuffExp(skillId5)));
            return buffElements.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// 길드전 버프 스킬 재료 선택
        /// </summary>
        public void ShowBuffMaterialSelect(int skillId)
        {
            UI.Show<UIGuildBattleBuffMaterialSelect>().SetData(skillId);
        }

        private class GuildBattleBuffElementInfo : GuildBattleBuffSelectElement.IInput
        {
            private readonly SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl;
            private int skillLevel;
            private int skillExp;
            private BattleOption battleOption;
            private int skillNameId;
            private int maxLevel => BasisGuildWarInfo.BuffSkillMaxLevel.GetInt();
            private int levelUpExp => BasisGuildWarInfo.BuffNeedLevelUpExp.GetInt();

            public int SkillId { get; private set; }
            public UISkillInfo.IInfo Skill => GetSkill();
            public string SkillName => GetSkillName();
            public bool IsExpMax => GetIsMaxExp();
            public string OptionTitle => battleOption.GetTitleText();
            public string OptionValue => GetOptionValue();
            public float ExpProgressValue => GetProgressValue();
            public string ExpProgressText => GetProgressText();

            public GuildBattleBuffElementInfo(SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl, int skillId, int skillExp)
            {
                this.skillDataManagerImpl = skillDataManagerImpl;
                this.SkillId = skillId;
                this.skillExp = skillExp;
                UpdateExp(skillExp);
            }

            private void UpdateExp(int skillExp)
            {
                this.skillExp = skillExp;
                skillLevel = skillExp / levelUpExp;
                SkillData skillData = skillDataManagerImpl.Get(SkillId, Mathf.Max(1, skillLevel));

                if (skillData == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"{nameof(skillExp)} = {skillExp}, {nameof(levelUpExp)} = {levelUpExp}, {nameof(SkillId)} = {SkillId}, {nameof(skillLevel)} = {skillLevel}");
#endif
                    return;
                }

                battleOption = new BattleOption(skillData.battle_option_type_1, skillData.value1_b1, skillData.value2_b1);
                skillNameId = skillData.name_id;
            }

            private UISkillInfo.IInfo GetSkill()
            {
                return skillDataManagerImpl.Get(SkillId, Mathf.Max(1, skillLevel));
            }

            private string GetOptionValue()
            {
                return skillLevel == 0 ? "0" : battleOption.GetValueText();
            }

            private string GetSkillName()
            {
                return LocalizeKey._33811.ToText() // LV.{LEVEL} {NAME}
                    .Replace(ReplaceKey.LEVEL, skillLevel)
                    .Replace(ReplaceKey.NAME, skillNameId.ToText());
            }

            private float GetProgressValue()
            {
                return MathUtils.GetProgress(skillExp % levelUpExp, levelUpExp);
            }

            private string GetProgressText()
            {
                return StringBuilderPool.Get()
                    .Append(skillExp % levelUpExp)
                    .Append("/")
                    .Append(levelUpExp)
                    .Release();
            }

            private bool GetIsMaxExp()
            {
                return skillExp >= levelUpExp * maxLevel;
            }
        }
    }
}