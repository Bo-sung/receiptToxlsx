namespace Ragnarok
{
    public sealed class ResponseException : NetworkException
    {
        public readonly Response response;
        public readonly ResultCode resultCode;

        public override string Message => resultCode.GetDescription();

        public ResponseException(Response response)
        {
            this.response = response;
            resultCode = response.resultCode;
        }

        public override void Execute()
        {
            if (resultCode.Execute())
                return;

            resultCode.ShowResultCode();
        }
    }
}