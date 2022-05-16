namespace Ragnarok
{
    public class MazeMonsterPacket : IPacket<Response>, IMonsterBotInput
    {
        public const int BOSS_MONSTER_INDEX = 0;

        public MonsterType monsterType;
        /// <summary>
        /// 몬스터 고유 인덱스 (서버)
        /// </summary>
        public int index;
        private int id;
        private byte state;
        private float posX;
        private float posZ;
        private bool hasBossHp;
        /// <summary>
        /// 보스 Hp (보스의 경우에만 값 존재)
        /// </summary>
        public int bossHp;

        private int level;
        private float scale;
        private float? moveSpeed;

        MonsterType ISpawnMonster.Type => monsterType;
        int ISpawnMonster.Id => id;
        int ISpawnMonster.Level => level;
        float ISpawnMonster.Scale => scale;

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
        bool IMonsterBotInput.HasCurHp => hasBossHp;
        int IMonsterBotInput.CurHp => bossHp;
        CrowdControlType IMonsterBotInput.CrowdControl => default;
        float? IMonsterBotInput.MoveSpeed => moveSpeed;

        public virtual void Initialize(Response response)
        {
            index = response.GetByte("1");
            id = response.GetInt("2");
            state = response.GetByte("3");
            posX = response.GetFloat("4");
            posZ = response.GetFloat("5");
            hasBossHp = response.ContainsKey("6");
            bossHp = hasBossHp ? response.GetInt("6") : 0;
            monsterType = index == BOSS_MONSTER_INDEX ? MonsterType.Boss : MonsterType.Normal;
        }

        public void SetLevel(int level)
        {
            this.level = level;
        }

        public void SetScale(float scale)
        {
            this.scale = scale;
        }

        public void SetMoveSpeed(float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
        }
    }
}