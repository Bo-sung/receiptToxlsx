using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRewardList : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] RewardListView rewardListView;

        protected override void OnInit()
        {
            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;
        }

        protected override void OnClose()
        {
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
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인
        }

        public void Set(int titleKey, UIRewardListElement.IInput[] inputs)
        {
            popupView.MainTitleLocalKey = titleKey;
            rewardListView.SetData(inputs);
        }
    }
}