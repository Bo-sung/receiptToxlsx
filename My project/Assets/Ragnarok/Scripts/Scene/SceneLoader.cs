using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public static class SceneLoader
    {
        private const string TAG = nameof(SceneLoader);

        public const string PRELOAD = "PreLoad";
        public const string INTRO = "Intro";
        private const string LOADING = "Loading";

        public enum SceneType
        {
            Intro,
            Dungeon,
        }

        public enum LoadingStoryboardType
        {
            Immediate,
            Async,
        }

        public delegate void SceneLoadedEvent(Scene scene, LoadSceneMode mode);
        public delegate void TitleSceneLoadedEvent();
        public delegate void StartLoadSceneEvent();
        public delegate void FadeSceneEvent();
        public delegate void FinishLoadSceneEvent();

        /// <summary>
        /// 타이틀 씬 외의 씬 로드 완료 시 호출 (단순 씬 로드완료로 로딩 화면이 보일 수 있다)
        /// </summary>
        public static event SceneLoadedEvent OnSceneLoaded;

        /// <summary>
        /// 타이틀 씬 로드 완료 시 호출
        /// </summary>
        public static event TitleSceneLoadedEvent OnTitleSceneLoaded;

        /// <summary>
        /// 씬 이동 시작 전 호출 (로딩 화면 보이기 시작할 때 호출된다)
        /// </summary>
        public static event StartLoadSceneEvent OnStartLoadScene;

        /// <summary>
        /// 씬 이동 완료 시 호출 (로딩 화면까지 사라지고 난 후에 호출)
        /// </summary>
        public static event FinishLoadSceneEvent OnFinishLoadScene;

        /// <summary>
        /// 현재 씬
        /// </summary>
        public static SceneType CurrentType { get; private set; }

        /// <summary>
        /// 현재 씬 이름
        /// </summary>
        private static string currentSceneName;

        /// <summary>
        /// 인트로 씬으로 이동
        /// </summary>
        public static void LoadIntro()
        {
            CurrentType = SceneType.Intro;
            LoadScene(INTRO, LoadingStoryboardType.Immediate);
        }

        /// <summary>
        /// 던전 씬으로 이동
        /// </summary>
        public static void LoadDungeon(string sceneName, LoadingStoryboardType type, SceneType sceneType)
        {
            CurrentType = sceneType;
            LoadScene(sceneName, type);
        }

        /// <summary>
        /// 현재 씬 체크
        /// </summary>
        public static bool IsCheckCurrentScene(string sceneName)
        {
            return string.Equals(currentSceneName, sceneName);
        }

        /// <summary>
        /// 특정 씬으로 이동
        /// </summary>
        private static void LoadScene(string sceneName, LoadingStoryboardType storyboardType)
        {
            StopAllCoroutine(); // 모든 코루틴 종료

            switch (storyboardType)
            {
                case LoadingStoryboardType.Immediate:
                    LoadSceneImmediate(sceneName);
                    break;

                case LoadingStoryboardType.Async:
                    Timing.RunCoroutine(YieldLoadScene(sceneName), TAG);
                    break;
            }
        }

        /// <summary>
        /// 특정 씬으로 이동 (로딩화면 - Nothing)
        /// </summary>
        private static void LoadSceneImmediate(string sceneName)
        {
            OnStartLoadScene?.Invoke(); // 씬 로드 시작

            if (AssetManager.IsAllAssetReady)
            {
                SceneManager.LoadScene(LOADING);
                System.GC.Collect();
                UIDrawCall.ReleaseInactive();
                Resources.UnloadUnusedAssets();
            }

            SceneManager.LoadScene(sceneName);

            OnFinishLoadScene?.Invoke(); // 씬 로드 종료
        }

        /// <summary>
        /// 특정 씬으로 이동 (로딩화면 - Fade)
        /// </summary>
        private static IEnumerator<float> YieldLoadScene(string sceneName)
        {
            OnStartLoadScene?.Invoke(); // 씬 로드 시작
            yield return Timing.WaitUntilDone(SceneManager.LoadSceneAsync(sceneName));
            OnFinishLoadScene?.Invoke(); // 씬 로드 종료
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 로딩씬의 경우에는 이벤트 호출 금지
            if (scene.name.Equals(LOADING))
                return;

            currentSceneName = scene.name;
            OnSceneLoaded?.Invoke(scene, mode);

            if (scene.buildIndex == 1)
            {
                BattleTime.IsPause = false; // 일시정지 해제
                OnTitleSceneLoaded?.Invoke();
            }
        }

        private static void StopAllCoroutine()
        {
            Timing.KillCoroutines(TAG);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }
    }
}