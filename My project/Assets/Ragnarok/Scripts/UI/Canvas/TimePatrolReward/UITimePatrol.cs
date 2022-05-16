using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UITimePatrol : UICanvas, TutorialTimePatrolOpen.IEnterImpl
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SelectPopupView selectPopupView;
        [SerializeField] TimePatrolView timePatrolView;

        TimePatrolPresenter presenter;

        private int curLevel;

        protected override void OnInit()
        {
            presenter = new TimePatrolPresenter();

            selectPopupView.OnExit += OnBack;
            selectPopupView.OnCancel += OnBack;
            selectPopupView.OnConfirm += OnEnterTimePatrol;

            timePatrolView.OnDownLevel += OnDownLevel;
            timePatrolView.OnUpLevel += OnUpLevel;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            selectPopupView.OnExit -= OnBack;
            selectPopupView.OnCancel -= OnBack;
            selectPopupView.OnConfirm -= OnEnterTimePatrol;

            timePatrolView.OnDownLevel -= OnDownLevel;
            timePatrolView.OnUpLevel -= OnUpLevel;
        }

        protected override void OnShow(IUIData data = null)
        {
            int finalTimePatrolLevel = presenter.GetFinalTimePatrolLevel();
            int lastTimePatrolLevel = presenter.GetLastEnterTimePatrolLevel();
            timePatrolView.Init(finalTimePatrolLevel, lastTimePatrolLevel);
            SetTimePatrol(lastTimePatrolLevel);
            timePatrolView.SetRewardData(presenter.GetRewards());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            selectPopupView.MainTitleLocalKey = LocalizeKey._48281; // 타임패트롤
            selectPopupView.CancelLocalKey = LocalizeKey._2; // 취소
            selectPopupView.ConfirmLocalKey = LocalizeKey._48235; // 입장
        }

        void SetTimePatrol(int level)
        {
            int finalLevel = presenter.GetFinalTimePatrolLevel();
            curLevel = Mathf.Clamp(level, 1, finalLevel);
            int zone = presenter.GetPatrolZone(curLevel);          
            timePatrolView.Show();
            timePatrolView.Set(curLevel, zone, 0, 0);           
        }

        private void OnDownLevel(int level)
        {
            SetTimePatrol(--level);
        }

        private void OnUpLevel(int level)
        {
            SetTimePatrol(++level);
        }

        private void OnEnterTimePatrol()
        {
            isSelectedBtnEnter = true;
            presenter.StartTimePatrol(curLevel);
        }

        private bool isSelectedBtnEnter;
        public bool IsSelectedBtnEnter()
        {
            if (isSelectedBtnEnter)
            {
                isSelectedBtnEnter = false;
                return true;
            }

            return false;
        }

        #region Tutorial
        UIWidget TutorialTimePatrolOpen.IEnterImpl.GetLevelSelectWidget()
        {
            return timePatrolView.GetLevelWidget();
        }        

        UIWidget TutorialTimePatrolOpen.IEnterImpl.GetBtnEnterWidget()
        {
            return selectPopupView.GetBtnConfirm();
        }

        bool TutorialTimePatrolOpen.IEnterImpl.IsSelectedBtnEnter()
        {
            return IsSelectedBtnEnter();
        }
        #endregion
    }
}