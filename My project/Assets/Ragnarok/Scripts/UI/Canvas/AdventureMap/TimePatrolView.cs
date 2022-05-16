using System;
using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UITimePatrol"/>
    /// </summary>
    public class TimePatrolView : UIView, IInspectorFinder
    {
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] UIButtonhWithGrayScale btnDownLevel;
        [SerializeField] UIButtonhWithGrayScale btnUpLevel;
        [SerializeField] TimePatrolStageSlot[] timePatrolStageSlots;
        [SerializeField] UIButtonHelper btnHelp;
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] RewardListView rewardListView;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIWidget levelView;

        private int finalLevel;
        private int lastLevel;
        private int maxLevel;
        private int curLevel;

        public event Action<int> OnDownLevel;
        public event Action<int> OnUpLevel;
        public event Action<int> OnTimePatrolEnter;

        protected override void Awake()
        {
            base.Awake();

            maxLevel = BasisType.TP_MAX_LEVEL.GetInt();
            EventDelegate.Add(btnDownLevel.OnClick, OnClickedBtnDownLevel);
            EventDelegate.Add(btnUpLevel.OnClick, OnClickedBtnUpLevel);
            EventDelegate.Add(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnDownLevel.OnClick, OnClickedBtnDownLevel);
            EventDelegate.Remove(btnUpLevel.OnClick, OnClickedBtnUpLevel);
            EventDelegate.Remove(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnLocalize()
        {
            timePatrolStageSlots[0].SetName(LocalizeKey._48243.ToText()); // A
            timePatrolStageSlots[1].SetName(LocalizeKey._48244.ToText()); // B
            timePatrolStageSlots[2].SetName(LocalizeKey._48245.ToText()); // C
            timePatrolStageSlots[3].SetName(LocalizeKey._48246.ToText()); // D
            timePatrolStageSlots[4].SetName(LocalizeKey._48247.ToText()); // E
            timePatrolStageSlots[5].SetName(LocalizeKey._48248.ToText()); // F
            labelRewardTitle.LocalKey = LocalizeKey._48263; // 타임패트롤 보상
            labelNotice.LocalKey = LocalizeKey._48266; // 타임패트롤의 Lv이 높아질 수록 아이템 드랍 확률이 높아집니다.
        }

        public void Init(int finalLevel, int lastLevel)
        {
            this.finalLevel = finalLevel;
            this.lastLevel = lastLevel;
        }

        public void Set(int level, int zone, int curCore, int needCore)
        {
            curLevel = level;

            labelLevel.Text = LocalizeKey._48238.ToText().Replace(ReplaceKey.LEVEL, curLevel); // Lv. {LEVEL}

            if (curLevel == 1)
            {
                btnDownLevel.SetMode(UIGraySprite.SpriteMode.Grayscale);
                btnDownLevel.IsEnabled = false;
                btnUpLevel.SetMode(UIGraySprite.SpriteMode.None);
                btnUpLevel.IsEnabled = true;
            }
            else if (curLevel == maxLevel || curLevel == finalLevel)
            {
                btnDownLevel.SetMode(UIGraySprite.SpriteMode.None);
                btnDownLevel.IsEnabled = true;
                btnUpLevel.SetMode(UIGraySprite.SpriteMode.Grayscale);
                btnUpLevel.IsEnabled = false;
            }
            else
            {
                btnDownLevel.SetMode(UIGraySprite.SpriteMode.None);
                btnDownLevel.IsEnabled = true;
                btnUpLevel.SetMode(UIGraySprite.SpriteMode.None);
                btnUpLevel.IsEnabled = true;
            }

            for (int i = 0; i < timePatrolStageSlots.Length; i++)
            {
                int zoneIndex = i + 1;

                if (curLevel != lastLevel)
                {
                    // 첫번째만 오픈해주고 나머지 모두 잠금
                    if (zoneIndex == 1)
                    {
                        timePatrolStageSlots[i].SetState(TimePatrolStageSlot.State.Clear);
                    }
                    else
                    {
                        timePatrolStageSlots[i].SetState(TimePatrolStageSlot.State.Clear);
                    }
                }
                else
                {

                    if (zoneIndex == zone)
                    {
                        timePatrolStageSlots[i].SetState(TimePatrolStageSlot.State.LastEnter);
                    }
                    else if (zoneIndex < zone)
                    {
                        timePatrolStageSlots[i].SetState(TimePatrolStageSlot.State.Clear);
                    }
                    else
                    {
                        timePatrolStageSlots[i].SetState(TimePatrolStageSlot.State.Clear);
                    }
                }
            }
        }

        public void SetRewardData(UIRewardListElement.IInput[] inputs)
        {
            rewardListView.SetData(inputs);
        }

        void OnClickedBtnDownLevel()
        {
            OnDownLevel?.Invoke(curLevel);
        }

        void OnClickedBtnUpLevel()
        {
            OnUpLevel?.Invoke(curLevel);
        }

        void OnClickedBtnHelp()
        {
            int dungeonInfoId = DungeonInfoType.TimePatrol.GetDungeonInfoId();
            UI.Show<UIDungeonInfoPopup>().Show(dungeonInfoId);
        }

        public UIWidget GetLevelWidget()
        {
            return levelView;
        }

        bool IInspectorFinder.Find()
        {
            timePatrolStageSlots = GetComponentsInChildren<TimePatrolStageSlot>();
            return true;
        }
    }
}