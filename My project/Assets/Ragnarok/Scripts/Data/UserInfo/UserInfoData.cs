using Sfs2X.Util;

namespace Ragnarok
{
    /// <summary>
    /// 유저 정보
    /// </summary>
    public class UserInfoData : IPacket<ByteArray>
    {
        public int uid;
        public int totalCatCoin;
        public bool pushOn;
        public byte tutorialState;
        public int connectTime;
        public int dayConnectTime;
        public byte loginBonusCount;
        public short loginBonusGroupId;
        public byte slotSize; // 캐릭터 기본 슬롯 개수, 재화를 통해 늘릴수 있다.
        public byte pay_field_cnt; // 필드관련 패키지 카운트 계정당 1회용
        public byte pay_level_cnt; // 레벨업 패키지 카운드 계정단 1회용
        public byte connectTimeRewardIndex;
        public byte daily_quest_clear; // 일일퀘 최종보상 수령 여부
        public int max_clear_stage_id; // 클리어한 최대 스테이지
        public byte is_share_buff; // 셰어영구버프 활성유무
        public long tree_pack_remaintime; // 금빛 패키지 남은시간 (0이상이면 활성상태에 남은시간)
        public byte first_purchase_state; // 첫결제 보상 상태 (0 : 첫결제 하지 않은 상태, 1: 첫결제 후 보상이 나간 상태)
        public int mileage; // 마일리지
        public int mileage_reward_step; // 마일리지 보상 단계
        public bool is_liked_facebook; // 페북 좋아요보상 수령 여부

        public void Initialize(ByteArray byteArray)
        {
            uid                         = byteArray.ReadInt();
            totalCatCoin                = byteArray.ReadInt();
            pushOn                      = byteArray.ReadBool();
            tutorialState               = byteArray.ReadByte();
            connectTime                 = byteArray.ReadInt();
            dayConnectTime              = byteArray.ReadInt();
            loginBonusCount             = byteArray.ReadByte();
            loginBonusGroupId           = byteArray.ReadShort();
            var temp                    = byteArray.ReadLong(); // TODO 제거 예정
            slotSize                    = byteArray.ReadByte();
            pay_field_cnt               = byteArray.ReadByte();
            pay_level_cnt               = byteArray.ReadByte();
            connectTimeRewardIndex      = byteArray.ReadByte();
            daily_quest_clear           = byteArray.ReadByte();
            max_clear_stage_id          = byteArray.ReadInt();
            is_share_buff               = byteArray.ReadByte();
            tree_pack_remaintime        = byteArray.ReadLong();
            first_purchase_state        = byteArray.ReadByte();
            mileage                     = byteArray.ReadInt();
            mileage_reward_step         = byteArray.ReadInt();
            is_liked_facebook              = byteArray.ReadBool();
        }
    }
}