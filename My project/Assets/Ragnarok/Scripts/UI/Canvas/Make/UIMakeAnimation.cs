using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIMakeAnimation : UICanvas
    {
        private readonly string TAG = nameof(UIMakeAnimation);
        protected override UIType uiType => UIType.Destroy;

        private const float CLOSE_UI_TIME = 5.0f;

        [SerializeField] Camera unitCamera;
        [SerializeField] GameObject unitViewer;
        [SerializeField] UITexture unitView;

        private RenderTexture renderTexture;

        protected override void OnInit()
        {
            renderTexture = unitCamera.RenderTexture(1024, 1024);
            unitView.mainTexture = renderTexture;
        }

        protected override void OnClose()
        {
            NGUITools.Destroy(renderTexture);
        }

        protected override void OnShow(IUIData data = null)
        {
            PlayerPrefs.SetInt(nameof(UIMakeAnimation), 1);
            NGUITools.SetLayer(unitViewer, Layer.UI_3D);
            Timing.RunCoroutine(YieldCloseUI(CLOSE_UI_TIME), TAG);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        private IEnumerator<float> YieldCloseUI(float closeTime)
        {
            yield return Timing.WaitForSeconds(closeTime);

            UI.Close<UIMakeAnimation>();
        }
    }
}