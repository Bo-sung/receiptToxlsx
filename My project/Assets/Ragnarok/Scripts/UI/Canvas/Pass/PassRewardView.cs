namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIPass"/>
    /// </summary>
    public class PassRewardView : BasePassRewardView
    {
        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelFree.LocalKey = LocalizeKey._39807; // 일반
            labelPass.LocalKey = LocalizeKey._39808; // 라비린스 패스
        }
    }
}