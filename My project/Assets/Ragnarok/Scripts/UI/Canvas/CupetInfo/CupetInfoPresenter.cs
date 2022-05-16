using Ragnarok.View.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <see cref="UICupetInfo"/>
    public class CupetInfoPresenter : ViewPresenter, IEqualityComparer<SkillInfo>
    {
        public interface IView
        {
            void ShowFxLevelUpEffect();
            void Refresh();

            void SkillViewSelectSkill(int index);
            void ShowSkillView();
        }

        private readonly GuildModel guildModel;
        private readonly CupetListModel cupetListModel;
        private readonly SkillDataManager skillDataRepo;

        IView view;
        CupetEntity cupetEntity;
        public CupetEntity Cupet => cupetEntity;
        public CupetModel CPModel => cupetEntity.Cupet;

        private readonly Dictionary<SkillInfo, SkillGroupInfo> skillGropuInfoDic;

        /// <summary>
        /// 큐펫 SkillInfo container
        /// </summary>
        private readonly Dictionary<int, CupetSkillInfo> cupetSkillInfoDic;

        public event System.Action OnUpdateCupetList
        {
            add { cupetListModel.OnUpdateCupetList += value; }
            remove { cupetListModel.OnUpdateCupetList -= value; }
        }

        public event System.Action OnEvolution
        {
            add { cupetListModel.OnEvolution += value; }
            remove { cupetListModel.OnEvolution -= value; }
        }

        public CupetInfoPresenter(IView view)
        {
            this.view = view;

            guildModel = Entity.player.Guild;
            cupetListModel = Entity.player.CupetList;
            skillDataRepo = SkillDataManager.Instance;
            skillGropuInfoDic = new Dictionary<SkillInfo, SkillGroupInfo>(this);
            cupetSkillInfoDic = new Dictionary<int, CupetSkillInfo>(IntEqualityComparer.Default);
        }

        public void SetData(CupetEntity cupetEntity)
        {
            RemoveInfoEvent();
            this.cupetEntity = cupetEntity;
            AddInfoEvent();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
            RemoveInfoEvent();
        }

        private void AddInfoEvent()
        {
            if (cupetEntity != null)
                Cupet.OnUpdateEvent += view.Refresh;
        }

        public void RemoveInfoEvent()
        {
            if (cupetEntity != null)
            {
                cupetEntity.OnUpdateEvent -= view.Refresh;
                cupetEntity = null;
            }
        }

        /// <summary>
        /// 현재 큐펫의 CupetSkillInfo를 반환.
        /// </summary>
        /// <returns></returns>
        public CupetSkillInfo GetCupetSkillInfo(int cupetID = 0)
        {
            if (cupetID == 0)
            {
                cupetID = CPModel.CupetID;
            }

            if (!cupetSkillInfoDic.ContainsKey(cupetID))
            {
                CupetSkillInfo cupetSkillInfo = new CupetSkillInfo(CPModel.CupetID);
                cupetSkillInfoDic[CPModel.CupetID] = cupetSkillInfo;
            }

            return cupetSkillInfoDic[CPModel.CupetID];
        }

        /// <summary>
        /// 현재 큐펫이 최대 랭크인가
        /// </summary>
        public bool IsMaxRank()
        {
            if (IsInvalid())
            {
#if UNITY_EDITOR
                Debug.LogError("Invalid Cupet");
#endif
                return default;
            }

            return CPModel.IsMaxRank();
        }

        /// <summary>
        /// 현재 큐펫을 보유하고있는가
        /// </summary>
        /// <returns></returns>
        public bool IsInPossesion()
        {
            if (IsInvalid())
            {
#if UNITY_EDITOR
                Debug.LogError("Invalid Cupet");
#endif
                return default;
            }

            return CPModel.IsInPossession;
        }

        /// <summary>
        /// 현재 큐펫의 스테이터스 반환
        /// </summary>
        /// <returns></returns>
        public BattleStatusInfo GetCurrentCupetStatus()
        {
            return Cupet.battleStatusInfo;
        }

        /// <summary>
        /// 다음 랭크의 큐펫 스테이터스를 반환
        /// </summary>
        /// <returns></returns>
        public BattleStatusInfo GetNextRankCupetStatus()
        {
            if (CPModel.IsMaxRank())
                return null;

            return GetDummyCupetStatus(CPModel.CupetID, CPModel.Rank + 1, CPModel.Level);
        }

        /// <summary>
        /// 1랭크 1레벨의 큐펫 스테이터스를 반환
        /// </summary>
        /// <returns></returns>
        public BattleStatusInfo GetDefaultCupetStatus()
        {
            return GetDummyCupetStatus(CPModel.CupetID, rank: 1, level: 1);
        }

        /// <summary>
        /// 평타 사거리 반환
        /// </summary>
        public int GetBasicAttackRange()
        {
            return Cupet.battleSkillInfo.basicActiveSkill.SkillRange;
        }

        /// <summary>
        /// 현재 큐펫의 스프라이트명 반환
        /// </summary>
        /// <returns></returns>
        public string GetCupetIconName()
        {
            return CPModel.ThumbnailName;
        }

        /// <summary>
        /// 현재 큐펫의 몬스터조각 수 반환
        /// </summary>
        public int GetCurrentMonsterPieceCount()
        {
            return CPModel.Count;
        }

        /// <summary>
        /// 진화에 필요한 몬스터조각 수 반환
        /// </summary>
        public int GetNeedMonsterPieceCount()
        {
            return CPModel.GetNeedEvolutionPieceCount();
        }

        /// <summary>
        /// 진화에 필요한 제니 반환
        /// </summary>
        public int GetNeedEvolutionPrice()
        {
            return CPModel.GetNeedEvolutionPrice();
        }

        /// <summary>
        /// 현재 큐펫의 ID 반환
        /// </summary>
        public int GetCurrentCupetID()
        {
            return CPModel.CupetID;
        }

        /// <summary>
        /// 현재 큐펫의 랭크 반환
        /// </summary>
        public int GetCurrentCupetRank()
        {
            return CPModel.Rank;
        }

        /// <summary>
        /// 현재 큐펫의 레벨 반환
        /// </summary>
        public int GetCurrentCupetLevel()
        {
            return CPModel.Level;
        }

        /// <summary>
        /// 큐펫 소환 가능 여부
        /// </summary>
        public bool IsSummon()
        {
            // 보유
            if (CPModel.IsInPossession)
                return false;

            // 일반 길드원은 큐펫 소환 불가
            if (guildModel.GuildPosition == GuildPosition.Member || guildModel.GuildPosition == GuildPosition.None)
                return false;

            // 소환에 필요한 조각 부족
            if (CPModel.Count < CPModel.GetNeedSummonPieceCount())
                return false;

            return true;
        }

        /// <summary>
        /// 큐펫 진화 가능 여부
        /// </summary>
        public bool IsEvolution()
        {
            // 미보유
            if (!CPModel.IsInPossession)
                return false;

            // 최대 랭크 도달
            if (CPModel.IsMaxRank())
                return false;

            // 일반 길드원은 큐펫 진화 불가
            if (guildModel.GuildPosition == GuildPosition.Member || guildModel.GuildPosition == GuildPosition.None)
                return false;

            // 이미 최대 등급
            if (CPModel.IsMaxRank())
                return false;

            // 진화에 필요한 조각 부족
            if (CPModel.Count < CPModel.GetNeedEvolutionPieceCount())
                return false;

            // 제니 부족
            if (!CoinType.Zeny.Check(CPModel.GetNeedEvolutionPrice(), isShowPopup: false))
                return false;

            return true;
        }

        /// <summary>
        /// 특정 스킬 정보
        /// </summary>
        public ISkillViewInfo GetSkillInfo(int skillIndex)
        {
            if (IsInvalid())
                return null;

            CupetSkillInfo cupetSkillInfo = GetCupetSkillInfo();

            List<SkillInfo> skillList = cupetSkillInfo.GetCupetSkillList(GetCurrentCupetRank(), isPreview: true); //CPModel.GetValidSkillList();
            if (skillList == null || skillList.Count <= skillIndex)
                return null;

            return GetSkillGroupInfo(skillList[skillIndex], skillIndex: skillIndex);
        }

        /// <summary>
        /// [정보 뷰] 스킬 뷰로 이동하고 해당 스킬 선택
        /// </summary>
        public void InfoViewSelectSkill(int index)
        {
            view.ShowSkillView();
            SkillViewSelectSkill(index);
        }

        /// <summary>
        /// [스킬 뷰] 해당 스킬 선택
        /// </summary>
        public void SkillViewSelectSkill(int index)
        {
            view.SkillViewSelectSkill(index);
        }

        public bool IsInvalid()
        {
            return cupetEntity == null;
        }

        /// <summary>
        /// 큐펫 진화
        /// </summary>
        public void RequestCupetEvolution()
        {
            cupetListModel.RequestCupetEvolution(cupetEntity.Cupet.CupetID).WrapNetworkErrors();
            view.Refresh();
            view.ShowFxLevelUpEffect();
        }

        /// <summary>
        /// 큐펫 소환
        /// </summary>
        public void ReqeustCupetSummon()
        {
            cupetListModel.RequestSummonCupet(cupetEntity.Cupet.CupetID).WrapNetworkErrors();
        }

        /// <summary>
        /// 큐펫 레벨업 버튼 클릭 이벤트
        /// </summary>
        public void OnClickedBtnLevelUp()
        {
            // 큐펫 현재등급의 최대 레벨 도달 상태
            if (CPModel.IsMaxLevel())
            {
                UI.ShowToastPopup("큐펫 진화 시 레벨업이 가능합니다.");
                return;
            }

            // 큐펫 레벨업 재료 선택 팝업
            UI.Show<UICupetExpMaterialSelect>().SetData(CPModel.CupetID);
        }

        /// <summary>
        /// 큐펫 레벨업 버튼 표시 여부
        /// </summary>
        public bool IsLevelUpButton()
        {
            // 미보유
            if (!CPModel.IsInPossession)
                return false;

            // 최대 랭크에 최대 레벨 도달
            if (CPModel.IsMaxRank() && CPModel.IsMaxLevel())
                return false;

            return true;
        }

        /// <summary>
        /// 해당 스펙의 큐펫 엔터티를 반환
        /// </summary>
        private CupetEntity GetDummyCupet(int cupetID, int rank, int level)
        {
            return CupetEntity.Factory.CreateDummyCupet(cupetID, rank, level);
        }

        /// <summary>
        /// 해당 스펙의 큐펫 스테이터스를 반환
        /// </summary>
        private BattleStatusInfo GetDummyCupetStatus(int cupetID, int rank, int level)
        {
            CupetEntity cupetEntity = GetDummyCupet(cupetID, rank, level);
            return cupetEntity.battleStatusInfo;
        }


        private class SkillGroupInfo : ISkillViewInfo
        {
            private readonly SkillInfo skillInfo;
            private readonly SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl;
            private readonly int cupetRank;
            private readonly int skillIndex;
            private readonly CupetSkillInfo cupetSkillInfo; // 해당 큐펫의 스킬 테이블

            public SkillGroupInfo(SkillInfo skillInfo, int cupetRank, int skillIndex, CupetSkillInfo cupetSkillInfo, SkillDataManager.ISkillDataRepoImpl skillDataManagerImpl)
            {
                this.skillInfo = skillInfo;
                this.cupetRank = cupetRank;
                this.skillIndex = skillIndex;
                this.cupetSkillInfo = cupetSkillInfo;
                this.skillDataManagerImpl = skillDataManagerImpl;
            }

            public int GetSkillId()
            {
                return skillInfo.SkillId;
            }

            public int GetSkillLevel()
            {
                return cupetSkillInfo.GetSkillLevel(skillIndex, cupetRank);
            }

            public bool HasSkill(int level)
            {
                return cupetSkillInfo.HasSkill(skillIndex, cupetRank);
            }

            public SkillData.ISkillData GetSkillData(int level)
            {
                return skillDataManagerImpl.Get(skillInfo.SkillId, level: 1);
            }

            public int GetSkillLevelNeedPoint(int plusLevel)
            {
                return default;
            }

            // 큐펫 스킬 전용
            public int GetSkillLevelUpNeedRank()
            {
                return cupetSkillInfo.GetNextLevelRank(skillIndex, cupetRank);
            }
        }

        /// <summary>
        /// 스킬 정보 뷰에 들어갈 수 있는 형태로 스킬정보 반환
        /// </summary>
        private SkillGroupInfo GetSkillGroupInfo(SkillInfo skillInfo, int skillIndex)
        {
            if (skillInfo == null)
                return null;

            if (!skillGropuInfoDic.ContainsKey(skillInfo))
            {
                CupetSkillInfo cupetSkillInfo = GetCupetSkillInfo();
                skillGropuInfoDic.Add(skillInfo, new SkillGroupInfo(skillInfo, CPModel.Rank, skillIndex, cupetSkillInfo, skillDataRepo));
            }

            return skillGropuInfoDic[skillInfo];
        }

        bool IEqualityComparer<SkillInfo>.Equals(SkillInfo x, SkillInfo y)
        {
            return (x == y);
        }

        int IEqualityComparer<SkillInfo>.GetHashCode(SkillInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}