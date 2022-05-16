using UnityEngine;

namespace Ragnarok
{
    public class HudCharacterInfo : HudUnitInfo
    {
        [SerializeField] UIButtonHelper btnChat;

        public event System.Action OnChat;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnChat.OnClick, OnClickedBtnChat);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnChat.OnClick, OnClickedBtnChat);
        }

        void OnClickedBtnChat()
        {
            OnChat?.Invoke();
        }
    }
}