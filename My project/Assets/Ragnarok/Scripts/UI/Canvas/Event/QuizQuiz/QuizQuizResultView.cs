using UnityEngine;

namespace Ragnarok.View
{
    public sealed class QuizQuizResultView : UIView
    {
        [SerializeField] UIRewardHelper result;
        [SerializeField] UILabelHelper labelResultMessage;
        [SerializeField] UIButtonHelper btnResult;

        private bool isCorrect;
        private RewardData reward;

        public event System.Action OnNext;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnResult.OnClick, Hide);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnResult.OnClick, Hide);
        }

        protected override void OnLocalize()
        {
            btnResult.LocalKey = LocalizeKey._5411; // 확인
        }

        public override void Hide()
        {
            base.Hide();

            if (reward == null)
                return;

            isCorrect = false;
            reward = null;

            OnNext?.Invoke();
        }

        public void SetData(bool isCorrect, RewardData reward)
        {
            this.isCorrect = isCorrect;
            this.reward = reward;
            Refresh();
        }

        private void Refresh()
        {
            if (reward == null)
                return;

            result.SetData(reward);

            // 정답입니다. 다음 보상을 획득하였습니다.
            // 오답입니다. 다음 보상을 획득하였습니다.
            labelResultMessage.LocalKey = isCorrect ? LocalizeKey._5409 : LocalizeKey._5410;
        }
    }
}