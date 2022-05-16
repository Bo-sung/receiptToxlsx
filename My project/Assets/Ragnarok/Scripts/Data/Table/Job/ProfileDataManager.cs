using MsgPack;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ProfileDataManager : Singleton<ProfileDataManager>, IDataManger, ProfileDataManager.IProfileDataRepoImpl
    {
        public interface IProfileDataRepoImpl
        {
            ProfileData Get(int id);
        }

        private readonly BetterList<ProfileData> dataList;

        public ResourceType DataType => ResourceType.ProfileDataDB;

        public ProfileDataManager()
        {
            dataList = new BetterList<ProfileData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ProfileData data = new ProfileData(mpo.AsList());
                    dataList.Add(data);
                }
            }
        }

        /// <summary>
        /// 데이터 배열 반환
        /// </summary>
        public ProfileData[] GetArrayData()
        {
            return dataList.ToArray();
        }

        public ProfileData Get(int id)
        {
            if (id == 0)
                return null;

            for (int i = 0; i < dataList.size; i++)
            {
                if (dataList[i].Id == id)
                    return dataList[i];
            }

            Debug.LogError($"83.프로필테이블가 존재하지 않습니다: {nameof(id)} = {id}");
            return null;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            dataList.Sort((x, y) => x.sort.CompareTo(y.sort));
        }

        /// <summary>
        /// 데이터 검증
        /// </summary>
        public void VerifyData()
        {
        }
    }
}