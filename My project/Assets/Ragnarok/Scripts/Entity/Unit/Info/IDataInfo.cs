namespace Ragnarok
{
    public interface IDataInfo<TData> : IInfo, IUIData
        where TData : class, IData
    {
        void SetData(TData data);
    }
}