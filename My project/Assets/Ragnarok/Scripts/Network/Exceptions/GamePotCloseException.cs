using UnityEngine;

namespace Ragnarok
{
    public sealed class GamePotCloseException : NetworkException
    {
        /// <summary>
        /// 게임팟 종료
        /// </summary>
        public override void Execute()
        {
            // TODO: 강제 업데이트나 점검 기능을 case 2 방식으로 구현하는 경우
            // TODO: 앱을 강제 종료할 수 있기 때문에 이 곳에 앱을 종료할 수 있도록 구현하세요.
            Application.Quit();
        }
    }
}