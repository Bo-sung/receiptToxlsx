using UnityEngine;

namespace Ragnarok.View
{
    public sealed class NabihoMainView : UIView
    {
        [SerializeField] UILabelHelper labelFriendShip;
        [SerializeField] UILabel labelLevel;
        [SerializeField] UILabelValue decreaseTime;
        [SerializeField] UIProgressBar progressBar;
        [SerializeField] UILabel labelProgress;
        [SerializeField] UILabel labelLevelValue;
        [SerializeField] UIButtonHelper btnGift;
        [SerializeField] UINabihoSelectBar equipment, box, special;

        private int currentLevel = -1;

        public event System.Action OnSelectGift;
        public event System.Action OnSelectEquipment;
        public event System.Action OnSelectBox;
        public event System.Action OnSelectSpecial;
        public event System.Action<int> OnTryShowAd;
        public event System.Action<int> OnTryCancel;
        public event System.Action<int> OnTryReward;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnGift.OnClick, OnClickedBtnGift);

            equipment.OnSelectEnter += OnEnterEquipment;
            box.OnSelectEnter += OnEnterBox;
            special.OnSelectEnter += OnEnterSpecial;

            equipment.OnSelectShowAd += OnSelectShowAd;
            box.OnSelectShowAd += OnSelectShowAd;
            special.OnSelectShowAd += OnSelectShowAd;

            equipment.OnSelectCancel += OnSelectCancel;
            box.OnSelectCancel += OnSelectCancel;
            special.OnSelectCancel += OnSelectCancel;

            equipment.OnSelectReward += OnSelectReward;
            box.OnSelectReward += OnSelectReward;
            special.OnSelectReward += OnSelectReward;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnGift.OnClick, OnClickedBtnGift);

            equipment.OnSelectEnter -= OnEnterEquipment;
            box.OnSelectEnter -= OnEnterBox;
            special.OnSelectEnter -= OnEnterSpecial;

            equipment.OnSelectShowAd -= OnSelectShowAd;
            box.OnSelectShowAd -= OnSelectShowAd;
            special.OnSelectShowAd -= OnSelectShowAd;

            equipment.OnSelectCancel -= OnSelectCancel;
            box.OnSelectCancel -= OnSelectCancel;
            special.OnSelectCancel -= OnSelectCancel;

            equipment.OnSelectReward -= OnSelectReward;
            box.OnSelectReward -= OnSelectReward;
            special.OnSelectReward -= OnSelectReward;
        }

        void OnClickedBtnGift()
        {
            OnSelectGift?.Invoke();
        }

        void OnEnterEquipment()
        {
            OnSelectEquipment?.Invoke();
        }

        void OnEnterBox()
        {
            OnSelectBox?.Invoke();
        }

        void OnEnterSpecial()
        {
            OnSelectSpecial?.Invoke();
        }

        void OnSelectShowAd(int id)
        {
            OnTryShowAd?.Invoke(id);
        }

        void OnSelectCancel(int id)
        {
            OnTryCancel?.Invoke(id);
        }

        void OnSelectReward(int id)
        {
            OnTryReward?.Invoke(id);
        }

        protected override void OnLocalize()
        {
            labelFriendShip.LocalKey = LocalizeKey._10901; // 도림족 친밀도
            decreaseTime.TitleKey = LocalizeKey._10903; // 시간 단축
            btnGift.LocalKey = LocalizeKey._10904; // 선물하기
            RefreshLevelText();

            equipment.LocalKey = LocalizeKey._10905; // 장비의뢰
            box.LocalKey = LocalizeKey._10906; // 상자의뢰
            special.LocalKey = LocalizeKey._10907; // 특수의뢰
        }

        public void Initialize(int equipmentNeedLevel, int boxNeedLevel, int specialNeedLevel)
        {
            equipment.Initialize(equipmentNeedLevel);
            box.Initialize(boxNeedLevel);
            special.Initialize(specialNeedLevel);

            RefreshLevel();
        }

        public void UpdateLevel(int level, int cur, int max, int reduceMinutes)
        {
            currentLevel = level;

            progressBar.value = MathUtils.GetProgress(cur, max);
            labelProgress.text = StringBuilderPool.Get()
                .Append(cur).Append('/').Append(max)
                .Release();
            labelLevelValue.text = currentLevel.ToString();
            System.TimeSpan timeSpan = System.TimeSpan.FromMinutes(reduceMinutes);
            decreaseTime.Value = StringUtils.TimeToText(timeSpan);
            RefreshLevelText();

            RefreshLevel();
        }

        public void UpdateEquipment(UINabihoSelectBar.IInput input)
        {
            equipment.SetData(input);
        }

        public void UpdateBox(UINabihoSelectBar.IInput input)
        {
            box.SetData(input);
        }

        public void UpdateSpecial(UINabihoSelectBar.IInput input)
        {
            special.SetData(input);
        }

        private void RefreshLevel()
        {
            equipment.RefreshLevel(currentLevel);
            box.RefreshLevel(currentLevel);
            special.RefreshLevel(currentLevel);
        }

        private void RefreshLevelText()
        {
            labelLevel.text = LocalizeKey._10902.ToText() // {VALUE}단계
                .Replace(ReplaceKey.VALUE, currentLevel);
        }
    }
}