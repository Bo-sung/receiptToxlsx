namespace Ragnarok
{
    public class EventRoulettePacket : IPacket<Response>
    {
        public int normal_gacha_group_id; // 일반 돌림판 보상 그룹 ID
        public long remain_time;          // 돌림판 이벤트 남은시간
        public int normal_item_id;        // 일반 돌림판 소모 재료
        public int rare_gach_group_id;    // 레어 돌림판 보상 그룹 ID
        public int rare_item_id;          // 레어 돌림판 소모 재료
        public byte is_active_rare_gacha; // 레어 돌림판 활성봐 여부 0: 비활성화, 1: 활성화

        void IInitializable<Response>.Initialize(Response response)
        {
            normal_gacha_group_id = response.GetInt("1");
            remain_time = response.GetLong("2");
            normal_item_id = response.GetInt("3");
            rare_gach_group_id = response.GetInt("4");
            rare_item_id = response.GetInt("5");
            is_active_rare_gacha = response.GetByte("6");
        }
    }
}