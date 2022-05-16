using Ragnarok.View.Main;
using UnityEngine;

namespace Ragnarok
{
    public class UIMainTop : UICanvas
    {
        public enum MenuContent
        {
            Zeny = 1,
            Exp,
            JobExp,
            CatCoin,
            RoPoint,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIUserInfoView userInfoView;
        [SerializeField] UIWidget roPointWidget;

        MainTopPresenter presenter;

        public UIWidget RoPointWidget { get { return roPointWidget; } }

        protected override void OnInit()
        {
            presenter = new MainTopPresenter();

            userInfoView.OnSelectJob += OnSelectJob;
            presenter.OnUpdateCharacterJob += UpdateCharacterJob;
            presenter.OnUpdateCharacterBaseLevel += UpdateCharacterBaseLevel;
            presenter.OnUpdateCharacterBaseLevelExp += UpdateCharacterBaseLevelExp;
            presenter.OnUpdateCharacterJobLevel += UpdateCharacterJobLevel;
            presenter.OnUpdateCharacterJobLevelExp += UpdateCharacterJobLevelExp;
            presenter.OnUpdateGoodsZeny += UpdateGoodsZeny;
            presenter.OnUpdateGoodsCatCoin += UpdateGoodsCatCoin;
            presenter.OnUpdateGoodsRoPoint += UpdateGoodsRoPoint;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            userInfoView.OnSelectJob -= OnSelectJob;
            presenter.OnUpdateCharacterJob -= UpdateCharacterJob;
            presenter.OnUpdateCharacterBaseLevel -= UpdateCharacterBaseLevel;
            presenter.OnUpdateCharacterBaseLevelExp -= UpdateCharacterBaseLevelExp;
            presenter.OnUpdateCharacterJobLevel -= UpdateCharacterJobLevel;
            presenter.OnUpdateCharacterJobLevelExp -= UpdateCharacterJobLevelExp;
            presenter.OnUpdateGoodsZeny -= UpdateGoodsZeny;
            presenter.OnUpdateGoodsCatCoin -= UpdateGoodsCatCoin;
            presenter.OnUpdateGoodsRoPoint -= UpdateGoodsRoPoint; ;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateCharacterJob();
            UpdateCharacterBaseLevel();
            UpdateCharacterBaseLevelExp();
            UpdateCharacterJobLevel();
            UpdateCharacterJobLevelExp();
            UpdateGoodsZeny();
            UpdateGoodsCatCoin();
            UpdateGoodsRoPoint();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        private void UpdateCharacterJob()
        {
            Job job = presenter.GetCharacterJob();
            userInfoView.SetJobIcon(job.GetJobIcon());
        }

        private void UpdateCharacterBaseLevel()
        {
            userInfoView.SetBaseLevel(presenter.GetCharacterBaseLevel());
        }

        private void UpdateCharacterBaseLevelExp()
        {
            ExpDataManager.Output output = presenter.GetCharacterBaseLevelExpInfo();
            userInfoView.SetProgressBaseLevel(MathUtils.GetProgress(output.curExp, output.maxExp));
        }

        private void UpdateCharacterJobLevel()
        {
            userInfoView.SetJobLevel(presenter.GetCharacterJobLevel());
        }

        private void UpdateCharacterJobLevelExp()
        {
            ExpDataManager.Output output = presenter.GetCharacterJobLevelExpInfo();

            int addLevel = output.level - presenter.GetMaxJobLevel();
            // 최대 레벨 넘음
            if (addLevel > 0)
            {
                userInfoView.SetProgressJobLevel(1f);
                userInfoView.SetProgressAddJobLevel(MathUtils.GetProgress(output.curExp, output.maxExp));
                userInfoView.SetAddJobLevel(addLevel);
            }
            else
            {
                userInfoView.SetProgressJobLevel(MathUtils.GetProgress(output.curExp, output.maxExp));
                userInfoView.SetProgressAddJobLevel(0f);
                userInfoView.SetAddJobLevel(0);
            }
        }

        private void UpdateGoodsZeny()
        {
            userInfoView.SetZeny(presenter.GetZeny());
        }

        private void UpdateGoodsCatCoin()
        {
            userInfoView.SetCatCoin(presenter.GetCatCoin());
        }

        private void UpdateGoodsRoPoint()
        {
            userInfoView.SetRoPoint(presenter.GetRoPoint());
        }

        public void Initialize(bool showCatCoin, bool showRoPoint)
        {
            userInfoView.SetActiveBtnCatCoin(showCatCoin);
            userInfoView.SetActiveBtnRoPoint(showRoPoint);
        }

        public void SetEnableButton(bool value)
        {
            userInfoView.SetEnableButton(value);
        }

        void OnSelectJob()
        {
            presenter.OnSelectJob();
        }

        public UIWidget GetWidget(MenuContent content)
        {
            return userInfoView.GetWidget(content);
        }

        public Vector3 GetPosition(MenuContent content)
        {
            UIWidget widget = GetWidget(content);
            return widget == null ? Vector3.zero : widget.cachedTransform.position;
        }
    }
}