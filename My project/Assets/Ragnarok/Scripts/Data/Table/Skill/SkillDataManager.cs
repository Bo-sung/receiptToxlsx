using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class SkillDataManager : Singleton<SkillDataManager>, IDataManger, SkillDataManager.ISkillDataRepoImpl
    {
        public interface ISkillDataRepoImpl
        {
            SkillData Get(int id, int level);
        }

        /// <summary>
        /// Key: Id
        /// Value: Dictionary(Level, Data)
        /// </summary>
        private readonly Dictionary<ObscuredInt, Dictionary<ObscuredInt, SkillData>> dataDic;

        public ResourceType DataType => ResourceType.SkillDataDB;

        public SkillDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, Dictionary<ObscuredInt, SkillData>>(ObscuredIntEqualityComparer.Default);
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
                    SkillData data = new SkillData(mpo.AsList());

                    int id = data.id;
                    int level = data.lv;

                    if (!dataDic.ContainsKey(id))
                        dataDic.Add(id, new Dictionary<ObscuredInt, SkillData>(ObscuredIntEqualityComparer.Default));

                    if (dataDic[id].ContainsKey(level))
                    {
                        Debug.LogWarning($"중복되는 스킬 값이 존재합니다: id = {id}, level = {level}");
                        continue;
                    }

                    dataDic[id].Add(level, data);
                }
            }

            SkillData emptySkill = new SkillData();

            if (!dataDic.ContainsKey(emptySkill.id))
                dataDic.Add(emptySkill.id, new Dictionary<ObscuredInt, SkillData>(ObscuredIntEqualityComparer.Default));

            dataDic[emptySkill.id].Add(emptySkill.lv, emptySkill);
        }

        /// <summary>
        /// ID 와 Level에 해당하는 스킬 데이터 반환
        /// </summary>
        public SkillData Get(int id, int level)
        {
            if (!dataDic.ContainsKey(id))
                return null;

            if (!dataDic[id].ContainsKey(level))
                return null;

            return dataDic[id][level];
        }

        /// <summary>
        /// 현재 id에 해당하는 스킬의 최대 레벨 반환
        /// </summary>
        public int GetMaxLevel(int id)
        {
            int level = 1;

            if (dataDic.ContainsKey(id))
            {
                foreach (var item in dataDic[id].Values)
                {
                    if (item.lv > level)
                        level = item.lv;
                }
            }

            return level;
        }

        public SkillData[] GetArray()
        {
            int passiveType = (int)SkillType.Passive;
            List<SkillData> list = new List<SkillData>();
            foreach (var item in dataDic.Values)
            {
                list.AddRange(item.Values);
            }
            return list.FindAll(a => a.skill_type == passiveType && !a.icon_name.Equals("0")).ToArray();
        }

        public int[] GetCentralLabSkills(int entry_id)
        {
            List<int> list = new List<int>();

            // 다단히트 증가
            // 화속성공격
            // 공격시 상태이상
            // 사정거리 증가
            // 고급카드스킬 확률증가

            return list.ToArray();
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