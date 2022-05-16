using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class CardSmeltMaterialView : UIView
    {
        [SerializeField] GameObject material;
        [SerializeField] UIPartsProfile partsProfile;
        [SerializeField] UILabelValue labelHaveZeny;
        [SerializeField] UILabelValue labelNeedZeny;

        [SerializeField] UILabelHelper labelNoData;     

        protected override void OnLocalize()
        {            
            labelHaveZeny.TitleKey = LocalizeKey._18501; // 보유 제니
            labelNeedZeny.TitleKey = LocalizeKey._18502; // 소모 제니
            labelNoData.LocalKey = LocalizeKey._18508; // 사용 가능한 재료가 없습니다.
        }

        public void Set(ItemInfo info)
        {
            if(info == null)
            {
                labelNoData.SetActive(true);
                material.SetActive(false);
            }
            else
            {
                labelNoData.SetActive(false);
                material.SetActive(true);
                partsProfile.SetData(info);
            }
        }

        public void SetHaveZeny(long zeny)
        {
            labelHaveZeny.Value = zeny.ToString("N0");
        }

        public void SetNeedZeny(int zeny)
        {
            labelNeedZeny.Value = zeny.ToString("N0");
        }
    }
}