using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(Renderer))]
    public class ScrollUV : MonoBehaviour
    {
        public enum Direction
        {
            Vertical,
            Horizontal
        }

        public float speed;
        public Direction direction;

        Renderer myRenderer;

        private Vector2 offset;

        void Awake()
        {
            myRenderer = GetComponent<Renderer>();
        }

        void Update()
        {
            if (myRenderer.material == null)
                return;

            if (direction == Direction.Vertical)
            {
                offset.y += speed * Time.deltaTime;

                if (offset.y >= 1)
                    offset.y = 0;
            }
            else
            {
                offset.x += speed * Time.deltaTime;

                if (offset.x >= 1)
                    offset.x = 0;
            }

            myRenderer.material.mainTextureOffset = offset;
        }
    }
}