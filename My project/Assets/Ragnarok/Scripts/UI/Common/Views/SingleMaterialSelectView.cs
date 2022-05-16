using UnityEngine;

namespace Ragnarok.View
{
    public abstract class SingleMaterialSelectView : MaterialSelectView
    {
        [SerializeField] UIAniProgressBar point;

        private int maxPoint;

        public void SetPoint(int curPoint, int maxPoint)
        {
            this.maxPoint = maxPoint;
            
            point.Set(curPoint, maxPoint);

            SetNeedPoint(maxPoint);
            UpdateActiveWarning(curPoint);
        }

        public override void UpdatePoint(int curPoint)
        {
            point.Tween(curPoint, maxPoint);
            UpdateActiveWarning(curPoint);
        }

        private void UpdateActiveWarning(int curPoint)
        {
            SetActiveWarning(curPoint > maxPoint);
        }
    }
}