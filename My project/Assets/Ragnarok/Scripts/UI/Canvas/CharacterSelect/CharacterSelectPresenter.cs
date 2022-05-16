namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICharacterSelect"/>
    /// </summary>
    public class CharacterSelectPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly CharacterListModel characterListModel;
        private readonly CharacterModel characterModel;

        // <!-- Repositories --!>
        private readonly ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl;

        // <!-- Managers --!>
        private readonly ConnectionManager connectionManager;

        // <!-- Data --!>
        private CharacterSelectElement[] characterSelectElements;
        private CharacterEntity dummyUIEntity;
        private SimpleCharacterPacket selectedCharacter; // 선택한 캐릭터

        // <!-- Event --!>
        public event System.Action OnUpdateView;

        public event System.Action OnUpdateCharacter
        {
            add { characterListModel.OnUpdateCharacter += value; }
            remove { characterListModel.OnUpdateCharacter -= value; }
        }

        public CharacterSelectPresenter()
        {
            characterListModel = Entity.player.CharacterList;
            characterModel = Entity.player.Character;
            profileDataRepoImpl = ProfileDataManager.Instance;
            connectionManager = ConnectionManager.Instance;
            SetSelectCharacter(characterListModel.SelectedCharacterCid); // 접속중인 캐릭터 선택
        }

        public override void AddEvent()
        {
            characterListModel.OnCharacterDeleteWaiting += InvokeCharacterDeleteWaiting;
            characterListModel.OnCharacterDelete += InvokeCharacterDelete;
            characterListModel.OnCharacterDeleteCancel += InvokeCharacterDeleteCancel;
        }

        public override void RemoveEvent()
        {
            characterListModel.OnCharacterDeleteWaiting -= InvokeCharacterDeleteWaiting;
            characterListModel.OnCharacterDelete -= InvokeCharacterDelete;
            characterListModel.OnCharacterDeleteCancel -= InvokeCharacterDeleteCancel;
        }

        public void Dispose()
        {
            if (dummyUIEntity != null)
            {
                dummyUIEntity.ResetData();
                dummyUIEntity.Dispose();
            }
        }

        /// <summary>
        /// 캐릭터 목록 요청
        /// </summary>
        public void RequestCharacterList()
        {
            characterListModel.RequestCharacterList(isInitialize: true).WrapNetworkErrors();
        }

        /// <summary>
        /// 선택한 캐릭터 세팅
        /// </summary>
        public void SetSelectCharacter(int cid)
        {
            selectedCharacter = characterListModel.FindSimpleCharacter(cid);
        }

        /// <summary>
        /// 삭제 예약 여부
        /// </summary>
        public bool IsDeleteWaiting()
        {
            return selectedCharacter.IsDeleteWaiting;
        }

        /// <summary>
        /// 삭제가능까지 남은시간
        /// </summary>
        public RemainTime GetRemainTimeDeleteWaiting()
        {
            return selectedCharacter.RemainTimeDeleteWaiting;
        }

        /// <summary>
        /// 현재 플레이중인 캐릭터인지 여부
        /// </summary>
        public bool IsCurrentPlayingCharacter()
        {
            return characterModel.Cid == selectedCharacter.Cid;
        }

        /// <summary>
        /// 캐릭터 생성 UI 열기
        /// </summary>
        public void OnShowCharacterCreate()
        {
            UI.Show<UICharacterCreate>();
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void RequestJoinGame()
        {
            characterListModel.RequestGotoCharList(selectedCharacter.Cid).WrapNetworkErrors();
        }

        /// <summary>
        /// 삭제 예약 요청
        /// </summary>
        public async void RequestDeleteCharacterWaiting()
        {
            string title = LocalizeKey._5.ToText(); // 알람
            string description = LocalizeKey._90040.ToText(); // 캐릭터를 삭제하시겠습니까?\n\n[FF0000]※ 캐릭터는 24시간 후 부터 삭제 할 수 있으며 취소할 수 있습니다.\n삭제 후에는 모든 정보가 사라집니다.[-]"
            if (!await UI.SelectPopup(title, description))
                return;

            if (selectedCharacter.IsGuild)
            {
                description = LocalizeKey._90053.ToText(); // 길드탈퇴 또는 길드 해산 후 캐릭터 삭제가 가능합니다.
                UI.ConfirmPopup(title, description);
                return;
            }

            characterListModel.RequestDeleteCharacterWaiting(selectedCharacter.Cid, selectedCharacter.Name).WrapNetworkErrors();
        }

        private void InvokeCharacterDeleteWaiting(int cid)
        {
            SetSelectCharacter(cid);
            OnUpdateView?.Invoke();
        }

        /// <summary>
        /// 캐릭터 삭제 요청
        /// </summary>
        public async void RequestDeleteCharacterComplete()
        {
            string title = LocalizeKey._5.ToText(); // 알람
            string description = LocalizeKey._90040.ToText(); // 캐릭터를 삭제하시겠습니까?\n\n[FF0000]※ 캐릭터는 24시간 후 부터 삭제 할 수 있으며 취소할 수 있습니다.\n삭제 후에는 모든 정보가 사라집니다.[-]"
            if (!await UI.SelectPopup(title, description))
                return;

            if (selectedCharacter.IsGuild)
            {
                description = LocalizeKey._90053.ToText(); // 길드탈퇴 또는 길드 해산 후 캐릭터 삭제가 가능합니다.
                UI.ConfirmPopup(title, description);
                return;
            }

            characterListModel.RequestDeleteCharacterComplete(selectedCharacter.Cid, selectedCharacter.Name).WrapNetworkErrors();
        }

        private void InvokeCharacterDelete()
        {
            SetSelectCharacter(characterListModel.SelectedCharacterCid);
            OnUpdateView?.Invoke();
        }

        /// <summary>
        /// 캐릭터 삭제 취소 요청
        /// </summary>
        public void RequestDeleteCharacterCancel()
        {
            characterListModel.RequestDeleteCharacterCancel(selectedCharacter.Cid, selectedCharacter.Name).WrapNetworkErrors();
        }

        private void InvokeCharacterDeleteCancel(int cid)
        {
            SetSelectCharacter(cid);
            OnUpdateView?.Invoke();
        }

        public int GetCurrentServerNameKey()
        {
            return connectionManager.GetServerNameKey();
        }

        /// <summary>
        /// UnitViewer용 더미 플레이어 반환
        /// </summary>
        public CharacterEntity GetDummyUIPlayer()
        {
            if (dummyUIEntity is null)
                dummyUIEntity = CharacterEntity.Factory.CreateDummyUIPlayer(isBookPreview: false);
            DummyCharacterModel dummyCharacterModel = dummyUIEntity.Character as DummyCharacterModel;

            SimpleCharacterPacket input = characterListModel.FindSimpleCharacter(selectedCharacter.Cid);
            dummyCharacterModel.Set(input.Job.ToEnum<Job>(), input.Gender.ToEnum<Gender>());
            dummyCharacterModel.Set(input.WeaponItemId);
            dummyCharacterModel.SetCostume(input.EquippedItems);

            return dummyUIEntity;
        }

        /// <summary>
        /// 캐릭터 정보 목록
        /// </summary>
        public UICharacterSelectElement.IInput[] GetArrayData()
        {
            characterSelectElements = new CharacterSelectElement[BasisType.MAX_CHAR_SLOT.GetInt()];

            SimpleCharacterPacket[] characters = characterListModel.GetCharacters();

            for (int i = 0; i < characterSelectElements.Length; i++)
            {
                if (i < characters.Length)
                {
                    characters[i].Initialize(profileDataRepoImpl);
                }
                CharacterSelectElement temp = new CharacterSelectElement(i < characters.Length ? characters[i] : null, CheckSelect);
                characterSelectElements[i] = temp;
            }

            return characterSelectElements;
        }

        /// <summary>
        /// 선택한 캐릭터인지 여부
        /// </summary>
        public bool CheckSelect(int cid)
        {
            return selectedCharacter.Cid == cid;
        }

        public int GetSelectedIndex()
        {
            int cid = characterListModel.SelectedCharacterCid;
            for (int i = 0; i < characterSelectElements.Length; i++)
            {
                if (characterSelectElements[i].Cid == cid)
                    return i;
            }

            return 0;
        }

        private class CharacterSelectElement : UICharacterSelectElement.IInput
        {
            public bool IsEmpty { get; }
            public int Cid { get; }
            public bool IsSelect => IsEmpty ? false : checkSelect(Cid);
            public string ProfileName { get; }
            public int JobLevel { get; }
            public string Name { get; }
            public string JobIconName { get; }
            public bool IsDeleletWaiting { get; }
            public RemainTime RemainTimeDeleteWaiting { get; }
            public bool IsShare { get; }

            private readonly System.Func<int, bool> checkSelect;

            public CharacterSelectElement(SimpleCharacterPacket data, System.Func<int, bool> checkSelect)
            {
                if (data == null)
                {
                    IsEmpty = true;
                    return;
                }

                IsEmpty = false;
                Cid = data.Cid;
                ProfileName = data.ProfileName;
                JobLevel = data.JobLevel;
                Name = data.Name;
                JobIconName = data.Job.ToEnum<Job>().GetJobIcon();
                IsDeleletWaiting = data.IsDeleteWaiting;
                RemainTimeDeleteWaiting = data.RemainTimeDeleteWaiting;
                IsShare = data.IsShare;
                this.checkSelect = checkSelect;
            }
        }
    }
}