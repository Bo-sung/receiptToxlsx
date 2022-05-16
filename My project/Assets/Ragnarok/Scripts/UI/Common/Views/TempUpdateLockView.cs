using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UICupet"/>
    /// <see cref="UIChallenge"/>
    /// </summary>
    public class TempUpdateLockView : UIView, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelDescription;

        protected override void OnLocalize()
        {
            labelDescription.LocalKey = LocalizeKey._90045; // 업데이트 예정입니다.
        }
    }
}