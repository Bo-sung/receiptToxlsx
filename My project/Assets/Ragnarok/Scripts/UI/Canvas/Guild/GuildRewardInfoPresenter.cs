using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildRewardInfo"/>
    /// </summary>
    public class GuildRewardInfoPresenter : ViewPresenter
    {
        public interface IView
        {
            void SetScrollView();
            void SetAttendCount(int count);
        }

        private readonly IView view;
        private readonly GuildModel guildModel;

        public GuildRewardInfoPresenter(IView view)
        {
            this.view = view;
            guildModel = Entity.player.Guild;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void Refresh()
        {
            view.SetScrollView();
            view.SetAttendCount(guildModel.YesterdayMemberCount);
        }

        public GuildAttendRewardInfo[] GetGuildAttendRewardInfos()
        {
            return guildModel.guildAttendRewardInfos.ToArray();
        }
    }
}
