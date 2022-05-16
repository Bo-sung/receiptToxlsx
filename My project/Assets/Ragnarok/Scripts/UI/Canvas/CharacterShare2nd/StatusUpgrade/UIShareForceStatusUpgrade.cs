using UnityEngine;

namespace Ragnarok
{
    public sealed class UIShareForceStatusUpgrade : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper labelOptionTitle;
        [SerializeField] UILabelValue before, after;
        [SerializeField] UIPressButton btnLeft, btnRight;
        [SerializeField] UILabel labelPlus;
        [SerializeField] UILabelValue statPoint, needPoint;
        [SerializeField] SwitchWidgetColor switchNeedPoint;
        [SerializeField] UIButtonHelper btnCancel, btnLevelUp;

        ShareForceStatusUpgradePresenter presenter;

        private DataGroup<ShareStatBuildUpData> dataGroup;
        private ShareStatBuildUpData beforeData;
        private ShareStatBuildUpData afterData;
        private int beforeLevel;
        private int beforeNeedPoint;
        private int max;
        private int cur;
        private int curStatPoint;

        protected override void OnInit()
        {
            presenter = new ShareForceStatusUpgradePresenter();

            EventDelegate.Add(btnLeft.onClick, OnClickedBtnLeft);
            EventDelegate.Add(btnRight.onClick, OnClickedBtnRight);
            EventDelegate.Add(btnCancel.OnClick, HideUI);
            EventDelegate.Add(btnLevelUp.OnClick, OnClickedBtnLevelUp);

            presenter.OnUpdateShareForceStatus += HideUI;
            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            presenter.OnUpdateShareForceStatus -= HideUI;

            EventDelegate.Remove(btnLeft.onClick, OnClickedBtnLeft);
            EventDelegate.Remove(btnRight.onClick, OnClickedBtnRight);
            EventDelegate.Remove(btnCancel.OnClick, HideUI);
            EventDelegate.Remove(btnLevelUp.OnClick, OnClickedBtnLevelUp);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
            dataGroup = null;
            beforeData = null;
            afterData = null;
            beforeLevel = 0;
            cur = 0;
            max = 0;
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._10275; // 스탯 강화
            statPoint.TitleKey = LocalizeKey._10276; // 현재 스탯 포인트
            needPoint.TitleKey = LocalizeKey._10277; // 필요 포인트
            btnCancel.LocalKey = LocalizeKey._10278; // 나가기
            btnLevelUp.LocalKey = LocalizeKey._10279; // 강화하기
        }

        void OnClickedBtnLeft()
        {
            if (cur > 1)
            {
                --cur;
            }
            else
            {
                // 누르고 있을 때에는 처리하지 않음
                if (btnLeft.IsPressing)
                    return;

                if (cur == max)
                    return;

                cur = max; // 최대치로 변경
            }

            Refresh();
        }

        void OnClickedBtnRight()
        {
            if (cur < max)
            {
                ++cur;
            }
            else
            {
                // 누르고 있을 때에는 처리하지 않음
                if (btnRight.IsPressing)
                    return;

                if (cur == 1)
                    return;

                cur = 1; // 최소치로 변경
            }

            Refresh();
        }

        void OnClickedBtnLevelUp()
        {
            presenter.RequestShareStatBuildUp(afterData.group, afterData.stat_lv);
        }

        public void SetGroup(int group)
        {
            dataGroup = presenter.Get(group);
            beforeLevel = presenter.GetLevel(group);
            beforeData = beforeLevel > 0 ? Find(beforeLevel) : dataGroup.First;
            beforeNeedPoint = beforeLevel > 0 ? beforeData.GetNeedPoint() : 0;
            max = dataGroup.Last.stat_lv - beforeLevel;
            cur = 1;
            curStatPoint = presenter.GetStatPoint();

            BattleOption beforeOption = beforeData.GetBattleOption();
            labelOptionTitle.Text = beforeOption.GetTitleText();

            before.Title = LocalizeKey._4102.ToText() // Lv. {LEVEL}
                .Replace(ReplaceKey.LEVEL, beforeLevel);
            before.Value = beforeLevel > 0 ? beforeOption.GetValueText() : string.Empty;
            statPoint.Value = curStatPoint.ToString("N0");

            Refresh();
        }

        private void Refresh()
        {
            int afterLevel = beforeLevel + cur;
            afterData = Find(afterLevel);
            int needStatPoint = afterData.GetNeedPoint() - beforeNeedPoint;
            BattleOption afterOption = afterData.GetBattleOption();
            after.Title = LocalizeKey._4102.ToText() // Lv. {LEVEL}
                .Replace(ReplaceKey.LEVEL, afterLevel);
            after.Value = afterOption.GetValueText();
            labelPlus.text = cur.ToString();
            needPoint.Value = needStatPoint.ToString("N0");

            bool canUpgrade = needStatPoint <= curStatPoint;
            switchNeedPoint.Switch(!canUpgrade);
            btnLevelUp.IsEnabled = canUpgrade;
        }

        private ShareStatBuildUpData Find(int level)
        {
            if (dataGroup == null)
                return null;

            foreach (var item in dataGroup)
            {
                if (item.stat_lv == level)
                    return item;
            }

            return null;
        }

        private void HideUI()
        {
            UI.Close<UIShareForceStatusUpgrade>();
        }

        public override bool Find()
        {
            bool baseResult = base.Find();
            bool result = false;

            if (switchNeedPoint == null)
                result = switchNeedPoint = needPoint.GetComponentInChildren<SwitchWidgetColor>();

            return baseResult || result;
        }
    }
}