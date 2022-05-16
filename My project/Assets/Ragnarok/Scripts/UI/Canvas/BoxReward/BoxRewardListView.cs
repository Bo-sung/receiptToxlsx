using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class BoxRewardListView : UIView
    {
        [SerializeField] UITable table;
        [SerializeField] BoxRewardElement element;

        protected override void OnLocalize()
        {
        }

        public void Set(List<BoxRewardGroup> infos)
        {
            NGUITools.DestroyChildren(table.transform);

            if (infos == null)
                return;

            for (int i = 0; i < infos.Count; i++)
            {
                BoxRewardElement boxRewardElement = NGUITools.AddChild(table.gameObject, element.gameObject).GetComponent<BoxRewardElement>();
                boxRewardElement.Set(infos[i], i, Reposition);
            }
            Reposition();
        }

        private void Reposition()
        {
            table.Reposition();
        }
    }
}