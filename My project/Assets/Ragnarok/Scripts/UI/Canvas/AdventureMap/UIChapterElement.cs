using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIChapterElement : UIElement<UIChapterElement.IInput>
    {
        public interface IInput
        {
            int Chapter { get; }
            int LocalKey { get; }
            string IconName { get; }
            bool IsLock { get; }
            bool IsSelected { get; }
            bool IsOpened { get; }
            bool HasNotice { get; }

            event System.Action OnUpdateSelect;
            event System.Action OnUpdateOpen;
            event System.Action OnUpdateNotice;
        }

        [SerializeField] UIButtonHelper button;
        [SerializeField] SwitchLocalScale scale;
        [SerializeField] UITextureHelper active, deactive;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(button.OnClick, OnClickedBtn);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(button.OnClick, OnClickedBtn);
        }

        void OnClickedBtn()
        {
            if (info.IsLock)
            {
                UI.ShowToastPopup(LocalizeKey._90045.ToText()); // 업데이트 예정입니다.
                return;
            }

            OnSelect?.Invoke(info.Chapter);
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            info.OnUpdateSelect += RefreshSelect;
            info.OnUpdateOpen += RefreshOpen;
            info.OnUpdateNotice += RefreshNotice;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            info.OnUpdateSelect -= RefreshSelect;
            info.OnUpdateOpen -= RefreshOpen;
            info.OnUpdateNotice -= RefreshNotice;
        }

        protected override void OnLocalize()
        {
            UpdateChapterName();
        }

        protected override void Refresh()
        {
            active.SetAdventure(info.IconName);
            deactive.SetAdventure(info.IconName);

            UpdateChapterName();
            RefreshSelect();
            RefreshOpen();
            RefreshNotice();
        }

        private void UpdateChapterName()
        {
            if (info == null)
            {
                button.Text = string.Empty;
                return;
            }

            button.LocalKey = info.LocalKey;
        }

        private void RefreshSelect()
        {
            scale.Switch(info.IsSelected);
        }

        private void RefreshOpen()
        {
            NGUITools.SetActive(deactive.cachedGameObject, !info.IsOpened);
        }

        private void RefreshNotice()
        {
            button.SetNotice(info.HasNotice);
        }
    }
}