using UnityEngine;

namespace Ragnarok.View
{
    public class UIStatusElement : UIView, IInspectorFinder
    {
        private readonly Color COLOR_DEFUALT = Color.white;
        private readonly Color COLOR_OVER_STATUS = new Color32(127, 179, 254, 255);

        [SerializeField] UISprite statBG;
        [SerializeField] UILabelValue status;
        [SerializeField] UIButtonHelper btnPlus;
        [SerializeField] UIButtonHelper btnMax;

        public event System.Action OnPlus;
        public event System.Action OnMax;

        protected override void Awake()
        {
            base.Awake();
            if (btnPlus)
                EventDelegate.Add(btnPlus.OnClick, OnClickedBtnPlus);
            if (btnMax)
                EventDelegate.Add(btnMax.OnClick, OnClickedBtnMax);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (btnPlus)
                EventDelegate.Remove(btnPlus.OnClick, OnClickedBtnPlus);
            if (btnMax)
                EventDelegate.Remove(btnMax.OnClick, OnClickedBtnMax);
        }

        protected override void OnLocalize()
        {

        }

        private void OnClickedBtnPlus()
        {
            OnPlus?.Invoke();
        }

        private void OnClickedBtnMax()
        {
            OnMax?.Invoke();
        }

        public UILabelValue Get()
        {
            return status;
        }

        public void SetIsMax(bool isMax)
        {
            btnPlus.SetActive(!isMax);
            btnMax.SetActive(isMax);
        }

        public void SetCanOverStatus(bool canOverStatus)
        {
            btnMax.SetNotice(canOverStatus);
        }

        public void SetIsOverStatus(bool isOverStatus)
        {
            statBG.color = isOverStatus ? COLOR_OVER_STATUS : COLOR_DEFUALT;
        }

        public void SetEnableBtnPlus(bool isEnable)
        {
            btnPlus.IsEnabled = isEnable;
        }

        public UIButtonHelper GetBtnPlus()
        {
            return btnPlus;
        }

        bool IInspectorFinder.Find()
        {
            status = GetComponent<UILabelValue>();
            return true;
        }
    }
}