namespace Ragnarok
{
    /// <summary>
    /// 연결에러: 강제업데이트 or 점검
    /// </summary>
    public class ConnectException : NetworkException
    {
        private readonly ResultCode resultCode;
        private readonly string serverErrorMessage;

        public override string Message
        {
            get
            {
                if (!string.IsNullOrEmpty(serverErrorMessage))
                    return serverErrorMessage;

                return resultCode.GetDescription();
            }
        }

        public ConnectException(ResultCode resultCode, string serverErrorMessage)
        {
            this.resultCode = resultCode;
            this.serverErrorMessage = serverErrorMessage;
        }

        public override void Execute()
        {
            // 같은 계정으로 중복 접속 시 호출
            if (resultCode == ResultCode.DUPLICATION_LOGIN)
            {
                ConnectionManager.Reconnect(); // 재접속 시도
                return;
            }

            if (resultCode.Execute())
                return;

            UI.ConfirmPopup(Message, SceneLoader.LoadIntro);
        }
    }
}