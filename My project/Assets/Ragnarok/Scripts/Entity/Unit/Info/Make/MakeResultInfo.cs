using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class MakeResultInfo : DataInfo<RewardData>
    {        

        public MakeResultInfo(bool isSuccess)
        {
            this.IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }

        public bool IsShow { get; private set; }
        public bool IsEffect { get; private set; }

        public RewardData RewardData => data;

        public string ItemName => data.ItemName;

        public void SetShow(bool isShow, bool isEffect = true)
        {
            IsShow = isShow;
            IsEffect = isEffect;
            InvokeEvent();
        }
    }
}
