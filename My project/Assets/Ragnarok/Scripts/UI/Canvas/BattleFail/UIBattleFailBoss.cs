using MEC;
using Ragnarok.View;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIBattleFailBoss : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UILabelHelper labelTitle, labelTime;
        [SerializeField] UIButtonHelper btnRebirth;
        [SerializeField] UIGrid grid;
        [SerializeField] UIBattleFailBossSlot slotPrefab;
        [SerializeField] UIScrollView scroll;

        public event System.Action OnConfirm;
        private CharacterEntity charaEntity;
        private int duration;

        private bool[] cardClassTypeSet = new bool[20]; // 적당히 큰 수 20 입니다.
        private List<UIBattleFailBossSlot> slotInstanceList = new List<UIBattleFailBossSlot>();
        private int curShowingInstanceCount = 0;

        protected override void OnInit()
        {
            charaEntity = null;
            EventDelegate.Add(btnRebirth.OnClick, Confirm);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnRebirth.OnClick, Confirm);

            RemoveEvent();
            StopAllCoroutine();
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
            RemoveEvent();
            StopAllCoroutine();
        }

        protected override void OnLocalize()
        {
            labelTitle.Text = LocalizeKey._49000.ToText();
            btnRebirth.LocalKey = LocalizeKey._49005;
        }

        public void Confirm()
        {
            OnConfirm?.Invoke();
            Hide();
        }

        public void Show(CharacterEntity charaEntity = null)
        {
            base.Show();
            SetPlayer(charaEntity);

            duration = (int)(BasisType.UNIT_DEATH_COOL_TIME.GetInt() * 0.001f);

            curShowingInstanceCount = 0;
            for (int i = 0; i < slotInstanceList.Count; ++i)
                slotInstanceList[i].gameObject.SetActive(false);

            StopAllCoroutine();

            Timing.RunCoroutine(YieldAutoConfirm(), gameObject);

            var inventoryModel = Entity.player.Inventory;
            var equippedItems = inventoryModel.GetEquippedItems();
            var inventory = inventoryModel.itemList;

            System.Array.Sort(equippedItems, (a, b) =>
            {
                int priorityA = 0;
                int priorityB = 0;

                if (a.ClassType == EquipmentClassType.Weapon)
                    priorityA = 100000;
                else if (a.ClassType == EquipmentClassType.Armor)
                    priorityA = 10000;
                else if (a.ClassType == EquipmentClassType.HeadGear)
                    priorityA = 1000;
                else if (a.ClassType == EquipmentClassType.Garment)
                    priorityA = 100;
                else if (a.ClassType == EquipmentClassType.Accessory1)
                    priorityA = 10;
                else if (a.ClassType == EquipmentClassType.Accessory2)
                    priorityA = 1;

                if (b.ClassType == EquipmentClassType.Weapon)
                    priorityB = 100000;
                else if (b.ClassType == EquipmentClassType.Armor)
                    priorityB = 10000;
                else if (b.ClassType == EquipmentClassType.HeadGear)
                    priorityB = 1000;
                else if (b.ClassType == EquipmentClassType.Garment)
                    priorityB = 100;
                else if (b.ClassType == EquipmentClassType.Accessory1)
                    priorityB = 10;
                else if (b.ClassType == EquipmentClassType.Accessory2)
                    priorityB = 1;

                return priorityB - priorityA;
            });

            for (int i = 0; i < cardClassTypeSet.Length; ++i)
                cardClassTypeSet[i] = false;

            long playerZeny = Entity.player.Goods.Zeny;

            ItemInfo enchantableCard = null;
            int maxBattleScore = 0;

            for (int i = 0; i < inventory.Count; ++i)
            {
                var each = inventory[i];
                if (each is CardItemInfo)
                {
                    if (!each.IsEquipped)
                    {
                        int classType = (int)each.ClassType;

                        if (classType == (int)EquipmentClassType.All)
                        {
                            for (int j = 0; j < cardClassTypeSet.Length; ++j)
                                cardClassTypeSet[j] = true;
                        }
                        else
                        {
                            int index = 0;

                            while (classType > 0)
                            {
                                bool hasValue = (classType & 1) > 0;
                                cardClassTypeSet[index] |= hasValue;
                                classType = classType >> 1;
                                ++index;
                            }
                        }
                    }
                    else
                    {
                        var material = inventoryModel.itemList.Find(x => x is PartsItemInfo && x.IsSmeltMaterial && x.MaxSmelt > each.CardLevel);

                        if (material != null && each.NeedCardSmeltZeny <= playerZeny)
                        {
                            if (maxBattleScore < each.BattleScore)
                            {
                                maxBattleScore = each.BattleScore;
                                enchantableCard = each;
                            }
                        }
                    }
                }
            }

            EquipItemLevelupDataManager levelupMatDataRepo = EquipItemLevelupDataManager.Instance;

            ItemInfo cardEquippableItem = null;
            ItemInfo enchantableItem = null;

            for (int i = 0; i < equippedItems.Length; ++i)
            {
                var each = equippedItems[i];

                if (cardEquippableItem == null)
                {
                    bool hasEmptySlot = false;

                    for (int j = 0; j < 4; ++j)
                    {
                        bool isOpen = each.IsOpenCardSlot(j);
                        bool hasCard = each.GetCardItem(j) != null;

                        if (isOpen && !hasCard)
                        {
                            hasEmptySlot = true;
                            break;
                        }
                    }

                    if (hasEmptySlot)
                    {
                        int index = 0;
                        int classType = (int)each.ClassType;

                        while (classType > 0)
                        {
                            bool hasValue = (classType & 1) > 0;
                            if (hasValue)
                                break;

                            classType = classType >> 1;
                            ++index;
                        }

                        if (cardClassTypeSet[index])
                            cardEquippableItem = each;
                    }
                }

                if (enchantableItem == null)
                {
                    if (each.IsMaxSmelt)
                        continue;

                    int type = each.SlotType == ItemEquipmentSlotType.Weapon ? 1 : 2;

                    var mat = levelupMatDataRepo.Get(type, each.Rating, each.Smelt);

                    if (mat == null)
                        continue;

                    var material = new RewardData(mat.resource_type, mat.resource_value, mat.resource_count);

                    if (material.RewardType == RewardType.Item)
                    {
                        int count = inventoryModel.GetItemCount(material.ItemId);
                        if (count < material.Count) // 재료 부족
                            continue;
                    }
                    else
                    {
                        CoinType coinType = material.RewardType.ToCoinType();
                        long count = coinType.GetCoin();
                        if (count < material.Count)
                            continue;
                    }

                    enchantableItem = each;
                }
            }

            var questModel = Entity.player.Quest;
            bool isShowSkill = false;
            bool isShowStat = false;
            bool isShowCardEquip = false;
            bool isShowCardEnchant = false;
            bool isShowItemEnchant = false;
            bool isStageNormalMode = Entity.player.Dungeon.StageMode == StageMode.Normal;

            if (Entity.player.Skill.SkillPoint > 0 && questModel.IsOpenContent(ContentType.Skill, false))
            {
                isShowSkill = true;

                var slot = CreateSlot();
                slot.OpenSkill();
                slot.SetTitle(LocalizeKey._49006.ToText());
                slot.SetDesc(LocalizeKey._49007.ToText().Replace(ReplaceKey.VALUE, Entity.player.Skill.SkillPoint));
                slot.SetOnClick(() =>
                {
                    UI.Show<UISkill>();
                    Confirm();
                });
            }

            if (Entity.player.Status.StatPoint > 0)
            {
                isShowStat = true;

                var slot = CreateSlot();
                slot.OpenStat();
                slot.SetTitle(LocalizeKey._49008.ToText());
                slot.SetDesc(LocalizeKey._49009.ToText().Replace(ReplaceKey.VALUE, Entity.player.Skill.SkillPoint));
                slot.SetOnClick(() =>
                {
                    UI.Show<UICharacterInfo>();
                    Confirm();
                });
            }

            if (cardEquippableItem != null)
            {
                isShowCardEquip = true;

                var slot = CreateSlot();
                slot.OpenItem(cardEquippableItem);
                slot.SetTitle(LocalizeKey._49010.ToText());
                slot.SetDesc(LocalizeKey._49011.ToText());
                slot.SetOnClick(() =>
                {
                    UIInven.tabType = UIInven.TabType.Equipment;
                    UI.Show<UIInven>();
                    UI.Show<UIEquipmentInfo>().Set(cardEquippableItem.ItemNo);
                    Confirm();
                });
            }

            if (WhenAllOfTheseAreFalse(isShowCardEquip) && enchantableCard != null)
            {
                isShowCardEnchant = true;

                var slot = CreateSlot();
                slot.OpenCard(enchantableCard);
                slot.SetTitle(LocalizeKey._49012.ToText());
                slot.SetDesc(LocalizeKey._49013.ToText());
                slot.SetOnClick(() =>
                {
                    UIInven.tabType = UIInven.TabType.Equipment;
                    UI.Show<UICardSmelt>(enchantableCard);
                    Confirm();
                });
            }

            if (WhenAllOfTheseAreFalse(isShowCardEquip) && enchantableItem != null && questModel.IsOpenContent(ContentType.ItemEnchant, false))
            {
                isShowItemEnchant = true;

                var slot = CreateSlot();
                slot.OpenItem(enchantableItem);
                slot.SetTitle(LocalizeKey._49014.ToText());
                slot.SetDesc(LocalizeKey._49015.ToText());
                slot.SetOnClick(() =>
                {
                    UIInven.tabType = UIInven.TabType.Equipment;
                    UI.Show<UIInven>();
                    UI.Show<UIEquipmentInfo>().Set(enchantableItem.ItemNo);
                    Confirm();
                });
            }

            if (WhenAllOfTheseAreFalse(isShowSkill, isShowStat, isShowCardEquip, isShowCardEnchant, isShowItemEnchant, !isStageNormalMode) && Entity.player.Dungeon.LastEnterStageId > 1)
            {
                StageDataManager stm = StageDataManager.Instance;
                var stageData = stm.Get(Entity.player.Dungeon.LastEnterStageId - 1);
                var monsterData = MonsterDataManager.Instance.Get(stageData.normal_monster_id_1);
                var slot = CreateSlot();

                slot.OpenMonster(monsterData.icon_name);
                slot.SetTitle(LocalizeKey._49016.ToText().Replace(ReplaceKey.NAME, stageData.name_id.ToText()));
                slot.SetDesc(LocalizeKey._49017.ToText().Replace(ReplaceKey.VALUE, stageData.normal_monster_level).Replace(ReplaceKey.NAME, monsterData.name_id.ToText()));
                slot.SetOnClick(() =>
                {
                    Entity.player.Dungeon.StartBattleStageMode(StageMode.Normal, stageData.id);
                    Hide();
                });
            }

            if (WhenAllOfTheseAreFalse(isShowSkill, isShowStat, isShowItemEnchant)
                && (Entity.player.Dungeon.GetFreeEntryCount(DungeonType.ZenyDungeon) > 0 || Entity.player.Dungeon.GetFreeEntryCount(DungeonType.ExpDungeon) > 0)
                && questModel.IsOpenContent(ContentType.Dungeon, false))
            {
                var slot = CreateSlot();
                slot.OpenDungeon();
                slot.SetTitle(LocalizeKey._49018.ToText());
                slot.SetDesc(LocalizeKey._49019.ToText());
                slot.SetOnClick(() =>
                {
                    UI.Show<UIDungeon>();
                    Confirm();
                });
            }

            var secretShopItems = Entity.player.ShopModel.GetSecretShops();
            bool showScretShop = true;
            for (int i = 0; i < secretShopItems.Length; ++i)
                if (secretShopItems[i].IsSoldOut)
                    showScretShop = false;

            if (showScretShop && questModel.IsOpenContent(ContentType.SecretShop, false))
            {
                var slot = CreateSlot();
                slot.OpenSecret();
                slot.SetTitle(LocalizeKey._49020.ToText());
                slot.SetDesc(LocalizeKey._49021.ToText());
                slot.SetOnClick(() =>
                {
                    UI.Show<UIShop>().Set(UIShop.ViewType.Secret);
                    Confirm();
                });
            }

            var dailyQuests = Entity.player.Quest.GetDailyQuests();
            bool showDailyQuest = false;

            for (int i = 0; i < dailyQuests.Length; ++i)
                if (dailyQuests[i].CompleteType != QuestInfo.QuestCompleteType.ReceivedReward)
                    showDailyQuest = true;

            if (WhenAllOfTheseAreFalse(isShowItemEnchant) && showDailyQuest)
            {
                var slot = CreateSlot();
                slot.OpenDaily();
                slot.SetTitle(LocalizeKey._49022.ToText());
                slot.SetDesc(LocalizeKey._49023.ToText());
                slot.SetOnClick(() =>
                {
                    UIQuest.view = UIQuest.ViewType.Daily;
                    UI.Show<UIQuest>();
                    Confirm();
                });
            }

            if (curShowingInstanceCount == 0)
            {
                var slot = CreateSlot();
                slot.OpenChatting();
                slot.SetTitle(LocalizeKey._49024.ToText());
                slot.SetDesc(LocalizeKey._49025.ToText());
                slot.SetOnClick(() =>
                {
                    UI.Show<UIChat>();
                    Confirm();
                });
            }

            grid.Reposition();
            StartCoroutine(UpdateScroll());
        }

        private IEnumerator UpdateScroll()
        {
            yield return null;

            scroll.UpdatePosition();
            scroll.SetDragAmount(0, 0, false);
        }

        private bool WhenAllOfTheseAreFalse(params bool[] values)
        {
            for (int i = 0; i < values.Length; ++i)
                if (values[i])
                    return false;
            return true;
        }

        private UIBattleFailBossSlot CreateSlot()
        {
            if (curShowingInstanceCount < slotInstanceList.Count)
            {
                int index = curShowingInstanceCount++;
                slotInstanceList[index].gameObject.SetActive(true);
                return slotInstanceList[index];
            }
            else
            {
                var slot = Instantiate(slotPrefab);
                slot.gameObject.SetActive(true);
                slot.transform.parent = grid.transform;
                slot.transform.localScale = Vector3.one;
                slotInstanceList.Add(slot);
                ++curShowingInstanceCount;
                return slot;
            }
        }

        private IEnumerator<float> YieldAutoConfirm()
        {
            int remainTime = duration;

            while (remainTime > 0)
            {
                labelTime.Text = LocalizeKey._27002.ToText().Replace(ReplaceKey.TIME, remainTime);

                yield return Timing.WaitForSeconds(1f);
                remainTime -= 1;
            }

            Confirm();
        }

        /// <summary>
        /// 해당 캐릭터가 레벨업으로 인해 부활하는 경우를 감지.
        /// </summary>
        private void SetPlayer(CharacterEntity charaEntity)
        {
            RemoveEvent();

            this.charaEntity = charaEntity;

            if (this.charaEntity != null)
            {
                this.charaEntity.Character.OnUpdateJobLevel += OnPlayerLevelUp;
            }
        }

        /// <summary>
        /// 대상 캐릭터가 레벨업으로 인해 부활하면, 부활버튼을 누른 것과 동일하게 작동
        /// </summary>
        private void OnPlayerLevelUp(int jobLevel)
        {
            RemoveEvent();

            StopAllCoroutine();
            Confirm();
        }

        private void RemoveEvent()
        {
            if (charaEntity == null)
                return;

            charaEntity.Character.OnUpdateJobLevel -= OnPlayerLevelUp;
            charaEntity = null;
        }

        /// <summary>
        /// 모든 코루틴 종료
        /// </summary>
        private void StopAllCoroutine()
        {
            Timing.KillCoroutines(gameObject);
        }
    }
}
