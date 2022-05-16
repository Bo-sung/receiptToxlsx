using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class SelectPropertyPopupView : UIView
    {
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelMainProperty;
        [SerializeField] UISprite spriteMainProperty;
        [SerializeField] UISelectPropertySlot inverseStrong;
        [SerializeField] UISelectPropertySlot inverseWeak;
        [SerializeField] UISelectPropertySlot directStrong;
        [SerializeField] UISelectPropertySlot directWeak;
        [SerializeField] UILabelHelper labelDetail;

        public event System.Action HideUI;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.OnClick, OnHideUI);
            EventDelegate.Add(btnConfirm.OnClick, OnHideUI);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.OnClick, OnHideUI);
            EventDelegate.Remove(btnConfirm.OnClick, OnHideUI);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._5200;// 속성 정보
            btnConfirm.LocalKey = LocalizeKey._1; // 확인

            if(GameServerConfig.IsKoreaLanguage())
            {
                labelDetail.SetActive(true);
                labelDetail.Text = BasisUrl.KoreanElementInfo.AppendText(string.Empty, useColor: true);
            }
            else
            {
                labelDetail.SetActive(false);
            }
        }

        public void ShowElementDesc(ElementType type, Dictionary<PropertyInfoType, List<ElementType>> infos)
        {
            labelMainProperty.Text = type.ToText();
            spriteMainProperty.spriteName = type.GetIconName();

            inverseStrong.InitData(infos[PropertyInfoType.INVERSE_STRONG], false);
            inverseWeak.InitData(infos[PropertyInfoType.INVERSE_WEAK], true);
            directStrong.InitData(infos[PropertyInfoType.DIRECT_STRONG], false);
            directWeak.InitData(infos[PropertyInfoType.DIRECT_WEAK], true);
        }

        private void OnHideUI()
        {
            HideUI?.Invoke();
        }
    }
}