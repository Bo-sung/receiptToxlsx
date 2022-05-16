using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public static class NetworkExtensions
    {
        public static async void WrapNetworkErrors(this Task task)
        {
            try
            {
                await task;
            }
            catch (System.Exception exception)
            {
                string description = exception.Message;

                if (exception is WWWErrorException wwwException)
                    Debug.Log(wwwException.url);

                if (exception is NetworkException networkException)
                {
                    networkException.Execute();
                    return;
                }

                if (exception is UIException uiException)
                {
                    uiException.Execute();
                    return;
                }

#if UNITY_EDITOR
                Debug.LogError(exception.GetType());
                Debug.LogError(exception.StackTrace);
#endif

                UI.ConfirmPopup(description);
                //UI.HideIndicator();
            }
        }
    }
}