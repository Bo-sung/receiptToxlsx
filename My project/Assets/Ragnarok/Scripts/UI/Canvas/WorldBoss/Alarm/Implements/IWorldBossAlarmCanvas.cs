namespace Ragnarok
{
    public interface IWorldBossAlarmCanvas
    {
        void SetActive(bool isActive);
        void SetMonster(MonsterInfo info);
        void SetProgress(float value);
        bool IsActive();
    } 
}
