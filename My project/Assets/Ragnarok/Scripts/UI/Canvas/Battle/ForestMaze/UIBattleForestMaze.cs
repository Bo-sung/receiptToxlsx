using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleForestMaze : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIGrid grid;
        [SerializeField] UISprite[] gauges;
        [SerializeField] UIGrid gridLine;
        [SerializeField] Transform[] lines;
        [SerializeField] UIForestBossIcon hardBoss, easyBoss, currentBoss;

        BattleForestMazePresenter presenter;
        private int needCount, maxCount;
        private int groupId;

        protected override void OnInit()
        {
            presenter = new BattleForestMazePresenter();

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            const string YELLOW = "Ui_labyforest_gauge_emperium_1";
            const string GREEN = "Ui_labyforest_gauge_emperium_2";

            int gaugeLength = gauges.Length;
            needCount = Mathf.Min(presenter.GetNeedCount(), gaugeLength);
            maxCount = Mathf.Min(presenter.GetMaxCount(), gaugeLength);

            // Set Gauge Sprites
            for (int i = 0; i < gaugeLength; i++)
            {
                gauges[i].spriteName = i < needCount ? YELLOW : GREEN;
            }

            // Set Position (HardBoss)
            float? hardPosX = GetPosX(needCount);
            if (hardPosX.HasValue)
            {
                hardBoss.SetPosX(hardPosX.Value);
            }

            // Set Position (EasyBoss)
            float? easyPosX = GetPosX(maxCount);
            if (easyPosX.HasValue)
            {
                easyBoss.SetPosX(easyPosX.Value);
            }
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Initialize(int groupId)
        {
            this.groupId = groupId;

            hardBoss.SetLevel(presenter.GetBossMaxLevel(this.groupId));
            easyBoss.SetLevel(presenter.GetBossMinLevel(this.groupId));
        }

        public void SetCount(int count)
        {
            // Set Gauge
            for (int i = 0; i < gauges.Length; i++)
            {
                NGUITools.SetActive(gauges[i].cachedGameObject, i < count);
            }

            // SetCurrentBoss
            if (count < needCount)
            {
                currentBoss.SetActive(false);
            }
            else
            {
                currentBoss.SetActive(true);

                // SetLevel
                int level = presenter.GetCurrentBossLevel(groupId, count);
                currentBoss.SetLevel(level);

                // SetIcon
                string iconName = count < maxCount ? hardBoss.GetIconName() : easyBoss.GetIconName();
                currentBoss.SetIconName(iconName);

                // SetPosX
                float? posX = GetPosX(count);
                if (posX.HasValue)
                {
                    currentBoss.SetPosX(posX.Value);
                }
            }

            hardBoss.SetActive(count < needCount); // SetActive HardBoss
            easyBoss.SetActive(count < maxCount); // SetActive EasyBoss
        }

        /// <summary>
        /// Count 에 해당하는 WorldPosX 를 반환
        /// </summary>
        private float? GetPosX(int count)
        {
            int lineIndex = count - 1;
            if (lineIndex < 0 || lineIndex >= lines.Length)
                return null;

            return lines[lineIndex].position.x;
        }

        public override bool Find()
        {
            base.Find();

            if (grid)
            {
                gauges = grid.GetComponentsInChildren<UISprite>();
            }

            if (gridLine)
            {
                lines = gridLine.GetChildList().ToArray();
            }

            return true;
        }
    }
}