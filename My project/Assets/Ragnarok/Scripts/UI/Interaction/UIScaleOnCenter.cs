using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIScaleOnCenter : MonoBehaviour
    {
        public Vector3 startingScale = Vector3.one;
        public Vector3 scaleIncreaseBy = Vector3.zero;
        public int childsInvolved = 2;

        float maxOffset;

        List<UIWidget[]> widgets;

        bool scrollViewIsHorizontal;

        UIScrollView mScroll;

        void Start()
        {
            mScroll = NGUITools.FindInParents<UIScrollView>(gameObject);
            if (mScroll == null)
            {
                Destroy(this);
            }

            UIGrid grid = GetComponent<UIGrid>();
            if (grid == null)
            {
                Destroy(this);
            }

            if (grid.arrangement == UIGrid.Arrangement.Horizontal)
            {
                maxOffset = grid.cellWidth * childsInvolved;
                scrollViewIsHorizontal = true;
            }
            else
            {
                maxOffset = grid.cellHeight * childsInvolved;
                scrollViewIsHorizontal = false;
            }

            Transform tran = transform;
            int childCount = tran.childCount;

            widgets = new List<UIWidget[]>(childCount);

            for (int i = 0; i < childCount; i++)
            {
                Transform child = tran.GetChild(i);
                widgets.Add(child.GetComponentsInChildren<UIWidget>());
            }
        }

        void Update()
        {
            if (mScroll.panel == null) return;

            Transform dt = mScroll.panel.cachedTransform;
            Vector3 center = dt.localPosition;

            for (int i = 0; i < widgets.Count; i++)
            {
                float distance = (scrollViewIsHorizontal) ? Mathf.Abs(center.x + widgets[i][0].transform.localPosition.x) :
                                                            Mathf.Abs(center.y + widgets[i][0].transform.localPosition.y);
                float modifierStrength = 1.0f - distance / maxOffset;

                if (modifierStrength > 0.0f)
                {
                    int depth = Mathf.RoundToInt(modifierStrength * 10.0f);
                    for (int j = 0; j < widgets[i].Length; j++)
                    {
                        widgets[i][j].depth = depth + j;
                    }
                    widgets[i][0].transform.localScale = new Vector3(startingScale.x + scaleIncreaseBy.x * modifierStrength,
                                                                       startingScale.y + scaleIncreaseBy.y * modifierStrength,
                                                                       startingScale.z + scaleIncreaseBy.z * modifierStrength);
                }
                else
                {
                    for (int j = 0; j < widgets[i].Length; j++)
                    {
                        widgets[i][j].depth = 0 + j;
                    }
                    widgets[i][0].transform.localScale = new Vector3(startingScale.x, startingScale.y, startingScale.z);
                }
            }
        }
    }
}