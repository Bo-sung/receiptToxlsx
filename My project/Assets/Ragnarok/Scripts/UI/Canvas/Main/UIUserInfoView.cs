using UnityEngine;

namespace Ragnarok.View.Main
{
    public class UIUserInfoView : UIView, IInspectorFinder
    {
        [SerializeField] UIButtonHelper btnUserInfo;
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UILabelHelper labelBaseLevel;
        [SerializeField] UISlider progressBaseLevel;
        [SerializeField] UILabelHelper labelJobLevel;
        [SerializeField] UISlider progressJobLevel;
        [SerializeField] UIButtonHelper btnZeny;
        [SerializeField] UIButtonHelper btnCatCoin;
        [SerializeField] UIButtonHelper btnRoPoint;
        [SerializeField] GameObject[] plusIcons;
        [SerializeField] UISlider progressAddJobLevel;
        [SerializeField] UILabelHelper labelAddJobLevel;

        [SerializeField] UIWidget targetZeny;
        [SerializeField] UIWidget targetCatCoin;
        [SerializeField] UIWidget targetRoPoint;

        public event System.Action OnSelectJob;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnUserInfo.OnClick, OnClickedBtnUserInfo);
            EventDelegate.Add(btnZeny.OnClick, OnClickedBtnZeny);
            EventDelegate.Add(btnCatCoin.OnClick, OnClickedBtnCatCoin);
            EventDelegate.Add(btnRoPoint.OnClick, OnClickedBtnRoPoint);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnUserInfo.OnClick, OnClickedBtnUserInfo);
            EventDelegate.Remove(btnZeny.OnClick, OnClickedBtnZeny);
            EventDelegate.Remove(btnCatCoin.OnClick, OnClickedBtnCatCoin);
            EventDelegate.Remove(btnRoPoint.OnClick, OnClickedBtnRoPoint);
        }

        protected override void OnLocalize()
        {
        }

        /// <summary>
        /// 유저 정보 버튼
        /// </summary>
        void OnClickedBtnUserInfo()
        {
            OnSelectJob?.Invoke();
            //UI.ShortCut<UICharacterInfo>();
        }

        /// <summary>
        /// 제니 버튼
        /// </summary>
        void OnClickedBtnZeny()
        {
            UI.ShowZenyShop();
        }

        /// <summary>
        /// 냥다래 버튼
        /// </summary>
        void OnClickedBtnCatCoin()
        {
            UI.ShowCashShop();
        }

        /// <summary>
        /// RoPoint 버튼
        /// </summary>
        void OnClickedBtnRoPoint()
        {
            UI.ShowRoPointShop();
        }

        public void SetJobIcon(string name)
        {
            iconJob.Set(name);
        }

        public void SetBaseLevel(int value)
        {
            labelBaseLevel.Text = LocalizeKey._3021.ToText()
                .Replace("{LEVEL}", value.ToString()); // BASE Lv. {LEVEL}
        }

        public void SetProgressBaseLevel(float value)
        {
            progressBaseLevel.value = value;
        }

        public void SetJobLevel(int value)
        {
            labelJobLevel.Text = LocalizeKey._3022.ToText()
                .Replace("{LEVEL}", value.ToString()); // JOB Lv. {LEVEL}
        }

        public void SetProgressJobLevel(float value)
        {
            progressJobLevel.value = value;
        }

        public void SetProgressAddJobLevel(float value)
        {
            progressAddJobLevel.value = value;
        }

        public void SetAddJobLevel(int value)
        {
            labelAddJobLevel.Text = value > 0 ? $"+{value}" : string.Empty;
        }

        public void SetZeny(long value)
        {
            btnZeny.Text = value.ToString("N0");
        }

        public void SetCatCoin(long value)
        {
            btnCatCoin.Text = value.ToString("N0");
        }

        public void SetRoPoint(long value)
        {
            btnRoPoint.Text = value.ToString("N0");
        }

        public void SetActiveBtnCatCoin(bool isActive)
        {
            btnCatCoin.SetActive(isActive);
        }

        public void SetActiveBtnRoPoint(bool isActive)
        {
            btnRoPoint.SetActive(isActive);
        }

        public UIWidget GetWidget(UIMainTop.MenuContent content)
        {
            switch (content)
            {
                case UIMainTop.MenuContent.Zeny:
                    return targetZeny;

                case UIMainTop.MenuContent.Exp:
                    return labelBaseLevel.uiLabel;

                case UIMainTop.MenuContent.JobExp:
                    return labelJobLevel.uiLabel;

                case UIMainTop.MenuContent.CatCoin:
                    return targetCatCoin;

                case UIMainTop.MenuContent.RoPoint:
                    return targetRoPoint;
            }

            return null;
        }

        public void SetEnableButton(bool value)
        {
            btnUserInfo.GetComponent<UIButton>().enabled = value;
            btnZeny.GetComponent<UIButton>().enabled = value;
            btnCatCoin.GetComponent<UIButton>().enabled = value;
            btnRoPoint.GetComponent<UIButton>().enabled = value;

            for (int i = 0; i < plusIcons.Length; ++i)
                plusIcons[i].SetActive(value);
        }

        bool IInspectorFinder.Find()
        {
            if (btnZeny)
                targetZeny = btnZeny.transform.GetChild(0).GetComponent<UIWidget>();

            if (btnCatCoin)
                targetCatCoin = btnCatCoin.transform.GetChild(0).GetComponent<UIWidget>();

            if (btnRoPoint)
                targetRoPoint = btnRoPoint.transform.GetChild(0).GetComponent<UIWidget>();

            return true;
        }
    }
}