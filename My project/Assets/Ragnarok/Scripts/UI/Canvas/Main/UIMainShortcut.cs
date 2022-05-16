using Ragnarok.View.Main;
using UnityEngine;

namespace Ragnarok
{
    public class UIMainShortcut : UICanvas
    {
        public enum MenuContent
        {
            Event = 1,
            Quest,
            Mail,
            Map,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIMainQuickMenuView mainQuickMenuView;

        MainShortcutPresenter presenter;

        protected override void OnInit()
        {
            presenter = new MainShortcutPresenter();

            mainQuickMenuView.OnSelect += OnSelectContent;

            presenter.OnUpdateQuestMainRewardState += UpdateEventNotice;
            presenter.OnUpdateTreeRewardState += UpdateEventNotice;
            presenter.OnUpdateBingoQuestRewardState += UpdateEventNotice;
            presenter.OnUpdateAlarm += UpdateMailNotice;
            presenter.OnUpdateQuestNotice += UpdateQuestNotice;
            presenter.OnUpdateNewOpenContent += UpdateNewIcon;
            presenter.OnUpdateEventNotice += UpdateEventNotice;
            presenter.OnUpdateEventStageInfo += UpdateMapNotice;
            presenter.OnUpdateEventStageCount += UpdateMapNotice;
            presenter.OnUpdateClearedStage += UpdateMapNotice;
            presenter.OnUpdateDarkTree += UpdateEventNotice;
            presenter.OnUpdateAttendEventReward += UpdateEventNotice;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            mainQuickMenuView.OnSelect -= OnSelectContent;

            presenter.OnUpdateQuestMainRewardState -= UpdateEventNotice;
            presenter.OnUpdateTreeRewardState -= UpdateEventNotice;
            presenter.OnUpdateBingoQuestRewardState -= UpdateEventNotice;
            presenter.OnUpdateAlarm -= UpdateMailNotice;
            presenter.OnUpdateQuestNotice -= UpdateQuestNotice;
            presenter.OnUpdateNewOpenContent -= UpdateNewIcon;
            presenter.OnUpdateEventNotice -= UpdateEventNotice;
            presenter.OnUpdateEventStageInfo -= UpdateMapNotice;
            presenter.OnUpdateEventStageCount -= UpdateMapNotice;
            presenter.OnUpdateClearedStage -= UpdateMapNotice;
            presenter.OnUpdateDarkTree -= UpdateEventNotice;
            presenter.OnUpdateAttendEventReward -= UpdateEventNotice;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateNotice();
            UpdateNewIcon();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnSelectContent(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Event:
                    UI.Show<UIDailyCheck>();
                    break;

                case MenuContent.Quest:
                    UI.Show<UIQuest>();
                    break;

                case MenuContent.Mail:
                    if (GameServerConfig.IsOnBuff())
                    {
                        UI.Show<UIMailOnBuff>().Set(tabIndex: 0);
                    }
                    else
                    {
                        UI.Show<UIMail>().Set(tabIndex: 0);
                    }
                    break;

                case MenuContent.Map:
                    UI.Show<UIAdventureMap>();
                    break;
            }
        }

        private void UpdateNotice()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UpdateNotice(item);
            }
        }

        private void UpdateNewIcon()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                mainQuickMenuView.SetNewIcon(item, presenter.GetHasNewIcon(item));
            }
        }

        private void UpdateEventNotice()
        {
            UpdateNotice(MenuContent.Event);
        }

        private void UpdateMailNotice()
        {
            UpdateNotice(MenuContent.Mail);
        }

        private void UpdateQuestNotice()
        {
            UpdateNotice(MenuContent.Quest);
        }

        private void UpdateMapNotice()
        {
            UpdateNotice(MenuContent.Map);
        }

        private void UpdateNotice(MenuContent content)
        {
            mainQuickMenuView.SetNotice(content, presenter.GetHasNotice(content));
        }

        public UISprite GetMenuIcon(MenuContent content)
        {
            UIButtonWithLock button = mainQuickMenuView.GetButton(content);

#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return null;
            }
#endif

            return button.GetIcon();
        }

        public Vector3 GetPosition(MenuContent content)
        {
            return mainQuickMenuView.GetPosition(content);
        }

        public void PlayTweenEffect(MenuContent content)
        {
            // Do Nothing
        }
    }
}