using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIDragItem : UIDragDropItem
    {
        public event System.Action OnDragStop;

        protected override void OnDragEnd()
        {
            base.OnDragEnd();

            OnDragStop?.Invoke();
        }
    }
}