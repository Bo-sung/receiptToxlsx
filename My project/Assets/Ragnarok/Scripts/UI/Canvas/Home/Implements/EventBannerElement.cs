using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class EventBannerElement : IInfo
    {
        public bool IsInvalidData => false;
        public event System.Action OnUpdateEvent;
    } 
}
