using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIAdventureMazeSelect"/>
    /// </summary>
    public sealed class AdventureMazeSelectPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GoodsModel goodsModel;
        private readonly DungeonModel dungeonModel;
        private readonly QuestModel questModel;

        // <!-- Repositories --!>
        private readonly MultiMazeWaitingRoomDataManager multiMazeWaitingRoomDataRepo;
        private readonly StageDataManager stageDataRepo;
        private readonly AdventureDataManager adventureDataRepo;
        private readonly MultiMazeDataManager multiMazeDataRepo;
        private readonly Dictionary<int, MultiMazeWaitingRoomData[]> dic;

        // <!-- Managers --!>
        private readonly BattleManager battleManager;

        // <!-- Event --!>
        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public event System.Action OnUpdateClearedStage;

        public AdventureMazeSelectPresenter()
        {
            goodsModel = Entity.player.Goods;
            dungeonModel = Entity.player.Dungeon;
            questModel = Entity.player.Quest;

            multiMazeWaitingRoomDataRepo = MultiMazeWaitingRoomDataManager.Instance;
            stageDataRepo = StageDataManager.Instance;
            adventureDataRepo = AdventureDataManager.Instance;
            multiMazeDataRepo = MultiMazeDataManager.Instance;
            dic = new Dictionary<int, MultiMazeWaitingRoomData[]>(IntEqualityComparer.Default);

            battleManager = BattleManager.Instance;
        }

        public override void AddEvent()
        {
            dungeonModel.OnUpdateClearedStage += InvokeClearedStage;
        }

        public override void RemoveEvent()
        {
            dungeonModel.OnUpdateClearedStage -= InvokeClearedStage;
        }

        void InvokeClearedStage()
        {
            RefreshElement();
            OnUpdateClearedStage?.Invoke();
        }

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContent_Maze()
        {
            questModel.RemoveNewOpenContent(ContentType.Maze); // 신규 컨텐츠 플래그 제거 (미로)
        }

        /// <summary>
        /// 마지막 입장한 모험 그룹
        /// </summary>
        public int GetAdventureGroup()
        {
            int stageId = battleManager.Mode == BattleMode.TimePatrol ? BasisType.TP_OPEN_STAGE_ID.GetInt() : dungeonModel.LastEnterStageId;
            int chapter = GetChapter(stageId);
            return GetAdventureGroupByChapter(chapter);
        }

        /// <summary>
        /// MultiMazeWaitingRoom Id 에 해당하는 모험 그룹
        /// </summary>
        public int GetAdventureGroupByMultiMazeWaitingRoom(int multiMazeWaitingRoomId)
        {
            MultiMazeWaitingRoomData data = multiMazeWaitingRoomDataRepo.Get(multiMazeWaitingRoomId);
            return data == null ? 1 : data.GetGroup();
        }

        /// <summary>
        /// Index 반환
        /// </summary>
        public int FindIndex(int adventureGroup, int multiMazeWaitingRoomId)
        {
            if (!dic.ContainsKey(adventureGroup))
                return 0;

            for (int i = 0; i < dic[adventureGroup].Length; i++)
            {
                if (dic[adventureGroup][i].Id == multiMazeWaitingRoomId)
                    return i;
            }

            return 0;
        }

        /// <summary>
        /// 모험 그룹에 해당하는 Element
        /// </summary>
        public UIAdventureMazeElement.IInput[] GetData(int adventureGroup)
        {
            if (!dic.ContainsKey(adventureGroup))
            {
                dic.Add(adventureGroup, multiMazeWaitingRoomDataRepo.GetData(adventureGroup));
                RefreshElement(adventureGroup);
            }

            return dic[adventureGroup];
        }

        /// <summary>
        /// 선택
        /// </summary>
        public void Select(int id)
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            // 타임패트롤
            if (id == MultiMazeWaitingRoomData.TIME_PATROL)
            {
                if (!dungeonModel.IsOpenTimePatrol(isShowPopup: true))
                    return;

                UI.Show<UITimePatrol>();
                return;
            }

            MultiMazeWaitingRoomData data = multiMazeWaitingRoomDataRepo.Get(id);
            if (data == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"해당 미로대기실 데이터가 존재하지 않음: {id}");
#endif
                return;
            }

            battleManager.StartBattle(BattleMode.MultiMazeLobby, data.GetMultiMazeDataId());
            Analytics.TrackEvent(TrackType.LabyrinthEnter); // 미궁섬 입장
        }

        /// <summary>
        /// Stgae 에 해당하는 Chapter 반환 (챕터는 최소 1 이상)
        /// </summary>
        private int GetChapter(int stageId)
        {
            StageData data = stageDataRepo.Get(stageId);
            if (data == null)
                return 1;

            return data.chapter;
        }

        /// <summary>
        /// 챕터에 해당하는 모험 Group
        /// </summary>
        private int GetAdventureGroupByChapter(int chapter)
        {
            AdventureData data = adventureDataRepo.GetChapterData(chapter);
            if (data == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"해당 Chapter에 해당하는 AdventureData가 존재하지 않음: {nameof(chapter)} = {chapter}");
#endif
                return 0;
            }

            return data.scenario_id;
        }

        private void RefreshElement()
        {
            foreach (var item in dic)
            {
                RefreshElement(item.Key);
            }
        }

        private void RefreshElement(int adventureGroup)
        {
            int currentGuideQuestMazeId = GetCurrentGuideQuestMazeId();
            foreach (var item in dic[adventureGroup])
            {
                bool isOpen = IsOpend(item);
                bool isCurrentGuideQuest = item.Id == currentGuideQuestMazeId;
                item.Update(isOpen ? string.Empty : GetOpenConditionMessage(item), isCurrentGuideQuest);
            }
        }

        private bool IsOpend(MultiMazeWaitingRoomData data)
        {
            if (data.id == MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_FOREST_MAZE)
            {
                if (!dungeonModel.IsOpened(DungeonType.ForestMaze, isShowPopup: false))
                    return false;
            }
            else if (data.id == MultiMazeWaitingRoomData.TIME_PATROL)
            {
                if (!dungeonModel.IsOpenTimePatrol(isShowPopup: false))
                    return false;
            }

            return data.open_stage_id <= dungeonModel.FinalStageId;
        }

        private string GetOpenConditionMessage(MultiMazeWaitingRoomData data)
        {
            if (data.id == MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_FOREST_MAZE)
            {
                if (!dungeonModel.IsOpened(DungeonType.ForestMaze, isShowPopup: false))
                {
                    IOpenConditional openConditional = dungeonModel.GetFirstOpenConditional(DungeonType.ForestMaze);
                    return dungeonModel.GetOpenConditionalDetailText(openConditional);
                }
            }
            else if (data.id == MultiMazeWaitingRoomData.TIME_PATROL)
            {
                int checkStageId = BasisType.TP_OPEN_STAGE_ID.GetInt() - 1;
                StageData checkStageData = stageDataRepo.Get(checkStageId);
                return LocalizeKey._90131.ToText() // {NAME} 클리어시 오픈 됩니다.
                    .Replace(ReplaceKey.NAME, checkStageData.name_id.ToText());
            }

            StageData stage = stageDataRepo.Get(data.open_stage_id);
            return LocalizeKey._48801 // {NAME}에 도달하면 오픈됩니다.
                .ToText().Replace(ReplaceKey.NAME, stage.name_id.ToText());
        }

        /// <summary>
        /// 현재 가이드퀘스트에 해당하는 StageId
        /// </summary>
        private int GetCurrentGuideQuestMazeId()
        {
            QuestInfo guideQuest = questModel.GetMaintQuest();
            if (guideQuest == null || guideQuest.IsInvalidData)
                return 0;

            // 미로 타입이 아니거나 진행중이 아닐 경우에는 없는 것으로 간주
            if (!IsMazeShortcutType(guideQuest.ShortCutType))
                return 0;

            if (guideQuest.CompleteType != QuestInfo.QuestCompleteType.InProgress)
                return 0;

            //return guideQuest.ShortCutValue;
            return guideQuest.ConditionValue;
        }

        private bool IsMazeShortcutType(ShortCutType shortCutType)
        {
            switch (shortCutType)
            {
                case ShortCutType.Maze:
                case ShortCutType.MultiMaze:
                case ShortCutType.ForestMaze:
                    return true;
            }

            return false;
        }
    }
}