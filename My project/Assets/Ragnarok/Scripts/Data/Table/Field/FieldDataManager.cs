using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    //// TODO 던전 데이터 작업 후 삭제
    //public sealed class FieldDataManager : Singleton<FieldDataManager>, IDataManger
    //{
    //    private readonly Dictionary<ObscuredInt, FieldData> dataDic;

    //    /// <summary>
    //    /// Key: 필드리스트데이터 아이디
    //    /// Value: 리스트 형식의 FieldData
    //    /// </summary>
    //    private readonly Dictionary<ObscuredInt, List<FieldData>> dataListDic;

    //    /// <summary>
    //    /// 필드 리스트 아이디 목록
    //    /// </summary>
    //    private readonly HashSet<ObscuredInt> fieldIdHashSet;

    //    public ResourceType DataType => ResourceType.DungeonDataDB;

    //    public FieldDataManager()
    //    {
    //        dataDic = new Dictionary<ObscuredInt, FieldData>(ObscuredIntEqualityComparer.Default);
    //        dataListDic = new Dictionary<ObscuredInt, List<FieldData>>(ObscuredIntEqualityComparer.Default);
    //        fieldIdHashSet = new HashSet<ObscuredInt>(ObscuredIntEqualityComparer.Default);
    //    }

    //    protected override void OnTitle()
    //    {
    //        if (IntroScene.IsBackToTitle)
    //            return;

    //        ClearData();
    //    }

    //    public void ClearData()
    //    {
    //        dataDic.Clear();
    //        dataListDic.Clear();
    //        fieldIdHashSet.Clear();
    //    }

    //    public void LoadData(byte[] bytes)
    //    {
    //        using (var unpack = Unpacker.Create(bytes))
    //        {
    //            while (unpack.ReadObject(out MessagePackObject mpo))
    //            {
    //                FieldData data = new FieldData(mpo.AsList());

    //                ObscuredInt fieldListDataID = data.quest_condition;

    //                if (!dataListDic.ContainsKey(fieldListDataID))
    //                    dataListDic.Add(fieldListDataID, new List<FieldData>());

    //                dataDic.Add(data.id, data);
    //                fieldIdHashSet.Add(fieldListDataID);
    //                dataListDic[fieldListDataID].Add(data);
    //            }
    //        }

    //        // Sort
    //        foreach (var item in dataListDic.Values)
    //        {
    //            item.Sort(SortByWaveBump);
    //        }
    //    }

    //    /// <summary>
    //    /// 필드데이터 아이디에 해당하는 필드데이터 반환
    //    /// </summary>
    //    public FieldData Get(int fieldID)
    //    {
    //        if (!dataDic.ContainsKey(fieldID))
    //        {
    //            Debug.LogError($"필드 데이터가 존재하지 않습니다: {nameof(fieldID)} = {fieldID}");
    //            return null;
    //        }

    //        return dataDic[fieldID];
    //    }

    //    /// <summary>
    //    /// 필드리스트데이터 아이디에 해당하는 필드데이터 목록 반환
    //    /// </summary>
    //    public List<FieldData> GetDataList(int fieldListDataID)
    //    {
    //        if (!dataListDic.ContainsKey(fieldListDataID))
    //        {
    //            Debug.LogError($"필드 데이터가 존재하지 않습니다: {nameof(fieldListDataID)} = {fieldListDataID}");
    //            return new List<FieldData>();
    //        }

    //        return dataListDic[fieldListDataID];
    //    }

    //    public int GetMaxWave(int fieldListDataID)
    //    {
    //        if (GetDataList(fieldListDataID) == null || GetDataList(fieldListDataID).Count == 0)
    //            return 0;
    //        return GetDataList(fieldListDataID).Max(x => x.wave_bump);
    //    }

    //    /// <summary>
    //    /// 필드리스트데이터 아이디 목록
    //    /// </summary>
    //    public IEnumerable<ObscuredInt> GetFieldListDataIDs()
    //    {
    //        return fieldIdHashSet;
    //    }

    //    /// <summary>
    //    /// Wave Bump 가 낮은 것으로 정렬
    //    /// </summary>
    //    private int SortByWaveBump(FieldData x, FieldData y)
    //    {
    //        return x.wave_bump.CompareTo(y.wave_bump);
    //    }
    //}
}