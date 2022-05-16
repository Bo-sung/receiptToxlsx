using UnityEditor;

namespace Ragnarok
{
    public static class EditorLocalValue
    {
        private enum Type
        {
            /// <summary>
            /// 회사코드
            /// </summary>
            CompanyCode,
        }

        public static void Clear()
        {
            foreach (Type item in System.Enum.GetValues(typeof(Type)))
            {
                string key = item.ToString();
                if (!EditorPrefs.HasKey(key))
                    continue;

                EditorPrefs.DeleteKey(key);
            }
        }

        /// <summary>
        /// 회사코드
        /// </summary>
        public static string CompanyCode
        {
            get => EditorPrefs.GetString(nameof(Type.CompanyCode), defaultValue: string.Empty);
            set => EditorPrefs.SetString(nameof(Type.CompanyCode), value);
        }
    }
}