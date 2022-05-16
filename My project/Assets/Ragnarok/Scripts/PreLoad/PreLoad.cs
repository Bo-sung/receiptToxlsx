using UnityEngine;

namespace Ragnarok
{
    public class PreLoad : MonoBehaviour
    {
        void Awake()
        {
            // 게임 실행 중 화면이 꺼지지 않도록 설정
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            // 기본 타겟프레임 30FPS
            Application.targetFrameRate = 30;
        }

        void Start()
        {
            SceneLoader.LoadIntro(); // 타이틀 화면으로 이동
        }
    }
}