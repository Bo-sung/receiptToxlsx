namespace Ragnarok
{
    public struct Issue
    {
        /// <summary>
        /// 처음 시작 시, 언어 선택 팝업을 띄움 여부
        /// <para> true : 처음 시작 시, 무조건 언어 선택 팝업을 띄운다 </para>
        /// <para> false : 처음 시작 시, 선택한 언어 값이 없을 경우에만 언어 선택 팝업을 띄운다 </para>
        /// </summary>
        public static readonly Issue TEST_ALWAYS_SHOW_LANGUAGE_POPUP = false;

        /// <summary>
        /// 큐펫 페널 적용 테스트
        /// </summary>
        public static readonly Issue TEST_APPLY_CUPET_PANEL = false;

        /// <summary>
        /// 보스가 죽었을 때 코인 Drop 유무
        /// <para> true: 보스가 죽었을 때 코인을 Drop 한다 </para>
        /// <para> false: 보스가 죽었을 때 코인을 Drop 하지 않는다 </para>
        /// </summary>
        public static readonly Issue DROP_BOSS_COIN = false;

        /// <summary>
        /// 코나미 커맨드 사용 (△ △ ▽ ▽ ◁ ▷ ◁ ▷ B A)
        /// </summary>
        public static readonly Issue USE_KONAMI_COMMAND = true;

        /// <summary>
        /// 큐펫 패널버프 시스템 사용 안함
        /// </summary>
        public static readonly Issue DISABLE_PANEL_BUFF = true;

        /// <summary>
        /// 튜토리얼 사용 여부
        /// </summary>
        public static readonly Issue USE_TUTORIAL = true;

        /// <summary>
        /// 던전 전투 준비 액터 NULL 체크
        /// </summary>
        public static readonly Issue ACTOR_NULL = true;

        /// <summary>
        /// 스킬 쿨타임 체크 에러 표시 여부
        /// </summary>
        public static readonly Issue SHOW_SKILL_COOLTIME_CHECK_ERROR = false;

#if UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
        /// <summary>
        /// UI 캐싱
        /// </summary>
        public static readonly Issue UI_CACHING = false;
#else
        /// <summary>
        /// UI 캐싱
        /// </summary>
        public static readonly Issue UI_CACHING = true;
#endif

        public static readonly Issue NAVER_LOUNGE = true;

        private readonly bool value;
        private Issue(bool value)
        {
            this.value = value;
        }

        public static implicit operator Issue(bool value)
        {
            return new Issue(value);
        }

        public static implicit operator bool(Issue issue)
        {
            return issue.value;
        }
    }
}