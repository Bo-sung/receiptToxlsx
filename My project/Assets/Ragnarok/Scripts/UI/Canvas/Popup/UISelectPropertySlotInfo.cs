using UnityEngine;

namespace Ragnarok
{
    public class UISelectPropertySlotInfo : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UIButtonHelper btnSprite;
        ElementType elementType;

        private void Awake()
        {
            EventDelegate.Add(btnSprite.OnClick, OnClickElement);
        }

        private void OnDestroy()
        {
            EventDelegate.Remove(btnSprite.OnClick, OnClickElement);
        }

        public void InitData(ElementType type)
        {
            elementType = type;
            btnSprite.SpriteName = elementType.GetIconName();
        }

        private void OnClickElement()
        {
            UI.Show<UISelectPropertyPopup>().ShowElementView(elementType);
        }

        bool IInspectorFinder.Find()
        {
            btnSprite = GetComponent<UIButtonHelper>();
            return true;
        }
    }
}