using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// 리그 정보
    /// </summary>
    public class LeagueModel : CharacterEntityModel
    {
        /// <summary>
        /// 리그전 티켓
        /// </summary>
        private ObscuredInt leagueFreeTicket, leagueEntryCount;

        /// <summary>
        /// 현 시즌 점수
        /// </summary>
        private ObscuredInt seasonScore;

        /// <summary>
        /// 싱글 점수
        /// </summary>
        private ObscuredInt singleScore;

        /// <summary>
        /// 남아있는 리그전 입장 티켓 수
        /// </summary>
        public int LeagueFreeTicket => leagueFreeTicket;

        /// <summary>
        /// 현 시즌 점수
        /// </summary>
        public int SeasonScore => seasonScore;

        /// <summary>
        /// 싱글 점수
        /// </summary>
        public int SingleScore => singleScore;

        /// <summary>
        /// 대전 싱글모드 여부
        /// </summary>
        public bool IsSingle { get; private set; }

        /// <summary>
        /// 티켓 수량 변경 이벤트
        /// </summary>
        public event System.Action OnUpdateTicket;

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        internal void Initialize(CharacterPacket characterPacket)
        {
            leagueFreeTicket = characterPacket.dayPveTicket;
            leagueEntryCount = characterPacket.dayPveCount;
        }

        /// <summary>
        /// 무료 티켓 세팅
        /// </summary>
        internal void UpdateData(int? leagueFreeTicket, int? leagueEntryCount)
        {
            bool isDirty = false;

            if (this.leagueFreeTicket.Replace(leagueFreeTicket))
                isDirty = true;

            if (this.leagueEntryCount.Replace(leagueEntryCount))
                isDirty = true;

            if (isDirty)
                OnUpdateTicket?.Invoke();
        }

        /// <summary>
        /// 현 시즌 점수 저장
        /// </summary>
        public void SetCurrentScore(int score)
        {
            seasonScore = score;
        }

        /// <summary>
        /// 싱글 점수 저장
        /// </summary>
        public void SetCurrentSingleScore(int score)
        {
            singleScore = score;
        }

        /// <summary>
        /// 대전 모드 세팅
        /// </summary>
        public void SetSelectMode(bool isSingle)
        {
            IsSingle = isSingle;
        }

        /// <summary>
        /// PVE 시작
        /// </summary>
        public async Task<Response> RequestPveStart()
        {
            var sfs = Protocol.NewInstance();
            sfs.PutBool("1", IsSingle);
            var response = await Protocol.REQUEST_PVE_START.SendAsync(sfs);
            if (response.isSuccess)
            {
                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);
                }

                Entity.ResetSkillCooldown(); // 쿨타임 초기화

                // 퀘스트 처리
                Quest.QuestProgress(QuestType.PVE_COUNT); // 대전 입장 횟수
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }
    }
}