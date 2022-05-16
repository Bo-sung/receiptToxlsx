using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{    
    public class CardSmeltMaterialSelectView : UIView
    {
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnSmelt;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        UICardSmeltMaterialSelectSlot.Info[] infos;

        public event System.Action OnSmelt;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Add(btnCancel.OnClick, OnClickedBtnExit);
            EventDelegate.Add(btnSmelt.OnClick, OnClickedBtnSmelt);

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Remove(btnCancel.OnClick, OnClickedBtnExit);
        }       

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._18509; // 투입 재료 선택
            labelDescription.LocalKey = LocalizeKey._18510; // 카드 제련에 사용 할 재료를 선택하세요.
            btnCancel.LocalKey = LocalizeKey._18511; // 취소
            btnSmelt.LocalKey = LocalizeKey._18512; // 제련
        }     

        public void Set(UICardSmeltMaterialSelectSlot.Info[] infos)
        {
            if (infos is null)
            {
                Hide();
                return;
            }
            this.infos = infos;
            wrapper.Resize(infos.Length);
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UICardSmeltMaterialSelectSlot slot = go.GetComponent<UICardSmeltMaterialSelectSlot>();
            slot.Set(infos[index]);        
        }        

        void OnClickedBtnExit()
        {
            Hide();
        }

        void OnClickedBtnSmelt()
        {
            Hide();
            OnSmelt?.Invoke();
        }       

        public void SetEnabledBtnSmelt(bool isEnabled)
        {
            btnSmelt.IsEnabled = isEnabled;
        }
    }
}