using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class ExpandMenuSlot : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper icon;
        [SerializeField] GameObject goAddSlot;
        [SerializeField] UITweener tweener;

        // 부활 타이머
        [SerializeField] GameObject goTimerBase;
        [SerializeField] UISprite sprProgress;
        [SerializeField] UILabelHelper labelRemainReviveTime;

        // 체력 바
        [SerializeField] GameObject goHpBase;
        [SerializeField] UIAniProgressBar prgHpBar;

        // 조종 아이콘
        [SerializeField] GameObject goSelectBase;

        [SerializeField] GameObject goTexturePlay;

        private bool hasCharacter;
        private RelativeRemainTime remainReviveTime;
        private float maxReviveTime;

        private GameObject cachedGameObject;

        public event System.Action<int> OnSelect;

        void Update()
        {
            if (!this.hasCharacter)
                return;

            float remainTime = remainReviveTime.GetRemainTime();

            bool isDie = remainTime > 0f;
            if (goTimerBase.activeSelf != isDie)
                goTimerBase.SetActive(isDie);

            if (isDie)
            {
                float t = remainTime / maxReviveTime;
                sprProgress.fillAmount = t;
                labelRemainReviveTime.Text = remainTime.ToString("N0");
            }
        }

        void OnClick()
        {
            OnSelect?.Invoke(transform.GetSiblingIndex());
        }

        public void SetData(string spriteName)
        {
            bool isNullOrEmpty = (string.IsNullOrEmpty(spriteName));
            hasCharacter = !isNullOrEmpty;
            SetActiveAddSlot(isNullOrEmpty);

            goTimerBase.SetActive(false); // 부활 백그라운드
            labelRemainReviveTime.Text = string.Empty; // 부활 타이머
            goHpBase.SetActive(!isNullOrEmpty); // HP바
            icon.SetActive(!isNullOrEmpty); // 아이콘
            SetSelectState(false);

            maxReviveTime = BasisType.UNIT_DEATH_COOL_TIME.GetInt() * 0.001f;

            if (isNullOrEmpty)
                return;

            // 풀피, 부활상태로 초기화
            SetHpProgress(1, 1, skipAnim: true);
            SetReviveTime(0f);


            icon.Set(spriteName);
        }

        public void SetNoticeMode(bool isNoticeMode)
        {
            if (isNoticeMode)
            {
                tweener.PlayForward();
            }
            else
            {
                tweener.Sample(1f, isFinished: true);
                tweener.enabled = false;
            }
        }

        /// <summary>
        /// 부활 남은 시간 세팅
        /// </summary>
        public void SetReviveTime(float reviveTime)
        {
            remainReviveTime = reviveTime;
        }

        /// <summary>
        /// 남은 체력 % 세팅
        /// </summary>
        public void SetHpProgress(int cur, int max, bool skipAnim = false)
        {
            if (skipAnim)
            {
                prgHpBar.Set(cur, max);
            }
            else
            {
                prgHpBar.Tween(cur, max);
            }
        }

        /// <summary>
        /// 조종 아이콘 설정
        /// </summary>
        public void SetSelectState(bool isSelect)
        {
            goSelectBase.SetActive(isSelect);
            if (isSelect) goTexturePlay.SetActive(Entity.player.Quest.IsOpenContent(ContentType.ShareControl, false));
        }

        public void SetActiveGO(bool isActive)
        {
            if (cachedGameObject == null)
                cachedGameObject = gameObject;

            gameObject.SetActive(isActive);
        }

        private void SetActiveAddSlot(bool isActive)
        {
            NGUITools.SetActive(goAddSlot, isActive);
        }
    }
}