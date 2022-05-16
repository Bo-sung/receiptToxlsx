using Ragnarok.View.Home;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIHome"/>
    /// </summary>
    public class HomePresenter : ViewPresenter
    {
        private readonly ConnectionManager connectionManager;
        private readonly GoodsModel goodsModel;
        private readonly GuildModel guildModel;
        private readonly SkillModel skillModel;
        private readonly EventModel eventModel;
        private readonly CharacterModel characterModel;
        private readonly QuestModel questModel;
        private readonly DungeonModel dungeonModel;
        private readonly BookModel bookModel;
        private readonly ShopModel shopModel;
        private readonly int needGuildJobLevel;

        public event System.Action OnUpdateGoodsZeny;
        public event System.Action OnUpdateGoodsCatCoin;
        public event System.Action OnUpdateJobLevel;
        public event System.Action OnUpdateSkillPoint;

        public event System.Action OnUpdateDungeonFree
        {
            add { dungeonModel.OnUpdateTicket += value; }
            remove { dungeonModel.OnUpdateTicket -= value; }
        }

        public event System.Action OnUpdateGuideQuest
        {
            add { questModel.OnUpdateMainQuest += value; }
            remove { questModel.OnUpdateMainQuest -= value; }
        }

        public event System.Action OnUpdateNewOpenContent
        {
            add { questModel.OnUpdateNewOpenContent += value; }
            remove { questModel.OnUpdateNewOpenContent -= value; }
        }

        public event System.Action<BookTabType> OnBookStateChanged
        {
            add { bookModel.OnBookStateChange += value; }
            remove { bookModel.OnBookStateChange -= value; }
        }

        public event System.Action OnTamingMazeOpen;

        public event System.Action OnPurchaseSuccess
        {
            add { shopModel.OnPurchaseSuccess += value; }
            remove { shopModel.OnPurchaseSuccess -= value; }
        }

        public event System.Action OnResetFreeItemBuyCount
        {
            add { shopModel.OnResetFreeItemBuyCount += value; }
            remove { shopModel.OnResetFreeItemBuyCount -= value; }
        }

        public event System.Action OnUpdateEndlessTowerFreeTicket
        {
            add { dungeonModel.OnUpdateEndlessTowerFreeTicket += value; }
            remove { dungeonModel.OnUpdateEndlessTowerFreeTicket -= value; }
        }

        public HomePresenter()
        {
            connectionManager = ConnectionManager.Instance;
            goodsModel = Entity.player.Goods;
            guildModel = Entity.player.Guild;
            skillModel = Entity.player.Skill;
            eventModel = Entity.player.Event;
            characterModel = Entity.player.Character;
            questModel = Entity.player.Quest;
            dungeonModel = Entity.player.Dungeon;
            bookModel = Entity.player.Book;
            shopModel = Entity.player.ShopModel;

            needGuildJobLevel = BasisType.GUILD_JOIN_NEED_JOB_LEVEL.GetInt();
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += InvokeUpdateGoodsZeny;
            goodsModel.OnUpdateCatCoin += InvokeUpdateGoodsCatCoin;
            characterModel.OnUpdateJobLevel += OnUpdateCharacterJobLevel;
            skillModel.OnUpdateHasNewSkillPoint += OnUpdateCharacterSkillPoint;
            guildModel.OnTamingMazeOpen += InvokeUpdateTamingMazeOpen;
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= InvokeUpdateGoodsZeny;
            goodsModel.OnUpdateCatCoin -= InvokeUpdateGoodsCatCoin;
            characterModel.OnUpdateJobLevel -= OnUpdateCharacterJobLevel;
            skillModel.OnUpdateHasNewSkillPoint -= OnUpdateCharacterSkillPoint;
            guildModel.OnTamingMazeOpen -= InvokeUpdateTamingMazeOpen;
        }

        void InvokeUpdateGoodsZeny(long value)
        {
            OnUpdateGoodsZeny?.Invoke();
        }

        void InvokeUpdateGoodsCatCoin(long value)
        {
            OnUpdateGoodsCatCoin?.Invoke();
        }

        void OnUpdateCharacterJobLevel(int value)
        {
            OnUpdateJobLevel?.Invoke();
        }

        void OnUpdateCharacterSkillPoint()
        {
            OnUpdateSkillPoint?.Invoke();
        }

        void InvokeUpdateTamingMazeOpen(bool isOpen)
        {
            OnTamingMazeOpen?.Invoke();
        }

        /// <summary>
        /// 제니
        /// </summary>
        public long GetZeny()
        {
            return goodsModel.Zeny;
        }

        /// <summary>
        /// 냥다래
        /// </summary>
        public long GetCatCoin()
        {
            return goodsModel.CatCoin;
        }

        /// <summary>
        /// 컨텐츠 오픈 여부
        /// </summary>
        public bool IsOpenContent(UIHome.MenuContent content, bool isShowPopup)
        {
            switch (content)
            {
                case UIHome.MenuContent.Skill:
                    return questModel.IsOpenContent(ContentType.Skill, isShowPopup);

                case UIHome.MenuContent.Guild:
                    return characterModel.IsCheckJobLevel(needGuildJobLevel, isShowPopup);

                case UIHome.MenuContent.Dungeon:
                    return questModel.IsOpenContent(ContentType.Dungeon, isShowPopup);

                case UIHome.MenuContent.FreeFight:
                    return true;

                case UIHome.MenuContent.Pvp:
                    return questModel.IsOpenContent(ContentType.Pvp, isShowPopup);

                case UIHome.MenuContent.GuildBattle:
                    return characterModel.IsCheckJobLevel(needGuildJobLevel, isShowPopup);

                case UIHome.MenuContent.Rank:
                    return true;

                case UIHome.MenuContent.Settings:
                    return true;

                case UIHome.MenuContent.Book:
                    return true;

                case UIHome.MenuContent.Community:
                    return true;

                case UIHome.MenuContent.NaverLounge:
                    return true;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIHome.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 알림 표시 여부
        /// </summary>
        public bool GetHasNotice(UIHome.MenuContent content)
        {
            // 컨텐츠가 오픈되지 않았다면 Notice를 띄워주지 않습니다.
            if (!IsOpenContent(content, isShowPopup: false))
                return false;

            switch (content)
            {
                case UIHome.MenuContent.Skill:
                    return HasSkillNotice();

                case UIHome.MenuContent.Guild:
                    return HasGuildNotice();

                case UIHome.MenuContent.Dungeon:
                    return HasDungeonNotice();

                case UIHome.MenuContent.FreeFight:
                    return false;

                case UIHome.MenuContent.Pvp:
                    return false;

                case UIHome.MenuContent.GuildBattle:
                    return false;

                case UIHome.MenuContent.Rank:
                    return false;

                case UIHome.MenuContent.Settings:
                    return false;

                case UIHome.MenuContent.Book:
                    return HasBookNotice();

                case UIHome.MenuContent.Community:
                    return false;

                case UIHome.MenuContent.NaverLounge:
                    return false;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIHome.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 신규 컨텐츠 여부
        /// </summary>
        public bool GetHasNewIcon(UIHome.MenuContent content)
        {
            switch (content)
            {
                case UIHome.MenuContent.Skill:
                    return questModel.HasNewOpenContent(ContentType.Skill);

                case UIHome.MenuContent.Guild:
                    return questModel.HasNewOpenContent(ContentType.Guild);

                case UIHome.MenuContent.Dungeon:
                    return questModel.HasNewOpenContent(ContentType.Dungeon);

                case UIHome.MenuContent.FreeFight:
                    return false;

                case UIHome.MenuContent.Pvp:
                    return questModel.HasNewOpenContent(ContentType.Pvp);

                case UIHome.MenuContent.GuildBattle:
                    return false;

                case UIHome.MenuContent.Rank:
                    return false;

                case UIHome.MenuContent.Settings:
                    return false;

                case UIHome.MenuContent.Book:
                    return false;

                case UIHome.MenuContent.Community:
                    return false;

                case UIHome.MenuContent.NaverLounge:
                    return false;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIHome.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        public UIHomeButton.LockType GetLockType(UIHome.MenuContent content)
        {
            switch (content)
            {
                case UIHome.MenuContent.Skill:
                    return UIHomeButton.LockType.MainQuest;

                case UIHome.MenuContent.Guild:
                    return UIHomeButton.LockType.JobLevel;

                case UIHome.MenuContent.Dungeon:
                    return UIHomeButton.LockType.MainQuest;

                case UIHome.MenuContent.FreeFight:
                    return UIHomeButton.LockType.None;

                case UIHome.MenuContent.Pvp:
                    return UIHomeButton.LockType.MainQuest;

                case UIHome.MenuContent.GuildBattle:
                    return UIHomeButton.LockType.JobLevel;

                case UIHome.MenuContent.Rank:
                    return UIHomeButton.LockType.None;

                case UIHome.MenuContent.Settings:
                    return UIHomeButton.LockType.None;

                case UIHome.MenuContent.Book:
                    return UIHomeButton.LockType.None;

                case UIHome.MenuContent.Community:
                    return UIHomeButton.LockType.None;

                case UIHome.MenuContent.NaverLounge:
                    return UIHomeButton.LockType.None;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIHome.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 던전 오픈 조건 텍스트 반환
        /// </summary>
        public string GetOpenConditionalText(UIHome.MenuContent content)
        {
            UIHomeButton.LockType lockType = GetLockType(content);
            if (lockType == UIHomeButton.LockType.None)
                return string.Empty;

            int conditionValue = GetOpenConditionValue(content);

            switch (lockType)
            {
                case UIHomeButton.LockType.MainQuest:
                    return LocalizeKey._2511.ToText() // [c][FF6C88]QUEST {VALUE}[-][/c]
                        .Replace(ReplaceKey.VALUE, conditionValue);

                case UIHomeButton.LockType.JobLevel:
                    return LocalizeKey._2512.ToText() // [c][FFB625]JOB Lv.{LEVEL}[-][/c]
                        .Replace(ReplaceKey.LEVEL, conditionValue);

                case UIHomeButton.LockType.Update:
                    return LocalizeKey._2513.ToText(); // [c][4E7FEB]UPDATE[-][/c]

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIHome.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 길드 가입 유무
        /// </summary>
        public bool HasGuild()
        {
            return guildModel.HaveGuild;
        }

        /// <summary>
        /// 배너데이터 리스트 반환
        /// </summary>
        public IEventBanner[] GetEventBannerArrayData()
        {
            return eventModel.GetEventBanners();
        }

        private int GetOpenConditionValue(UIHome.MenuContent content)
        {
            switch (content)
            {
                case UIHome.MenuContent.Skill:
                    return GetMainQuestId(ContentType.Skill);

                case UIHome.MenuContent.Guild:
                    return needGuildJobLevel;

                case UIHome.MenuContent.Dungeon:
                    return GetMainQuestId(ContentType.Dungeon);

                case UIHome.MenuContent.FreeFight:
                    return 0;

                case UIHome.MenuContent.Pvp:
                    return GetMainQuestId(ContentType.Pvp);

                case UIHome.MenuContent.GuildBattle:
                    return needGuildJobLevel;

                case UIHome.MenuContent.Rank:
                    return 0;

                case UIHome.MenuContent.Settings:
                    return 0;

                case UIHome.MenuContent.Book:
                    return 0;

                case UIHome.MenuContent.Community:
                    return 0;

                case UIHome.MenuContent.NaverLounge:
                    return 0;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIHome.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        private int GetMainQuestId(ContentType content)
        {
            QuestInfo info = questModel.GetNeedQuest(content);
            if (info == null)
                return 0;

            return info.GetMainQuestGroup();
        }

        /// <summary>
        /// 무료 입장 가능한 던전이 있을 경우 true 반환, +@ 무료 보상이 있을 경우도 추가함
        /// </summary>
        private bool HasDungeonNotice()
        {
            foreach (DungeonType item in System.Enum.GetValues(typeof(DungeonType)))
            {
                // 멀티미로는 던전UI에 속해있지 않다.
                if (item == DungeonType.MultiMaze || item == DungeonType.EventMultiMaze || item == DungeonType.ForestMaze || item == DungeonType.Gate)
                    continue;

                if (dungeonModel.IsOpened(item, isShowPopup: false) && // 컨텐츠 해금상태
                    (dungeonModel.IsFreeEntry(item) || dungeonModel.PossibleFreeReward(item))) // 무료입장 가능하거나 무료아이템을 받을 수 있을때
                    return true;
            }

            return false;
        }

        private bool HasBookNotice()
        {
            return bookModel.IsThereAvailableLevelUp();
        }

        /// <summary>
        /// 레벨업 가능한 스킬 존재 여부
        /// </summary>
        private bool HasSkillNotice()
        {
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
        /// 길드전 버튼 클릭 이벤트
        /// </summary>
        public void OnClickedBtnGuildBattle()
        {
            guildModel.RequestGuildBattleSeasonInfo().WrapNetworkErrors();
        }

        public bool IsTaiwan()
        {
            return connectionManager.IsTaiwan();
        }
    }
}