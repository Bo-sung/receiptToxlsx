using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildBattleReward : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        private const int TAB_ATTACK = 0; // 공격 보상
        private const int TAB_DEFENSE = 1; // 수비 보상
        private const int TAB_RANK = 2; // 랭킹 보상

        [SerializeField] PopupView popupView;
        [SerializeField] UITabHelper tab;
        [SerializeField] GuildBattleRewardView guildBattleRewardView;

        GuildBattleRewardPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildBattleRewardPresenter();

            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;
            tab.OnSelect += OnSelectTab;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            popupView.OnConfirm -= OnBack;
            popupView.OnExit -= OnBack;
            tab.OnSelect -= OnSelectTab;
        }

        protected override void OnShow(IUIData data = null)
        {
            guildBattleRewardView.SetData(null);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._33920; // 보상
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인

            tab[TAB_ATTACK].LocalKey = LocalizeKey._33921; // 공격 보상
            tab[TAB_DEFENSE].LocalKey = LocalizeKey._33922; // 수비 보상
            tab[TAB_RANK].LocalKey = LocalizeKey._33923; // 랭킹 보상
        }

        void OnSelectTab(int index)
        {
            guildBattleRewardView.SetTitleKey(GetTitleKey(index));
            guildBattleRewardView.SetNoticeKey(GetNoticeKey(index));
            guildBattleRewardView.SetData(GetRewards(index));
        }

        private int GetTitleKey(int index)
        {
            switch (index)
            {
                default:
                case TAB_ATTACK: return LocalizeKey._33924; // 입힌 피해량
                case TAB_DEFENSE: return LocalizeKey._33925; // 남은 엠펠리움 HP
                case TAB_RANK: return LocalizeKey._33926; // 순위
            }
        }

        private int GetNoticeKey(int index)
        {
            switch (index)
            {
                default:
                case TAB_ATTACK: return LocalizeKey._33927; // 특정 길드 엠펠리움에 입힌 피해량에 따른 보상 정보입니다.\n(길드전 플레이 후 캐릭터에게 즉시 지급됩니다.)
                case TAB_DEFENSE: return LocalizeKey._33928; // 길드전 종료 시 남아있는 엠펠리움 Hp에 따른 보상 정보입니다.\n(길드전 기간 종료 시 길드원 모두에게 우편함으로 지급됩니다.)
                case TAB_RANK: return LocalizeKey._33929; // 모든 길드원의 누적 피해량을 총합하여 랭킹에 반영합니다.\n(길드전 기간 종료 시 길드원 모두에게 우편함으로 지급됩니다.)
            }
        }

        private UIRankRewardElement.IInput[] GetRewards(int index)
        {
            switch (index)
            {
                default:
                case TAB_ATTACK: return presenter.GetAttackRewards();
                case TAB_DEFENSE: return presenter.GetDefenseRewards();
                case TAB_RANK: return presenter.GetRankRewards();
            }
        }
    }
}