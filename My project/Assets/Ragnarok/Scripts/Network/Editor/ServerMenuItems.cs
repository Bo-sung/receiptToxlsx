using UnityEditor;

namespace Ragnarok
{
    public static class ServerMenuItems
    {
        [MenuItem("라그나로크 서버/임시 호출/셰어 강제 종료", priority = 9999)]
        private static void RequestShareForceQuit()
        {
            Entity.player.Sharing.RequestShareForceQuit().WrapNetworkErrors();
        }

        [MenuItem("라그나로크 서버/임시 호출/셰어중인 캐릭터 전체 해제", priority = 10000)]
        private static void RequestShareRelease()
        {
            Entity.player.Sharing.RequestShareCharacterRelease(isSave: false).WrapNetworkErrors();
        }
    }
}