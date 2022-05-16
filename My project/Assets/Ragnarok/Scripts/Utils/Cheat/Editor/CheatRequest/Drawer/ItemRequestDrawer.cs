using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ItemRequestDrawer : CheatRequestDrawer
    {
        public override int OrderNum => 2;

        public override string Title => "아이템";

        // <!-- Repositories --!>
        private readonly EnumDrawer upgradeItemEnumDrawer = new EnumDrawer(isShowId: true);
        private readonly EnumDrawer cardUpgradeMaterialItemEnumDrawer = new EnumDrawer(isShowId: true);
        private readonly EnumDrawer makeItemEnumDrawer = new SortedEnumDrawer(isShowId: true);
        private readonly EnumDrawer tamingItemEnumDrawer = new EnumDrawer(isShowId: true);
        private readonly EnumDrawer cupetUpgradeMaterialItemEnumDrawer = new EnumDrawer(isShowId: true);
        private readonly EnumDrawer guildBattleUpgradeMaterialItemEnumDrawer = new EnumDrawer(isShowId: true);
        private readonly EnumDrawer dungeonItemEnumDrawer = new EnumDrawer(isShowId: true);
        private readonly EnumDrawer darkTreeMaterialItemEnumDrawer = new SortedEnumDrawer(isShowId: true);
        private readonly EnumDrawer specialConsumableItemEnumDrawer = new EnumDrawer(isShowId: true);
        private readonly EnumDrawer specialPartsItemEnumDrawer = new EnumDrawer(isShowId: true);

        private readonly EnumDrawer itemEnumDrawer = new SortedEnumDrawer(isShowId: true);

        // <!-- Editor Fields --!>
        private int upgradeItemCount = 1_000;
        private int cardUpgradeItemCount = 100_000;
        private int makeItemCount = 1_000;
        private int tamingItemCount = 100;
        private int cupetUpgradeItemCount = 1_000;
        private int guildBattleUpgradeItemCount = 100;
        private int dungeonItemCount = 1_000;
        private int darkTreeItemCount = 100;
        private int specialConsumableItemCount = 1_000;
        private int specialPartsItemCount = 1_000;
        private int customItemCount;

        protected override void Awake()
        {
            ItemDataManager itemDataRepo = ItemDataManager.Instance;
            EquipItemLevelupDataManager equipItemLevelupDataRepo = EquipItemLevelupDataManager.Instance;
            MakeDataManager makeDataRepo = MakeDataManager.Instance;
            TamingDataManager tamingDataRepo = TamingDataManager.Instance;

            upgradeItemEnumDrawer.Clear();
            cardUpgradeMaterialItemEnumDrawer.Clear();
            makeItemEnumDrawer.Clear();
            tamingItemEnumDrawer.Clear();
            cupetUpgradeMaterialItemEnumDrawer.Clear();
            guildBattleUpgradeMaterialItemEnumDrawer.Clear();
            dungeonItemEnumDrawer.Clear();
            darkTreeMaterialItemEnumDrawer.Clear();
            specialConsumableItemEnumDrawer.Clear();
            specialPartsItemEnumDrawer.Clear();

            itemEnumDrawer.Clear();

            // 강화 재료: 장비 강화 테이블 아이템 추가
            foreach (var item in equipItemLevelupDataRepo.GetItemMaterialId())
            {
                ItemData data = itemDataRepo.Get(item);
                if (data == null)
                    continue;

                string name = data.name_id.ToText(LanguageType.KOREAN);
                upgradeItemEnumDrawer.Add(data.id, name);
            }

            // 강화 재료: 기초데이터 아이템 추가
            AddBasisItems(upgradeItemEnumDrawer, BasisItem.WeaponOverSmelt, BasisItem.ArmorOverSmelt, BasisItem.BeyondSword, BasisItem.BeyondArmor, BasisItem.ShadowCardSlotOpen);

            // 제작 재료: 제작 테이블 아이템 추가
            foreach (var item in makeDataRepo.GetItemMaterialId())
            {
                ItemData data = itemDataRepo.Get(item);
                if (data == null)
                    continue;

                if (data.ItemType != ItemType.ProductParts)
                    continue;

                string name = data.name_id.ToText(LanguageType.KOREAN);
                makeItemEnumDrawer.Add(data.id, name);
            }

            // 테이밍 재료
            foreach (var item in tamingDataRepo.GetItemMaterialId())
            {
                ItemData data = itemDataRepo.Get(item);
                if (data == null)
                    continue;

                string name = data.name_id.ToText(LanguageType.KOREAN);
                tamingItemEnumDrawer.Add(data.id, name);
            }

            // 던전 재료: 기초데이터 아이템 추가
            AddBasisItems(dungeonItemEnumDrawer, BasisItem.DungeonClearTicket, BasisItem.LeagueTicket, BasisItem.EventStageTicket, BasisItem.EndlessTowerTicket, BasisItem.EndlessTowerSkipItem, BasisItem.ForestMazeTicket, BasisItem.Emperium);

            // 특수 재료: 기초데이터 아이템 추가
            AddBasisItems(specialPartsItemEnumDrawer, BasisItem.RebirthMaterial, BasisItem.RebirthMaterial_CanTrade);

            foreach (ItemData data in itemDataRepo.EntireItems)
            {
                string name = data.name_id.ToText(LanguageType.KOREAN);

                // 어둠의나무 재료
                if (data.ItemType == ItemType.ProductParts && data.skill_rate > 0)
                {
                    darkTreeMaterialItemEnumDrawer.Add(data.id, name);
                }

                // 카드 강화 재료
                if (data.ItemType == ItemType.CardSmeltMaterial)
                {
                    cardUpgradeMaterialItemEnumDrawer.Add(data.id, name);
                }

                // 큐펫 경험치 재료
                if (data.ItemType == ItemType.ProductParts && data.matk_min > 0)
                {
                    cupetUpgradeMaterialItemEnumDrawer.Add(data.id, name);
                }

                // 길드전버프 경험치 재료
                if (data.ItemType == ItemType.ProductParts && data.matk_max > 0)
                {
                    guildBattleUpgradeMaterialItemEnumDrawer.Add(data.id, name);
                }

                // 특수 소모품 아이템
                if (data.ItemType == ItemType.ConsumableItem && data.skill_rate > 0)
                {
                    specialConsumableItemEnumDrawer.Add(data.id, name);
                }

                // 사용자 정의 재료: 아이템 테이블 전체 추가
                itemEnumDrawer.Add(data.id, name);
            }

            upgradeItemEnumDrawer.Ready();
            cardUpgradeMaterialItemEnumDrawer.Ready();
            makeItemEnumDrawer.Ready();
            tamingItemEnumDrawer.Ready();
            cupetUpgradeMaterialItemEnumDrawer.Ready();
            guildBattleUpgradeMaterialItemEnumDrawer.Ready();
            dungeonItemEnumDrawer.Ready();
            darkTreeMaterialItemEnumDrawer.Ready();
            specialConsumableItemEnumDrawer.Ready();
            specialPartsItemEnumDrawer.Ready();

            itemEnumDrawer.Ready();

            #region 기초데이터 아이템 추가 Method
            void AddBasisItems(EnumDrawer drawer, params BasisItem[] basisItems)
            {
                foreach (var item in basisItems)
                {
                    int itemId = item.GetID();
                    ItemData data = itemDataRepo.Get(itemId);
                    if (data == null)
                        continue;

                    string name = data.name_id.ToText(LanguageType.KOREAN);
                    drawer.Add(data.id, name);
                }
            }
            #endregion
        }

        protected override void OnDestroy()
        {
            upgradeItemEnumDrawer.Clear();
            cardUpgradeMaterialItemEnumDrawer.Clear();
            makeItemEnumDrawer.Clear();
            tamingItemEnumDrawer.Clear();
            cupetUpgradeMaterialItemEnumDrawer.Clear();
            guildBattleUpgradeMaterialItemEnumDrawer.Clear();
            dungeonItemEnumDrawer.Clear();
            darkTreeMaterialItemEnumDrawer.Clear();
            specialConsumableItemEnumDrawer.Clear();
            specialPartsItemEnumDrawer.Clear();

            itemEnumDrawer.Clear();
        }

        protected override void OnDraw()
        {
            if (DrawMiniHeader("강화 재료"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("일반");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            upgradeItemEnumDrawer.DrawEnum();
                            upgradeItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", upgradeItemCount));
                            DrawRequest(RequestUpgradeItem);
                        }
                    }
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("카드");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            cardUpgradeMaterialItemEnumDrawer.DrawEnum();
                            cardUpgradeItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", cardUpgradeItemCount));
                            DrawRequest(RequestCardUpgradeItem);
                        }
                    }
                }
            }

            if (DrawMiniHeader("제작 재료"))
            {
                using (ContentDrawer.Default)
                {
                    makeItemEnumDrawer.DrawEnum();
                    makeItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", makeItemCount));
                    DrawRequest(RequestMakeItem);
                }
            }

            if (DrawMiniHeader("길드 관련"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("테이밍 재료");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            tamingItemEnumDrawer.DrawEnum();
                            tamingItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", tamingItemCount));
                            DrawRequest(RequestTamingItem);
                        }
                    }
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("큐펫경험치 재료");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            cupetUpgradeMaterialItemEnumDrawer.DrawEnum();
                            cupetUpgradeItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", cupetUpgradeItemCount));
                            DrawRequest(RequestCupetUpgradeItem);
                        }
                    }
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("길드전버프경험치 재료");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            guildBattleUpgradeMaterialItemEnumDrawer.DrawEnum();
                            guildBattleUpgradeItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", guildBattleUpgradeItemCount));
                            DrawRequest(RequestGuildBattleUpgradeItem);
                        }
                    }
                }
            }

            if (DrawMiniHeader("기타"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("던전 재료");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            dungeonItemEnumDrawer.DrawEnum();
                            dungeonItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", dungeonItemCount));
                            DrawRequest(RequestDungeonItem);
                        }
                    }
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("어둠의나무 재료");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            darkTreeMaterialItemEnumDrawer.DrawEnum();
                            darkTreeItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", darkTreeItemCount));
                            DrawRequest(RequestDarkTreeItem);
                        }
                    }
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("특수 소모품");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            specialConsumableItemEnumDrawer.DrawEnum();
                            specialConsumableItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", specialConsumableItemCount));
                            DrawRequest(RequestSpecialConsumableItem);
                        }
                    }
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("특수 재료");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            specialPartsItemEnumDrawer.DrawEnum();
                            specialPartsItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", specialPartsItemCount));
                            DrawRequest(RequestSpecialPartsItem);
                        }
                    }
                }
            }

            if (DrawMiniHeader("사용자 정의"))
            {
                using (ContentDrawer.Default)
                {
                    itemEnumDrawer.DrawEnum();
                    customItemCount = Mathf.Max(1, EditorGUILayout.IntField("count", customItemCount));
                    DrawRequest(RequestCustomItem);
                }
            }
        }

        private void RequestUpgradeItem()
        {
            int itemId = upgradeItemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("강화 재료 선택필요");
                return;
            }

            SendItem(itemId, upgradeItemCount);
        }

        private void RequestCardUpgradeItem()
        {
            int itemId = cardUpgradeMaterialItemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("카드강화 재료 선택필요");
                return;
            }

            SendItem(itemId, cardUpgradeItemCount);
        }

        private void RequestMakeItem()
        {
            int itemId = makeItemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("제작 재료 선택필요");
                return;
            }

            SendItem(itemId, makeItemCount);
        }

        private void RequestTamingItem()
        {
            int itemId = tamingItemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("테이밍 재료 선택필요");
                return;
            }

            SendItem(itemId, tamingItemCount);
        }

        private void RequestCupetUpgradeItem()
        {
            int itemId = cupetUpgradeMaterialItemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("큐펫경험치 재료 선택필요");
                return;
            }

            SendItem(itemId, cupetUpgradeItemCount);
        }

        private void RequestGuildBattleUpgradeItem()
        {
            int itemId = guildBattleUpgradeMaterialItemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("길드전버프경험치 재료 선택필요");
                return;
            }

            SendItem(itemId, guildBattleUpgradeItemCount);
        }

        private void RequestDungeonItem()
        {
            int itemId = dungeonItemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("던전 재료 선택필요");
                return;
            }

            SendItem(itemId, dungeonItemCount);
        }

        private void RequestDarkTreeItem()
        {
            int itemId = darkTreeMaterialItemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("어둠의나무 재료 선택필요");
                return;
            }

            SendItem(itemId, darkTreeItemCount);
        }

        private void RequestSpecialConsumableItem()
        {
            int itemId = specialConsumableItemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("특수 소모품 선택필요");
                return;
            }

            SendItem(itemId, specialConsumableItemCount);
        }

        private void RequestSpecialPartsItem()
        {
            int itemId = specialPartsItemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("특수 재료 선택필요");
                return;
            }

            SendItem(itemId, specialPartsItemCount);
        }

        private void RequestCustomItem()
        {
            int itemId = itemEnumDrawer.id;
            if (itemId == 0)
            {
                AddWarningMessage("아이템 선택필요");
                return;
            }

            SendItem(itemId, customItemCount);
        }
    }
}