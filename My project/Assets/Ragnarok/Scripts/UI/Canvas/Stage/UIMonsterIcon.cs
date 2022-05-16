using UnityEngine;

namespace Ragnarok
{
    public class UIMonsterIcon : MonoBehaviour, IAutoInspectorFinder
    {
        public interface IInput
        {
            string IconName { get; }
            string ElementIconName { get; }
            UnitSizeType UnitSizeType { get; }
            bool IsBoss { get; }
        }

        [SerializeField] UITextureHelper icon;
        [SerializeField] UISprite element;
        [SerializeField] UISizeType unitSize;
        [SerializeField] GameObject boss;
        [SerializeField] UIButton button;

        GameObject myGameObject;

        private IInput info;

        void Awake()
        {
            myGameObject = gameObject;

            if (button)
                EventDelegate.Add(button.onClick, OnClick);
        }

        void OnDestroy()
        {
            myGameObject = null;

            if (button)
                EventDelegate.Remove(button.onClick, OnClick);
        }

        void OnClick()
        {
            if (info == null)
                return;

            if (info is MonsterInfo monsterInfo)
                UI.Show<UIMonsterDetail>().ShowMonster(monsterInfo.ID, monsterInfo.Level);
        }

        public void SetData(IInput info)
        {
            this.info = info;
            Refresh();
        }

        private void Refresh()
        {
            if (info == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            icon.Set(info.IconName);

            string elementIconName = info.ElementIconName;
            if (string.IsNullOrEmpty(elementIconName))
            {
                if (element)
                {
                    element.cachedGameObject.SetActive(false);
                }
            }
            else
            {
                if (element)
                {
                    element.cachedGameObject.SetActive(true);
                    element.spriteName = elementIconName;
                }
            }

            UnitSizeType unitSizeType = info.UnitSizeType;
            if (unitSizeType == UnitSizeType.None)
            {
                if (unitSize)
                    unitSize.gameObject.SetActive(false);
            }
            else
            {
                if (unitSize)
                {
                    unitSize.gameObject.SetActive(true);
                    unitSize.Set(unitSizeType);
                }
            }

            NGUITools.SetActive(boss, info.IsBoss);
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }
    }
}