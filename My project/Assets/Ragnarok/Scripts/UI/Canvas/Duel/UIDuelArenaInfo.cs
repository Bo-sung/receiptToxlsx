using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIDuelArenaInfo : UIView
    {
        public interface IInput
        {
            int NameId { get; }
            int Start { get; }
            int Max { get; }
            RewardData[] Rewards { get; }
        }

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelArena;
        [SerializeField] UILabelHelper labelFlagCount;
        [SerializeField] UILabelHelper labelReward;
        [SerializeField] UIGrid gridReward;
        [SerializeField] UIRewardHelper[] rewards;

        private int titleKey = LocalizeKey._47866; // 아레나
        private int arenaKey = LocalizeKey._47866; // 아레나

        protected override void OnLocalize()
        {
            labelReward.LocalKey = LocalizeKey._47845; // 보상
            UpdateTitleText();
            UpdateArenaText();
        }

        public void SetTitleKey(int titleKey)
        {
            this.titleKey = titleKey;
            UpdateTitleText();
        }

        public void SetData(IInput input)
        {
            if (input == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            arenaKey = input.NameId;
            UpdateArenaText();

            var sb = StringBuilderPool.Get()
                .Append(input.Start).Append('~');

            if (input.Max < int.MaxValue)
                sb.Append(input.Max);

            labelFlagCount.Text = sb.Release();

            int length = input.Rewards == null ? 0 : input.Rewards.Length;
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < length ? input.Rewards[i] : null);
            }

            gridReward.Reposition();
        }

        private void UpdateTitleText()
        {
            labelTitle.LocalKey = titleKey;
        }

        private void UpdateArenaText()
        {
            labelArena.LocalKey = arenaKey;
        }
    }
}