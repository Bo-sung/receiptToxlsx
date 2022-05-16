using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIFreeFightResult : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIGrid rewardGrid;
        [SerializeField] UIRewardHelper[] rewards;

        public event System.Action OnAutoHide;

        FreeFightResultPresenter presenter;

        protected override void OnInit()
        {
            presenter = new FreeFightResultPresenter();
            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Show(FreeFightEventType eventType, int killCount, int round, float autoHideMilliseconds)
        {
            Show();

            RewardData[] arrReward = presenter.GetCurrentRewards(eventType, killCount);
            int rewardCount = arrReward == null ? 0 : arrReward.Length;
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(i < rewardCount ? arrReward[i] : null);
            }

            rewardGrid.Reposition();
            labelDescription.SetActive(rewardCount > 0);

            if (rewardCount < presenter.totalRound)
            {
                labelMainTitle.Text = "ROUND\nFINISHED";
            }
            else
            {
                labelMainTitle.Text = "FINAL\nFINISHED";
            }

            if (round < presenter.totalRound)
            {
                labelDescription.Text = LocalizeKey._40018.ToText() // 제 {INDEX} 라운드의 보상이 우편으로 지급되었습니다.
                    .Replace(ReplaceKey.INDEX, round);
            }
            else
            {
                labelDescription.Text = LocalizeKey._40019.ToText(); // 마지막 라운드의 보상이 우편으로 지급되었습니다.
            }

            AsyncAutoHide(autoHideMilliseconds).WrapErrors();
        }

        private async Task AsyncAutoHide(float delayMilliseconds)
        {
            await Task.Delay(System.TimeSpan.FromMilliseconds(delayMilliseconds));
            Hide();
            OnAutoHide?.Invoke();
        }

        public override bool Find()
        {
            base.Find();

            rewards = GetComponentsInChildren<UIRewardHelper>();
            return true;
        }
    }
}