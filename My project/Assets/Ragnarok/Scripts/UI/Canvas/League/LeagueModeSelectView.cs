using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UILeague"/>
    /// </summary>
    public class LeagueModeSelectView : UIView
    {
        [SerializeField] UIEventTrigger unselect;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UIButtonHelper btnSingle;
        [SerializeField] UIButtonHelper btnAgent;
        [SerializeField] UILabelHelper labelNotice;

        public event System.Action<bool> OnSelectMode;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(unselect.onClick, Hide);
            EventDelegate.Add(btnExit.onClick, Hide);
            EventDelegate.Add(btnSingle.OnClick, OnClickedBtnSingle);
            EventDelegate.Add(btnAgent.OnClick, OnClickedBtnAgent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(unselect.onClick, Hide);
            EventDelegate.Remove(btnExit.onClick, Hide);
            EventDelegate.Remove(btnSingle.OnClick, OnClickedBtnSingle);
            EventDelegate.Remove(btnAgent.OnClick, OnClickedBtnAgent);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._47034; // 대전 선택
            btnSingle.LocalKey = LocalizeKey._47035; // 싱글 대전
            btnAgent.LocalKey = LocalizeKey._47036; // 협동 대전
            labelNotice.LocalKey = LocalizeKey._47037; // 싱글 대전 점수는 싱글 랭킹에만 적용 됩니다.
        }

        private void OnClickedBtnSingle()
        {
            OnSelectMode?.Invoke(true);
        }

        private void OnClickedBtnAgent()
        {
            OnSelectMode?.Invoke(false);
        }
    }
}