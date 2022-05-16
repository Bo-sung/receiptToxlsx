using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GuildBattleEntryPopupView : SelectPopupView
    {
        [SerializeField] UISingleGuildBattleElement targetGuild;
        [SerializeField] UITurretReadyInfo leftTurret, rightTurret;
        [SerializeField] UILabelHelper labelBattle;
        [SerializeField] UILabelHelper labelNotice;

        [Header("Agent")]
        [SerializeField] UILabelHelper labelAgent;
        [SerializeField] UITextureHelper profile;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UIButton btnAgent;
        [SerializeField] UIButtonHelper btnUnequip;

        [Header("AttackCupet")]
        [SerializeField] UILabelHelper labelAttackCupet;
        [SerializeField] UITurretInfo myTurret;

        public event System.Action OnSelectAgent;
        public event System.Action OnUnselectAgent;
        public event System.Action<ICupetModel> OnSelectCupet;
        public event System.Action<ICupetModel> OnSelectOtherCupet;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnAgent.onClick, OnClickedBtnAgent);
            EventDelegate.Add(btnUnequip.OnClick, OnClickedBtnUnequip);

            myTurret.OnSelectCupet += OnTurretCupet;
            leftTurret.OnSelectCupet += OnTurretOtherCupet;
            rightTurret.OnSelectCupet += OnTurretOtherCupet;

            MainTitleLocalKey = LocalizeKey._33727; // 전투 준비
            ConfirmLocalKey = LocalizeKey._33728; // 전투
            CancelLocalKey = LocalizeKey._2; // 취소
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnAgent.onClick, OnClickedBtnAgent);
            EventDelegate.Remove(btnUnequip.OnClick, OnClickedBtnUnequip);

            myTurret.OnSelectCupet -= OnTurretCupet;
            leftTurret.OnSelectCupet -= OnTurretOtherCupet;
            rightTurret.OnSelectCupet -= OnTurretOtherCupet;
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            leftTurret.SetName(LocalizeKey._33805.ToText()); // (좌) 포링포탑
            rightTurret.SetName(LocalizeKey._33806.ToText()); // (우) 포링포탑

            labelBattle.LocalKey = LocalizeKey._33729; // 전투 정보
            labelAgent.LocalKey = LocalizeKey._33730; // 지원 동료
            labelAttackCupet.LocalKey = LocalizeKey._33731; // 공격 큐펫
            labelNotice.LocalKey = LocalizeKey._33732; // 한 번 사용한 지원 캐릭터는 중복 사용할 수 없습니다.
        }

        void OnClickedBtnAgent()
        {
            OnSelectAgent?.Invoke();
        }

        void OnClickedBtnUnequip()
        {
            OnUnselectAgent?.Invoke();
        }

        void OnTurretCupet(ICupetModel cupet)
        {
            OnSelectCupet?.Invoke(cupet);
        }

        void OnTurretOtherCupet(ICupetModel cupet)
        {
            OnSelectOtherCupet?.Invoke(cupet);
        }

        /// <summary>
        /// 타겟 길드 세팅
        /// </summary>
        public void SetTargetGuildData(UISingleGuildBattleElement.IInput input, CupetModel[] leftCupets, CupetModel[] rightCupets)
        {
            targetGuild.SetData(input);
            leftTurret.SetCupet(leftCupets);
            rightTurret.SetCupet(rightCupets);
        }

        /// <summary>
        /// 지원 동료 세팅
        /// </summary>
        public void UpdateAgent(bool hasAgent, string profileName, string jobIconName)
        {
            profile.SetJobProfile(profileName);
            jobIcon.SetJobIcon(jobIconName);

            btnUnequip.SetActive(hasAgent);
        }

        /// <summary>
        /// 큐펫 정보 세팅
        /// </summary>
        public void UpdateCupet(CupetModel[] cupetModels)
        {
            myTurret.SetCupet(cupetModels);
        }
    }
}