using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ragnarok
{
    public class AuctionItemList : BetterList<AuctionItemData>
    {
        public int Count => size;

        public void LoadRegisteredItemList(AuctionItemPacket auctionItemPacket)
        {
            base.Release();

            if (auctionItemPacket is null)
                return;

            int count = auctionItemPacket.sfsObj.GetInt("1");
            for (int i = 0; i < count; ++i)
            {
                AuctionItemData item = new AuctionItemData(auctionItemPacket.sfsObj, i);
                Add(item);
            }
        }

        /// <summary>
        /// REQUEST_TRADE_SEARCH 검색결과로부터 리스트 얻기
        /// </summary>
        /// <param name="response"></param>
        public void LoadFromSearchResult(Response response)
        {
            base.Release();

            int searchCount = response.GetInt("1");
            for (int i = 0; i < searchCount; ++i)
            {
                long seq_no = response.GetLong((2 + i * 10).ToString());
                int item_id = response.GetInt((3 + i * 10).ToString());
                int tier_per = response.GetInt((4 + i * 10).ToString());
                byte item_level = response.GetByte((5 + i * 10).ToString());
                int item_count = response.GetInt((6 + i * 10).ToString());
                long card_id1 = response.GetLong((7 + i * 10).ToString());
                long card_id2 = response.GetLong((8 + i * 10).ToString());
                long card_id3 = response.GetLong((9 + i * 10).ToString());
                long card_id4 = response.GetLong((10 + i * 10).ToString());
                int item_price = response.GetInt((11 + i * 10).ToString());

                AuctionItemData data = new AuctionItemData(seq_no, -1, item_id, item_count, item_price, tier_per, item_level, card_id1, card_id2, card_id3, card_id4);
                base.Add(data);
            }
        }

        /// <summary>
        /// 등록한 아이템 seq_no로 찾기
        /// </summary>
        public AuctionItemData GetRegisteredItem(long seq_no)
        {
            return this.Find(e => e.seq_no == seq_no);
        }

        /// <summary>
        /// 거래소 목록에 아이템 등록
        /// </summary>
        public void Register(AuctionItemData newItem)
        {
            base.Add(newItem);
        }

        public new void Remove(AuctionItemData removeItem)
        {
            base.Remove(removeItem);
        }
    }
}