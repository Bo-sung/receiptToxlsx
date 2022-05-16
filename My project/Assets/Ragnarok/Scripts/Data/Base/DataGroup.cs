namespace Ragnarok
{
    public class DataGroup<T> : BetterList<T>, IInitializable
        where T : class, IData
    {
        public T First { get; private set; }
        public T Last { get; private set; }

        public void Initialize()
        {
            First = size > 0 ? base[0] : null;
            Last = size > 0 ? base[size - 1] : null;
        }
    }
}