namespace Ragnarok
{
    /// <summary>
    /// 현재 적용중인 버프 - 스테이지 축복아이템 (소모품)
    /// </summary>
    public class BlessBuffItemInfo : BuffItemInfo
    {
        public override bool IsEventBuff => true;

        public override bool HasRemainTime => false;
    }
}