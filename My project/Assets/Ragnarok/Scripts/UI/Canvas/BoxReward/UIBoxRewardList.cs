using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBoxRewardList : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] BoxRewardItemView itemView;
        [SerializeField] BoxRewardListView listView;
        [SerializeField] UILabelHelper labelNotice;

        BoxRewardListPresenter presenter;

        protected override void OnInit()
        {
            presenter = new BoxRewardListPresenter();

            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            popupView.OnConfirm -= OnBack;
            popupView.OnExit -= OnBack;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._8300; // 구성품 정보
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인
            labelNotice.LocalKey = LocalizeKey._90303; // 확률은 소수점 3자리에서 반올림하여 2자리까지 표기가 됩니다.\n표기된 확률은 합계가 100%보다 높거나 낮을 수 있습니다.
        }

        public void Set(int itemId)
        {
            itemView.Set(presenter.GetItemViewInfo(itemId));
            listView.Set(presenter.GetBoxRewardGroup(itemId));
        }
    }
}