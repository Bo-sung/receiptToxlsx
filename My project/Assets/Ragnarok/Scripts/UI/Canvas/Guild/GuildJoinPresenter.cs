using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class GuildJoinPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
        }

        private readonly IView view;
        private readonly GuildModel guildModel;

        public GuildJoinPresenter(IView view)
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

        /// <summary>
        /// 추천 길드 목록 반환
        /// </summary>
        /// <returns></returns>
        public GuildSimpleInfo[] GetRecommends()
        {
            return guildModel.GetGuildRecommends();
        }

        /// <summary>
        /// 가입 신청한 길드 목록 반환
        /// </summary>
        /// <returns></returns>
        public GuildSimpleInfo[] GetGuildRequests()
        {
            return guildModel.GetGuildRequests();
        }

        /// <summary>
        /// 가입 신청한 길드 수
        /// </summary>
        public int GuildRequestCount => guildModel.GuildJoinSubmitCount;

        public async Task RequestGuildJoinSubmitList()
        {
            await guildModel.RequestJoinSubmitGuildList();
        }

        /// <summary>
        /// 길드 가입 신청
        /// </summary>
        /// <param name="info"></param>
        public async void RequestGuildJoin(GuildSimpleInfo info)
        {
            bool isSuccess = await guildModel.RequestGuildJoin(info);
            if (isSuccess && info.IsAutoJoin)
            {
                UI.Close<UIGuildJoin>();
                UI.Show<UIGuildMain>();
                return;
            }
            view.Refresh();
        }

        /// <summary>
        /// 추천 길드 목록 재갱신
        /// </summary>
        public async void RequestGuildRecommend()
        {
            await guildModel.RequestGuildRandom();
            view.Refresh();
        }

        /// <summary>
        /// 길드 검색
        /// </summary>
        public async void RequestGuildSearch(string guildName)
        {
            await guildModel.RequestGuildSearch(guildName);
            view.Refresh();
        }

        /// <summary>
        /// 길드 신청 취소
        /// </summary>
        /// <param name="info"></param>
        public async void RequstGuildJoinCancel(GuildSimpleInfo info)
        {
            await guildModel.RequestGuildJoinCancel(info);
            view.Refresh();
        }

        void OnUpdateGuildState()
        {
            if (guildModel.HaveGuild)
            {
                UI.Close<UIGuildJoin>();
            }
        }
    }
}
