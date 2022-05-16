using UnityEngine;

namespace Ragnarok
{
    public class UICupetInfo : UICanvas, CupetInfoPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        // 타이틀 바
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;

        // 큐펫 기본 정보
        [SerializeField] UICupetDetailInfo cupetStatusInfo;
        [SerializeField] GameObject fxLevelUpEffect;
        [SerializeField] UIButtonHelper btnSummon;
        [SerializeField] UIButtonHelper btnLevelUp;

        // 탭
        [SerializeField] UITabHelper tab;
        [SerializeField] CupetInfoInfoView infoView;
        [SerializeField] CupetInfoEvolutionView evolutionView;
        [SerializeField] CupetInfoSkillView skillView;

        CupetInfoPresenter presenter;
        private UISubCanvas currentSubCanvas;

        protected override void OnInit()
        {
            presenter = new CupetInfoPresenter(this);
            infoView.Initialize(presenter);
            evolutionView.Initialize(presenter);
            skillView.Initialize(presenter);

            presenter.OnUpdateCupetList += Refresh;
            presenter.OnEvolution += OnEvolution;

            presenter.AddEvent();

            EventDelegate.Add(tab[0].OnChange, OnClickedTabInfo);
            EventDelegate.Add(tab[1].OnChange, OnClickedTabEvolution);
            EventDelegate.Add(tab[2].OnChange, OnClickedTabSkill);
            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Add(btnSummon.OnClick, presenter.ReqeustCupetSummon);
            EventDelegate.Add(btnLevelUp.OnClick, presenter.OnClickedBtnLevelUp);
        }

        protected override void OnClose()
        {
            presenter.OnUpdateCupetList -= Refresh;
            presenter.OnEvolution -= OnEvolution;

            presenter.RemoveEvent();

            EventDelegate.Remove(tab[0].OnChange, OnClickedTabInfo);
            EventDelegate.Remove(tab[1].OnChange, OnClickedTabEvolution);
            EventDelegate.Remove(tab[2].OnChange, OnClickedTabSkill);
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Remove(btnSummon.OnClick, presenter.ReqeustCupetSummon);
            EventDelegate.Remove(btnLevelUp.OnClick, presenter.OnClickedBtnLevelUp);
        }

        protected override void OnHide()
        {
            presenter.RemoveEvent();
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._19000; // 큐펫 정보
            tab[0].LocalKey = LocalizeKey._19026; // 정보
            tab[1].LocalKey = LocalizeKey._19027; // 진화
            tab[2].LocalKey = LocalizeKey._19028; // 스킬
            btnSummon.LocalKey = LocalizeKey._19040; // 소환
            btnLevelUp.LocalKey = LocalizeKey._19002; // 레벨업
        }

        protected override void OnShow(IUIData data = null)
        {
            fxLevelUpEffect.SetActive(false);
            if (data is CupetEntity == false)
            {
                CloseUI();
                return;
            }

            currentSubCanvas = infoView; // 첫 화면은 info뷰로 고정

            presenter.SetData(data as CupetEntity);
            cupetStatusInfo.SetData(presenter);
            ShowSubCanvas(currentSubCanvas);
            Refresh();
        }


        public void Refresh()
        {
            if (presenter.IsInvalid())
                return;

            for (int i = 0; i < tab.Count; ++i)
            {
                tab[i].SetNotice(false);
            }

            tab[1].SetNotice(presenter.IsEvolution());
            cupetStatusInfo.RefreshInfo();
            RefreshSubCanvas();

            if (presenter.IsInPossesion())
            {
                btnSummon.SetActive(false);

                btnLevelUp.SetActive(true);
                btnLevelUp.IsEnabled = presenter.IsLevelUpButton();
            }
            else
            {
                btnSummon.SetActive(true);
                bool canSummon = presenter.IsSummon();
                btnSummon.IsEnabled = canSummon;
                btnSummon.SetNotice(canSummon);

                btnLevelUp.SetActive(false);
            }
        }

        void CloseUI()
        {
            UI.Close<UICupetInfo>();
        }

        /// <summary>
        /// 서브캔버스 Refresh
        /// </summary>
        void RefreshSubCanvas()
        {
            if (currentSubCanvas is CupetInfoInfoView)
            {
                infoView.Refresh();
            }
            if (currentSubCanvas is CupetInfoEvolutionView)
            {
                evolutionView.Refresh();
            }
            if (currentSubCanvas is CupetInfoSkillView)
            {
                skillView.Refresh();
            }
        }

        void OnEvolution()
        {
            fxLevelUpEffect.SetActive(false);
            fxLevelUpEffect.SetActive(true);
        }

        #region 이벤트

        void OnClickedBtnExit()
        {
            CloseUI();
        }

        void OnClickedTabInfo()
        {
            SetSubCanvas(infoView);
        }

        void OnClickedTabEvolution()
        {
            SetSubCanvas(evolutionView);
        }

        void OnClickedTabSkill()
        {
            SetSubCanvas(skillView);
        }

        private void SetSubCanvas(UISubCanvas view)
        {
            if (!UIToggle.current.value)
                return;

            ShowSubCanvas(view);
        }

        private void ShowSubCanvas(UISubCanvas subCanvas)
        {
            currentSubCanvas = subCanvas;

            if (currentSubCanvas is CupetInfoInfoView)
            {
                tab.Value = 0;
            }
            if (currentSubCanvas is CupetInfoEvolutionView)
            {
                tab.Value = 1;
            }
            if (currentSubCanvas is CupetInfoSkillView)
            {
                tab.Value = 2;
            }

            HideAllSubCanvas();
            currentSubCanvas.Show();
        }

        #endregion

        /// <summary>
        /// 레벨업 이펙트 출력
        /// </summary>
        void CupetInfoPresenter.IView.ShowFxLevelUpEffect()
        {
            fxLevelUpEffect.SetActive(false);
            fxLevelUpEffect.SetActive(true);
        }

        void CupetInfoPresenter.IView.SkillViewSelectSkill(int index)
        {
            skillView.SelectSkill(index);
        }

        void CupetInfoPresenter.IView.ShowSkillView()
        {
            ShowSubCanvas(skillView);
        }
    }
}