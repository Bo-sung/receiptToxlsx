namespace Ragnarok
{
    public class CharQuestDailyProgress : IPacket<Response>
    {
        public short quest_type;
        public int condition_value;
        public int progress;

        void IInitializable<Response>.Initialize(Response response)
        {
            quest_type = response.GetShort("1");
            condition_value = response.GetInt("2");
            progress = response.GetInt("3");
        }
    }
}