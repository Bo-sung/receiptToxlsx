using UnityEngine;

namespace Ragnarok.View
{
    public sealed class QuizQuizView : UIView
    {
        public interface IInput
        {
            int Id { get; }
            int StartDate { get; }
            string ImageName { get; }
            int QuizLocalKey { get; }
            RewardData Reward { get; }
            int Seq { get; }
            int MaxSeq { get; }
            bool GetAnswer(byte answer);
        }

        public delegate void SelectEvent(int id, byte answer, bool isCorrect);

        private enum ButtonType
        {
            Yes = 1,
            No = 2,
        }

        [SerializeField] UILabelHelper labelQuizTitle;
        [SerializeField] UITextureHelper texture;
        [SerializeField] UILabelHelper labelQuiz;
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UIButton btnYes, btnNo;

        public event SelectEvent OnSelect;

        private IInput input;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnYes.onClick, OnClickedBtnYes);
            EventDelegate.Add(btnNo.onClick, OnClickedBtnNo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnYes.onClick, OnClickedBtnYes);
            EventDelegate.Remove(btnYes.onClick, OnClickedBtnNo);
        }

        protected override void OnLocalize()
        {
            labelRewardTitle.LocalKey = LocalizeKey._5407; // 보  상
        }

        void OnClickedBtnYes()
        {
            Select(ButtonType.Yes);
        }

        void OnClickedBtnNo()
        {
            Select(ButtonType.No);
        }

        public void SetData(IInput input)
        {
            this.input = input;
            Refresh();
        }

        private void Refresh()
        {
            if (input == null)
                return;

            string dateText = input.StartDate.ToString();
            if (dateText.Length == 8)
            {
                dateText = StringBuilderPool.Get()
                    .Append(dateText.Substring(0, 4)) // yyyy
                    .Append("-")
                    .Append(dateText.Substring(4, 2)) // MM
                    .Append("-")
                    .Append(dateText.Substring(6, 2)) // dd
                    .Release();
            }

            string quizTitleText = LocalizeKey._5406.ToText() // {DATE} 퀴즈
                .Replace(ReplaceKey.DATE, dateText);

            labelQuizTitle.Text = StringBuilderPool.Get()
                .Append(quizTitleText)
                .Append("(").Append(input.Seq).Append("/").Append(input.MaxSeq).Append(")")
                .Release();

            texture.Set(input.ImageName);

            labelQuiz.LocalKey = input.QuizLocalKey;
            reward.SetData(input.Reward);
        }

        private void Select(ButtonType buttonType)
        {
            if (input == null)
                return;

            byte answer = (byte)buttonType;
            OnSelect?.Invoke(input.Id, answer, input.GetAnswer(answer));
        }
    }
}