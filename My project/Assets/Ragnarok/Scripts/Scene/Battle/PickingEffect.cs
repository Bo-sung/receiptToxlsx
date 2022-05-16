using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(ProgressAction))]
    public class PickingEffect : PoolObject
    {
        [SerializeField] Renderer[] renderers;
        [SerializeField] public Vector2 start = new Vector2(0.5f, 0f);
        [SerializeField] public Vector2 end = new Vector2(1.5f, 0f);
        [SerializeField] float duration = 1f;

        ProgressAction progressAction;

        protected override void Awake()
        {
            base.Awake();

            progressAction = GetComponent<ProgressAction>();

            progressAction.onProgress += OnProgress;
            progressAction.onEnd += Release;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            progressAction.onEnd -= Release;
            progressAction.onProgress -= OnProgress;

            progressAction = null;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            progressAction.Play(duration);
        }

        public override void OnDespawn()
        {
            base.OnDespawn();

            progressAction.Stop();
        }

        void OnProgress(float progress)
        {
            Vector2 offset = Vector2.Lerp(start, end, progress);

            foreach (var item in renderers)
            {
                if (item.material == null)
                    return;

                item.material.mainTextureOffset = offset;
            }
        }
    }
}