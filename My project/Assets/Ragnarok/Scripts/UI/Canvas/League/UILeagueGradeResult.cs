using UnityEngine;

namespace Ragnarok.View.League
{
    public class UILeagueGradeResult : MonoBehaviour, IAutoInspectorFinder
    {
        public interface IInput
        {
            string TierName { get; }
            string TierIconName { get; }
            int Ranking { get; }
        }

        [SerializeField] UITextureHelper iconTier;
        [SerializeField] UILabelHelper labelTier;
        [SerializeField] UILabelHelper labelRank;

        GameObject myGameObject;

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void SetData(IInput input)
        {
            if (input == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            iconTier.SetRankTier(input.TierIconName);
            labelTier.Text = input.TierName;

            if (input.Ranking > 0)
            {
                labelRank.Text = LocalizeKey._47007.ToText() // {RANK}위
                    .Replace(ReplaceKey.RANK, input.Ranking);
            }
            else
            {
                labelRank.Text = LocalizeKey._47030.ToText(); // 순위밖
            }
        }

        private void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }
    }
}