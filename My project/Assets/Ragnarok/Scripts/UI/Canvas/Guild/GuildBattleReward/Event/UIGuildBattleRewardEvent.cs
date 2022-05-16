using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildBattleRewardEvent : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] GuildBattleRewardView guildBattleRewardView;

        GuildBattleRewardEventPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildBattleRewardEventPresenter();

            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            popupView.OnConfirm -= OnBack;
            popupView.OnExit -= OnBack;
        }

        protected override void OnShow(IUIData data = null)
        {
            guildBattleRewardView.SetData(presenter.GetEventRewards());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._33920; // 보상
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인

            guildBattleRewardView.SetTitleKey(LocalizeKey._33926); // 순위
            guildBattleRewardView.SetNoticeKey(LocalizeKey._33929); // 모든 길드원의 누적 피해량을 총합하여 랭킹에 반영합니다.\n(길드전 기간 종료 시 길드원 모두에게 우편함으로 지급됩니다.)
        }
    }
}