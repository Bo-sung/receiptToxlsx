using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildBattleElement : UIBaseGuildElement<UIGuildBattleElement.IInput>
    {
        public interface IInput : IGuildElementInput
        {
            int CurHp { get; }
            int MaxHp { get; }
        }

        [SerializeField] UILabelHelper labelRemainHp;
        [SerializeField] UIProgressBar hp;
        [SerializeField] UILabel labelValue;
        [SerializeField] UIButtonHelper btnEnter;

        public event System.Action<IInput> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnLocalize()
        {
            labelRemainHp.LocalKey = LocalizeKey._33711; // 엠펠리움 HP
            btnEnter.LocalKey = LocalizeKey._33712; // 전투
        }

        protected override void Refresh()
        {
            base.Refresh();

            float progress = MathUtils.GetProgress(info.CurHp, info.MaxHp);
            hp.value = progress;
            labelValue.text = MathUtils.GetPercentText(progress);
        }

        void OnClickedBtnEnter()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info);
        }
    }
}