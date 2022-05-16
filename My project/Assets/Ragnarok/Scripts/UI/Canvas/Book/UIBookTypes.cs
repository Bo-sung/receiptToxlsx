namespace Ragnarok
{
    public class BookStateDecoratedData
    {
        public object data;
        public bool isRecorded;

        public T GetData<T>() where T : class
        {
            return data as T;
        }
    }
}
