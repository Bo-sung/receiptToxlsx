using GamePotUnity;

namespace Ragnarok
{
    public sealed class GamePotConnectException : NetworkException
    {
        private readonly NAppStatus status;

        /// <summary>
        /// 강제업데이트/점검
        /// </summary>
        public GamePotConnectException(NAppStatus status) : base(status.message)
        {
            this.status = status;
        }

        public override void Execute()
        {
            // TODO: 파라미터로 넘어온 status 정보를 토대로 팝업을 만들어 사용자에게 알려줘야 합니다.
            // TODO: 아래 두 가지 방식 중 한 가지를 선택하세요.
            // case 1: 인게임 팝업을 통해 개발사에서 직접 UI 구현
            // case 2: SDK의 팝업을 사용(이 경우에는 아래 코드를 호출해 주세요.)
            GamePot.showAppStatusPopup(status.ToJson());
        }
    }
}