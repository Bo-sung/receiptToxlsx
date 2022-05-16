using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class UIItemSkillInfo : MonoBehaviour, IAutoInspectorFinder
    {
        public interface IInput
        {
            int SkillId { get; }
            TargetType TargetType { get; }
            int Grade { get; }
            string SkillName { get; }
            string SkillDescription { get; }
            string IconName { get; }
        }

        [SerializeField] UISprite background;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UITextureHelper itemSkillIcon;
        [SerializeField] GameObject goSelect;
        [SerializeField] UISprite itemSkillTypeIcon;

        GameObject myGameObject;

        public event System.Action<int> OnSelect;

        private IInput input;

        void Awake()
        {
            myGameObject = gameObject;
        }

        void OnClick()
        {
            if (input == null)
                return;

            OnSelect?.Invoke(input.SkillId);
        }

        public void SetData(IInput input)
        {
            this.input = input;
            Refresh();
        }

        private void Refresh()
        {
            if (input == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            const string SPRITE_NAME = "Ui_Common_BG_Item_Type2_{0:D2}";
            background.spriteName = string.Format(SPRITE_NAME, input.Grade);
            labelName.Text = input.SkillName;
            itemSkillIcon.SetSkill(input.IconName);
            itemSkillTypeIcon.spriteName = GetSkillTypeIcon(input.TargetType);
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        private string GetSkillTypeIcon(TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.Allies:
                case TargetType.AlliesCharacter:
                case TargetType.AlliesCupet:
                    return "Ui_Common_Support";

                case TargetType.Enemy:
                case TargetType.EnemyCharacter:
                case TargetType.EnemyCupet:
                    return "Ui_Common_Attack";
            }

            return string.Empty;
        }
    }
}