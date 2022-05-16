using UnityEngine;

namespace Ragnarok.View
{
    public class UIEndlessTowerElement : UIElement<UIEndlessTowerElement.IInput>, IInspectorFinder
    {
        public interface IInput
        {
            int GetFloor();
            bool IsBossFloor();
            RewardData[] GetRewards();
        }

        [SerializeField] UILabelHelper labelFloor;
        [SerializeField] UILabelHelper labelBoss;
        [SerializeField] UIGrid rewardGrid;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] GameObject goLock;

        private int clearedFloor;

        protected override void OnLocalize()
        {
            UpdateFloorText();
            labelBoss.LocalKey = LocalizeKey._39501; // BOSS
        }

        protected override void Refresh()
        {
            // Floor
            bool isBossFloor = info.IsBossFloor();
            labelFloor.SetActive(!isBossFloor);
            labelBoss.SetActive(isBossFloor);
            UpdateFloorText();

            // Rewards
            RewardData[] arrData = info.GetRewards();
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < arrData.Length ? arrData[i] : null);
            }
            rewardGrid.Reposition();

            // Lock
            NGUITools.SetActive(goLock, IsLock());
        }

        public void SetClearedFloor(int clearedFloor)
        {
            this.clearedFloor = clearedFloor;
        }

        private void UpdateFloorText()
        {
            int floor = info == null ? 0 : info.GetFloor();
            labelFloor.Text = LocalizeKey._39500.ToText() // {INDEX}층
                .Replace(ReplaceKey.INDEX, floor);
        }

        private bool IsLock()
        {
            if (info == null)
                return false;

            int floor = info == null ? 0 : info.GetFloor();
            return floor > clearedFloor; // 클리어한 층보다 높을 경우
        }

        bool IInspectorFinder.Find()
        {
            rewards = GetComponentsInChildren<UIRewardHelper>();
            return true;
        }
    }
}