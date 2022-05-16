using UnityEngine;

namespace Ragnarok
{
    public class PrivateStoreChatSlot : UIInfo<PrivateStorePresenter, ChatModel.ISimpleChatInput>
    {
        [SerializeField] GameObject goMyChatBase;
        [SerializeField] GameObject goOthersChatBase;

        [SerializeField] UILabelHelper labelNickname_Other;
        [SerializeField] UILabelHelper labelNickname_Mine;
        [SerializeField] UILabelHelper labelMessage_Other;
        [SerializeField] UILabelHelper labelMessage_Mine;

        public enum ChatType
        {
            MINE,
            OTHERS,
        }

        private ChatType chatType;

        protected override void Refresh()
        {
            chatType = (info.CID == presenter.MyCID) ? ChatType.MINE : ChatType.OTHERS;

            goMyChatBase.SetActive(chatType == ChatType.MINE);
            goOthersChatBase.SetActive(chatType == ChatType.OTHERS);

            switch (chatType)
            {
                case ChatType.MINE:
                    labelMessage_Mine.Text = info.Message;
                    labelNickname_Mine.uiLabel.color = GetChatNameColor(chatType);
                    labelNickname_Mine.Text = presenter.MyName;
                    break;
                case ChatType.OTHERS:
                    bool isSeller = (info.CID == presenter.CID);
                    if (isSeller)
                    {
                        labelNickname_Other.uiLabel.color = GetChatNameColor(chatType, isSeller);
                        labelNickname_Other.Text = StringBuilderPool.Get()
                                            .Append(info.Nickname)
                                            .Append($" ({LocalizeKey._45300.ToText()})")
                                            .Release(); // 닉네임 (판매자)
                    }
                    else
                    {
                        labelNickname_Other.uiLabel.color = GetChatNameColor(chatType, isSeller);
                        labelNickname_Other.Text = info.Nickname;
                    }
                    labelMessage_Other.Text = info.Message;
                    break;
                default:
                    break;
            }
        }

        private Color GetChatNameColor(ChatType chatter, bool isSeller = false)
        {
            switch (chatter)
            {
                case ChatType.MINE:
                    return new Color32(0x6a, 0x8f, 0xde, 0xff);

                case ChatType.OTHERS:
                    if (isSeller)
                    {
                        return new Color32(0x44, 0xa0, 0xff, 0xff);
                    }

                    return new Color32(0x77, 0x77, 0x77, 0xff);
            }
            return Color.black;
        }
    }


}