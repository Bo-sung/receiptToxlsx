using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class StageDataManager : Singleton<StageDataManager>, IDataManger, StageDataManager.IStageDataRepoImpl
    {
        public interface IStageDataRepoImpl
        {
            StageData Get(int id);
        }

        private readonly Dictionary<ObscuredInt, StageData> dataDic;

        public ResourceType DataType => ResourceType.StageDataDB;

        public StageDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, StageData>(ObscuredIntEqualityComparer.Default);
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
                    StageData data = new StageData(mpo.AsList());
                    dataDic.Add(data.id, data);
                }
            }
        }

        public StageData Get(int id)
        {
            if (!IsExists(id))
            {
                Debug.LogError($"스테이지 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
                return null;
            }

            return dataDic[id];
        }

        public bool IsExists(int id)
        {
            return dataDic.ContainsKey(id);
        }

        /// <summary>
        /// 특정 아이템을 드랍하는 스테이지 목록 반환
        /// </summary>
        public StageData[] GetStagesCanDropItem(int item_id)
        {
            List<StageData> stageList = new List<StageData>();

            foreach (var stage in dataDic.Values)
            {
                if (IsStageDropItem(stage, item_id))
                {
                    stageList.Add(stage);
                }
            }

            return stageList.ToArray();
        }

        /// <summary>
        /// chapter 에 해당하는 처음 Stage를 반환
        /// </summary>
        public StageData FindWithChapter(StageChallengeType type, int chapter)
        {
            int typeValue = (int)type;
            foreach (var item in dataDic.Values)
            {
                if (item.challenge_type == typeValue && item.chapter == chapter)
                    return item;
            }

            return null;
        }

        private bool IsStageDropItem(StageData stageData, int item_id)
        {
            if (stageData == null)
                return false;

            if (stageData.boss_drop_1 == item_id && stageData.boss_drop_rate_1 > 0 ||
                    stageData.boss_drop_2 == item_id && stageData.boss_drop_rate_2 > 0 ||
                    stageData.boss_drop_3 == item_id && stageData.boss_drop_rate_3 > 0 ||
                    stageData.boss_drop_4 == item_id && stageData.boss_drop_rate_4 > 0 ||
                    stageData.normal_drop_1 == item_id && stageData.normal_drop_rate_1 > 0 ||
                    stageData.normal_drop_2 == item_id && stageData.normal_drop_rate_2 > 0 ||
                    stageData.normal_drop_3 == item_id && stageData.normal_drop_rate_3 > 0 ||
                    stageData.normal_drop_4 == item_id && stageData.normal_drop_rate_4 > 0)
            {
                return true;
            }

            return false;
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
            foreach (var item in dataDic.Values)
            {
                int monsterId = item.boss_monster_id;
                MonsterData monsterData = MonsterDataManager.Instance.Get(monsterId);
                if (monsterData == null)
                    throw new System.Exception($"004.스테이지테이블 테이블 오류 : boss_monster_id : {monsterId}가 005.몬스터테이블에 없음");

                if (item.boss_drop_1 > 0 && item.boss_drop_rate_1 == 0)
                {
                    Debug.LogError($"004.스테이지테이블 테이블 오류: id : {item.id} {nameof(item.boss_drop_1)}={item.boss_drop_1}, {nameof(item.boss_drop_rate_1)}={item.boss_drop_rate_1} 드랍 확률 0%");
                }
                if (item.boss_drop_2 > 0 && item.boss_drop_rate_2 == 0)
                {
                    Debug.LogError($"004.스테이지테이블 테이블 오류: id : {item.id} {nameof(item.boss_drop_2)}={item.boss_drop_2}, {nameof(item.boss_drop_rate_2)}={item.boss_drop_rate_2} 드랍 확률 0%");
                }
                if (item.boss_drop_3 > 0 && item.boss_drop_rate_3 == 0)
                {
                    Debug.LogError($"004.스테이지테이블 테이블 오류: id : {item.id} {nameof(item.boss_drop_3)}={item.boss_drop_3}, {nameof(item.boss_drop_rate_3)}={item.boss_drop_rate_3} 드랍 확률 0%");
                }
                if (item.boss_drop_4 > 0 && item.boss_drop_rate_4 == 0)
                {
                    Debug.LogError($"004.스테이지테이블 테이블 오류: id : {item.id} {nameof(item.boss_drop_4)}={item.boss_drop_4}, {nameof(item.boss_drop_rate_4)}={item.boss_drop_rate_4} 드랍 확률 0%");
                }
                if (item.normal_drop_1 > 0 && item.normal_drop_rate_1 == 0)
                {
                    Debug.LogError($"004.스테이지테이블 테이블 오류: id : {item.id} {nameof(item.normal_drop_1)}={item.normal_drop_1}, {nameof(item.normal_drop_rate_1)}={item.normal_drop_rate_1} 드랍 확률 0%");
                }
                if (item.normal_drop_2 > 0 && item.normal_drop_rate_2 == 0)
                {
                    Debug.LogError($"004.스테이지테이블 테이블 오류: id : {item.id} {nameof(item.normal_drop_2)}={item.normal_drop_2}, {nameof(item.normal_drop_rate_2)}={item.normal_drop_rate_2} 드랍 확률 0%");
                }
                if (item.normal_drop_3 > 0 && item.normal_drop_rate_3 == 0)
                {
                    Debug.LogError($"004.스테이지테이블 테이블 오류: id : {item.id} {nameof(item.normal_drop_3)}={item.normal_drop_3}, {nameof(item.normal_drop_rate_3)}={item.normal_drop_rate_3} 드랍 확률 0%");
                }
                if (item.normal_drop_4 > 0 && item.normal_drop_rate_4 == 0)
                {
                    Debug.LogError($"004.스테이지테이블 테이블 오류: id : {item.id} {nameof(item.normal_drop_4)}={item.normal_drop_4}, {nameof(item.normal_drop_rate_4)}={item.normal_drop_rate_4} 드랍 확률 0%");
                }
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 일반 스테이지 데이터 전체 반환
        /// </summary>
        [System.Obsolete]
        public IEnumerable<StageData> GetNormalStageData()
        {
            int typeValue = (int)StageChallengeType.Normal;
            foreach (var item in dataDic.Values)
            {
                if (item.challenge_type != typeValue)
                    continue;

                yield return item;
            }
        }
#endif
    }
}