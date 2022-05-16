namespace Ragnarok
{
    public class ServerChangePresenter : ViewPresenter
    {
        // <!-- Models --!>
        CharacterModel characterModel;
        CharacterListModel characterListModel;

        // <!-- Repositories --!>
        ConnectionManager connectionRepo;
        private readonly ProfileDataManager profileDataRepo;

        // <!-- Event --!>

        ServerInfoPacket[] serverInfos;

        public ServerChangePresenter()
        {
            characterModel = Entity.player.Character;
            characterListModel = Entity.player.CharacterList;
            connectionRepo = ConnectionManager.Instance;
            profileDataRepo = ProfileDataManager.Instance;
            serverInfos = null;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public int GetServerNameKey()
        {
            return connectionRepo.GetServerNameKey();
        }

        public void GetServerList(System.Action<ServerInfoPacket[], bool> refreshServer)
        {
            if (true)//serverInfos == null) // 서버 요청, 서버 포화도 때문에 매번 요청하는것으로..
            {
                connectionRepo.AsyncAllServerInfo(refreshServer).WrapNetworkErrors();
            }
            else // 현재 캐릭터만 갱신
            {
                var currentServerId = connectionRepo.GetSelectServerGroupId();
                for (int i = 0; i < serverInfos.Length; i++)
                {
                    if (serverInfos[i].ServerGroupId == currentServerId)
                    {
                        serverInfos[i].UpdateCharacterInfo(characterModel.Name, (byte)characterModel.Job, characterModel.JobLevel, (byte)characterModel.Gender);
                        break;
                    }
                }

                refreshServer?.Invoke(serverInfos, false);
            }
        }

        public ServerInfoPacket GetServerInfo(int idx)
        {
            return serverInfos[idx];
        }

        public void SetServerInfos(ServerInfoPacket[] infos)
        {
            serverInfos = infos;

            if (serverInfos != null)
            {
                foreach (var item in serverInfos)
                {
                    item.Initialize(profileDataRepo);
                }
            }
        }

        public bool ChangeableServer(int idx)
        {
            if (idx < 0) return false;
            if (!serverInfos[idx].HasCharacter() && !serverInfos[idx].AvailableCreate) return false; // 캐릭터 없고, 생성불가
            if (serverInfos[idx].State != ServerState.EXTERNAL_OPEN) return false; // 외부오픈이 아닐 때,

            return serverInfos[idx].ServerGroupId != connectionRepo.GetSelectServerGroupId();
        }

        private ServerInfoPacket GetServerInfoByServerId(int serverId)
        {
            for (int i = 0; i < serverInfos.Length; i++)
            {
                if (serverInfos[i].ServerGroupId == serverId)
                {
                    return serverInfos[i];
                }
            }
            return null;
        }

        private bool CanChangeableServer(int serverId)
        {
            ServerInfoPacket serverInfo = GetServerInfoByServerId(serverId);
            if (serverInfo == null)
                return false;

            // 캐릭터 없고, 생성불가
            if (!serverInfo.HasCharacter() && !serverInfo.AvailableCreate)
                return false;

            // 외부오픈이 아닐 때,
            if (serverInfo.State != ServerState.EXTERNAL_OPEN)
                return false;

            return true;
        }

        public async void ChangeServer(int serverId)
        {
            // 서버 접속 가능 여부 체크
            if (!CanChangeableServer(serverId))
                return;

            if (!await UI.SelectPopup(LocalizeKey._1106.ToText()))
                return;

            characterListModel.ResetSelectCharacterCid();
            connectionRepo.SelectServer(serverId);
            connectionRepo.Disconnect();

            SceneLoader.LoadIntro(); // 타이틀 화면으로 이동
        }
    }
}