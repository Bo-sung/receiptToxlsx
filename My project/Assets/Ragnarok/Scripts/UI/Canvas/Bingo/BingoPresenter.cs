using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class BingoPresenter : ViewPresenter
    {
        private readonly InventoryModel inventoryModel;
        private readonly BingoModel bingoModel;
        private readonly BingoDataManager bingoRepo;

        private (int, int, int, int)[] lineRewardDataCoords = new (int, int, int, int)[]
        {
            (1, 0, 0, 1),
            (2, 0, 0, 1),
            (3, 0, 0, 1),
            (4, 0, 0, 1),
            (5, 0, 0, 1),
            (6, 0, -1, 1),
            (6, 1, -1, 0),
            (6, 2, -1, 0),
            (6, 3, -1, 0),
            (6, 4, -1, 0),
            (6, 5, -1, 0),
            (6, 6, -1, -1)
        };

        private UIBingo view;

        private int curShowingSeasonGroup;

        private int curCheckedCount = 0;
        private bool isThereCheckable = false;
        private bool isThereRewardable = false;
        private BingoStateDecoratedData[,] buffer = new BingoStateDecoratedData[7, 7];

        public BingoPresenter(UIBingo view)
        {
            inventoryModel = Entity.player.Inventory;
            bingoModel = Entity.player.Bingo;
            bingoRepo = BingoDataManager.Instance;
            this.view = view;
            curShowingSeasonGroup = -1;
        }

        public override void AddEvent()
        {
            bingoModel.OnMissionStateChanged += OnMissionStateChanged;
        }

        public override void RemoveEvent()
        {
            bingoModel.OnMissionStateChanged -= OnMissionStateChanged;
        }

        public async void OnShow()
        {
            if (await bingoModel.ValidateSeasonInfo())
            {
                view.ShowView();
                UpdateBoardDataBuffer();
                
                view.SetFinalRewardPanel(buffer[0, 0], curCheckedCount);
                view.SetBingoBoard(ReadBoardFromBuffer());
                view.SetRemainTime(bingoModel.CurSeasonEndTime);
                view.SetMission(bingoModel.CurMission, BingoModel.MaxMissionCountPerDay - bingoModel.CurClearedMissionCount);
                SetPoringText();
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._40101.ToText());
                UI.Close<UIBingo>();
                return;
            }
        }

        private void InitBoard()
        {
            curShowingSeasonGroup = bingoModel.CurBingoSeasonGroup;
            var bingoDatas = bingoRepo.Get(bingoModel.CurBingoSeasonGroup);

            foreach (var each in bingoDatas)
                buffer[each.bingo_x, each.bingo_y] = new BingoStateDecoratedData()
                {
                    bingoData = each,
                    state = BingoStateDecoratedData.State.Normal
                };
        }

        private void UpdateBoardDataBuffer()
        {
            if (curShowingSeasonGroup != bingoModel.CurBingoSeasonGroup)
                InitBoard();

            curCheckedCount = 0;
            isThereCheckable = false;
            isThereRewardable = false;

            bool allChecked = true;

            for (int x = 1; x <= 5; ++x)
            {
                for (int y = 1; y <= 5; ++y)
                {
                    buffer[x, y].isInRewardableLine = false;

                    if (bingoModel.IsChecked(buffer[x, y].bingoData))
                    {
                        buffer[x, y].state = BingoStateDecoratedData.State.Checked;
                        ++curCheckedCount;
                    }
                    else
                    {
                        allChecked = false;
                        int count = inventoryModel.GetItemCount(buffer[x, y].bingoData.collect_id);
                        buffer[x, y].state = count > 0 ? BingoStateDecoratedData.State.Checkable : BingoStateDecoratedData.State.Normal;

                        if (count > 0)
                            isThereCheckable = true;
                    }
                }
            }

            foreach (var coord in lineRewardDataCoords)
            {
                int x = coord.Item1;
                int y = coord.Item2;
                int dx = coord.Item3;
                int dy = coord.Item4;

                var each = buffer[x, y];

                each.isInRewardableLine = false;

                if (bingoModel.IsRewarded(each.bingoData))
                {
                    each.state = BingoStateDecoratedData.State.Rewarded;
                }
                else
                {
                    bool canGetReward = true;

                    for (int i = 1; i <= 5; ++i)
                    {
                        int bx = x + dx * i;
                        int by = y + dy * i;

                        if (buffer[bx, by].state != BingoStateDecoratedData.State.Checked)
                        {
                            canGetReward = false;
                            break;
                        }
                    }

                    each.state = canGetReward ? BingoStateDecoratedData.State.Rewardable : BingoStateDecoratedData.State.Normal;

                    if (canGetReward)
                    {
                        each.isInRewardableLine = true;

                        isThereRewardable = true;

                        for (int i = 1; i <= 5; ++i)
                        {
                            int bx = x + dx * i;
                            int by = y + dy * i;

                            buffer[bx, by].isInRewardableLine = true;
                        }
                    }
                }
            }

            if (bingoModel.IsRewarded(buffer[0, 0].bingoData))
            {
                buffer[0, 0].state = BingoStateDecoratedData.State.Rewarded;
            }
            else
            {
                buffer[0, 0].state = allChecked ? BingoStateDecoratedData.State.Rewardable : BingoStateDecoratedData.State.Normal;
                if (allChecked)
                    isThereRewardable = true;
            }
        }

        private IEnumerable<BingoStateDecoratedData> ReadBoardFromBuffer()
        {
            // 그리드
            for (int x = 1; x <= 5; ++x)
                for (int y = 1; y <= 5; ++y)
                    yield return buffer[x, y];

            // 보상
            for (int x = 1; x <= 5; ++x)
                yield return buffer[x, 0];
            for (int y = 1; y <= 5; ++y)
                yield return buffer[6, y];
            yield return buffer[6, 0];
            yield return buffer[6, 6];
        }

        private void SetPoringText()
        {
            if (isThereRewardable)
                view.SetPoringText(UIBingo.PoringTextType.GetReward);
            else if (isThereCheckable)
                view.SetPoringText(UIBingo.PoringTextType.DoCheck);
            else if (bingoModel.CurClearedMissionCount == BingoModel.MaxMissionCountPerDay)
                view.SetPoringText(UIBingo.PoringTextType.AllMissionCleared);
            else
                view.SetPoringText(UIBingo.PoringTextType.None);
        }

        public void ViewEventHandler(UIBingo.Event eventType, object _data)
        {
            if (eventType == UIBingo.Event.OnClickCheck)
            {
                BingoStateDecoratedData data = _data as BingoStateDecoratedData;
                if (data.state == BingoStateDecoratedData.State.Checkable)
                    RequestCheck(data.bingoData);
            }
            else if (eventType == UIBingo.Event.OnClickLineReward)
            {
                BingoStateDecoratedData data = _data as BingoStateDecoratedData;
                if (data.state == BingoStateDecoratedData.State.Rewardable)
                    RequestReward(data.bingoData);
            }
            else if (eventType == UIBingo.Event.OnClickMissionButton)
            {
                if (bingoModel.CurMission.CompleteType == QuestInfo.QuestCompleteType.StandByReward)
                {
                    RequestClearMission();
                }
                else if (bingoModel.CurMission.CompleteType == QuestInfo.QuestCompleteType.InProgress)
                {
                    UI.Close<UIBingo>();
                    bingoModel.CurMission.GoShortCut();
                }
            }
            else if (eventType == UIBingo.Event.OnClickGetFinalReward)
            {
                if (buffer[0, 0].state == BingoStateDecoratedData.State.Rewardable)
                    RequestReward(buffer[0, 0].bingoData);
            }
            else if (eventType == UIBingo.Event.OnClickClose)
            {
                UI.Close<UIBingo>();
            }
        }

        private async void RequestReward(BingoData bingoData)
        {
            if (await bingoModel.RequestGetReward(bingoData))
            {
                UpdateBoardDataBuffer();
                view.SetFinalRewardPanel(buffer[0, 0], curCheckedCount);
                view.RefreshBoard();
                SetPoringText();
            }
            else
            {
                UI.Close<UIBingo>();
            }
        }

        public void Ref()
        {
            UpdateBoardDataBuffer();
            view.SetFinalRewardPanel(buffer[0, 0], curCheckedCount);
            view.RefreshBoard();
            SetPoringText();
        }

        private async void RequestCheck(BingoData bingoData)
        {
            if (await bingoModel.RequestCheckBoard(bingoData))
            {
                UpdateBoardDataBuffer();
                view.SetFinalRewardPanel(buffer[0, 0], curCheckedCount);
                view.RefreshBoard();
                SetPoringText();
            }
            else
            {
                UI.Close<UIBingo>();
            }
        }

        private async void RequestClearMission()
        {
            if (await bingoModel.RequestClearMission())
            {
                view.SetMission(bingoModel.CurMission, BingoModel.MaxMissionCountPerDay - bingoModel.CurClearedMissionCount);
                UpdateBoardDataBuffer();
                view.SetFinalRewardPanel(buffer[0, 0], curCheckedCount);
                view.RefreshBoard();
                SetPoringText();
            }
            else
            {
                UI.Close<UIBingo>();
            }
        }

        private void OnMissionStateChanged()
        {
            view.SetMission(bingoModel.CurMission, BingoModel.MaxMissionCountPerDay - bingoModel.CurClearedMissionCount);
            SetPoringText();
        }
    }
}
