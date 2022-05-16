using UnityEngine;

namespace Ragnarok
{
    public class UIGuildEmblemInfo : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper background;
        [SerializeField] UITextureHelper frame;
        [SerializeField] UITextureHelper icon;

        GameObject myGameObject;

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void SetData(int backgroundValue, int frameValue, int iconValue)
        {
            background.SetGuildEmblem($"background_{backgroundValue}");
            frame.SetGuildEmblem($"frame_{frameValue}");
            icon.SetGuildEmblem($"icon_{iconValue}");
        }

        private void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }
    }
}