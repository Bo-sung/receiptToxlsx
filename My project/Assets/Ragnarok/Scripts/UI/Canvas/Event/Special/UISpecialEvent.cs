using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UISpecialEvent : UICanvas, IInspectorFinder, TutorialMazeExit.IImpl
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UITabHelper tab;
        [SerializeField] UIGrid tabGrid;

        [SerializeField] PopupView popupView;
        [SerializeField] QuizQuizView quizQuizView;
        [SerializeField] QuizQuizResultView quizQuizResultView;
        [SerializeField] MultiMazeQuestView multiMazeQuestView;
        [SerializeField] CatCoinGiftView catCoinGiftView;

        [SerializeField] string correctSound = "[SYSTEM] Dungeon_Clear";
        [SerializeField] string incorrectSound = "poring_die";

        SpecialEventPresenter presenter;
        public static SpecialEventType selectedType = SpecialEventType.QuizQuiz;
        const int MaxWidth = 640;

        protected override void OnInit()
        {
            presenter = new SpecialEventPresenter();

            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;
            quizQuizView.OnSelect += presenter.RequestEventQuizReward;
            quizQuizResultView.OnNext += Refresh;
            multiMazeQuestView.OnSelect += OnSelectMultiMazeQuestReward;

            presenter.OnQuizReward += OnQuizReward;
            presenter.OnNotInProgress += OnNotInProgress;
            presenter.OnCatCoinGiftReward += Refresh;
            presenter.OnStandByReward += Refresh;

            presenter.AddEvent();

            EventDelegate.Add(tab[(int)SpecialEventType.QuizQuiz].OnChange, ShowQuizTab);
            EventDelegate.Add(tab[(int)SpecialEventType.MultiMazeQuestView].OnChange, ShowMultiMazeTab);
            EventDelegate.Add(tab[(int)SpecialEventType.CatCoinGift].OnChange, ShowGiftTab);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(tab[(int)SpecialEventType.QuizQuiz].OnChange, ShowQuizTab);
            EventDelegate.Remove(tab[(int)SpecialEventType.MultiMazeQuestView].OnChange, ShowMultiMazeTab);
            EventDelegate.Remove(tab[(int)SpecialEventType.CatCoinGift].OnChange, ShowGiftTab);

            presenter.RemoveEvent();

            popupView.OnConfirm -= OnBack;
            popupView.OnExit -= OnBack;
            quizQuizView.OnSelect -= presenter.RequestEventQuizReward;
            quizQuizResultView.OnNext -= Refresh;
            multiMazeQuestView.OnSelect -= OnSelectMultiMazeQuestReward;

            presenter.OnQuizReward -= OnQuizReward;
            presenter.OnNotInProgress -= OnNotInProgress;
            presenter.OnCatCoinGiftReward -= Refresh;
            presenter.OnStandByReward -= Refresh;

            selectedType = SpecialEventType.QuizQuiz;
        }

        protected override void OnShow(IUIData data = null)
        {
            // 결과 팝업은 숨김
            quizQuizResultView.Hide();

            // 탭 셋팅 및 뷰 선택
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._5414; // 스페셜 이벤트
            popupView.ConfirmLocalKey = LocalizeKey._5408; // 닫기

            tab[(int)SpecialEventType.QuizQuiz].LocalKey = LocalizeKey._5405; // 퀴즈퀴즈!
            tab[(int)SpecialEventType.MultiMazeQuestView].LocalKey = LocalizeKey._5422; // 미궁 정복자
            tab[(int)SpecialEventType.CatCoinGift].LocalKey = BasisProjectTypeLocalizeKey.CatCoinGiftTab.GetInt();
        }

        void OnSelectMultiMazeQuestReward(int id)
        {
            presenter.RequestMultiMazeReward(id);
            isSelectMultiMazeQuestReward = true;
        }

        private void SetTab(SpecialEventType type)
        {
            // 탭 선택
            tab[(int)type].Set(true);

            // 빨콩 셋팅
            for (int i = 0; i < tab.Count; i++)
            {
                if (i == (int)type)
                {
                    tab[i].SetNotice(false); // 선택중인 탭은 빨콩표시 안함
                }
                else
                {
                    switch ((SpecialEventType)i)
                    {
                        case SpecialEventType.QuizQuiz:
                            tab[i].SetNotice(true); // 퀴즈퀴즈는 무조건 표시
                            break;

                        case SpecialEventType.MultiMazeQuestView:
                            tab[i].SetNotice(presenter.CanMultiMazeQuest()); // 보상수령 가능할 때 표시
                            break;

                        case SpecialEventType.CatCoinGift:
                            tab[i].SetNotice(presenter.CanRewardCatCoinGift()); // 보상수령 가능할 때 표시
                            break;

                        // 새로 추가되는 탭이 있으려나..
                        default:
                            tab[i].SetNotice(false);
                            break;
                    }
                }
            }
        }

        private void Refresh()
        {
            // 이벤트 진행 체크 및 탭 셋팅..
            bool activeQuizEvent = presenter.ActivationQuizEvent();
            bool activeGiftEvent = presenter.ActivationCatCoinGiftEvent();
            bool activeMultiMazeQuest = presenter.ActiveMultiMazeQuest();

            // 진행중인 이벤트가 없을경우..
            if (!activeQuizEvent && !activeGiftEvent && !activeMultiMazeQuest)
            {
                CloseUI();
                return;
            }

            // 탭 활성화 상태
            var tabCount = 0;
            if (activeQuizEvent)
                ++tabCount;

            if (activeMultiMazeQuest)
                ++tabCount;

            if (activeGiftEvent)
                ++tabCount;

            int width = MaxWidth / tabCount;
            tab.SetToggleWidgetSize((int)SpecialEventType.QuizQuiz, width: width);
            tab.SetToggleWidgetSize((int)SpecialEventType.MultiMazeQuestView, width: width);
            tab.SetToggleWidgetSize((int)SpecialEventType.CatCoinGift, width: width);

            tab[(int)SpecialEventType.QuizQuiz].SetActive(activeQuizEvent);
            tab[(int)SpecialEventType.MultiMazeQuestView].SetActive(activeMultiMazeQuest);
            tab[(int)SpecialEventType.CatCoinGift].SetActive(activeGiftEvent);
            tabGrid.cellWidth = width;
            tabGrid.Reposition();

            if (selectedType == SpecialEventType.QuizQuiz && !activeQuizEvent)
                selectedType = SpecialEventType.MultiMazeQuestView;

            if (selectedType == SpecialEventType.MultiMazeQuestView && !activeMultiMazeQuest)
                selectedType = SpecialEventType.CatCoinGift;

            // 탭 선택
            switch (selectedType)
            {
                case SpecialEventType.QuizQuiz:
                    if (activeQuizEvent)
                    {
                        RefreshQuiz();
                    }
                    break;

                case SpecialEventType.MultiMazeQuestView:
                    if (activeMultiMazeQuest)
                    {
                        RefreshMultiMazeQuest();
                    }
                    break;

                case SpecialEventType.CatCoinGift:
                    if (activeGiftEvent)
                    {
                        RefreshGift();
                    }
                    break;
            }
        }

        private void RefreshQuiz()
        {
            selectedType = SpecialEventType.QuizQuiz;
            SetTab(selectedType);

            QuizQuizView.IInput data = presenter.GetData();
            if (data == null)
            {
                CloseUI();
                return;
            }

            catCoinGiftView.Hide();
            multiMazeQuestView.Hide();

            quizQuizView.Show();
            quizQuizView.SetData(data);
        }

        private void RefreshMultiMazeQuest()
        {
            selectedType = SpecialEventType.MultiMazeQuestView;
            SetTab(selectedType);

            quizQuizView.Hide();
            catCoinGiftView.Hide();

            multiMazeQuestView.Show();
            multiMazeQuestView.SetData(presenter.GetArrayData());
        }

        private void RefreshGift()
        {
            selectedType = SpecialEventType.CatCoinGift;
            SetTab(selectedType);

            CatCoinGiftData[] datas = presenter.GetGiftData();
            if (datas == null)
            {
                CloseUI();
                return;
            }

            quizQuizView.Hide();
            multiMazeQuestView.Hide();

            catCoinGiftView.Show();
            catCoinGiftView.SetData(datas);
        }

        private void ShowQuizTab()
        {
            if (!UIToggle.current.value)
                return;

            RefreshQuiz();
        }

        private void ShowMultiMazeTab()
        {
            if (!UIToggle.current.value)
                return;

            RefreshMultiMazeQuest();
        }

        private void ShowGiftTab()
        {
            if (!UIToggle.current.value)
                return;

            RefreshGift();
        }

        void OnQuizReward(bool isCorrect, RewardData rewardData)
        {
            quizQuizResultView.Show();
            quizQuizResultView.SetData(isCorrect, rewardData);

            PlaySfx(isCorrect ? correctSound : incorrectSound);
        }

        private void OnNotInProgress()
        {
            UI.ShowToastPopup(LocalizeKey._5412.ToText()); // 현재 진행중이 아닙니다.
            //CloseUI();
            Refresh(); // 냥다래 선물 이벤트가 남아있을 수 있으므로.. 리프레시에서 체크
        }

        private void CloseUI()
        {
            UI.Close<UISpecialEvent>();
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

        bool IInspectorFinder.Find()
        {
            if (tab != null)
            {
                tabGrid = tab.GetComponent<UIGrid>();
            }
            return true;
        }

        #region Tutorial
        private bool isSelectMultiMazeQuestReward;

        public UIWidget GetBtnCompleteWidget()
        {
            return multiMazeQuestView.GetBtnCompleteWidget();
        }

        public UIWidget GetBtnConfirmWidget()
        {
            return popupView.GetBtnConfirm();
        }

        public bool IsSelectMultiMazeQuestReward()
        {
            if (isSelectMultiMazeQuestReward)
            {
                isSelectMultiMazeQuestReward = false;
                return true;
            }

            return false;
        }
        #endregion
    }
}