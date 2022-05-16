using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class EmperiumBuffView : UIView
    {
        [SerializeField] UILabelHelper labelTitleBuff;
        [SerializeField] BattleOptionListView buffList;

        protected override void OnLocalize()
        {
            labelTitleBuff.LocalKey = LocalizeKey._38405; // 엠펠리움 버프
        }

        public void SetData(IEnumerable<BattleOption> collection)
        {
            buffList.SetData(collection);
        }
    }
}