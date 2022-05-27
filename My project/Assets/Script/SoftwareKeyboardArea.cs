using UnityEngine;

namespace Ragnarok
{

    /// <summary> 
    /// 소프트웨어 키보드의 표시 영역을 관리하는 class 
    /// </summary>
    public static class SoftwareKeyboardArea
    {
        //출처 https://github.com/baba-s/UniSoftwareKeyboardArea/blob/master/Scripts/SoftwareKeyboardArea.cs
        //안드로이드의 경우 키보드area를 받을수 없어서 직접 받아오는식으로 구현.
        // ================================================================================ 
        // 속성(static) 
        // ================================================================================ 
        /// <summary>
        /// 높이를 return합니다. 
        /// </summary> 
        public static int GetHeight()
        {
            return GetHeight(false);
        }

        /// <summary>
        /// 높이를 return합니다.
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

                var decorHeight = decorView.Call<int>("getHeight"); result += decorHeight;

                return result;
            } 
#else
            var area = TouchScreenKeyboard.area;
            var height = Mathf.RoundToInt(area.height);
            //if (area.height == 0)
            //    return Screen.height;
            return Screen.height <= height ? 0 : height;
#endif
        }
        //************************ 추가 및 수정 부분 ******************************/

        static Rect ScreenRect;

        public static Rect GetRect()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                var unityPlayer = currentActivity.Get<AndroidJavaObject>("mUnityPlayer");
                var view = unityPlayer.Call<AndroidJavaObject>("getView");

                if (view == null) return new Rect();

                Rect result;

                using (var rect = new AndroidJavaObject("android.graphics.Rect"))
                {
                    view.Call("getWindowVisibleDisplayFrame", rect);
                    int rHeight = rect.Call<int>("height");
                    int rWidth = rect.Call<int>("width");
                    Vector2 rCenter = new Vector2(rect.Call<int>("centerX"), rect.Call<int>("centerY"));
                    rCenter.x = rCenter.x - Mathf.Abs(rWidth /2);
                    rCenter.y = rCenter.y - Mathf.Abs(rHeight / 2);
                    Rect keyboardRect = new Rect(rCenter.x, rCenter.y, rWidth, rHeight);
                    //result = new Rect(keyboardRect.x,keyboardRect.y - rHeight,rWidth, Screen.height - rHeight );
                    result = keyboardRect;
                }
                return result;
            }
#else
            var area = TouchScreenKeyboard.area;
            var height = Mathf.RoundToInt(area.height);
            ScreenRect = new Rect(0, 0, Screen.width, Screen.height);
            Rect returnRec = new Rect(0, ScreenRect.y - area.height, ScreenRect.width,ScreenRect.height - area.height);
            return returnRec;
#endif
        }
    }
}