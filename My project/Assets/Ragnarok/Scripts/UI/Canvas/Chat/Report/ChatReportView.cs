using UnityEngine;

namespace Ragnarok.View
{
    public class ChatReportView : UIView
    {
        [SerializeField] UILabelHelper labelNickName;
        [SerializeField] UITabHelper tab;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            tab.OnSelect += OnSelectTab;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            tab.OnSelect -= OnSelectTab;
        }

        protected override void OnLocalize()
        {
            tab[0].LocalKey = LocalizeKey._6202; // 부적절한 언행
            tab[1].LocalKey = LocalizeKey._6203; // 채팅 도배
            tab[2].LocalKey = LocalizeKey._6204; // 현금 거래 유도
        }

        public void SetName(string nickName)
        {
            labelNickName.Text = LocalizeKey._6201.ToText()
                .Replace(ReplaceKey.NAME, nickName); // [7F7D7F]대상 유저 :[-] [84A2EC]{NAME}[-]
        }

        private void OnSelectTab(int tabIndex)
        {
            OnSelect?.Invoke(tabIndex);
        }

        public void ResetTab()
        {
            tab.Value = -1;
        }
    }
}