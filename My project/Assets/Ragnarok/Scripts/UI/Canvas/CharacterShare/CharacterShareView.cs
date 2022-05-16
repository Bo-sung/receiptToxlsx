using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View.CharacterShare
{
    public class CharacterShareView : UIView, IInspectorFinder, TutorialSharingCharacterEquip.IEquipSharingCharacterImpl
    {
        [SerializeField] GameObject normal;
        [SerializeField] GameObject advanced;
        [SerializeField] UILabelValue viceLevel;
        [SerializeField] UILabelValue maxPower;
        [SerializeField] GameObject buffInfo;
        [SerializeField] UILabelHelper labelTotalLevel;
        [SerializeField] UILabel labelBuffTime;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper labelDescription;
        //[SerializeField] UILabelHelper labelTitle;
        [SerializeField] UICharacterShareBar[] characterShareBars;
        [SerializeField] UILabelHelper labelRemainTime;
        [SerializeField] UILabel labelTime;
        //[SerializeField] UILabelHelper labelTicketCount;
        [SerializeField] UIButtonHelper btnFreeCharge;
        [SerializeField] UIButtonHelper btnCharge;
        [SerializeField] UIButtonHelper btnLevelUp;
        [SerializeField] UIButtonHelper btnFilter;
        [SerializeField] UIButtonHelper btnAutoSetting;
        [SerializeField] UIButtonHelper btnLock;
        [SerializeField] UIGraySprite levelUpSprite;
        [SerializeField] UIGraySprite levelUpIcon;
        [SerializeField] UIGraySprite filterSprite;
        [SerializeField] UIGraySprite filterIcon;
        [SerializeField] UIGraySprite filterLabelBack;
        [SerializeField] UILabelHelper labelLevelUp;

        [SerializeField] GameObject levelUpFX;
        [SerializeField] GameObject autoShareFX;

        [SerializeField] UIGridHelper grid;

        [SerializeField] UIWidget widgetViceStatus;
        [SerializeField] UIWidget widgetViceLevelUp;
        [SerializeField] UIWidget widgetCharPanel;

        public event System.Action OnFinishShareviceBuff;
        public event System.Action OnSelectFreeCharge;
        public event System.Action OnSelectCharge;
        public event System.Action OnSelectAutoSetting;
        public event System.Action OnSelectAddSlot;
        public event System.Action OnSelectCloneAddSlot;
        public event System.Action OnClickLevelUP;
        public event UICharacterShare.SelectShareCharacterEvent OnSelectDelete;
        public event UICharacterShare.SelectCloneCharacterEvent OnSelectCloneDelete;

        RemainTimeStopwatch remainTimeForShare;
        private int inputCount, inputCloneCount;

        private int jobGrade;
        private int shareSlotCount;
        private int levelUpShareviceId;

        private bool unlockFilter;

        protected override void Awake()
        {
            base.Awake();

            remainTimeForShare = new RemainTimeStopwatch();

            for (int i = 0; i < characterShareBars.Length; i++)
            {
                if (i < Constants.Size.SHARE_SLOT_SIZE) // 셰어 캐릭터
                {
                    characterShareBars[i].OnSelectAddSlot += OnCharacterShareAddSlot;
                    characterShareBars[i].OnSelectDelete += OnCharacterShareDelete;
                }
                else // 클론 캐릭터
                {
                    characterShareBars[i].OnSelectAddSlot += OnCharacterCloneAddSlot;
                    characterShareBars[i].OnSelectCloneDelete += OnCharacterCloneDelete;
                }
            }

            EventDelegate.Add(btnFreeCharge.OnClick, OnClickedBtnFreeCharge);
            EventDelegate.Add(btnCharge.OnClick, OnClickedBtnCharge);
            EventDelegate.Add(btnLevelUp.OnClick, OnClickedBtnLevelUp);
            EventDelegate.Add(btnFilter.OnClick, OnClickedBtnFilter);
            EventDelegate.Add(btnAutoSetting.OnClick, OnClickedBtnAutoSetting);
            EventDelegate.Add(btnLock.OnClick, OnClickedBtnLock);

            ResetData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            for (int i = 0; i < characterShareBars.Length; i++)
            {
                if (i < Constants.Size.SHARE_SLOT_SIZE) // 셰어 캐릭터
                {
                    characterShareBars[i].OnSelectAddSlot -= OnCharacterShareAddSlot;
                    characterShareBars[i].OnSelectDelete -= OnCharacterShareDelete;
                }
                else // 클론 캐릭터
                {
                    characterShareBars[i].OnSelectAddSlot -= OnCharacterCloneAddSlot;
                    characterShareBars[i].OnSelectCloneDelete -= OnCharacterCloneDelete;
                }
            }

            EventDelegate.Remove(btnFreeCharge.OnClick, OnClickedBtnFreeCharge);
            EventDelegate.Remove(btnCharge.OnClick, OnClickedBtnCharge);
            EventDelegate.Remove(btnLevelUp.OnClick, OnClickedBtnLevelUp);
            EventDelegate.Remove(btnFilter.OnClick, OnClickedBtnFilter);
            EventDelegate.Remove(btnAutoSetting.OnClick, OnClickedBtnAutoSetting);
            EventDelegate.Remove(btnLock.OnClick, OnClickedBtnLock);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._10231; // 이용 안내
            labelDescription.LocalKey = LocalizeKey._10201; // 셰어 캐릭터를 고용하여 [c][84A2EC]같이 사냥[-][/c]을 진행할 수 있습니다.\n남은 이용 시간은 이용 중인 셰어 캐릭터의 수에 따라 감소합니다.
            //labelTitle.LocalKey = LocalizeKey._10203; // 필드 내 셰어 캐릭터
            labelRemainTime.LocalKey = LocalizeKey._10206; // 남은 이용 시간
            btnFreeCharge.LocalKey = LocalizeKey._10244; // 무료 충전
            btnCharge.LocalKey = LocalizeKey._10207; // 충전
            btnAutoSetting.LocalKey = LocalizeKey._10237; // 자동장착
            btnLevelUp.LocalKey = LocalizeKey._10243; // 레벨업

            viceLevel.TitleKey = LocalizeKey._10246; // Sharevice
            maxPower.TitleKey = LocalizeKey._10247; // Max Power BP
            btnLock.LocalKey = LocalizeKey._2019; // 슬롯 오픈
        }

        void Update()
        {
            if (isTutorialModel)
                return;

            if (remainTimeForShare.IsFinished())
                return;

            if (RefreshTime())
                return;

            // Finished
        }

        void OnCharacterShareAddSlot()
        {
            OnSelectAddSlot?.Invoke();
        }

        void OnCharacterCloneAddSlot()
        {
            OnSelectCloneAddSlot?.Invoke();
        }

        void OnCharacterShareDelete(int cid, int uid, SharingModel.SharingCharacterType sharingCharacterType)
        {
            OnSelectDelete?.Invoke(cid, uid, sharingCharacterType);
        }

        void OnCharacterCloneDelete(int cid, int uid, SharingModel.CloneCharacterType cloneCharacterType)
        {
            OnSelectCloneDelete?.Invoke(cid, uid, cloneCharacterType);
        }

        void OnClickedBtnFreeCharge()
        {
            OnSelectFreeCharge?.Invoke();
        }

        void OnClickedBtnCharge()
        {
            OnSelectCharge?.Invoke();
        }

        void OnClickedBtnLevelUp()
        {
            if (!Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp, false)) // 컨텐츠 해금 전
            {
                UI.ShowToastPopup(LocalizeKey._2022.ToText().Replace(ReplaceKey.NAME, levelUpShareviceId.ToText())); // 시나리오 미궁 {NAME} 클리어 필요
                return;
            }

            OnClickLevelUP?.Invoke();
        }

        void OnClickedBtnFilter()
        {
            if (unlockFilter)
            {
                UI.Show<UIShareFilter>();
            }
            else
            {
                int needGrade = Constants.OpenCondition.NEED_SHARE_FILTER_JOB_GRADE;

                string description = LocalizeKey._90170.ToText() // {VALUE}차 전직 후 이용 가능합니다.
                           .Replace(ReplaceKey.VALUE, needGrade)
                           .Replace(ReplaceKey.LEVEL, BasisType.JOB_MAX_LEVEL.GetInt(needGrade));

                UI.ShowToastPopup(description);
            }
        }

        void OnClickedBtnLock()
        {
            if (jobGrade > 0)
            {
                UI.Show<UIJobGrowth>();
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._26030.ToText()); // 튜토리얼 중에는 이용할 수 없습니다.
            }
        }

        void OnClickedBtnAutoSetting()
        {
            if (inputCount < shareSlotCount)
            {
                OnSelectAutoSetting?.Invoke();
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._10245.ToText()); // 장착 가능한 슬롯이 없습니다.
            }
        }

        void UpdateViceContentsState()
        {
            if (!Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp, false)) // 컨텐츠 해금 전
            {
                normal.SetActive(true);
                advanced.SetActive(false);
                //btnLevelUp.IsEnabled = false;
                levelUpSprite.Mode = UIGraySprite.SpriteMode.Grayscale;
                levelUpIcon.Mode = UIGraySprite.SpriteMode.Grayscale;
                labelLevelUp.Color = new Color32(92, 92, 92, 255);
            }
            else // 컨텐츠 해금 후
            {
                normal.SetActive(false);
                advanced.SetActive(true);
                //btnLevelUp.IsEnabled = true;
                levelUpSprite.Mode = UIGraySprite.SpriteMode.None;
                levelUpIcon.Mode = UIGraySprite.SpriteMode.None;
                labelLevelUp.Color = new Color32(52, 115, 188, 255);
            }
        }

        void UpdateShareViceFilterState(bool unlockFilter)
        {
            this.unlockFilter = unlockFilter;

            if (unlockFilter)
            {
                filterSprite.Mode = UIGraySprite.SpriteMode.None;
                filterIcon.Mode = UIGraySprite.SpriteMode.None;
                filterLabelBack.Mode = UIGraySprite.SpriteMode.None;
            }
            else
            {
                filterSprite.Mode = UIGraySprite.SpriteMode.Grayscale;
                filterIcon.Mode = UIGraySprite.SpriteMode.Grayscale;
                filterLabelBack.Mode = UIGraySprite.SpriteMode.Grayscale;
            }
        }

        public void UpdateShareViceValue(BuffItemInfo info, int level, int buffLevel, int bp, ShareviceState state, bool hasViceItem)
        {
            viceLevel.Value = LocalizeKey._18005.ToText(). // Lv. {LEVEL}
                   Replace(ReplaceKey.LEVEL, level + buffLevel);
            maxPower.Value = bp.ToString("#,##0");

            bool enableBuff = info != null && info.IsValid();

            if (enableBuff)
            {
                buffInfo.SetActive(true);
                labelTotalLevel.Text = string.Format("+{0}", buffLevel);
                Timing.RunCoroutineSingleton(YieldRefreshBuffTime(info).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
            else
            {
                buffInfo.SetActive(false);
            }

            // 레벨업 버튼 이펙트
            bool isOpenContentShareLevelUp = Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp, false); // 컨텐츠 해금
            if (isOpenContentShareLevelUp)
            {
                if ((state == ShareviceState.Normal && hasViceItem) // 레벨업 중이 아닐 때, 쉐어바이스 레벨업 아이템을 가지고 있을 때
                    || state == ShareviceState.LevelUpComplete) // 레벨업 완료 할수 있을 때
                {
                    levelUpFX.SetActive(true);
                }
                else
                {
                    levelUpFX.SetActive(false);
                }
            }
            else
            {
                levelUpFX.SetActive(false);
            }
        }
        public void ResetData()
        {
            SetData(null, null);
            SetRemainTime(0);
        }

        public void SetData(UISimpleCharacterShareBar.IInput[] inputs, UISimpleCharacterShareBar.IInput[] inputsClone, int jobGrade = 0, int shareSlotCount = 0, int cloneSlotCount = 0, int levelUpViceId = 0, bool unlockFilter = true)
        {
            grid.SetValue(shareSlotCount); // 셰어 슬롯개수 셋팅
            for (int i = 0; i < cloneSlotCount; i++) // 클론 슬롯
            {
                characterShareBars[Constants.Size.SHARE_SLOT_SIZE + i].SetActiveGO(true);
            }
            grid.Reposition(); // 슬롯위치 갱신

            inputCount = inputs == null ? 0 : inputs.Length;
            inputCloneCount = inputsClone == null ? 0 : inputsClone.Length;
            levelUpShareviceId = levelUpViceId;
            this.jobGrade = jobGrade;
            this.shareSlotCount = shareSlotCount;

            for (int i = 0; i < characterShareBars.Length; i++)
            {
                if (i < Constants.Size.SHARE_SLOT_SIZE) // 기존 셰어
                {
                    if (i < inputCount)
                    {
                        characterShareBars[i].SetBarType(UICharacterShareBar.BarType.Info);
                        characterShareBars[i].SetData(inputs[i]);
                    }
                    else
                    {
                        characterShareBars[i].SetBarType(i == inputCount ? UICharacterShareBar.BarType.Slot : UICharacterShareBar.BarType.None);
                        characterShareBars[i].SetData(null);
                    }
                }
                else // 클론 셰어
                {
                    var j = i - Constants.Size.SHARE_SLOT_SIZE; // 클론의 인덱스

                    if (j < inputCloneCount)
                    {
                        characterShareBars[i].SetBarType(UICharacterShareBar.BarType.Info);
                        characterShareBars[i].SetData(inputsClone[j]);
                    }
                    else
                    {
                        characterShareBars[i].SetBarType(j == inputCloneCount ? UICharacterShareBar.BarType.Slot : UICharacterShareBar.BarType.None);
                        characterShareBars[i].SetData(null);
                    }
                }
            }

            //btnAutoSetting.IsEnabled = inputCount < characterShareBars.Length;
            UpdateStopwatch();
            UpdateViceContentsState();
            UpdateShareViceFilterState(unlockFilter);

            // 자동장착 버튼 이펙트
            RefreshAutoButtonFX();
        }

        public void SetRemainTime(float remainTime)
        {
            remainTimeForShare.Set(remainTime);
            UpdateStopwatch();
            RefreshTime();

            // 자동장착 버튼 이펙트
            RefreshAutoButtonFX();
        }

        public void SetChargeNotice(bool hasFreeTicket)
        {
            btnFreeCharge.SetActive(hasFreeTicket);
            btnCharge.SetActive(!hasFreeTicket);
        }

        public void SetFilterCount(int count)
        {
            // 필터 갯수를 갱신해주자.
            btnFilter.Text = count.ToString();
        }

        private void UpdateStopwatch()
        {
            if (inputCount == 0 && inputCloneCount == 0)
            {
                remainTimeForShare.Pause();
            }
            else
            {
                // 튜토리얼 모드의 경우에는 시간이 흐르지 않도록 처리
                if (isTutorialModel)
                    return;

                remainTimeForShare.Resume();
            }
        }

        private bool RefreshTime()
        {
            float remainTime = remainTimeForShare.ToRemainTime();

            // Apply TimeScale
            int totalShareCount = inputCount + inputCloneCount;
            if (totalShareCount > 1)
                remainTime *= totalShareCount;

            var timeSpan = remainTime.ToTimeSpan();
            int totalHours = (int)timeSpan.TotalHours;
            labelTime.text = StringBuilderPool.Get()
                .Append(totalHours.ToString("00")).Append(":").Append(timeSpan.Minutes.ToString("00")).Append(":").Append(timeSpan.Seconds.ToString("00"))
                .Release();
            return remainTimeForShare.IsFinished();
        }

        // 자동장착버튼 이펙트 활성화상태 갱신
        private void RefreshAutoButtonFX()
        {
            autoShareFX.SetActive(inputCount < shareSlotCount // 장착 가능한 슬롯이 있고,
                && !remainTimeForShare.IsFinished()); // 남은 쉐어시간이 있을 때.
        }

        private IEnumerator<float> YieldRefreshBuffTime(BuffItemInfo info)
        {
            while (info.IsValid())
            {
                labelBuffTime.text = info.RemainTimeText;
                yield return Timing.WaitForSeconds(1f);
            }

            OnFinishShareviceBuff?.Invoke(); // 셰어 UI가 활성화 되어있는동안 버프가 끝날경우
        }

        bool IInspectorFinder.Find()
        {
            characterShareBars = transform.GetComponentsInChildren<UICharacterShareBar>();

            if (btnLevelUp)
                levelUpSprite = btnLevelUp.GetComponent<UIGraySprite>();

            if (btnFilter)
                filterSprite = btnFilter.GetComponent<UIGraySprite>();

            return true;
        }

        #region Tutorial
        bool isTutorialModel;

        void TutorialSharingCharacterEquip.IEquipSharingCharacterImpl.SetTutorialMode(bool isTutorialModel)
        {
            this.isTutorialModel = isTutorialModel;

            // 스톱워치 업데이트
            if (isTutorialModel)
                UpdateStopwatch();
        }

        UIWidget TutorialSharingCharacterEquip.IEquipSharingCharacterImpl.GetBtnAddShare()
        {
            return characterShareBars[0].AddButton;
        }

        bool TutorialSharingCharacterEquip.IEquipSharingCharacterImpl.IsEquippedSingleSharingCharacter()
        {
            return inputCount == 1;
        }

        UIWidget TutorialSharingCharacterEquip.IEquipSharingCharacterImpl.GetBtnAutoSetting()
        {
            return btnAutoSetting.GetComponent<UIWidget>();
        }

        bool TutorialSharingCharacterEquip.IEquipSharingCharacterImpl.IsEquippedSharingCharacter()
        {
            return inputCount > 1;
        }

        public UIWidget GetWidgetViceStatus()
        {
            return widgetViceStatus;
        }

        public UIWidget GetBtnViceLevelUp()
        {
            return widgetViceLevelUp;
        }

        public UIWidget GetWidgetCharacterPanel()
        {
            return widgetCharPanel;
        }

        #endregion
    }
}