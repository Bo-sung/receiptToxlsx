using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [ExecuteInEditMode]
    public class UIMaskSprite : UISprite
    {
        public enum SpriteMode
        {
            /// <summary>
            /// 일반
            /// </summary>
            None,

            /// <summary>
            /// 알파 마스크
            /// </summary>
            AlphaMask,
        }

        [HideInInspector, SerializeField] SpriteMode mMode;
        public SpriteMode mode
        {
            get { return mMode; }
            set
            {
                if (mMode == value)
                    return;

                mMode = value;
                
                RemoveFromPanel();
                MarkAsChanged();
            }
        }

        [HideInInspector, SerializeField] Texture2D mMaskTexture;

        public Texture2D maskTexture
        {
            get
            {
                return mMaskTexture;
            }
            set
            {
                if (mMaskTexture != value)
                {
                    mMaskTexture = value;
                    MarkAsChanged();
                }
            }
        }

        public override Shader shader
        {
            get
            {
                if (mMode == SpriteMode.None || maskTexture == null)
                    return base.shader;
               
                return Shader.Find("Unlit/AlphaMask");
            }
            set { base.shader = value; }
        }

        Material mMaterial;

        public override Material material
        { 
            get
            {
                if (mMode == SpriteMode.None || maskTexture == null)
                    return base.material;

                if (mMaterial == null)
                {
                    mMaterial = new Material(shader);
                    mMaterial.SetTexture("_MaskTex", mMaskTexture);
                    mMaterial.SetFloat("_WidthRate", GetAtlasSprite().width * 1f / atlas.spriteMaterial.mainTexture.width);
                    mMaterial.SetFloat("_HeightRate", GetAtlasSprite().height * 1f / atlas.spriteMaterial.mainTexture.height);
                    mMaterial.SetFloat("_XOffset", GetAtlasSprite().x * 1f / atlas.spriteMaterial.mainTexture.width);
                    mMaterial.SetFloat("_YOffset", (atlas.spriteMaterial.mainTexture.height - (GetAtlasSprite().y + GetAtlasSprite().height)) * 1f / atlas.spriteMaterial.mainTexture.height);
                }
                return mMaterial;
            }
            set => base.material = value; 
        }
    }
}
