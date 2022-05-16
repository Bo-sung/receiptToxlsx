using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildSupportSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIGuildSupportSelectElement element;
        [SerializeField] UILabelHelper labelNoData;

        private SuperWrapContent<UIGuildSupportSelectElement, UIGuildSupportSelectElement.IInput> wrapContent;
        private System.Action<UIGuildSupportSelectElement.IInput> onSelected;

        GuildSupportSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildSupportSelectPresenter();

            wrapContent = wrapper.Initialize<UIGuildSupportSelectElement, UIGuildSupportSelectElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelect += OnSelectElement;
            }

            popupView.OnConfirm += CloseUI;
            popupView.OnExit += CloseUI;

            presenter.OnUpdateGuildMember += OnUpdateGuildMember;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            foreach (var item in wrapContent)
            {
                item.OnSelect -= OnSelectElement;
            }

            popupView.OnConfirm -= CloseUI;
            popupView.OnExit -= CloseUI;

            presenter.OnUpdateGuildMember -= OnUpdateGuildMember;

        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RequestGuildMeber();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._34200; // 지원 캐릭터
            popupView.ConfirmLocalKey = LocalizeKey._34202; // 닫기
            labelNoData.LocalKey = LocalizeKey._34203; // 사용 가능한 지원 동료가 없습니다.
        }

        void OnSelectElement(UIGuildSupportSelectElement.IInput element)
        {
            onSelected?.Invoke(element);
            onSelected = null;
            CloseUI();
        }

        public void Set(System.Action<UIGuildSupportSelectElement.IInput> onSelected)
        {
            this.onSelected = onSelected;
        }

        private void OnUpdateGuildMember()
        {
            UIGuildSupportSelectElement.IInput[] inputs = presenter.GetArray();
            wrapContent.SetData(inputs);

            int length = inputs == null ? 0 : inputs.Length;
            labelNoData.SetActive(length == 0);
        }

        private void CloseUI()
        {
            UI.Close<UIGuildSupportSelect>();
        }
    }
}