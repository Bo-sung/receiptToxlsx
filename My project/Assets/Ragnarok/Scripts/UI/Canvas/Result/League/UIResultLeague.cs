using UnityEngine;

namespace Ragnarok
{
    public class UIResultLeague : UICanvas
    {
        protected override UIType uiType => UIType.Hide;
        [SerializeField] UIResultLeagueSlot before;
        [SerializeField] UIResultLeagueSlot after;
        [SerializeField] UILabelHelper labelChangePoint;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] GameObject goWin;
        [SerializeField] GameObject goLose;

        ResultLeaguePresenter presenter;

        protected override void OnInit()
        {
            presenter = new ResultLeaguePresenter();
            presenter.AddEvent();

            EventDelegate.Add(btnConfirm.OnClick, presenter.OnClickedBtnConfirm);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnConfirm.OnClick, presenter.OnClickedBtnConfirm);

            if (presenter != null)
                presenter = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            CorrectUIEntryAnimation();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            btnConfirm.LocalKey = LocalizeKey._47600; // 확인
        }

        public void Show(bool isWin, int beforePoint, int afterPoint, bool isShowButton)
        {
            Show();

            int chagePoint = Mathf.Abs(afterPoint - beforePoint); // 변화된 포인트
            bool isChangeTier = presenter.IsChangeTier(beforePoint, afterPoint); // 티어 변경 여부

            if (isWin)
            {
                SoundManager.Instance.PlayUISfx(Sfx.UI.BigSuccess);
            }

            goWin.SetActive(isWin);
            goLose.SetActive(!isWin);

            before.SetData(presenter.GetSlotInfo(beforePoint));
            after.SetData(presenter.GetSlotInfo(afterPoint));

            if (isWin)
            {
                labelChangePoint.Text = LocalizeKey._47606.ToText().Replace(ReplaceKey.VALUE, chagePoint); // [00B050]+{VALUE}[-]
                labelDesc.Text = isChangeTier ? LocalizeKey._47603.ToText() : LocalizeKey._47601.ToText(); // 대전에서 승리하여 등급이 상승했습니다. : 대전에서 승리하였습니다.
            }
            else
            {
                labelChangePoint.Text = LocalizeKey._47607.ToText().Replace(ReplaceKey.VALUE, chagePoint); // [C40F0F]-{VALUE}[-]
                labelDesc.Text = isChangeTier ? LocalizeKey._47604.ToText() : LocalizeKey._47602.ToText(); // 대전에서 패배하여 등급이 하락했습니다. : 대전에서 패배하였습니다.
            }

            btnConfirm.SetActive(isShowButton);
        }
    }
}
