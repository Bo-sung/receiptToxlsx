using UnityEngine;

namespace Ragnarok
{
    public class UISkillPreview : MonoBehaviour, IInspectorFinder
    {
        public interface IInput
        {
            int SkillId { get; }
            string IconName { get; }
        }

        [SerializeField] UITextureHelper icon;
        [SerializeField] UIButtonHelper btnSelf;

        GameObject myGameObject;

        private IInput input;

        void Awake()
        {
            myGameObject = gameObject;

            if (btnSelf)
            {
                EventDelegate.Add(btnSelf.OnClick, OnClick);
            }
        }

        private void OnDestroy()
        {
            if (btnSelf)
            {
                EventDelegate.Remove(btnSelf.OnClick, OnClick);
            }
        }

        public void SetData(IInput input)
        {
            this.input = input;
            Refresh();
        }

        private void Refresh()
        {
            if (input == null || input.SkillId == 0)
            {
                SetActive(false);
                return;
            }

            SetActive(true);
            icon.Set(input.IconName);
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        void OnClick()
        {
            if (input == null || input.SkillId == 0)
                return;

            UI.Show<UISkillTooltip>(new UISkillTooltip.Input(input.SkillId, 1));
        }

        bool IInspectorFinder.Find()
        {
            icon = GetComponent<UITextureHelper>();
            return true;
        }
    }
}