using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildLobby : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] GuildLobbyView guildLobbyView;

        GuildLobbyPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildLobbyPresenter();

            guildLobbyView.OnSelectContent += OnSelectGuildContent;

            presenter.OnUpdateGuildAttackStartTime += Refresh;
            presenter.OnUpdateCreateEmperium += Refresh;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateGuildAttackStartTime -= Refresh;
            presenter.OnUpdateCreateEmperium -= Refresh;

            guildLobbyView.OnSelectContent -= OnSelectGuildContent;
        }

        protected override void OnShow(IUIData data = null)
        {
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        private void Refresh()
        {
            GuildContentInfo info = new GuildContentInfo
            {
                NameId = LocalizeKey._38002, // 길드 습격
                StartTime = presenter.GetStartTime(), // Comming Soon...
            };

            guildLobbyView.SetData(info);
        }

        void OnSelectGuildContent()
        {
            UI.Show<UIGuildAttack>();
        }

        private class GuildContentInfo : UIGuildContent.IInput
        {
            public int NameId { get; set; }
            public System.DateTime StartTime { get; set; }
        }
    }
}