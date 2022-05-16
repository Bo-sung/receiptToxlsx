namespace Ragnarok
{
    public static class ServerIssue
    {
        private enum Type
        {
            EnterSecondServer,
        }

        public static bool IsEnterSecondServer
        {
#if UNITY_EDITOR
            get => UnityEditor.EditorPrefs.GetBool(nameof(Type.EnterSecondServer), defaultValue: false);
            set => UnityEditor.EditorPrefs.SetBool(nameof(Type.EnterSecondServer), value);
#else
            get => false;
#endif
        }
    }
}