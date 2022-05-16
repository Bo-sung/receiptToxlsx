using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class SkillEffectDataManager : Singleton<SkillEffectDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, SkillEffectData> dataDic;

        public ResourceType DataType => ResourceType.SkillEffectDataDB;

        public SkillEffectDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, SkillEffectData>(ObscuredIntEqualityComparer.Default);
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