using UnityEngine;

namespace Ragnarok
{
    public class UIDuelRank : MonoBehaviour, IAutoInspectorFinder
    {
        public interface IInput
        {
            Job GetJob();
            Gender GetGender();
            string GetNameText();
            string GetJobLevelText();
            int GetBattleScore();
            string GetRankValueText();
            int GetRank();
            int GetServerGroupId();
            int GetAlphabetIndex();
            string GetProfileName();
        }

        private const int TOP_RANK = 3; // 3위까지는 아이콘으로 표시
        private const string TOP_RANK_ICON_FORMAT = "Ui_Common_Icon_Rank_{0:D2}";

        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UITextureHelper thumbnail;
        [SerializeField] UILabelHelper labelName, labelJobLevel, labelBattleScore;
        [SerializeField] UILabelHelper labelWinCount, labelRank;
        [SerializeField] UISprite iconRank;
        [SerializeField] UIDuelAlphabetCollection alphabetCube;

        GameObject myGameObject;

        private IInput input;

        void Awake()
        {
            myGameObject = gameObject;
            UI.AddEventLocalize(OnLocalize);
        }

        void OnDestroy()
        {
            UI.RemoveEventLocalize(OnLocalize);
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

            iconJob.Set(input.GetJob().GetJobIcon());
            thumbnail.Set(input.GetProfileName());
            labelBattleScore.Text = input.GetBattleScore().ToString();
            SetRank(input.GetRank());
            alphabetCube.Use(input.GetServerGroupId(), input.GetAlphabetIndex());

            OnLocalize();
        }

        void OnLocalize()
        {
            if (input == null)
                return;

            labelName.Text = input.GetNameText();
            labelJobLevel.Text = input.GetJobLevelText();
            labelWinCount.Text = input.GetRankValueText();
        }

        private void SetRank(int rank)
        {
            if (rank > TOP_RANK) // 3위 밖
            {
                NGUITools.SetActive(iconRank.cachedGameObject, false);

                labelRank.SetActive(true);
                labelRank.Text = LocalizeKey._47914.ToText() // {RANK}위
                    .Replace(ReplaceKey.RANK, rank);
            }
            else if (rank > 0) // 3위 안
            {
                NGUITools.SetActive(iconRank.cachedGameObject, true);
                iconRank.spriteName = string.Format(TOP_RANK_ICON_FORMAT, rank);

                labelRank.SetActive(false);
            }
            else // 순위 밖
            {
                NGUITools.SetActive(iconRank.cachedGameObject, false);

                labelRank.SetActive(true);
                labelRank.Text = "-";
            }
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }
    }
}