using Sfs2X.Entities.Data;
namespace Ragnarok
{
    /// <see cref="AuctionItemList"/> // 거래소 아이템 리스트
    /// <summary>
    /// 개인 상점 아이템 리스트
    /// </summary>
    public class PrivateStoreItemList : BetterList<PrivateStoreItemData>
    {
        public int Count => size;

        public PrivateStoreItemList()
        {

        }

        public void LoadRegisteredItemList(PrivateStoreItemPacket[] itemPackets)
        {
            base.Release();

            if (itemPackets == null)
                return;
            for (int i = 0; i < itemPackets.Length; ++i)
            {
                ISFSObject sfsObj = itemPackets[i].sfsObj;
                var resItemNo = sfsObj.GetLong("1");
                var resCID2 = sfsObj.GetInt("2");
                var resItemID = sfsObj.GetInt("3");
                var resItemCount = sfsObj.GetInt("4");
                var resItemTierPer = sfsObj.GetInt("5");
                var resItemLevel = sfsObj.GetByte("6");
                var resItemPos = sfsObj.GetByte("7");
                resItemPos = 0; /// TODO: Pos는 항상 0이어야 한다.
                var resCardID1 = sfsObj.GetLong("8");
                var resCardID2 = sfsObj.GetLong("9");
                var resCardID3 = sfsObj.GetLong("10");
                var resCardID4 = sfsObj.GetLong("11");
                var resPrice = sfsObj.GetLong("12");
                var resTranscend = sfsObj.GetInt("13"); // 초월 수치 (카드일때 서버에서 다른 값으로 사용중)
                var resElementChange = sfsObj.ContainsKey("14") ? sfsObj.GetInt("14") : 0;
                int resElementLevel = sfsObj.GetInt("15");

                PrivateStoreItemData data = new PrivateStoreItemData(resItemID);
                data.SetInfo(resItemNo, resItemCount, (int)resPrice, resItemTierPer, resItemLevel, resItemPos.ToEnum<ItemEquipmentSlotType>(), resCardID1, resCardID2, resCardID3, resCardID4, resTranscend, resElementChange, resElementLevel);

                Register(data, false);
            }
        }

        /// <summary>
        /// 개인상점 인벤에 아이템 추가
        /// </summary>
        /// <param name="isVirtual">가상등록인지</param>
        public void Register(PrivateStoreItemData newItem, bool isVirtual)
        {
            newItem.isVirtualRegister = isVirtual;
            base.Add(newItem);
        }

        /// <summary>
        /// 개인상점 인벤에서 제거 (회수한 경우)
        /// </summary>
        /// <param name="removeItem"></param>
        public new void Remove(PrivateStoreItemData removeItem)
        {
            base.Remove(removeItem);
        }

        /// <summary>
        /// 가상등록했던 아이템들을 전부 실제 등록한 아이템으로 설정 
        /// </summary>
        public void ConfirmRegisterAll()
        {
            foreach (var e in this)
            {
                e.isVirtualRegister = false;
            }
        }
    }
}