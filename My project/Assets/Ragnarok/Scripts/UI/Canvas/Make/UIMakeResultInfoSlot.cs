using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIMakeResultInfoSlot : UIInfo<MakeResultInfo>
    {
        [SerializeField] GameObject prefab;
        [SerializeField] UIRewardHelper item;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] GameObject success;
        [SerializeField] GameObject failed;
        [SerializeField] GameObject failedBG;
        [SerializeField] GameObject successFx;

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            SetActivePrefab(info.IsShow);
            item.SetData(info.RewardData);
            labelName.Text = info.ItemName;
            success.SetActive(info.IsSuccess);
            failed.SetActive(!info.IsSuccess);
            failedBG.SetActive(!info.IsSuccess);
            successFx.SetActive(info.IsEffect);
        }

        void SetActivePrefab(bool isActive)
        {
            prefab.SetActive(isActive);
        }
    }
}
