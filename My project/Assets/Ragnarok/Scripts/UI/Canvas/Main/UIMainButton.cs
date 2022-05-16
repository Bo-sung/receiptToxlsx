using UnityEngine;

namespace Ragnarok
{
    public class UIMainButton : UIButtonWithLock
    {
        [SerializeField] GameObject goSelect;
        [SerializeField] UIPlayTween tween;

        public void SetActiveSelect(bool isSelect)
        {
            NGUITools.SetActive(goSelect, isSelect);
        }

        public void PlayTween()
        {
            if (tween == null)
                return;

            tween.Play();
        }

        public override bool Find()
        {
            base.Find();

            tween = GetComponent<UIPlayTween>();
            return true;
        }
    }
}