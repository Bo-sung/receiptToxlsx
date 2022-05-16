using UnityEngine;

namespace Ragnarok.View.Home
{
    public class UIHomeButton : UIButtonWithLock
    {
        public enum LockType
        {
            None,
            MainQuest,
            JobLevel,
            Update,
        }

        [SerializeField, EnumIndex(typeof(UIHome.MenuContent))] int contentIndex;
        [SerializeField] GameObject background;
        [SerializeField] UISprite lockBase;
        [SerializeField] UILabelHelper labelOpenCondition;

        public event System.Action<UIHome.MenuContent> OnSelect;

        public UIHome.MenuContent Content => contentIndex.ToEnum<UIHome.MenuContent>();

        void Awake()
        {
            EventDelegate.Add(OnClick, OnClicked);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(OnClick, OnClicked);
        }

        public void SetLockType(LockType lockType)
        {
            NGUITools.SetActive(background, lockType == LockType.None);
            NGUITools.SetActive(lockBase.cachedGameObject, lockType != LockType.None);

            switch (lockType)
            {
                case LockType.MainQuest:
                    lockBase.spriteName = Constants.CommonAtlas.UI_COMMON_BG_MENU_UPDATE;
                    break;

                case LockType.JobLevel:
                    lockBase.spriteName = Constants.CommonAtlas.UI_COMMON_BG_MENU_UPDATE;
                    break;

                case LockType.Update:
                    lockBase.spriteName = Constants.CommonAtlas.UI_COMMON_BG_MENU_LOCK;
                    break;
            }
        }

        public void SetOpenConditionText(string text)
        {
            labelOpenCondition.Text = text;
        }

        void OnClicked()
        {
            OnSelect?.Invoke(Content);
        }
    }
}