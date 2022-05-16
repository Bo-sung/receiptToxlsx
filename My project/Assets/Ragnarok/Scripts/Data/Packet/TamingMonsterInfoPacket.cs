namespace Ragnarok
{
    public class TamingMonsterInfoPacket : IPacket<Response>, ITamingMonsterPotInput
    {
        private int spwan_position; // 몬스터 생성 위치
        private byte state;
        private RemainTime ready_cool; // status가 1일경우 값이 셋팅된다.
        private int monster_level;
        private int monster_id;

        MonsterType ISpawnMonster.Type => MonsterType.Normal;
        int ISpawnMonster.Id => monster_id;
        int ISpawnMonster.Level => monster_level;
        float ISpawnMonster.Scale => 1f;

        int ITamingMonsterPotInput.Index => spwan_position;
        byte ITamingMonsterPotInput.State => state;

        void IInitializable<Response>.Initialize(Response response)
        {
            state = (byte)response.GetInt("1");
            spwan_position = response.GetInt("2");
            ready_cool = response.GetLong("3");
            monster_level = response.GetInt("4");
            monster_id = response.GetInt("5");
        }
    }
}