using UnityEngine;

namespace Ragnarok
{
    public class TextureLoopFlow : MonoBehaviour
    {
        [SerializeField] UITexture[] texture;
        [SerializeField] float speed;

        int[] textureSize;

        void Start()
        {
            textureSize = new int[texture.Length];
            for (int i = 0; i < texture.Length; ++i)
            {
                textureSize[i] = (int)(texture[i].width * texture[i].cachedTransform.localScale.x);
            }
        }

        void Update()
        {
            for (int i = 0; i < texture.Length; ++i)
            {
                texture[i].cachedTransform.localPosition += Vector3.right * speed * Time.deltaTime;

                // over left
                if (speed < 0 && texture[i].cachedTransform.localPosition.x <= -textureSize[i] * 1.5)
                {
                    texture[i].cachedTransform.localPosition += Vector3.right * textureSize[i] * 2;
                }
                // over right
                else if (speed > 0 && texture[i].cachedTransform.localPosition.x >= textureSize[i] / 2)
                {
                    texture[i].cachedTransform.localPosition += Vector3.left * textureSize[i] * 2;
                }
            }
        }

        public void SetTexture(Texture2D texture2D)
        {
            if (texture2D == null)
                return;

            foreach (var t in texture)
            {
                t.mainTexture = texture2D;
            }
        }
    }
}