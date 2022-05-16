using UnityEngine;

namespace Ragnarok
{
    public class UIChatMessage : UIInfo<ChatPresenter, ChatInfo>, IAutoInspectorFinder
    {
        [SerializeField] UILabel labelSize;

        [SerializeField] UILabel labelName_MyChat;
        [SerializeField] UILabel labelMessage_MyChat;
        [SerializeField] UITextureHelper iconThumbnail_MyChat;

        [SerializeField] UILabel labelName_OtherChat;
        [SerializeField] UILabel labelMessage_OtherChat;
        [SerializeField] UITextureHelper iconThumbnail_OtherChat;

        [SerializeField] UILabel labelMessage_SystemChat;

        [SerializeField] UILabel labelName_GM;
        [SerializeField] UILabel labelMessage_GM;

        [SerializeField] GameObject goMyChatBase;
        [SerializeField] GameObject goOtherChatBase;
        [SerializeField] GameObject goSystemChatBase;
        [SerializeField] GameObject goGMChatBase;

        [SerializeField] UIButtonHelper btnSelf;
        [SerializeField] UIDragEvent dragEvent;

        public enum Mode
        {
            MY_CHAT = 1,
            OTHER_CHAT,
            SYSTEM,
            GM,
        }

        private Mode mode;

        public ChatInfo Info => info;

        protected override void Start()
        {
            base.Start();

            EventDelegate.Add(btnSelf.OnClick, OnClickedBtnSelf);

            dragEvent.OnDragEvent += OnDragSlide;
        }

        protected override void OnDestroy()
        {
            EventDelegate.Remove(btnSelf.OnClick, OnClickedBtnSelf);

            dragEvent.OnDragEvent -= OnDragSlide;

            base.OnDestroy();
        }

        void OnDragSlide(UIDragEvent.DragArrow dragArrow)
        {
            if (!isActiveAndEnabled)
                return;

            presenter.DragEvent(dragArrow);
        }

        public override void SetData(ChatPresenter presenter, ChatInfo info)
        {
            // Mode 먼저 설정
            bool isMine = presenter.GetPlayerCID().Equals(info.cid);

            if (info.isGMMsg)
            {
                mode = Mode.GM;
            }
            else if (isMine)
            {
                mode = Mode.MY_CHAT;
            }
            else
            {
                mode = Mode.OTHER_CHAT;
            }

            base.SetData(presenter, info);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            goMyChatBase.SetActive(mode == Mode.MY_CHAT);
            goOtherChatBase.SetActive(mode == Mode.OTHER_CHAT);
            goSystemChatBase.SetActive(mode == Mode.SYSTEM);
            goGMChatBase.SetActive(mode == Mode.GM);

            if (mode == Mode.MY_CHAT)
            {
                labelName_MyChat.text = info.name;
                iconThumbnail_MyChat.Set(info.thumbnailName);

                labelSize.text = info.message;
                labelSize.ProcessText();

                int width;
                if (labelSize.finalFontSize < labelSize.fontSize) // over되어 사이즈가 줄어듦
                {
                    width = labelSize.overflowWidth; // 가로는 최대 사이즈
                }
                else
                {
                    width = Mathf.Min(labelSize.width, labelSize.overflowWidth); // 가로는 최대 사이즈를 넘길 수 음슴
                }

                labelMessage_MyChat.SetDimensions(width, 0);
                labelMessage_MyChat.text = info.message;
            }

            if (mode == Mode.OTHER_CHAT)
            {
                labelName_OtherChat.text = info.name;
                iconThumbnail_OtherChat.Set(info.thumbnailName);

                labelSize.text = info.message;
                labelSize.ProcessText();

                int width;
                if (labelSize.finalFontSize < labelSize.fontSize) // over되어 사이즈가 줄어듦
                {
                    width = labelSize.overflowWidth; // 최대 사이즈로 맞춤
                }
                else
                {
                    width = Mathf.Min(labelSize.width, labelSize.overflowWidth);
                }

                labelMessage_OtherChat.SetDimensions(width, 0);
                labelMessage_OtherChat.text = info.message;
            }

            if (mode == Mode.SYSTEM)
            {
                labelMessage_SystemChat.text = info.message;
            }

            if (mode == Mode.GM)
            {
                labelName_GM.text = LocalizeKey._29010.ToText();

                labelSize.text = info.message;
                labelSize.ProcessText();

                int width;
                if (labelSize.finalFontSize < labelSize.fontSize) // over되어 사이즈가 줄어듦
                {
                    width = labelSize.overflowWidth; // 최대 사이즈로 맞춤
                }
                else
                {
                    width = Mathf.Min(labelSize.width, labelSize.overflowWidth);
                }

                labelMessage_GM.SetDimensions(width, 0);
                labelMessage_GM.text = info.message;
            }
        }

        void OnClickedBtnSelf()
        {
            if (mode != Mode.OTHER_CHAT || info is null)
                return;

            presenter.OnClickedChatMessage(this);
        }

        public UILabel GetLabelName()
        {
            return labelName_OtherChat;
        }
    }
}