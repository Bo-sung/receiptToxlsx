using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ScenarioMazeDataManager : Singleton<ScenarioMazeDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, ScenarioMazeData> dataDic;
        private readonly Dictionary<ContentType, ScenarioMazeData> scenarioDependentContents;

        public ResourceType DataType => ResourceType.ScenarioMazeDataDB;

        public ScenarioMazeDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, ScenarioMazeData>(ObscuredIntEqualityComparer.Default);
            scenarioDependentContents = new Dictionary<ContentType, ScenarioMazeData>();
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
            scenarioDependentContents.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ScenarioMazeData data = new ScenarioMazeData(mpo.AsList());
                    dataDic.Add(data.id, data);
                    if (data.OpenContent.HasValue)
                        scenarioDependentContents.Add(data.OpenContent.Value, data);
                }
            }
        }

        public ScenarioMazeData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
#if UNITY_EDITOR
                Debug.LogError($"시나리오 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
#endif
                return null;
            }

            return dataDic[id];
        }

        public ScenarioMazeData GetByContents(ContentType contentsType)
        {
            ScenarioMazeData data = null;
            scenarioDependentContents.TryGetValue(contentsType, out data);
            return data;
        }

        /// <summary>
        /// 시나리오모드에 해당하는 첫번째 데이터 반환
        /// </summary>
        public ScenarioMazeData Get(ScenarioMazeMode mode)
        {
            foreach (var item in dataDic.Values)
            {
                if (mode == item.scenario_maze_type.ToEnum<ScenarioMazeMode>())
                    return item;
            }

            return null;
        }

        public ScenarioMazeData GetEventContentUnlock(ContentType type)
        {
            foreach (var item in dataDic.Values)
            {
                if (item.OpenContent.HasValue)
                {
                    if (item.OpenContent.Value == type)
                        return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            MultiMazeDataManager multiMazeDataManager = MultiMazeDataManager.Instance;

            foreach (var item in dataDic.Values)
            {
                MultiMazeData data = multiMazeDataManager.GetByOpenScenarioId(item.id);
                item.SetChapter(data.chapter);
            }
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            foreach (var item in dataDic.Values)
            {
                if (MonsterDataManager.Instance.Get(item.boss_monster_id) == null)
                {
                    throw new System.Exception($"39.시나리오 테이블 오류 ID={item.id}, 없는 몬스터={item.boss_monster_id}");
                }
                if (MonsterDataManager.Instance.Get(item.normal_monster_id) == null)
                {
                    throw new System.Exception($"39.시나리오 테이블 오류 ID={item.id}, 없는 몬스터={item.normal_monster_id}");
                }
            }
#endif
        }
    }
}