using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIWidget))]
    public class CustomClicker : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UIWidget widget;
        [SerializeField] Collider2D customCollider;

        public event System.Action OnSelect;

        void Awake()
        {
            widget.hitCheck += CustomHitCheck;
        }

        void OnDestroy()
        {
            widget.hitCheck -= CustomHitCheck;
        }

        bool CustomHitCheck(Vector3 worldPos)
        {
            if (customCollider == null)
                return false;

            return customCollider.OverlapPoint(worldPos);
        }

        void OnClick()
        {
            OnSelect?.Invoke();
        }

        bool IInspectorFinder.Find()
        {
            widget = GetComponent<UIWidget>();
            customCollider = GetComponent<Collider2D>();
            return true;
        }
    } 
}
