#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public class TestMazeRank : TestCode
    {
        [TextArea(1, 1)]
        public string Description =
            "스페이스바 : 미로맵 클리어 랭킹 요청";

        [Tooltip("랭킹 페이지")]
        [SerializeField] int page = 1;

        [Tooltip("미로맵 ID")]
        [SerializeField] int mapId = 1;

        protected override void OnMainTest()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", page);
            sfs.PutInt("2", mapId);

            Protocol.REQUEST_MAZE_RANK_LIST.SendAsync(sfs).WrapNetworkErrors();
        }
    }
}
#endif
