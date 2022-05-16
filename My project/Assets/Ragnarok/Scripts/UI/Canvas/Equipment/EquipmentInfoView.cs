using UnityEngine;

namespace Ragnarok.View.EquipmentView
{
    public class EquipmentInfoView : UIView
    {
        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelHelper labelWeight;
        [SerializeField] UISprite iconClassBit;
        [SerializeField] UIButtonHelper btnGetSource;
        [SerializeField] UIButtonHelper btnUseSource;
        [SerializeField] GameObject goGetSourceLine;
        [SerializeField] GameObject goUseSourceLine;
        [SerializeField] UIGrid weightGrid;
        [SerializeField] UIToolTipHelper classTypeToolTip;
        [SerializeField] UIButtonWithIcon btnElement;
        [SerializeField] UILabel labelAniText;
        [SerializeField] UIPlayTween playTween;

        ElementType elementType;

        public event System.Action<UIItemSource.Mode> OnSelectSource;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnGetSource.OnClick, OnClickedBtnGetSource);
            EventDelegate.Add(btnUseSource.OnClick, OnClickedBtnUseSource);

            if (btnElement)
            {
                EventDelegate.Add(btnElement.OnClick, OnClickedBtnElement);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnGetSource.OnClick, OnClickedBtnGetSource);
            EventDelegate.Remove(btnUseSource.OnClick, OnClickedBtnUseSource);

            if (btnElement)
            {
                EventDelegate.Remove(btnElement.OnClick, OnClickedBtnElement);
            }
        }

        protected override void OnLocalize()
        {
        }

        public void SetData(ItemInfo info)
        {
            elementType = info.ElementType;

            equipmentProfile.SetData(info);
#if UNITY_EDITOR
            if (info.Smelt > 0)
            {
                labelName.Text = StringBuilderPool.Get()
                    .Append(info.Name)
                    .Append(" +").Append(info.Smelt)
                    .Append("(").Append(info.ItemId).Append(")")
                    .Release();

                labelAniText.text = StringBuilderPool.Get()
                    .Append("[00000000]").Append(info.Name).Append("[-]")
                    .Append(" +").Append(info.Smelt)
                    .Append("[00000000](").Append(info.ItemId).Append(")[-]")
                    .Release();
            }
            else
            {
                labelName.Text = StringBuilderPool.Get()
                    .Append(info.Name).Append("(").Append(info.ItemId).Append(")")
                    .Release();

                labelAniText.text = string.Empty;
            }
#else
            if (info.Smelt > 0)
            {
                labelName.Text = StringBuilderPool.Get()
                    .Append(info.Name)
                    .Append(" +").Append(info.Smelt)
                    .Release();

                labelAniText.text = StringBuilderPool.Get()
                    .Append("[00000000]").Append(info.Name).Append("[-]")
                    .Append(" +").Append(info.Smelt)
                    .Release();
            }
            else
            {
                labelName.Text = info.Name;
                labelAniText.text = string.Empty;
            }
#endif

            ItemEquipmentSlotType slotType = info.SlotType;
            bool isShowElementIcon = slotType == ItemEquipmentSlotType.Weapon || slotType == ItemEquipmentSlotType.Armor;

            if (btnElement)
            {
                btnElement.SetActive(isShowElementIcon);
                if (isShowElementIcon)
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

            labelDescription.Text = info.Description;
            labelWeight.Text = info.TotalWeightText;
            iconClassBit.spriteName = info.ClassType.GetIconName(info.ItemDetailType);

            weightGrid.Reposition();

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

            classTypeToolTip.SetToolTipLocalizeKey(info.ClassType.ToLocalizeKey());
        }

        public void PlayTween()
        {
            playTween.Play();
        }

        void OnClickedBtnGetSource()
        {
            OnSelectSource?.Invoke(UIItemSource.Mode.GetSource);
        }

        void OnClickedBtnUseSource()
        {
            OnSelectSource?.Invoke(UIItemSource.Mode.Use);
        }

        void OnClickedBtnElement()
        {
            if (elementType == ElementType.None)
                return;

            UI.Show<UISelectPropertyPopup>().ShowElementView(elementType);
        }
    }
}