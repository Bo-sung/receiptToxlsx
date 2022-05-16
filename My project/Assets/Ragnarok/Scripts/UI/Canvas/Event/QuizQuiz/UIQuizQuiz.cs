using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIQuizQuiz : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] QuizQuizView quizQuizView;
        [SerializeField] QuizQuizResultView quizQuizResultView;

        [SerializeField] string correctSound = "[SYSTEM] Dungeon_Clear";
        [SerializeField] string incorrectSound = "poring_die";

        QuizQuizPresenter presenter;

        protected override void OnInit()
        {
            presenter = new QuizQuizPresenter();

            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;
            quizQuizView.OnSelect += presenter.RequestEventQuizReward;
            quizQuizResultView.OnNext += Refresh;

            presenter.OnQuizReward += OnQuizReward;
            presenter.OnNotInProgress += OnNotInProgress;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            popupView.OnConfirm -= OnBack;
            popupView.OnExit -= OnBack;
            quizQuizView.OnSelect -= presenter.RequestEventQuizReward;
            quizQuizResultView.OnNext -= Refresh;

            presenter.OnQuizReward -= OnQuizReward;
            presenter.OnNotInProgress -= OnNotInProgress;
        }

        protected override void OnShow(IUIData data = null)
        {
            quizQuizResultView.Hide();

            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._5405; // 퀴즈퀴즈!
            popupView.ConfirmLocalKey = LocalizeKey._5408; // 닫기
        }

        private void Refresh()
        {
            QuizQuizView.IInput data = presenter.GetData();
            if (data == null)
            {
                CloseUI();
                return;
            }

            quizQuizView.SetData(data);
        }

        void OnQuizReward(bool isCorrect, RewardData rewardData)
        {
            quizQuizResultView.Show();
            quizQuizResultView.SetData(isCorrect, rewardData);

            PlaySfx(isCorrect ? correctSound : incorrectSound);
        }

        void OnNotInProgress()
        {
            UI.ShowToastPopup(LocalizeKey._5412.ToText()); // 현재 진행중이 아닙니다.
            CloseUI();
        }

        private void CloseUI()
        {
            UI.Close<UIQuizQuiz>();
        }

        protected override void OnBack()
        {
            if (quizQuizResultView.IsShow)
            {
                quizQuizResultView.Hide();
                return;
            }

            base.OnBack();
        }
    }
}