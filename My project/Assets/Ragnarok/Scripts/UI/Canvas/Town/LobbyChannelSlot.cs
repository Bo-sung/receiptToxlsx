using UnityEngine;

namespace Ragnarok
{
    public class LobbyChannelSlot : UIInfo<TownPresenter, LobbyChannelInfo>
    {
        [SerializeField] UIButtonHelper btnChannel;
        [SerializeField] UILabelHelper labelChannelCount;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnChannel.OnClick, OnClickElement);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnChannel.OnClick, OnClickElement);
        }

        protected override void Refresh()
        {
            btnChannel.Text = LocalizeKey._3032.ToText().Replace(ReplaceKey.VALUE, info.channel); // 채널{VALUE}

            labelChannelCount.Text = LocalizeKey._3033.ToText().Replace(ReplaceKey.VALUE, info.playerCount).Replace(ReplaceKey.MAX, info.maxPlayerCount); // {VALUE}/{MAX}
        }

        // 클릭 시 해당 채널로 이동
        void OnClickElement()
        {
            presenter.GoToPersonalShop(info.channel);
        }
    }
}