using UnityEngine;

namespace Ragnarok
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UILabel), typeof(BoxCollider2D))]
    public class OpenBasisUrlOnClick : MonoBehaviour
    {
        private const string START = "{URL:";
        private const string END = "}";

        UILabel label;

        void Awake()
        {
            label = GetComponent<UILabel>();

            if (!label.autoResizeBoxCollider)
            {
                label.autoResizeBoxCollider = true;
                label.ResizeCollider();
            }
        }

        void OnClick()
        {
            string url = label.GetUrlAtPosition(UICamera.lastWorldPosition);
            if (string.IsNullOrEmpty(url))
                return;

            if (url.StartsWith(START) && url.EndsWith(END))
            {
                string basisUrl = url
                    .Replace(START, string.Empty)
                    .Replace(END, string.Empty);

                if (int.TryParse(basisUrl, out int value))
                {
#if UNITY_EDITOR
                    Debug.Log($"BasisUrl: {value}");
#endif
                    value.ToEnum<BasisUrl>().OpenUrl();
                    return;
                }
            }

            Application.OpenURL(url);
        }
    }
}