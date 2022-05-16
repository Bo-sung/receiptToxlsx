using UnityEngine;

namespace Ragnarok
{
    public class LobbyPrivateStoreBalloon : HUDObject
    {
        [SerializeField] UILabel label;
        [SerializeField] UIButtonHelper btnBalloon;

        public event System.Action OnSelect;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnBalloon.OnClick, OnClickBalloon);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnBalloon.OnClick, OnClickBalloon);
        }

        public void SetComment(string comment)
        {
            label.text = comment;
        }

        void OnClickBalloon()
        {
            OnSelect?.Invoke();
        }
    }
}