using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class MonsterDataManager : Singleton<MonsterDataManager>, IDataManger, MonsterDataManager.IImpl
    {
        public interface IImpl
        {
            MonsterData Get(int id);
        }

        private readonly Dictionary<ObscuredInt, MonsterData> dataDic;

        public IEnumerable<MonsterData> EntireMonsters => dataDic.Values;

        public ResourceType DataType => ResourceType.MonsterDataDB;

        public MonsterDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, MonsterData>(ObscuredIntEqualityComparer.Default);
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
                    MonsterData data = new MonsterData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public MonsterData Get(int id)
        {
            if (id == 0)
                return null;

            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"몬스터 데이터가 존재하지 않습니다: id = {id}");
                return null;
            }

            return dataDic[id];
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
                if(item.basic_active_skill_id != 0)
                {
                    var skill = SkillDataManager.Instance.Get(item.basic_active_skill_id, 1);
                    if(skill == null)
                    {
                        throw new System.Exception($"5.몬스터 테이블 ID={item.id} {item.name_id.ToText()}, 8.스킬테이블에 평타 스킬 없음 {nameof(item.basic_active_skill_id)}={item.basic_active_skill_id}");
                    }
                }

                if(item.skill_id_1 != 0)
                {
                    var skill = SkillDataManager.Instance.Get(item.skill_id_1, 1);
                    if (skill == null)
                    {
                        throw new System.Exception($"5.몬스터 테이블 ID={item.id} {item.name_id.ToText()}, 8.스킬테이블에 스킬 없음 {nameof(item.skill_id_1)}={item.skill_id_1}");
                    }
                }

                if (item.skill_id_2 != 0)
                {
                    var skill = SkillDataManager.Instance.Get(item.skill_id_2, 1);
                    if (skill == null)
                    {
                        throw new System.Exception($"5.몬스터 테이블 ID={item.id} {item.name_id.ToText()}, 8.스킬테이블에 스킬 없음 {nameof(item.skill_id_2)}={item.skill_id_2}");
                    }
                }

                if (item.skill_id_3 != 0)
                {
                    var skill = SkillDataManager.Instance.Get(item.skill_id_3, 1);
                    if (skill == null)
                    {
                        throw new System.Exception($"5.몬스터 테이블 ID={item.id} {item.name_id.ToText()}, 8.스킬테이블에 스킬 없음 {nameof(item.skill_id_3)}={item.skill_id_3}");
                    }
                }

                if (item.skill_id_4 != 0)
                {
                    var skill = SkillDataManager.Instance.Get(item.skill_id_4, 1);
                    if (skill == null)
                    {
                        throw new System.Exception($"5.몬스터 테이블 ID={item.id} {item.name_id.ToText()}, 8.스킬테이블에 스킬 없음 {nameof(item.skill_id_4)}={item.skill_id_4}");
                    }
                }
            }
#endif
        }
    }
}