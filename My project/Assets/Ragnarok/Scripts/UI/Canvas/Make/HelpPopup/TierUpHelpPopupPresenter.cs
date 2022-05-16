using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UITierUpHelpPopup"/>
    /// </summary>
    public sealed class TierUpHelpPopupPresenter : ViewPresenter
    {
        private readonly BetterList<TierUpHelpElementInput> list;
        private readonly ConnectionManager connectionManager;

        public TierUpHelpPopupPresenter()
        {
            connectionManager = ConnectionManager.Instance;
            list = new BetterList<TierUpHelpElementInput>();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public UITierUpHelpElement.IInput[] GetArrayInfo()
        {
            if (list.size == 0)
            {
                int maxJobLevel = GetMaxJobLevel();
                List<int> keyList = BasisType.ITEM_TRANSCEND_JOB_LEVEL.GetKeyList();
                foreach (int key in keyList)
                {
                    int tier = key;
                    int needJobLevel = BasisType.ITEM_TRANSCEND_JOB_LEVEL.GetInt(tier);
                    bool isNeedUpdate = maxJobLevel < needJobLevel;

                    list.Add(new TierUpHelpElementInput(tier, needJobLevel, isNeedUpdate));

                    // 업데이트 예정인 데이터는 하나만 보여주기
                    if (isNeedUpdate)
                        break;
                }
            }

            return list.ToArray();
        }

        public int GetMaxJobLevel()
        {
            return BasisType.MAX_JOB_LEVEL_STAGE_BY_SERVER.GetInt(connectionManager.GetSelectServerGroupId());
        }

        private class TierUpHelpElementInput : UITierUpHelpElement.IInput
        {
            public int Tier { get; private set; }
            public int NeedJobLevel { get; private set; }
            public bool IsNeedUpdate { get; private set; }

            public TierUpHelpElementInput(int tier, int needJobLevel, bool isNeedUpdate)
            {
                Tier = tier;
                NeedJobLevel = needJobLevel;
                IsNeedUpdate = isNeedUpdate;
            }
        }
    }
}