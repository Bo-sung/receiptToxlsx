using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="AttendEventView"/>
    /// </summary>
    public class UIAttendEventElement : UIElement<UIAttendEventElement.IInput>
    {
        public interface IInput
        {
            int Day { get; }
            RewardData Reward { get; }
            State State { get; }
        }

        public enum State
        {
            /// <summary>
            /// 보상 대기 중
            /// </summary>
            StandByReward = 1,

            /// <summary>
            /// 이전 보상 수령 대기
            /// </summary>
            StandByRewardWait = 2,

            /// <summary>
            /// 보상 완료
            /// </summary>
            ReceivedReward = 3,

            /// <summary>
            /// 진행 대기
            /// </summary>
            ProgressWait = 4,
        }

        [SerializeField] UILabelHelper labelDay;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelRewardName;
        [SerializeField] UIButtonHelper btnReward;
        [SerializeField] UILabelHelper labelReceived;
        [SerializeField] GameObject goReceivedReward, goStandByRewardWait, goStandByReward, goProgressWait;
        [SerializeField] UILabelHelper labelRewardCount;

        /// <summary>
        /// 보상 받기 버튼 이벤트
        /// </summary>
        public event System.Action OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnReward.OnClick, OnClickedBtnReward);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnReward.OnClick, OnClickedBtnReward);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            labelDay.Text = LocalizeKey._9001.ToText().Replace("{DAY}", info.Day.ToString()); // {DAY}일
            rewardHelper.SetData(info.Reward);
            labelRewardName.Text = info.Reward.ItemName;

            goStandByReward.SetActive(info.State == State.StandByReward);
            goStandByRewardWait.SetActive(info.State == State.StandByRewardWait);
            goReceivedReward.SetActive(info.State == State.ReceivedReward);
            goProgressWait.SetActive(info.State == State.ProgressWait);

            // 마지막 14일차 보상 수량 표시
            if (labelRewardCount)
            {
                labelRewardCount.Text = $"X{info.Reward.Count}";
            }
        }

        private void OnClickedBtnReward()
        {
            OnSelect?.Invoke();
        }
    }
}