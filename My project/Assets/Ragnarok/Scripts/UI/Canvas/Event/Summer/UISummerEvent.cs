using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UISummerEvent : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SimplePopupView simplePopupView;
        [SerializeField] SummerEventView summerEventView;
        [SerializeField] SummerEventCompleteView summerEventCompleteView;

        SummerEventPresenter presenter;

        protected override void OnInit()
        {
            presenter = new SummerEventPresenter();

            simplePopupView.OnExit += OnBack;
            summerEventView.OnSelectBtnComplete += presenter.RequestQuestReward;
            summerEventView.OnSelectBtnSkip += presenter.RequestQuestSkip;
            summerEventView.OnSelectElementBtnComplete += presenter.RequestQuestReward;
            summerEventCompleteView.OnSelectBtnComplete += presenter.ReqeustLastQuestReward;

            presenter.OnStandByReward += UpdateView;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            simplePopupView.OnExit -= OnBack;
            summerEventView.OnSelectBtnComplete -= presenter.RequestQuestReward;
            summerEventView.OnSelectBtnSkip -= presenter.RequestQuestSkip;
            summerEventView.OnSelectElementBtnComplete -= presenter.RequestQuestReward;
            summerEventCompleteView.OnSelectBtnComplete -= presenter.ReqeustLastQuestReward;

            presenter.OnStandByReward -= UpdateView;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            if (!presenter.HasEventData())
            {
                UI.Close<UISummerEvent>();
                return;
            }
            UpdateView();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            simplePopupView.MainTitle = presenter.GetTitle();
        }

        private void UpdateView()
        {
            summerEventView.SetData(presenter.GetArrayData());
            summerEventCompleteView.SetData(presenter.GetLastQuestInfo());
        }
    }
}