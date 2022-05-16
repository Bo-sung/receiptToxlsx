using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    public static class AppPauseMenuItems
    {
        private const string PAUSE_ON_BACKGROUND_PATH = "라그나로크 서버/백그라운드 Pause On";

        [MenuItem(PAUSE_ON_BACKGROUND_PATH, priority = 2000)]
        private static void TogglePauseBackground()
        {
            Application.runInBackground = !Application.runInBackground;
        }

        [MenuItem(PAUSE_ON_BACKGROUND_PATH, validate = true)]
        private static bool TogglePauseBackgroundValidate()
        {
            Menu.SetChecked(PAUSE_ON_BACKGROUND_PATH, !Application.runInBackground);
            return true;
        }
    }
}