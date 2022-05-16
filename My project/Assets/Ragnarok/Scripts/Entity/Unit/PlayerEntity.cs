using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class PlayerEntity : CharacterEntity
    {
        private readonly string TAG = nameof(PlayerEntity);

        public override UnitEntityType type => UnitEntityType.Player;

        /// <summary>
        /// 최근 Reload된 Status를 보관
        /// </summary>
        BattleStatusData savedBattleStatusData;
        public BattleStatusData SavedBattleStatusData => savedBattleStatusData;

        /// <summary>
        /// 전투력 변동 이벤트
        /// </summary>
        public event System.Action<int> OnChangeAP;

        /// <summary>
        /// 게임 접속 여부
        /// </summary>
        public static bool IsJoinGameMap { get; private set; }

        public PlayerEntity()
        {
            User = Create<UserModel>();
            Character = Create<CharacterModel>();
            Status = Create<StatusModel>();
            BuffItemList = Create<BuffItemListModel>();
            EventBuff = Create<EventBuffModel>();
            Inventory = Create<InventoryModel>();
            CupetList = Create<CupetListModel>();
            Skill = Create<SkillModel>();
            Guild = Create<GuildModel>();
            Goods = Create<GoodsModel>();
            CharacterList = Create<CharacterListModel>();
            Achievement = Create<AchievementModel>();
            Daily = Create<DailyModel>();
            Dungeon = Create<DungeonModel>();
            Quest = Create<QuestModel>();
            Friend = Create<FriendModel>();
            Mail = Create<MailModel>();
            VIP = Create<VIPModel>();
            Make = Create<MakeModel>();
            AlarmModel = Create<AlarmModel>();
            ShopModel = Create<ShopModel>();
            ChatModel = Create<ChatModel>();
            RankModel = Create<RankModel>();
            Trade = Create<TradeModel>();
            Event = Create<EventModel>();
            League = Create<LeagueModel>();
            Agent = Create<AgentModel>();
            Tutorial = Create<TutorialModel>();
            Sharing = Create<SharingModel>();
            Duel = Create<DuelModel>();
            Book = Create<BookModel>();
            Bingo = Create<BingoModel>();

            SceneLoader.OnTitleSceneLoaded += ResetData;
            AssetManager.OnAllAssetReady += OnAllAssetReady;
        }

        ~PlayerEntity()
        {
            SceneLoader.OnTitleSceneLoaded -= ResetData;
            AssetManager.OnAllAssetReady -= OnAllAssetReady;
        }

        public override void Initialize()
        {
            base.Initialize();

            CharacterList.OnCharacterInit += OnCharacterInit;
        }

        public override void Dispose()
        {
            base.Dispose();

            CharacterList.OnCharacterInit -= OnCharacterInit;
        }

        public override void ResetData()
        {
            base.ResetData();

            IsJoinGameMap = false;
            Timing.KillCoroutines(TAG);
        }

        void OnAllAssetReady()
        {
            int id = BasisItem.PronteraBless.GetID();
            if (id == 0)
                return;

            ItemData itemData = ItemDataManager.Instance.Get(id);
            if (itemData == null)
                return;

            pronteraBlessBuff.SetData(itemData);
        }

        /// <summary>
        /// 캐릭터 초기화 이벤트
        /// </summary>
        void OnCharacterInit()
        {
            IsJoinGameMap = true;
            savedBattleStatusData = new BattleStatusData(player);

            duelDecreaseDamageLevel = BasisType.DUEL_DAMAGE_LEVEL.GetInt();
            duelDecreaseDamagePer = MathUtils.ToPercentValue(BasisType.DUEL_DAMAGE_PER.GetInt());
        }

        /// <summary>
        /// 전투력 변동 이벤트
        /// </summary>
        void OnUpdateAP(int receivedBattleScore)
        {
            if (savedBattleStatusData == null)
                return;

            // 바뀐 전투력 계산
            BattleStatusData afterStatusData = new BattleStatusData(player);
            OnChangeAP?.Invoke(afterStatusData.AP);

#if UNITY_EDITOR
            if (afterStatusData.AP != receivedBattleScore)
            {
                int totalAP = player.GetTotalAttackPower();
                int characterAP = player.attackPowerInfo.GetCharacterAttackPower();
                int equipmentAP = player.attackPowerInfo.GetEquipmentAttackPower();
                int cardAP = player.attackPowerInfo.GetCardAttackPower();
                int skillAP = player.attackPowerInfo.GetSkillAttackPower();
                int cupetAP = player.attackPowerInfo.GetCupetAttackPower();
                int haveAgentAp = player.attackPowerInfo.GetHaveAgentAttackPower();
                float equippedAgentAp = player.attackPowerInfo.GetEquippedAgentAttackPower();

                var sb = StringBuilderPool.Get();

                sb.Append($"[총 전투력] {totalAP}");
                sb.AppendLine();
                sb.Append("\t").Append($"[캐릭터 전투력] {characterAP}");
                sb.AppendLine();
                sb.Append("\t").Append($"[장비 전투력] {equipmentAP}");
                sb.AppendLine();
                sb.Append("\t\t").Append($"[ATK] {player.battleItemInfo.TotalItemAtk}");
                sb.AppendLine();
                sb.Append("\t\t").Append($"[DEF] {player.battleItemInfo.TotalItemDef}");
                sb.AppendLine();
                sb.Append("\t\t").Append($"[MATK] {player.battleItemInfo.TotalItemMatk}");
                sb.AppendLine();
                sb.Append("\t\t").Append($"[MDEF] {player.battleItemInfo.TotalItemMdef}");
                sb.AppendLine();
                sb.Append("\t").Append($"[카드인챈트 전투력] {cardAP}");
                sb.AppendLine();
                sb.Append("\t").Append($"[스킬 전투력] {skillAP}");
                sb.AppendLine();
                sb.Append("\t").Append($"[큐펫 전투력] {cupetAP}");
                sb.AppendLine();
                sb.Append("\t").Append($"[보유동료 전투력] {haveAgentAp}");
                sb.AppendLine();
                sb.Append("\t").Append($"[장착동료 전투력] {equippedAgentAp}");

                Debug.LogError($"전투력 싱크가 맞지 않음. 클라이언트 : {afterStatusData.AP}  서버 : {receivedBattleScore}\n{sb.Release()}");
            }
#endif

            // 게임 접속 중이 아니라면 띄우지 않습니다.
            if (!IsJoinGameMap)
                return;

            // 화면에 띄움.
            BattleStatusData difference = BattleStatusData.GetDifference(savedBattleStatusData, afterStatusData);

            if (!UIPowerUpdate.IsIgnoreOnce) // 특정 전투력 변동을 연출하고 싶지 않을 때.
            {
                if (!difference.IsZero()) // 변동치가 없으면 UI를 띄우지 않는다.
                {
                    Timing.KillCoroutines(TAG);
                    Timing.RunCoroutine(YieldWaitForRewardPopup(difference), TAG);
                }
            }
            else
            {
                UIPowerUpdate.IsIgnoreOnce = false;
            }

            savedBattleStatusData = afterStatusData;
        }

        /// <summary>
        /// 보상 관련 팝업이 끝날때까지 대기
        /// </summary>
        private IEnumerator<float> YieldWaitForRewardPopup(BattleStatusData difference)
        {
            // 이미 표시중인 UI 가 있다면 지워줍니다.
            UIPowerUpdate uiPowerUpdate = UI.GetUI<UIPowerUpdate>();
            if (uiPowerUpdate && uiPowerUpdate.IsVisible)
                uiPowerUpdate.Hide();

            yield return Timing.WaitForOneFrame;

            yield return Timing.WaitUntilFalse(() =>
            {
                UIQuestReward uiQuestReward = UI.GetUI<UIQuestReward>();
                bool isQuestReward = (uiQuestReward != null && uiQuestReward.gameObject.activeSelf);
                return (isQuestReward);
            });

            yield return Timing.WaitUntilFalse(() =>
            {
                UIContentsUnlock uiContentsUnlock = UI.GetUI<UIContentsUnlock>();

                // 한 번이라도 UISingleReward 가 뜰 때까지 기다립니다.
                if (UIContentsUnlock.attackPowerInfoDelay > 0 && (uiContentsUnlock == null || !uiContentsUnlock.gameObject.activeInHierarchy))
                {
                    UIContentsUnlock.attackPowerInfoDelay -= Time.deltaTime;
                    return true;
                }
                else
                {
                    UIContentsUnlock.attackPowerInfoDelay = 0f;
                }

                bool isContentsUnlock = (uiContentsUnlock != null && uiContentsUnlock.gameObject.activeSelf);
                return (isContentsUnlock);
            });

            yield return Timing.WaitUntilFalse(() =>
            {
                UISingleReward uiSingleReward = UI.GetUI<UISingleReward>();

                // 한 번이라도 UISingleReward 가 뜰 때까지 기다립니다.
                if (UISingleReward.attackPowerInfoDelay > 0 && (uiSingleReward == null || !uiSingleReward.gameObject.activeInHierarchy))
                {
                    UISingleReward.attackPowerInfoDelay -= Time.deltaTime;
                    return true;
                }
                else
                {
                    UISingleReward.attackPowerInfoDelay = 0f;
                }

                bool isSingleReward = (uiSingleReward != null && uiSingleReward.gameObject.activeSelf);
                return (isSingleReward);
            });

            yield return Timing.WaitUntilFalse(() =>
            {
                UIJobReward ui = UI.GetUI<UIJobReward>();

                // 한 번이라도 UIJobReward 가 뜰 때까지 기다립니다.
                if (UIJobReward.attackPowerInfoDelay > 0 && (ui == null || !ui.gameObject.activeInHierarchy))
                {
                    UIJobReward.attackPowerInfoDelay -= Time.deltaTime;
                    return true;
                }
                else
                {
                    UIJobReward.attackPowerInfoDelay = 0f;
                }

                bool isReward = (ui != null && ui.gameObject.activeSelf);
                return (isReward);
            });

            UIContentsUnlock.attackPowerInfoDelay = 0f;
            UISingleReward.attackPowerInfoDelay = 0f;
            UIJobReward.attackPowerInfoDelay = 0f;

            UI.ShowAttackPowerChange(difference);
        }

        protected override UnitActor SpawnEntityActor()
        {
            switch (State)
            {
                case UnitState.Stage:
                    return unitActorPool.SpawnPlayer();
                case UnitState.Maze:
                    return unitActorPool.SpawnMazePlayer();
                case UnitState.GVG:
                    return unitActorPool.SpawnGVGPlayer();
            }

            return unitActorPool.SpawnPlayer();
        }

        public override int GetDamageFontSize()
        {
            return Damage.PLAYER_HIT_FONT_SIZE;
        }

        protected override bool IsNeedSaveDamagePacket()
        {
            return true;
        }

        /// <summary>
        /// 서버로부터 BattleScore 수신
        /// </summary>
        public override void UpdateBattleScore(int? battleScore)
        {
            if (battleScore == null)
                return;

            OnUpdateAP(battleScore.GetValueOrDefault());
        }

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            return new DamagePacket.UnitKey(DamagePacket.DamageUnitType.Character, Character.Cid, Character.JobLevel);
        }
    }
}