using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPartsInfo : UICanvas, PartsInfoPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIPartsProfile partsProfile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelHelper labelOwned;
        [SerializeField] UILabelHelper labelOwnedValue;

        [SerializeField] UIButtonHelper btnGetSource;
        [SerializeField] UIButtonHelper btnUseSource;
        [SerializeField] GameObject goGetSourceLine;
        [SerializeField] GameObject goUseSourceLine;

        [SerializeField] UIGrid grid;
        [SerializeField] UIButtonWithIcon btnElement;

        [SerializeField] UIButtonHelper btnBoxReward;

        PartsInfoPresenter presenter;

        private ElementType elementType;

        protected override void OnInit()
        {
            presenter = new PartsInfoPresenter(this);

            presenter.AddEvent();
            EventDelegate.Add(background.OnClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnGetSource.OnClick, OnClickBtnGetSource);
            EventDelegate.Add(btnUseSource.OnClick, OnClickBtnUseSource);

            if (btnElement)
            {
                EventDelegate.Add(btnElement.OnClick, OnClickedBtnElement);
            }
            EventDelegate.Add(btnBoxReward.OnClick, OnClickedBtnBoxReward);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(background.OnClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnGetSource.OnClick, OnClickBtnGetSource);
            EventDelegate.Remove(btnUseSource.OnClick, OnClickBtnUseSource);

            if (btnElement)
            {
                EventDelegate.Remove(btnElement.OnClick, OnClickedBtnElement);
            }
            EventDelegate.Remove(btnBoxReward.OnClick, OnClickedBtnBoxReward);
        }

        protected override void OnShow(IUIData data)
        {
            if (data is ItemInfo)
            {
                presenter.SelectInfo(data as ItemInfo);
                return;
            }

            Debug.LogError("data가 존재하지 않습니다");
            CloseUI();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._20000; // 아이템 정보
            labelOwned.LocalKey = LocalizeKey._20005; // 보유
            btnBoxReward.LocalKey = LocalizeKey._20006; // 구성품 보기
        }

        public void Refresh()
        {
            ItemInfo info = presenter.info;

            partsProfile.SetData(info);
#if UNITY_EDITOR
            labelName.Text = $"{info.Name}({info.ItemId})";
#else
            labelName.Text = info.Name;
#endif
            labelDescription.Text = info.Description;

            bool hasGetSource = (info.Get_ClassBitType != ItemSourceCategoryType.None);
            btnGetSource.Text = StringBuilderPool.Get()
                .Append(UIItemSource.GetActivationColorString(isActive: hasGetSource))
                .Append(LocalizeKey._20004.ToText()).Release(); // 획득처 보기
            goGetSourceLine.SetActive(hasGetSource);

            bool hasUseSource = (info.Use_ClassBitType != ItemSourceCategoryType.None);
            btnUseSource.Text = StringBuilderPool.Get()
                .Append(UIItemSource.GetActivationColorString(isActive: hasUseSource))
                .Append(LocalizeKey._20003.ToText()).Release(); // 사용처 보기
            goUseSourceLine.SetActive(hasUseSource);

            labelOwnedValue.Text = presenter.GetItemCount().ToString();

            if (btnElement)
            {
                btnElement.SetActive(info.IsElementStone);
                if (info.IsElementStone)
                {
                    elementType = info.ElementType;
                    btnElement.SetIconName(elementType.GetIconName());
                    btnElement.Text = info.GetElementLevelText();
                }
                else
                {
                    elementType = ElementType.None;
                }
            }

            grid.Reposition();

            bool isBox = info.ItemType == ItemType.Box;
            BoxType boxType = presenter.GetBoxType();
            bool isJobBox = boxType == BoxType.JobRef || boxType == BoxType.JobGradeRef;

            // 직업 전용 상자가 아닐때
            if (isBox && !isJobBox)
            {
                btnBoxReward.SetActive(true);
            }
            else
            {
                btnBoxReward.SetActive(false);
            }
        }

        private void CloseUI()
        {
            UI.Close<UIPartsInfo>();
        }

        /// <summary>
        /// 획득처 보기
        /// </summary>
        void OnClickBtnGetSource()
        {
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.GetSource, presenter.info));
        }

        /// <summary>
        /// 사용처 보기
        /// </summary>
        void OnClickBtnUseSource()
        {
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.Use, presenter.info));
        }

        void OnClickedBtnElement()
        {
            if (elementType == ElementType.None)
                return;

            UI.Show<UISelectPropertyPopup>().ShowElementView(elementType);
        }

        /// <summary>
        /// 구성품 보기 버튼
        /// </summary>
        void OnClickedBtnBoxReward()
        {
            if (presenter.info == null)
                return;

            UI.Show<UIBoxRewardList>().Set(presenter.info.ItemId);
        }
    }
}