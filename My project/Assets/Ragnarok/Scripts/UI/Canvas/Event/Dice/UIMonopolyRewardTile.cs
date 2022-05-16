using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIMonopolyRewardTile : UIMonopolyTile
    {
        public override MonopolyTileType TileType => MonopolyTileType.Reward;

        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] GameObject effect;

        IInput input;

        public override void SetData(IInput input)
        {
            this.input = input;
            Refresh();
        }

        private void Refresh()
        {
            if (input == null)
            {
                rewardHelper.SetData(null);
                return;
            }

            rewardHelper.SetData(input.Reward);
        }

        public override void ShowChangeEffect()
        {
            NGUITools.SetActive(effect, false);
            NGUITools.SetActive(effect, true);
        }
    }
}