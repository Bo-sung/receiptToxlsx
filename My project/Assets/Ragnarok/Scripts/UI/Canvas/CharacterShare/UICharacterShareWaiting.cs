using MEC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICharacterShareWaiting : UICanvas
    {
        /// <summary>
        /// <see cref="SharingRewardPacket"/>
        /// </summary>
        public interface ISharingRewardData
        {
            int GetTotalTime(); // 누적 전투 시간
            int GetZeny();
            int GetExp();
            int GetJobExp();
            UIRewardItem.IInput[] GetRewardItems();
            int GetEmployerBattleTime(); // 고용주 전투 시간
            int GetTotalItemWeight();
        }

        /// <summary>
        /// <see cref="SharingEmployerPacket"/> 
        /// </summary>
        public interface ISharingEmployerData
        {
            string GetStageName();

            Job GetJob();
            int GetJobLevel();
            string GetName();
            int GetPower();
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UILabelHelper labelField;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabel labelTotalTime;
        [SerializeField] UILabelHelper labelRewards;
        [SerializeField] UIRewardGoods zeny, exp, jobExp;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabel labelWeightValue;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] GameObject goEmployerInfo;
        [SerializeField] UILabelHelper labelEmployer, labelBattleTime;
        [SerializeField] UILabel labelTime;
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UILabel labelJobLevel, labelName, labelPower;
        [SerializeField] UIButtonHelper btnChangeCharacter, btnQuit;
        [SerializeField] UIButtonHelper btnFinishSharing;
        [SerializeField] UIPlayTween center;
        [SerializeField] UILoopNotice loopNotice;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] GameObject kafraBuff, shareBuff;
        [SerializeField] UILabelHelper labelKafraBuffTime, labelShareBuffTime;
        [SerializeField] UILabelHelper labelKafraBuffValue, labelShareBuffValue;

        CharacterShareWaitingPresenter presenter;
        CumulativeTimeStopwatch totalTimeStopwatch, employerBattleTimeStopwatch;

        private UIRewardItem.IInput[] rewardItems;
        private bool hasEmployer;
        private bool isLimitExposeTime;
        private bool isAddEvent;

        protected override void OnInit()
        {
            presenter = new CharacterShareWaitingPresenter();
            totalTimeStopwatch = new CumulativeTimeStopwatch();
            employerBattleTimeStopwatch = new CumulativeTimeStopwatch();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnChangeCharacter.OnClick, OnClickedBtnChangeCharacter);
            EventDelegate.Add(btnQuit.OnClick, OnClickedBtnQuit);
            EventDelegate.Add(btnFinishSharing.OnClick, OnClickedBtnFinishSharing);

            AddEvent();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnChangeCharacter.OnClick, OnClickedBtnChangeCharacter);
            EventDelegate.Remove(btnQuit.OnClick, OnClickedBtnQuit);
            EventDelegate.Remove(btnFinishSharing.OnClick, OnClickedBtnFinishSharing);

            RemoveEvent();

            KillAllCoroutine();
        }

        protected override void OnShow(IUIData data = null)
        {
            AddEvent();

            UpdateSharingRewards();
            UpdateEmployer();
            UpdateLimitExposeTime();
            UpdateShareBuff();
            UpdateKafraBuff();
        }

        protected override void OnHide()
        {
            RemoveEvent();

            KillAllCoroutine();
        }

        private void AddEvent()
        {
            if (isAddEvent)
                return;

            isAddEvent = true;
            presenter.OnUpdateSharingState += UpdateSharingState;
            presenter.OnUpdateSharingRewards += UpdateSharingRewards;
            presenter.OnUpdateEmployer += UpdateEmployer;
            presenter.AddEvent();
        }

        private void RemoveEvent()
        {
            if (!isAddEvent)
                return;

            presenter.OnUpdateSharingState -= UpdateSharingState;
            presenter.OnUpdateSharingRewards -= UpdateSharingRewards;
            presenter.OnUpdateEmployer -= UpdateEmployer;
            presenter.RemoveEvent();
            isAddEvent = false;
        }

        protected override void OnLocalize()
        {
            labelRewards.LocalKey = LocalizeKey._10302; // 전투 보상
            labelEmployer.LocalKey = LocalizeKey._10304; // 고용주 정보
            labelBattleTime.LocalKey = LocalizeKey._10305; // 전투 시간
            btnChangeCharacter.LocalKey = LocalizeKey._10307; // 변경
            btnFinishSharing.LocalKey = LocalizeKey._10308; // 해제 후 보상받기
            btnQuit.LocalKey = LocalizeKey._10317; // 휴식 하기
            labelNoData.LocalKey = LocalizeKey._10320; // 아직 보상을 획득하지 못했습니다.

            loopNotice.Clear();
            loopNotice.AddNotice(LocalizeKey._10312); // 무게가 초과할 경우, 일부 아이템 획득이 제한 됩니다.
            loopNotice.AddNotice(LocalizeKey._10313); // 게임을 종료해도 셰어 상태는 해제 되지 않습니다.
            loopNotice.AddNotice(LocalizeKey._10314); // 전투 중 셰어 등록을 종료하더라도 불이익은 없습니다.
            loopNotice.AddNotice(LocalizeKey._10315); // 캐릭터를 변경하더라도 셰어 상태는 해제 되지 않습니다.
            loopNotice.Refresh();

            UpdateLimitExposeTimeText();

            labelShareBuffTime.LocalKey = LocalizeKey._10322; // 영구
            labelShareBuffValue.Text = presenter.GetShareBuffValueText();
            labelKafraBuffValue.Text = presenter.GetKafraBuffValueText();
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIRewardItem ui = go.GetComponent<UIRewardItem>();
            ui.SetData(rewardItems[index]);
        }

        void OnClickedBtnChangeCharacter()
        {
            if (presenter.GetCurrentBattleMode() == BattleMode.MultiMazeLobby)
            {
                UI.ShowToastPopup(LocalizeKey._90172.ToText()); // 미로 모드에서는 캐릭터 변경을 할 수 없습니다
                return;
            }

            UI.Show<UICharacterSelect>();
        }

        void OnClickedBtnFinishSharing()
        {
            AsyncShowFinishSharingPopup().WrapNetworkErrors();
        }

        void OnClickedBtnQuit()
        {
            UI.ShowExitPopup();
        }

        private void UpdateSharingState()
        {
            SharingModel.SharingState state = presenter.GetSharingState();
            if (state == SharingModel.SharingState.None)
            {
                UI.ShowToastPopup(LocalizeKey._10321.ToText()); // 정산받을 보상이 없습니다.
                HideUI();
            }
            else if (state == SharingModel.SharingState.StandByReward)
            {
                HideUI();
                UI.Show<UICharacterShareReward>();
            }
        }

        private void UpdateSharingRewards()
        {
            ISharingRewardData sharingRewardData = presenter.GetSharingRewardData();
            int totalTime = sharingRewardData == null ? 0 : sharingRewardData.GetTotalTime();
            int totalZeny = sharingRewardData == null ? 0 : sharingRewardData.GetZeny();
            int totalExp = sharingRewardData == null ? 0 : sharingRewardData.GetExp();
            int totalJobExp = sharingRewardData == null ? 0 : sharingRewardData.GetJobExp();
            rewardItems = sharingRewardData?.GetRewardItems();
            int employerBattleTime = sharingRewardData == null ? 0 : sharingRewardData.GetEmployerBattleTime();
            int totalItemWeight = sharingRewardData == null ? 0 : sharingRewardData.GetTotalItemWeight();
            int curWeight = presenter.GetCurrentInvenWeight() + totalItemWeight;
            int maxWeight = presenter.GetMaxInvenWeight();

            totalTimeStopwatch.Set(totalTime);
            employerBattleTimeStopwatch.Set(employerBattleTime);

            zeny.SetValue(RewardType.Zeny, totalZeny);
            exp.SetValue(RewardType.LevelExp, totalExp);
            jobExp.SetValue(RewardType.JobExp, totalJobExp);

            int length = rewardItems == null ? 0 : rewardItems.Length;
            wrapper.Resize(length);
            labelNoData.SetActive(length == 0);

            labelWeightValue.text = StringBuilderPool.Get()
                .Append((curWeight * 0.1f).ToString("0.#"))
                .Append("/")
                .Append((maxWeight * 0.1f).ToString("0.#"))
                .Release();

            if (hasEmployer)
            {
                totalTimeStopwatch.Resume();
                employerBattleTimeStopwatch.Resume();
            }
            else
            {
                totalTimeStopwatch.Pause();
                employerBattleTimeStopwatch.Pause();
            }

            RereshTime();
        }

        private void UpdateEmployer()
        {
            KillAllCoroutine();

            ISharingEmployerData sharingEmployerData = presenter.GetSharingEmployerData();

            hasEmployer = sharingEmployerData != null;
            labelField.SetActive(hasEmployer);
            labelNotice.SetActive(!hasEmployer);
            NGUITools.SetActive(goEmployerInfo, hasEmployer);

            if (hasEmployer)
            {
                totalTimeStopwatch.Resume();
                employerBattleTimeStopwatch.Resume();

                labelField.Text = sharingEmployerData.GetStageName();
                iconJob.SetJobIcon(sharingEmployerData.GetJob().GetJobIcon());
                labelJobLevel.text = LocalizeKey._10306.ToText() // JOB Lv.{LEVEL}
                    .Replace(ReplaceKey.LEVEL, sharingEmployerData.GetJobLevel());
                labelName.text = sharingEmployerData.GetName();
                labelPower.text = sharingEmployerData.GetPower().ToString();

                Timing.RunCoroutine(YieldUpdateTime(), gameObject);
                Timing.RunCoroutine(YieldRequestShareCharacterRewardInfo(), gameObject);
            }
            else
            {
                totalTimeStopwatch.Pause();
                employerBattleTimeStopwatch.Set(0);
                employerBattleTimeStopwatch.Pause();
            }
        }

        private void HideUI()
        {
            UI.Close<UICharacterShareWaiting>();
        }

        IEnumerator<float> YieldRequestShareCharacterRewardInfo()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(60f);
                presenter.RequestShareCharacterRewardInfo();
            }
        }

        IEnumerator<float> YieldUpdateTime()
        {
            while (true)
            {
                RereshTime();
                yield return Timing.WaitForSeconds(1f);
            }
        }

        private void RereshTime()
        {
            var span = totalTimeStopwatch.ToCumulativeTime().ToTimeSpan();
            int totalHours = (int)span.TotalHours;
            int minutes = span.Minutes;
            labelTotalTime.text = LocalizeKey._10301.ToText() // 누적 전투 시간 {HOURS}:{MINUTES}
                .Replace(ReplaceKey.HOURS, totalHours.ToString("00"))
                .Replace(ReplaceKey.MINUTES, minutes.ToString("00"));

            labelTime.text = employerBattleTimeStopwatch.ToStringTime(@"hh\:mm");
            SetLimitExposeTime(span.Ticks >= presenter.GetMaxExposeTimeTicks());
        }

        private void SetLimitExposeTime(bool isLimitExposeTime)
        {
            if (this.isLimitExposeTime == isLimitExposeTime)
                return;

            this.isLimitExposeTime = isLimitExposeTime;
            UpdateLimitExposeTime();
        }

        private void UpdateLimitExposeTime()
        {
            center.Play(isLimitExposeTime);
            UpdateLimitExposeTimeText();
        }

        private void UpdateLimitExposeTimeText()
        {
            labelMainTitle.LocalKey = isLimitExposeTime
                ? LocalizeKey._10310 // 캐릭터 셰어 만료
                : hasEmployer ? LocalizeKey._10300 : LocalizeKey._10316; // 캐릭터 셰어 중 or 캐릭터 셰어 대기 중

            labelNotice.LocalKey = isLimitExposeTime
                ? LocalizeKey._10311 // 셰어 등록이 만료 되었습니다.
                : LocalizeKey._10303; // 캐릭터 셰어 대기 중 입니다.
        }

        private async Task AsyncShowFinishSharingPopup()
        {
            if (hasEmployer)
            {
                string title = LocalizeKey._10308.ToText(); // 셰어 등록 종료하기
                string message = LocalizeKey._10309.ToText(); // 현재 전투가 진행 중입니다.\n해당 필드에서 이탈하고 정산을 진행 하시겠습니까?

                if (!await UI.SelectPopup(title, message))
                    return;
            }

            presenter.RequestShareRegisterEnd();
        }

        /// <summary>
        /// 영구 셰어 버프 정보 갱신
        /// </summary>
        private void UpdateShareBuff()
        {
            shareBuff.SetActive(presenter.IsShareBuff());
        }

        /// <summary>
        /// 카프라 버프 정보 갱신
        /// </summary>
        private void UpdateKafraBuff()
        {
            BuffItemInfo info = presenter.GetKafraBuffItem();

            bool enableBuff = info != null && info.IsValid();

            if (enableBuff)
            {
                kafraBuff.SetActive(true);
                Timing.RunCoroutineSingleton(YieldRemainTime(info).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
            else
            {
                kafraBuff.SetActive(false);
            }
        }

        private IEnumerator<float> YieldRemainTime(BuffItemInfo info)
        {
            while (info.IsValid())
            {
                labelKafraBuffTime.Text = info.RemainTimeText;
                yield return Timing.WaitForSeconds(1f);
            }
        }
        
        private void KillAllCoroutine()
        {
            Timing.KillCoroutines(gameObject);
        }
    }
}