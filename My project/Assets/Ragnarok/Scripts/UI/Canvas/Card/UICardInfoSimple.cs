using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 보상 표시용
    /// </summary>
    public class UICardInfoSimple : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnConfirm;

        [SerializeField] CardInfoSimpleView cardInfoSimpleView;
        [SerializeField] CardInfoSimpleOptionView cardInfoSimpleOptionView;
        [SerializeField] UILabelHelper labelNotice;

        ItemInfo info;

        protected override void OnInit()
        {
            cardInfoSimpleView.OnSelectGetSource += OnClickBtnGetSource;
            cardInfoSimpleView.OnSelectUseSource += OnClickBtnUseSource;

            EventDelegate.Add(background.OnClick, OnBack);
            EventDelegate.Add(btnExit.OnClick, OnBack);
            EventDelegate.Add(btnConfirm.OnClick, OnBack);
        }

        protected override void OnClose()
        {
            cardInfoSimpleView.OnSelectGetSource -= OnClickBtnGetSource;
            cardInfoSimpleView.OnSelectUseSource -= OnClickBtnUseSource;

            EventDelegate.Remove(background.OnClick, OnBack);
            EventDelegate.Remove(btnExit.OnClick, OnBack);
            EventDelegate.Remove(btnConfirm.OnClick, OnBack);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._18000; // 아이템 정보
            btnConfirm.LocalKey = LocalizeKey._18003; // 확인
            labelNotice.LocalKey = LocalizeKey._18013; // 5랭크 쉐도우 카드는 쉐도우 장비에 하나만 장착할 수 있습니다.
        }

        public void Show(ItemInfo info)
        {
            this.info = info;
            cardInfoSimpleView.Set(info);
            cardInfoSimpleOptionView.Set(info);
            labelNotice.SetActive(info.IsShadow);
        }

        /// <summary>
        /// 획득처 보기
        /// </summary>
        void OnClickBtnGetSource()
        {
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.GetSource, info));
        }

        /// <summary>
        /// 사용처 보기
        /// </summary>
        void OnClickBtnUseSource()
        {
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.Use, info));
        }
    }
}
