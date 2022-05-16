using UnityEngine;

namespace Ragnarok
{
    public class UIFreeFightReward : MonoBehaviour, IInspectorFinder
    {
        public interface IInput
        {
            int KillCount { get; }
            RewardData[] GetRewards();
        }

        [SerializeField] UILabelHelper labelKillCount;
        [SerializeField] UIGrid grid;
        [SerializeField] UIRewardHelper[] rewards;

        GameObject myGameObject;
        private IInput input;

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void SetData(IInput input)
        {
            this.input = input;
            Refresh();
        }

        private void Refresh()
        {
            if (input == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            labelKillCount.Text = LocalizeKey._40005.ToText() // {COUNT} KILL
                .Replace(ReplaceKey.COUNT, input.KillCount);

            RewardData[] arrRewardData = input.GetRewards();
            int rewardDataCount = arrRewardData.Length;
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < rewardDataCount ? arrRewardData[i] : null);
            }

            grid.Reposition();
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        bool IInspectorFinder.Find()
        {
            rewards = GetComponentsInChildren<UIRewardHelper>();
            return true;
        }
    }
}