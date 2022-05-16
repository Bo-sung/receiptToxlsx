using UnityEngine;
namespace Ragnarok
{

    public class UICardInfoInventorySlot : UICardInfoSlot
    {
        [SerializeField] GameObject select;
        [SerializeField] TweenAlpha select_tween;

        static TweenAlpha publicTweenAlpha = null;

        protected override void Refresh()
        {
            base.Refresh();

            if (IsInvalid())
                return;

            select.SetActive(presenter.IsDisassemble(info.ItemNo));

            // Tween Alpha를 동일하게 적용
            if (publicTweenAlpha == null)
                publicTweenAlpha = presenter.GetCardItemView().publicTweenAlpha;

            select_tween.tweenFactor = publicTweenAlpha.tweenFactor;

            if (publicTweenAlpha.amountPerDelta > 0)
                select_tween.PlayForward();
            else
                select_tween.PlayReverse();
        }
    }
}