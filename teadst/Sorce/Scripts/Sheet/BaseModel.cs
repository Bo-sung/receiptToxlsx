namespace SheetViewer
{
    public class Model
    {
        protected static Model instance;
        public static Model GetInstance()
        {
            if (instance == null)
                instance = new Model();
            return instance;
        }
        protected Model()
        {
            instance = this;
        }
    }
    public interface IModel
    {

    }
}
