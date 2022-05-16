namespace Ragnarok
{
    public class BattleInputDuel : IBattleInput
    {
        public readonly UIDuel.State state;
        public readonly int cid;
        public readonly int uid;

        // <!-- Chapter --!>
        public readonly int chapter;
        public readonly int alphabetBit;

        // <!-- Event --!>
        public readonly int serverId;

        /// <summary>
        /// 챕터 듀얼 전용
        /// </summary>
        public BattleInputDuel(int cid, int uid, int chapter, int alphabetBit)
        {
            state = UIDuel.State.Chapter;
            this.cid = cid;
            this.uid = uid;
            this.chapter = chapter;
            this.alphabetBit = alphabetBit;
        }

        /// <summary>
        /// 이벤트 듀얼 전용
        /// </summary>
        public BattleInputDuel(int cid, int uid, int serverId)
        {
            state = UIDuel.State.Event;
            this.cid = cid;
            this.uid = uid;
            this.serverId = serverId;
        }

        /// <summary>
        /// 아레나 듀얼 전용
        /// </summary>
        public BattleInputDuel(int cid, int uid)
        {
            state = UIDuel.State.Arena;
            this.cid = cid;
            this.uid = uid;
        }
    }
}