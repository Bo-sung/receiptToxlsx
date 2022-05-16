using UnityEngine;

namespace Ragnarok
{
    public class UIStageInfoPopup : UICanvas, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Hide | UIType.Back;

        [SerializeField] UIButtonHelper backgound;
        [SerializeField] UILabelHelper labelStageName;
        [SerializeField] UIButtonHelper btnEnter;
        [SerializeField] UILabelHelper labelMonster;
        [SerializeField] UILabelHelper labelReward;
        [SerializeField] UIButtonHelper btnExit;

        [SerializeField] SuperScrollListWrapper wrapperMonster;
        [SerializeField] GameObject prefabMonster;

        [SerializeField] SuperScrollListWrapper wrapperReward;
        [SerializeField] GameObject prefabReward;

        private MonsterInfo[] monsterInfos;
        private (RewardInfo reward, bool isBoss)[] rewardInfos;

        protected override void OnInit()
        {
            wrapperMonster.SetRefreshCallback(OnItemRefreshMonster);
            wrapperMonster.SpawnNewList(prefabMonster, 0, 0);

            wrapperReward.SetRefreshCallback(OnItemRefreshReward);
            wrapperReward.SpawnNewList(prefabReward, 0, 0);

            EventDelegate.Add(backgound.OnClick, CloseUI);
            EventDelegate.Add(btnEnter.OnClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
        }

        protected override void OnHide()
        {
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(backgound.OnClick, CloseUI);
            EventDelegate.Remove(btnEnter.OnClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
        }

        protected override void OnLocalize()
        {
            labelMonster.LocalKey = LocalizeKey._8502; // 몬스터
            labelReward.LocalKey = LocalizeKey._8503; // 아이템
            btnEnter.LocalKey = LocalizeKey._8600; // 확인
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void Show(int stageId, bool isShowMvp = true)
        {
            Show(StageDataManager.Instance.Get(stageId), isShowMvp);
        }

        public void Show(StageData currentData, bool isShowMvp = true)
        {
            MvpDataManager mvpDataRepo = MvpDataManager.Instance;
            MonsterDataManager monsterDataRepo = MonsterDataManager.Instance;

            IStageInfoPopupData stageInfoPopupData = currentData;

            Buffer<MonsterInfo> monsterBuffer = new Buffer<MonsterInfo>();
            Buffer<(RewardInfo reward, bool isBoss)> rewardBuffer = new Buffer<(RewardInfo reward, bool isBoss)>();

            // MvpMonster
            if (isShowMvp)
            {
                MvpData[] arrMvpData = mvpDataRepo.GetArrayGroup(stageInfoPopupData.MvpMonsterGroupId);
                for (int i = 0; i < arrMvpData.Length; i++)
                {
                    MonsterInfo monsterInfo = GetMonsterInfo(monsterDataRepo, arrMvpData[i].monster_id, stageInfoPopupData.MvpMonsterLevel, isBoss: true);
                    if (monsterInfo == null)
                        continue;

                    monsterBuffer.Add(monsterInfo);
                }
            }

            //. NormalMonster
            for (int i = 0; i < stageInfoPopupData.MaxNormalMonsterIndex; i++)
            {
                MonsterInfo monsterInfo = GetMonsterInfo(monsterDataRepo, stageInfoPopupData.GetNormalMonsterId(i), stageInfoPopupData.NormalMonsterLevel, isBoss: false);
                if (monsterInfo == null)
                    continue;

                monsterBuffer.Add(monsterInfo);
            }

            // MvpMonster - Reward
            if (isShowMvp)
            {
                for (int i = 0; i < stageInfoPopupData.MaxMvpMonsterRewardIndex; i++)
                {
                    int itemId = stageInfoPopupData.GetMvpMonsterRewardId(i);
                    if (itemId == 0)
                        continue;

                    rewardBuffer.Add((new RewardInfo(RewardType.Item, itemId, rewardCount: 1), isBoss: true));
                }
            }

            // NormalMonster - Reward
            for (int i = 0; i < stageInfoPopupData.MaxNormalMonsterRewardIndex; i++)
            {
                int itemId = stageInfoPopupData.GetNormalMonsterRewardId(i);
                if (itemId == 0)
                    continue;

                rewardBuffer.Add((new RewardInfo(RewardType.Item, itemId, rewardCount: 1), isBoss: false));
            }

            string stageName = currentData.name_id.ToText();

            Show(stageName, monsterBuffer.GetBuffer(isAutoRelease: true), rewardBuffer.GetBuffer(isAutoRelease: true));
        }

        private void Show(string stageName, MonsterInfo[] monsterInfos, (RewardInfo reward, bool isBoss)[] rewardInfos)
        {
            this.monsterInfos = monsterInfos;
            this.rewardInfos = rewardInfos;

            labelStageName.Text = stageName;

            wrapperMonster.Resize(monsterInfos == null ? 0 : monsterInfos.Length);
            wrapperReward.Resize(rewardInfos == null ? 0 : rewardInfos.Length);
        }

        /// <summary>
        /// 몬스터 정보
        /// </summary>
        private MonsterInfo GetMonsterInfo(MonsterDataManager monsterDataRepo, int monsterId, int monsterLevel, bool isBoss)
        {
            if (monsterId == 0)
                return null;

            MonsterData data = monsterDataRepo.Get(monsterId);
            if (data == null)
                return null;

            MonsterInfo info = new MonsterInfo(isBoss);
            info.SetData(data);
            info.SetLevel(monsterLevel);
            return info;
        }

        void OnItemRefreshMonster(GameObject go, int index)
        {
            UIMonsterIcon ui = go.GetComponent<UIMonsterIcon>();
            ui.SetData(monsterInfos[index]);
        }

        void OnItemRefreshReward(GameObject go, int index)
        {
            UIMonsterRewardSlot ui = go.GetComponent<UIMonsterRewardSlot>();
            ui.Set(rewardInfos[index]);
        }

        void CloseUI()
        {
            UI.Close<UIStageInfoPopup>();
        }
    }
}