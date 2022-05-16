using UnityEditor;

namespace Ragnarok
{
    public static class ServerIssueMenuItems
    {
        private const string BASE_PATH = "라그나로크 서버/Issue/";
        private const string ENTER_SECOND_SERVER_PATH = BASE_PATH + "두번째 서버 입장";

        [MenuItem(ENTER_SECOND_SERVER_PATH)]
        private static void ToggleEnterSecondServer()
        {
            ServerIssue.IsEnterSecondServer = !ServerIssue.IsEnterSecondServer;
        }

        [MenuItem(ENTER_SECOND_SERVER_PATH, validate = true)]
        private static bool ToggleEnterSecondServerValidate()
        {
            Menu.SetChecked(ENTER_SECOND_SERVER_PATH, ServerIssue.IsEnterSecondServer);
            return true;
        }
    }
}