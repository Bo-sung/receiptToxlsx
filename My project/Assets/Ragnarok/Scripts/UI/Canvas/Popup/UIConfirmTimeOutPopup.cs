using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// 시간 제한이 있는 팝업
    /// </summary>
    public sealed class UIConfirmTimeOutPopup : UIConfirmPopup
    {
        protected override void OnShow(IUIData data = null)
        {
            base.OnShow(data);

            Timing.RunCoroutine(YieldTimeOut(uiData.timeout), gameObject);
        }

        protected override void OnLocalize()
        {
        }

        private IEnumerator<float> YieldTimeOut(float timeout)
        {
            RemainTime remainTime = timeout * 1000f;
            while (true)
            {
                float leftTime = remainTime.ToRemainTime() * 0.001f;
                btnConfirm.Text = $"{LocalizeKey._1.ToText()} ({leftTime.ToString("N0")})";

                if (leftTime == 0f)
                    break;

                yield return Timing.WaitForSeconds(0.1f);
            }

            CloseUI();
        }

        protected override void CloseUI()
        {
            UI.Close<UIConfirmTimeOutPopup>();
            uiData.callback?.Invoke();
        }

        protected override void OnShowTooltip()
        {
            UI.Close<UIConfirmTimeOutPopup>();
        }
    }
}