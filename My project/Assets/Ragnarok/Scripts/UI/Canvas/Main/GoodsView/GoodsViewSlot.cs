using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGoodsView"/>
    /// 제니, 경험치 뷰 슬롯
    /// </summary>
    public class GoodsViewSlot : UIInfo<GoodsViewPresenter, GoodsViewData>, IAutoInspectorFinder
    {
        private readonly float REMAIN_TIME = 1.5f;
        private readonly float DISAPPEAR_TIME = 0.6f;
        private readonly float MOVE_SPEED = 7f;

        [SerializeField] UIPlayTween tweenPlayAlpha;
        [SerializeField] UISprite iconGoods;
        [SerializeField] UILabelHelper labelValue;

        enum State
        {
            None,
            Idle,
            Disappear,
            Release,
        };

        private Transform cachedTransform;
        private Vector3 destPos;
        private float elapsedTime;
        private State state;

        /// <summary>
        /// 뷰에서의 인덱스
        /// </summary>
        private int posIndex;


        public GoodsViewData Data => info;

        void FixedUpdate()
        {
            if (info is null)
                return;

            elapsedTime += Time.deltaTime;

            cachedTransform.localPosition = Vector3.Lerp(cachedTransform.localPosition, destPos, Time.deltaTime * MOVE_SPEED);

            switch (state)
            {
                case State.Idle:
                    // 1초 뒤 Idle -> Disappear
                    if (elapsedTime < REMAIN_TIME)
                        return;

                    if (this.posIndex > 0)
                        return;

                    tweenPlayAlpha.Play(forward: false);
                    elapsedTime = 0f;
                    state = State.Disappear;
                    break;

                case State.Disappear:
                    // 0.6초 뒤 Disappear -> Release
                    if (elapsedTime < DISAPPEAR_TIME)
                        return;

                    elapsedTime = 0f;
                    state = State.Release;
                    break;

                case State.Release:
                    state = State.None;
                    DeleteSlot();
                    return;
            }


        }

        public override void SetData(GoodsViewPresenter presenter, GoodsViewData info)
        {
            base.SetData(presenter, info);

            cachedTransform = transform;

            // 켜질때마다 TweenPlay 실행
            tweenPlayAlpha.Play();

            // 스폰 위치로 이동
            cachedTransform.transform.position = presenter.SpawnTransform.position;

            elapsedTime = 0f;
            state = State.Idle;
        }

        /// <summary>
        /// 자기 자신 제거
        /// </summary>
        void DeleteSlot()
        {
            presenter.DeleteSlot(this.posIndex);
        }

        protected override void Refresh()
        {
            iconGoods.spriteName = info.type.GetIconName();
            labelValue.Text = info.value.ToString("N0");
        }

        public void SetIndex(int index)
        {
            this.posIndex = index;
            destPos = presenter.GetTopPositionByIndex(this.posIndex);
        }
    }
}