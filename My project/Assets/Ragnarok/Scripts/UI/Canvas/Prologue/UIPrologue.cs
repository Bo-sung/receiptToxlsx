using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPrologue : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        [SerializeField] PrologueView prologueView;
        [SerializeField] PrologueBlinkView prologueBlinkView;
        [SerializeField] PrologueSelectView prologueSelectView;
        [SerializeField] PrologueGetDeviceView prologueGetDeviceView;
        [SerializeField] PrologueEffectView prologueEffectView;

        [SerializeField] UIButton btnUINext;

        ProloguePresenter presenter;

        public event System.Action OnHideUI;
        public event System.Action OnFinished;
        public event System.Action OnTutorial;
        public event System.Action OnShowPortal;

        protected override void OnInit()
        {
            presenter = new ProloguePresenter(); // 다이얼로그 테이블 셋팅

            EventDelegate.Add(btnUINext.onClick, OnPrologueNext);

            prologueView.OnShowPopup += ShowPopupView;
            prologueView.OnHideView += OnHideView;
            prologueBlinkView.OnHideView += OnHideView;
            prologueEffectView.OnHideView += OnHideView;
            prologueSelectView.OnHidePopup += NextDialog;
            prologueGetDeviceView.OnHidePopup += NextDialog;
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnUINext.onClick, OnPrologueNext);

            prologueView.OnShowPopup -= ShowPopupView;
            prologueView.OnHideView -= OnHideView;
            prologueBlinkView.OnHideView -= OnHideView;
            prologueEffectView.OnHideView -= OnHideView;
            prologueSelectView.OnHidePopup -= NextDialog;
            prologueGetDeviceView.OnHidePopup -= NextDialog;
        }

        protected override void OnShow(IUIData data = null)
        {
            prologueView.Hide();
            prologueBlinkView.Hide();
            prologueSelectView.Hide();
            prologueGetDeviceView.Hide();
            prologueEffectView.Hide();

            switch (presenter.CurrentProgressType)
            {
                case ProloguePresenter.ProgressType.Flow1_table:
                    prologueView.Show(presenter.GetPrologueTable1(), true);
                    break;

                case ProloguePresenter.ProgressType.Flow2_blink:
                    prologueBlinkView.Show();
                    break;

                case ProloguePresenter.ProgressType.Flow3_table:
                    prologueView.Show(presenter.GetPrologueTable2());
                    break;

                case ProloguePresenter.ProgressType.Flow4_table:
                    prologueView.Show(presenter.GetPrologueTable3());
                    break;

                case ProloguePresenter.ProgressType.Flow5_effect:
                    prologueEffectView.Show();
                    break;
            }
        }

        public void InitPrologueData(bool isMale)
        {
            presenter.CurrentProgressType = ProloguePresenter.ProgressType.Flow1_table;
            prologueView.SetGender(isMale, presenter.GetTalkerColumnName(), presenter.GetTextColumnName());
        }

        protected override void OnHide()
        {
            OnHideUI?.Invoke();

            if (PossibleStep(ProloguePresenter.ProgressType.Flow5_effect))
            {
                OnShowPortal?.Invoke();
            }
        }

        protected override void OnLocalize()
        {
        }

        void ShowPopupView(ProloguePresenter.TalkerType type)
        {
            switch (type)
            {
                case ProloguePresenter.TalkerType.SelectPopup:
                    prologueSelectView.Show();
                    break;

                case ProloguePresenter.TalkerType.GainPopup:
                    prologueGetDeviceView.Show();
                    Analytics.TrackEvent(TrackType.Tutorial04_FirstSharevice);
                    break;
            }
        }

        void OnHideView()
        {
            switch (presenter.CurrentProgressType)
            {
                case ProloguePresenter.ProgressType.Flow1_table:
                    prologueView.Hide();
                    break;

                case ProloguePresenter.ProgressType.Flow2_blink:
                    prologueBlinkView.Hide();
                    break;

                case ProloguePresenter.ProgressType.Flow3_table:
                    prologueView.Hide();
                    Hide();

                    OnTutorial?.Invoke();
                    return;

                case ProloguePresenter.ProgressType.Flow4_table:
                    prologueView.Hide();
                    Hide();
                    break;

                case ProloguePresenter.ProgressType.Flow5_effect:
                    //prologueEffectView.Hide();
                    break;

                default:
                    throw new System.InvalidOperationException(presenter.CurrentProgressType.ToString());
            }

            OnPrologueNext();
        }

        public bool PossibleStep(ProloguePresenter.ProgressType type)
        {
            switch (type)
            {
                case ProloguePresenter.ProgressType.Flow4_table:
                    if (presenter.CurrentProgressType == ProloguePresenter.ProgressType.Flow3_table) return true;
                    break;
                case ProloguePresenter.ProgressType.Flow5_effect:
                    if (presenter.CurrentProgressType == ProloguePresenter.ProgressType.Flow4_table) return true;
                    break;
            }

            return false;
        }

        public void ShowNextView()
        {
            presenter.CurrentProgressType++;
            Show();
        }

        private void NextDialog()
        {
            prologueView.ShowNextDialog();
        }

        void OnPrologueNext()
        {
            switch (presenter.CurrentProgressType)
            {
                case ProloguePresenter.ProgressType.Flow1_table:
                    if (prologueView.isActiveAndEnabled) return;

                    presenter.CurrentProgressType = ProloguePresenter.ProgressType.Flow2_blink;
                    Show();
                    break;

                case ProloguePresenter.ProgressType.Flow2_blink:
                    if (prologueBlinkView.isActiveAndEnabled) return;

                    presenter.CurrentProgressType = ProloguePresenter.ProgressType.Flow3_table;
                    Show();
                    break;

                case ProloguePresenter.ProgressType.Flow5_effect:
                    OnFinished?.Invoke();
                    break;
            }
        }
    }
}