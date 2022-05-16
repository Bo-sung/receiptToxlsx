using UnityEngine;

namespace Ragnarok.View
{
    public class CostumeInfoView : UIView
    {
        [SerializeField] UICostumeProfile costumeProfile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UISprite iconClassBit;
        [SerializeField] UIButtonHelper btnGetSource;
        [SerializeField] UIButtonHelper btnUseSource;
        [SerializeField] GameObject goGetSourceLine;
        [SerializeField] GameObject goUseSourceLine;
        [SerializeField] UIToolTipHelper classTypeToolTip;

        public event System.Action<UIItemSource.Mode> OnSelectSource;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnGetSource.OnClick, OnClickedBtnGetSource);
            EventDelegate.Add(btnUseSource.OnClick, OnClickedBtnUseSource);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnGetSource.OnClick, OnClickedBtnGetSource);
            EventDelegate.Remove(btnUseSource.OnClick, OnClickedBtnUseSource);
        }

        protected override void OnLocalize()
        {
        }

        public void SetData(ItemInfo info)
        {
            costumeProfile.Set(info);
#if UNITY_EDITOR
            labelName.Text = $"{info.Name}({info.ItemId})";
#else
            labelName.Text = info.Name;
#endif

            labelDescription.Text = info.Description;
            iconClassBit.spriteName = info.CostumeType.GetIconName();

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

            classTypeToolTip.SetToolTipLocalizeKey(info.CostumeType.ToLocalizeKey());
        }

        void OnClickedBtnGetSource()
        {
            OnSelectSource?.Invoke(UIItemSource.Mode.GetSource);
        }

        void OnClickedBtnUseSource()
        {
            OnSelectSource?.Invoke(UIItemSource.Mode.Use);
        }
    }
}