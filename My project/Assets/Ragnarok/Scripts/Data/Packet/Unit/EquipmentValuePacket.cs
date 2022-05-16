using Sfs2X.Entities.Data;
using System.Linq;

namespace Ragnarok
{
    /// <summary>
    /// 장비 아이템 정보 (카드 포함)
    /// <see cref="BattleCharacterPacket"/>
    /// </summary>
    public class EquipmentValuePacket : IPacket<Response>, ItemInfo.IEquippedItemValue
    {
        public struct CardElement
        {
            public int item_id;
            public int item_level;
        }

        public int item_id;
        public int item_level;
        public int item_transcend;
        public int item_elementChange;
        public int item_elementLevel;
        public CardElement[] cards;

        int ItemInfo.IEquippedItemValue.ItemId => item_id;
        int ItemInfo.IEquippedItemValue.ItemLevel => item_level;
        int ItemInfo.IEquippedItemValue.ItemTranscend => item_transcend;
        int ItemInfo.IEquippedItemValue.ItemChangedElement => item_elementChange;
        int ItemInfo.IEquippedItemValue.ElementLevel => item_elementLevel;
        int? ItemInfo.IEquippedItemValue.GetEquippedCardId(int index) => index < cards.Length ? (int?)cards[index].item_id : null;
        int? ItemInfo.IEquippedItemValue.GetEquippedCardLevel(int index) => index < cards.Length ? (int?)cards[index].item_level : null;       

        void IInitializable<Response>.Initialize(Response response)
        {
            item_id = response.GetInt("1");
            item_level = response.GetInt("2");
            item_transcend = response.GetInt("3");
            item_elementChange = response.GetInt("4");

            cards = (
                from SFSObject cardObject in response.GetSFSArray("5")
                select new CardElement
                {
                    item_id = cardObject.GetInt("1"),
                    item_level = cardObject.GetInt("2")
                }).ToArray();

            item_elementLevel = response.GetInt("6");
        }

        public EquipmentValuePacket()
        {
        }

        public EquipmentValuePacket(int itemId)
        {
            item_id = itemId;
        }       
    }
}