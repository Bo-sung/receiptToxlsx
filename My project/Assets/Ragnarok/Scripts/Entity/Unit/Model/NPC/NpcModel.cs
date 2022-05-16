using UnityEngine;

namespace Ragnarok
{
    public class NpcModel : UnitModel<NpcEntity>
    {
        private NpcType npcType;
        private Npc npc;

        public Npc GetNpc()
        {
            return npc;
        }

        public void Initialize(NpcType npcType)
        {
            bool isDirtyData = SetData(npcType);

            if (!isDirtyData)
                return;
        }

        private bool SetData(NpcType npcType)
        {
            if (npcType == default)
            {
                ResetData();
                return false;
            }

            // 바뀐 값이 없을 경우
            if (this.npcType == npcType)
                return false;

            ResetData(); // 현재 세팅 초기화

            npc = Npc.GetByKey(npcType);
            if (npc == null)
            {
                Debug.LogError($"[NPC 세팅 실패] 데이터가 없음: {nameof(npcType)} = {npcType}");
                return false;
            }

            return true;
        }

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        public override void ResetData()
        {
            base.ResetData();

            npcType = default;
            npc = null;
        }
    }
}