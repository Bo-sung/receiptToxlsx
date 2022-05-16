using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class MakeResultPresenter : ViewPresenter
    {
        public interface IView
        {
        }

        private readonly IView view;
        private readonly MakeModel makeModel;

        public MakeResultPresenter(IView view)
        {
            this.view = view;
            makeModel = Entity.player.Make;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }
       
        public int SuccessCount => makeModel.successCount;
        public int FailCount => makeModel.failCount;

        public MakeResultInfo[] GetMakeResultInfos()
        {
            return makeModel.GetMakeResultInfos();
        }
    }
}
