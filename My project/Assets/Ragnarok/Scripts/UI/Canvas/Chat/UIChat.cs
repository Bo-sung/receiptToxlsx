using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIChat : UICanvas, ChatPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;
        public override int layer => Layer.UI_Chatting;

        // Top
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnChannel;
        [SerializeField] UIButtonHelper btnSlide; // 슬라이드 버튼 (채팅모드 변경탭 열고 닫기)
        [SerializeField] GameObject goChangeChannel; // 채널변경 아이콘

        // Center
        [SerializeField] ChatView chatView;
        [SerializeField] UILabelHelper labelNotice; // 공지사항

        [SerializeField] UIWidget chatBaseAnchor; // 채팅뷰 슬라이드
        [SerializeField] GameObject chatBaseMode_Default;
        [SerializeField] GameObject chatBaseMode_Slide;

        // Bottom
        [SerializeField] UIInput input;
        [SerializeField] UIButtonHelper btnSend;

        // Extra
        [SerializeField] ChatModeView chatModeView;
        [SerializeField] UIDragEvent[] dragEvent;
        [SerializeField] UIButtonHelper btnUserInfoPopupOutBound; // 다른 유저 기능 메뉴 바깥영역
        [SerializeField] UIChatUserInfoPopup userInfoPopup; // 다른 유저 클릭 시 뜨는 메뉴

        private ChatPresenter presenter;
        private bool isChatModeSlide;

        public delegate void ChangeActivateEvent();

        public event ChangeActivateEvent OnChangeActivate;

        protected override void OnInit()
        {
            presenter = new ChatPresenter(this);

            presenter.AddEvent();
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(input.onChange, RefreshBtnSend);
            EventDelegate.Add(input.onSubmit, OnClickedBtnSend);
            EventDelegate.Add(btnSend.OnClick, OnClickedBtnSend);
            EventDelegate.Add(btnChannel.OnClick, OnClickedBtnChannel);
            EventDelegate.Add(btnSlide.OnClick, OnClickedBtnSlide);
            EventDelegate.Add(btnUserInfoPopupOutBound.OnClick, OnClickedBtnUserInfoPopupOutBound);


            chatView.Initialize(presenter);
            chatView.AddEvent();

            chatModeView.Initialize(presenter);
            chatModeView.AddEvent();

            foreach (var item in dragEvent)
            {
                item.OnDragEvent += OnDragEvent;
            }

            presenter.OnUpdateChatMode += OnUpdateChatMode;
            presenter.OnUpdateAllChatNotice += OnUpdateAllChatNotice;

            userInfoPopup.OnSelect += OnSelectUser;
            userInfoPopup.OnChat += OnChatUser;
            userInfoPopup.OnReport += OnReport;
            userInfoPopup.OnDisabled += OnUserInfoPopupDisabled;
        }

        protected override void OnClose()
        {
            presenter.OnUpdateChatMode -= OnUpdateChatMode;
            presenter.OnUpdateAllChatNotice -= OnUpdateAllChatNotice;

            foreach (var item in dragEvent)
            {
                item.OnDragEvent -= OnDragEvent;
            }

            presenter.RemoveEvent();
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(input.onChange, RefreshBtnSend);
            EventDelegate.Remove(input.onSubmit, OnClickedBtnSend);
            EventDelegate.Remove(btnSend.OnClick, OnClickedBtnSend);
            EventDelegate.Remove(btnChannel.OnClick, OnClickedBtnChannel);
            EventDelegate.Remove(btnSlide.OnClick, OnClickedBtnSlide);
            EventDelegate.Remove(btnUserInfoPopupOutBound.OnClick, OnClickedBtnUserInfoPopupOutBound);

            chatView.RemoveEvent();

            chatModeView.RemoveEvent();

            userInfoPopup.OnSelect -= OnSelectUser;
            userInfoPopup.OnChat -= OnChatUser;
            userInfoPopup.OnReport -= OnReport;
            userInfoPopup.OnDisabled -= OnUserInfoPopupDisabled;

            if (presenter != null)
                presenter = null;

            UI.AddMask(Layer.UI, Layer.UI_ExceptForCharZoom);
        }

        protected override void OnShow(IUIData data = null)
        {
            UI.RemoveMask(Layer.UI, Layer.UI_ExceptForCharZoom);
            isChatModeSlide = false;
            btnUserInfoPopupOutBound.SetActive(false);
            userInfoPopup.gameObject.SetActive(false);
            chatView.Show();
            chatModeView.Hide();
            Refresh();

            UpdateNotice(); // 공지사항 업데이트
        }

        public void Show(ChatMode mode, int whisperCid = default)
        {
            presenter.SetChatMode(mode, whisperCid);
            Show();
        }

        protected override void OnHide()
        {
            UI.AddMask(Layer.UI, Layer.UI_ExceptForCharZoom);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._29002; // 채팅
            input.defaultText = LocalizeKey._29001.ToText(); // 채팅 내용을 입력하세요
            btnSend.Text = LocalizeKey._29000.ToText(); // 전송
        }

        public void Refresh()
        {
            chatView.Resize(presenter.GetDataSize(), progress: 1);
            if (isChatModeSlide)
            {
                chatModeView.Resize(presenter.GetChatModeSize());
            }

            // 채널 라벨 세팅
            string channelString = string.Empty;
            switch (presenter.ChatMode)
            {
                case ChatMode.Channel: channelString = LocalizeKey._29004.ToText().Replace(ReplaceKey.VALUE, presenter.Channel); break; // 자유 채팅 / 채널 {VALUE}
                case ChatMode.Guild: channelString = LocalizeKey._29005.ToText().Replace(ReplaceKey.NAME, presenter.GetGuildName()); break; // 길드 채팅 / {NAME}
                case ChatMode.Lobby: channelString = LocalizeKey._29006.ToText().Replace(ReplaceKey.VALUE, Entity.player.ChatModel.LobbyChannel); break; // 거래소 채팅 / 채널 {VALUE}
                case ChatMode.Whisper: channelString = $"{Entity.player.ChatModel.CurrentWhisperInfo.nickname} ({MathUtils.CidToHexCode(Entity.player.ChatModel.CurrentWhisperInfo.cid)})"; break;
            }
            btnChannel.Text = channelString;

            // 채널 변경 아이콘
            goChangeChannel.SetActive(presenter.ChatMode == ChatMode.Channel);

            // 슬라이드 버튼 아이콘 (화살표)
            btnSlide.Flip = isChatModeSlide ? UIBasicSprite.Flip.Nothing : UIBasicSprite.Flip.Horizontally;

            // 채팅뷰 슬라이드
            if (isChatModeSlide)
            {
                chatBaseAnchor.SetAnchor(chatBaseMode_Slide, 0, 0, 0, 0);
                chatModeView.SetActive(true);
                chatModeView.Resize(presenter.GetChatModeSize());
            }
            else
            {
                chatBaseAnchor.SetAnchor(chatBaseMode_Default, 0, 0, 0, 0);
                chatModeView.SetActive(false);
            }

            // 노티스 아이콘 갱신
            btnSlide.SetNotice(presenter.HasNewChatting(allCheck:true));
            // 노티스 아이콘 제거
            presenter.DeactiveNotice();
        }

        public void CloseUI()
        {
            UI.Close<UIChat>();
        }

        private void UpdateNotice()
        {
            presenter.UpdateNotice().WrapNetworkErrors();
        }

        void OnClickedBtnSend()
        {
            string message = NGUIText.StripSymbols(input.value);
            if (string.IsNullOrWhiteSpace(message))
            {
                input.value = string.Empty; // 초기화
                UI.ShowToastPopup(LocalizeKey._29001.ToText()); // 채팅 내용을 입력하세요
                return;
            }

            presenter.SendChatMessage(message);

            input.value = string.Empty; // 초기화
            chatView.ResetPosition(); // 내가 보낼 경우에는 ResetPosition
        }

        private void RefreshBtnSend()
        {
            string message = NGUIText.StripSymbols(input.value);
            btnSend.IsEnabled = !string.IsNullOrEmpty(message);
        }

        private void OnClickedBtnChannel()
        {
            if (presenter.ChatMode != ChatMode.Channel)
                return;

            presenter.ChangeChannel().WrapNetworkErrors();
        }

        private void OnClickedBtnSlide()
        {
            isChatModeSlide = !isChatModeSlide;

            Refresh();
        }

        void OnUpdateChatMode()
        {
            isChatModeSlide = false;
        }

        private void OnDragEvent(UIDragEvent.DragArrow dragArrow)
        {
            switch (dragArrow)
            {
                case UIDragEvent.DragArrow.Right:
                    {
                        if (isChatModeSlide)
                        {
                            isChatModeSlide = false;
                            Refresh();
                        }
                    }
                    break;

                case UIDragEvent.DragArrow.Left:
                    if (!isChatModeSlide)
                    {
                        isChatModeSlide = true;
                        Refresh();
                    }
                    break;
            }
        }

        /// <summary>
        /// 정보 보기 버튼
        /// </summary>
        void OnSelectUser(int uid, int cid)
        {
            presenter.RequestOthersCharacterInfo(uid, cid);
        }

        /// <summary>
        /// 귓속말 보내기 버튼
        /// </summary>
        void OnChatUser(int uid, int cid, string nickname)
        {
            presenter.AddWhisperInfo(uid, cid, nickname);
            Show(ChatMode.Whisper, whisperCid: cid);
        }

        /// <summary>
        /// 유저 신고하기 버튼
        /// </summary>
        void OnReport(int uid, int cid, string nickname)
        {
            OnUserInfoPopupDisabled();
            UI.Show<UIChatReport>().Set(cid, nickname);
        }

        /// <summary>
        /// UIChatUserInfoPopup이 사라짐
        /// </summary>
        void OnUserInfoPopupDisabled()
        {
            btnUserInfoPopupOutBound.SetActive(false);
            userInfoPopup.gameObject.SetActive(false);
        }

        void OnClickedBtnUserInfoPopupOutBound()
        {
            btnUserInfoPopupOutBound.SetActive(false);
            userInfoPopup.gameObject.SetActive(false);
        }

        void OnUpdateAllChatNotice(bool isActive)
        {
            btnSlide.SetNotice(isActive);
        }

        void ChatPresenter.IView.SetNotice(string msg)
        {
            // 공지사항 내용 수정 
            labelNotice.Text = msg;
        }

        void ChatPresenter.IView.Drag(UIDragEvent.DragArrow dragArrow)
        {
            OnDragEvent(dragArrow);
        }

        void ChatPresenter.IView.ChatMessageClickEvent(UIChatMessage uiChatMessage)
        {
            btnUserInfoPopupOutBound.SetActive(true);
            userInfoPopup.SetData(uiChatMessage);
        }

    }
}