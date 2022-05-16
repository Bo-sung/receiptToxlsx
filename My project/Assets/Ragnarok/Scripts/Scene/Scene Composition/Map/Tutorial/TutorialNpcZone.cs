using UnityEngine;

namespace Ragnarok.SceneComposition
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class TutorialNpcZone : TutorialZone, IInspectorFinder
    {
        [SerializeField] NpcType npcType;
        [Range(0f, 10f)]
        [SerializeField] float avoidanceRadius;

        Collider myCollider;

        private NpcEntity npcEntity;

        protected override void Awake()
        {
            base.Awake();

            myCollider = GetComponent<SphereCollider>();

            SetEnablePortal(false); // 기본 false 상태
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Despawn();

            SetEnablePortal(false); // 기본 상태로 되돌림
        }

        public void Spawn()
        {
            if (npcEntity == null)
                npcEntity = NpcEntity.Factory.CreateNPC(npcType);

            UnitActor actor = npcEntity.GetActor();

            if (actor == null)
                actor = npcEntity.SpawnActor();

            actor.Movement.ForceWarp(CachedTransform.position);
            actor.CachedTransform.localRotation = CachedTransform.localRotation;
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

        public void Despawn()
        {
            if (npcEntity == null)
                return;

            npcEntity.DespawnActor();
            npcEntity = null;
        }

        public void SetEnablePortal(bool isEnable)
        {
            myCollider.enabled = isEnable;
        }

        public NpcEntity GetNpcEntity()
        {
            return npcEntity;
        }

        public NpcType GetNpcType()
        {
            return npcType;
        }

        bool IInspectorFinder.Find()
        {
            npcType = name.ToEnum<NpcType>();

            SphereCollider collider = GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 3.6f;
            collider.tag = Tag.PORTAL;
            return true;
        }
    }
}