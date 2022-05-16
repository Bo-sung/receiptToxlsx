using UnityEngine;

namespace Ragnarok
{
    public class UIResultWorldBossClear : UICanvas
    {
        protected override UIType uiType => UIType.Hide;

        [SerializeField] GameObject[] resultPanelRoots;
        [SerializeField] UILabelHelper[] labelRank;
        [SerializeField] UILabelHelper[] labelName;
        [SerializeField] UILabelHelper[] labelDamage;
        [SerializeField] UILabelHelper labelNoti;
        [SerializeField] UILabelHelper labelReward;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIWorldBossRankSlot[] clearWorldBossRankSlots;
        [SerializeField] UIWorldBossRankSlot clearMyRankSlot;
        [SerializeField] UIWorldBossRankSlot[] resultWorldBossRankSlots;
        [SerializeField] UIWorldBossRankSlot resultMyRankSlot;
        [SerializeField] GameObject[] titles;

        public event System.Action OnExit;

        protected override void OnInit()
        {
            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelRank[0].LocalKey = labelRank[1].LocalKey = LocalizeKey._7602; // 등수
            labelName[0].LocalKey = labelName[1].LocalKey = LocalizeKey._7603; // 유저
            labelDamage[0].LocalKey = labelDamage[1].LocalKey = LocalizeKey._7604; // 피해량(%)
            labelNoti.LocalKey = LocalizeKey._7605; // 보상은 우편함으로 지급됩니다.
            labelReward.LocalKey = LocalizeKey._2204;
            btnExit.LocalKey = LocalizeKey._7606; // 돌아가기
        }

        public void Show(int ranking, string name, int damage, int maxHp, bool isClear, WorldBossRankPacket[] ranks, RewardData[] rewards, RewardData playerReward)
        {
            Show();

            resultPanelRoots[0].SetActive(isClear);
            resultPanelRoots[1].SetActive(!isClear);
            titles[0].SetActive(isClear);
            titles[1].SetActive(!isClear);

            if (isClear)
            {
                for (int i = 0; i < clearWorldBossRankSlots.Length; i++)
                {
                    if (ranks != null && i < ranks.Length)
                    {
                        clearWorldBossRankSlots[i].SetActive(true);
                        clearWorldBossRankSlots[i].Set(ranks[i].ranking, ranks[i].char_name, ranks[i].score, maxHp, rewards[i]);
                    }
                    else
                    {
                        clearWorldBossRankSlots[i].SetActive(false);
                    }
                }

                clearMyRankSlot.Set(ranking, name, damage, maxHp, playerReward);
            }
            else
            {
                for (int i = 0; i < resultWorldBossRankSlots.Length; i++)
                {
                    if (ranks != null && i < ranks.Length)
                    {
                        resultWorldBossRankSlots[i].SetActive(true);
                        resultWorldBossRankSlots[i].Set(ranks[i].ranking, ranks[i].char_name, ranks[i].score, maxHp, null);
                    }
                    else
                    {
                        resultWorldBossRankSlots[i].SetActive(false);
                    }
                }

                resultMyRankSlot.Set(ranking, name, damage, maxHp, null);
            }
        }

        void OnClickedBtnExit()
        {
            OnExit?.Invoke();
            CloseUI();
        }

        private void CloseUI()
        {
            UI.Close<UIResultWorldBossClear>();
        }

        protected override void OnBack()
        {
            OnClickedBtnExit();
        }
    }
}