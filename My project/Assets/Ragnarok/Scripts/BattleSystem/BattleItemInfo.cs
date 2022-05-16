using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;

namespace Ragnarok
{
    public class BattleItemInfo : List<BattleOption>, BattleItemInfo.IValue, BattleItemInfo.IAttackerValue
    {
        public struct Settings
        {
            public EquipmentClassType weaponType;
            public ElementType weaponElementType;
            public int weaponElementLevel;
            public ItemInfo[] equippedItems; // 플레이어의 경우
            public int serverTotalItemAtk, serverTotalItemMatk, serverTotalItemDef, serverTotalItemMdef; // 멀티플레이어의 경우
        }

        public interface IValue
        {
            int TotalItemAtk { get; }
            int TotalItemMatk { get; }
            int TotalItemDef { get; }
            int TotalItemMdef { get; }
        }

        public interface IAttackerValue
        {
            BattleItemIndex WeaponBattleIndex { get; }
        }

        private ObscuredInt totalItemAtk, totalItemMatk, totalItemDef, totalItemMdef;
        private ObscuredInt weaponType;
        private ObscuredInt weaponElementType;
        private ObscuredInt weaponElementLevel;

#if UNITY_EDITOR
        BattleOption testOption;
#endif

        public int TotalItemAtk => totalItemAtk;
        public int TotalItemMatk => totalItemMatk;
        public int TotalItemDef => totalItemDef;
        public int TotalItemMdef => totalItemMdef;

        public EquipmentClassType WeaponType => weaponType.ToEnum<EquipmentClassType>();
        public BattleItemIndex WeaponBattleIndex => WeaponType.ToBattleItemIndex();
        public ElementType WeaponElementType => weaponElementType.ToEnum<ElementType>();
        public int WeaponElementLevel => weaponElementLevel;

        /// <summary>
        /// 정보 리셋
        /// </summary>
        public new void Clear()
        {
            base.Clear();

            SetWeaponType(default);
            SetWeaponElementType(default);
            SetWeaponElementLevel(0);

            totalItemAtk = 0;
            totalItemMatk = 0;
            totalItemDef = 0;
            totalItemMdef = 0;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Settings settings)
        {
            Clear(); // 리셋

            SetWeaponType(settings.weaponType); // 무기 세팅
            SetWeaponElementType(settings.weaponElementType); // 무기 속성 세팅
            SetWeaponElementLevel(settings.weaponElementLevel); // 무기 속성 레벨 세팅

            if (settings.equippedItems != null)
            {
                int totalItemAtk = 0;
                int totalItemMatk = 0;
                int totalItemDef = 0;
                int totalItemMdef = 0;

                for (int i = 0; i < settings.equippedItems.Length; i++)
                {
                    // 장착 장비 다시 한 번 확인
                    if (settings.equippedItems[i].ItemGroupType != ItemGroupType.Equipment)
                        continue;

                    // 장비 스탯 세팅
                    ItemInfo.Status status = settings.equippedItems[i].GetStatus();
                    totalItemAtk += status.atk;
                    totalItemMatk += status.matk;
                    totalItemDef += status.def;
                    totalItemMdef += status.mdef;

                    // 전투옵션 세팅
                    AddRange(settings.equippedItems[i]); // 해당 장비의 전투옵션 세팅
                    for (int index = 0; index < Constants.Size.MAX_EQUIPPED_CARD_COUNT; index++)
                    {
                        ItemInfo cardItemInfo = settings.equippedItems[i].GetCardItem(index);

                        if (cardItemInfo == null)
                            continue;

                        AddRange(cardItemInfo); // 카드의 전투옵션 세팅
                    }
                }

                this.totalItemAtk = totalItemAtk;
                this.totalItemMatk = totalItemMatk;
                this.totalItemDef = totalItemDef;
                this.totalItemMdef = totalItemMdef;
            }
            else
            {
                totalItemAtk = settings.serverTotalItemAtk;
                totalItemMatk = settings.serverTotalItemMatk;
                totalItemDef = settings.serverTotalItemDef;
                totalItemMdef = settings.serverTotalItemMdef;
            }

#if UNITY_EDITOR
            if (testOption.battleOptionType != BattleOptionType.None)
                Add(testOption);
#endif
        }

#if UNITY_EDITOR
        public void SetTestOption(BattleOptionType type, int value1, int value2)
        {
            testOption = new BattleOption(type, value1, value2);
        }
#endif

        private void SetWeaponType(EquipmentClassType weaponType)
        {
            this.weaponType = (int)weaponType;
        }

        private void SetWeaponElementType(ElementType elementType)
        {
            weaponElementType = (int)elementType;
        }

        private void SetWeaponElementLevel(int elementLevel)
        {
            weaponElementLevel = elementLevel;
        }
    }
}