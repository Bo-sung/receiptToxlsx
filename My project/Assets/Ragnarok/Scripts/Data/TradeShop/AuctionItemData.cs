using Sfs2X.Entities.Data;

namespace Ragnarok
{
    /// <see cref="PrivateStoreItemData"/> // 개인상점 아이템 데이터
    /// <summary>
    /// 거래소(경매) 아이템 데이터
    /// </summary>
    public class AuctionItemData : TradeItemData
    {
        public long seq_no;
        int item_id;

        public void SetSeqNo(long seq_no)
        {
            this.seq_no = seq_no;
        }

        /// <summary>
        /// NotifyReceivedPacket으로 받은 데이터로 초기화
        /// </summary>
        /// <param name="res"></param>
        /// <param name="index">0-based</param>
        public AuctionItemData(ISFSObject res, int index)
        {
            seq_no = res.GetLong((2 + index * 5).ToString());
            item_No = res.GetLong((3 + index * 5).ToString());
            item_id = res.GetInt((4 + index * 5).ToString());
            SetData(item_id);
            item_count = res.GetInt((5 + index * 5).ToString());
            item_price = res.GetInt((6 + index * 5).ToString());
        }
        public AuctionItemData(long seq_no, long item_no, int item_id, int item_count, int item_price)
        {
            this.seq_no = seq_no;
            this.item_No = item_no;
            this.item_id = item_id;
            this.item_count = item_count;
            this.item_price = item_price;
        }
        public AuctionItemData(long seq_no, long item_no, int item_id, int item_count, int item_price, int tier_per, byte item_level, long card_id1, long card_id2, long card_id3, long card_id4)
        {
            this.seq_no = seq_no;
            this.item_No = item_no;
            this.item_id = item_id;
            this.item_count = item_count;
            this.item_price = item_price;

            this.tier_per = tier_per;
            this.item_level = item_level;
            this.card_id1 = card_id1;
            this.card_id2 = card_id2;
            this.card_id3 = card_id3;
            this.card_id4 = card_id4;
        }
    }
}