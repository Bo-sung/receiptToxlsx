using UnityEngine;

namespace Ragnarok.View.League
{
    public class UILeagueGradeRewardBar : UIElement<UILeagueGradeRewardBar.IInput>
    {
        public interface IInput
        {
            string GetName();
            string GetIconName();
            int GetNeedPoint();
            RewardData GetRewardData(int index);
        }

        [SerializeField] UITextureHelper iconTier;
        [SerializeField] UILabelHelper labelName, labelNeedPoint;
        [SerializeField] UIGrid grid;
        [SerializeField] UIRewardHelper[] rewards;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            iconTier.SetRankTier(info.GetIconName());
            labelName.Text = info.GetName();
            labelNeedPoint.Text = LocalizeKey._47015.ToText() // 필요 포인트 : {VALUE}
                .Replace(ReplaceKey.VALUE, info.GetNeedPoint());

            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(info.GetRewardData(i));
            }

            grid.Reposition();
        }
    }
}