using UnityEngine;

namespace Ragnarok.View.League
{
    public class LeagueResultPopupView : UIView
    {
        public interface IInput
        {
            int SeasonNo { get; }
            UILeagueGradeResult.IInput Before { get; }
            UILeagueGradeResult.IInput After { get; }
            RewardData GetReward(int index);
        }

        [SerializeField] UIEventTrigger unselect;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILeagueGradeResult before, after;
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] UIGrid grid;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIButtonHelper btnReward;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(unselect.onClick, Hide);
            EventDelegate.Add(btnExit.onClick, Hide);
            EventDelegate.Add(btnReward.OnClick, Hide);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(unselect.onClick, Hide);
            EventDelegate.Remove(btnExit.onClick, Hide);
            EventDelegate.Remove(btnReward.OnClick, Hide);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.Text = LocalizeKey._47021.ToText(); // 시즌 결과
            labelRewardTitle.Text = LocalizeKey._47024.ToText(); // 보  상
            labelNotice.Text = LocalizeKey._47025.ToText(); // 우편함에서 보상을 확인하세요.
            btnReward.Text = LocalizeKey._47026.ToText(); // 확인
        }

        public void Show(IInput input)
        {
            labelTitle.Text = LocalizeKey._47022.ToText() // 시즌 {VALUE} 랭킹 결과
                .Replace(ReplaceKey.VALUE, input.SeasonNo);

            before.SetData(input.Before);
            after.SetData(input.After);

            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(input.GetReward(i));
            }

            grid.Reposition();

            Show();
        }
    }
}