using UnityEngine;

namespace Ragnarok
{
    public class AppPauseManager : MonoBehaviour
    {
        public static event System.Action<bool> OnAppPause;

        void OnApplicationPause(bool pause)
        {
            OnAppPause?.Invoke(pause);
        }
    }
}