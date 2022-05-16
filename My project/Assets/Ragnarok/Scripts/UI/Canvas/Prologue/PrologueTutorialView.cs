using UnityEngine;

namespace Ragnarok.View
{
    public class PrologueTutorialView : UIView
    {
        [SerializeField] UILabelHelper labelTalk;

        protected override void OnLocalize()
        {
            labelTalk.LocalKey = LocalizeKey._900; // 화면을 드래그해서 움직여 보세요.
        }
    }
}