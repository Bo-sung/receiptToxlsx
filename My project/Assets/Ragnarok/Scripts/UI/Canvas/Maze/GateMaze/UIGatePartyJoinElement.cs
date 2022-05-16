using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIGatePartyJoinElement : UIElement<UIGatePartyJoinElement.IInput>
    {
        public interface IInput
        {
            UIGatePartyJoinSlot.IInput[] Inputs { get; }
        }

        [SerializeField] UIGatePartyJoinSlot[] slots;

        public event UserModel.UserInfoEvent OnSelectUserInfo;
        public event UIGatePartyJoinSlot.JoinEvent OnSelectJoin;

        protected override void Awake()
        {
            base.Awake();

            foreach (var item in slots)
            {
                item.OnSelectUserInfo += OnUserInfo;
                item.OnSelectJoin += OnJoin;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in slots)
            {
                item.OnSelectUserInfo -= OnUserInfo;
                item.OnSelectJoin -= OnJoin;
            }
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            int inputLength = info.Inputs == null ? 0 : info.Inputs.Length;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].SetData(i < inputLength ? info.Inputs[i] : null);
            }
        }

        void OnUserInfo(int uid, int cid)
        {
            OnSelectUserInfo?.Invoke(uid, cid);
        }

        void OnJoin(int channelId)
        {
            OnSelectJoin?.Invoke(channelId);
        }
    }
}