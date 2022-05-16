using MEC;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="TotalDamage"/>
    /// <see cref="ComingSurprise"/>
    /// <see cref="Heal"/>
    /// <see cref="HpGaugeBar"/>
    /// <see cref="Damage"/>
    /// <see cref="UITouchEffect"/>
    /// <see cref="PlainText"/>
    /// <see cref="ChatBalloon"/>
    /// </summary>
    public abstract class HUDObject : PoolObject
    {
        [SerializeField] Transform anchor;
        [SerializeField] Vector3 anchorPosition;
        [SerializeField] bool isFixed = false;

        protected override void Awake()
        {
            base.Awake();

            UI.AddEventLocalize(OnLocalize);
        }

        protected override void OnDestroy()
        {
            base.OnDespawn();

            UI.RemoveEventLocalize(OnLocalize);
            Timing.KillCoroutines(CachedGameObject);
        }

        protected virtual void Update()
        {
            if (isFixed)
                return;

            UpdatePosition();
        }

        protected virtual void OnLocalize()
        {
        }

        public override void OnDespawn()
        {
            base.OnDespawn();

            anchor = null;
            anchorPosition = Vector3.zero;
        }

        public void SetAnchor(Transform anchor, bool isFollow)
        {
            if (isFollow)
            {
                this.anchor = anchor;
            }
            else
            {
                anchorPosition = anchor.position;
            }

            UpdatePosition();

            NGUITools.MarkParentAsChanged(CachedGameObject);
        }

        public void SetPosition(Vector3 position, bool isFixed)
        {
            CachedTransform.localPosition = position;
            this.isFixed = isFixed;

            if (isFixed)
                return;

            UpdatePosition();
            NGUITools.MarkParentAsChanged(CachedGameObject);
        }

        private void UpdatePosition()
        {
            if (anchor == null)
            {
                UpdatePosition(anchorPosition);
            }
            else
            {
                UpdatePosition(anchor.position);
            }
        }

        private void UpdatePosition(Vector3 position)
        {
            if (mainCamera == null)
                return;

            Vector3 pos = mainCamera.WorldToViewportPoint(position);
            CachedTransform.position = UI.CurrentCamera.ViewportToWorldPoint(pos);
        }
    }
}