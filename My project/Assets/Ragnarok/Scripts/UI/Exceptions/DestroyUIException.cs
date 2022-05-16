namespace Ragnarok
{
    public class DestroyUIException : UIException
    {
        static volatile DestroyUIException defaultException;

        public static DestroyUIException Default
        {
            get
            {
                DestroyUIException exception = defaultException;

                if (exception == null)
                {
                    exception = new DestroyUIException();
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