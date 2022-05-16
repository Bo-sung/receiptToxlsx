using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleRebirth"/>
    /// </summary>
    public sealed class BattleRebirthPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GoodsModel goodsModel;

        // <!-- Event --!>
        public event System.Action OnUpdateZeny;

        public BattleRebirthPresenter()
        {
            goodsModel = Entity.player.Goods;
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += InvokeUpdateZeny;
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= InvokeUpdateZeny;
        }

        private void InvokeUpdateZeny(long zeny)
        {
            OnUpdateZeny?.Invoke();
        }

        /// <summary>
        /// 재화 사용 가능 여부
        /// </summary>
        public bool CanUseZeny(int needZeny)
        {
            return goodsModel.Zeny >= needZeny;
        }
    }
}