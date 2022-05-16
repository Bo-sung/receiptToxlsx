using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildEmblem"/>
    /// </summary>
    public class GuildEmblemPresenter : ViewPresenter
    {
        public interface IView
        {
            void SetEmblem(int background, int frame, int icon);
            void SetConfrim(string name);
        }

        private readonly GuildModel guildModel;
        private readonly IView view;

        int emblemBackground;
        int emblemFrame;
        int emblemIcon;

        public GuildEmblemPresenter(IView view)
        {
            this.view = view;
            guildModel = Entity.player.Guild;
        }

        public override void AddEvent()
        {
            guildModel.OnUpdateGuildState += OnUpdateGuildState;
        }

        public override void RemoveEvent()
        {
            guildModel.OnUpdateGuildState -= OnUpdateGuildState;
        }

        public void SetView()
        {
            view.SetEmblem(emblemBackground, emblemFrame, emblemIcon);
            if (guildModel.HaveGuild)
            {
                view.SetConfrim(LocalizeKey._33078.ToText()); // 변경
            }
            else
            {
                view.SetConfrim(LocalizeKey._33077.ToText()); // 확 인
            }
        }

        public void SetEmblemBackground(byte index)
        {
            emblemBackground = index;
            SetView();
        }

        public void SetEmblemFrame(byte index)
        {
            emblemFrame = index;
            SetView();
        }

        public void SetEmblemIcon(byte index)
        {
            emblemIcon = index;
            SetView();
        }

        public async void OnClickedBtnConfirm()
        {
            guildModel.SelectEmblemBg = (byte)emblemBackground;
            guildModel.SelectEmblemFrame = (byte)emblemFrame;
            guildModel.SelectEmblemIcon = (byte)emblemIcon;

            if (guildModel.HaveGuild)
            {
                await guildModel.RequestChangeEmblem();
            }
            else
            {
                UI.Close<UIGuildEmblem>();
                UI.Show<UIGuildCreate>();
            }
        }

        void OnUpdateGuildState()
        {
            if (guildModel.HaveGuild)
                UI.Close<UIGuildEmblem>();
        }
    }
}