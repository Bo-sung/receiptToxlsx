using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class QuestDataManager : Singleton<QuestDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, QuestData> dataDic;
        private readonly Dictionary<ObscuredByte, List<QuestData>> dataListDic;
        private readonly Dictionary<ObscuredInt, QuestData> mainDataDic;
        private readonly Dictionary<ObscuredInt, QuestData> timePatrolDataDic;

        public ResourceType DataType => ResourceType.QuestDataDB;

        public QuestDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, QuestData>(ObscuredIntEqualityComparer.Default);
            dataListDic = new Dictionary<ObscuredByte, List<QuestData>>(ObscuredByteEqualityComparer.Default);
            mainDataDic = new Dictionary<ObscuredInt, QuestData>(ObscuredIntEqualityComparer.Default);
            timePatrolDataDic = new Dictionary<ObscuredInt, QuestData>(ObscuredIntEqualityComparer.Default);
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
            dataListDic.Clear();
            mainDataDic.Clear();
            timePatrolDataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            ObscuredByte mainCategory = (byte)QuestCategory.Main;
            ObscuredByte timePatrolCategory = (byte)QuestCategory.TimePatrol;

            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    QuestData data = new QuestData(mpo.AsList());

                    ObscuredInt id = data.id; // 퀘스트 id

                    // Add DataDic
                    if (!dataDic.ContainsKey(id))
                        dataDic.Add(id, data);

                    ObscuredByte category = data.quest_category; // 퀘스트 카테고리

                    // Add DataListDic
                    if (!dataListDic.ContainsKey(category))
                        dataListDic.Add(category, new List<QuestData>());

                    dataListDic[category].Add(data);

                    ObscuredInt questSeq = data.daily_group; // 퀘스트 Seq

                    // Add mainDataDic
                    if (category == mainCategory)
                        mainDataDic.Add(questSeq, data);

                    // Add timePatrolDataDic
                    if (category == timePatrolCategory)
                        timePatrolDataDic.Add(questSeq, data);
                }
            }
        }

        public QuestData Get(int id)
        {
            ObscuredInt key = id;

            if (dataDic.ContainsKey(key))
                return dataDic[key];

            Debug.Log($"퀘스트 데이터가 존재하지 않습니다: {nameof(id)}] {nameof(id)} = {id}");
            return null;
        }

        public QuestData GetMainQuest(int mainQuestSeq)
        {
            if (mainDataDic.ContainsKey(mainQuestSeq))
                return mainDataDic[mainQuestSeq];

            Debug.LogError($"퀘스트 데이터가 존재하지 않습니다: {nameof(mainQuestSeq)}] {nameof(mainQuestSeq)} = {mainQuestSeq}");
            return null;
        }

        public QuestData GetTimePatrolQuest(int timePatrolQuestSeq)
        {
            if (timePatrolDataDic.ContainsKey(timePatrolQuestSeq))
                return timePatrolDataDic[timePatrolQuestSeq];

            Debug.LogError($"퀘스트 데이터가 존재하지 않습니다: {nameof(timePatrolQuestSeq)}] {nameof(timePatrolQuestSeq)} = {timePatrolQuestSeq}");
            return null;
        }

        public List<QuestData> Get(QuestCategory category)
        {
            ObscuredByte key = (byte)category;

            if (dataListDic.ContainsKey(key))
                return dataListDic[key];

            Debug.LogError($"퀘스트 데이터가 존재하지 않습니다: {nameof(category)}] {nameof(category)} = {category}");
            return null;
        }

        public QuestData Get(QuestType type)
        {
            ObscuredShort key = (short)type;

            foreach (var item in dataDic.Values)
            {
                if (item.quest_type == key)
                    return item;
            }

            Debug.LogError($"퀘스트 데이터가 존재하지 않습니다: {nameof(type)}] {nameof(type)} = {type}");
            return null;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// 데이터 검증
        /// </summary>
        public void VerifyData()
        {
#if UNITY_EDITOR

            HashSet<(short, int)> mainQuestHash = new HashSet<(short, int)>();

            foreach (var item in dataDic.Values)
            {
                QuestType type = item.quest_type.ToEnum<QuestType>();

                if (!System.Enum.IsDefined(typeof(QuestType), type))
                {
                    throw new System.Exception($"22.퀘스트 테이블 오류(미사용타입) {nameof(item.id)}={item.id}, {nameof(item.quest_type)}={item.quest_type}");
                }

                if (item.quest_category.ToEnum<QuestCategory>() == QuestCategory.Main)
                {
                    if (mainQuestHash.Contains((item.quest_type, item.condition_value)))
                        throw new System.Exception($"22.퀘스트 테이블 오류 {nameof(item.id)}={item.id}, {nameof(item.quest_type)}={item.quest_type}, {nameof(item.condition_value)}={item.condition_value} = 메인퀘스트 키중복");

                    mainQuestHash.Add((item.quest_type, item.condition_value));
                }

                switch (type)
                {
                    case QuestType.MONSTER_KILL_TARGET:
                        if (MonsterDataManager.Instance.Get(item.condition_value) == null)
                        {
                            throw new System.Exception($"22.퀘스트 테이블 오류 {nameof(item.id)}={item.id}, {nameof(type)}={type}({(int)type}), 없는 몬스터 테이블 ID={item.condition_value}");
                        }
                        break;

                    case QuestType.ITEM_GAIN:
                    case QuestType.ITEM_USE:
                    case QuestType.MAKE_ITEM_COUNT:
                        if (ItemDataManager.Instance.Get(item.condition_value) == null)
                        {
                            throw new System.Exception($"22.퀘스트 테이블 오류 {nameof(item.id)}={item.id}, {nameof(type)}={type}({(int)type}), 없는 아이템 테이블 ID={item.condition_value}");
                        }
                        break;
                    case QuestType.CUPET_FOUND:
                        if (CupetDataManager.Instance.Get(item.condition_value) == null)
                        {
                            throw new System.Exception($"22.퀘스트 테이블 오류 {nameof(item.id)}={item.id}, {nameof(type)}={type}({(int)type}), 없는 큐펫 테이블 ID={item.condition_value}");
                        }
                        break;

                    case QuestType.WORLD_BOSS_TYPE_COUNT:
                        if (WorldBossDataManager.Instance.Get(item.condition_value) == null)
                        {
                            throw new System.Exception($"22.퀘스트 테이블 오류 {nameof(item.id)}={item.id}, {nameof(type)}={type}({(int)type}), 없는 월드보스 테이블 ID={item.condition_value}");
                        }
                        else
                        {
                            int monsterId = WorldBossDataManager.Instance.Get(item.condition_value).monster_id;
                            if (MonsterDataManager.Instance.Get(monsterId) == null)
                            {
                                throw new System.Exception($"22.퀘스트 테이블 오류 {nameof(item.id)}={item.id}, {nameof(type)}={type}({(int)type}), 없는 몬스터 테이블 ID={monsterId}");
                            }
                        }
                        break;
                    case QuestType.SCENARIO_MAZE_ID_CLEAR_COUNT:
                        if (ScenarioMazeDataManager.Instance.Get(item.condition_value) == null)
                        {
                            throw new System.Exception($"22.퀘스트 테이블 오류 {nameof(item.id)}={item.id}, {nameof(type)}={type}({(int)type}), 없는 시나리오 테이블 ID={item.condition_value}");
                        }
                        break;
                    case QuestType.FIELD_ID_MVP_CLEAR_COUNT:
                    case QuestType.FILED_ID_RARE_3_MVP_CLEAR_COUNT:
                    case QuestType.FIELD_ID_ASSEMBLE_COUNT:
                        if (StageDataManager.Instance.Get(item.condition_value) == null)
                        {
                            throw new System.Exception($"22.퀘스트 테이블 오류 {nameof(item.id)}={item.id}, {nameof(type)}={type}({(int)type}), 없는 스테이지 테이블 ID={item.condition_value}");
                        }
                        break;

                }
            }
#endif
        }
    }
}