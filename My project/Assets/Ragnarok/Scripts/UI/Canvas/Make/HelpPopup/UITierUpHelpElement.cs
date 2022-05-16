using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UITierUpHelpElement : UIElement<UITierUpHelpElement.IInput>
    {
        public interface IInput
        {
            int Tier { get; }
            int NeedJobLevel { get; }
            bool IsNeedUpdate { get; }
        }

        [SerializeField] TweenColor background;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelNeedJobLevel;
        [SerializeField] GameObject goUpdateInfo;
        [SerializeField] UILabelHelper labelUpdateInfo;

        protected override void OnLocalize()
        {
            labelUpdateInfo.Text = LocalizeKey._5504.ToText(); // 업데이트 예정
        }

        protected override void Refresh()
        {
            int tier = info.Tier;

            background.Play(tier % 2 == 0);
            labelTitle.Text = LocalizeKey._5501.ToText() // {INDEX} 초월
                .Replace(ReplaceKey.INDEX, tier);

            NGUITools.SetActive(goUpdateInfo, info.IsNeedUpdate);
            if (info.IsNeedUpdate)
            {
                labelNeedJobLevel.SetActive(false);
            }
            else
            {
                labelNeedJobLevel.SetActive(true);
                labelNeedJobLevel.Text = LocalizeKey._5502.ToText() // [JOB Lv {LEVEL}]
                    .Replace(ReplaceKey.LEVEL, info.NeedJobLevel);
            }
        }
    }
}