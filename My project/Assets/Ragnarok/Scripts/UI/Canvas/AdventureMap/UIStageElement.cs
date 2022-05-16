using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIStageElement : UIElement<UIStageElement.IInput>, IInspectorFinder
    {
        public interface IInput
        {
            int StageId { get; }
            int LocalKey { get; }
            string MonsterIconName { get; }
            int MonsterLocalKey { get; }
            string MapIconName { get; }
            RewardData Reward { get; }
            bool IsCurrentGuideQuest { get; }
            bool IsOpened { get; }
            bool IsCompleted { get; }
            bool IsPlayerOnHere { get; }
            string PlayerThumbnailName { get; }
            bool IsEventMode { get; }
            int EventStageLevel { get; }

            event System.Action OnUpdateCurrentQuest;
            event System.Action OnUpdateOpen;
            event System.Action OnUpdateComplete;
            event System.Action OnUpdatePlayerOnHere;
            event System.Action OnUpdatePlayerThumbnail;
            event System.Action OnUpdateMode;
            event System.Action OnUpdateEventStageLevel;
        }

        [SerializeField] UILabelHelper labelStage;
        [SerializeField] SwitchLabelOutlineColor switchLabelStage;
        [SerializeField] GameObject guideNotice;
        [SerializeField] UITextureHelper iconMonster;
        [SerializeField] UILabelHelper labelMonster;
        [SerializeField] UIButton btnInfo;
        [SerializeField] UITextureHelper iconMap;
        [SerializeField] SwitchWidgetColor tweenIconMap;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UILabelHelper labelStageReward;
        [SerializeField] SwitchSpriteName complete;
        [SerializeField] UILabelHelper labelEventLevel;
        [SerializeField] SwitchWidgetColor tweenEventLevel;
        [SerializeField] UIButtonHelper btnEnter;
        [SerializeField] UIButtonHelper btnLock;
        [SerializeField] GameObject goCurrentPos;
        [SerializeField] UITextureHelper profile;

        public event System.Action<int> OnSelectEnter;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnInfo.onClick, OnClickedBtnInfo);
            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Add(btnLock.OnClick, OnClickedBtnLock);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnInfo.onClick, OnClickedBtnInfo);
            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Remove(btnLock.OnClick, OnClickedBtnLock);
        }

        void OnClickedBtnInfo()
        {
            UI.Show<UIStageInfoPopup>().Show(info.StageId, false);
        }

        void OnClickedBtnEnter()
        {
            OnSelectEnter?.Invoke(info.StageId);
        }

        void OnClickedBtnLock()
        {
            UI.ShowToastPopup(LocalizeKey._90227.ToText()); // 전 필드의 보스를 클리어해 주세요.
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            info.OnUpdateCurrentQuest += RefreshCurrentQuest;
            info.OnUpdateOpen += RefreshOpen;
            info.OnUpdateComplete += RefreshComplete;
            info.OnUpdatePlayerOnHere += RefreshPlayerOnHere;
            info.OnUpdatePlayerThumbnail += RefreshPlayerThumbnail;
            info.OnUpdateMode += RefreshMode;
            info.OnUpdateEventStageLevel += RefreshEventStageLevel;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            info.OnUpdateCurrentQuest -= RefreshCurrentQuest;
            info.OnUpdateOpen -= RefreshOpen;
            info.OnUpdateComplete -= RefreshComplete;
            info.OnUpdatePlayerOnHere -= RefreshPlayerOnHere;
            info.OnUpdatePlayerThumbnail -= RefreshPlayerThumbnail;
            info.OnUpdateMode -= RefreshMode;
            info.OnUpdateEventStageLevel -= RefreshEventStageLevel;
        }

        protected override void OnLocalize()
        {
            UpdateStageName();
            UpdateMonsterName();

            labelStageReward.LocalKey = LocalizeKey._48202; // 필드 보상
        }

        protected override void Refresh()
        {
            iconMonster.SetMonster(info.MonsterIconName);
            iconMap.SetAdventure(info.MapIconName, isAsync: false);
            reward.SetData(info.Reward);

            UpdateStageName();
            UpdateMonsterName();
            RefreshCurrentQuest();
            RefreshOpen();
            RefreshComplete();
            RefreshPlayerOnHere();
            RefreshPlayerThumbnail();
            RefreshMode();
            RefreshEventStageLevel();
        }

        private void UpdateStageName()
        {
            if (info == null)
            {
                labelStage.Text = string.Empty;
                return;
            }

            labelStage.LocalKey = info.LocalKey;
        }

        private void UpdateMonsterName()
        {
            if (info == null)
            {
                labelMonster.Text = string.Empty;
                return;
            }

            labelMonster.LocalKey = info.MonsterLocalKey;
        }

        private void RefreshCurrentQuest()
        {
            guideNotice.SetActive(info.IsCurrentGuideQuest);
        }

        private void RefreshOpen()
        {
            tweenIconMap.Switch(info.IsOpened);
            btnEnter.SetActive(info.IsOpened);
            btnLock.SetActive(!info.IsOpened);
        }

        private void RefreshComplete()
        {
            complete.SetActive(info.IsCompleted);
            tweenEventLevel.Switch(info.IsCompleted);
        }

        private void RefreshPlayerOnHere()
        {
            NGUITools.SetActive(goCurrentPos, info.IsPlayerOnHere);
        }

        private void RefreshPlayerThumbnail()
        {
            profile.SetJobProfile(info.PlayerThumbnailName);
        }

        private void RefreshMode()
        {
            complete.Switch(info.IsEventMode);
            labelEventLevel.SetActive(info.IsEventMode);
            switchLabelStage.Switch(info.IsEventMode);
        }

        private void RefreshEventStageLevel()
        {
            labelEventLevel.Text = LocalizeKey._48221.ToText() // Lv. {LEVEL}
                .Replace(ReplaceKey.LEVEL, info.EventStageLevel);
        }

        bool IInspectorFinder.Find()
        {
            bool isFind = false;

            if (switchLabelStage == null)
                isFind = switchLabelStage = labelStage.GetComponent<SwitchLabelOutlineColor>();

            if (tweenIconMap == null)
                isFind = tweenIconMap = iconMap.GetComponent<SwitchWidgetColor>();

            if (tweenEventLevel == null)
                isFind = tweenEventLevel = labelEventLevel.GetComponent<SwitchWidgetColor>();

            return isFind;
        }
    }
}