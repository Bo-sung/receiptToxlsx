using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UISingleGuildBattleElement : UIBaseGuildElement<UISingleGuildBattleElement.IInput>
    {
        public interface IInput : IGuildElementInput
        {
            int CurHp { get; }
            int MaxHp { get; }
        }

        [SerializeField] UILabelHelper labelRemainHp;
        [SerializeField] UIProgressBar hp;
        [SerializeField] UILabel labelValue;

        protected override void OnLocalize()
        {
            labelRemainHp.LocalKey = LocalizeKey._33722; // 남은 엠펠리움 HP
        }

        protected override void Refresh()
        {
            base.Refresh();

            float progress = MathUtils.GetRate(info.CurHp, info.MaxHp);
            hp.value = progress;

            string percentText = MathUtils.GetPercentText(progress);
            labelValue.text = StringBuilderPool.Get()
                .Append(info.CurHp).Append("/").Append(info.MaxHp).Append(" (").Append(percentText).Append(")")
                .Release();
        }
    }
}