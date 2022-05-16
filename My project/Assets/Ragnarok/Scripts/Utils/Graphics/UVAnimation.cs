using UnityEngine;

namespace Ragnarok
{
    public class UVAnimation : MonoBehaviour
    {
        protected Material textureToAnimate;

        protected Vector2 uvOffset = Vector2.zero;
        public Vector2 uvAnimationRate = new Vector2(0.3f, 0.3f);
        public string textureName = "_MainTex";

        protected MeshRenderer backgroundMeshRenderer;

        [SerializeField]
        protected bool resetPositionToZero = false;

        protected void Start()
        {
            backgroundMeshRenderer = GetComponent<MeshRenderer>();

            if (backgroundMeshRenderer != null)
            {
                if (resetPositionToZero)
                    backgroundMeshRenderer.transform.position = Vector3.zero;

                textureToAnimate = backgroundMeshRenderer.material;
            }
        }

        protected void Update()
        {
            if (textureToAnimate != null)
            {
                if (uvOffset.x >= 1.0f)
                {
                    uvOffset.x = 0.0f;
                }

                if (uvOffset.y >= 1.0f)
                {
                    uvOffset.y = 0.0f;
                }

                uvOffset += uvAnimationRate * Time.deltaTime;
                textureToAnimate.mainTextureOffset = uvOffset;
            }
        }
    }
}