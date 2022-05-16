using System;
using UnityEngine;
namespace Ragnarok
{
    /// <summary>
    /// 개인 상점 데이터 (품목 리스트; public  이름 등)
    /// </summary>
    public class PrivateStoreData : IInfo
    {
        public PrivateStoreItemList itemList;
        public int CID;
        public string CIDHex;
        public string nickName;
        public string storeComment;

        public bool IsInvalidData => throw new NotImplementedException();

        public event Action OnUpdateEvent;

        public PrivateStoreData() { }
        public PrivateStoreData(PrivateStoreItemList itemList, int CID, string CIDHex, string nickName, string storeComment)
        {
            Initialize(itemList, CID, CIDHex, nickName, storeComment);
        }

        public void Initialize(PrivateStoreItemList itemList, int CID, string CIDHex, string nickName, string storeComment)
        {
            this.itemList = itemList;
            this.CID = CID;
            this.CIDHex = CIDHex;
            this.nickName = nickName;
            this.storeComment = storeComment;
        }

        public void SetDummyData(string nickName, string storeComment)
        {
            this.itemList = new PrivateStoreItemList();
            this.CID = -1;
            this.CIDHex = MathUtils.CidToHexCode(this.CID);
            this.nickName = nickName;
            this.storeComment = storeComment;
        }
    }
}