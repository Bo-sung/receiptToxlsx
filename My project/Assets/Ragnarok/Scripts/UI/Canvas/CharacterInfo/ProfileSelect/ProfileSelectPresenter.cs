using Ragnarok.View;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIProfileSelect"/>
    /// </summary>
    public class ProfileSelectPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly CharacterModel characterModel;
        private readonly ShopModel shopModel;

        // <!-- Repositories --!>
        private readonly ProfileDataManager profileDataRepo;
        private readonly BetterList<ProfileSelectElement.IInput> list;
        private readonly ProfileElement defaultProfile;

        // <!-- Event --!>
        public event System.Action OnSelectProfile;
        public event System.Action OnFinished;
        public event System.Action OnUpdateMileage
        {
            add { shopModel.OnUpdateShopMail += value; }
            remove { shopModel.OnUpdateShopMail -= value; }
        }

        private int selectedProfileId = -1;

        public ProfileSelectPresenter()
        {
            characterModel = Entity.player.Character;
            shopModel = Entity.player.ShopModel;
            profileDataRepo = ProfileDataManager.Instance;
            list = new Buffer<ProfileSelectElement.IInput>();
            defaultProfile = new ProfileElement();

            list.Add(defaultProfile); // Add Default

            // Add Data
            ProfileData[] arrayData = profileDataRepo.GetArrayData();
            if (arrayData != null)
            {
                for (int i = 0; i < arrayData.Length; i++)
                {
                    list.Add(arrayData[i]);
                }
            }

            RefreshDefaultProfile();
        }

        public override void AddEvent()
        {
            characterModel.OnChangedJob += OnChangedJob;
            characterModel.OnChangedGender += OnChangedGender;
        }

        public override void RemoveEvent()
        {
            characterModel.OnChangedJob -= OnChangedJob;
            characterModel.OnChangedGender -= OnChangedGender;
        }

        void OnChangedJob(bool isInit)
        {
            RefreshDefaultProfile();
        }

        void OnChangedGender()
        {
            RefreshDefaultProfile();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize()
        {
            SetSelectProfileId(characterModel.ProfileId);
        }

        /// <summary>
        /// 프로필 정보 목록 가져오기
        /// </summary>
        public ProfileSelectElement.IInput[] GetArrayData()
        {
            return list.ToArray();
        }

        /// <summary>
        /// ProfileId 선택
        /// </summary>
        public void SetSelectProfileId(int selectedProfileId)
        {
            if (this.selectedProfileId == selectedProfileId)
                return;

            this.selectedProfileId = selectedProfileId;
            OnSelectProfile?.Invoke();
        }

        /// <summary>
        /// 헌재 선택한 데이터
        /// </summary>
        public ProfileSelectElement.IInput GetSelectedData()
        {
            ProfileData profileData = profileDataRepo.Get(selectedProfileId);
            if (profileData == null)
                return defaultProfile;

            return profileData;
        }

        /// <summary>
        /// 마일리지 반환
        /// </summary>
        public int GetCurMileage()
        {
            return shopModel.Mileage;
        }

        /// <summary>
        /// 선택한 프로필로 서버 호출
        /// </summary>
        public void RequestSelectProfile()
        {
            AsyncRequestSelectProfile().WrapNetworkErrors();
        }

        /// <summary>
        /// 서버 호출 (프로필 선택)
        /// </summary>
        private async Task AsyncRequestSelectProfile()
        {
            if (characterModel.ProfileId == selectedProfileId)
            {
                OnFinished?.Invoke();
                return;
            }

            await characterModel.RequestChangeProfile(selectedProfileId);
            OnFinished?.Invoke();
        }

        private void RefreshDefaultProfile()
        {
            defaultProfile.Initialize(characterModel.Job, characterModel.Gender);
        }

        private class ProfileElement : ProfileSelectElement.IInput
        {
            public int Id => 0;
            public string ProfileName { get; private set; }
            public int NeedMileage => 0;

            public void Initialize(Job job, Gender gender)
            {
                ProfileName = job.GetJobProfile(gender);
            }
        }
    }
}