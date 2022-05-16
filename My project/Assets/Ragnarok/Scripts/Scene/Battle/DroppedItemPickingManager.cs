using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(ObjectPicking))]
    public class DroppedItemPickingManager : GameObjectSingleton<DroppedItemPickingManager>, IInspectorFinder
    {
        [SerializeField] public ObjectPicking picking;

        protected override void OnTitle()
        {
        }

        bool IInspectorFinder.Find()
        {
            picking = GetComponent<ObjectPicking>();
            return true;
        }
    }
}