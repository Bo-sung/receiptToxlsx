using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIShareForceUpgradeElement : UIElement<UIShareForceUpgradeElement.IInput>
    {
        public interface IInput
        {
            int Group { get; }
            int Level { get; }
            string TitleText { get; }
            string ValueText { get; }
            bool IsMaxLevel { get; }
            bool HasNotice { get; }
        }

        [SerializeField] UILabelHelper labelName, labelLevel, labelValue;
        [SerializeField] UIButtonHelper btnUpgrade, btnMax;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnUpgrade.OnClick, OnClickedBtnUpgrade);
            EventDelegate.Add(btnMax.OnClick, OnClickedBtnMax);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnUpgrade.OnClick, OnClickedBtnUpgrade);
            EventDelegate.Remove(btnMax.OnClick, OnClickedBtnMax);
        }

        protected override void OnLocalize()
        {
            btnUpgrade.LocalKey = LocalizeKey._10270; // 강화하기
            btnMax.LocalKey = LocalizeKey._10271; // MAX
        }

        protected override void Refresh()
        {
            labelName.Text = info.TitleText;
            labelLevel.Text = LocalizeKey._4102.ToText() // Lv. {LEVEL}
                .Replace(ReplaceKey.LEVEL, info.Level);
            labelValue.Text = info.ValueText;
            btnUpgrade.SetActive(!info.IsMaxLevel);
            btnUpgrade.SetNotice(info.HasNotice);
            btnMax.SetActive(info.IsMaxLevel);
        }

        void OnClickedBtnUpgrade()
        {
            UI.Show<UIShareForceStatusUpgrade>().SetGroup(info.Group);
        }

        void OnClickedBtnMax()
        {
            string message = LocalizeKey._48222.ToText(); // 최대 레벨에 도달하였습니다.
            UI.ShowToastPopup(message);
        }
    }
}