using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class PrologueTipView : UIView, IInspectorFinder
    {
        [SerializeField] UIButtonHelper btnLeft;
        [SerializeField] UIButtonHelper btnRight;

        [SerializeField] UITextureHelper[] prologueTips;

        [SerializeField] UIPanel scrollPanel;
        [SerializeField] UIGrid grid;

        [SerializeField] float tweenStrength = 10f;
        [SerializeField] float tweenCycle = 5f;

        UIManager uiManager;
        UIWrapContent wrapContent;
        private float destIdx = 0;
        private RelativeRemainTime remainTime;

        protected override void Awake()
        {
            base.Awake();

            uiManager = UIManager.Instance;
            uiManager.OnResizeSafeArea += InitTweenPosition;

            EventDelegate.Add(btnLeft.OnClick, OnClickLeft);
            EventDelegate.Add(btnRight.OnClick, OnClickRight);

            wrapContent = grid.GetComponent<UIWrapContent>();
            InitTweenPosition();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            uiManager.OnResizeSafeArea -= InitTweenPosition;

            EventDelegate.Remove(btnLeft.OnClick, OnClickLeft);
            EventDelegate.Remove(btnRight.OnClick, OnClickRight);

            KillAllCoroutines();
        }

        protected override void OnLocalize()
        {

        }

        private void InitTweenPosition()
        {
            var imageWidth = prologueTips[0].width;
            var imageHeight = prologueTips[0].height;
            var destScale = 1 / uiManager.SafeAreaScale;
            var destWidth = (int)(imageWidth * destScale);
            var destHeight = (int)(imageHeight * destScale);

            // 패널 사이즈 변경
            var baseClipRegion = scrollPanel.baseClipRegion;
            scrollPanel.baseClipRegion = new Vector4(baseClipRegion.x, baseClipRegion.y, destWidth, destHeight);

            // 셀 사이즈 변경
            wrapContent.itemSize = destWidth;
            grid.cellWidth = destWidth;
            grid.enabled = true;

            // 이미지 사이즈 변경
            Vector3 tempScale = new Vector3(destScale, destScale, 1);
            for (int i = 0; i < prologueTips.Length; i++)
            {
                prologueTips[i].cachedTransform.localScale = tempScale;

                string textureName = string.Format(Constants.LocalizeTexture.PROLOGUE_TIP_NAME, i + 1);
                prologueTips[i].SetPrologueTip(textureName, isAsync: false);
            }

            // 위치 초기화
            destIdx = 0;
            OnTweenTip(true);
        }

        public override void Hide()
        {
            base.Hide();

            KillAllCoroutines();
        }

        public override void Show()
        {
            base.Show();

            Timing.RunCoroutine(YieldTween(), gameObject);
        }

        private IEnumerator<float> YieldTween()
        {
            while (true)
            {
                yield return Timing.WaitUntilTrue(IsPossibleTween);
                OnClickRight();
            }
        }

        private bool IsPossibleTween()
        {
            if (remainTime > 0f)
            {
                return false;
            }
            else
            {
                UpdateRemainTime();
                return true;
            }
        }

        private void OnClickLeft()
        {
            destIdx++;
            OnTweenTip();
        }

        private void OnClickRight()
        {
            destIdx--;
            OnTweenTip();
        }

        private void OnTweenTip(bool isInstant = false)
        {
            UpdateRemainTime();

            Vector3 targetPosition;

            if (destIdx == 0) targetPosition = Vector3.zero;
            else targetPosition = Vector3.right * destIdx * wrapContent.itemSize;

            SpringPanel.Begin(scrollPanel.cachedGameObject, targetPosition, isInstant ? 1000f : tweenStrength);
        }

        private void UpdateRemainTime()
        {
            remainTime = tweenCycle;
        }

        private void KillAllCoroutines()
        {
            Timing.KillCoroutines(gameObject);
        }

        bool IInspectorFinder.Find()
        {
            if (grid)
            {
                prologueTips = grid.GetComponentsInChildren<UITextureHelper>();
            }
            return true;
        }
    }
}