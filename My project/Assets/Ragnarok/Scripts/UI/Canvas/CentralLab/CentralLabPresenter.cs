using Ragnarok.View.Skill;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICentralLab"/>
    /// </summary>
    public class CentralLabPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GoodsModel goodsModel;
        private readonly CharacterModel characterModel;
        private readonly InventoryModel inventoryModel;
        private readonly DungeonModel dungeonModel;

        // <!-- Repositories --!>
        private readonly JobDataManager jobDataRepo;
        private readonly CentralLabDataManager centralLabDataRepo;
        private readonly CentralLabSkillDataManager centralLabSkillDataRepo;
        public readonly int centralLabMaxIndex;
        private readonly int dungoenTicketItemId;
        public readonly int maxFreeTicketCount;

        // <!-- Managers --!>
        private readonly BattleManager battleManager;

        // <!-- Event --!>
        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public event System.Action OnUpdateJobLevel;
        public event System.Action OnUpdateDungeonTicket;
        public event System.Action OnUpdateFreeTicketCount
        {
            add { dungeonModel.OnUpdateCentralLabTicket += value; }
            remove { dungeonModel.OnUpdateCentralLabTicket -= value; }
        }

        public event System.Action OnStartBattleSuccess;

        private Job cloneJob;
        private JobData cloneJobData;
        private bool isStartBattle;

        public CentralLabPresenter()
        {
            goodsModel = Entity.player.Goods;
            characterModel = Entity.player.Character;
            inventoryModel = Entity.player.Inventory;
            dungeonModel = Entity.player.Dungeon;

            jobDataRepo = JobDataManager.Instance;
            centralLabDataRepo = CentralLabDataManager.Instance;
            centralLabSkillDataRepo = CentralLabSkillDataManager.Instance;
            centralLabMaxIndex = centralLabDataRepo.GetMaxIndex();
            dungoenTicketItemId = BasisItem.DungeonClearTicket.GetID();
            maxFreeTicketCount = dungeonModel.GetFreeEntryMaxCount(DungeonType.CentralLab);

            battleManager = BattleManager.Instance;
        }

        public override void AddEvent()
        {
            characterModel.OnUpdateJobLevel += OnUpdateCharacterJobLevel;
            inventoryModel.AddItemEvent(dungoenTicketItemId, OnUpdateDungeonTicketItem);
            BattleManager.OnStart += OnStartBattle;
        }

        public override void RemoveEvent()
        {
            characterModel.OnUpdateJobLevel -= OnUpdateCharacterJobLevel;
            inventoryModel.RemoveItemEvent(dungoenTicketItemId, OnUpdateDungeonTicketItem);
            BattleManager.OnStart -= OnStartBattle;
        }

        void OnUpdateCharacterJobLevel(int jobLevel)
        {
            OnUpdateJobLevel?.Invoke();
        }

        void OnUpdateDungeonTicketItem()
        {
            OnUpdateDungeonTicket?.Invoke();
        }

        void OnStartBattle(BattleMode mode)
        {
            if (isStartBattle)
            {
                isStartBattle = false;
                OnStartBattleSuccess?.Invoke();
            }
        }

        public long GetZeny()
        {
            return goodsModel.Zeny;
        }

        public long GetCatCoin()
        {
            return goodsModel.CatCoin;
        }

        public Gender GetGender()
        {
            return characterModel.Gender;
        }

        public int GetJobLevel()
        {
            return characterModel.JobLevel;
        }

        public Job[] GetCloneJobs()
        {
            int jobGrade = characterModel.JobGrade();
            JobData[] arrData = jobDataRepo.GetJobs(jobGrade);
            return System.Array.ConvertAll(arrData, a => a.id.ToEnum<Job>());
        }

        /// <summary>
        /// 클리어 한 index 반환
        /// </summary>
        public int GetClearedDataIndex()
        {
            int clearedId = dungeonModel.GetClearedId(DungeonType.CentralLab); // 마지막으로 클리어한 중앙실험실 id
            if (clearedId == 0)
                return -1;

            return centralLabDataRepo.GetIndex(clearedId);
        }

        /// <summary>
        /// 인덱스에 해당하는 데이터 반환
        /// </summary>
        public CentralLabData GetCentralLabDataByIndex(int index)
        {
            return centralLabDataRepo.GetByIndex(index);
        }

        /// <summary>
        /// 클론 직업 선택
        /// </summary>
        public void SelectCloneJob(Job job)
        {
            cloneJob = job;
            cloneJobData = jobDataRepo.Get((int)cloneJob);
        }

        /// <summary>
        /// 현재 선택한 직업 이름
        /// </summary>
        public int GetCloneJobNameId()
        {
            if (cloneJobData == null)
                return 0;

            return cloneJobData.name_id;
        }

        /// <summary>
        /// 현재 선택한 직업 설명
        /// </summary>
        public int GetCloneJobDescriptionId()
        {
            if (cloneJobData == null)
                return 0;

            return cloneJobData.des_id;
        }

        /// <summary>
        /// 현재 선택한 첫번째 스테이지 스킬
        /// </summary>
        public UISkillInfoSelect.IInfo[] GetCloneSkills()
        {
            if (cloneJobData == null)
                return null;

            return centralLabSkillDataRepo.GetSkills(cloneJob, CentralLabSkillType.FirstSelect);
        }

        /// <summary>
        /// 던전 소탕권 개수
        /// </summary>
        public int GetDungeonTicketCount()
        {
            return inventoryModel.GetItemCount(dungoenTicketItemId);
        }

        /// <summary>
        /// 무료 입장 수
        /// </summary>
        public int GetFreeTicketCount()
        {
            return dungeonModel.GetFreeEntryCount(DungeonType.CentralLab);
        }

        /// <summary>
        /// 던전 소탕
        /// </summary>
        public void RequestFastClear(int index, bool isFree)
        {
            CentralLabData data = GetCentralLabDataByIndex(index);
            if (data == null)
            {
                Debug.LogError($"존재하지 않는 데이터: {nameof(index)} = {index}");
                return;
            }

            int labId = data.id;
            dungeonModel.RequestFastClearDungeon(DungeonType.CentralLab, labId, isFree).WrapNetworkErrors();
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartBattle(int index)
        {
            bool isCleard = index <= GetClearedDataIndex();
            if(isCleard && GetFreeTicketCount() > 0)
            {
                // 무료 소탕
                RequestFastClear(index, isFree: true);
                return;
            }

            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            if (battleManager.Mode == BattleMode.MultiMazeLobby)
            {
                UI.ShowToastPopup(LocalizeKey._90226.ToText()); // 미로섬에서는 입장할 수 없습니다.\n사냥 필드로 이동해주세요.
                return;
            }

            CentralLabData data = GetCentralLabDataByIndex(index);
            if (data == null)
            {
                Debug.LogError($"존재하지 않는 데이터: {nameof(index)} = {index}");
                return;
            }

            AsyncStartBattle(data.id).WrapUIErrors();
        }

        private async Task AsyncStartBattle(int labId)
        {
            if (!await dungeonModel.IsEnterDungeon(DungeonType.CentralLab))
                return;

            isStartBattle = true;
            battleManager.StartBattle(BattleMode.CentralLab, new CentralLabEntry.BattleInput { labId = labId, cloneJob = cloneJob });
        }
    }
}