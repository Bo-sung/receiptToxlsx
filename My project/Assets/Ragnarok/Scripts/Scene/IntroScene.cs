using UnityEngine;

namespace Ragnarok
{
    public class IntroScene : MonoBehaviour
    {
        /// <summary>
        /// 어떤 목적으로 타이틀로 돌아갔을 경우 (임시 처리)
        /// (게임 중 캐릭터 선택하러, 로그아웃 하러)
        /// </summary>
        public static bool IsBackToTitle;       

        /// <summary>
        /// 다른서버로 접속 (인증부터 다시 접속해야함)
        /// </summary>
        public static bool IsAutoAuthLogin;

        void Start()
        {
            //if (!IsBackToTitle)
            //{
            //    PlayerPrefs.DeleteKey(nameof(UIMakeAnimation)); // UIMake 애니메이션 최초 한번만 실행하도록
            //}

            UI.Show<UIIntro>().OnTitle();
        }
    }
}