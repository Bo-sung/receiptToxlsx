using UnityEngine;

namespace Ragnarok.View
{
    public class CardInfoSimpleView : UIView
    {
        [SerializeField] UICardProfile cardProfile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelHelper labelWeight;
        [SerializeField] UISprite iconClassBitType;
        [SerializeField] UIButtonHelper btnGetSource;
        [SerializeField] UIButtonHelper btnUseSource;
        [SerializeField] GameObject goGetSourceLine;
        [SerializeField] GameObject goUseSourceLine;
        [SerializeField] UIToolTipHelper classTypeToolTip;

        public event System.Action OnSelectGetSource, OnSelectUseSource;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnGetSource.OnClick, OnClickedBtnGetSource);
            EventDelegate.Add(btnUseSource.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnGetSource.OnClick, OnClickedBtnGetSource);
            EventDelegate.Remove(btnUseSource.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnLocalize()
        {
        }

        public void Set(ItemInfo info)
        {
            cardProfile.SetData(info);
            var level = LocalizeKey._18005.ToText(). // Lv. {LEVEL}
                    Replace(ReplaceKey.LEVEL, info.GetCardLevelView());

#if UNITY_EDITOR
            labelName.Text = $"{level} {info.Name}({info.ItemId})";
#else
            labelName.Text = $"{level} {info.Name}"; 
#endif
            labelDescription.Text = info.Description;
            labelWeight.Text = info.TotalWeightText;
            iconClassBitType.spriteName = info.ClassType.GetIconName(info.ItemDetailType);

            bool hasGetSource = (info.Get_ClassBitType != ItemSourceCategoryType.None);
            btnGetSource.Text = StringBuilderPool.Get()
                .Append(UIItemSource.GetActivationColorString(isActive: hasGetSource))
                .Append(LocalizeKey._18007.ToText()).Release(); // 획득처 보기
            goGetSourceLine.SetActive(hasGetSource);

            bool hasUseSource = (info.Use_ClassBitType != ItemSourceCategoryType.None);
            btnUseSource.Text = StringBuilderPool.Get()
                .Append(UIItemSource.GetActivationColorString(isActive: hasUseSource))
                .Append(LocalizeKey._18006.ToText()).Release(); // 사용처 보기
            goUseSourceLine.SetActive(hasUseSource);

            classTypeToolTip.SetToolTipLocalizeKey(info.ClassType.ToLocalizeKey());
        }

        void OnClickedBtnGetSource()
        {
            OnSelectGetSource?.Invoke();
        }

        void OnClickedBtnConfirm()
        {
            OnSelectUseSource?.Invoke();
        }
    }
}