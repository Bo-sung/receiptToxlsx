using UnityEngine;

namespace Ragnarok.View
{
    public class TitleView : UIView, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonWithIcon btnZeny;
        [SerializeField] UIButtonWithIcon btnGuildCoin;
        [SerializeField] UIButtonWithIcon btnCatCoin;
        [SerializeField] UIButtonWithIcon btnRoPoint;

        /// <summary>
        /// 첫번째 재화 타입
        /// </summary>
        public enum FirstCoinType
        {
            Zeny = 1,
            GuildCoin,
        }

        /// <summary>
        /// 두번째 재화 타입
        /// </summary>
        public enum SecondCoinType
        {
            CatCoin = 1,
            RoPoint,
        }

        private FirstCoinType firstCoinType;
        private SecondCoinType secondCoinType;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnZeny.OnClick, OnClickedBtnZeny);
            EventDelegate.Add(btnGuildCoin.OnClick, OnClickedBtnGuildCoin);
            EventDelegate.Add(btnCatCoin.OnClick, OnClickedBtnCatCoin);
            EventDelegate.Add(btnRoPoint.OnClick, OnClickedBtnRoPoint);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnZeny.OnClick, OnClickedBtnZeny);
            EventDelegate.Remove(btnGuildCoin.OnClick, OnClickedBtnGuildCoin);
            EventDelegate.Remove(btnCatCoin.OnClick, OnClickedBtnCatCoin);
            EventDelegate.Remove(btnRoPoint.OnClick, OnClickedBtnRoPoint);
        }

        protected override void OnLocalize()
        {
        }

        public void Initialize(FirstCoinType firstCoinType, SecondCoinType secondCoinType)
        {
            this.firstCoinType = firstCoinType;
            this.secondCoinType = secondCoinType;

            ShowGoods();
        }

        void OnClickedBtnZeny()
        {
            UI.ShowZenyShop();
        }

        void OnClickedBtnGuildCoin()
        {
            // Do Nothing
        }

        void OnClickedBtnCatCoin()
        {
            UI.ShowCashShop();
        }

        void OnClickedBtnRoPoint()
        {
            UI.ShowRoPointShop();
        }

        public void HideGoods()
        {
            btnZeny.SetActive(false);
            btnGuildCoin.SetActive(false);
            btnCatCoin.SetActive(false);
            btnRoPoint.SetActive(false);
        }

        public void ShowGoods()
        {
            btnZeny.SetActive(firstCoinType == FirstCoinType.Zeny);
            btnGuildCoin.SetActive(firstCoinType == FirstCoinType.GuildCoin);
            btnCatCoin.SetActive(secondCoinType == SecondCoinType.CatCoin);
            btnRoPoint.SetActive(secondCoinType == SecondCoinType.RoPoint);
        }

        /// <summary>
        /// 타이틀 표시
        /// </summary>
        public void ShowTitle(string text)
        {
            labelMainTitle.Text = text;
        }

        /// <summary>
        /// 제니 표시
        /// </summary>
        public void ShowZeny(long zeny)
        {
            btnZeny.Text = zeny.ToString("N0");
        }

        /// <summary>
        /// 길드코인 표시
        /// </summary>
        public void ShowGuildCoin(long guildCoin)
        {
            btnGuildCoin.Text = guildCoin.ToString("N0");
        }

        /// <summary>
        /// 캣코인 표시
        /// </summary>
        public void ShowCatCoin(long catCoin)
        {
            btnCatCoin.Text = catCoin.ToString("N0");
        }

        /// <summary>
        /// RoPoint 표시
        /// </summary>
        public void ShowRoPoint(long roPoint)
        {
            btnRoPoint.Text = roPoint.ToString("N0");
        }
    }
}