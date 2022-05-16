using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIBattleFail : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        public enum ConfirmType
        {
            Rebirth = 1,
            Exit,
            Retry,
        }

        [SerializeField] UILabelHelper labelTitle, labelTime;
        [SerializeField] UIButtonHelper btnRebirth;

        public event System.Action OnConfirm;
        private CharacterEntity charaEntity;

        private ConfirmType confirmType;

        protected override void OnInit()
        {
            charaEntity = null;

            EventDelegate.Add(btnRebirth.OnClick, Confirm);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnRebirth.OnClick, Confirm);

            RemoveEvent();
            StopAllCoroutine();
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
            RemoveEvent();
            StopAllCoroutine();
        }

        protected override void OnLocalize()
        {
            labelTitle.Text = LocalizeKey._27001.ToText(); // 캐릭터가 사망했습니다.
            Refresh();
        }

        public void Confirm()
        {
            OnConfirm?.Invoke();
            Hide();
        }

        /// <summary>
        /// 해당 캐릭터가 레벨업으로 인해 부활하는 경우를 감지.
        /// </summary>
        private void SetPlayer(CharacterEntity charaEntity)
        {
            RemoveEvent();

            this.charaEntity = charaEntity;

            if (this.charaEntity != null)
            {
                this.charaEntity.Character.OnUpdateJobLevel += OnPlayerLevelUp;
            }
        }

        /// <summary>
        /// 대상 캐릭터가 레벨업으로 인해 부활하면, 부활버튼을 누른 것과 동일하게 작동
        /// </summary>
        private void OnPlayerLevelUp(int jobLevel)
        {
            RemoveEvent();

            StopAllCoroutine();
            Confirm();
        }

        private void RemoveEvent()
        {
            if (charaEntity == null)
                return;

            charaEntity.Character.OnUpdateJobLevel -= OnPlayerLevelUp;
            charaEntity = null;
        }

        public void Show(ConfirmType confirmType, CharacterEntity charaEntity = null)
        {
            int duration = Mathf.RoundToInt(BasisType.UNIT_DEATH_COOL_TIME.GetInt() * 0.001f);
            Show(confirmType, duration, charaEntity);
        }

        public void Show(ConfirmType confirmType, int duration, CharacterEntity charaEntity = null)
        {
            Show(confirmType, duration, isShowButton: true, charaEntity: charaEntity);
        }

        public void Show(ConfirmType confirmType, int duration, bool isShowButton, CharacterEntity charaEntity = null)
        {
            SetPlayer(charaEntity);

            this.confirmType = confirmType;
            btnRebirth.SetActive(isShowButton);

            Show();
            Refresh();

            StopAllCoroutine();
            Timing.RunCoroutine(YieldAutoConfirm(duration), gameObject);
        }

        private void Refresh()
        {
            switch (confirmType)
            {
                case ConfirmType.Rebirth:
                    btnRebirth.LocalKey = LocalizeKey._27003; // 바로 부활
                    break;

                case ConfirmType.Exit:
                    btnRebirth.LocalKey = LocalizeKey._27004; // 나가기
                    break;

                case ConfirmType.Retry:
                    btnRebirth.LocalKey = LocalizeKey._27006; // 다시 시도
                    break;
            }
        }

        private string GetTimeDescription()
        {
            switch (confirmType)
            {
                case ConfirmType.Rebirth:
                    return LocalizeKey._27002.ToText(); // {TIME}초 후 자동으로 부활합니다.

                case ConfirmType.Exit:
                    return LocalizeKey._27005.ToText(); // {TIME}초 후 자동으로 퇴장합니다.

                case ConfirmType.Retry:
                    return LocalizeKey._27007.ToText(); // {TIME}초 후 자동으로 재시도합니다.
            }

            return string.Empty;
        }

        private IEnumerator<float> YieldAutoConfirm(int seconds)
        {
            while (seconds > 0)
            {
                labelTime.Text = GetTimeDescription().Replace(ReplaceKey.TIME, seconds);

                yield return Timing.WaitForSeconds(1f);
                seconds -= 1;
            }

            Confirm();
        }

        /// <summary>
        /// 모든 코루틴 종료
        /// </summary>
        private void StopAllCoroutine()
        {
            Timing.KillCoroutines(gameObject);
        }
    }
}