using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public static class UIExtensions
    {
        public static async void WrapUIErrors(this Task task)
        {
            try
            {
                await task;
            }
            catch (System.Exception exception)
            {
                if (exception is UIException uiException)
                {
                    uiException.Execute();
                    return;
                }

#if UNITY_EDITOR
                Debug.LogError(exception.GetType());
                Debug.LogError(exception.StackTrace);
#endif

                string description = exception.Message;
                UI.ConfirmPopup(description);
            }
        }
    }
}