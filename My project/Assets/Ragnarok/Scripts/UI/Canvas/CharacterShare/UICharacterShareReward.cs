using UnityEngine;

namespace Ragnarok
{
    public sealed class UICharacterShareReward : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UITextureHelper thumbnail;
        [SerializeField] UILabel labelTotalTime;
        [SerializeField] UIRewardGoods zeny, exp, jobExp;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UILabelHelper labelNotice;

        CharacterShareRewardPresenter presenter;

        private UIRewardItem.IInput[] rewardItems;

        protected override void OnInit()
        {
            presenter = new CharacterShareRewardPresenter();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnConfirm.OnClick, presenter.RequestShareCharacterRewardGet);

            presenter.OnUpdateSharingState += UpdateSharingState;
            presenter.OnUpdateShareAddReward += ShowRewardLauncher;
            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            presenter.OnUpdateSharingState -= UpdateSharingState;
            presenter.OnUpdateShareAddReward -= ShowRewardLauncher;

            EventDelegate.Remove(btnConfirm.OnClick, presenter.RequestShareCharacterRewardGet);
        }

        protected override void OnShow(IUIData data = null)
        {
            thumbnail.Set(presenter.GetThumbnail());
            UICharacterShareWaiting.ISharingRewardData sharingRewardData = presenter.GetSharingRewardData();
            float totalTime = sharingRewardData == null ? 0f : sharingRewardData.GetTotalTime();
            int totalZeny = sharingRewardData == null ? 0 : sharingRewardData.GetZeny();
            int totalExp = sharingRewardData == null ? 0 : sharingRewardData.GetExp();
            int totalJobExp = sharingRewardData == null ? 0 : sharingRewardData.GetJobExp();
            rewardItems = sharingRewardData?.GetRewardItems();

            zeny.SetValue(RewardType.Zeny, totalZeny);
            exp.SetValue(RewardType.LevelExp, totalExp);
            jobExp.SetValue(RewardType.JobExp, totalJobExp);

            wrapper.Resize(rewardItems == null ? 0 : rewardItems.Length);

            var span = totalTime.ToTimeSpan();
            int totalHours = (int)span.TotalHours;
            int minutes = span.Minutes;

            labelTotalTime.text = LocalizeKey._10401.ToText() // 누적 전투 시간 {HOURS}:{MINUTES}
                .Replace(ReplaceKey.HOURS, totalHours.ToString("00"))
                .Replace(ReplaceKey.MINUTES, minutes.ToString("00"));
        }

        protected override void OnHide()
        {           
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._10400; // 셰어 보상 정산
            btnConfirm.LocalKey = LocalizeKey._10402; // 확인
            labelNotice.LocalKey = LocalizeKey._10403; // 쉐어 보상 버프는 정산 시간을 기준으로 적용됩니다.
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIRewardItem ui = go.GetComponent<UIRewardItem>();
            ui.SetData(rewardItems[index]);
        }

        private void UpdateSharingState()
        {
            SharingModel.SharingState state = presenter.GetSharingState();
            if (state == SharingModel.SharingState.None)
            {
                HideUI();
            }
        }

        private void ShowRewardLauncher(bool isZeny, bool isBaseExp, bool isJobExp)
        {
            if(isZeny)
                zeny.Launch();

            if(isBaseExp)
                exp.Launch();

            if(isJobExp)
                jobExp.Launch();
        }

        private void HideUI()
        {
            UI.Close<UICharacterShareReward>();
        }
    }
}