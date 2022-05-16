using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    //public class DungeonInfo : DataInfo<FieldListData>
    //{
    //    private readonly MonsterDataManager monsterDataRepo;

    //    private readonly List<FieldData> fieldDataList;

    //    ObscuredInt startWave, chapterClearCount;
    //    ObscuredInt currentWave, maxWave;

    //    private FieldData currentFieldData;

    //    /// <summary>
    //    /// 진행 웨이브
    //    /// </summary>
    //    public int StartWave => startWave;

    //    /// <summary>
    //    /// 해당 던전 클리어 수
    //    /// </summary>
    //    public int ChapterClearCount => chapterClearCount;

    //    /// <summary>
    //    /// 던전 최초 클리어 여부
    //    /// </summary>
    //    public bool IsFirstClear => ChapterClearCount > 0;

    //    /// <summary>
    //    /// 현재 진행중인 웨이브
    //    /// </summary>
    //    public int CurrentWave => currentWave;

    //    /// <summary>
    //    /// 최대 웨이브
    //    /// </summary>
    //    public int MaxWave => maxWave;

    //    /// <summary>
    //    /// 필드리스트데이터 아이디
    //    /// </summary>
    //    public int FieldListDataID => data.id;

    //    /// <summary>
    //    /// 던전 이름
    //    /// </summary>
    //    public string DungeonName => data.name_id.ToText();

    //    /// <summary>
    //    /// 던전 타입
    //    /// </summary>
    //    public DungeonType DungeonType => data.type.ToEnum<DungeonType>();

    //    /// <summary>
    //    /// 챕터
    //    /// </summary>
    //    public int Chapter => data.seq;

    //    /// <summary>
    //    /// 난이도
    //    /// </summary>
    //    public int Difficulty => data.level;

    //    /// <summary>
    //    /// 씬 이름
    //    /// </summary>
    //    public string SceneName => data.scene_name;

    //    /// <summary>
    //    /// 필드데이터 아이디
    //    /// </summary>
    //    public int FieldDataID => currentFieldData.id;

    //    /// <summary>
    //    /// 보스 몬스터 ID
    //    /// </summary>
    //    public int BossID => currentFieldData.boss_id;

    //    /// <summary>
    //    /// 모든 wave를 클리어하여 선택 UI 띄우기 여부 (다음필드/나가기, 다시 시작)
    //    /// </summary>
    //    public bool IsShowWaveClearUI => startWave == 0 && chapterClearCount > 0;

    //    /// <summary>
    //    /// 던전 오픈 조건 직업레벨
    //    /// </summary>
    //    public int NeedJobLevel => data.need_job_level;

    //    public DungeonInfo()
    //    {
    //        monsterDataRepo = MonsterDataManager.Instance;
    //        fieldDataList = new List<FieldData>();
    //    }

    //    public void Initialize(FieldListData data, List<FieldData> list)
    //    {
    //        SetData(data);

    //        fieldDataList.Clear();
    //        fieldDataList.AddRange(list);

    //        maxWave = list[list.Count - 1].wave_bump; // Set MaxWave
    //    }

    //    public int GetDungeonNameID()
    //    {
    //        return data.name_id;
    //    }

    //    /// <summary>
    //    /// 진행 웨이브 세팅 (값이 5일경우 5까지 클리어했음, 6을 시작할 차례)
    //    /// </summary>
    //    public void SetStartWave(int startWave)
    //    {
    //        this.startWave = startWave;
    //        InvokeEvent(); // 해당 던전 정보 업데이트
    //    }

    //    /// <summary>
    //    /// 클리어 세팅 (현재 클리어 여부로 사용한다: 0 or 1)
    //    /// </summary>
    //    public void SetChapterClearCount(int chapterClearCount)
    //    {
    //        this.chapterClearCount = chapterClearCount;
    //        InvokeEvent(); // 해당 던전 정보 업데이트
    //    }

    //    /// <summary>
    //    /// 현재 웨이브 세팅
    //    /// </summary>
    //    public void SetCurrentWave(int value)
    //    {
    //        if (currentWave == value)
    //            return;

    //        currentWave = value;
    //        currentFieldData = GetFieldData(currentWave);
    //    }

    //    /// <summary>
    //    /// 보상 정보
    //    /// </summary>
    //    public RewardInfo[] GetRewards()
    //    {
    //        HashSet<int> hashSet = new HashSet<int>(IntEqualityComparer.Default);
    //        for (int i = 0; i < fieldDataList.Count; i++)
    //        {
    //            for (int index = 0; index < FieldData.MAX_REWARD_INDEX; index++)
    //            {
    //                int dropItemID = fieldDataList[i].GetItemRewardInfo(index);
    //                if (dropItemID == 0)
    //                    continue;

    //                hashSet.Add(dropItemID);
    //            }
    //        }

    //        List<RewardInfo> list = new List<RewardInfo>(hashSet.Count);
    //        foreach (var item in hashSet)
    //        {
    //            list.Add(new RewardInfo(RewardType.Item, item, rewardCount: 1));
    //        }

    //        list.Sort(SortByRaredItem);
    //        return list.ToArray();
    //    }

    //    /// <summary>
    //    /// 소환할 몬스터 리스트
    //    /// </summary>
    //    public List<int> GetMonsterList()
    //    {
    //        List<int> list = new List<int>();

    //        List<SpawnInfo> spawnInfoList = GetSpawnInfos();
    //        int waveCost = currentFieldData.wave_cost;

    //        while (waveCost > 0)
    //        {
    //            // 몬스터 비용이 큰 값은 제외
    //            spawnInfoList.RemoveAll(a => waveCost - a.cost < 0);

    //            // 체크할 몬스터가 없을 경우
    //            if (spawnInfoList.Count == 0)
    //                break;

    //            // 소환 전체 확률 계산
    //            int totalSpawnRate = 0;
    //            for (int i = 0; i < spawnInfoList.Count; i++)
    //            {
    //                totalSpawnRate += spawnInfoList[i].spawnRate;
    //            }

    //            // 소환할 몬스터 확률 체크
    //            int spwanRate;
    //            for (int i = 0; i < spawnInfoList.Count; i++)
    //            {
    //                spwanRate = spawnInfoList[i].spawnRate;

    //                // 소환 당첨
    //                if (Random.Range(0, totalSpawnRate) < spwanRate)
    //                {
    //                    list.Add(spawnInfoList[i].monsterID);
    //                    waveCost -= spawnInfoList[i].cost; // 비용 감소
    //                    break;
    //                }

    //                totalSpawnRate -= spwanRate; // 전체 소환 확률 감소
    //            }
    //        }

    //        return list;
    //    }

    //    /// <summary>
    //    /// wave에 해당하는 FieldData 를 반환
    //    /// </summary>
    //    private FieldData GetFieldData(int wave)
    //    {
    //        for (int i = 0; i < fieldDataList.Count; i++)
    //        {
    //            if (wave <= fieldDataList[i].wave_bump)
    //                return fieldDataList[i];
    //        }

    //        return null;
    //    }

    //    public List<FieldData> GetFieldDataList()
    //    {
    //        return fieldDataList;
    //    }

    //    /// <summary>
    //    /// 몬스터 소환 정보 리스트
    //    /// </summary>
    //    private List<SpawnInfo> GetSpawnInfos()
    //    {
    //        List<SpawnInfo> list = new List<SpawnInfo>();

    //        for (int i = 0; i < FieldData.MAX_MONSTER_INDEX; i++)
    //        {
    //            currentFieldData.GetSpawnInfo(i, out int spawnRate, out int monsterID);

    //            // 확률이 음슴 or 몬스터가 음슴
    //            if (spawnRate == 0 || monsterID == 0)
    //                continue;

    //            MonsterData monsterData = monsterDataRepo.Get(monsterID);

    //            // 몬스터 데이터가 음슴
    //            if (monsterData == null)
    //            {
    //                Debug.LogError($"몬스터 데이터가 존재하지 않습니다: {nameof(FieldDataID)} = {FieldDataID}, {nameof(monsterID)} = {monsterID}");
    //                continue;
    //            }

    //            list.Add(new SpawnInfo(monsterID, monsterData.cost, spawnRate));
    //        }

    //        return list;
    //    }

    //    /// <summary>
    //    /// 희귀 아이템으로 분류
    //    /// </summary>
    //    private int SortByRaredItem(RewardInfo x, RewardInfo y)
    //    {
    //        int result1 = x.data.ItemData.rating.CompareTo(y.data.ItemData.rating); // rating
    //        int result2 = result1 == 0 ? x.data.ItemData.item_type.CompareTo(y.data.ItemData.item_type) : result1; // itemType
    //        return result2;
    //    }

    //    /// <summary>
    //    /// 몬스터 나오는 웨이브 구간 정보 반환
    //    /// </summary>
    //    /// <param name="monsterId"></param>
    //    /// <returns></returns>
    //    public List<System.Tuple<int, int>> GetWaveBump(int monsterId)
    //    {
    //        List<System.Tuple<int, int>> waveBump = new List<System.Tuple<int, int>>();
    //        var fieldData = GetFieldDataList();
    //        int startBump;
    //        for (int i = 0; i < fieldDataList.Count; i++)
    //        {
    //            if (fieldData[i].monster_id_1 == monsterId)
    //            {
    //                startBump = i == 0 ? 1 : fieldData[i - 1].wave_bump + 1;
    //                waveBump.Add(new System.Tuple<int, int>(startBump, fieldData[i].wave_bump));
    //                continue;
    //            }
    //            if (fieldData[i].monster_id_2 == monsterId)
    //            {
    //                startBump = i == 0 ? 1 : fieldData[i - 1].wave_bump + 1;
    //                waveBump.Add(new System.Tuple<int, int>(startBump, fieldData[i].wave_bump));
    //                continue;
    //            }
    //            if (fieldData[i].monster_id_3 == monsterId)
    //            {
    //                startBump = i == 0 ? 1 : fieldData[i - 1].wave_bump + 1;
    //                waveBump.Add(new System.Tuple<int, int>(startBump, fieldData[i].wave_bump));
    //                continue;
    //            }
    //            if (fieldData[i].monster_id_4 == monsterId)
    //            {
    //                startBump = i == 0 ? 1 : fieldData[i - 1].wave_bump + 1;
    //                waveBump.Add(new System.Tuple<int, int>(startBump, fieldData[i].wave_bump));
    //                continue;
    //            }
    //        }

    //        return waveBump;
    //    }

    //    /// <summary>
    //    /// 몬스터 소환 정보
    //    /// </summary>
    //    private class SpawnInfo
    //    {
    //        public readonly int monsterID;
    //        public readonly int cost;
    //        public readonly int spawnRate;

    //        public SpawnInfo(int monsterID, int cost, int spawnRate)
    //        {
    //            this.monsterID = monsterID;
    //            this.cost = cost;
    //            this.spawnRate = spawnRate;
    //        }
    //    }
    //}
}