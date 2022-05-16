using GamePotUnity;

namespace Ragnarok
{
    public static class Cheat
    {
        private enum Option
        {
            AllOpenContent, // 모든 컨텐츠 오픈
            ForceJoinFreeFight, // 난전강제입장
            UseTutorial, // 튜토리얼
            IsSnow, // 멀티미로 대기실 눈효과
            MonsterLevel, // 몬스터 레벨 보기
            IsGlobal, // 글로벌 여부
            IsFacebook, // 페이스북 친구 여부
            IsOnBuff, // 온버프 여부
            IsAgreeGDPR, // 약관동의 GDPR 여부
        }

        public static bool All_OPEN_CONTENT
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Option.AllOpenContent), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Option.AllOpenContent), value);
#else
            get => false;
#endif
        }

        public static bool FORCE_JOIN_FREE_FIGHT
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Option.ForceJoinFreeFight), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Option.ForceJoinFreeFight), value);
#else
            get => false;
#endif
        }

        public static bool USE_TUTORIAL
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Option.UseTutorial), defaultValue: true);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Option.UseTutorial), value);
#else
            get => Issue.USE_TUTORIAL;
#endif
        }

        public static bool IS_SNOW
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Option.IsSnow), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Option.IsSnow), value);
#else
            get => false;
#endif
        }

        public static bool IS_MONSTER_LEVEL
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Option.MonsterLevel), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Option.MonsterLevel), value);
#else
            get => false;
#endif
        }

        public static bool IS_GLOBAL
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Option.IsGlobal), defaultValue: true);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Option.IsGlobal), value);
#else
            get => GamePot.getConfig("Project").Equals("Global");
#endif
        }

        public static bool IsFacebook
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Option.IsFacebook), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Option.IsFacebook), value);
#else
            get => ConnectionManager.Instance.IsFaceBookFriend();
#endif
        }

        public static bool IS_OnBuff
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Option.IsOnBuff), defaultValue: true);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Option.IsOnBuff), value);
#else
            get => ConnectionManager.Instance.IsOnBuff();
#endif
        }

        public static bool IsAgreeGDPR
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Option.IsAgreeGDPR), defaultValue: true);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Option.IsAgreeGDPR), value);
#else
            get => ConnectionManager.Instance.IsAgreeGDPR();
#endif
        }
    }
}