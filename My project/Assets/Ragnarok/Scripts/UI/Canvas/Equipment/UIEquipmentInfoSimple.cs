using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEquipmentInfoSimple : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIEquipmentStatusInfo equipmentStatusInfo;

        [SerializeField] UIButtonHelper btnGetSource;
        [SerializeField] UIButtonHelper btnUseSource;
        [SerializeField] GameObject goGetSourceLine;
        [SerializeField] GameObject goUseSourceLine;

        ItemInfo info;

        protected override void OnInit()
        {
            EventDelegate.Add(background.OnClick, OnBack);
            EventDelegate.Add(btnExit.OnClick, OnBack);
            EventDelegate.Add(btnConfirm.OnClick, OnBack);
            EventDelegate.Add(btnGetSource.OnClick, OnClickBtnGetSource);
            EventDelegate.Add(btnUseSource.OnClick, OnClickBtnUseSource);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(background.OnClick, OnBack);
            EventDelegate.Remove(btnExit.OnClick, OnBack);
            EventDelegate.Remove(btnConfirm.OnClick, OnBack);
            EventDelegate.Remove(btnGetSource.OnClick, OnClickBtnGetSource);
            EventDelegate.Remove(btnUseSource.OnClick, OnClickBtnUseSource);
        }

        protected override void OnShow(IUIData data = null)
        {
            info = data as ItemInfo;
            Refresh();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._16000; // 아이템 정보
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
        }

        void Refresh()
        {
            equipmentStatusInfo.SetData(info);

            bool hasGetSource = (info.Get_ClassBitType != ItemSourceCategoryType.None);
            btnGetSource.Text = StringBuilderPool.Get()
                .Append(UIItemSource.GetActivationColorString(isActive: hasGetSource))
                .Append(LocalizeKey._16012.ToText()).Release(); // 획득처 보기
            goGetSourceLine.SetActive(hasGetSource);

            bool hasUseSource = (info.Use_ClassBitType != ItemSourceCategoryType.None);
            btnUseSource.Text = StringBuilderPool.Get()
                .Append(UIItemSource.GetActivationColorString(isActive: hasUseSource))
                .Append(LocalizeKey._16013.ToText()).Release(); // 사용처 보기
            goUseSourceLine.SetActive(hasUseSource);
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

