using System.Collections.Generic;

namespace Ragnarok
{
    public class DuelInfo : IPacket<Response>
    {
        public string Name { get; private set; }
        public Job Job { get; private set; }
        public short JobLevel { get; private set; }
        public int BattleScore { get; private set; }
        public int WinCount { get; private set; }
        public int DefeatCount { get; private set; }

        // 듀얼 조각 정보
        public int DuelChapter1 { get; private set; }
        public int DuelChapter2 { get; private set; }
        public int DuelChapter3 { get; private set; }
        public int DuelChapter4 { get; private set; }
        public int DuelChapter5 { get; private set; }
        public int DuelChapter6 { get; private set; }
        public int DuelChapter7 { get; private set; }
        public int DuelChapter8 { get; private set; }
        public int DuelChapter9 { get; private set; }
        public int DuelChapter10 { get; private set; }
        public int DuelChapter11 { get; private set; }
        public int DuelChapter12 { get; private set; }
        public int DuelChapter13 { get; private set; }
        public int DuelChapter14 { get; private set; }
        public int DuelChapter15 { get; private set; }
        public int DuelChapter16 { get; private set; }
        public int DuelChapter17 { get; private set; }
        public int DuelChapter18 { get; private set; }
        public int DuelChapter19 { get; private set; }
        public int DuelChapter20 { get; private set; }
        public int DuelChapter21 { get; private set; }
        public int DuelChapter22 { get; private set; }

        // 듀얼 보상 받은 횟수
        public short DuelChapter1Reward { get; private set; }
        public short DuelChapter2Reward { get; private set; }
        public short DuelChapter3Reward { get; private set; }
        public short DuelChapter4Reward { get; private set; }
        public short DuelChapter5Reward { get; private set; }
        public short DuelChapter6Reward { get; private set; }
        public short DuelChapter7Reward { get; private set; }
        public short DuelChapter8Reward { get; private set; }
        public short DuelChapter9Reward { get; private set; }
        public short DuelChapter10Reward { get; private set; }
        public short DuelChapter11Reward { get; private set; }
        public short DuelChapter12Reward { get; private set; }
        public short DuelChapter13Reward { get; private set; }
        public short DuelChapter14Reward { get; private set; }
        public short DuelChapter15Reward { get; private set; }
        public short DuelChapter16Reward { get; private set; }
        public short DuelChapter17Reward { get; private set; }
        public short DuelChapter18Reward { get; private set; }
        public short DuelChapter19Reward { get; private set; }
        public short DuelChapter20Reward { get; private set; }
        public short DuelChapter21Reward { get; private set; }
        public short DuelChapter22Reward { get; private set; }

        public IEnumerable<(int curOwningBit, short curRewardedCount)> GetChapterStates()
        {
            yield return (DuelChapter1, DuelChapter1Reward);
            yield return (DuelChapter2, DuelChapter2Reward);
            yield return (DuelChapter3, DuelChapter3Reward);
            yield return (DuelChapter4, DuelChapter4Reward);
            yield return (DuelChapter5, DuelChapter5Reward);
            yield return (DuelChapter6, DuelChapter6Reward);
            yield return (DuelChapter7, DuelChapter7Reward);
            yield return (DuelChapter8, DuelChapter8Reward);
            yield return (DuelChapter9, DuelChapter9Reward);
            yield return (DuelChapter10, DuelChapter10Reward);
            yield return (DuelChapter11, DuelChapter11Reward);
            yield return (DuelChapter12, DuelChapter12Reward);
            yield return (DuelChapter13, DuelChapter13Reward);
            yield return (DuelChapter14, DuelChapter14Reward);
            yield return (DuelChapter15, DuelChapter15Reward);
            yield return (DuelChapter16, DuelChapter16Reward);
            yield return (DuelChapter17, DuelChapter17Reward);
            yield return (DuelChapter18, DuelChapter18Reward);
            yield return (DuelChapter19, DuelChapter19Reward);
            yield return (DuelChapter20, DuelChapter20Reward);
            yield return (DuelChapter21, DuelChapter21Reward);
            yield return (DuelChapter22, DuelChapter22Reward);
            yield break;
        }

        public void Initialize(Response t)
        {
            Name = t.GetUtfString("1");
            Job = t.GetByte("2").ToEnum<Job>();
            JobLevel = t.GetShort("3");
            BattleScore = t.GetInt("4");
            WinCount = t.GetInt("5");
            DefeatCount = t.GetInt("6");

            DuelChapter1 = t.GetInt("7");
            DuelChapter2 = t.GetInt("9");
            DuelChapter3 = t.GetInt("11");
            DuelChapter4 = t.GetInt("13");
            DuelChapter5 = t.GetInt("15");
            DuelChapter6 = t.GetInt("17");
            DuelChapter7 = t.GetInt("19");
            DuelChapter8 = t.GetInt("21");
            DuelChapter9 = t.GetInt("23");
            DuelChapter10 = t.GetInt("25");
            DuelChapter11 = t.GetInt("27");
            DuelChapter12 = t.GetInt("29");
            DuelChapter13 = t.GetInt("33");
            DuelChapter14 = t.GetInt("35");
            DuelChapter15 = t.GetInt("37");
            DuelChapter16 = t.GetInt("39");
            DuelChapter17 = t.GetInt("41");
            DuelChapter18 = t.GetInt("43");
            DuelChapter19 = t.GetInt("45");
            DuelChapter20 = t.GetInt("47");
            DuelChapter21 = t.GetInt("49");
            DuelChapter22 = t.GetInt("51");

            DuelChapter1Reward = t.GetShort("8");
            DuelChapter2Reward = t.GetShort("10");
            DuelChapter3Reward = t.GetShort("12");
            DuelChapter4Reward = t.GetShort("14");
            DuelChapter5Reward = t.GetShort("16");
            DuelChapter6Reward = t.GetShort("18");
            DuelChapter7Reward = t.GetShort("20");
            DuelChapter8Reward = t.GetShort("22");
            DuelChapter9Reward = t.GetShort("24");
            DuelChapter10Reward = t.GetShort("26");
            DuelChapter11Reward = t.GetShort("28");
            DuelChapter12Reward = t.GetShort("30");
            DuelChapter13Reward = t.GetShort("34");
            DuelChapter14Reward = t.GetShort("36");
            DuelChapter15Reward = t.GetShort("38");
            DuelChapter16Reward = t.GetShort("40");
            DuelChapter17Reward = t.GetShort("42");
            DuelChapter18Reward = t.GetShort("44");
            DuelChapter19Reward = t.GetShort("46");
            DuelChapter20Reward = t.GetShort("48");
            DuelChapter21Reward = t.GetShort("50");
            DuelChapter22Reward = t.GetShort("52");
        }
    }
}
