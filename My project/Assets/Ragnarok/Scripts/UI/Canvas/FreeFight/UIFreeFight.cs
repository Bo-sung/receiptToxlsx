using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIFreeFight : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single;

        [SerializeField] TitleView titleView;
        [SerializeField] NpcView npcView;
        [SerializeField] FreeFightListView freeFightListView;
        [SerializeField] SimpleFreeFightHelpView simpleFreeFightHelpView;
        [SerializeField] SkillFreeFightHelpView skillFreeFightHelpView;
        [SerializeField] FreeFightEventType[] displayTypes;

        FreeFightPresenter presenter;

        protected override void OnInit()
        {
            presenter = new FreeFightPresenter();

            freeFightListView.OnHelp += ShowHelpView;

            presenter.OnUpdateZeny += UpdateZeny;
            presenter.OnUpdateCatCoin += UpdateCatCoin;
            presenter.OnEnterFreeFight += CloseUI;

            presenter.AddEvent();

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);
            npcView.ShowNotice(string.Empty);
        }

        protected override void OnClose()
        {
            presenter.Dispose();
            presenter.RemoveEvent();

            freeFightListView.OnHelp -= ShowHelpView;

            presenter.OnUpdateZeny -= UpdateZeny;
            presenter.OnUpdateCatCoin -= UpdateCatCoin;
            presenter.OnEnterFreeFight -= CloseUI;
        }

        protected override void OnShow(IUIData data = null)
        {
            npcView.PlayTalk();

            freeFightListView.SetData(presenter.GetElements(displayTypes));

            simpleFreeFightHelpView.Hide();
            skillFreeFightHelpView.Hide();

            UpdateZeny(presenter.GetZeny());
            UpdateCatCoin(presenter.GetCatCoin());

            presenter.Initialize();
        }

        protected override void OnHide()
        {
            presenter.Dispose();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (presenter != null)
                presenter.Dispose();
        }

        protected override void OnLocalize()
        {
            // Title 언어 세팅
            titleView.ShowTitle(LocalizeKey._40000.ToText()); // 난전

            // Npc 언어 세팅
            npcView.ShowNpcName(LocalizeKey._82001.ToText()); // 엘레나 볼코바
            npcView.AddTalkLocalKey(LocalizeKey._83001); // 실력을 입증하러 왔나? 싸움이라면 환영이다!

            string notice = LocalizeKey._40022 // 난전은 총 {COUNT} 라운드로 이루어져 있습니다.
                .ToText().Replace(ReplaceKey.COUNT, presenter.roundCount);
            npcView.ShowNotice(notice);
        }

        void ShowHelpView(string title, string[] times, UIFreeFightReward.IInput[] rewards, UIEventMazeSkill.IInput[] skills)
        {
            const int MAIN_TITLE_LOCAL_KEY = LocalizeKey._40021; // 난전 설명
            const int DESC_LOCAL_KEY = LocalizeKey._49511; // 난전 입장 시간

            if (skills.Length > 0)
            {
                skillFreeFightHelpView.SetText(MAIN_TITLE_LOCAL_KEY.ToText(), title, DESC_LOCAL_KEY.ToText());
                skillFreeFightHelpView.InitializeTime(times);
                skillFreeFightHelpView.InitializeReward(rewards);
                skillFreeFightHelpView.InitializeSkill(skills);
                skillFreeFightHelpView.Show();
            }
            else
            {
                simpleFreeFightHelpView.SetText(MAIN_TITLE_LOCAL_KEY.ToText(), title, DESC_LOCAL_KEY.ToText());
                simpleFreeFightHelpView.InitializeTime(times);
                simpleFreeFightHelpView.InitializeReward(rewards);
                simpleFreeFightHelpView.Show();
            }
        }

        /// <summary>
        /// 제니 업데이트
        /// </summary>
        private void UpdateZeny(long zeny)
        {
            titleView.ShowZeny(zeny);
        }

        /// <summary>
        /// 캣코인 업데이트
        /// </summary>
        private void UpdateCatCoin(long catCoin)
        {
            titleView.ShowCatCoin(catCoin);
        }

        /// <summary>
        /// 창 닫기
        /// </summary>
        private void CloseUI()
        {
            UI.Close<UIFreeFight>();
        }

        protected override void OnBack()
        {
            if (simpleFreeFightHelpView.IsShow)
            {
                simpleFreeFightHelpView.Hide();
                return;
            }

            if (skillFreeFightHelpView.IsShow)
            {
                skillFreeFightHelpView.Hide();
                return;
            }

            base.OnBack();
        }
    }
}