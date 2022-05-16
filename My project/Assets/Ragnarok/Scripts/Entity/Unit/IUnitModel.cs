namespace Ragnarok
{
    public interface IUnitModel
    {
        void AddEvent(UnitEntityType type);

        void RemoveEvent(UnitEntityType type);

        void ResetData();
    }
}