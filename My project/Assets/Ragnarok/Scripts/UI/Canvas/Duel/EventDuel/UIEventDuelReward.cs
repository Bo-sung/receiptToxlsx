using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEventDuelReward : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        private const int SERVER_REWARD = 0; // 서버 보상
        private const int WORLD_REWARD = 1; // 개인 보상(전체 서버)
        private const int REWARD = 2; // 개인 보상(내 서버)

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UITabHelper tab;
        [SerializeField] EventDuelServerRewardView eventDuelServerRewardView;
        [SerializeField] EventDuelRewardView eventDuelRewardView;
        [SerializeField] UIButtonHelper btnConfirm;

        EventDuelRewardPresenter presenter;

        protected override void OnInit()
        {
            presenter = new EventDuelRewardPresenter();

            EventDelegate.Add(btnExit.onClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
            tab.OnSelect += OnSelectTab;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnExit.onClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
            tab.OnSelect -= OnSelectTab;
        }

        protected override void OnShow(IUIData data = null)
        {
            tab[SERVER_REWARD].Set(false, false);
            tab.Value = SERVER_REWARD;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._47905; // 보상 안내
            tab[SERVER_REWARD].LocalKey = LocalizeKey._47906; // 서버 보상
            tab[WORLD_REWARD].LocalKey = LocalizeKey._47907; // 개인 보상\n(전체 서버)
            tab[REWARD].LocalKey = LocalizeKey._47908; // 개인 보상\n(내 서버)
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
        }

        void OnSelectTab(int index)
        {
            eventDuelServerRewardView.SetActive(index == SERVER_REWARD);
            eventDuelRewardView.SetActive(index == WORLD_REWARD || index == REWARD);

            switch (index)
            {
                case SERVER_REWARD:
                    eventDuelServerRewardView.SetData(presenter.GetPerfectBuffReward(), presenter.GetBuffRewards());
                    break;

                case WORLD_REWARD:
                    eventDuelRewardView.SetData(presenter.GetWorldServerRewards());
                    break;

                case REWARD:
                    eventDuelRewardView.SetData(presenter.GetServerRewards());
                    break;
            }
        }

        private void CloseUI()
        {
            UI.Close<UIEventDuelReward>();
        }
    }
}