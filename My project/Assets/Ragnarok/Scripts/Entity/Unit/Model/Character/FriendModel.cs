using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// 친구 정보
    /// </summary>
    public class FriendModel : CharacterEntityModel
    {
        private List<ObscuredString> inviteList;
        private ObscuredInt inviteCount;

        public System.Action onRefresh;

        public FriendModel()
        {
            inviteList = new List<ObscuredString>();
        }

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        internal void Initialize(FriendInvitePacket invitePacket)
        {
            inviteList = invitePacket.friendAry.ToList()
                .ConvertAll(x => (ObscuredString)x);

            inviteCount = invitePacket.InviteCount;
        }

        /// <summary>
        /// 초대한 친구 리스트
        /// </summary>
        private List<string> GetInviteFriendList()
        {
            return inviteList.ConvertAll(x => x.ToString());
        }

        /// <summary>
        /// 초대한 친구 리스트에 추가
        /// </summary>
        /// <param name="InitFirst">리스트 초기화가 필요할 때..</param>
        private void AddInviteFriend(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                // 리스트에 없으면 추가
                if (!inviteList.Contains(list[i]))
                {
                    inviteList.Add(list[i]);
                }
            }
        }

        /// <summary>
        /// 데이터 갱신
        /// </summary>
        /// <param name="ids">완료한 친구ID는 리스트에서 제거</param>
        /// <param name="count">초대완료 카운트는 서버에서 받은 값으로 갱신</param>
        private void UpdateData(string[] ids, int count)
        {
            // 완료 친구 리스트에서 제거
            for (int i = 0; i < ids.Length; i++)
            {
                inviteList.Remove(ids[i]);
            }

            // 초대완료 카운트 갱신
            inviteCount = count;
        }

        public int GetInviteCount()
        {
            return inviteCount;
        }

        /// <summary>
        /// 초대 리스트에 없는 값만 리스트 추가 요청
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task RequestInviteFriend(string[] ids)
        {
            var list = GetInviteFriendList();
            var sendList = new List<string>();

            for (int i = 0; i < ids.Length; i++)
            {
                // 초대 리스트에 없는 값만 추가
                if (!list.Contains(ids[i]))
                {
                    sendList.Add(ids[i]);
                }
            }

            // 초대한 ID가 없으면 보내지 않음.
            if (sendList.Count > 0)
            {
                var sfs = Protocol.NewInstance();
                sfs.PutUtfStringArray("1", sendList.ToArray());
                Response response = await Protocol.REQUEST_INVITE_FB_IDS.SendAsync(sfs);

                if (!response.isSuccess)
                {
                    response.ShowResultCode();
                    return;
                }

                // 초대한 친구 리스트에 추가
                AddInviteFriend(sendList);
            }

            // 페북 메시지가 발송되었으면 표시(중복이면 게임서버에는 보내지 않는경우도 있음
            if (ids.Length > 0)
            {
                // 친구 초대를 보냈습니다.
                UI.ConfirmPopup(LocalizeKey._6610); // 초대 메시지를 보냈습니다.
            }
        }

        /// <summary>
        /// 초대 리스트와 비교해서 동일한 값이 있으면 완료카운트 증가 요청
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task RequestAcceptFriend(string[] ids)
        {
            var list = GetInviteFriendList();
            var sendList = new List<string>();

            for (int i = 0; i < ids.Length; i++)
            {
                // 초대 리스트의 값들 중에서 일치하는 값만
                if (list.Contains(ids[i]))
                {
                    sendList.Add(ids[i]);
                }
            }

            // 초대한 ID가 없어도 보냄.
            var sfs = Protocol.NewInstance();
            sfs.PutUtfStringArray("1", sendList.ToArray());
            Response response = await Protocol.REQUEST_ACCEPT_FB_IDS.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            var acceptCnt = response.GetInt("1");
            var hasReward = response.GetByte("2") == 1;

            UpdateData(sendList.ToArray(), acceptCnt);

            if (hasReward)
            {
                onRefresh?.Invoke(); // 완료아이콘 갱신

                // 보상이 메일로 지급되었다는 메세지 표시
                UI.ConfirmPopup(LocalizeKey._7605); // 보상은 우편함으로 지급됩니다.
            }
        }
    }
}