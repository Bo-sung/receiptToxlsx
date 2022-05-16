using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIItemResult : UICanvas
    {
        protected override UIType uiType => UIType.Destroy | UIType.Hide;

        [SerializeField] UITextureHelper itemIcon;
        [SerializeField] UILabel title;
        [SerializeField] float movingAnimTime;
        [SerializeField] AnimationCurve scaleCurve;
        [SerializeField] AnimationCurve moveCurve;
        [SerializeField] GameObject bezier;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnLocalize()
        {
        }

        protected override void OnHide()
        {
        }

        public void SetItem(string itemName, string iconName, Vector3 endPos)
        {
            title.text = itemName;

            itemIcon.SetItem(iconName, isAsync: false);
            itemIcon.cachedTransform.localPosition = Vector3.zero;
            itemIcon.cachedTransform.localScale = Vector3.one;

            Timing.RunCoroutineSingleton(YieldPlay(endPos).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> YieldPlay(Vector3 endPos)
        {
            yield return Timing.WaitForSeconds(1.8f);

            float animProg = 0f;
            float stopWatch = 0f;

            Vector3 startPos = itemIcon.cachedTransform.position;
            Vector3 bezierCenter = bezier.transform.position;

            while (animProg < 1f)
            {
                stopWatch += Time.deltaTime;
                animProg = Mathf.Clamp01(stopWatch / movingAnimTime);

                itemIcon.cachedTransform.position = GetBezier(startPos, bezierCenter, endPos, moveCurve.Evaluate(animProg));
                float scale = scaleCurve.Evaluate(animProg);
                itemIcon.cachedTransform.localScale = new Vector3(scale, scale, 1);

                yield return Timing.WaitForOneFrame;
            }

            CloseUI();
        }

        private Vector3 GetBezier(Vector3 a, Vector3 b, Vector3 c, float v)
        {
            return Vector3.Lerp(Vector3.Lerp(a, b, v), Vector3.Lerp(b, c, v), v);
        }

        private void CloseUI()
        {
            UI.Close<UIItemResult>();
        }
    }
}