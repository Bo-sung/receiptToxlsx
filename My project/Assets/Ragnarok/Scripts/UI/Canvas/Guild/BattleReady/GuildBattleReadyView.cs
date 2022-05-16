using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIGuildBattleReady"/>
    /// </summary>
    public sealed class GuildBattleReadyView : UIView
    {
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UITurretReadyInfo leftTurret, rightTurret;
        [SerializeField] UIButtonhWithGrayScale btnRequest;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UILabelValue requestState;

        public event System.Action OnSelectBtnRequest;
        public event System.Action OnSelectLeftTurretCupet;
        public event System.Action OnSelectRightTurretCupet;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnRequest.OnClick, OnClickedBtnRequest);
            leftTurret.OnSelectCupet += OnLeftTurretCupet;
            rightTurret.OnSelectCupet += OnRightTurretCupet;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnRequest.OnClick, OnClickedBtnRequest);
            leftTurret.OnSelectCupet -= OnLeftTurretCupet;
            rightTurret.OnSelectCupet -= OnRightTurretCupet;
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._33804; // 포링 포탑
            leftTurret.SetName(LocalizeKey._33805.ToText()); // (좌) 포링포탑
            rightTurret.SetName(LocalizeKey._33806.ToText()); // (우) 포링포탑
            labelNotice.LocalKey = LocalizeKey._33807; // 길드전 신청은 평일에만 가능하며, 길드장, 부길드장 권한입니다.\n양측 포팝에 큐펫이 등록되어 있어야 신청이 가능합니다.
            requestState.TitleKey = LocalizeKey._33814; // 신청 현황
        }

        /// <summary>
        /// 길드전 신청 버튼 클릭 이벤트
        /// </summary>
        void OnClickedBtnRequest()
        {
            OnSelectBtnRequest?.Invoke();
        }

        void OnLeftTurretCupet(ICupetModel cupet)
        {
            OnSelectLeftTurretCupet?.Invoke();
        }

        void OnRightTurretCupet(ICupetModel cupet)
        {
            OnSelectRightTurretCupet?.Invoke();
        }

        public void SetLeftTurret(CupetModel[] cupetModels)
        {
            leftTurret.SetCupet(cupetModels);
        }

        public void SetRightTurret(CupetModel[] cupetModels)
        {
            rightTurret.SetCupet(cupetModels);
        }

        /// <summary>
        /// 길드전 신청 버튼 상태
        /// </summary>
        public void SetBtnRequest(bool isReqeust)
        {
            // 길드전 신청은 길드장, 부길드장만 가능
            if (isReqeust)
            {
                btnRequest.IsEnabled = true;
                btnRequest.SetMode(UIGraySprite.SpriteMode.None);
            }
            else
            {
                btnRequest.IsEnabled = false;
                btnRequest.SetMode(UIGraySprite.SpriteMode.Grayscale);
            }
        }

        /// <summary>
        /// 길드전 신청 현황
        /// </summary>
        public void SetRequestState(bool isRequest)
        {
            requestState.ValueKey = isRequest ? LocalizeKey._33816 : LocalizeKey._33815; // [4383E3]신청 완료[-] : [D4585B]미신청[-]
            btnRequest.LocalKey = isRequest ? LocalizeKey._33817 : LocalizeKey._33808; // 설정 변경 : 길드전 신청
        }
    }
}