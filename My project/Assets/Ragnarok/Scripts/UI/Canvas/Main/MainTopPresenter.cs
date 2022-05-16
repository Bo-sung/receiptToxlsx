using System;

namespace Ragnarok
{
    class MainTopPresenter : ViewPresenter
    {
        public event Action OnUpdateCharacterJob;
        public event Action OnUpdateCharacterBaseLevel;
        public event Action OnUpdateCharacterBaseLevelExp;
        public event Action OnUpdateCharacterJobLevel;
        public event Action OnUpdateCharacterJobLevelExp;
        public event Action OnUpdateGoodsZeny;
        public event Action OnUpdateGoodsCatCoin;
        public event Action OnUpdateGoodsRoPoint;

        private readonly CharacterModel characterModel;
        private readonly GoodsModel goodsModel;
        private readonly ExpDataManager expDataManager;
        private readonly BattleManager battleManager;
        private readonly ConnectionManager connectionManager;

        public MainTopPresenter()
        {
            characterModel = Entity.player.Character;
            goodsModel = Entity.player.Goods;
            expDataManager = ExpDataManager.Instance;
            battleManager = BattleManager.Instance;
            connectionManager = ConnectionManager.Instance;
        }

        public override void AddEvent()
        {
            characterModel.OnChangedJob += InvokeUpdateChangedJob;
            characterModel.OnUpdateLevel += InvokeUpdateCharacterBaseLevel;
            characterModel.OnUpdateLevelExp += InvokeUpdateCharacterBaseLevelExp;
            characterModel.OnUpdateJobLevel += InvokeUpdateCharacterJobLevel;
            characterModel.OnUpdateJobExp += InvokeUpdateCharacterJobLevelExp;
            goodsModel.OnUpdateZeny += InvokeUpdateGoodsZeny;
            goodsModel.OnUpdateCatCoin += InvokeUpdateGoodsCatCoin;
            goodsModel.OnUpdateRoPoint += InvokeUpdateGoodsRoPoint;
        }

        public override void RemoveEvent()
        {
            characterModel.OnChangedJob -= InvokeUpdateChangedJob;
            characterModel.OnUpdateLevel -= InvokeUpdateCharacterBaseLevel;
            characterModel.OnUpdateLevelExp -= InvokeUpdateCharacterBaseLevelExp;
            characterModel.OnUpdateJobLevel -= InvokeUpdateCharacterJobLevel;
            characterModel.OnUpdateJobExp -= InvokeUpdateCharacterJobLevelExp;
            goodsModel.OnUpdateZeny -= InvokeUpdateGoodsZeny;
            goodsModel.OnUpdateCatCoin -= InvokeUpdateGoodsCatCoin;
            goodsModel.OnUpdateRoPoint -= InvokeUpdateGoodsRoPoint;
        }

        void InvokeUpdateChangedJob(bool isInit)
        {
            OnUpdateCharacterJob?.Invoke();
        }

        void InvokeUpdateCharacterBaseLevel(int level)
        {
            OnUpdateCharacterBaseLevel?.Invoke();
        }

        void InvokeUpdateCharacterBaseLevelExp(int exp)
        {
            OnUpdateCharacterBaseLevelExp?.Invoke();
        }

        void InvokeUpdateCharacterJobLevel(int level)
        {
            OnUpdateCharacterJobLevel?.Invoke();
        }

        void InvokeUpdateCharacterJobLevelExp(long exp)
        {
            OnUpdateCharacterJobLevelExp?.Invoke();
        }

        void InvokeUpdateGoodsZeny(long value)
        {
            OnUpdateGoodsZeny?.Invoke();
        }

        void InvokeUpdateGoodsCatCoin(long value)
        {
            OnUpdateGoodsCatCoin?.Invoke();
        }

        void InvokeUpdateGoodsRoPoint(long value)
        {
            OnUpdateGoodsRoPoint?.Invoke();
        }

        /// <summary>
        /// 캐릭터 직업
        /// </summary>
        public Job GetCharacterJob()
        {
            return characterModel.Job;
        }

        /// <summary>
        /// 캐릭터 Base레벨
        /// </summary>
        public int GetCharacterBaseLevel()
        {
            return characterModel.Level;
        }

        /// <summary>
        /// 캐릭터 Base레벨 경험치 정보
        /// </summary>
        public ExpDataManager.Output GetCharacterBaseLevelExpInfo()
        {
            return expDataManager.Get(characterModel.LevelExp, ExpDataManager.ExpType.CharacterBase);
        }

        /// <summary>
        /// 캐릭터 Job레벨
        /// </summary>
        public int GetCharacterJobLevel()
        {
            return characterModel.JobLevel;
        }

        /// <summary>
        /// 캐릭터 Job레벨 경험치 정보
        /// </summary>
        public ExpDataManager.Output GetCharacterJobLevelExpInfo()
        {
            return expDataManager.Get(characterModel.JobLevelExp, ExpDataManager.ExpType.CharacterJob);
        }

        /// <summary>
        /// 제니
        /// </summary>
        public long GetZeny()
        {
            return goodsModel.Zeny;
        }

        /// <summary>
        /// 냥다래
        /// </summary>
        public long GetCatCoin()
        {
            return goodsModel.CatCoin;
        }

        /// <summary>
        /// RoPoint
        /// </summary>
        public long GetRoPoint()
        {
            return goodsModel.RoPoint;
        }

        public void OnSelectJob()
        {
            if (battleManager.Mode.IsDungeon())
                return;

            UI.ShortCut<UICharacterInfo>();
        }

        public int GetMaxJobLevel()
        {
            return BasisType.MAX_JOB_LEVEL_STAGE_BY_SERVER.GetInt(connectionManager.GetSelectServerGroupId());
        }
    }
}
