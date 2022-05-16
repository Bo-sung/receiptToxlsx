using UnityEngine;

namespace Ragnarok
{
    public static class Tutorial
    {
        public static bool isInProgress; // 튜토리얼 진행 중
        public static TutorialType current; // 진행중인 튜토리얼 타입

        public static event System.Action<TutorialType> OnStart;
        public static event System.Action<TutorialType> OnFinish;

        /// <summary>
        /// 이미 진행한 튜토리얼 여부
        /// </summary>
        public static bool HasAlreadyFinished(TutorialType tutorialType)
        {
            return Entity.player.Tutorial.HasAlreadyFinished(tutorialType);
        }

        /// <summary>
        /// 튜토리얼 실행
        /// </summary>
        public static bool Run(TutorialType tutorialType)
        {
            if (!Cheat.USE_TUTORIAL)
                return false;

            // 이미 진행한 튜토리얼
            if (HasAlreadyFinished(tutorialType))
                return false;

            return Start(tutorialType);
        }

        /// <summary>
        /// 튜토리얼 강제 실행
        /// </summary>
        public static bool ForceRun(TutorialType tutorialType)
        {
            if (!Cheat.USE_TUTORIAL)
                return false;

            return Start(tutorialType);
        }

        /// <summary>
        /// 튜토리얼 종료
        /// </summary>
        public static void Finish()
        {
            if (!isInProgress)
                return;

            TutorialType savedTutorialType = current;
            isInProgress = false;
            current = TutorialType.None;

            OnFinish?.Invoke(savedTutorialType);
        }

        /// <summary>
        /// 튜토리얼 스텝 서버로 보내기
        /// </summary>
        public static void RequestTutorialStep(TutorialType tutorialType)
        {
            Entity.player.Tutorial.RequestTutorialStep(tutorialType).WrapNetworkErrors();
        }

        /// <summary>
        /// 튜토리얼 시작
        /// </summary>
        private static bool Start(TutorialStep step)
        {
            // 다른 튜토리얼이 이미 진행중
            if (isInProgress)
                return false;

            // 튜토리얼 스텝이 존재하지 않음
            if (step == null)
                return false;

            // 튜토리얼 조건 만족하지 않음
            if (!step.IsCheckCondition())
                return false;

            isInProgress = true;
            current = step.type;
            OnStart?.Invoke(step.type);

            step.Start();
            return true;
        }

        /// <summary>
        /// 튜토리얼 중단
        /// </summary>
        private static void Abort(TutorialStep step)
        {
            if (step == null)
                return;

            isInProgress = false;
            current = default;

            step.Abort();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            // TutorialStep 를 상속받는 모든 Type의 Intsance 생성
            var types = ReflectionUtils.GetAllClasses<TutorialStep>(); // TutorialStep 를 상속받는 모든 Type 호출
            foreach (var item in types)
            {
                System.Activator.CreateInstance(item); // Instance 생성
            }

            //SceneLoader.OnTitleSceneLoaded += Abort;
            OnFinish += OnFinishTutorial;
        }

        /// <summary>
        /// 튜토리얼 강제 중단
        /// </summary>
        public static void Abort()
        {
            Abort(current);
        }

        private static void OnFinishTutorial(TutorialType tutorialType)
        {
        }
    }
}