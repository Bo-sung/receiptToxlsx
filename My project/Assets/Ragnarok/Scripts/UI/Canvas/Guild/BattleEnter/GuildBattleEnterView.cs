using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GuildBattleEnterView : UIView
    {
        private const string TAG = nameof(GuildBattleEnterView) + "+" + nameof(YieldDelayRefresh);

        [Header("Character")]
        [SerializeField] UILabelHelper labelCharacter;
        [SerializeField] UITextureHelper profile;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelHelper labelCharacterName;
        [SerializeField] UILabelHelper labelBattleScore;
        [SerializeField] UILabelHelper labelTotalDamage;

        [Header("Guild")]
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIGuildBattleElement element;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIButtonHelper btnRefresh, btnLock;
        [SerializeField] UILabelHelper labelRemainCount;

        SuperWrapContent<UIGuildBattleElement, UIGuildBattleElement.IInput> wrapContent;

        public event System.Action OnRefresh;
        public event System.Action<UIGuildBattleElement.IInput> OnSelect;

        private int requestGuildListDelay;
        private int dailyEntryCount;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIGuildBattleElement, UIGuildBattleElement.IInput>(element);

            foreach (UIGuildBattleElement item in wrapContent)
            {
                item.OnSelect += OnSelectGuild;
            }

            EventDelegate.Add(btnRefresh.OnClick, OnClickedBtnRefresh);
            EventDelegate.Add(btnLock.OnClick, OnClickedBtnLock);

            btnLock.SetActive(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (UIGuildBattleElement item in wrapContent)
            {
                item.OnSelect -= OnSelectGuild;
            }

            EventDelegate.Remove(btnRefresh.OnClick, OnClickedBtnRefresh);
            EventDelegate.Remove(btnLock.OnClick, OnClickedBtnLock);

            Timing.KillCoroutines(TAG);
        }

        protected override void OnLocalize()
        {
            labelCharacter.LocalKey = LocalizeKey._33716; // 내 정보

            labelNoData.LocalKey = LocalizeKey._33710; // 길드 정보가 없습니다.
            labelNotice.LocalKey = LocalizeKey._33736; // 전투를 참여하지 않으면, 수비와 랭킹 보상을 받을 수 없습니다.
            btnRefresh.LocalKey = LocalizeKey._33713; // 목록 변경
        }

        void OnSelectGuild(UIGuildBattleElement.IInput id)
        {
            OnSelect?.Invoke(id);
        }

        void OnClickedBtnRefresh()
        {
            OnRefresh?.Invoke();
        }

        void OnClickedBtnLock()
        {
            UI.ShowToastPopup(LocalizeKey._33715.ToText()); // 잠시 후 다시 시도하십시오.
        }

        public void Initialize(int requestGuildListDelay, int dailyEntryCount)
        {
            this.requestGuildListDelay = requestGuildListDelay;
            this.dailyEntryCount = dailyEntryCount;
        }

        public void SetData(UIGuildBattleElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
            wrapContent.SetProgress(0);

            int length = inputs == null ? 0 : inputs.Length;
            labelNoData.SetActive(length == 0);
            btnRefresh.IsEnabled = length > 0;

            if (length > 0)
            {
                Timing.KillCoroutines(TAG);
                Timing.RunCoroutine(YieldDelayRefresh(), TAG);
            }
        }

        private IEnumerator<float> YieldDelayRefresh()
        {
            btnLock.SetActive(true);

            int seconds = requestGuildListDelay;
            while (seconds > 0)
            {
                btnLock.Text = LocalizeKey._33714.ToText() // {SECONDS} 초
                    .Replace(ReplaceKey.SECONDS, seconds);

                yield return Timing.WaitForSeconds(1f);
                --seconds;
            }

            btnLock.SetActive(false);
        }

        public void SetProfile(string profileName)
        {
            profile.SetJobProfile(profileName);
        }

        public void SetJobIcon(string jobIconName)
        {
            jobIcon.SetJobIcon(jobIconName);
        }

        public void SetCharacterName(int jobLevel, string characterName)
        {
            labelCharacterName.Text = StringBuilderPool.Get()
                .Append("Lv. ").Append(jobLevel).Append(" ").Append(characterName)
                .Release();
        }

        public void SetBattleScore(int battleScore)
        {
            labelBattleScore.Text = LocalizeKey._33717.ToText() // 전투력 {VALUE}
                .Replace(ReplaceKey.VALUE, battleScore.ToString("N0"));
        }

        public void SetTotalDamage(long totalDamage)
        {
            labelTotalDamage.Text = LocalizeKey._33708.ToText() // 누적 피해량 {VALUE}
                .Replace(ReplaceKey.VALUE, totalDamage.ToString("N0"));
        }

        public void RemainCount(int remainCount)
        {
            labelRemainCount.Text = StringBuilderPool.Get()
                .Append(remainCount).Append("/").Append(dailyEntryCount)
                .Release();
        }
    }
}