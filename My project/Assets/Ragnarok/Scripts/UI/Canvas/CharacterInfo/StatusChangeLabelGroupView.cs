using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class StatusChangeLabelGroupView : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] StatusChangeLabelView[] statusChangeView;
        [SerializeField] UIGridHelper grid;

        public StatusChangeLabelView[] View => statusChangeView;
        BattleStatusData battleStatus;

        /// <summary>
        /// 능력치 최대치로 세팅하고 정렬.
        /// </summary>
        /// <param name="status"></param>
        public void Initialize(BattleStatusData status)
        {
            const int SHOW_CHANGED_STATUS_THRESHOLD = 10; // 백분율 0.1

            this.battleStatus = status;
            for (int i = 0; i < statusChangeView.Length; ++i)
            {
                View[i].Initialize();
                int statValue = battleStatus.GetStatusByIndex(i);
                int absStatValue = Mathf.Abs(statValue);
                bool isPercentStat = IsPercentStat(i.ToEnum<BattleStatusData.Stat>());
                bool isActive = (isPercentStat && absStatValue >= SHOW_CHANGED_STATUS_THRESHOLD) || (!isPercentStat && statValue != 0);
                View[i].SetActive(isActive); // 액티브가 먼저 와야 한다.
                if (isActive)
                {
                    View[i].SetSignType(statValue >= 0);
                    View[i].SetData(battleStatus.GetStatusNameByIndex(i), battleStatus.GetStatusByIndex(i)); // 최대 길이를 측정하기 위해 최종값 입력.
                }
            }

            Fit();
        }

        private void Fit()
        {
            // 이름 맞추기
            var uiLabelNames = from view in statusChangeView select view.Name.uiLabel;
            int maxNameWidth = GetMaxWidth(uiLabelNames);

            // 밸류 맞추기
            var uiLabelValues = from view in statusChangeView select view.Value.uiLabel;
            int maxValueWidth = GetMaxWidth(uiLabelValues); // 너비
            float valuePosX = statusChangeView[0].Name.transform.localPosition.x + maxNameWidth + 45 + maxValueWidth; // X : Name.x + MaxNameWidth + 30 + MaxValueWidth - Value[i].w
            foreach (var uiLabel in uiLabelValues)
            {
                if (!uiLabel.cachedGameObject.activeInHierarchy)
                    continue;

                Transform tf = uiLabel.cachedTransform;
                tf.localPosition = new Vector3(valuePosX, tf.localPosition.y, tf.localPosition.z);
            }

            // 기호 맞추기
            var tfgoSigns = from view in statusChangeView select new { tf = view.Sign.transform, go = view.Sign.gameObject };
            float signPosX = valuePosX + 10;
            foreach (var tfgo in tfgoSigns)
            {
                if (!tfgo.go.activeInHierarchy)
                    continue;

                tfgo.tf.localPosition = new Vector3(signPosX, tfgo.tf.localPosition.y, tfgo.tf.localPosition.z);
            }

            grid.Reposition();
        }

        private int GetMaxWidth(IEnumerable<UILabel> uiLabels)
        {
            int maxWidth = 0;
            foreach (var uiLabel in uiLabels)
            {
                if (!uiLabel.cachedGameObject.activeInHierarchy)
                    continue;

                uiLabel.Update();
                int thisWidth = uiLabel.width;
                if (maxWidth < thisWidth)
                    maxWidth = thisWidth;
            }
            return maxWidth;
        }

        private void Reposition(IEnumerable<Transform> transforms, float x)
        {
            foreach (var tf in transforms)
            {
                tf.localPosition = new Vector3(x, tf.localPosition.y, tf.localPosition.z);
            }
        }

        /// <summary>
        /// 백분율로 표기해야하는 스탯인지.
        /// </summary>
        bool IsPercentStat(BattleStatusData.Stat stat)
        {
            switch (stat)
            {
                case BattleStatusData.Stat.FLEE:
                case BattleStatusData.Stat.CRI:
                    return true;
            }
            return false;
        }
    }
}