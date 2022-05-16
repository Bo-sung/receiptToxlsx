using Ragnarok.View.League;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UILeague"/>
    /// </summary>
    public interface ILeagueCanvas : ICanvas
    {
        /// <summary>
        /// 리그 메인 정보 세팅
        /// </summary>
        void SetData(UILeagueMainInfo.IInput input);

        /// <summary>
        /// 리그 랭킹 정보 세팅
        /// </summary>
        void SetData(UILeagueRankInfo.IInput input);

        /// <summary>
        /// 받은 보상 표시
        /// </summary>
        void ShowRewardPopup(LeagueResultPopupView.IInput input);

        void CloseUI();
    }
}