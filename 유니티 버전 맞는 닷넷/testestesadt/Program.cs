using System;
using System.Text.RegularExpressions;
using NTextCat;

namespace testestesadt
{
    internal class Program
    {
        public class LanguageClass
        {
            #region 클래스 모음
            public static readonly LanguageClass BasicLatin                                       = new LanguageClass( '\u0000','\u007F', "IsBasicLatin");
            public static readonly LanguageClass Latin_1Supplement                                = new LanguageClass( '\u0080','\u00FF',"IsLatin_1Suppleme");
            public static readonly LanguageClass LatinExtended_A                                  = new LanguageClass( '\u0100','\u017F',"IsLatinExtended_A");
            public static readonly LanguageClass LatinExtended_B                                  = new LanguageClass( '\u0180','\u024F',"IsLatinExtended_B");
            public static readonly LanguageClass IPAExtensions                                    = new LanguageClass( '\u0250','\u02AF',"IsIPAExtensio");
            public static readonly LanguageClass SpacingModifierLetters                           = new LanguageClass( '\u02B0','\u02FF',"IsSpacingModifierLett");
            public static readonly LanguageClass CombiningDiacriticalMarks                        = new LanguageClass( '\u0300','\u036F',"IsCombiningDiacriticalMar");
            public static readonly LanguageClass GreekandCoptic                                   = new LanguageClass( '\u0370','\u03FF',"IsGreekandCop");
            public static readonly LanguageClass Cyrillic                                         = new LanguageClass( '\u0400','\u04FF',"IsCyrilli");
            public static readonly LanguageClass CyrillicSupplement                               = new LanguageClass( '\u0500','\u052F',"IsCyrillicSupplem");
            public static readonly LanguageClass Armenian                                         = new LanguageClass( '\u0530','\u058F',"IsArmenia");
            public static readonly LanguageClass Hebrew                                           = new LanguageClass( '\u0590','\u05FF',"IsHeb");
            public static readonly LanguageClass Arabic                                           = new LanguageClass( '\u0600','\u06FF',"IsAra");
            public static readonly LanguageClass Syriac                                           = new LanguageClass( '\u0700','\u074F',"IsSyr");
            public static readonly LanguageClass Thaana                                           = new LanguageClass( '\u0780','\u07BF',"IsTha");
            public static readonly LanguageClass Devanagari                                       = new LanguageClass( '\u0900','\u097F',"IsDevanag");
            public static readonly LanguageClass Bengali                                          = new LanguageClass( '\u0980','\u09FF',"IsBengali");
            public static readonly LanguageClass Gurmukhi                                         = new LanguageClass( '\u0A00','\u0A7F',"IsGurmukh");
            public static readonly LanguageClass Gujarati                                         = new LanguageClass( '\u0A80','\u0AFF',"IsGujarat");
            public static readonly LanguageClass Oriya                                            = new LanguageClass( '\u0B00','\u0B7F',"IsOri");
            public static readonly LanguageClass Tamil                                            = new LanguageClass( '\u0B80','\u0BFF',"IsTam");
            public static readonly LanguageClass Telugu                                           = new LanguageClass( '\u0C00','\u0C7F',"IsTel");
            public static readonly LanguageClass Kannada                                          = new LanguageClass( '\u0C80','\u0CFF',"IsKannada");
            public static readonly LanguageClass Malayalam                                        = new LanguageClass( '\u0D00','\u0D7F',"IsMalayal");
            public static readonly LanguageClass Sinhala                                          = new LanguageClass( '\u0D80','\u0DFF',"IsSinhala");
            public static readonly LanguageClass Thai                                             = new LanguageClass( '\u0E00','\u0E7F',"IsThai");
            public static readonly LanguageClass Lao                                              = new LanguageClass( '\u0E80','\u0EFF',"IsLao");
            public static readonly LanguageClass Tibetan                                          = new LanguageClass( '\u0F00','\u0FFF',"IsTibetan");
            public static readonly LanguageClass Myanmar                                          = new LanguageClass( '\u1000','\u109F',"IsMyanmar");
            public static readonly LanguageClass Georgian                                         = new LanguageClass( '\u10A0','\u10FF',"IsGeorgia");
            public static readonly LanguageClass HangulJamo                                       = new LanguageClass( '\u1100','\u11FF',"IsHangulJ");
            public static readonly LanguageClass Ethiopic                                         = new LanguageClass( '\u1200','\u137F',"IsEthiopi");
            public static readonly LanguageClass Cherokee                                         = new LanguageClass( '\u13A0','\u13FF',"IsCheroke");
            public static readonly LanguageClass UnifiedCanadianAboriginalSyllabics               = new LanguageClass( '\u1400','\u167F',"IsUnifiedCanadianAboriginalSyllab");
            public static readonly LanguageClass Ogham                                            = new LanguageClass( '\u1680','\u169F',"IsOgh");
            public static readonly LanguageClass Runic                                            = new LanguageClass( '\u16A0','\u16FF',"IsRun");
            public static readonly LanguageClass Tagalog                                          = new LanguageClass( '\u1700','\u171F',"IsTagalog");
            public static readonly LanguageClass Hanunoo                                          = new LanguageClass( '\u1720','\u173F',"IsHanunoo");
            public static readonly LanguageClass Buhid                                            = new LanguageClass( '\u1740','\u175F',"IsBuh");
            public static readonly LanguageClass Tagbanwa                                         = new LanguageClass( '\u1760','\u177F',"IsTagbanw");
            public static readonly LanguageClass Khmer                                            = new LanguageClass( '\u1780','\u17FF',"IsKhm");
            public static readonly LanguageClass Mongolian                                        = new LanguageClass( '\u1800','\u18AF',"IsMongoli");
            public static readonly LanguageClass Limbu                                            = new LanguageClass( '\u1900','\u194F',"IsLim");
            public static readonly LanguageClass TaiLe                                            = new LanguageClass( '\u1950','\u197F',"IsTai");
            public static readonly LanguageClass KhmerSymbols                                     = new LanguageClass( '\u19E0','\u19FF',"IsKhmerSymbol");
            public static readonly LanguageClass PhoneticExtensions                               = new LanguageClass( '\u1D00','\u1D7F',"IsPhoneticExtensi");
            public static readonly LanguageClass LatinExtendedAdditional                          = new LanguageClass( '\u1E00','\u1EFF',"IsLatinExtendedAdditional");
            public static readonly LanguageClass GreekExtended                                    = new LanguageClass( '\u1F00','\u1FFF',"IsGreekExtend");
            public static readonly LanguageClass GeneralPunctuation                               = new LanguageClass( '\u2000','\u206F',"IsGeneralPunctuat");
            public static readonly LanguageClass SuperscriptsandSubscripts                        = new LanguageClass( '\u2070','\u209F',"IsSuperscriptsandSubscrip");
            public static readonly LanguageClass CurrencySymbols                                  = new LanguageClass( '\u20A0','\u20CF',"IsCurrencySymbols");
            public static readonly LanguageClass CombiningDiacriticalMarksforSymbols              = new LanguageClass( '\u20D0','\u20FF',"IsCombiningDiacriticalMarksforSymbols");
            public static readonly LanguageClass LetterlikeSymbols                                = new LanguageClass( '\u2100','\u214F',"IsLetterlikeSymbo");
            public static readonly LanguageClass NumberForms                                      = new LanguageClass( '\u2150','\u218F',"IsNumberForms");
            public static readonly LanguageClass Arrows                                           = new LanguageClass( '\u2190','\u21FF',"IsArr");
            public static readonly LanguageClass MathematicalOperators                            = new LanguageClass( '\u2200','\u22FF',"IsMathematicalOperato");
            public static readonly LanguageClass MiscellaneousTechnical                           = new LanguageClass( '\u2300','\u23FF',"IsMiscellaneousTechni");
            public static readonly LanguageClass ControlPictures                                  = new LanguageClass( '\u2400','\u243F',"IsControlPictures");
            public static readonly LanguageClass OpticalCharacterRecognition                      = new LanguageClass( '\u2440','\u245F',"IsOpticalCharacterRecognition");
            public static readonly LanguageClass EnclosedAlphanumerics                            = new LanguageClass( '\u2460','\u24FF',"IsEnclosedAlphanumeri");
            public static readonly LanguageClass BoxDrawing                                       = new LanguageClass( '\u2500','\u257F',"IsBoxDraw");
            public static readonly LanguageClass BlockElements                                    = new LanguageClass( '\u2580','\u259F',"IsBlockElemen");
            public static readonly LanguageClass GeometricShapes                                  = new LanguageClass( '\u25A0','\u25FF',"IsGeometricShapes");
            public static readonly LanguageClass MiscellaneousSymbols                             = new LanguageClass( '\u2600','\u26FF',"IsMiscellaneousSymbol");
            public static readonly LanguageClass Dingbats                                         = new LanguageClass( '\u2700','\u27BF',"IsDingbat");
            public static readonly LanguageClass MiscellaneousMathematicalSymbols_A               = new LanguageClass( '\u27C0','\u27EF',"IsMiscellaneousMathematicalSymbol");
            public static readonly LanguageClass SupplementalArrows_A                             = new LanguageClass( '\u27F0','\u27FF',"IsSupplementalArrows_");
            public static readonly LanguageClass BraillePatterns                                  = new LanguageClass( '\u2800','\u28FF',"IsBraillePatterns");
            public static readonly LanguageClass SupplementalArrows_B                             = new LanguageClass( '\u2900','\u297F',"IsSupplementalArrows_");
            public static readonly LanguageClass MiscellaneousMathematicalSymbols_B               = new LanguageClass( '\u2980','\u29FF',"IsMiscellaneousMathematicalSymbol");
            public static readonly LanguageClass SupplementalMathematicalOperators                = new LanguageClass( '\u2A00','\u2AFF',"IsSupplementalMathematicalOperato");
            public static readonly LanguageClass MiscellaneousSymbolsandArrows                    = new LanguageClass( '\u2B00','\u2BFF',"IsMiscellaneousSymbolsandArro");
            public static readonly LanguageClass CJKRadicalsSupplement                            = new LanguageClass( '\u2E80','\u2EFF',"IsCJKRadicalsSuppleme");
            public static readonly LanguageClass KangxiRadicals                                   = new LanguageClass( '\u2F00','\u2FDF',"IsKangxiRadic");
            public static readonly LanguageClass IdeographicDescriptionCharacters                 = new LanguageClass( '\u2FF0','\u2FFF',"IsIdeographicDescriptionCharacter");
            public static readonly LanguageClass CJKSymbolsandPunctuation                         = new LanguageClass( '\u3000','\u303F',"IsCJKSymbolsandPunctuatio");
            public static readonly LanguageClass Hiragana                                         = new LanguageClass( '\u3040','\u309F',"IsHiragan");
            public static readonly LanguageClass Katakana                                         = new LanguageClass( '\u30A0','\u30FF',"IsKatakan");
            public static readonly LanguageClass Bopomofo                                         = new LanguageClass( '\u3100','\u312F',"IsBopomof");
            public static readonly LanguageClass HangulCompatibilityJamo                          = new LanguageClass( '\u3130','\u318F',"IsHangulCompatibilityJamo");
            public static readonly LanguageClass Kanbun                                           = new LanguageClass( '\u3190','\u319F',"IsKan");
            public static readonly LanguageClass BopomofoExtended                                 = new LanguageClass( '\u31A0','\u31BF',"IsBopomofoExtende");
            public static readonly LanguageClass KatakanaPhoneticExtensions                       = new LanguageClass( '\u31F0','\u31FF',"IsKatakanaPhoneticExtensi");
            public static readonly LanguageClass EnclosedCJKLettersandMonths                      = new LanguageClass( '\u3200','\u32FF',"IsEnclosedCJKLettersandMonths");
            public static readonly LanguageClass CJKCompatibility                                 = new LanguageClass( '\u3300','\u33FF',"IsCJKCompatibilit");
            public static readonly LanguageClass CJKUnifiedIdeographsExtensionA                   = new LanguageClass( '\u3400','\u4DBF',"IsCJKUnifiedIdeographsExtensi");
            public static readonly LanguageClass YijingHexagramSymbols                            = new LanguageClass( '\u4DC0','\u4DFF',"IsYijingHexagramSymbo");
            public static readonly LanguageClass CJKUnifiedIdeographs                             = new LanguageClass( '\u4E00','\u9FFF',"IsCJKUnifiedIdeograph");
            public static readonly LanguageClass YiSyllables                                      = new LanguageClass( '\uA000','\uA48F',"IsYiSyllables");
            public static readonly LanguageClass YiRadicals                                       = new LanguageClass( '\uA490','\uA4CF',"IsYiRadic");
            public static readonly LanguageClass HangulSyllables                                  = new LanguageClass( '\uAC00','\uD7AF',"IsHangulSyllables");
            public static readonly LanguageClass HighSurrogates                                   = new LanguageClass( '\uD800','\uDB7F',"IsHighSurroga");
            public static readonly LanguageClass HighPrivateUseSurrogates                         = new LanguageClass( '\uDB80','\uDBFF',"IsHighPrivateUseSurrogate");
            public static readonly LanguageClass LowSurrogates                                    = new LanguageClass( '\uDC00','\uDFFF',"IsLowSurrogat");
            public static readonly LanguageClass PrivateUse                                       = new LanguageClass( '\uE000','\uF8FF',"IsPrivate");
            public static readonly LanguageClass CJKCompatibilityIdeographs                       = new LanguageClass( '\uF900','\uFAFF',"IsCJKCompatibilityIdeogra");
            public static readonly LanguageClass AlphabeticPresentationForms                      = new LanguageClass( '\uFB00','\uFB4F',"IsAlphabeticPresentationForms");
            public static readonly LanguageClass ArabicPresentationForms_A                        = new LanguageClass( '\uFB50','\uFDFF',"IsArabicPresentationForms");
            public static readonly LanguageClass VariationSelectors                               = new LanguageClass( '\uFE00','\uFE0F',"IsVariationSelect");
            public static readonly LanguageClass CombiningHalfMarks                               = new LanguageClass( '\uFE20','\uFE2F',"IsCombiningHalfMa");
            public static readonly LanguageClass CJKCompatibilityForms                            = new LanguageClass( '\uFE30','\uFE4F',"IsCJKCompatibilityFor");
            public static readonly LanguageClass SmallFormVariants                                = new LanguageClass( '\uFE50','\uFE6F',"IsSmallFormVarian");
            public static readonly LanguageClass ArabicPresentationForms_B                        = new LanguageClass( '\uFE70','\uFEFF',"IsArabicPresentationForms");
            public static readonly LanguageClass HalfwidthandFullwidthForms                       = new LanguageClass( '\uFF00','\uFFEF',"IsHalfwidthandFullwidthFo");
            public static readonly LanguageClass Specials                                         = new LanguageClass( '\uFFF0','\uFFFF',"IsSpecial");
            #endregion

            #region 언어 순서
            public enum LangType
            {
                IsBasicLatin                                                         ,
IsLatin_1Supplement                                                                  ,
IsLatinExtended_A                                                                    ,
IsLatinExtended_B                                                                    ,
IsIPAExtensions                                                                      ,
IsSpacingModifierLetters                                                             ,
IsCombiningDiacriticalMarks                                                          ,
IsGreekandCoptic                                                                     ,
IsCyrillic                                                                           ,
IsCyrillicSupplement                                                                 ,
IsArmenian                                                                           ,
IsHebrew                                                                             ,
IsArabic                                                                             ,
IsSyriac                                                                             ,
IsThaana                                                                             ,
IsDevanagari                                                                         ,
IsBengali                                                                            ,
IsGurmukhi                                                                           ,
IsGujarati                                                                           ,
IsOriya                                                                              ,
IsTamil                                                                              ,
IsTelugu                                                                             ,
IsKannada                                                                            ,
IsMalayalam                                                                          ,
IsSinhala                                                                            ,
IsThai                                                                               ,
IsLao                                                                                ,
IsTibetan                                                                            ,
IsMyanmar                                                                            ,
IsGeorgian                                                                           ,
IsHangulJamo                                                                         ,
IsEthiopic                                                                           ,
IsCherokee                                                                           ,
IsUnifiedCanadianAboriginalSyllabics                                                 ,
IsOgham                                                                              ,
IsRunic                                                                              ,
IsTagalog                                                                            ,
IsHanunoo                                                                            ,
IsBuhid                                                                              ,
IsTagbanwa                                                                           ,
IsKhmer                                                                              ,
IsMongolian                                                                          ,
IsLimbu                                                                              ,
IsTaiLe                                                                              ,
IsKhmerSymbols                                                                       ,
IsPhoneticExtensions                                                                 ,
IsLatinExtendedAdditional                                                            ,
IsGreekExtended                                                                      ,
IsGeneralPunctuation                                                                 ,
IsSuperscriptsandSubscripts                                                          ,
IsCurrencySymbols                                                                    ,
IsCombiningDiacriticalMarksforSymbols                                                ,
IsLetterlikeSymbols                                                                  ,
IsNumberForms                                                                        ,
IsArrows                                                                             ,
IsMathematicalOperators                                                              ,
IsMiscellaneousTechnical                                                             ,
IsControlPictures                                                                    ,
IsOpticalCharacterRecognition                                                        ,
IsEnclosedAlphanumerics                                                              ,
IsBoxDrawing                                                                         ,
IsBlockElements                                                                      ,
IsGeometricShapes                                                                    ,
IsMiscellaneousSymbols                                                               ,
IsDingbats                                                                           ,
IsMiscellaneousMathematicalSymbols_A                                                 ,
IsSupplementalArrows_A                                                               ,
IsBraillePatterns                                                                    ,
IsSupplementalArrows_B                                                               ,
IsMiscellaneousMathematicalSymbols_B                                                 ,
IsSupplementalMathematicalOperators                                                  ,
IsMiscellaneousSymbolsandArrows                                                      ,
IsCJKRadicalsSupplement                                                              ,
IsKangxiRadicals                                                                     ,
IsIdeographicDescriptionCharacters                                                   ,
IsCJKSymbolsandPunctuation                                                           ,
IsHiragana                                                                           ,
IsKatakana                                                                           ,
IsBopomofo                                                                           ,
IsHangulCompatibilityJamo                                                            ,
IsKanbun                                                                             ,
IsBopomofoExtended                                                                   ,
IsKatakanaPhoneticExtensions                                                         ,
IsEnclosedCJKLettersandMonths                                                        ,
IsCJKCompatibility                                                                   ,
IsCJKUnifiedIdeographsExtensionA                                                     ,
IsYijingHexagramSymbols                                                              ,
IsCJKUnifiedIdeographs                                                               ,
IsYiSyllables                                                                        ,
IsYiRadicals                                                                         ,
IsHangulSyllables                                                                    ,
IsHighSurrogates                                                                     ,
IsHighPrivateUseSurrogates                                                           ,
IsLowSurrogates                                                                      ,
IsPrivateUse                                                                         ,
IsCJKCompatibilityIdeographs                                                         ,
IsAlphabeticPresentationForms                                                        ,
IsArabicPresentationForms_A                                                          ,
IsVariationSelectors                                                                 ,
IsCombiningHalfMarks                                                                 ,
IsCJKCompatibilityForms                                                              ,
IsSmallFormVariants                                                                  ,
IsArabicPresentationForms_B                                                          ,
IsHalfwidthandFullwidthForms                                                         ,
IsSpecials                                                                           ,
            }
            #endregion


            public char PointStart { get; set; }
            public char PointEnd { get; set; }
            public String Name;
            public String BlockName;
            public const int TOTAL_COUNT = 105;
            protected LanguageClass(char Start, char End, String Tag)
            {
                PointStart = Start;
                PointEnd = End;
                BlockName = Tag;
                Name = Tag.Replace("Is", "");
            }

            /// <summary>
            /// 언어 단순 비교. 해당여부 확인후 결과 리턴
            /// </summary>
            public bool CheckLanguage(string data)
            {
                string param = @"\b\p{" + this.BlockName +"}+";
                return Regex.IsMatch(data, param);
            }

            public static LangType[] FindLanguage(string data)
            {
                List<LangType> temp = new List<LangType>();
                for (int i = 0; i < TOTAL_COUNT; i++)
                {
                    LangType test = (LangType)i;
                    if (Regex.IsMatch(data, @"\b\p{" + test.ToString().Replace("_","-") + "}+"))
                        temp.Add(test);
                }
                return temp.ToArray();
            }
        };




        static void Main(string[] args)
        {
            string aaa = "ภาษาไทย aaaa 가가가가 !!!!";
            bool result = LanguageClass.Thai.CheckLanguage(aaa);
            Console.WriteLine($"{aaa} = {result}");
            foreach (var i in LanguageClass.FindLanguage(aaa))
            {
                Console.WriteLine($"{aaa} = {i}");
            }

        }
    }
}