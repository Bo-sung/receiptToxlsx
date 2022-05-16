using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="MultiMazeWaitingRoomDataManager"/>
    /// </summary>
    public sealed class MultiMazeWaitingRoomData : IData, UIAdventureMazeElement.IInput
    {
        public const int MULTI_MAZE_LOBBY_CHRISTMAS_EVENT = 13; // 크리스마스 이벤트 등
        public const int MULTI_MAZE_LOBBY_FOREST_MAZE = 14; // 미궁숲
        public const int MULTI_MAZE_LOBBY_DARK_MAZE_EVENT = 15; // 이벤트미궁:암흑

        public const int TIME_PATROL = 101; // 타임패트롤
        public const int GATE_1 = 16; // 게이트 1
        public const int GATE_2 = 17; // 게이트 2

        public readonly int id;
        public readonly int name_id;
        public readonly string scene_name;
        public readonly string bgm;
        public readonly int multi_maze1_chapter;
        public readonly int multi_maze2_chapter;
        public readonly int multi_maze3_chapter;
        public readonly int multi_maze4_chapter;
        public readonly int multi_maze5_chapter;
        public readonly int multi_maze6_chapter;
        public readonly int multi_maze7_chapter;
        public readonly int multi_maze8_chapter;
        public readonly int multi_maze9_chapter;
        public readonly int multi_maze10_chapter;
        public readonly int multi_maze11_chapter;
        public readonly int multi_maze12_chapter;
        public readonly ObscuredInt open_stage_id;
        public readonly int battle_score;
        public readonly string texture_name;
        private readonly RewardData rewardData;
        public readonly int multi_event_chapter;
        public readonly int sort_index; // 0보다 작으면 노출되지 않는다.

        public int Id => id;
        public int LocalKey => name_id;
        public string IconName => texture_name;
        public int RecommandedBattleScore => battle_score;
        public RewardData Reward => rewardData;
        public bool IsLock { get; private set; }
        public string OpenConditionMessage { get; private set; } // 클라 전용
        public bool IsCurrentGuideQuest { get; private set; }

        private int multiMazeDataId; // 클라 전용

        public MultiMazeWaitingRoomData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            name_id = data[index++].AsInt32();
            scene_name = data[index++].AsString();
            bgm = data[index++].AsString();
            multi_maze1_chapter = data[index++].AsInt32();
            multi_maze2_chapter = data[index++].AsInt32();
            multi_maze3_chapter = data[index++].AsInt32();
            multi_maze4_chapter = data[index++].AsInt32();
            multi_maze5_chapter = data[index++].AsInt32();
            multi_maze6_chapter = data[index++].AsInt32();
            multi_maze7_chapter = data[index++].AsInt32();
            multi_maze8_chapter = data[index++].AsInt32();
            multi_maze9_chapter = data[index++].AsInt32();
            multi_maze10_chapter = data[index++].AsInt32();
            multi_maze11_chapter = data[index++].AsInt32();
            multi_maze12_chapter = data[index++].AsInt32();
            open_stage_id = data[index++].AsInt32();
            battle_score = data[index++].AsInt32();
            texture_name = data[index++].AsString();
            int drop_item = data[index++].AsInt32();
            rewardData = drop_item > 0 ? new RewardData(RewardType.Item, drop_item, 1) : null;
            multi_event_chapter = data[index++].AsInt32();
            sort_index = data[index++].AsInt32();
        }

        public void SetMultiMazeDataId(int multiMazeDataId)
        {
            this.multiMazeDataId = multiMazeDataId;
        }

        public int GetMultiMazeDataId()
        {
            // 따로 세팅되어있지 않다면 id 값이 waitingRoomId 와 동일
            if (multiMazeDataId == 0)
                return id;

            return multiMazeDataId;
        }

        public int GetGroup()
        {
            if (id == TIME_PATROL || IsGate())
                return 2;

            return 1;
        }

        public bool IsGate()
        {
            return id == GATE_1 || id == GATE_2;
        }

        public void Update(string message, bool isCurrentGuideQuest)
        {
            OpenConditionMessage = message;
            IsLock = !string.IsNullOrEmpty(message);
            IsCurrentGuideQuest = isCurrentGuideQuest;
        }

        public int GetMultiMazeId(int zoneIndex)
        {
            switch (zoneIndex)
            {
                case 1: return multi_maze1_chapter;
                case 2: return multi_maze2_chapter;
                case 3: return multi_maze3_chapter;
                case 4: return multi_maze4_chapter;
                case 5: return multi_maze5_chapter;
                case 6: return multi_maze6_chapter;
                case 7: return multi_maze7_chapter;
                case 8: return multi_maze8_chapter;
                case 9: return multi_maze9_chapter;
                case 10: return multi_maze10_chapter;
                case 11: return multi_maze11_chapter;
                case 12: return multi_maze12_chapter;

                case MultiMazePortalZone.CHRISTMAS_EVENT_INDEX: return multi_event_chapter;
                case MultiMazePortalZone.FOREST_MAZE_INDEX: return MULTI_MAZE_LOBBY_FOREST_MAZE;
                case MultiMazePortalZone.DARK_MAZE_EVENT_INDEX: return MULTI_MAZE_LOBBY_DARK_MAZE_EVENT;
                case MultiMazePortalZone.GATE_MAZE_1_INDEX: return GATE_1;
                case MultiMazePortalZone.GATE_MAZE_2_INDEX: return GATE_2;

                default: throw new System.ArgumentException($"잘못된 인덱스: index = {zoneIndex}");
            }
        }

        /// <summary>
        /// 게이트 데이터 Id
        /// </summary>
        public int GetGateId(int zoneIndex)
        {
            switch (zoneIndex)
            {
                case MultiMazePortalZone.GATE_MAZE_1_INDEX: return 1;
                case MultiMazePortalZone.GATE_MAZE_2_INDEX: return 2;

                default: throw new System.ArgumentException($"잘못된 인덱스: index = {zoneIndex}");
            }
        }
    }
}