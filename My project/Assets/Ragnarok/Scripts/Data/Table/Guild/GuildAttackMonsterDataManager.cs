using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class GuildAttackMonsterDataManager : Singleton<GuildAttackMonsterDataManager>, IDataManger
    {
        private readonly BetterList<GuildAttackMonsterData> dataList;

        public ResourceType DataType => ResourceType.GuildAttackMonsterDataDB;

        public GuildAttackMonsterDataManager()
        {
            dataList = new BetterList<GuildAttackMonsterData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Release();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    GuildAttackMonsterData data = new GuildAttackMonsterData(mpo.AsList());
                    dataList.Add(data);
                }
            }
        }

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