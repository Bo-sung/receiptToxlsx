using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UITimePatrolReward : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] RewardListView normalView;
        [SerializeField] RewardListView bossView;
        [SerializeField] UILabelHelper labelNormalTitle;
        [SerializeField] UILabelHelper labelBossTitle;
        [SerializeField] UILabelHelper labelNotice;

        TimePatrolRewardPresenter presenter;

        protected override void OnInit()
        {
            presenter = new TimePatrolRewardPresenter();
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
            normalView.SetData(presenter.GetNormalRewards());
            bossView.SetData(presenter.GetBossRewards());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._48263; // 타임패트롤 보상
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인
            labelNormalTitle.LocalKey = LocalizeKey._48264; // 일반 보상
            labelBossTitle.LocalKey = LocalizeKey._48265; // 타임키퍼 보상
            labelNotice.LocalKey = LocalizeKey._48266; // 타임패트롤의 Lv이 높아질 수록 아이템 드랍 확률이 높아집니다.
        }
    }
}