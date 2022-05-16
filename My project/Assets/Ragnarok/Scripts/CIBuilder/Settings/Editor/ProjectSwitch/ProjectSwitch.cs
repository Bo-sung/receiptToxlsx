using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public static class ProjectSwitch
    {
        private const string TITLE = "프로젝트 변경";
        private const string MENU_NAME = Builder.BaseMenu + Builder.BuildMenu + TITLE;
        private const string ASSET_PATH = "Assets/Ragnarok/Scripts/CIBuilder/Settings/Editor/ProjectSwitch/Settings/{0}/Settings.asset";

        [MenuItem(MENU_NAME, priority = 1001)]
        private static void EmptySpace() { }

        [MenuItem(MENU_NAME + "/1. 동남아시아")]
        private static void Switch1()
        {
            Switch("SoutheastAsia");
        }

        [MenuItem(MENU_NAME + "/2. 글로벌")]
        private static void Switch2()
        {
            Switch("Global");
        }

        [MenuItem(MENU_NAME + "/3. 페이스북")]
        private static void Switch3()
        {
            Switch("Facebook");
        }

        [MenuItem(MENU_NAME + "/4. 페이스북_글로벌")]
        private static void Switch4()
        {
            Switch("Facebook_Global");
        }

        [MenuItem(MENU_NAME + "/5. 동남아시아_NFT")]
        private static void Switch5()
        {
            Switch("NFT");
        }

        public static bool Switch(string production)
        {
            ProductSettings settings = AssetDatabase.LoadAssetAtPath<ProductSettings>(string.Format(ASSET_PATH, production));
            if (settings == null)
            {
                Debug.LogError($"Setting 값 음슴: {nameof(production)} = {production}");
                return false;
            }

            settings.Switch();
            return true;
        }
    }
}