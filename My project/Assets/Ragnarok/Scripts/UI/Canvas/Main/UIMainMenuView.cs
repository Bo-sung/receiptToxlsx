using UnityEngine;

namespace Ragnarok.View.Main
{
    public class UIMainMenuView : UIView
    {
        [SerializeField] UIMainButton btnMenu, btnCharInfo, btnInven, btnMake, btnShop, btnChat;
        [SerializeField] UIWidget menuWidget;
        [SerializeField] UIWidget invenWidget;
        [SerializeField] UIWidget makeBtnWidget;

        public event System.Action<UIMain.MenuContent> OnSelect;
        public event System.Action<UIMain.MenuContent> OnUnselect;

        private UIMain.MenuContent currentContent;

        UIManager uiManager;

        public UIWidget InvenWidget => invenWidget;
        public UIWidget MenuWidget => menuWidget;
        public UIWidget MakeBtn => makeBtnWidget;

        protected override void Awake()
        {
            base.Awake();

            uiManager = UIManager.Instance;

            EventDelegate.Add(btnMenu.OnClick, OnClickedBtnMenu);
            EventDelegate.Add(btnCharInfo.OnClick, OnClickedBtnCharInfo);
            EventDelegate.Add(btnInven.OnClick, OnClickedBtnInven);
            EventDelegate.Add(btnMake.OnClick, OnClickedBtnMake);
            EventDelegate.Add(btnShop.OnClick, OnClickedBtnShop);
            EventDelegate.Add(btnChat.OnClick, OnClickedBtnChat);

            uiManager.OnUIShow += OnShowCanvas;
            uiManager.OnUIClose += OnCloseCanvas;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnMenu.OnClick, OnClickedBtnMenu);
            EventDelegate.Remove(btnCharInfo.OnClick, OnClickedBtnCharInfo);
            EventDelegate.Remove(btnInven.OnClick, OnClickedBtnInven);
            EventDelegate.Remove(btnMake.OnClick, OnClickedBtnMake);
            EventDelegate.Remove(btnShop.OnClick, OnClickedBtnShop);
            EventDelegate.Remove(btnChat.OnClick, OnClickedBtnChat);

            uiManager.OnUIShow -= OnShowCanvas;
            uiManager.OnUIClose -= OnCloseCanvas;
        }

        protected override void Start()
        {
            base.Start();

            foreach (UIMain.MenuContent item in System.Enum.GetValues(typeof(UIMain.MenuContent)))
            {
                Unselect(item);
            }
        }

        protected override void OnLocalize()
        {
            foreach (UIMain.MenuContent item in System.Enum.GetValues(typeof(UIMain.MenuContent)))
            {
                UIMainButton button = GetButton(item);
                if (button == null)
                    continue;

                button.Text = GetButtonText(item);
            }
        }

        void OnClickedBtnMenu()
        {
            SelectContent(UIMain.MenuContent.Menu);
        }

        void OnClickedBtnCharInfo()
        {
            SelectContent(UIMain.MenuContent.CharacterInfo);
        }

        void OnClickedBtnInven()
        {
            SelectContent(UIMain.MenuContent.Inven);
        }

        void OnClickedBtnMake()
        {
            SelectContent(UIMain.MenuContent.Make);
        }

        void OnClickedBtnShop()
        {
            SelectContent(UIMain.MenuContent.Shop);
        }

        void OnClickedBtnChat()
        {
            SelectContent(UIMain.MenuContent.Chat);
        }

        void OnShowCanvas(ICanvas canvas)
        {
            if (canvas is UIEscape)
            {
                btnMenu.SetActive(false);
                return;
            }

            UIMain.MenuContent content = GetContent(canvas);
            if (content == default)
                return;

            Select(content);
        }

        void OnCloseCanvas(ICanvas canvas)
        {
            if (canvas is UIEscape)
            {
                btnMenu.SetActive(true);
                return;
            }

            UIMain.MenuContent content = GetContent(canvas);
            if (content == default)
                return;

            Unselect(content);
        }

        public void SetNotice(UIMain.MenuContent content, bool isNotice)
        {
            UIMainButton button = GetButton(content);
#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(UIMain.MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return;
            }
#endif
            button.SetNotice(isNotice);
        }

        public Vector3 GetPosition(UIMain.MenuContent content)
        {
            UIMainButton button = GetButton(content);

#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(UIMain.MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return Vector3.zero;
            }
#endif

            return button.transform.position;
        }

        public void PlayTweenEffect(UIMain.MenuContent content)
        {
            UIMainButton button = GetButton(content);

#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(UIMain.MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return;
            }
#endif
            button.PlayTween();
        }

        public UISprite GetMenuIcon(UIMain.MenuContent content)
        {
            UIMainButton button = GetButton(content);

#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(UIMain.MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return null;
            }
#endif

            return button.GetIcon();
        }

        public void SetNewIcon(UIMain.MenuContent content, bool isNew)
        {
            UIMainButton button = GetButton(content);

#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(UIMain.MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return;
            }
#endif

            button.SetActiveNew(isNew);
        }

        private void SelectContent(UIMain.MenuContent content)
        {
            /** currentContent 는 UI가 켜지거나 꺼졌을 때 바꿔줌
             * 1. 컨텐츠가 아직 오픈 안 되었을 경우를 대비
             * 2. 외부 입력으로 해당 컨텐츠의 UI가 켜졌을 때를 대비
             */
            if (currentContent == content)
            {
                OnUnselect?.Invoke(currentContent); // 기존 컨텐츠 Unselect
            }
            else
            {
                OnSelect?.Invoke(content); // 현재 컨텐츠 select
            }
        }

        private void Select(UIMain.MenuContent content)
        {
            currentContent = content;
            UIMainButton button = GetButton(content);
#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(UIMain.MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return;
            }
#endif
            button.SetActiveSelect(true);
        }

        private void Unselect(UIMain.MenuContent content)
        {
            currentContent = default;
            UIMainButton button = GetButton(content);
#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(UIMain.MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return;
            }
#endif
            button.SetActiveSelect(false);
        }

        private UIMainButton GetButton(UIMain.MenuContent content)
        {
            switch (content)
            {
                case UIMain.MenuContent.Menu:
                    return btnMenu;

                case UIMain.MenuContent.CharacterInfo:
                    return btnCharInfo;

                case UIMain.MenuContent.Inven:
                    return btnInven;

                case UIMain.MenuContent.Make:
                    return btnMake;

                case UIMain.MenuContent.Shop:
                    return btnShop;

                case UIMain.MenuContent.Chat:
                    return btnChat;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIMain.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        private UIMain.MenuContent GetContent(ICanvas canvas)
        {
            if (canvas is UIHome)
                return UIMain.MenuContent.Menu;

            if (canvas is UICharacterInfo)
                return UIMain.MenuContent.CharacterInfo;

            if (canvas is UIInven)
                return UIMain.MenuContent.Inven;

            if (canvas is UIMake)
                return UIMain.MenuContent.Make;

            if (canvas is UIShop)
                return UIMain.MenuContent.Shop;

            if (canvas is UIChat)
                return UIMain.MenuContent.Chat;

            return default;
        }

        private string GetButtonText(UIMain.MenuContent content)
        {
            switch (content)
            {
                case UIMain.MenuContent.Menu:
                    return LocalizeKey._3038.ToText(); // 메뉴

                case UIMain.MenuContent.CharacterInfo:
                    return LocalizeKey._4000.ToText(); // 영웅

                case UIMain.MenuContent.Inven:
                    return LocalizeKey._3002.ToText(); // 가방

                case UIMain.MenuContent.Make:
                    return LocalizeKey._3003.ToText(); // 제작

                case UIMain.MenuContent.Shop:
                    return LocalizeKey._3005.ToText(); // 상점

                case UIMain.MenuContent.Chat:
                    return LocalizeKey._29002.ToText(); // 채팅
            }

            Debug.LogError($"[올바르지 않은 {nameof(UIMain.MenuContent)}] {nameof(content)} = {content}");

            return string.Empty;
        }
    }
}