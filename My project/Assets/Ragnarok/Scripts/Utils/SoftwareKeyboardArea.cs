using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 소프트웨어 키보드의 표시 영역을 관리하는 클래스
    /// </summary>
    public static class SoftwareKeyboardArea
    {
        //================================================================================
        // 속성(static)
        //================================================================================
        /// <summary>
        /// 높이를 반환합니다. 
        /// </summary>
        public static int GetHeight()
        {
            return GetHeight(false);
        }

        /// <summary>
        /// 높이를 반환합니다. 
        /// </summary>
        public static int GetHeight(bool includeInput)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                var unityPlayer = currentActivity.Get<AndroidJavaObject>("mUnityPlayer");
                var view = unityPlayer.Call<AndroidJavaObject>("getView");

                if (view == null) return 0;

                int result;

                using (var rect = new AndroidJavaObject("android.graphics.Rect"))
                {
                    view.Call("getWindowVisibleDisplayFrame", rect);
                    result = Screen.height - rect.Call<int>("height");
                }

                if (!includeInput) return result;

                var softInputDialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");
                var window = softInputDialog?.Call<AndroidJavaObject>("getWindow");
                var decorView = window?.Call<AndroidJavaObject>("getDecorView");

                if (decorView == null) return result;

                var decorHeight = decorView.Call<int>("getHeight");
                result += decorHeight;

                return result;
            }
#else
            var area = TouchScreenKeyboard.area;
            var height = Mathf.RoundToInt(area.height);
            return Screen.height <= height ? 0 : height;
#endif
        }
    }
}
