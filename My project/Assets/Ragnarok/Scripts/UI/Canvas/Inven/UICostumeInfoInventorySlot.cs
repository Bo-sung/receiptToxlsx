using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class UICostumeInfoInventorySlot : UICostumeInfoSlot
    {
        [SerializeField] GameObject select;
        [SerializeField] TweenAlpha selectTween;

        static TweenAlpha publicTweenAlpha = null;

        protected override void Refresh()
        {
            base.Refresh();         

            select.SetActive(info.IsDisassemble());

            // Tween Alpha를 동일하게 적용
            if (publicTweenAlpha == null)
                publicTweenAlpha = info.GetTweenAlpha();

            selectTween.tweenFactor = publicTweenAlpha.tweenFactor;

            if (publicTweenAlpha.amountPerDelta > 0)
                selectTween.PlayForward();
            else
                selectTween.PlayReverse();
        }
    }
}