using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class BotEntityPoolManager : Singleton<BotEntityPoolManager>
    {
        // <!-- Player Bot --!>
        private readonly BetterList<PlayerBotEntity> playerEntityList; // 사용중인 entity List (Player)
        private readonly Stack<PlayerBotEntity> playerPooledStack; // 사용대기중인(사용 중 x) entity Stack (Player)

        // <!-- Monster Bot --!>
        private readonly BetterList<MonsterBotEntity> monsterEntityList; // 사용중인 entity List (Monster)
        private readonly Stack<MonsterBotEntity> monsterPooledStack; // 사용대기중인(사용 중 x) entity Stack (Monster)

        public BotEntityPoolManager()
        {
            playerEntityList = new BetterList<PlayerBotEntity>();
            playerPooledStack = new Stack<PlayerBotEntity>();

            monsterEntityList = new BetterList<MonsterBotEntity>();
            monsterPooledStack = new Stack<MonsterBotEntity>();
        }

        protected override void OnTitle()
        {
            Clear();
        }

        /// <summary>
        /// 데이터 삭제
        /// </summary>
        public void Clear()
        {
            playerEntityList.Release();
            playerPooledStack.Clear();

            monsterEntityList.Release();
            monsterPooledStack.Clear();
        }

        /// <summary>
        /// 사용중인 모든 Entity 재활용
        /// </summary>
        public void Recycle()
        {
            PlayerRecycle();
            MonsterRecycle();
        }

        /// <summary>
        /// 사용중인 모든 플레이어 Entity 재활용
        /// </summary>
        public void PlayerRecycle()
        {
            while (playerEntityList.size > 0)
            {
                Recycle(playerEntityList[0]);
            }
        }

        /// <summary>
        /// 사용중인 모든 몬스터 Entity 재활용
        /// </summary>
        public void MonsterRecycle()
        {
            while (monsterEntityList.size > 0)
            {
                Recycle(monsterEntityList[0]);
            }
        }

        /// <summary>
        /// 사용중인 Entity 회수
        /// </summary>
        public void Recycle(PlayerBotEntity entity)
        {
            if (playerEntityList.Remove(entity))
            {
                entity.Initialize(PlayerBotEntity.DEFAULT); // 초기화
                entity.ResetData();

                playerPooledStack.Push(entity); // Stack에 관리 (Pool)
            }
        }

        /// <summary>
        /// 사용중인 Entity 회수
        /// </summary>
        public void Recycle(MonsterBotEntity entity)
        {
            if (monsterEntityList.Remove(entity))
            {
                entity.Initialize(MonsterBotEntity.DEFAULT); // 초기화
                entity.ResetData();

                monsterPooledStack.Push(entity); // Stack에 관리 (Pool)
            }
        }

        /// <summary>
        /// Entity 생성
        /// </summary>
        public void Create(IMultiPlayerInput[] inputs)
        {
            if (inputs == null)
                return;

            for (int i = 0; i < inputs.Length; i++)
            {
                Create(inputs[i]);
            }
        }

        /// <summary>
        /// Entity 생성
        /// </summary>
        public PlayerBotEntity Create(IMultiPlayerInput input)
        {
            PlayerBotEntity entity = playerPooledStack.Count > 0 ? playerPooledStack.Pop() : CharacterEntity.Factory.CreatePlayerBot();

            entity.Initialize(input); // 초기화
            playerEntityList.Add(entity); // List에 관리

            return entity;
        }

        /// <summary>
        /// Entity 생성
        /// </summary>
        public void Create(IMonsterBotInput[] inputs)
        {
            if (inputs == null)
                return;

            for (int i = 0; i < inputs.Length; i++)
            {
                Create(inputs[i]);
            }
        }

        /// <summary>
        /// Entity 생성
        /// </summary>
        public MonsterBotEntity Create(IMonsterBotInput input)
        {
            MonsterBotEntity entity = monsterPooledStack.Count > 0 ? monsterPooledStack.Pop() : MonsterEntity.Factory.CreateMonsterBot();

            entity.Initialize(input); // 초기화
            monsterEntityList.Add(entity); // List에 관리

            return entity;
        }

        /// <summary>
        /// 사용중인 플레이어 반환
        /// </summary>
        public PlayerBotEntity[] GetPlayers()
        {
            return playerEntityList.ToArray() ?? System.Array.Empty<PlayerBotEntity>();
        }

        /// <summary>
        /// 사용중인 몬스터 반환
        /// </summary>
        public MonsterBotEntity[] GetMonsters()
        {
            return monsterEntityList.ToArray() ?? System.Array.Empty<MonsterBotEntity>();
        }

        /// <summary>
        /// 사용중인 Entity 반환 (key: cid)
        /// </summary>
        public PlayerBotEntity FindPlayer(int cid)
        {
            for (int i = 0; i < playerEntityList.size; i++)
            {
                if (playerEntityList[i].Character.Cid == cid)
                    return playerEntityList[i];
            }

            return null;
        }

        /// <summary>
        /// 사용중인 Entity 반환 (key: monsterServerIndex)
        /// </summary>
        public MonsterBotEntity FindMonster(int serverIndex)
        {
            for (int i = 0; i < monsterEntityList.size; i++)
            {
                if (monsterEntityList[i].BotServerIndex == serverIndex)
                    return monsterEntityList[i];
            }

            return null;
        }

        /// <summary>
        /// 플레이어 나감
        /// </summary>
        [System.Obsolete("Find 후에 Recycle 처리 필수", true)]
        public PlayerBotEntity LeavePlayer(int cid)
        {
            return null;
        }

        /// <summary>
        /// 몬스터 나감
        /// </summary>
        [System.Obsolete("Find 후에 Recycle 처리 필수", true)]
        public MonsterBotEntity LeaveMonster(int serverIndex)
        {
            return null;
        }

        /// <summary>
        /// 플레이어 상태 변화
        /// </summary>
        public PlayerBotEntity UpdatePlayerState(int cid, byte state)
        {
            PlayerBotEntity finded = FindPlayer(cid);
            if (finded == null)
                return null;

            finded.SetBotState(state);
            return finded;
        }

        /// <summary>
        /// 플레이어 maxHp 변화
        /// </summary>
        public PlayerBotEntity UpdatePlayerMaxHp(int cid, int maxHp)
        {
            PlayerBotEntity finded = FindPlayer(cid);
            if (finded == null)
                return null;

            finded.SetBotMaxHp(maxHp);
            return finded;
        }

        /// <summary>
        /// 플레이어 curHp 변화
        /// </summary>
        public PlayerBotEntity UpdatePlayerCurHp(int cid, int curHp)
        {
            PlayerBotEntity finded = FindPlayer(cid);
            if (finded == null)
                return null;

            finded.SetBotCurHp(curHp);
            return finded;
        }

        /// <summary>
        /// 플레이어 움직임 변화
        /// </summary>
        public PlayerBotEntity UpdatePlayerMove(int cid, Vector3 pos)
        {
            PlayerBotEntity finded = FindPlayer(cid);
            if (finded == null)
                return null;

            finded.SetBotPosition(pos);
            return finded;
        }

        /// <summary>
        /// 플레이어 상점상태 변화
        /// </summary>
        public PlayerBotEntity UpdatePlayerSellingState(int cid, PrivateStoreSellingState sellingState, string storeComment)
        {
            PlayerBotEntity finded = FindPlayer(cid);
            if (finded == null)
                return null;

            finded.SetBotSellingState(sellingState, storeComment);
            return finded;
        }

        /// <summary>
        /// 몬스터 상태 변화
        /// </summary>
        public MonsterBotEntity UpdateMonsterState(int serverIndex, byte state)
        {
            MonsterBotEntity finded = FindMonster(serverIndex);
            if (finded == null)
                return null;

            finded.SetBotState(state);
            return finded;
        }

        /// <summary>
        /// 몬스터 curHp 변화
        /// </summary>
        public MonsterBotEntity UpdateMonsterCurHp(int serverIndex, int curHp)
        {
            MonsterBotEntity finded = FindMonster(serverIndex);
            if (finded == null)
                return null;

            finded.SetBotCurHp(curHp);
            return finded;
        }

        /// <summary>
        /// 몬스터 움직임 변화
        /// </summary>
        public MonsterBotEntity UpdateMonsterMove(int serverIndex, Vector3 pos)
        {
            MonsterBotEntity finded = FindMonster(serverIndex);
            if (finded == null)
                return null;

            finded.SetBotPosition(pos);
            return finded;
        }

        /// <summary>
        /// 몬스터 움직임 변화 (타겟)
        /// </summary>
        public MonsterBotEntity UpdateMonsterMove(int serverIndex, Vector3 pos, Vector3 targetPos)
        {
            MonsterBotEntity finded = FindMonster(serverIndex);
            if (finded == null)
                return null;

            finded.SetBotPosition(pos, targetPos);
            return finded;
        }

        /// <summary>
        /// 몬스터 변화
        /// </summary>
        public MonsterBotEntity UpdateMonster(int serverIndex, int id, int level, float scale)
        {
            MonsterBotEntity finded = FindMonster(serverIndex);
            if (finded == null)
                return null;

            finded.SetBotMonster(id, level, scale);
            return finded;
        }
    }
}