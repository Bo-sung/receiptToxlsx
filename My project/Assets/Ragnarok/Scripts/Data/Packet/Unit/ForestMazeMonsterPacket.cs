namespace Ragnarok
{
    public class ForestMazeMonsterPacket : IPacket<Response>, IMonsterBotInput
    {
        public int index;
        private byte state;
        private float posX;
        private float posZ;
        public int forestMonsterDataId;
        private ISpawnMonster spawnInfo;
        private float moveSpeed;

        int IMonsterBotInput.Index => index;
        byte IMonsterBotInput.State => state;
        float IMonsterBotInput.PosX => posX;
        float IMonsterBotInput.PosY => 0f;
        float IMonsterBotInput.PosZ => posZ;
        bool IMonsterBotInput.HasTargetPos => false;
        float IMonsterBotInput.SavedTargetingTime => 0f;
        float IMonsterBotInput.TargetPosX => 0f;
        float IMonsterBotInput.TargetPosY => 0f;
        float IMonsterBotInput.TargetPosZ => 0f;
        bool IMonsterBotInput.HasMaxHp => false;
        int IMonsterBotInput.MaxHp => 0;
        bool IMonsterBotInput.HasCurHp => false;
        int IMonsterBotInput.CurHp => 0;
        CrowdControlType IMonsterBotInput.CrowdControl => default;
        MonsterType ISpawnMonster.Type => spawnInfo.Type;
        int ISpawnMonster.Id => spawnInfo.Id;
        int ISpawnMonster.Level => spawnInfo.Level;
        float ISpawnMonster.Scale => spawnInfo.Scale;
        float? IMonsterBotInput.MoveSpeed => moveSpeed;

        public virtual void Initialize(Response response)
        {
            index = response.GetByte("1");
            state = response.GetByte("2");
            posX = response.GetFloat("3");
            posZ = response.GetFloat("4");
            forestMonsterDataId = response.GetShort("5");
        }

        public void SetSpawnInfo(ISpawnMonster spawnInfo)
        {
            this.spawnInfo = spawnInfo;
        }

        public void SetMoveSpeed(float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
        }
    }
}