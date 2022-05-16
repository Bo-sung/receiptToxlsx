using UnityEngine;

namespace Ragnarok.View
{
    public abstract class LevelUpMaterialSelectView : MaterialSelectView
    {
        [SerializeField] UILevelAniProgressBar point;
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] UILabelHelper labelTotalPoint;

        protected int Level => point.Level;
        protected int MaxLevel => point.MaxLevel;
        protected int MaxPoint => point.Max;

        protected int FirstLevel { get; private set; } = -1;
        protected int FirstTotalPoint { get; private set; } = -1;

        /// <summary>
        /// 보여지는 시작 레벨
        /// </summary>
        protected int startDisplayLevel = 0;

        protected override void Awake()
        {
            base.Awake();

            point.OnUpdateLevel += OnUpdateLevel;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            point.OnUpdateLevel -= OnUpdateLevel;
        }

        protected virtual void OnUpdateLevel(int level, int cur, int max)
        {
            int displayLevel = level + startDisplayLevel;

            // 처음 레벨 세팅
            if (FirstLevel == -1)
                FirstLevel = level;

            SetNeedPoint(max);
            SetActiveWarning(cur > max);

            string text = LocalizeKey._33905.ToText() // Lv. {LEVEL}
                .Replace(ReplaceKey.LEVEL, displayLevel);

            var sb = StringBuilderPool.Get().Append(text);

            int diff = level - FirstLevel;
            if (diff > 0)
            {
                sb.Append("[c][D4585B]");

                if (level == MaxLevel)
                {
                    sb.Append(" (").Append(LocalizeKey._33906.ToText()).Append(")"); // (MAX)
                }
                else
                {
                    sb.Append(" (+").Append(diff).Append(")"); // (+1)
                }

                sb.Append("[-][/c]");
            }

            SetLevelText(sb.Release());
        }

        public void SetPoint(int curPoint, int maxPoint, int maxLevel)
        {
            FirstTotalPoint = curPoint;

            point.Set(curPoint, maxPoint, maxLevel);
            UpdateTotalPoint(0);
        }

        public void SetPoint(int curPoint, int[] maxPoints)
        {
            FirstTotalPoint = curPoint;

            point.Set(curPoint, maxPoints);
            UpdateTotalPoint(0);
        }

        public override void UpdatePoint(int curPoint)
        {
            point.Tween(curPoint);

            UpdateTotalPoint(curPoint - FirstTotalPoint);
        }

        protected void SetLevelText(string text)
        {
            labelLevel.Text = text;
        }

        protected void UpdateTotalPoint(int totalPoint)
        {
            var sb = StringBuilderPool.Get();

            if (totalPoint > 0)
                sb.Append('+');

            sb.Append(totalPoint.ToString("N0"));
            labelTotalPoint.Text = sb.Release();
        }
    }
}