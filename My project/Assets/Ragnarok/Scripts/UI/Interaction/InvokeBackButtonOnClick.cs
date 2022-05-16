using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIButton))]
    public class InvokeBackButtonOnClick : MonoBehaviour
    {
        UIButton button;

        void Awake()
        {
            button = GetComponent<UIButton>();
            EventDelegate.Add(button.onClick, InvokeBackButton);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(button.onClick, InvokeBackButton);
        }

        private void InvokeBackButton()
        {
            if (Tutorial.isInProgress)
                return;

            UIManager.Instance.Escape();
        }
    }
}