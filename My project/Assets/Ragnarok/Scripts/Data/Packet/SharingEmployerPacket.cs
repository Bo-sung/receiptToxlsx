using UnityEngine;

namespace Ragnarok
{
    public sealed class SharingEmployerPacket : IPacket<Response>, UICharacterShareWaiting.ISharingEmployerData
    {
        public int job_level;
        public int job_id;
        public string name;
        public int cid;
        public int battle_score;
        public int stage_id;

        // 클라이어트 전용
        private int stageNameId;

        void IInitializable<Response>.Initialize(Response response)
        {
            job_level = response.GetShort("1");
            job_id = response.GetByte("2");
            name = response.GetUtfString("3");
            cid = response.GetInt("4");
            battle_score = response.GetInt("5");
            stage_id = response.GetShort("6");
        }

        public void Initialize(StageDataManager.IStageDataRepoImpl stageDataRepoImpl)
        {
            if (stage_id == 0)
                return;

            StageData stageData = stageDataRepoImpl.Get(stage_id);
            if (stageData == null)
            {
                stageNameId = 0;
#if UNITY_EDITOR
                Debug.LogError($"skillData is Null: {nameof(stage_id)} = {stage_id}");
#endif
            }
            else
            {
                stageNameId = stageData.name_id;
            }
        }

        public string GetStageName()
        {
            return stageNameId.ToText();
        }

        public Job GetJob()
        {
            return job_id.ToEnum<Job>();
        }

        public int GetJobLevel()
        {
            return job_level;
        }

        public string GetName()
        {
            return name;
        }

        public int GetPower()
        {
            return battle_score;
        }
    }
}