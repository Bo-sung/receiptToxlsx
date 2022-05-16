using System;

namespace Ragnarok
{
    public class OverStatusPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly StatusModel statusModel;
        private readonly GoodsModel goodsModel;

        // <!-- Repositories --!>

        // <!-- Event --!>        
        public event Action<bool> OnOverStatusResult
        {
            add { statusModel.OnOverStatusResult += value; }
            remove { statusModel.OnOverStatusResult -= value; }
        }

        private BasicStatusType basicStatusType;

        public OverStatusPresenter()
        {
            statusModel = Entity.player.Status;
            goodsModel = Entity.player.Goods;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void Set(BasicStatusType basicStatusType)
        {
            this.basicStatusType = basicStatusType;
        }

        /// <summary>
        /// 오버스탯 성공 확률
        /// </summary>
        /// <returns></returns>
        public string GetOverStateSuccessRate()
        {
            int count = statusModel.MaxStatusPlusCount(basicStatusType);

            // 최대 스탯 도달
            if (count >= BasisType.OVER_STATUS_MAX.GetInt())
                return string.Empty;

            string rateText = LocalizeKey._6302.ToText() // [4E4D4F]강화 성공 확률 : [-][ff3030]{VALUE}%[-]
               .Replace(ReplaceKey.VALUE, (BasisType.OVER_STATUS_SUCCESS_RATE.GetInt(count) * 0.01f).ToString("0.##"));

            return rateText;
        }

        /// <summary>
        /// 오버 스탯 요청
        /// </summary>
        public void RequestOverStatus()
        {
            int status = statusModel.MaxStatusPlusCount(basicStatusType);
            if (status >= BasisType.OVER_STATUS_MAX.GetInt())
            {
                UI.ShowToastPopup(LocalizeKey._90266.ToText()); // STATUS POINT가 최대치에 도달하였습니다.
                return;
            }

            if (statusModel.StatPoint == 0)
            {
                UI.ShowToastPopup(LocalizeKey._90264.ToText()); // STATUS POINT가 부족합니다.
                return;
            }

            if (goodsModel.Zeny < GetNeedZeny())
            {
                string description = LocalizeKey._90000.ToText() // {COIN}가 부족합니다.
                   .Replace("{COIN}", CoinType.Zeny.ToText());

                UI.ConfirmPopup(description);
                return;
            }

            statusModel.RequestOverStatus(basicStatusType).WrapNetworkErrors();
        }

        /// <summary>
        /// 오버 스탯 필요 제니
        /// </summary>
        /// <returns></returns>
        public int GetNeedZeny()
        {
            int zeny = BasisType.OVER_STATUS_ZENY.GetInt(); // 기본 제니
            int zenyInc = BasisType.OVER_STATUS_INC_ZENY.GetInt(); // 가중치
            int count = statusModel.MaxStatusPlusCount(basicStatusType);

            return zeny + (zenyInc * count);
        }

        public long GetHaveZeny()
        {
            return goodsModel.Zeny;
        }
    }
}