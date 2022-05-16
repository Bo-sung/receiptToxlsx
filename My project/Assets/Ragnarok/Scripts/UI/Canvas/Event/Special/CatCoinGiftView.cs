using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class CatCoinGiftView : UIView
    {
        public interface IInput
        {
            int Id { get; }
            RewardData RewardData { get; }
        }

        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper LabelDescription;

        [SerializeField] UILabelHelper labelNotice;

        CatCoinGiftData[] datas;
        
        protected override void Awake()
        {
            base.Awake();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = BasisProjectTypeLocalizeKey.CatCoinGift.GetInt();
            LabelDescription.LocalKey = LocalizeKey._5417; // 일일 퀘스트를 완료하고 냥다래 받아가세요~

            labelNotice.LocalKey = LocalizeKey._5420; // 계정당 하루 1회만 보상을 수령할 수 있습니다.
        }

        public void SetData(CatCoinGiftData[] datas)
        {
            this.datas = datas;

            wrapper.Resize(datas.Length);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UISpecialEventInfo ui = go.GetComponent<UISpecialEventInfo>();
            ui.SetData(datas[index]);
        }
    }
}