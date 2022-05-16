using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDungeonInfoPopup"/>
    /// </summary>
    public class DungeonInfoPopupPresenter : ViewPresenter
    {
        public interface IView
        {
        }

        // <!-- Models --!>

        // <!-- Repositories --!>
        DungeonInfoDataManager dungeonInfoDataRepo;

        // <!-- Event --!>

        private IView view;

        public DungeonInfoPopupPresenter(IView view)
        {
            this.view = view;
            dungeonInfoDataRepo = DungeonInfoDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 던전 안내 타이틀 반환
        /// </summary>
        public string GetTitle(int dungeonInfoId)
        {
            DungeonInfoData dungeonInfoData = dungeonInfoDataRepo.Get(dungeonInfoId);
            return dungeonInfoData.title_name_id.ToText();
        }

        /// <summary>
        /// 던전 이름 반환
        /// </summary>
        public string[] GetTitles(int dungeonInfoId)
        {
            List<string> ret = new List<string>();
            DungeonInfoData dungeonInfoData = dungeonInfoDataRepo.Get(dungeonInfoId);
            if (dungeonInfoData.name_id_1 > 0)
                ret.Add(dungeonInfoData.name_id_1.ToText());
            if (dungeonInfoData.name_id_2 > 0)
                ret.Add(dungeonInfoData.name_id_2.ToText());
            if (dungeonInfoData.name_id_3 > 0)
                ret.Add(dungeonInfoData.name_id_3.ToText());
            return ret.ToArray();
        }

        /// <summary>
        /// 던전 설명 반환
        /// </summary>
        public string[] GetDescriptions(int dungeonInfoId)
        {
            List<string> ret = new List<string>();
            DungeonInfoData dungeonInfoData = dungeonInfoDataRepo.Get(dungeonInfoId);
            ret.Add(dungeonInfoData.des_id_1.ToText());
            ret.Add(dungeonInfoData.des_id_2.ToText());
            ret.Add(dungeonInfoData.des_id_3.ToText());
            return ret.ToArray();
        }


    }
}