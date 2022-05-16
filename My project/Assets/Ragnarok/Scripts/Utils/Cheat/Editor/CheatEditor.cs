using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public static class CheatEditor
    {
        private const string BASE_PATH                  = "라그나로크/치트/";
        private const string ALL_OPEN_CONTENT_PATH      = BASE_PATH + "모든 컨텐츠 오픈";
        private const string FORCE_JOIN_FREE_FIGHT_PATH = BASE_PATH + "난전 강제입장";
        private const string USE_TUTORIAL_PATH          = BASE_PATH + "튜토리얼";
        private const string IS_SNOW_PATH               = BASE_PATH + "눈 효과(멀티미로대기실)";
        private const string IS_MONSTER_LEVEL_PATH      = BASE_PATH + "몬스터 레벨 보기";
        private const string IS_GLOBAL_PATH             = BASE_PATH + "프로젝트(글로벌)";
        private const string IS_FACEBOOK_PATH           = BASE_PATH + "페이스북 친구";
        private const string IS_ONBUFF_PATH             = BASE_PATH + "온버프";

        [MenuItem(BASE_PATH + CheatRequestWindow.TITLE, priority = 9000)]
        private static void ShowWindow()
        {
            EditorWindow window = EditorWindow.GetWindow<CheatRequestWindow>(title: CheatRequestWindow.TITLE, focus: true);
            window.minSize = new Vector2(480f, 520f);
            window.Focus();
            window.Repaint();
            window.Show();
        }

        [MenuItem(ALL_OPEN_CONTENT_PATH)]
        private static void ToggleAllOpenContent()
        {
            Cheat.All_OPEN_CONTENT = !Cheat.All_OPEN_CONTENT;
        }

        [MenuItem(ALL_OPEN_CONTENT_PATH, validate = true)]
        private static bool ToggleAllOpenContentValidate()
        {
            Menu.SetChecked(ALL_OPEN_CONTENT_PATH, Cheat.All_OPEN_CONTENT);
            return true;
        }

        [MenuItem(FORCE_JOIN_FREE_FIGHT_PATH)]
        private static void ToggleForceJoinFreeFight()
        {
            Cheat.FORCE_JOIN_FREE_FIGHT = !Cheat.FORCE_JOIN_FREE_FIGHT;
        }

        [MenuItem(FORCE_JOIN_FREE_FIGHT_PATH, validate = true)]
        private static bool ToggleForceJoinFreeFightValidate()
        {
            Menu.SetChecked(FORCE_JOIN_FREE_FIGHT_PATH, Cheat.FORCE_JOIN_FREE_FIGHT);
            return true;
        }

        [MenuItem(USE_TUTORIAL_PATH)]
        private static void ToggleUseTutoril()
        {
            Cheat.USE_TUTORIAL = !Cheat.USE_TUTORIAL;
        }

        [MenuItem(USE_TUTORIAL_PATH, validate = true)]
        private static bool ToggleUseTutorilValidate()
        {
            Menu.SetChecked(USE_TUTORIAL_PATH, Cheat.USE_TUTORIAL);
            return true;
        }

        [MenuItem(IS_SNOW_PATH)]
        private static void ToggleIsSnow()
        {
            Cheat.IS_SNOW = !Cheat.IS_SNOW;
        }

        [MenuItem(IS_SNOW_PATH, validate = true)]
        private static bool ToggleIsSnowValidate()
        {
            Menu.SetChecked(IS_SNOW_PATH, Cheat.IS_SNOW);
            return true;
        }

        [MenuItem(IS_MONSTER_LEVEL_PATH)]
        private static void ToggleMonsterLevel()
        {
            Cheat.IS_MONSTER_LEVEL = !Cheat.IS_MONSTER_LEVEL;
        }

        [MenuItem(IS_MONSTER_LEVEL_PATH, validate = true)]
        private static bool ToggleMonsterLevelValidate()
        {
            Menu.SetChecked(IS_MONSTER_LEVEL_PATH, Cheat.IS_MONSTER_LEVEL);
            return true;
        }

        [MenuItem(IS_GLOBAL_PATH)]
        private static void ToggleIsGlobal()
        {
            Cheat.IS_GLOBAL = !Cheat.IS_GLOBAL;
        }

        [MenuItem(IS_GLOBAL_PATH, validate = true)]
        private static bool ToggleIsGlobalValidate()
        {
            Menu.SetChecked(IS_GLOBAL_PATH, Cheat.IS_GLOBAL);
            return true;
        }

        [MenuItem(IS_FACEBOOK_PATH)]
        private static void ToggleIsFacebook()
        {
            Cheat.IsFacebook = !Cheat.IsFacebook;
        }

        [MenuItem(IS_FACEBOOK_PATH, validate = true)]
        private static bool ToggleIsFacebookValidate()
        {
            Menu.SetChecked(IS_FACEBOOK_PATH, Cheat.IsFacebook);
            return true;
        }

        [MenuItem(IS_ONBUFF_PATH)]
        private static void ToggleIsOnBuff()
        {
            Cheat.IS_OnBuff = !Cheat.IS_OnBuff;
        }

        [MenuItem(IS_ONBUFF_PATH, validate = true)]
        private static bool ToggleIsOnBuffValidate()
        {
            Menu.SetChecked(IS_ONBUFF_PATH, Cheat.IS_OnBuff);
            return true;
        }
    }
}