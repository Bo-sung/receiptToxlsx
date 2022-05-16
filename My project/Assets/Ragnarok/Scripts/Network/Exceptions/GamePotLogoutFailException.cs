using UnityEngine;

namespace Ragnarok
{
    public sealed class GamePotLogoutFailException : LogoutFailException
    {
        private readonly NError error;

        /// <summary>
        /// 게임팟 로그아웃 실패
        /// </summary>
        public GamePotLogoutFailException(NError error) : base(error.message)
        {
            this.error = error;
        }

        public override void Execute()
        {
            base.Execute();

            // 로그아웃을 실패하는 경우
            // error.message를 팝업 등으로 유저에게 알려주세요.
            Debug.LogError(error.code);
        }
    }
}