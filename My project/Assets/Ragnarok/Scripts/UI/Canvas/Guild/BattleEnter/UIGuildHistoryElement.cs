using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIGuildHistoryElement : UIBaseGuildElement<UIGuildHistoryElement.IInput>
    {
        public interface IInput : IGuildElementInput
        {
            int Cid { get; }
            int Uid { get; }
            string CharacterName { get; }
            int Damage { get; }
            int MaxHp { get; }
        }

        [SerializeField] UILabelHelper labelAttacker, labelDamage;
        [SerializeField] UIButton btnInfo;

        public event System.Action<IInput> OnSelectBtnInfo;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnInfo.onClick, OnClickedBtnInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnInfo.onClick, OnClickedBtnInfo);
        }

        protected override void OnLocalize()
        {
        }

        void OnClickedBtnInfo()
        {
            OnSelectBtnInfo?.Invoke(info);
        }

        protected override void Refresh()
        {
            base.Refresh();

            labelAttacker.Text = StringBuilderPool.Get()
                .Append(LocalizeKey._33726.ToText()) // 공격자
                .Append(" : ").Append(info.CharacterName).Append(" (").Append(MathUtils.CidToHexCode(info.Cid)).Append(")")
                .Release();

            float progress = MathUtils.GetRate(info.Damage, info.MaxHp);
            string percentText = MathUtils.GetPercentText(progress);
            labelDamage.Text = StringBuilderPool.Get()
                .Append(LocalizeKey._33724.ToText()) // 입힌 피해량
                .Append(" : ").Append(info.Damage.ToString("N0")).Append(" (").Append(percentText).Append(")")
                .Release();
        }
    }
}