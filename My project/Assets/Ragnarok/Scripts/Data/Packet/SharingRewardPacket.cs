namespace Ragnarok
{
    public sealed class SharingRewardPacket : IPacket<Response>, UICharacterShareWaiting.ISharingRewardData
    {
        private int zeny;
        private int exp;
        private int job_exp;
        private SimpleRewardItemPacket[] simpleRewards;
        //private int is_sharing;
        private int total_shared_sec; // 누적 셰어 시간
        //private int use_cid;
        //private int stage_id;
        private int battle_time; // 유저가 나를 사용한 시간
        private bool isInvenFull;

        void IInitializable<Response>.Initialize(Response response)
        {
            zeny = response.GetInt("1");
            exp = response.GetInt("2");
            job_exp = response.GetInt("3");
            simpleRewards = response.ContainsKey("4") ? response.GetPacketArray<SimpleRewardItemPacket>("4") : null;
            //is_sharing = response.GetInt("5");
            total_shared_sec = response.GetInt("6");
            //use_cid = response.GetInt("7");
            //stage_id = response.GetInt("8");
            battle_time = response.GetInt("9");
            isInvenFull = response.GetBool("10");
        }

        public void Initialize(ItemDataManager.IItemDataRepoImpl itemDataRepoImpl)
        {
            if (simpleRewards == null)
                return;

            foreach (var item in simpleRewards)
            {
                item.Initialize(itemDataRepoImpl);
            }
        }

        public int GetTotalTime()
        {
            return total_shared_sec;
        }

        public int GetZeny()
        {
            return zeny;
        }

        public int GetExp()
        {
            return exp;
        }

        public int GetJobExp()
        {
            return job_exp;
        }

        public UIRewardItem.IInput[] GetRewardItems()
        {
            return simpleRewards;
        }

        public int GetEmployerBattleTime()
        {
            return battle_time;
        }

        public int GetTotalItemWeight()
        {
            if (simpleRewards == null)
                return 0;

            int weight = 0;
            foreach (var item in simpleRewards)
            {
                weight += item.GetWeight();
            }

            return weight;
        }
    }
}