using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildDismissal : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] GuildDismissalView guildDismissalView;

        GuildDismissalPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildDismissalPresenter();

            guildDismissalView.OnSelectExit += CloseUi;
            guildDismissalView.OnSelectMasterDismissal += presenter.RequestMasterDismissal;

            presenter.OnSuccessGuildMasterGet += CloseUi;
            presenter.AddEvent();

            guildDismissalView.Initialize(presenter.guildMasterDismissalDays);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            presenter.OnSuccessGuildMasterGet -= CloseUi;

            guildDismissalView.OnSelectExit -= CloseUi;
            guildDismissalView.OnSelectMasterDismissal -= presenter.RequestMasterDismissal;
        }

        protected override void OnShow(IUIData data = null)
        {
            bool canMasterDismissal = presenter.CanMasterDismissal();
            guildDismissalView.SetMasterDismissal(canMasterDismissal);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        private void CloseUi()
        {
            UI.Close<UIGuildDismissal>();
        }
    }
}