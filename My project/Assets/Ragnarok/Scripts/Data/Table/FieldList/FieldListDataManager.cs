using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    //public sealed class FieldListDataManager : Singleton<FieldListDataManager>, IDataManger, IEqualityComparer<FieldListDataManager.Key>
    //{
    //    private struct Key
    //    {
    //        private readonly ObscuredByte dungeonType;
    //        private readonly ObscuredInt chapter;
    //        private readonly ObscuredInt difficulty;

    //        public Key(ObscuredByte dungeonType, ObscuredInt chapter, ObscuredInt difficulty)
    //        {
    //            this.dungeonType = dungeonType;
    //            this.chapter = chapter;
    //            this.difficulty = difficulty;
    //        }

    //        public override bool Equals(object obj)
    //        {
    //            if (obj is Key)
    //                return Equals((Key)obj);

    //            return false;
    //        }

    //        public override int GetHashCode()
    //        {
    //            int hash = 17;

    //            hash = hash * 29 + dungeonType.GetHashCode();
    //            hash = hash * 29 + chapter.GetHashCode();
    //            hash = hash * 29 + difficulty.GetHashCode();

    //            return hash;
    //        }

    //        public bool Equals(Key obj)
    //        {
    //            return dungeonType == obj.dungeonType && chapter == obj.chapter && difficulty == obj.difficulty;
    //        }
    //    }

    //    private readonly Dictionary<ObscuredInt, FieldListData> dataDic;
    //    private readonly Dictionary<Key, ObscuredInt> fieldIdDic;       

    //    public ResourceType DataType => ResourceType.StageDataDB;

    //    public FieldListDataManager()
    //    {
    //        dataDic = new Dictionary<ObscuredInt, FieldListData>(ObscuredIntEqualityComparer.Default);
    //        fieldIdDic = new Dictionary<Key, ObscuredInt>(this);           
    //    }

    //    public List<ObscuredInt> GetFieldIDList()
    //    {
    //        return new List<ObscuredInt>(fieldIdDic.Values);
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
    //        fieldIdDic.Clear();          
    //    }

    //    public void LoadData(byte[] bytes)
    //    {
    //        using (var unpack = Unpacker.Create(bytes))
    //        {
    //            while (unpack.ReadObject(out MessagePackObject mpo))
    //            {
    //                FieldListData data = new FieldListData(mpo.AsList());
    //                Key key = CreateKey(data.type, data.seq, data.level);

    //                if (dataDic.ContainsKey(data.id))
    //                    throw new System.ArgumentException($"중복된 데이터 입니다: {data.GetDump()}");
                    
    //                if (fieldIdDic.ContainsKey(key))
    //                    throw new System.ArgumentException($"중복된 데이터 입니다: {data.GetDump()}");

    //                dataDic.Add(data.id, data);
    //                fieldIdDic.Add(key, data.id);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 필드리스트데이터 반환
    //    /// </summary>
    //    public FieldListData Get(int id)
    //    {
    //        if (!dataDic.ContainsKey(id))
    //        {
    //            Debug.LogError($"필드리스트 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
    //            return null;
    //        }

    //        return dataDic[id];
    //    }

    //    /// <summary>
    //    /// 필드리스트데이터 아이디를 반환
    //    /// </summary>
    //    public int GetFieldListDataID(DungeonType dungeonType, int chapter, int difficulty)
    //    {
    //        Key key = CreateKey((byte)dungeonType, chapter, difficulty);

    //        if (!fieldIdDic.ContainsKey(key))
    //        {
    //            Debug.LogError($"필드리스트 데이터가 존재하지 않습니다: {nameof(dungeonType)} = {dungeonType}, {nameof(chapter)} = {chapter}, {nameof(difficulty)} = {difficulty}");
    //            return 0;
    //        }

    //        return fieldIdDic[key];
    //    }      

    //    private Key CreateKey(byte dungeonType, int chapter, int difficulty)
    //    {
    //        return new Key(dungeonType, chapter, difficulty);
    //    }

    //    bool IEqualityComparer<Key>.Equals(Key x, Key y)
    //    {
    //        return x.Equals(y);
    //    }

    //    int IEqualityComparer<Key>.GetHashCode(Key obj)
    //    {
    //        return obj.GetHashCode();
    //    }
    //}
}