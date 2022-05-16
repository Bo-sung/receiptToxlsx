namespace Ragnarok
{
    public abstract class AdventureRankPacket : ServerRankPacket
    {
        public sealed class Point : AdventureRankPacket
        {
            public override string GetRankValueText()
            {
                return LocalizeKey._48219.ToText() // {POINT}점
                    .Replace(ReplaceKey.POINT, rankValue);
            }
        }

        public sealed class Kill : AdventureRankPacket
        {
            public override string GetRankValueText()
            {
                return LocalizeKey._48220.ToText() // {COUNT}회
                    .Replace(ReplaceKey.COUNT, rankValue);
            }
        }
    }
}