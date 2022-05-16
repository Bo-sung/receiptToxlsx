using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 인증서버 정보
    /// </summary>
    [Serializable]
    public class ServerStatusJson
    {
        [Serializable]
        public class ServerStaus
        {
            public int group_id; // seqID
            public int complex; // [1] 원활 [2] 혼잡 [3] 매우혼잡
            public int state; // [0] 점검중 [1] 내부오픈 [2] 외부오픈
            public int create_flg; // [0] 캐릭터 생성 불가 [1] 캐릭터 생성 가능

            /// <summary>
            /// 서버 혼잡도 이미지 이름
            /// </summary>
            /// <returns></returns>
            public string GetComplexName()
            {
                // 점검
                if (state == 0)
                    return "Ui_Common_Icon_SeverState_04";

                // 원활
                if (complex == 1)
                    return "Ui_Common_Icon_SeverState_01";

                // 혼잡
                if (complex == 2)
                    return "Ui_Common_Icon_SeverState_02";

                // 매우혼잡
                if (complex == 3)
                    return "Ui_Common_Icon_SeverState_03";

                return string.Empty;
            }
        }

        [Serializable]
        public class IntroStaus // 인트로 이벤트 웹뷰
        {
            public int s; // seqID
            [SerializeField] string st;
            [SerializeField] string et;
            public DateTime startTime => Convert.ToDateTime(st); // 시작시간
            public DateTime endTime => Convert.ToDateTime(et); // 종료시간
            public string url;
            public int u; // [0] 비사용, [1] 사용
        }

        [Serializable]
        public class UpdateNotice  // 선택 업데이트
        {
            public int @as; // 앱스토어 버전
        }

        public int ret;
        public string msg;
        public List<ServerStaus> servers;
        public List<IntroStaus> intro;
        public List<UpdateNotice> update_notice;

        public bool IsServerStaus(int _group_id)
        {
            foreach (var item in servers)
            {
                if (item.group_id.Equals(_group_id))
                    return true;
            }
            return false;
        }

        public bool IsUpdateVesion()
        {
            if (update_notice.Count == 0)
                return false;

            if (update_notice[0].@as != BuildSettings.Instance.AppVersion)
                return true;

            return false;
        }

        public ServerStaus GetServer(int group_id)
        {
            return servers.Find(x => x.group_id == group_id);
        }
    }
}