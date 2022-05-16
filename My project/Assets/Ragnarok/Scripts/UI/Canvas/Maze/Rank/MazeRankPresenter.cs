using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIMazeRank"/>
    /// </summary>
    public class MazeRankPresenter : ViewPresenter
    {
        private readonly RankModel rankModel;
        private readonly MazeMapDataManager mazeMapDataRepo;

        private int curMazeMapId;

        IMazeRankCanvas canvas;

        public MazeRankPresenter(IMazeRankCanvas canvas)
        {
            this.canvas = canvas;
            rankModel = Entity.player.RankModel;
            mazeMapDataRepo = MazeMapDataManager.Instance;
        }

        public override void AddEvent()
        {
            
        }

        public override void RemoveEvent()
        {
            
        }

        public RankInfo GetMyRankInfo()
        {
            return rankModel.GetMyRankInfo(RankType.MazeClear);
        }

        public RankInfo[] GetRankInfos()
        {
            return rankModel.GetRankInfos(RankType.MazeClear);
        }

        public async void Set(int mazeMapId)
        {
            curMazeMapId = mazeMapId;
            await rankModel.RequestMazeRankList(1, mazeMapId);

            canvas.Refresh();           
        }

        public string GetMazeMapName()
        {
            var data = mazeMapDataRepo.Get(curMazeMapId);
            if (data == null)
                return string.Empty;

            return data.name_id.ToText();
        }        
    }
}
