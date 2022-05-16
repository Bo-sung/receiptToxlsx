#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    [ExecuteInEditMode]
    public class TestLocalization : TestCode
    {
        [SerializeField]
        LanguageType languageType = LanguageType.ENGLISH;

        protected override void OnMainTest()
        {
            LanguageType curLang = Language.Current;
            LanguageType newLang = curLang == LanguageType.KOREAN ? languageType : LanguageType.KOREAN;
            Language.SetLanguageType(newLang);
        }
    }
}
#endif