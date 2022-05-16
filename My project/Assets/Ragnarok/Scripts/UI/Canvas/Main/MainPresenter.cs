using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIMain"/>
    /// </summary>
    public sealed class MainPresenter : ViewPresenter
    {
        private readonly GoodsModel goodsModel;
        private readonly AlarmModel alarmModel;
        private readonly QuestModel questModel;
        private readonly ShopModel shopModel;
        private readonly UserModel userModel;
        private readonly ChatModel chatModel;
        private readonly InventoryModel inventoryModel;
        private readonly MakeModel makeModel;
        private readonly DungeonModel dungeonModel;
        private readonly SkillModel skillModel;
        private readonly GuildModel guildModel;
        private readonly CharacterModel characterModel;

        public event System.Action OnUpdateGoodsZeny;

        public event InventoryModel.ItemUpdateEvent OnUpdateInvenItem
        {
            add { inventoryModel.OnUpdateItem += value; }
            remove { inventoryModel.OnUpdateItem -= value; }
        }

        public event System.Action OnUpdateAlarm;

        public event System.Action OnUpdateDungeonTicket
        {
            add { dungeonModel.OnUpdateTicket += value; }
            remove { dungeonModel.OnUpdateTicket -= value; }
        }

        public event System.Action OnUpdateNewOpenContent
        {
            add { questModel.OnUpdateNewOpenContent += value; }
            remove { questModel.OnUpdateNewOpenContent -= value; }
        }

        public event System.Action OnUpdateHasNewSkillPoint
        {
            add { skillModel.OnUpdateHasNewSkillPoint += value; }
            remove { skillModel.OnUpdateHasNewSkillPoint -= value; }
        }

        public event System.Action OnEquipmentAttackPowerTableUpdate
        {
            add { inventoryModel.EquipmentItemAttackPowerInfo.OnAttackPowerTableUpdate += value; }
            remove { inventoryModel.EquipmentItemAttackPowerInfo.OnAttackPowerTableUpdate -= value; }
        }

        public event InventoryModel.CostumeEvent OnUpdateCostume
        {
            add { inventoryModel.OnUpdateCostume += value; }
            remove { inventoryModel.OnUpdateCostume -= value; }
        }

        public event System.Action OnTamingMazeOpen;
        public event System.Action OnUpdateJobLevel;

        public event System.Action OnPurchaseSuccess
        {
            add { shopModel.OnPurchaseSuccess += value; }
            remove { shopModel.OnPurchaseSuccess -= value; }
        }

        public event System.Action OnUpdateShopSecretFree
        {
            add { shopModel.OnSecretShopFree += value; }
            remove { shopModel.OnSecretShopFree -= value; }
        }

        public event System.Action OnUpdateMileageReward
        {
            add { shopModel.OnUpdateMileageReward += value; }
            remove { shopModel.OnUpdateMileageReward -= value; }
        }

        public event System.Action OnRewardPackageAchieve
        {
            add { shopModel.OnRewardPackageAchieve += value; }
            remove { shopModel.OnRewardPackageAchieve -= value; }
        }

        public event System.Action OnUpdateShopMail
        {
            add { shopModel.OnUpdateShopMail += value; }
            remove { shopModel.OnUpdateShopMail -= value; }
        }

        public event System.Action OnUpdateClearedStage
        {
            add { dungeonModel.OnUpdateClearedStage += value; }
            remove { dungeonModel.OnUpdateClearedStage -= value; }
        }

        public event System.Action OnResetFreeItemBuyCount
        {
            add { shopModel.OnResetFreeItemBuyCount += value; }
            remove { shopModel.OnResetFreeItemBuyCount -= value; }
        }

        public event System.Action OnUpdateEveryDayGoods
        {
            add { shopModel.OnUpdateEveryDayGoods += value; }
            remove { shopModel.OnUpdateEveryDayGoods -= value; }
        }

        public event System.Action<bool> OnUpdateAllChatNotice
        {
            add { chatModel.OnUpdateAllChatNotice += value; }
            remove { chatModel.OnUpdateAllChatNotice -= value; }
        }

        public event System.Action OnUpdateEndlessTowerFreeTicket
        {
            add { dungeonModel.OnUpdateEndlessTowerFreeTicket += value; }
            remove { dungeonModel.OnUpdateEndlessTowerFreeTicket -= value; }
        }

        public event System.Action OnStandByReward
        {
            add { questModel.OnStandByReward += value; }
            remove { questModel.OnStandByReward -= value; }
        }

        public event System.Action OnUpdatePassExp;
        public event System.Action OnUpdatePassReward;

        public MainPresenter()
        {
            goodsModel = Entity.player.Goods;
            alarmModel = Entity.player.AlarmModel;
            questModel = Entity.player.Quest;
            shopModel = Entity.player.ShopModel;
            userModel = Entity.player.User;
            chatModel = Entity.player.ChatModel;
            inventoryModel = Entity.player.Inventory;
            makeModel = Entity.player.Make;
            dungeonModel = Entity.player.Dungeon;
            skillModel = Entity.player.Skill;
            guildModel = Entity.player.Guild;
            characterModel = Entity.player.Character;
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += InvokeUpdateGoodsZeny;
            alarmModel.OnAlarm += InvokeUpdateAlarm;
            guildModel.OnTamingMazeOpen += InvokeUpdateTamingMazeOpen;
            characterModel.OnUpdateJobLevel += InvokeUpdateJobLevel;

            shopModel.AddUpdatePassExpEvent(PassType.Labyrinth, InvokeUpdatePassExp);
            shopModel.AddUpdatePassExpEvent(PassType.OnBuff, InvokeUpdatePassExp);
            shopModel.AddUpdatePassRewardEvent(PassType.Labyrinth, InvokeUpdatePassReward);
            shopModel.AddUpdatePassRewardEvent(PassType.OnBuff, InvokeUpdatePassReward);
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= InvokeUpdateGoodsZeny;
            alarmModel.OnAlarm -= InvokeUpdateAlarm;
            guildModel.OnTamingMazeOpen -= InvokeUpdateTamingMazeOpen;
            characterModel.OnUpdateJobLevel -= InvokeUpdateJobLevel;

            shopModel.RemoveUpdatePassExpEvent(PassType.Labyrinth, InvokeUpdatePassExp);
            shopModel.RemoveUpdatePassExpEvent(PassType.OnBuff, InvokeUpdatePassExp);
            shopModel.RemoveUpdatePassRewardEvent(PassType.Labyrinth, InvokeUpdatePassReward);
            shopModel.RemoveUpdatePassRewardEvent(PassType.OnBuff, InvokeUpdatePassReward);
        }

        void InvokeUpdateGoodsZeny(long value)
        {
            OnUpdateGoodsZeny?.Invoke();
        }

        void InvokeUpdateAlarm(AlarmType alarmType)
        {
            OnUpdateAlarm?.Invoke();
        }

        void InvokeUpdateTamingMazeOpen(bool isOpen)
        {
            OnTamingMazeOpen?.Invoke();
        }

        void InvokeUpdateJobLevel(int jobLevel)
        {
            OnUpdateJobLevel?.Invoke();
        }

        private void InvokeUpdatePassExp()
        {
            OnUpdatePassExp?.Invoke();
        }

        private void InvokeUpdatePassReward()
        {
            OnUpdatePassReward?.Invoke();
        }

        public void RequestItemShopLimitList()
        {
            shopModel.RequestShopInfoList().WrapNetworkErrors();
        }

        /// <summary>
        /// 알림 표시 여부
        /// </summary>
        public bool GetHasNotice(UIMain.MenuContent content)
        {
            switch (content)
            {
                case UIMain.MenuContent.Menu:
                    return HasDungeonNotice() || HasSkillNotice() || HasGuildNotice();

                case UIMain.MenuContent.CharacterInfo:
                    return HasStrongerEquipment() || EquipableCostume(); // 장착 가능한 코스튬이 있을때도 표시

                case UIMain.MenuContent.Inven:
                    return inventoryModel.itemList.Exists(a => a.IsNew);

                case UIMain.MenuContent.Make:
                    return HasMakeNotice();

                case UIMain.MenuContent.Shop:
                    return HasShopNotice();

                case UIMain.MenuContent.Chat:
                    return HasChatNotice();

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIMain.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 신규 컨텐츠 여부
        /// </summary>
        public bool GetHasNewIcon(UIMain.MenuContent content)
        {
            switch (content)
            {
                case UIMain.MenuContent.Menu:
                    return HasNewIcon(ContentType.Skill, ContentType.Dungeon, ContentType.Cupet, ContentType.CombatAgent, ContentType.ZenyDungeon, ContentType.ExpDungeon, ContentType.Pvp, ContentType.Guild, ContentType.FreeFight);

                case UIMain.MenuContent.CharacterInfo:
                    return false;

                case UIMain.MenuContent.Inven:
                    return HasNewIcon(ContentType.Rebirth, ContentType.ItemEnchant);

                case UIMain.MenuContent.Make:
                    return HasNewIcon(ContentType.Make);

                case UIMain.MenuContent.Shop:
                    return HasNewIcon(ContentType.SecretShop);

                case UIMain.MenuContent.Chat:
                    return false;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIMain.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 무료 입장 가능한 던전이 있을 경우 true 반환
        /// </summary>
        private bool HasDungeonNotice()
        {
            // 컨텐츠가 오픈되지 않았다면 Notice를 띄워주지 않습니다.
            if (!questModel.IsOpenContent(ContentType.Dungeon, isShowPopup: false))
                return false;

            foreach (DungeonType item in System.Enum.GetValues(typeof(DungeonType)))
            {
                // 멀티미로는 던전UI에 속해있지 않다.
                if (item == DungeonType.MultiMaze || item == DungeonType.EventMultiMaze || item == DungeonType.ForestMaze || item == DungeonType.Gate)
                    continue;

                if (dungeonModel.IsFreeEntry(item) && dungeonModel.IsOpened(item, isShowPopup: false))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 레벨업 가능한 스킬 존재 여부
        /// </summary>
        private bool HasSkillNotice()
        {
            // 컨텐츠가 오픈되지 않았다면 Notice를 띄워주지 않습니다.
            if (!questModel.IsOpenContent(ContentType.Skill, isShowPopup: false))
                return false;

            return skillModel.HasNewSkillPoint;
        }

        /// <summary>
        /// 길드 아지트 알림 여부
        /// </summary>
        private bool HasGuildNotice()
        {
            if (!guildModel.HaveGuild)
                return false;

            return guildModel.HasTamingMazeNotice || shopModel.HasFreeGuildShopItem();
        }

        /// <summary>
        /// 착용중인 장비보다 더 강한 장비 존재 여부
        /// </summary>
        /// <returns></returns>
        private bool HasStrongerEquipment()
        {
            return inventoryModel.HasStrongerEquipment();
        }

        /// <summary>
        /// 착용 가능한 코스튬이 있는지(장착된 슬롯 제외.)
        /// </summary>
        private bool EquipableCostume()
        {
            return inventoryModel.EquipableCostume();
        }

        private bool HasMakeNotice()
        {
            // 컨텐츠가 오픈되지 않았다면 Notice를 띄워주지 않습니다.
            if (!questModel.IsOpenContent(ContentType.Make, isShowPopup: false))
                return false;

            return makeModel.IsMake();
        }

        private bool HasShopNotice()
        {
            // 비밀 상점 무료 변경
            if (shopModel.IsSecretShopFree())
                return true;

            // 무료 상점 아이템 존재
            if (shopModel.HasFreeShopItem())
                return true;

            // 수령 가능한 마일리지 보상 존재
            if (shopModel.HasMileageReward())
                return true;

            // 수령 가능한 패키지 상점 보상 존재
            foreach (PackageType item in System.Enum.GetValues(typeof(PackageType)))
            {
                if (item == PackageType.None)
                    continue;

                if (shopModel.HasPackageNotice(item))
                    return true;
            }

            if (HasPassNotice(PassType.Labyrinth))
                return true;

            if (HasPassNotice(PassType.OnBuff))
                return true;

            return false;
        }

        private bool HasPassNotice(PassType passType)
        {
            // 패스 시즌 진행중일때만 체크
            if (shopModel.IsBattlePass(passType))
            {
                // 수령 가능한 패스 보상 존재
                if (shopModel.IsPassRewardNotice(passType))
                    return true;

                // 수령 가능한 패스 퀘스트 존재
                if (questModel.IsPassQuestStandByReward(passType))
                    return true;
            }

            return false;
        }

        private bool HasChatNotice()
        {
            return chatModel.HasNewChatting(allCheck: true);
        }

        private bool HasNewIcon(params ContentType[] args)
        {
            foreach (var item in args)
            {
                if (questModel.HasNewOpenContent(item))
                    return true;
            }

            return false;
        }
    }
}