namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICenterOnClick"/>
    /// 참조: http://www.tasharen.com/forum/index.php?topic=13086.0
    /// </summary>
    public class UILimitCenterOnClick : UILimitCenter
    {
        void OnClick()
        {
            Execute();
        }
    }
}