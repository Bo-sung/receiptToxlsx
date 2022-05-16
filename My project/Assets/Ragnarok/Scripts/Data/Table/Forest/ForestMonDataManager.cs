using MsgPack;
using UnityEngine;

namespace Ragnarok
{
    public class ForestMonDataManager : Singleton<ForestMonDataManager>, IDataManger
    {
        private readonly BetterList<ForestMonData> dataList;

        public ResourceType DataType => ResourceType.ForestMonDataDB;

        public ForestMonDataManager()
        {
            dataList = new BetterList<ForestMonData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ForestMonData data = new ForestMonData(mpo.AsList());
                    dataList.Add(data);
                }
            }
        }

        public ForestMonData Get(int id)
        {
            for (int i = 0; i < dataList.size; i++)
            {
                if (dataList[i].id == id)
                    return dataList[i];
            }

            Debug.LogError($"78.미궁숲 몬스터 그룹 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
            return null;
        }

        public int GetBossMonsterLevel(int monsterGroupId)
        {
            for (int i = 0; i < dataList.size; i++)
            {
                if (dataList[i].IsBoss() && dataList[i].group_id == monsterGroupId)
                    return dataList[i].monster_level;
            }

            return 0;
        }

        public ISpawnMonster GetBossMonster(int monsterGroupId)
        {
            for (int i = 0; i < dataList.size; i++)
            {
                if (dataList[i].IsBoss() && dataList[i].group_id == monsterGroupId)
                    return dataList[i];
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

#endif
        }
    }
}