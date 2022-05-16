using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildAttackFail : UICanvas
    {
        protected override UIType uiType => UIType.Hide;

        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UILabelHelper labelPreLevel, labelCurLevel;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UILabelHelper labelTitleBuff;
        [SerializeField] UIGuildAttackBuffResultList emperiumBuff;

        GuildAttackResultPresenter presenter;

        private System.Action onConfirm;

        protected override void OnInit()
        {
            presenter = new GuildAttackResultPresenter();

            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);

            presenter.RemoveEvent();

            onConfirm = null;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
            onConfirm = null;
        }

        protected override void OnLocalize()
        {
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
            labelDesc.LocalKey = LocalizeKey._38305; // 습격 방어에 실패하여 엠펠리움이 손상되었습니다.
            labelTitleBuff.LocalKey = LocalizeKey._38303; // 적용 중인 엠펠리움 버프
        }

        void OnClickedBtnConfirm()
        {
            onConfirm?.Invoke();
            CloseUI();
        }

        public void Show(int preLevel, int curLevel, System.Action onConfirm)
        {
            this.onConfirm = onConfirm;

            Show();

            string format = LocalizeKey._38300.ToText(); // Lv.{LEVEL}
            labelPreLevel.Text = format.Replace(ReplaceKey.LEVEL, preLevel);
            labelCurLevel.Text = format.Replace(ReplaceKey.LEVEL, curLevel);

            emperiumBuff.SetBuff(presenter.GetBattleOption(preLevel), presenter.GetBattleOption(curLevel));
        }

        private void CloseUI()
        {
            UI.Close<UIGuildAttackFail>();
        }
    }
}