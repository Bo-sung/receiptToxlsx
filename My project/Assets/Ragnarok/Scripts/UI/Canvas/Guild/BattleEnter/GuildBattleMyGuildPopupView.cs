using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GuildBattleMyGuildPopupView : PopupView
    {
        private const int TAB_DEFENSE = 0; // 수비 진형
        private const int TAB_RANK = 1; // 내 길드원 랭킹

        [SerializeField] UITabHelper tab;
        [SerializeField] MyGuildBattleDefenseView myGuildBattleDefenseView;
        [SerializeField] MyGuildBattleRankView myGuildBattleRankView;

        public event System.Action OnShowMyDefenseInfo;
        public event System.Action OnShowMyRankInfo;
        public event System.Action<int, int> OnSelectCharacterInfo;

        protected override void Awake()
        {
            base.Awake();

            tab.OnSelect += OnSelectTab;
            myGuildBattleRankView.OnSelectInfo += OnRankSelect;

            MainTitleLocalKey = LocalizeKey._33738; // 내 길드 정보
            ConfirmLocalKey = LocalizeKey._1; // 확인
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            tab.OnSelect -= OnSelectTab;
            myGuildBattleRankView.OnSelectInfo -= OnRankSelect;
        }

        protected override void OnLocalize()
        {
            tab[TAB_DEFENSE].LocalKey = LocalizeKey._33739; // 수비 진형
            tab[TAB_RANK].LocalKey = LocalizeKey._33740; // 내 길드원 랭킹
        }

        public override void Show()
        {
            base.Show();

            tab[TAB_DEFENSE].Set(false, false);
            tab.Value = TAB_DEFENSE;
        }

        public override void Hide()
        {
            base.Hide();

            myGuildBattleDefenseView.Hide();
            myGuildBattleRankView.Hide();
        }

        void OnSelectTab(int index)
        {
            myGuildBattleDefenseView.SetActive(index == TAB_DEFENSE);
            myGuildBattleRankView.SetActive(index == TAB_RANK);

            switch (index)
            {
                case TAB_DEFENSE:
                    OnShowMyDefenseInfo?.Invoke();
                    break;

                case TAB_RANK:
                    OnShowMyRankInfo?.Invoke();
                    break;
            }
        }

        void OnRankSelect(UIGuildBattleMyRankElement.IInput input)
        {
            OnSelectCharacterInfo?.Invoke(input.UID, input.CID);
        }

        /// <summary>
        /// 내 길드 세팅
        /// </summary>
        public void SetMyDefenseInfo(UISingleGuildBattleElement.IInput input, CupetModel[] leftCupets, CupetModel[] rightCupets)
        {
            myGuildBattleDefenseView.SetData(input, leftCupets, rightCupets);
        }

        /// <summary>
        /// 내 길드원 랭킹 세팅
        /// </summary>
        public void SetMyRankInfo(UIGuildBattleMyRankElement.IInput[] inputs)
        {
            myGuildBattleRankView.SetData(inputs);
        }
    }
}