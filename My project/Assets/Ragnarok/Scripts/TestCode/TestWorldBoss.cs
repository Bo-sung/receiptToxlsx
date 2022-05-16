#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public class TestWorldBoss : TestCode
    {
        [TextArea(5, 5)]
        public string Description =
            "숫자키 1 : 월드보스 전체목록 요청\n" +
            "숫자키 2 : 오픈된 월드보스 목록 요청\n" +
            "숫자키 3 : 월드보스 입장\n" +
            "숫자키 4 : 월드보스 퇴장\n" +
            "스페이스바 : 월드보스 공격";

        [Tooltip("월드보스 데미지 범위")]
        [Range(1, 1000)]
        [SerializeField] int damage = 100;

        [Tooltip("입장할 월드보스 ID")]
        [SerializeField] int worldBossId = 1;

        protected override void Awake()
        {
            base.Awake();
            Protocol.RECEIVE_WORLD_BOSS_RANK_INFO.AddEvent(OnRequestWorldBossRankInfo);
            Protocol.RECEIVE_WORLD_BOSS_CLOSE.AddEvent(OnRequestWorldBossClose);
        }

        private void OnDestroy()
        {
            Protocol.RECEIVE_WORLD_BOSS_RANK_INFO.RemoveEvent(OnRequestWorldBossRankInfo);
            Protocol.RECEIVE_WORLD_BOSS_CLOSE.RemoveEvent(OnRequestWorldBossClose);
        }

        private void OnRequestWorldBossRankInfo(Response response)
        {

        }

        private void OnRequestWorldBossClose(Response response)
        {
            Protocol.REQUEST_WORLD_BOSS_EXIT.SendAsync().WrapNetworkErrors();
        }

        protected override void OnMainTest()
        {
            // 월드 보스 공격
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", Random.Range(1, damage));
            Protocol.REQUEST_WORLD_BOSS_ATTACK.SendAsync(sfs).WrapNetworkErrors();
        }

        protected override void OnTest1()
        {
            // 월드 보스 목록 전체 호출
            var sfs = Protocol.NewInstance();
            sfs.PutBool("1", true);
            Protocol.REQUEST_WORLD_BOSS_LIST.SendAsync(sfs).WrapNetworkErrors();
        }

        protected override void OnTest2()
        {
            // 월드 보스 목록 오픈중인
            var sfs = Protocol.NewInstance();
            sfs.PutBool("1", false);
            Protocol.REQUEST_WORLD_BOSS_LIST.SendAsync(sfs).WrapNetworkErrors();
        }

        protected override void OnTest3()
        {
            // 월드 보스 입장
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", worldBossId);
            Protocol.REQUEST_WORLD_BOSS_ROOM_JOIN.SendAsync(sfs).WrapNetworkErrors();
        }

        protected override void OnTest4()
        {
            // 월드 보스 퇴장
            Protocol.REQUEST_WORLD_BOSS_EXIT.SendAsync().WrapNetworkErrors();
        }
    }
}
#endif
