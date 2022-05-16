namespace Ragnarok
{
    [System.Obsolete("큐펫 패널 관련 작업 다 지울것")]
    public class CharPanel : IPacket<Response>
    {
        public long no;
        public int cid;
        public byte pos1;
        public int buff_id1;
        public int cupet_id1;
        public byte pos2;
        public int buff_id2;
        public int cupet_id2;
        public byte pos3;
        public int buff_id3;
        public int cupet_id3;
        public byte pos4;
        public int buff_id4;
        public int cupet_id4;
        public byte is_use;

        void IInitializable<Response>.Initialize(Response response)
        {
            no = response.GetLong("1");
            cid = response.GetInt("2");
            pos1 = response.GetByte("3");
            buff_id1 = response.GetInt("4");
            cupet_id1 = response.GetInt("5");
            pos2 = response.GetByte("6");
            buff_id2 = response.GetInt("7");
            cupet_id2 = response.GetInt("8");
            pos3 = response.GetByte("9");
            buff_id3 = response.GetInt("10");
            cupet_id3 = response.GetInt("11");
            pos4 = response.GetByte("12");
            buff_id4 = response.GetInt("13");
            cupet_id4 = response.GetInt("14");
            is_use = response.GetByte("15");
        }
    }
}