using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(ObjectRadar))]
    public class NpcChat : MonoBehaviour
    {
        [SerializeField] int localKey;

        ObjectRadar rader;
        IHUDPool hudPool;

        private PoolObject npcChatBalloon;

        void Awake()
        {
            rader = GetComponent<ObjectRadar>();
            hudPool = HUDPoolManager.Instance;

            rader.OnFocusTarget += OnFocusTarget;
        }

        void OnDestroy()
        {
            rader.OnFocusTarget -= OnFocusTarget;
        }

        void OnFocusTarget(UnitActor actor)
        {
            if (actor == null)
            {
                npcChatBalloon?.Release();
                return;
            }

            npcChatBalloon = hudPool.SpawnNpcChatBalloon(transform, localKey.ToText());
        }
    }
}