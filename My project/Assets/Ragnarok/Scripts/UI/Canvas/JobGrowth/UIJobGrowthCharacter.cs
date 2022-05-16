using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobGrowthCharacter : UIView, IInspectorFinder
    {
        [SerializeField] UITextureHelper[] characterImages;
        [SerializeField] UIGrid grid; // 두 개의 이미지가 사용될 수 있을 때 사용.

        protected override void OnLocalize()
        {
        }

        /// <summary>특정 직업 캐릭터 이미지</summary>
        public void SetCharacterImages(string[] characters, string gender)
        {
            bool isActive;

            foreach (UITextureHelper texture in characterImages)
            {
                isActive = false;

                foreach (string character in characters)
                {
                    if (texture.name == $"JobSD_{character}_{gender}")
                    {
                        isActive = true;
                        texture.SetJobSD(texture.name, isAsync: false);
                        break;
                    }
                }

                texture.cachedGameObject.SetActive(isActive);
            }

            // 이미지 위치 정렬
            if (grid != null)
                grid.Reposition();
        }

        public void SetItemImage()
        {
            foreach (UITextureHelper texture in characterImages)
            {
                texture.SetItem(texture.name, isAsync: false);
            }
        }

        public void SetNpcImage()
        {
            foreach (UITextureHelper texture in characterImages)
            {
                texture.SetNPC(texture.name, isAsync: false);
            }
        }

        bool IInspectorFinder.Find()
        {
            characterImages = GetComponentsInChildren<UITextureHelper>();
            grid = GetComponent<UIGrid>();
            return true;
        }
    }
}