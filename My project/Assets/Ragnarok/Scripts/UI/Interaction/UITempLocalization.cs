using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UILabel))]
    public class UITempLocalization : MonoBehaviour
    {
        [SerializeField] int localKey;

        UILabel label;

        void Awake()
        {
            label = GetComponent<UILabel>();
        }

        void Start()
        {
            label.text = localKey.ToText();
        }
    }
}