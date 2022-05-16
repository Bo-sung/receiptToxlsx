using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIWidget))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DragSpin : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float speed = 1f;

        void OnDrag(Vector2 delta)
        {
            if (target != null)
            {
                target.localRotation = Quaternion.Euler(0f, -0.5f * delta.x * speed, 0f) * target.localRotation;
            }
        }
    } 
}
