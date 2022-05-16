using UnityEngine;

namespace Ragnarok.View.Main
{
    public class UIMainQuickMenuView : UIView
    {
        [SerializeField] UIButtonWithLock btnEvent, btnQuest, btnMail, btnMap;

        public event System.Action<UIMainShortcut.MenuContent> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnEvent.OnClick, OnClickedBtnEvent);
            EventDelegate.Add(btnQuest.OnClick, OnClickedBtnQuest);
            EventDelegate.Add(btnMail.OnClick, OnClickedBtnMail);
            EventDelegate.Add(btnMap.OnClick, OnClickedBtnMap);
        }

        protected override void Start()
        {
            base.Start();

            NGUITools.SetLayer(gameObject, Layer.UI_ExceptForCharZoom);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEvent.OnClick, OnClickedBtnEvent);
            EventDelegate.Remove(btnQuest.OnClick, OnClickedBtnQuest);
            EventDelegate.Remove(btnMail.OnClick, OnClickedBtnMail);
            EventDelegate.Remove(btnMap.OnClick, OnClickedBtnMap);
        }

        protected override void OnLocalize()
        {
            foreach (UIMainShortcut.MenuContent item in System.Enum.GetValues(typeof(UIMainShortcut.MenuContent)))
            {
                UIButtonHelper button = GetButton(item);
                if (button == null)
                    continue;

                button.Text = GetButtonText(item);
            }
        }

        void OnClickedBtnEvent()
        {
            OnSelect?.Invoke(UIMainShortcut.MenuContent.Event);
        }

        void OnClickedBtnQuest()
        {
            OnSelect?.Invoke(UIMainShortcut.MenuContent.Quest);
        }

        void OnClickedBtnMail()
        {
            OnSelect?.Invoke(UIMainShortcut.MenuContent.Mail);
        }

        void OnClickedBtnMap()
        {
            OnSelect?.Invoke(UIMainShortcut.MenuContent.Map);
        }

        public void SetNotice(UIMainShortcut.MenuContent content, bool isNotice)
        {
            UIButtonWithLock button = GetButton(content);

#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(UIMainShortcut.MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return;
            }
#endif
            button.SetNotice(isNotice);
        }

        public void SetNewIcon(UIMainShortcut.MenuContent content, bool isNew)
        {
            UIButtonWithLock button = GetButton(content);

#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(UIMainShortcut.MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return;
            }
#endif

            button.SetActiveNew(isNew);
        }

        public UIButtonWithLock GetButton(UIMainShortcut.MenuContent content)
        {
            switch (content)
            {
                case UIMainShortcut.MenuContent.Event:
                    return btnEvent;

                case UIMainShortcut.MenuContent.Quest:
                    return btnQuest;

                case UIMainShortcut.MenuContent.Mail:
                    return btnMail;

                case UIMainShortcut.MenuContent.Map:
                    return btnMap;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIMainShortcut.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        private string GetButtonText(UIMainShortcut.MenuContent content)
        {
            switch (content)
            {
                case UIMainShortcut.MenuContent.Event:
                    return LocalizeKey._3008.ToText(); // 이벤트

                case UIMainShortcut.MenuContent.Quest:
                    return LocalizeKey._3007.ToText(); // 퀘스트

                case UIMainShortcut.MenuContent.Mail:
                    return LocalizeKey._3010.ToText(); // 우편함

                case UIMainShortcut.MenuContent.Map:
                    return LocalizeKey._2102.ToText(); // 지도
            }

            Debug.LogError($"[올바르지 않은 {nameof(UIMainShortcut.MenuContent)}] {nameof(content)} = {content}");
            return string.Empty;
        }

        public Vector3 GetPosition(UIMainShortcut.MenuContent content)
        {
            UIButtonWithLock button = GetButton(content);

#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(UIMainShortcut.MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return Vector3.zero;
            }
#endif

            return button.transform.position;
        }

        public void PlayTweenEffect(UIMainShortcut.MenuContent content)
        {
        }
    }
}