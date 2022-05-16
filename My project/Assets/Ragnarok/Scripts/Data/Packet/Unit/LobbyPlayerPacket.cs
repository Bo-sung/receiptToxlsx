using Sfs2X.Entities.Data;
using UnityEngine;

namespace Ragnarok
{
    public class LobbyPlayerPacket : IPacket<ISFSObject>, ILobbyPlayerInfo
    {
        int CID;
        string nickName;
        string guildName;
        Vector3 pos;
        long weaponId;
        string privateStoreComment;
        Job job;
        Gender gender;
        int level;
        PrivateStoreSellingState sellingState;
        string costumeName;
        int uid;

        int ILobbyPlayerInfo.CID => CID;
        string ILobbyPlayerInfo.NickName => nickName;
        string ILobbyPlayerInfo.GuildName => guildName;
        Vector3 ILobbyPlayerInfo.Pos => pos;
        long ILobbyPlayerInfo.WeaponId => weaponId;
        string ILobbyPlayerInfo.PrivateStoreComment => privateStoreComment;
        Job ILobbyPlayerInfo.Job => job;
        Gender ILobbyPlayerInfo.Gender => gender;
        int ILobbyPlayerInfo.Level => level;
        PrivateStoreSellingState ILobbyPlayerInfo.SellingState => sellingState;
        string ILobbyPlayerInfo.CostumeName => costumeName;
        int ILobbyPlayerInfo.UID => uid;

        public void Initialize(ISFSObject sfsData)
        {
            nickName = sfsData.GetUtfString("n");
            guildName = sfsData.GetUtfString("g");
            if (string.Equals(guildName, "0")) // TODO: 길드 없을 때 0으로 들어오는데 길드명이 진짜 0이면 문제가 생길 수 있다 ..
                guildName = string.Empty;

            float x = sfsData.GetFloat("x");
            float z = sfsData.GetFloat("z");
            pos = new Vector3(x, Constants.Map.POSITION_Y, z);
            weaponId = sfsData.GetLong("wp");
            privateStoreComment = sfsData.GetUtfString("sc");
            int[] cInfos = sfsData.GetIntArray("app");
            CID = cInfos[0];
            job = cInfos[1].ToEnum<Job>();
            gender = cInfos[2].ToEnum<Gender>();
            sellingState = cInfos[3].ToEnum<PrivateStoreSellingState>();
            level = cInfos[4];
            if (cInfos.Length > 5)
                uid = cInfos[5];

            // 코스튬
            costumeName = string.Empty;
            if (sfsData.ContainsKey("10"))
            {
                ISFSObject costumeInfo = sfsData.GetSFSObject("10");
                //int costumeNo = costumeInfo.GetInt("1");
                //int cid = costumeInfo.GetInt("2");
                costumeName = costumeInfo.GetUtfString("3");
                //byte is_use = costumeInfo.GetByte("4");
            }
        }

        public void SetRandomData(float x, float z)
        {
            nickName = FilterUtils.GetAutoNickname();
            guildName = string.Empty;
            pos = new Vector3(x, Constants.Map.POSITION_Y, z);

            weaponId = 0;
            privateStoreComment = LocalizeKey._45008.ToText().Replace(ReplaceKey.NAME, nickName); // {NAME}의 상점;
            CID = -1;
            var jobs = (Job[])System.Enum.GetValues(typeof(Job));
            job = jobs[Random.Range(0, jobs.Length)];
            var genders = (Gender[])System.Enum.GetValues(typeof(Gender));
            gender = genders[Random.Range(0, genders.Length)];
            sellingState = PrivateStoreSellingState.SELLING;
            level = Random.Range(1, 11);
            costumeName = string.Empty;
        }
    }
}