namespace Ragnarok
{
    /// <summary>
    /// 아이템 소분류 타입
    /// 아이템 체크 처리 경우 보통 ItemGroupType을 사용
    /// Box 타입이나 재료 세부타입(CardSmeltMaterial,ShareVice) 예외적으로 처리할 경우 ItemType 사용
    /// 퀘스트중 타입 체크시 사용
    /// </summary>
    public enum ItemType : byte
    {
        None = 0,
        /// <summary>장비</summary>
        Equipment = 1,
        /// <summary>소모품</summary>
        ConsumableItem = 2,
        /// <summary>카드</summary>
        Card = 3,
        /// <summary>제작 재료</summary>
        ProductParts = 5,
        /// <summary>몬스터 조각</summary>
        MonsterPiece = 6,
        /// <summary>박스 아이템</summary>
        Box = 7,
        /// <summary>카드 제련 재료</summary>
        CardSmeltMaterial = 8,
        /// <summary>셰어 바이스</summary>
        ShareVice = 9,
        /// <summary>코스튬</summary>
        Costume = 10,
    }
}
