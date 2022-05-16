using Ragnarok.View;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDarkTreeRewardSelect"/>
    /// </summary>
    public class DarkTreeRewardSelectPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly InventoryModel inventoryModel;

        // <!-- Repositories --!>
        private readonly DarkTreeRewardDataManager darkTreeRewardDataRepo;
        private readonly DarkTreeRewardData[] arrData;

        // <!-- Event --!>
        public event System.Action OnSelectReward;
        public event System.Action OnFinished;

        private int selectedRewardId;

        public DarkTreeRewardSelectPresenter()
        {
            inventoryModel = Entity.player.Inventory;
            darkTreeRewardDataRepo = DarkTreeRewardDataManager.Instance;
            arrData = darkTreeRewardDataRepo.GetArrayData();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize()
        {
            int currentRewardId = GetCurrentRewardId();
            SetSelectRewardId(currentRewardId);
        }

        /// <summary>
        /// 보상 정보 목록 가져오기
        /// </summary>
        public DarkTreeRewardElement.IInput[] GetArrayData()
        {
            return arrData;
        }

        /// <summary>
        /// RewardId 선택
        /// </summary>
        public void SetSelectRewardId(int selectedRewardId)
        {
            if (this.selectedRewardId == selectedRewardId)
                return;

            this.selectedRewardId = selectedRewardId;
            OnSelectReward?.Invoke();
        }

        /// <summary>
        /// 헌재 선택한 데이터
        /// </summary>
        public DarkTreeRewardElement.IInput GetSelectedData()
        {
            return darkTreeRewardDataRepo.Get(selectedRewardId);
        }

        /// <summary>
        /// 선택한 보상으로 서버 호출
        /// </summary>
        public void RequestSelectReward()
        {
            AsyncRequestSelectReward().WrapNetworkErrors();
        }

        /// <summary>
        /// 현재 선택한 보상
        /// </summary>
        private int GetCurrentRewardId()
        {
            // 이미 선택된 보상이 있을 경우
            int currentRewardId = inventoryModel.DarkTree.GetSelectedRewardId();
            if (currentRewardId > 0)
                return currentRewardId;

            // 첫번째 rewardId
            if (arrData == null || arrData.Length == 0)
                return 0;

            return arrData[0].Id;
        }

        /// <summary>
        /// 서버 호출 (어둠의 나무 보상 선택)
        /// </summary>
        private async Task AsyncRequestSelectReward()
        {
            if (inventoryModel.DarkTree.GetSelectedRewardId() == selectedRewardId)
            {
                OnFinished?.Invoke();
                return;
            }

            await inventoryModel.RequestDarkTreeSelectReward(selectedRewardId);
            OnFinished?.Invoke();
        }
    }
}