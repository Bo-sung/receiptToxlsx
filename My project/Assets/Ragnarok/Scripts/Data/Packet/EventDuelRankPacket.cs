namespace Ragnarok
{
    public class EventDuelRankPacket : ServerRankPacket, UIDuelRank.IInput
    {
        public int alphabetIndex;

        public override void SetServerGroupId(int serverGroupId)
        {
            alphabetIndex = BasisType.EVENT_DUEL_ALPHABET.GetInt(serverGroupId);
        }

        public int GetAlphabetIndex()
        {
            return alphabetIndex;
        }
    }
}