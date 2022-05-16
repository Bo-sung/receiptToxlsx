namespace Ragnarok
{
    public class DuplicateUIException : UIException
    {
        static volatile DuplicateUIException defaultException;

        public static DuplicateUIException Default
        {
            get
            {
                DuplicateUIException exception = defaultException;

                if (exception == null)
                {
                    exception = new DuplicateUIException();
                    defaultException = exception;
                }

                return exception;
            }
        }

        public override void Execute()
        {
            // Do Nothing
        }
    }
}