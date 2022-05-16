using Sfs2X.Entities.Data;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class BingoModel : CharacterEntityModel
    {
        public const int MaxMissionCountPerDay = 3;

        private BingoDataManager bingoRepo;

        //                                        [6,6]

        //       [1,5] [2,5] [3,5] [4,5] [5,5]    [6,5]
        //       [1,4] [2,4] [3,4] [4,4] [5,4]    [6,4]
        //       [1,3] [2,3] [3,3] [4,3] [5,3]    [6,3]
        //       [1,2] [2,2] [3,2] [4,2] [5,2]    [6,2]
        //       [1,1] [2,1] [3,1] [4,1] [5,1]    [6,1]
        //                                        
        //       [1,0] [2,0] [3,0] [4,0] [5,0]    [6,0]

        // [0,0] // 전체 보상
        private bool[,] bingoState = new bool[7, 7];
        
        private RemainTime curSeasonEndRemainTime;
        private RemainTime nextSeasonStartRemainTime;
        private RemainTime nextSeasonEndRemainTime;
        public int CurBingoSeasonGroup { get; private set; }
        public int NextBingoSeasonGroup { get; private set; }
        public RemainTime CurSeasonEndTime => curSeasonEndRemainTime;
        public RemainTime NextSeasonStartTime => nextSeasonStartRemainTime;

        public QuestInfo CurMission { get; private set; }
        public int CurClearedMissionCount { get; private set; }

        public event Action OnMissionStateChanged;

        public override void ResetData()
        {
            base.ResetData();

            ResetBingoState();
        }

        private void ResetBingoState()
        {
            CurBingoSeasonGroup = 0;
            curSeasonEndRemainTime = 0;
            NextBingoSeasonGroup = 0;
            nextSeasonStartRemainTime = 0;
            nextSeasonEndRemainTime = 0;
            CurMission = null;
            CurClearedMissionCount = 0;

            for (int x = 0; x < 7; ++x)
                for (int y = 0; y < 7; ++y)
                    bingoState[x, y] = false;
        }

        public override void Initialize(CharacterEntity entity)
        {
            base.Initialize(entity);

            bingoRepo = BingoDataManager.Instance;
        }

        public void Initialize(BingoSeasonPacket curSeasonPacket, BingoSeasonPacket nextSeasonPacket)
        {
            if (curSeasonPacket != null)
            {
                CurBingoSeasonGroup = curSeasonPacket.group;
                curSeasonEndRemainTime = curSeasonPacket.seasonEndTime;
            }
            else
            {
                CurBingoSeasonGroup = 0;
                curSeasonEndRemainTime = 0;
            }
            
            if (nextSeasonPacket != null)
            {
                NextBingoSeasonGroup = nextSeasonPacket.group;
                nextSeasonStartRemainTime = nextSeasonPacket.seasonStartTime;
                nextSeasonEndRemainTime = nextSeasonPacket.seasonEndTime;
            }
            else
            {
                NextBingoSeasonGroup = 0;
                nextSeasonStartRemainTime = 0;
                nextSeasonEndRemainTime = 0;
            }
        }

        public async Task<bool> ValidateSeasonInfo()
        {
            if (CurBingoSeasonGroup == 0 || curSeasonEndRemainTime == 0)
            {
                if (NextBingoSeasonGroup != 0 && nextSeasonStartRemainTime <= 0)
                {
                    ResetBingoState();

                    var response = await Protocol.REQUEST_BINGO_SEASONINFO.SendAsync();

                    if (response.isSuccess)
                    {
                        BingoSeasonPacket curSeasonPacket = null;
                        BingoSeasonPacket nextSeasonPacket = null;

                        if (response.ContainsKey("evb"))
                            curSeasonPacket = response.GetPacket<BingoSeasonPacket>("evb");

                        if (response.ContainsKey("nevb"))
                            nextSeasonPacket = response.GetPacket<BingoSeasonPacket>("nevb");

                        Initialize(curSeasonPacket, nextSeasonPacket);

                        if (response.ContainsKey("bg"))
                            Initialize(response.GetPacket<BingoStatePacket>("bg"));

                        if (curSeasonPacket == null)
                            return false;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public void Initialize(BingoStatePacket packet)
        {
            for (int x = 0; x < 5; ++x)
                for (int y = 0; y < 5; ++y)
                    bingoState[1 + x, 1 + y] = packet.GetIsChecked(x, y);

            for (int x = 0; x < 7; ++x)
                bingoState[x, 0] = packet.GetIsRewarded(x, 0);
            for (int y = 1; y < 7; ++y)
                bingoState[6, y] = packet.GetIsRewarded(6, y);

            CurClearedMissionCount = packet.questClearCount;
            SetMission(packet.curQuestNumber, packet.curQuestProgress);
            if (CurClearedMissionCount == MaxMissionCountPerDay)
                CurMission.SetReceived(true);

            Debug.Log($"[BingoModel] {packet.GetDump()}");
        }

        public override void AddEvent(UnitEntityType type)
        {
            Quest.OnProgress += OnProgress;

            if (type == UnitEntityType.Player)
            {
                Protocol.RESULT_CHAR_DAILY_CALC.AddEvent(OnReceiveCharDailyCalc);
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            Quest.OnProgress -= OnProgress;

            if (type == UnitEntityType.Player)
            {
                Protocol.RESULT_CHAR_DAILY_CALC.RemoveEvent(OnReceiveCharDailyCalc);
            }
        }

        public bool IsChecked(BingoData bingoData)
        {
            return bingoState[bingoData.bingo_x, bingoData.bingo_y];
        }

        public bool IsRewarded(BingoData bingoData)
        {
            return bingoState[bingoData.bingo_x, bingoData.bingo_y];
        }

        private void OnProgress(QuestType type, int conditionValue, int questValue)
        {
            if (CurMission == null)
                return;

            // 진행중인 퀘스트가 아닐 경우
            if (CurMission.CompleteType == QuestInfo.QuestCompleteType.ReceivedReward || CurMission.CompleteType == QuestInfo.QuestCompleteType.StandByReward)
                return;

            // 퀘스트 타입이 동일하지 않음
            if (CurMission.QuestType != type)
                return;

            // 퀘스트 조건이 동일하지 않음
            if (CurMission.ConditionValue != conditionValue)
                return;

            if (type.IsMaxCondition())
                CurMission.MaxCurrentValue(questValue);
            else
                CurMission.PlusCurrentValue(questValue);

            OnMissionStateChanged?.Invoke();
        }

        public async Task<bool> RequestGetReward(BingoData bingoData)
        {
            if (curSeasonEndRemainTime == 0)
            {
                UI.ShowToastPopup(LocalizeKey._40100.ToText());
                return false;
            }

            SFSObject par = SFSObject.NewInstance();
            par.PutByte("1", (byte)bingoData.bingo_x);
            par.PutByte("2", (byte)bingoData.bingo_y);

            var response = await Protocol.REQUEST_BINGO_CLEARLINE.SendAsync(par);

            if (response.isSuccess)
            {
                bingoState[bingoData.bingo_x, bingoData.bingo_y] = true;
                if (response.ContainsKey("cud"))
                {
                    var cud = response.GetPacket<CharUpdateData>("cud");
                    Notify(cud);
                    UI.RewardInfo(cud.rewards);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> RequestCheckBoard(BingoData bingoData)
        {
            if (curSeasonEndRemainTime == 0)
            {
                UI.ShowToastPopup(LocalizeKey._40100.ToText());
                return false;
            }

            int collectID = bingoData.collect_id;
            var item = Entity.Inventory.itemList.Find(v => v.ItemId == collectID);

            if (item == null)
                return false;

            SFSObject par = SFSObject.NewInstance();
            par.PutByte("1", (byte)bingoData.bingo_x);
            par.PutByte("2", (byte)bingoData.bingo_y);
            par.PutLong("3", item.ItemNo);

            var response = await Protocol.REQUEST_BINGO_INPUTITEM.SendAsync(par);

            if (response.isSuccess)
            {
                bingoState[bingoData.bingo_x, bingoData.bingo_y] = true;
                if (response.ContainsKey("cud"))
                {
                    var cud = response.GetPacket<CharUpdateData>("cud");
                    Notify(cud);
                    UI.RewardInfo(cud.rewards);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> RequestClearMission()
        {
            var response = await Protocol.REQUEST_BINGO_QUESTCLEAR.SendAsync();

            if (response.isSuccess)
            {
                CurMission.SetReceived(true);

                if (response.ContainsKey("cud"))
                {
                    var cud = response.GetPacket<CharUpdateData>("cud");
                    Notify(cud);
                    UI.RewardInfo(cud.rewards);
                }

                CurClearedMissionCount = response.GetByte("1");

                if (response.ContainsKey("2"))
                {
                    int nextQuest = response.GetInt("2");
                    if (nextQuest != 0 && CurClearedMissionCount < MaxMissionCountPerDay)
                        SetMission(nextQuest, response.GetInt("3"));
                }

                OnMissionStateChanged?.Invoke();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetMission(int missionID, int progress)
        {
            var questData = QuestDataManager.Instance.Get(missionID);
            CurMission = new QuestInfo();
            CurMission.SetData(questData);
            CurMission.SetCurrentValue(progress);
        }

        private void OnReceiveCharDailyCalc(Response response)
        {
            if (response.isSuccess)
            {
                if (response.ContainsKey("2"))
                {
                    CurClearedMissionCount = 0;
                    SetMission(response.GetInt("2"), 0);
                    OnMissionStateChanged?.Invoke();
                }
                else
                {
                    var bingoUI = UI.GetUI<UIBingo>();
                    if (bingoUI != null && bingoUI.IsVisible)
                    {
                        UI.Close<UIBingo>();
                        UI.ShowToastPopup(LocalizeKey._40100.ToText());
                    }
                }
            }
        }

        /// <summary>
        /// 빙고 퀘스트 보상 받을 수 있는지 여부
        /// </summary>
        /// <returns></returns>
        public bool IsBingoQuestStandByReward()
        {
            if (CurMission == null)
                return false;

            return CurMission.CompleteType == QuestInfo.QuestCompleteType.StandByReward;
        }
    }
}
