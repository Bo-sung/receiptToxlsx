using UnityEngine;

namespace Ragnarok
{
    public sealed class SimpleNameSpawner : MonoBehaviour
    {
        [SerializeField] int localKey;
        [SerializeField] Color color;
        [SerializeField] int fontSize;
        [SerializeField] UILabel.Effect effectStyle;
        [SerializeField] Color effectColor;

        IHUDPool hudPool;

        private SimpleHudUnitName hudName;

        void Awake()
        {
            hudPool = HUDPoolManager.Instance;
        }

        void OnDestroy()
        {
            if (hudName)
            {
                hudName.Release();
                hudName = null;
            }
        }

        void Start()
        {
            if (hudName == null)
                hudName = hudPool.SpawnSimpleHudUnitName(transform);

            hudName.Initialize(localKey, color);

            if (fontSize > 0)
                hudName.SetFontSize(fontSize);

            hudName.SetEffect(effectStyle, effectColor);

            hudName.Show();
        }
    }
}