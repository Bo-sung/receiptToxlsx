using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class Share2ndSuitSlot : UIElement<Share2ndSuitSlot.IInput>
    {
        public interface IInput
        {
            ShareForceType Type { get; }
            int Level { get; }
        }

        [SerializeField] UIButtonHelper itemProfile;
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] UIButtonHelper warning;

        public event System.Action<ShareForceType> OnLockErrorMessage;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(itemProfile.OnClick, OnClickItem);
            EventDelegate.Add(warning.OnClick, OnClickLock);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(itemProfile.OnClick, OnClickItem);
            EventDelegate.Remove(warning.OnClick, OnClickLock);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            icon.SetItem(info.Type.GetTextureName());

            if (info.Level < 0)
            {
                warning.SetActive(true);
                labelLevel.Text = string.Empty;
            }
            else
            {
                warning.SetActive(false);
                labelLevel.Text = LocalizeKey._48238.ToText() // Lv. {LEVEL}
                    .Replace(ReplaceKey.LEVEL, info.Level);
            }
        }

        void OnClickItem()
        {
            UI.Show<UIShareForceLevelUp>().Set(info.Type);
        }

        void OnClickLock()
        {
            OnLockErrorMessage?.Invoke(info.Type);
        }
    }
}