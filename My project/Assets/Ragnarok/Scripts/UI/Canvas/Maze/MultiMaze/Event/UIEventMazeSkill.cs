using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEventMazeSkill : MonoBehaviour, IAutoInspectorFinder
    {
        public interface IInput
        {
            string SkillName { get; }
            string SkillDescription { get; }
            string IconName { get; }
        }

        [SerializeField] UITextureHelper skillIcon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;

        GameObject myGameObject;

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void SetData(IInput input)
        {
            if (input == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);
            skillIcon.SetSkill(input.IconName, isAsync: false);
            labelName.Text = input.SkillName;
            labelDescription.Text = input.SkillDescription;
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }
    }
}