using UnityEngine;

namespace Ragnarok
{
    public class UIWorldBossAlarm : UICanvas, IWorldBossAlarmCanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] GameObject goActive;
        [SerializeField] UIMonsterIcon monster;
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UIButtonHelper btnMove;
        [SerializeField] UISlider progressBar;
        [SerializeField] UILabelHelper labelDungeon;

        WorldBossAlarmPresenter presenter;

        protected override void OnInit()
        {
            presenter = new WorldBossAlarmPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(btnMove.OnClick, presenter.OnClickedBtnMove);
            EventDelegate.Add(btnClose.OnClick, presenter.OnClickedClose);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnMove.OnClick, presenter.OnClickedBtnMove);
            EventDelegate.Remove(btnClose.OnClick, presenter.OnClickedClose);

            if (presenter != null)
                presenter = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.SetView();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            btnMove.LocalKey = LocalizeKey._7800; // 즉시 이동
            labelDungeon.LocalKey = LocalizeKey._7801; // 무한의 공간
        }

        void IWorldBossAlarmCanvas.SetActive(bool isActive)
        {
            goActive.SetActive(isActive);
        }

        void IWorldBossAlarmCanvas.SetMonster(MonsterInfo info)
        {
            monster.SetData(info);
        }

        void IWorldBossAlarmCanvas.SetProgress(float value)
        {
            progressBar.value = value;
        }

        bool IWorldBossAlarmCanvas.IsActive()
        {
            return goActive.activeSelf;
        }
    }
}
