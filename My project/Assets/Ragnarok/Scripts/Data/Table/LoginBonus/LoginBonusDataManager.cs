using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public sealed class LoginBonusDataManager : Singleton<LoginBonusDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, LoginBonusData> dataDic;

        public ResourceType DataType => ResourceType.LoginBonusDataDB;

        public LoginBonusDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, LoginBonusData>(ObscuredIntEqualityComparer.Default);
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
                    LoginBonusData data = new LoginBonusData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public LoginBonusData Get(int id)
        {
            if (id == 0)
                return null;

            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"로그인보너스 데이터가 존재하지 않습니다: id = {id}");
                return null;
            }
            return dataDic[id];
        }

        public LoginBonusData[] GetByGrupId(int groupId)
        {
            return dataDic.Values.Where(x => x.group_id == groupId).ToArray();
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
                if (item.gift_type == 6) // 아이템
                {
                    if (ItemDataManager.Instance.Get(item.gift) == null)
                    {
                        throw new System.Exception($"21.로그인보너스 테이블블 오류 ID={item.id}, 없는 아이템={item.gift}");
                    }
                }
            }
#endif
        }
    }
}