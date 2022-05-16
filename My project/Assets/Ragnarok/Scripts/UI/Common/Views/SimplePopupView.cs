using UnityEngine;

namespace Ragnarok.View
{
    public class SimplePopupView : UIView
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;

        int mainTitleLocalKey;

        public int MainTitleLocalKey
        {
            set
            {
                mainTitleLocalKey = value;
                UpdateMainTitleText();
            }
        }

        public string MainTitle
        {
            set => labelMainTitle.Text = value;
        }

        public event System.Action OnExit;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.onClick, OnClickedBtnExit);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Add(btnExit.onClick, OnClickedBtnExit);
        }

        protected override void OnLocalize()
        {
            UpdateMainTitleText();
        }

        void OnClickedBtnExit()
        {
            OnExit?.Invoke();
        }

        private void UpdateMainTitleText()
        {
            if (mainTitleLocalKey > 0)
                labelMainTitle.LocalKey = mainTitleLocalKey;
        }
    }
}