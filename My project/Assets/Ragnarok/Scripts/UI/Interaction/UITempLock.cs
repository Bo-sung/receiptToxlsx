using UnityEngine;

namespace Ragnarok
{
    public class UITempLock : MonoBehaviour
    {
#if UNITY_EDITOR
        private void Start()
        {
            gameObject.SetActive(!Cheat.All_OPEN_CONTENT);
        }
#endif

        void OnClick()
        {
            UI.ShowToastPopup(LocalizeKey._90045.ToText()); // 업데이트 예정입니다.
        }
    }
}