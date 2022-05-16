namespace Ragnarok
{
    public class ChatReportPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly ChatModel chatModel;

        private int cid;
        private int tabIndex;

        public ChatReportPresenter()
        {
            chatModel = Entity.player.ChatModel;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void SetCid(int cid)
        {
            this.cid = cid;
        }

        public void SetTabIndex(int tabIndex)
        {
            this.tabIndex = tabIndex;
        }

        /// <summary>
        /// 유저 신고 요청
        /// </summary>
        public void RequestChatReport()
        {
            // 신고 항목 미선택
            if (tabIndex == -1)
            {
                UI.ConfirmPopup(LocalizeKey._90259.ToText()); // 신고 할 항목을 선택해주세요.
                return;
            }
            chatModel.RequestChatReport(cid, TabIndexToReasonType()).WrapNetworkErrors();
        }

        /// <summary>
        /// 선택한 탭 신고이유 타입으로 변환
        /// </summary>
        /// <returns></returns>
        private byte TabIndexToReasonType()
        {
            switch (tabIndex)
            {
                case 0:
                    return 1; // 부적절한 언어
                case 1:
                    return 3; // 민폐, 괴롭힘
                case 2:
                    return 5; // 현금거래
            }
            return default;
        }
    }
}