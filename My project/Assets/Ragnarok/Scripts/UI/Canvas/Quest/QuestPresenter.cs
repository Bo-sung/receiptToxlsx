using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIQuest"/>
    /// </summary>
    public sealed class QuestPresenter : ViewPresenter
    {
        private const string TAG = nameof(QuestPresenter);

        public interface IView
        {
            void UpdateView();
            void ShowNewView();
            void CloseUI();
        }

        private readonly IView view;
        private readonly QuestModel questModel;
        private readonly InventoryModel inventoryModel;
        private readonly GuildModel guildModel;
        private readonly DungeonModel dungeonModel;
        private readonly GoodsModel goodsModel;
        private readonly UserModel userModel;
        private readonly BattleManager battleManager;

        public int GuildQuestRewardLimit => questModel.GuildQuestRewardMaxCount;
        public int GuildQestRewardCount => questModel.GetGuildQuestRewardCount();

        public bool IsDailyQuestCleared => userModel.IsDailyQuestCleared;

        bool isShortcutStart;

        public QuestPresenter(IView view)
        {
            this.view = view;
            questModel = Entity.player.Quest;
            inventoryModel = Entity.player.Inventory;
            guildModel = Entity.player.Guild;
            dungeonModel = Entity.player.Dungeon;
            goodsModel = Entity.player.Goods;
            userModel = Entity.player.User;
            battleManager = BattleManager.Instance;
        }

        public override void AddEvent()
        {
            questModel.OnStandByReward += view.ShowNewView;
            questModel.OnNormalQuestFree += view.ShowNewView;
            questModel.OnUpdateMainQuest += view.UpdateView;
            goodsModel.OnUpdateNormalQuestCoin += OnUpdateNormalQuestCoin;
            BattleManager.OnStart += OnStartBattle;
        }

        public override void RemoveEvent()
        {
            questModel.OnStandByReward -= view.ShowNewView;
            questModel.OnNormalQuestFree -= view.ShowNewView;
            questModel.OnUpdateMainQuest -= view.UpdateView;
            goodsModel.OnUpdateNormalQuestCoin -= OnUpdateNormalQuestCoin;
            BattleManager.OnStart -= OnStartBattle;
        }

        void OnStartBattle(BattleMode mode)
        {
            if (isShortcutStart)
            {
                isShortcutStart = false;
                view.CloseUI();
            }
        }

        /// <summary>
        /// 길드 가입 여부 반환
        /// </summary>
        public bool IsHaveGuild()
        {
            return guildModel.HaveGuild;
        }

        public QuestInfo GetMainQuest()
        {
            return questModel.GetMaintQuest();
        }

        /// <summary>
        /// 그룹별 일일 퀘스트 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetDailyQuests()
        {
            return questModel.GetDailyQuests();
        }

        /// <summary>
        /// 길드 퀘스트 반환
        /// </summary>
        public QuestInfo[] GetGuildQuests()
        {
            return questModel.GetGuildQuests();
        }

        /// <summary>
        /// 모든 일일 퀘스트 클리어 정보 반환
        /// </summary>
        public QuestInfo GetDailyTotalClearQuest()
        {
            return questModel.GetDailyTotalClearQuest();
        }

        /// <summary>
        /// 그룹별 업적 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetAchievements()
        {
            return questModel.GetAchievements();
        }

        /// <summary>
        /// 업적 진행도
        /// </summary>
        public float GetAchievementProgress()
        {
            return questModel.GetAchievementProgress();
        }

        /// <summary>
        /// 모든 의뢰 퀘스트 클리어 정보 반환
        /// </summary>
        /// <returns></returns>
        public QuestInfo[] GetNormalQuests()
        {
            return questModel.GetNormalQuests();
        }

        /// <summary>
        /// 보상 받을수 있는 퀘스트 수
        /// </summary>
        /// <returns></returns>
        public Dictionary<QuestCategory, int> GetStandByRewards()
        {
            return questModel.GetStandByRewards();
        }

        /// <summary>
        /// 퀘스트 보상 받기
        /// </summary>
        public void RequestQuestReward(QuestInfo info)
        {
            RequestQuestRewardAsync(info).WrapNetworkErrors();
        }

        /// <summary>
        /// 보상 받기
        /// </summary>
        private async Task RequestQuestRewardAsync(QuestInfo info)
        {
            // QuestInfo의 내용이 변경되기 때문에 미리 데이터를 받아둬야한다..
            List<RewardInfo> rewardList = new List<RewardInfo>();
            QuestCategory category = info.Category;
            int questId = info.ID;

            if (category == QuestCategory.Main)
            {
                foreach (var item in info.RewardDatas)
                {
                    if (item == null || item.Count == 0)
                        continue;
                    int id = item.RewardType == RewardType.Item ? (int)item.ItemData.id : item.Count;
                    RewardInfo ri = new RewardInfo(item.RewardType, id, item.Count);
                    rewardList.Add(ri);
                }
            }

            Response response = await questModel.RequestQuestRewardAsync(info);

            if (response.isSuccess)
            {
                // 메인퀘스트 보상 UI 임시 적용.
                if (category == QuestCategory.Main && response != default)
                {
                    view.CloseUI();
                    //if (response != null)
                    //{
                    //    UIResultClearData param = new UIResultClearData();
                    //    param.rewardList = rewardList.ConvertAll(a => a.data).ToArray();
                    //    //param.isMainQuestClear = true;
                    //    UI.Show<UIResultClear>(param);
                    //}
                }
                else
                {
                    // 일일퀘스트 최종 보상 받을 시 Flag 처리.
                    if (category == QuestCategory.DailyStart && questId == questModel.GetDailyTotalClearQuest().ID)
                    {
                        userModel.IsDailyQuestCleared = true;
                    }

                    view.UpdateView();
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }        

        public ItemInfo GetItemInfo(QuestInfo info)
        {
            if (info.QuestType != QuestType.ITEM_GAIN)
                return null;

            return inventoryModel.CreateItmeInfo(info.ConditionValue);
        }

        /// <summary>
        /// 해당 스테이지로 바로 이동 가능 여부
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        public bool IsShortStage(int stageId)
        {
            if (!dungeonModel.IsStageOpend(stageId))
            {
                stageId = dungeonModel.FinalStageId + 1;
            }
            return !IsPlayingStage(stageId);
        }

        /// <summary>
        /// 스테이지 진행중 여부
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        private bool IsPlayingStage(int stageId)
        {
            return dungeonModel.LastEnterStageId == stageId;
        }

        public void OnClickedBtnShortCut()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            QuestInfo info = questModel.GetMaintQuest();
            if (info == null || info.IsInvalidData)
                return;

            if (info.ShortCutType == ShortCutType.Stage)
            {
                int stageId = info.ShortCutValue;

                if (!dungeonModel.IsStageOpend(stageId))
                {
                    stageId = dungeonModel.FinalStageId + 1;
                }

                if (IsPlayingStage(stageId))
                {
                    Debug.LogError($"진행중인 스테이지 = {stageId}");
                    return;
                }

                dungeonModel.StartBattleStageMode(StageMode.Normal, stageId);
                isShortcutStart = true;
                return;
            }

            info.GoShortCut();
        }

        /// <summary>
        /// 보상 받을 수 있는 길드 퀘스트 있는지 여부
        /// </summary>
        /// <returns></returns>
        public bool HasGuildQuestReward()
        {
            return questModel.HasGuildQuestReward();
        }

        private void OnUpdateNormalQuestCoin(int questCoin)
        {
            view.UpdateView();
            view.ShowNewView();
        }
    }
}