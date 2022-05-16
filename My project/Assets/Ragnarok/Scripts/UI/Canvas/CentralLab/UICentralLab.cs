using Ragnarok.View;
using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICentralLab : UICanvas
    {
        protected override UIType uiType => UIType.Single | UIType.Back | UIType.Hide;

        [SerializeField] TitleView titleView;
        [SerializeField] CentralLabCharacterSelectView centralLabCharacterSelectView;
        [SerializeField] CentralLabLevelSelectView centralLabLevelSelectView;

        CentralLabPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CentralLabPresenter();

            centralLabCharacterSelectView.OnSelect += OnSelectClone;
            centralLabLevelSelectView.OnSelectIndex += OnSelectLevelIndex;
            centralLabLevelSelectView.OnSelectFastClear += OnSelectFastClear;
            centralLabLevelSelectView.OnSelectEnter += presenter.StartBattle;

            presenter.OnUpdateZeny += UpdateZeny;
            presenter.OnUpdateCatCoin += UpdateCatCoin;
            presenter.OnUpdateJobLevel += UpdateJobLevel;
            presenter.OnUpdateDungeonTicket += UpdateDungeonTicket;
            presenter.OnUpdateFreeTicketCount += UpdateFreeTicketCount;
            presenter.OnStartBattleSuccess += CloseUI;

            presenter.AddEvent();

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateZeny -= UpdateZeny;
            presenter.OnUpdateCatCoin -= UpdateCatCoin;
            presenter.OnUpdateJobLevel -= UpdateJobLevel;
            presenter.OnUpdateDungeonTicket -= UpdateDungeonTicket;
            presenter.OnUpdateFreeTicketCount -= UpdateFreeTicketCount;
            presenter.OnStartBattleSuccess -= CloseUI;

            centralLabCharacterSelectView.OnSelect -= OnSelectClone;
            centralLabLevelSelectView.OnSelectIndex -= OnSelectLevelIndex;
            centralLabLevelSelectView.OnSelectFastClear -= OnSelectFastClear;
            centralLabLevelSelectView.OnSelectEnter -= presenter.StartBattle;
        }

        protected override void OnShow(IUIData data = null)
        {
            Job[] arrJob = presenter.GetCloneJobs();
            Gender gender = presenter.GetGender();
            centralLabCharacterSelectView.SetClone(arrJob, gender); // 클론 세팅
            centralLabCharacterSelectView.SelectFirstClone(); // 첫번째 클론 선택

            int clearedDataIndex = presenter.GetClearedDataIndex();
            centralLabLevelSelectView.SetMaxIndex(presenter.centralLabMaxIndex); // 인덱스 세팅
            centralLabLevelSelectView.SetClearedIndex(clearedDataIndex); // 클리어 한 인덱스 세팅
            centralLabLevelSelectView.SetMaxFreeTicketCount(presenter.maxFreeTicketCount); // 최대 무료 입장 수 세팅

            UpdateJobLevel();
            UpdateDungeonTicket();
            UpdateFreeTicketCount();
        }

        protected override void OnHide()
        {
            centralLabCharacterSelectView.ResetToggle(); // 토글 초기화 (직업 변경으로 인하여 첫번째 Job이 바뀌었을 때를 대비)
        }

        protected override void OnLocalize()
        {
            // Title 언어 세팅
            titleView.ShowTitle(LocalizeKey._58506.ToText()); // 중앙실험실
        }

        protected override void OnBack()
        {
            UIDungeon.viewType = UIDungeon.ViewType.None;
            UI.Show<UIDungeon>();
        }

        void OnSelectClone(Job job)
        {
            presenter.SelectCloneJob(job);

            int jobNameId = presenter.GetCloneJobNameId();
            int jobDescriptionId = presenter.GetCloneJobDescriptionId();
            UISkillInfoSelect.IInfo[] skills = presenter.GetCloneSkills();
            centralLabCharacterSelectView.SetSelectData(jobNameId, jobDescriptionId, skills);
        }

        void OnSelectLevelIndex(int index)
        {
            CentralLabLevelSelectView.IInput data = presenter.GetCentralLabDataByIndex(index);
            centralLabLevelSelectView.SetData(data);
        }

        /// <summary>
        /// 제니 업데이트
        /// </summary>
        private void UpdateZeny(long zeny)
        {
            titleView.ShowZeny(zeny);
        }

        /// <summary>
        /// 캣코인 업데이트
        /// </summary>
        private void UpdateCatCoin(long catCoin)
        {
            titleView.ShowCatCoin(catCoin);
        }

        private void UpdateJobLevel()
        {
            int jobLevel = presenter.GetJobLevel();
            centralLabLevelSelectView.SetCharacterJobLevel(jobLevel);
        }

        private void UpdateDungeonTicket()
        {
            int dungeonTicketCount = presenter.GetDungeonTicketCount();
            centralLabLevelSelectView.SetClearTicketCount(dungeonTicketCount);
        }

        private void UpdateFreeTicketCount()
        {
            int freeTicketCount = presenter.GetFreeTicketCount();
            centralLabLevelSelectView.SetFreeTicketCount(freeTicketCount);
        }

        private void OnSelectFastClear(int index)
        {
            presenter.RequestFastClear(index, isFree: false);
        }

        private void CloseUI()
        {
            UI.Close<UICentralLab>();
        }
    }
}