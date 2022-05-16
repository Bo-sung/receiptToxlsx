using UnityEngine;

namespace Ragnarok.SceneComposition
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class NpcSpawner : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] NpcType npcType;
        [Range(0f, 10f)]
        [SerializeField] float avoidanceRadius;
        [SerializeField] Transform signOffset;

        Transform myTransform;
        Collider myCollider;
        IHUDPool hudPool;

        private NpcEntity npcEntity;
        private HUDObject npcSign;

        void Awake()
        {
            myTransform = transform;
            myCollider = GetComponent<SphereCollider>();
            hudPool = HUDPoolManager.Instance;

            SetEnablePortal(false); // 기본 false 상태
        }

        void OnDestroy()
        {
            Despawn();
            DespawnSign();

            SetEnablePortal(false); // 기본 상태로 되돌림
        }

        public void Spawn()
        {
            if (npcEntity == null)
                npcEntity = NpcEntity.Factory.CreateNPC(npcType);

            UnitActor actor = npcEntity.GetActor();

            if (!actor)
                actor = npcEntity.SpawnActor();

            actor.Movement.ForceWarp(myTransform.position);
            actor.EffectPlayer.ShowName(); // 이름 표시

            SetEnablePortal(true); // Portal 활성화

            if (avoidanceRadius > 0f)
            {
                actor.Movement.SetAvoidancePriority(Constants.Movement.NPC_AVOIDANCE_PRIORITY);
                actor.Movement.SetAvoidanceRadius(avoidanceRadius);
            }
            else
            {
                actor.Movement.SetAvoidancePriority(Constants.Movement.ETC_AVOIDANCE_PRIORITY);
            }
        }

        public void SpawnSign(System.Action<NpcType> onClick)
        {
            if (npcSign == null)
                npcSign = hudPool.SpawnNpcSign(signOffset, npcType, onClick);
        }

        public void DespawnSign()
        {
            if (npcSign == null)
                return;

            npcSign.Release();
            npcSign = null;
        }

        private void Despawn()
        {
            if (npcEntity == null)
                return;

            npcEntity.DespawnActor();
            npcEntity = null;
        }

        private void SetEnablePortal(bool isEnable)
        {
            myCollider.enabled = isEnable;
        }

        public Vector3 GetPosition()
        {
            return myTransform.position;
        }

        public NpcType GetNpcType()
        {
            return npcType;
        }

        bool IInspectorFinder.Find()
        {
            npcType = name.ToEnum<NpcType>();

            if (signOffset is null)
            {
                GameObject go = new GameObject("SignOffset");
                signOffset = go.transform;
                signOffset.SetParent(transform);

                signOffset.localPosition = Vector3.up * 4.8f;

                SphereCollider collider = GetComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = 3.6f;
                collider.tag = Tag.PORTAL;
            }

            return true;
        }
    }
}