using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIPass"/>
    /// </summary>
    public class PassView : BasePassView
    {
        [SerializeField] UITextureHelper texture;

        protected override void OnLocalize()
        {
            labelPassTitle.LocalKey = BasisProjectTypeLocalizeKey.LabyrinthPass.GetInt();
            btnBuyPass.LocalKey = LocalizeKey._39804; // 라비린스 패스 구매
        }

        public void SetTexture(string name)
        {
            texture.SetEvent(name);
        }
    }
}