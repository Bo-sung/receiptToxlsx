using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class CardEquipSender
    {
        public readonly long ItemNo;
        public readonly byte SlotIndex;

        public CardEquipSender(long itemNo, byte slotIndex)
        {
            ItemNo = itemNo;
            SlotIndex = slotIndex;
        }
    }
}