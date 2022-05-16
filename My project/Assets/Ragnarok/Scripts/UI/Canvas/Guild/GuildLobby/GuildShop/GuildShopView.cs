using UnityEngine;

namespace Ragnarok.View
{
    public class GuildShopView : UIView
    {
        [SerializeField] NPCStyle npc;
        [SerializeField] UILabelHelper labelNotice;

        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        ShopPresenter presenter;
        ShopInfo[] arrayInfo;

        protected override void Awake()
        {
            base.Awake();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            npc.SetStyle();
        }

        protected override void OnLocalize()
        {
            labelNotice.LocalKey = LocalizeKey._8062; // 길드 코인은 길드 출석,길드 퀘스트, 테이밍 미궁에서 획득 가능합니다.
        }

        public override void Show()
        {
            base.Show();

            npc.PlayTalk();

            arrayInfo = presenter.GetShopInfos(ShopModel.TAB_GUILD_SHOP);
            System.Array.Sort(arrayInfo, SortByCustom);

            wrapper.Resize(arrayInfo == null ? 0 : arrayInfo.Length);
        }

        public void Initialize(ShopPresenter presenter)
        {
            this.presenter = presenter;
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIShopInfoSlot ui = go.GetComponent<UIShopInfoSlot>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        private int SortByCustom(ShopInfo x, ShopInfo y)
        {
            int result = y.IsSortCanbuyLimit().CompareTo(x.IsSortCanbuyLimit()); // 매진된 상품 뒤로 보내기
            return result == 0 ? x.SortIndex.CompareTo(y.SortIndex) : result;
        }
    }
}