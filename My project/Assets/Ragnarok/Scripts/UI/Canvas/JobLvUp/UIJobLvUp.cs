using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobLvUp : UICanvas
    {
        protected override UIType uiType => UIType.Hide | UIType.Back;

        private const float WAIT_TIME = 2f;

        [SerializeField] UILabelHelper labelTitleJobLv;
        [SerializeField] UILabelHelper labelJobLv;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitleJobLv.LocalKey = LocalizeKey._5600; // JOB Lv
        }

        public void Set(int jobLevel)
        {
            labelJobLv.Text = jobLevel.ToString();
            Timing.RunCoroutineSingleton(YieldWaitHide().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        IEnumerator<float> YieldWaitHide()
        {
            yield return Timing.WaitForSeconds(WAIT_TIME);
            Hide();
        }
    }
}