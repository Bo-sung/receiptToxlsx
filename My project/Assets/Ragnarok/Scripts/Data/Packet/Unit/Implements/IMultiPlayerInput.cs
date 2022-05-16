namespace Ragnarok
{
    public interface IMultiPlayerInput : CharacterModel.IInputValue, StatusModel.IInputValue, GuildModel.IInputValue, TradeModel.IInputValue
    {
        /// <summary>
        /// 유닛 스탯 계산 시, 장착아이템 옵션 포함 예외 여부
        /// 일반적으로는 장착아이템의 옵션을 포함시켜 계산하지만
        /// 멀티플레이어의 경우에는 장착아이템의 옵션 대신 서버에서 받은 ItemStatusValue, BattleOptions, GuildBattleOptions 로 처리한다.
        /// 추가적으로 패시브 스킬의 경우도 BattleOptions로 처리한다
        /// </summary>
        bool IsExceptEquippedItems { get; }

        int WeaponItemId { get; }
        BattleItemInfo.IValue ItemStatusValue { get; }
        int ArmorItemId { get; }
        ElementType WeaponChangedElement { get; }
        int WeaponElementLevel { get; }
        ElementType ArmorChangedElement { get; }
        int ArmorElementLevel { get; }

        SkillModel.ISkillValue[] Skills { get; }
        SkillModel.ISlotValue[] Slots { get; }

        CupetListModel.IInputValue[] Cupets { get; }

        /// <summary>
        /// 전투 옵션 합산 정보 (길드 제외)
        /// </summary>
        IBattleOption[] BattleOptions { get; }

        /// <summary>
        /// 전투 옵션 합상 정보 (길드)
        /// </summary>
        IBattleOption[] GuildBattleOptions { get; }

        /// <summary>
        /// 캐릭터 초기 생성 X 좌표
        /// </summary>
        float PosX { get; }

        /// <summary>
        /// 캐릭터 초기 생성 Y 좌표
        /// </summary>
        float PosY { get; }

        /// <summary>
        /// 캐릭터 초기 생성 Z 좌표
        /// </summary>
        float PosZ { get; }

        /// <summary>
        /// 플레이어 상태
        /// 멀티미로 <see cref="PlayerBotState"/>
        /// 길드난전 <see cref="GVGPlayerState"/>
        /// </summary>
        byte State { get; }

        /// <summary>
        /// 유저 아이디
        /// </summary>
        int UID { get; }

        /// <summary>
        /// 서버에서 받은 maxHp 유무
        /// </summary>
        bool HasMaxHp { get; }

        /// <summary>
        /// 서버에서 받은 maxHp
        /// </summary>
        int MaxHp { get; }

        /// <summary>
        /// 서버에서 받은 Hp 유무
        /// </summary>
        bool HasCurHp { get; }

        /// <summary>
        /// 서버에서 받은 Hp
        /// </summary>
        int CurHp { get; }

        /// <summary>
        /// 팀 인덱스
        /// </summary>
        byte TeamIndex { get; }

        /// <summary>
        /// 대미지 체크를 위한 유닛 고유 값
        /// </summary>
        DamagePacket.UnitKey GetDamageUnitKey();

        /// <summary>
        /// 장착중인 아이템 정보 목록(장비(카드포함), 코스튬)
        /// </summary>
        ItemInfo.IEquippedItemValue[] GetEquippedItems { get; }
    }
}