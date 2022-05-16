#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public class TestTamingMaze : TestCode
    {
        [TextArea(2, 2)]
        public string Description =
            "숫자키 1 : 테이밍 미로 입장\n" +
            "숫자키 2 : 테이밍 미로 퇴장";

        protected override void OnMainTest()
        {
        }

        protected override void OnTest1()
        {
            Protocol.REQUEST_TAMING_MAZE_ROOM_JOIN.SendAsync().WrapNetworkErrors();
        }

        protected override void OnTest2()
        {
            Protocol.REQUEST_TAMING_MAZE_ROOM_EXIT.SendAsync().WrapNetworkErrors();
        }
    }
}
#endif
