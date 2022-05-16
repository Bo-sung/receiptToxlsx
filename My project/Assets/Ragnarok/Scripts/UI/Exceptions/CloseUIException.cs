namespace Ragnarok
{
    public class CloseUIException : UIException
    {
        static volatile CloseUIException defaultException;

        public static CloseUIException Default
        {
            get
            {
                CloseUIException exception = defaultException;

                if (exception == null)
                {
                    exception = new CloseUIException();
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