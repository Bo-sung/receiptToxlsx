using UnityEngine;

namespace Ragnarok
{
    public class UIButtonWithLock : UIButtonWithIcon
    {
        [SerializeField] GameObject goNew;

        public void SetActiveNew(bool isShow)
        {
            NGUITools.SetActive(goNew, isShow);
        }
    }
}