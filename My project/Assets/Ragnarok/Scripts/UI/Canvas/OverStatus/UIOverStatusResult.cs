using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIOverStatusResult : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        public enum ResultType
        {
            Success = 0,
            Failed,
        }

        [SerializeField] GameObject goSuccess, goFailed;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] float hideWaitTime = 3f;
        [SerializeField] GameObject goExplosion;

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
        }

        public void Set(ResultType resultType)
        {
            goSuccess.SetActive(resultType == ResultType.Success);
            goFailed.SetActive(resultType == ResultType.Failed);
            goExplosion.SetActive(resultType == ResultType.Success);

            if (resultType == ResultType.Success)
            {
                labelDesc.LocalKey = LocalizeKey._6400; // 오버 스탯 성공!
            }
            else
            {
                labelDesc.LocalKey = LocalizeKey._6401; // 오버 스탯 실패!
            }

            Timing.RunCoroutineSingleton(YieldAutoHide().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        IEnumerator<float> YieldAutoHide()
        {
            yield return Timing.WaitForSeconds(hideWaitTime);
            UI.Close<UIOverStatusResult>();
        }
    }
}