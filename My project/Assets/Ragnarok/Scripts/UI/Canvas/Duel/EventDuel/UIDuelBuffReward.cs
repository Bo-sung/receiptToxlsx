using UnityEngine;

namespace Ragnarok
{
    public class UIDuelBuffReward : MonoBehaviour, IAutoInspectorFinder
    {
        public interface IInput
        {
            int Rank { get; }
            string GetTitle(bool isResult);
            string GetDescription();
        }

        private const int TOP_RANK = 3; // 3위까지는 아이콘으로 표시
        private const string TOP_RANK_ICON_FORMAT = "Ui_Common_Icon_Rank_{0:D2}";

        [SerializeField] UILabelValue reward;
        [SerializeField] UISprite iconRank;

        GameObject myGameObject;

        private IInput input;
        private bool isResult;

        void Awake()
        {
            myGameObject = gameObject;
            UI.AddEventLocalize(OnLocalize);
        }

        void OnDestroy()
        {
            UI.RemoveEventLocalize(OnLocalize);
        }

        public void SetData(IInput input, bool isResult)
        {
            this.input = input;
            this.isResult = isResult;
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
            SetRank(input.Rank);
            OnLocalize();
        }

        void OnLocalize()
        {
            if (input == null)
                return;

            reward.Title = input.GetTitle(isResult);
            reward.Value = input.GetDescription();
        }

        private void SetRank(int rank)
        {
            if (iconRank == null)
                return;

            if (rank > TOP_RANK) // 3위 밖
            {
                NGUITools.SetActive(iconRank.cachedGameObject, false);
            }
            else if (rank > 0) // 3위 안
            {
                NGUITools.SetActive(iconRank.cachedGameObject, true);
                iconRank.spriteName = string.Format(TOP_RANK_ICON_FORMAT, rank);
            }
            else
            {
                NGUITools.SetActive(iconRank.cachedGameObject, false);
            }
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }
    }
}