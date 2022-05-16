using UnityEngine;

namespace Ragnarok
{
    public class Damage : HUDObject
    {
        public const int PLAYER_HIT_FONT_SIZE = 54;
        public const int DEFAULT_HIT_FONT_SIZE = 42;
        private readonly Vector3 DEFAULT_VECTOR = new Vector3(0f, -200f); // 제자리에서 튀어오르는 기본 움직임 벡터
        private const float DEFAULT_DIRECTION_VECTOR_SCALAR = 100f; // 방향에 따른 기본 속도
        private const float DISTANCE_FACTOR = 10f; // 거리에 따른 추가 속도 계수

        [SerializeField] UILabel label;
        [SerializeField] UIPlayTween playTween;
        [SerializeField] TweenPosition tweenPosition;
        [SerializeField] GameObject critical, miss;

        [SerializeField, Rename(displayName = "왼쪽 좌표")]
        Vector3 leftPosition = new Vector3(-100f, -200f);

        [SerializeField, Rename(displayName = "오른쪽 좌표")]
        Vector3 rightPosition = new Vector3(100f, -200f);

        [SerializeField, Rename(displayName = "일반 플레이어")]
        Color normalPlayer = Color.red;

        [SerializeField, Rename(displayName = "일반 몬스터")]
        Color normalMonster = Color.white;

        [SerializeField, Rename(displayName = "크리티컬")]
        Color ciritcal = Color.yellow;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(playTween.onFinished, Release);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(playTween.onFinished, Release);
        }

        public void Initialize(int value, bool isShowCritical, bool isEnemy, Vector3 distance, int fontSize)
        {
            Clear();

            if (isShowCritical)
            {
                label.color = ciritcal;
                critical.SetActive(true);
            }
            else
            {
                label.color = isEnemy ? normalMonster : normalPlayer;
            }

            label.fontSize = fontSize;
            label.text = value.ToString();

            // 이동 방향, 거리 설정
            Vector3 vec = DEFAULT_VECTOR;
            Vector3 directionVec = distance.normalized;
            float directionX = directionVec.x * DEFAULT_DIRECTION_VECTOR_SCALAR; // 공격 방향에 따른 X좌표 이동거리
            float powerX = Mathf.Sqrt(distance.x * distance.x + distance.y * distance.y); // 공격 거리에 따른 X좌표 이동거리 (멀리서 공격할 수록 커짐)
            vec.x = directionX * powerX * DISTANCE_FACTOR;

            Play(vec);
        }

        public void InitializeFlee()
        {
            Clear();

            miss.SetActive(true);

            Play(DEFAULT_VECTOR);
        }

        private void Clear()
        {
            miss.SetActive(false);
            critical.SetActive(false);
            label.text = string.Empty;
        }

        private void Play(Vector3 vec)
        {
            tweenPosition.to = vec;
            playTween.Play(true);
        }
    }
}