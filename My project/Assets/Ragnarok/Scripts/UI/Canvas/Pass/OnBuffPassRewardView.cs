namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIOnBuffPass"/>
    /// </summary>
    public class OnBuffPassRewardView : BasePassRewardView
    {
        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelFree.LocalKey = LocalizeKey._39831; // 일반
            labelPass.LocalKey = LocalizeKey._39832; // OnBuff 패스
        }
    }
}