using UnityEngine;

namespace Ragnarok.View
{
    public abstract class UISkillInfo<T> : MonoBehaviour, IAutoInspectorFinder
        where T : UISkillInfo.IInfo
    {
        [SerializeField] UITextureHelper skillIcon;
        [SerializeField] GameObject skillBlock;
        [SerializeField] UISprite skillTypeIcon;

        protected GameObject myGameObject;
        protected T info;

        protected virtual void Awake()
        {
            myGameObject = gameObject;
        }

        public void Show(T info, bool isAsync = false)
        {
            this.info = info;
            Refresh(isAsync);
        }

        protected virtual void Refresh(bool isAsync)
        {
            if (info == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);
            skillIcon.SetSkill(info.SkillIcon, isAsync); // 아이콘

            if (skillTypeIcon != null)
                skillTypeIcon.spriteName = info.SkillType.GetIconName();
        }

        public void SetSpriteMode(UIGraySprite.SpriteMode spriteMode)
        {
            skillIcon.Mode = spriteMode;
        }

        public void SetSkillBlock(bool isActive)
        {
            if (skillBlock == null)
                return;

            skillBlock.SetActive(isActive);
        }

        private void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }
    }

    public class UISkillInfo : UISkillInfo<UISkillInfo.IInfo>, IAutoInspectorFinder
    {
        public interface IInfo
        {
            string SkillIcon { get; }
            SkillType SkillType { get; }
            bool IsAvailableWeapon(EquipmentClassType weaponType);
        }
    }
}