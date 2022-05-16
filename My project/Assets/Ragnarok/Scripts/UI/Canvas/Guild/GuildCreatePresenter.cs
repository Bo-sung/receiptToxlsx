using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildCreate"/>
    /// </summary>
    public class GuildCreatePresenter : ViewPresenter
    {
        public interface IView
        {
        }

        private readonly GuildModel guildModel;

        private readonly IView view;

        public GuildCreatePresenter(IView view)
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

        public byte EmblemBg => guildModel.SelectEmblemBg;
        public byte EmblemFrame => guildModel.SelectEmblemFrame;
        public byte EmblemIcon => guildModel.SelectEmblemIcon;

        /// <summary>
        /// 길드 생성 요청
        /// </summary>
        /// <param name="guildName"></param>
        /// <param name="guildIntroduction"></param>        
        public async void RequestGuildCreate(string guildName, string guildIntroduction)
        {
            await guildModel.RequestGuildCreate(guildName, guildIntroduction);            
        }

        void OnUpdateGuildState()
        {
            if (guildModel.HaveGuild)
                UI.Close<UIGuildCreate>();
        }
    } 
}
