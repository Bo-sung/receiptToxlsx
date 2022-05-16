using MsgPack;
using UnityEngine;

namespace Ragnarok
{
    public sealed class MultiMazeWaitingRoomDataManager : Singleton<MultiMazeWaitingRoomDataManager>, IDataManger
    {
        private readonly BetterList<MultiMazeWaitingRoomData> dataList;
        private readonly Buffer<MultiMazeWaitingRoomData> buffer;

        public ResourceType DataType => ResourceType.MultiMazeWaitingRoomDataDB;

        public MultiMazeWaitingRoomDataManager()
        {
            dataList = new BetterList<MultiMazeWaitingRoomData>();
            buffer = new Buffer<MultiMazeWaitingRoomData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Release();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    MultiMazeWaitingRoomData data = new MultiMazeWaitingRoomData(mpo.AsList());
                    dataList.Add(data);
                }
            }

            dataList.Sort(Compare);
        }

        public MultiMazeWaitingRoomData Get(int id)
        {
            foreach (var item in dataList)
            {
                if (item.id == id)
                    return item;
            }

            Debug.LogError($"멀티미로대기실 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
            return null;
        }

        public MultiMazeWaitingRoomData[] GetData(int adventureGroup)
        {
            foreach (var item in dataList)
            {
                if (item.sort_index < 0)
                    continue;

                if (item.GetGroup() != adventureGroup)
                    continue;

                buffer.Add(item);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }

        private int Compare(MultiMazeWaitingRoomData x, MultiMazeWaitingRoomData y)
        {
            int result1 = x.sort_index.CompareTo(y.sort_index); // sortIndex
            int result2 = result1 == 0 ? x.id.CompareTo(y.id) : result1; // id
            return result2;
        }

        public void Initialize()
        {
            GateDataManager gateDataRepo = GateDataManager.Instance;
            MultiMazeDataManager multiMazeDataRepo = MultiMazeDataManager.Instance;

            foreach (var item in dataList)
            {
                // 게이트일 경우
                if (item.IsGate())
                {
                    int gateId = (item.id - MultiMazeWaitingRoomData.GATE_1) + 1;
                    GateData gateData = gateDataRepo.Get(gateId);
                    if (gateData != null)
                    {
                        MultiMazeData multiMazeData = multiMazeDataRepo.GetByChapter(gateData.chapter);
                        if (multiMazeData != null)
                        {
                            item.SetMultiMazeDataId(multiMazeData.id);
                            gateData.SetMultiMazeDataId(multiMazeData.id);
                            multiMazeData.SetWaitingRoomId(item.id);
                        }
                    }
                }
            }
        }

        public void VerifyData()
        {
        }
    }
}