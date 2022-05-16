using UnityEngine;

namespace Ragnarok
{
    public class NavPoolObjectWithReady : NavPoolObject
    {
        [SerializeField] GameObject ready;
        [SerializeField] GameObject main;

        public virtual void ShowReady()
        {
            ready.SetActive(true);
        }

        public virtual void HideReady()
        {
            ready.SetActive(false);
        }

        public virtual void ShowMain()
        {
            main.SetActive(true);
        }

        public virtual void HideMain()
        {
            main.SetActive(false);
        }
    }
}