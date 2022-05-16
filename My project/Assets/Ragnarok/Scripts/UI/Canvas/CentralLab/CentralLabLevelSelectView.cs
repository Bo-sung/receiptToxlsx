using UnityEngine;

namespace Ragnarok.View
{
    public class CentralLabLevelSelectView : UIView
    {
        public interface IInput
        {
            int StageLevel { get; }
            int GetStatBonus(int jobLevel);
            RewardData GetBaseReward();
            int PlusRewardCount { get; }
        }

        [SerializeField] UILabelHelper labelStageLevel;
        [SerializeField] UIPressButton btnLeft, btnRight;
        [SerializeField] UIPlayTween levelDetail;
        [SerializeField] UILabelValue stageLevel, jobLevel, bonusBuff;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UILabelHelper labelRewardDescription;
        [SerializeField] UIItemCostButtonHelper btnFastClear, btnEnter;
        [SerializeField] UIButtonHelper btnHelp;
        [SerializeField] UIButton btnFastClearLock, btnEnterLock;
        [SerializeField] UILabelHelper labelClear;

        private int maxIndex;
        private int curIndex;
        private int clearedIndex;
        private int characterJobLevel;
        private IInput input;
        private int maxEntryCount;
        private int clearTicketCount;
        private int freeTicketCount;

        public event System.Action<int> OnSelectIndex;
        public event System.Action<int> OnSelectFastClear;
        public event System.Action<int> OnSelectEnter;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnLeft.onClick, OnClickedBtnLeft);
            EventDelegate.Add(btnRight.onClick, OnClickedBtnRight);
            EventDelegate.Add(btnFastClear.OnClick, OnClickedBtnFastClear);
            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Add(btnHelp.OnClick, OnClickedBtnHelp);
            EventDelegate.Add(btnFastClearLock.onClick, OnClickedBtnFastClearLock);
            EventDelegate.Add(btnEnterLock.onClick, OnClickedBtnEnterLock);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnLeft.onClick, OnClickedBtnLeft);
            EventDelegate.Remove(btnRight.onClick, OnClickedBtnRight);
            EventDelegate.Remove(btnFastClear.OnClick, OnClickedBtnFastClear);
            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Remove(btnHelp.OnClick, OnClickedBtnHelp);
            EventDelegate.Remove(btnFastClearLock.onClick, OnClickedBtnFastClearLock);
            EventDelegate.Remove(btnEnterLock.onClick, OnClickedBtnEnterLock);
        }

        protected override void OnLocalize()
        {
            stageLevel.TitleKey = LocalizeKey._48302; // Stage Level
            jobLevel.TitleKey = LocalizeKey._48303; // JOB Level
            btnFastClear.LocalKey = LocalizeKey._48309; // 소탕
            labelClear.LocalKey = LocalizeKey._7056; // CLEAR!
        }

        void OnClickedBtnLeft()
        {
            Select(curIndex - 1);
        }

        void OnClickedBtnRight()
        {
            Select(curIndex + 1);
        }

        void OnClickedBtnFastClear()
        {
            OnSelectFastClear?.Invoke(curIndex);
        }

        void OnClickedBtnEnter()
        {
            OnSelectEnter?.Invoke(curIndex);
        }

        void OnClickedBtnFastClearLock()
        {
            string description = LocalizeKey._90222.ToText(); // 던전 소탕 기능은 클리어한 던전에서만 이용 가능합니다.
            UI.ShowToastPopup(description);
        }

        void OnClickedBtnEnterLock()
        {
            string description = LocalizeKey._90221.ToText(); // 이전 난이도를 클리어해야 합니다.
            UI.ShowToastPopup(description);
        }

        void OnClickedBtnHelp()
        {
            int dungeonInfoId = DungeonInfoType.CentralLab.GetDungeonInfoId();
            UI.Show<UIDungeonInfoPopup>().Show(dungeonInfoId);
        }

        public void SetMaxIndex(int value)
        {
            maxIndex = value;
        }

        public void SetClearedIndex(int value)
        {
            clearedIndex = value;
            Select(clearedIndex + 1); // 클리어한 다음 인덱스를 보여줌
            UpdateBtnClear(); // 소탕 버튼 업데이트
        }

        public void SetData(IInput input)
        {
            this.input = input;
            Refresh();
        }

        public void SetCharacterJobLevel(int value)
        {
            characterJobLevel = value;
            jobLevel.Value = characterJobLevel.ToString();
            UpdateBonusBuff();
        }

        public void SetMaxFreeTicketCount(int value)
        {
            maxEntryCount = value;
            UpdateEntryCostText();
        }

        public void SetClearTicketCount(int value)
        {
            clearTicketCount = value;
            btnFastClear.SetItemCount(clearTicketCount);
            btnFastClear.SetCountColor(clearTicketCount > 0);

            UpdateBtnClear(); // 소탕 버튼 업데이트
        }

        public void SetFreeTicketCount(int value)
        {
            freeTicketCount = value;
            UpdateEntryCostText();
            UpdateBtnEnter(); // 입장 버튼 업데이트
        }

        private void Select(int index)
        {
            curIndex = MathUtils.Clamp(index, 0, maxIndex);
            OnSelectIndex?.Invoke(curIndex);
        }

        private void UpdateBtnIndex()
        {
            btnLeft.isEnabled = curIndex > 0;
            btnRight.isEnabled = curIndex < maxIndex;
        }

        private void Refresh()
        {
            if (input == null)
                return;

            labelStageLevel.Text = LocalizeKey._48301.ToText() // LEVEL {VALUE}
                .Replace(ReplaceKey.VALUE, curIndex + 1);

            stageLevel.Value = input.StageLevel.ToString();
            UpdateBonusBuff();
            reward.SetData(input.GetBaseReward());
            labelRewardDescription.Text = LocalizeKey._48308.ToText() // 스테이지를 많이 클리어 할 때마다 보상 개수가 [c][74D3FB]{VALUE}개씩 증가[-][/c]합니다.
                .Replace(ReplaceKey.VALUE, input.PlusRewardCount);

            UpdateBtnIndex(); // 인덱스 관련 버튼 업데이트
            UpdateBtnClear(); // 소탕 버튼 업데이트
            UpdateBtnEnter(); // 입장 버튼 업데이트
            UpdateClear();
        }

        private void UpdateBonusBuff()
        {
            if (input == null)
                return;

            int bonus = input.GetStatBonus(characterJobLevel);
            bool hasBonus = bonus > 0;
            levelDetail.Play(hasBonus);

            if (hasBonus)
            {
                bonusBuff.Title = LocalizeKey._48305.ToText() // 스탯 보너스 +{VALUE}%
                    .Replace(ReplaceKey.VALUE, bonus);
                bonusBuff.Value = LocalizeKey._48307.ToText(); // JOB 레벨이 높아 스탯 보너스가 적용됩니다.
            }
            else
            {
                bonusBuff.Title = LocalizeKey._48304.ToText(); // 스탯 보너스 없음
                bonusBuff.Value = LocalizeKey._48306.ToText(); // JOB 레벨이 스테이지 레벨보다 낮습니다.
            }
        }

        private void UpdateBtnClear()
        {
            btnFastClear.IsEnabled = (clearTicketCount > 0); //  소탕권 존재
            btnFastClearLock.isEnabled = (clearTicketCount > 0) && (curIndex > clearedIndex); // 소탕권은 존재하지만 클리어하지 않았음
        }

        private void UpdateBtnEnter()
        {
            bool isCleard = curIndex <= clearedIndex;
            if (isCleard && freeTicketCount > 0)
            {
                btnEnter.LocalKey = LocalizeKey._7055; // 무료 소탕
            }
            else
            {
                btnEnter.LocalKey = LocalizeKey._7027; // 입장
            }

            btnEnterLock.isEnabled = curIndex > clearedIndex + 1; // 입장 버튼 업데이트
        }

        private void UpdateEntryCostText()
        {
            btnEnter.SetItemCount(StringBuilderPool.Get()
                .Append(freeTicketCount).Append("/").Append(maxEntryCount)
                .Release());
            btnEnter.SetCountColor(freeTicketCount > 0);
            btnEnter.SetNotice(freeTicketCount > 0);
        }

        private void UpdateClear()
        {
            bool isCleard = curIndex <= clearedIndex;
            labelClear.SetActive(isCleard);
        }
    }
}