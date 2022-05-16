using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class MultiMazeDataManager : Singleton<MultiMazeDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, MultiMazeData> dataDic;

        public ResourceType DataType => ResourceType.MultiMazeDataDB;

        public MultiMazeDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, MultiMazeData>(ObscuredIntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    MultiMazeData data = new MultiMazeData(mpo.AsList());
                    dataDic.Add(data.id, data);
                }
            }
        }

        public MultiMazeData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"멀티미로 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
                return null;
            }

            return dataDic[id];
        }

        public MultiMazeData GetByChapter(int chapter)
        {
            foreach (var item in dataDic.Values)
            {
                if (item.chapter == chapter)
                    return item;
            }

            return null;
        }

        public MultiMazeData GetByOpenScenarioId(int openScenarioId)
        {
            foreach (var item in dataDic.Values)
            {
                if (item.open_scenario_id == openScenarioId)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            foreach (var item in dataDic.Values)
            {
                if (item.GetMazeMode() != MazeMode.EventMatch && MonsterDataManager.Instance.Get(item.boss_monster_id) == null)
                {
                    throw new System.Exception($"38.멀티미로 테이블 오류 ID={item.id}, 없는 몬스터={item.boss_monster_id}");
                }

                if (MonsterDataManager.Instance.Get(item.normal_monster_id) == null)
                {
                    throw new System.Exception($"38.멀티미로 테이블 오류 ID={item.id}, 없는 몬스터={item.normal_monster_id}");
                }

                if (item.reward_type1 == 6)
                {
                    if (ItemDataManager.Instance.Get(item.reward_value1) == null)
                        throw new System.Exception($"38.멀티미로 테이블 오류 ID={item.id}, 없는 아이템={item.reward_value1}");
                }

                if (item.reward_type2 == 6)
                {
                    if (ItemDataManager.Instance.Get(item.reward_value2) == null)
                        throw new System.Exception($"38.멀티미로 테이블 오류 ID={item.id}, 없는 아이템={item.reward_value2}");
                }

                if (item.reward_type3 == 6)
                {
                    if (ItemDataManager.Instance.Get(item.reward_value3) == null)
                        throw new System.Exception($"38.멀티미로 테이블 오류 ID={item.id}, 없는 아이템={item.reward_value3}");
                }

                if (item.reward_type4 == 6)
                {
                    if (ItemDataManager.Instance.Get(item.reward_value4) == null)
                        throw new System.Exception($"38.멀티미로 테이블 오류 ID={item.id}, 없는 아이템={item.reward_value4}");
                }

                if ((item.size_x * item.size_y) != item.multi_maze_data.Length)
                {
                    throw new System.Exception($"38.멀티미로 테이블 오류 ID={item.id}, {nameof(item.size_x)} = {item.size_x}, {nameof(item.size_y)} = {item.size_y}, {nameof(item.multi_maze_data)} = {item.multi_maze_data.Length}");
                }
            }
#endif
        }
    }
}