using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    public static class PreferenceItems
    {
        private readonly static DefineSymbols DEFINE_SYMBOLS;

        static PreferenceItems()
        {
            DEFINE_SYMBOLS = new DefineSymbols();
        }

        [PreferenceItem("라그나로크")]
        private static void EditorPreferences()
        {
            EditorGUILayout.Separator();
        }

        [PreferenceItem("라그나로크/Define Symbols")]
        private static void ShowDefineSymbols()
        {
            EditorGUILayout.Separator();
            DEFINE_SYMBOLS.Draw();
        }
    }
}