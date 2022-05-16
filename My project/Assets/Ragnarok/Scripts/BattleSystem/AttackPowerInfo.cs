using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class AttackPowerInfo
    {
        CharacterEntity entity;

        private readonly Buffer<ItemInfo> itemBuffer;

        public AttackPowerInfo(CharacterEntity entity)
        {
            this.entity = entity;
            itemBuffer = new Buffer<ItemInfo>();
        }

        /// <summary>
        /// 특정 장비를 장착했을때의 가상 Status를 반환
        /// </summary>
        (BattleStatusInfo, BattleItemInfo) GetVirtualBattleStatusInfo(EquipmentItemInfo virtualEquipment)
        {
            itemBuffer.Release();

            UnitEntity.UnitEntitySettings settings = entity.CreateUnitSettings();

            BattleUnitInfo battleUnitInfo = new BattleUnitInfo(); // 전투 유닛 기본 정보
            BattleItemInfo battleItemInfo = new BattleItemInfo(); // 전투 아이템 정보
            BattleSkillInfo battleSkillInfo = new BattleSkillInfo(); // 전투 스킬 정보
            BattlePanelInfo battlePanelInfo = new BattlePanelInfo(); // 전투 패널 정보

            // 아이템 가상 장착/교체
            for (int i = 0; i < settings.itemSettings.equippedItems.Length; i++)
            {
                // 장착중인 슬롯은 제외
                if (settings.itemSettings.equippedItems[i].SlotType == virtualEquipment.SlotType)
                    continue;

                itemBuffer.Add(settings.itemSettings.equippedItems[i]);
            }
            itemBuffer.Add(virtualEquipment); // 가상 아이템 세팅

            settings.itemSettings.equippedItems = itemBuffer.GetBuffer(isAutoRelease: true);

            battleUnitInfo.Initialize(settings.unitSettings); // 기본 정보 세팅
            battleItemInfo.Initialize(settings.itemSettings); // 아이템 정보 세팅
            battleSkillInfo.Initialize(settings.skillSettings, null); // 스킬 정보 세팅

            PassiveBattleOptionList options = new PassiveBattleOptionList();
            options.AddRange(battleItemInfo); // 아이템 옵션 추가
            options.AddRange(battleSkillInfo); // 스킬 옵션 추가
            options.AddRange(battlePanelInfo); // 패널 옵션 추가
            //options.AddRange(battleBuffItemInfo); // 아이템 버프 옵션 추가
            //options.AddRange(battleBuffSkillInfo); // 스킬 버프 옵션 추가

            Battle.StatusOutput result = Battle.ReloadStatus(settings.statusInput, battleUnitInfo, battleItemInfo, options, entity); // 가상 장비를 장착한 상태

            BattleStatusInfo virtualBattleStatusInfo = new BattleStatusInfo();
            virtualBattleStatusInfo.Initialize(result.statusSettings);

            return (virtualBattleStatusInfo, battleItemInfo);
        }

        /// <summary>
        /// 가상 전투력 (특정 장비를 장착했을 때의 전투력)
        /// </summary>
        /// <param name="virtualEquipment">가상 장착할 장비</param>
        public int GetVirtualAttackPower(EquipmentItemInfo virtualEquipment)
        {
            (BattleStatusInfo status, BattleItemInfo battleItemInfo) = GetVirtualBattleStatusInfo(virtualEquipment);

            int AP = 0;
            int characterAp = GetCharacterAttackPower(status, virtualEquipment);
            AP += GetEquipmentAttackPower(status, battleItemInfo, virtualEquipment);
            AP += GetCardAttackPower(status, virtualEquipment);
            AP += GetSkillAttackPower(status, virtualEquipment);
            AP += GetCupetAttackPower(status, virtualEquipment);
            AP += GetHaveAgentAttackPower();
            float equippedAgentAp = GetEquippedAgentAttackPower();
            AP += MathUtils.ToInt(characterAp * (1 + equippedAgentAp));
            return AP;
        }

        /// <summary>
        /// 전체 전투력 (최종 전투력)
        /// </summary>
        public int GetTotalAttackPower()
        {
            int AP = 0;
            int characterAp = GetCharacterAttackPower(entity.preBattleStatusInfo);
            AP += GetEquipmentAttackPower(entity.preBattleStatusInfo, entity.battleItemInfo);
            AP += GetCardAttackPower(entity.preBattleStatusInfo);
            AP += GetSkillAttackPower(entity.preBattleStatusInfo);
            AP += GetCupetAttackPower(entity.preBattleStatusInfo);
            AP += GetHaveAgentAttackPower();
            float equippedAgentAp = GetEquippedAgentAttackPower();
            AP += MathUtils.ToInt(characterAp * (1 + equippedAgentAp));
            return AP;
        }

        /// <summary>
        /// 캐릭터 전투력
        /// </summary>
        private int GetCharacterAttackPower(BattleStatusInfo status, EquipmentItemInfo equipment = null)
        {
            float AP = 0f;

            // 직업 레벨
            AP += entity.Character.JobLevel * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(1) * 0.01f;

            // 전체 능력치
            int totalStat = 0;
            totalStat += entity.Status.BasicStr;
            totalStat += entity.Status.BasicAgi;
            totalStat += entity.Status.BasicVit;
            totalStat += entity.Status.BasicInt;
            totalStat += entity.Status.BasicDex;
            totalStat += entity.Status.BasicLuk;
            AP += totalStat * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(2) * 0.01f;

            // 전직 차수
            AP += entity.Character.JobGrade() * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(3) * 0.01f;

            return MathUtils.ToInt(AP);
        }

        private int GetEquipmentAttackPower(BattleStatusInfo status, BattleItemInfo battleItemInfo, EquipmentItemInfo equipment = null)
        {
            float AP = 0f;
            if (DebugUtils.IsLogBattleLog)
            {
                Debug.Log($"{nameof(battleItemInfo.TotalItemAtk)}={battleItemInfo.TotalItemAtk},{nameof(battleItemInfo.TotalItemMatk)}={battleItemInfo.TotalItemMatk}");
                Debug.Log($"{nameof(battleItemInfo.TotalItemDef)}={battleItemInfo.TotalItemDef},{nameof(battleItemInfo.TotalItemMdef)}={battleItemInfo.TotalItemMdef}");
            }
            // 공격력
            AP += (battleItemInfo.TotalItemAtk + battleItemInfo.TotalItemMatk) * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(4) * 0.01f;
            // 방어력
            AP += (battleItemInfo.TotalItemDef + battleItemInfo.TotalItemMdef) * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(5) * 0.01f;
            AP *= BasisType.ATTACK_POWER_COEFFICIENT.GetInt(6) * 0.01f;
            if (DebugUtils.IsLogBattleLog)
                Debug.Log($"장비 {nameof(AP)}={MathUtils.ToInt(AP)}");
            return MathUtils.ToInt(AP);
        }

        private int GetCardAttackPower(BattleStatusInfo status, EquipmentItemInfo virtualEquipment = null)
        {
            if (entity.Inventory == null)
                return 0;

            float AP = 0f;

            // 장비 교체
            ItemInfo[] equippedItems = entity.Inventory.GetEquippedItems();
            ItemEquipmentSlotType virtualSlotType = virtualEquipment == null ? default : virtualEquipment.SlotType;
            for (int i = 0; i < equippedItems.Length; i++)
            {
                if (equippedItems[i].SlotType == virtualSlotType)
                    continue;

                itemBuffer.Add(equippedItems[i]);
            }

            if (virtualEquipment != null)
                itemBuffer.Add(virtualEquipment);

            ItemInfo[] buffer = itemBuffer.GetBuffer(isAutoRelease: true);
            foreach (var item in buffer)
            {
                for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; ++i)
                {
                    ItemInfo card = item.GetCardItem(i);
                    if (card == null)
                        continue;

                    float cardAP = 0f;
                    cardAP += card.Rating * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(7) * 0.01f;
                    cardAP *= 1f + (card.CardLevel * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(12) * 0.01f);

                    AP += cardAP;

                }
            }

            if (DebugUtils.IsLogBattleLog)
                Debug.Log($"카드 {nameof(AP)}={MathUtils.ToInt(AP)}");

            return MathUtils.ToInt(AP);
        }

        private int GetSkillAttackPower(BattleStatusInfo status, EquipmentItemInfo equipment = null)
        {
            int totalActiveSkillLevel = 0;
            int totalPassiveSkillLevel = 0;

            var skillList = entity.Skill.skillList;
            var equippedSkillList = new List<long>();
            for (int i = 0; i < entity.Skill.SkillSlotCount; ++i)
            {
                var skillNo = entity.Skill.GetSlotInfo(i).SkillNo;
                if (skillNo == 0)
                    continue;

                equippedSkillList.Add(skillNo);
            }

            foreach (var skill in skillList)
            {
                if (!skill.IsInPossession) // 보유 중인 스킬만 해당
                    continue;

                switch (skill.SkillType)
                {
                    case SkillType.Active:
                        if (equippedSkillList.Exists(e => e == skill.SkillNo))
                        {
                            totalActiveSkillLevel += skill.SkillLevel;
                        }
                        break;
                    case SkillType.Passive:
                        totalPassiveSkillLevel += skill.SkillLevel;
                        break;
                }
            }

            float AP = 0f;
            AP += totalPassiveSkillLevel * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(8) * 0.01f;
            AP += totalActiveSkillLevel * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(9) * 0.01f;
            return MathUtils.ToInt(AP);
        }

        /// <summary>
        /// 모든 큐펫의 총 전투력
        /// </summary>
        private int GetCupetAttackPower(BattleStatusInfo status, EquipmentItemInfo equipment = null)
        {
            return 0;
        }

        /// <summary>
        /// 보유 동료 전투력
        /// </summary>
        private int GetHaveAgentAttackPower()
        {
            if (entity.Agent is null)
                return 0;

            int totalRank = 0;

            foreach (var item in entity.Agent.GetAllAgents())
            {
                if (item.AgentData == null)
                    continue;

                totalRank += item.AgentData.agent_rating;
            }

            return MathUtils.ToInt(totalRank * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(13) * 0.01f);
        }

        /// <summary>
        /// 장착 동료 전투력
        /// </summary>
        private float GetEquippedAgentAttackPower()
        {
            if (entity.Agent is null)
                return 0;

            int totalRank = 0;

            foreach (var item in entity.Agent.GetEquipedCombatAgents())
            {
                if (item.AgentData == null)
                    continue;

                totalRank += item.AgentData.agent_rating;
            }

            return totalRank * BasisType.ATTACK_POWER_COEFFICIENT.GetInt(14) * 0.01f;
        }

#if UNITY_EDITOR
        public int GetCharacterAttackPower()
        {
            return GetCharacterAttackPower(entity.preBattleStatusInfo);
        }

        public int GetEquipmentAttackPower()
        {
            return GetEquipmentAttackPower(entity.preBattleStatusInfo, entity.battleItemInfo);
        }

        public int GetCardAttackPower()
        {
            return GetCardAttackPower(entity.preBattleStatusInfo);
        }

        public int GetSkillAttackPower()
        {
            return GetSkillAttackPower(entity.preBattleStatusInfo);
        }

        public int GetCupetAttackPower()
        {
            return GetCupetAttackPower(entity.preBattleStatusInfo);
        }

        public int GetHaveAgentAttackPower(bool isTest = false)
        {
            return GetHaveAgentAttackPower();
        }

        public float GetEquippedAgentAttackPower(bool isTest = false)
        {
            return GetEquippedAgentAttackPower();
        }
#endif
    }
}