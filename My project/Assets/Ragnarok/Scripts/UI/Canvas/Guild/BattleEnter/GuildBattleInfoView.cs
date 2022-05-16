using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GuildBattleInfoView : UIView
    {
        [Header("Turret")]
        [SerializeField] UILabelHelper labelTurret;
        [SerializeField] UITurretInfo myTurret;

        [Header("Buff")]
        [SerializeField] UILabelHelper labelHeader;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GuildBattleBuffElement element;
        [SerializeField] UILabelHelper labelNoData;

        SuperWrapContent<GuildBattleBuffElement, GuildBattleBuffElement.IInput> wrapContent;

        public event System.Action<ICupetModel> OnSelectTurretCupet;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<GuildBattleBuffElement, GuildBattleBuffElement.IInput>(element);
            myTurret.OnSelectCupet += OnTurretCupet;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            myTurret.OnSelectCupet -= OnTurretCupet;
        }

        protected override void OnLocalize()
        {
            labelTurret.LocalKey = LocalizeKey._33718; // 전투 포탑
            labelHeader.LocalKey = LocalizeKey._33719; // 버프 효과
            labelNoData.LocalKey = LocalizeKey._33720; // 적용 중인 버프 효과가 없습니다.
        }

        void OnTurretCupet(ICupetModel cupet)
        {
            OnSelectTurretCupet?.Invoke(cupet);
        }

        /// <summary>
        /// 큐펫 정보 세팅
        /// </summary>
        public void UpdateCupet(CupetModel[] cupetModels)
        {
            myTurret.SetCupet(cupetModels);
        }

        /// <summary>
        /// 버프 정보 세팅
        /// </summary>
        public void SetBuff(GuildBattleBuffElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);

            int length = inputs == null ? 0 : inputs.Length;
            labelNoData.SetActive(length == 0);
        }
    }
}