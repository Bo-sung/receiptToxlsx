using UnityEngine;

namespace Ragnarok.View.Home
{
    public class UIEventBannerView : UIView
    {
        [SerializeField] UICenterOnChild centerOnChild;
        [SerializeField] UIToggle[] dots;
        [SerializeField] UILabelHelper notiLabel;

        [SerializeField] GameObject wrapper;
        [SerializeField] GameObject prefab;

        BetterList<UIEventBannerSlot> slots;

        protected override void Awake()
        {
            base.Awake();

            slots = new BetterList<UIEventBannerSlot>();
            centerOnChild.onCenter += OnCenterEventBanner;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            centerOnChild.onCenter -= OnCenterEventBanner;
        }

        protected override void OnLocalize()
        {
        }

        void OnCenterEventBanner(GameObject go)
        {
            int index = int.Parse(go.name);
            dots[index].value = true; // Set Dot

            UIEventBannerSlot bannerData = go.GetComponent<UIEventBannerSlot>();
            notiLabel.Text = bannerData == null ? string.Empty : bannerData.GetNotice();
        }

        public void SetData(IEventBanner[] arrData)
        {
            int count = arrData == null ? 0 : arrData.Length;

            // 프리팹을 필요한만큼 추가하거나 제거.
            if (slots.size < count) // 차이만큼 추가
            {
                int addCount = count - slots.size;
                for (int i = 0; i < addCount; ++i)
                {
                    GameObject go = NGUITools.AddChild(wrapper, prefab);
                    go.name = i.ToString();
                    slots.Add(go.GetComponent<UIEventBannerSlot>());
                }
            }
            else if (slots.size > count) // 차이만큼 제거
            {
                int removeCount = slots.size - count;
                for (int i = 0; i < removeCount; ++i)
                {
                    UIEventBannerSlot slot = slots.Pop();
                    Destroy(slot.gameObject);
                }
            }

            // 모든 요소들에 데이터 할당
            for (int i = 0; i < count; ++i)
            {
                UIEventBannerSlot slot = slots[i];
                if (slot == null)
                    continue;

                slot.SetData(arrData[i]);
            }

            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].gameObject.SetActive(i < count);
            }

            notiLabel.SetActive(count > 0);
        }
    }
}