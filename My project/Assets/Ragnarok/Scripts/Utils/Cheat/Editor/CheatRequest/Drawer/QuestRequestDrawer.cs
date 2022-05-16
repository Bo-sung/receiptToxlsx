using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public sealed class QuestRequestDrawer : CheatRequestDrawer
    {
        public override int OrderNum => 3;

        public override string Title => "퀘스트";

        // <!-- Models --!>
        private DungeonModel dungeonModel;
        private QuestModel questModel;
        private CharacterModel characterModel;

        // <!-- Repositories --!>
        private readonly EnumDrawer stageEnumDrawer = new EnumDrawer(isShowId: false);
        private readonly EnumDrawer mazeEnumDrawer = new EnumDrawer(isShowId: false);
        private readonly EnumDrawer duelEnumDrawer = new EnumDrawer(isShowId: false);
        private readonly EnumDrawer mainQuestEnumDrawer = new EnumDrawer(isShowId: true);
        private readonly EnumDrawer mainQuestAutoClearEnumDrawer = new EnumDrawer(isShowId: true);
        private readonly int duelMaxIndex = 8;

        // <!-- Editor Fields --!>
        private int duelIndex;

        protected override void Awake()
        {
            dungeonModel = Entity.player.Dungeon;
            questModel = Entity.player.Quest;
            characterModel = Entity.player.Character;
            StageDataManager stageDataRepo = StageDataManager.Instance;
            ScenarioMazeDataManager scenarioMazeDataRepo = ScenarioMazeDataManager.Instance;
            QuestDataManager questDataRepo = QuestDataManager.Instance;

            stageEnumDrawer.Clear();
            mazeEnumDrawer.Clear();
            duelEnumDrawer.Clear();
            mainQuestEnumDrawer.Clear();
            mainQuestAutoClearEnumDrawer.Clear();

            string name;
            string displayName;
            int chapter = 0;
            int index = 0;
            foreach (var item in stageDataRepo.GetNormalStageData())
            {
                if (chapter != item.chapter)
                {
                    chapter = item.chapter;
                    index = 0;
                }

                ++index;

                // Add Stage
                name = item.name_id.ToText(LanguageType.KOREAN);
                displayName = StringBuilderPool.Get()
                    .Append(chapter).Append("/[").Append(index).Append("] ").Append(name)
                    .Release();
                stageEnumDrawer.Add(item.id, name, displayName);

                // Add Maze
                ScenarioMazeData mazeData = scenarioMazeDataRepo.Get(chapter);
                if (mazeData != null)
                {
                    name = StringBuilderPool.Get()
                        .Append("[").Append(mazeData.id).Append("] ").Append(mazeData.name_id.ToText(LanguageType.KOREAN))
                        .Release();
                    mazeEnumDrawer.Add(mazeData.id, name);
                }

                // Add Duel
                int stageLangId = BasisType.STAGE_TBLAE_LANGUAGE_ID.GetInt(chapter);
                name = StringBuilderPool.Get()
                    .Append("[").Append(chapter).Append("] ").Append(stageLangId.ToText(LanguageType.KOREAN))
                    .Release();
                duelEnumDrawer.Add(item.chapter, name);
            }

            var mainQuestList = questDataRepo.Get(QuestCategory.Main);
            if (mainQuestList != null)
            {
                const int GROUP_COUNT = 20; // 20개당 하나로 분리
                int seq;
                int minGroup = 1;
                int maxGroup = GROUP_COUNT;
                int seqSkipValue = Constants.Quest.MAIN_QUEST_JUMP_DESTINATION_GROUP_ID - Constants.Quest.MAIN_QUEST_JUMP_START_GROUP_ID - 1;
                foreach (var item in mainQuestList)
                {
                    seq = item.daily_group;

                    // 잘못된 퀘스트 그룹
                    if (seq > Constants.Quest.MAIN_QUEST_JUMP_START_GROUP_ID && seq < Constants.Quest.MAIN_QUEST_JUMP_DESTINATION_GROUP_ID)
                        continue;

                    name = item.name_id.ToText(LanguageType.KOREAN);

                    if (seq > Constants.Quest.MAIN_QUEST_JUMP_START_GROUP_ID)
                        seq -= seqSkipValue;

                    while (seq > maxGroup)
                    {
                        minGroup += GROUP_COUNT;
                        maxGroup += GROUP_COUNT;
                    }

                    displayName = StringBuilderPool.Get()
                        .Append(minGroup).Append('~').Append(maxGroup).Append("/[").Append(seq).Append("] ").Append(name)
                        .Release();
                    mainQuestEnumDrawer.Add(seq, name, displayName);
                    mainQuestAutoClearEnumDrawer.Add(seq, name, displayName);
                }
            }
        }

        protected override void OnDestroy()
        {
            dungeonModel = null;
            questModel = null;
            characterModel = null;
            stageEnumDrawer.Clear();
            mazeEnumDrawer.Clear();
            duelEnumDrawer.Clear();
            mainQuestEnumDrawer.Clear();
            mainQuestAutoClearEnumDrawer.Clear();
        }

        protected override void OnDraw()
        {
            if (DrawMiniHeader("가이드 퀘스트"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("모든 가이드 퀘스트 클리어");
                    DrawRequest(SendGuideQuestAllClear);
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("특정 가이드 퀘스트 클리어");
                    mainQuestEnumDrawer.DrawEnum();
                    DrawRequest(RequestMainQuestClear);
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("완료 까지 자동 보상받기");
                    mainQuestAutoClearEnumDrawer.DrawEnum();
                    DrawRequest(AutoRequestGuideReward);
                }
            }

            if (DrawMiniHeader("스테이지 클리어"))
            {
                using (ContentDrawer.Default)
                {
                    stageEnumDrawer.DrawEnum();
                    DrawRequest(RequestStageClear);
                }
            }

            if (DrawMiniHeader("미로 클리어"))
            {
                using (ContentDrawer.Default)
                {
                    mazeEnumDrawer.DrawEnum();
                    DrawRequest(RequestMazeClear);
                }
            }

            if (DrawMiniHeader("듀얼 조각 획득"))
            {
                using (ContentDrawer.Default)
                {
                    duelEnumDrawer.DrawEnum();
                    duelIndex = MathUtils.Clamp(EditorGUILayout.IntField(nameof(duelIndex), duelIndex), 1, duelMaxIndex);
                    DrawRequest(RequestDuel);
                }
            }
        }

        private void RequestMainQuestClear()
        {
            int seq = mainQuestEnumDrawer.id;
            if (seq == 0)
            {
                AddWarningMessage("클리어 할 퀘스트 seq 필요");
                return;
            }

            int seqSkipValue = Constants.Quest.MAIN_QUEST_JUMP_DESTINATION_GROUP_ID - Constants.Quest.MAIN_QUEST_JUMP_START_GROUP_ID - 1;
            if (seq > Constants.Quest.MAIN_QUEST_JUMP_START_GROUP_ID)
                seq += seqSkipValue;
            //마지막 메인퀘스트를 클리어하려고 할시 
            if(seq >=QuestDataManager.Instance.Get(QuestCategory.Main).Count - 1)
            {
                AddWarningMessage("마지막 메인 퀘스트 클리어 시도. 클리어시 데이터 날아감");
                return;
            }
            SendGuideQuestClear(seq + 1);
        }

        private void AutoRequestGuideReward()
        {
            int seq = mainQuestAutoClearEnumDrawer.id;
            if (seq == 0)
            {
                AddWarningMessage("완료 지점 퀘스트 seq 필요");
                return;
            }

            AysncAutoRequestGuideReward().WrapNetworkErrors();
        }

        private void RequestStageClear()
        {
            int stageId = stageEnumDrawer.id;
            if (stageId == 0)
            {
                AddWarningMessage("클리어 할 스테이지 선택 필요");
                return;
            }

            int lastEnterStageId = dungeonModel.LastEnterStageId;
            if (stageId <= lastEnterStageId)
            {
                AddWarningMessage($"이미 해당 스테이지에 도달: {nameof(lastEnterStageId)} = {lastEnterStageId}");
                return;
            }

            SendStage(stageId);
        }

        private void RequestMazeClear()
        {
            int mazeId = mazeEnumDrawer.id;
            if (mazeId == 0)
            {
                AddWarningMessage("클리어 할 시나리오미로 선택 필요");
                return;
            }

            int lastClearedScenarioID = dungeonModel.LastClearedScenarioID;
            if (mazeId <= lastClearedScenarioID)
            {
                AddWarningMessage($"이미 해당 미로에 도달: {nameof(lastClearedScenarioID)} = {lastClearedScenarioID}");
                return;
            }

            SendMaze(mazeId);
        }

        private void RequestDuel()
        {
            int chapter = duelEnumDrawer.id;
            if (chapter == 0)
            {
                AddWarningMessage("획득할 듀얼의 챕터 필요");
                return;
            }

            SendDuel(chapter, duelIndex);
        }

        private async Task AysncAutoRequestGuideReward()
        {
            int seq = mainQuestAutoClearEnumDrawer.id;
            while (questModel != null && questModel.IsMainQuestReward())
            {
                QuestInfo info = questModel.GetMaintQuest();

                // 1차 전직 완료 퀘스트일 경우
                if (info.QuestType == QuestType.JOB_DEGREE && info.ConditionValue == 1)
                {
                    // 1차 전직 완료 조건 불충족
                    if (characterModel.JobGrade() < 1)
                    {
                        AddWarningMessage("반드시 전직 필요 (후에 듀얼 조각 아이템 보상에서 에러남)");
                        break;
                    }
                }
                
                if (info.GetMainQuestGroup() > seq)
                    break;

                Response response = await questModel.RequestQuestRewardAsync(info);

                // 결과 음슴 or 성공 아님
                if (response == null)
                {
                    AddWarningMessage("가방 무게 확인 필요");
                    break;
                }

                if (!response.isSuccess)
                {
                    AddWarningMessage(response.resultCode.GetDescription());
                    break;
                }
            }
        }
    }
}