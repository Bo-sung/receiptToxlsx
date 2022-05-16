using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIBattleBossHp : UICanvas
    {
        public enum MonsterType { MVP, NormalBoss, Normal }

        /// <summary>
        /// HP 50 당 line 개수
        /// </summary>
        private const int LINE_PER_HP = 50;

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        public enum Mode
        {
            Default,
            NoName,
        }

        public enum Offset
        {
            Default,
            BossMonster,
        }

        [SerializeField] UIWidget anchors;
        [SerializeField] UILineProgressBar hp;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UISprite bossTypeIcon;
        [SerializeField] UISprite elementIcon;
        [SerializeField] UISprite sizeIcon;
        [SerializeField] UIGrid iconGrid;

        [SerializeField] GameObject goBackground; // 이름 있을 때 쓰일 백그라운드
        [SerializeField] GameObject goBackground_NoName; // 이름 없을 때

        [SerializeField] Transform offset;

        private Transform hpHudTarget;

        protected override void OnInit()
        {
            SetMode(Mode.Default);
            SetOffset();
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public Transform GetHpHudTarget() => hpHudTarget;

        public void SetMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.Default:
                    goBackground.SetActive(true);
                    goBackground_NoName.SetActive(false);
                    elementIcon.enabled = true;
                    bossTypeIcon.enabled = true;
                    sizeIcon.enabled = true;
                    labelName.SetActive(true);
                    break;
                case Mode.NoName:
                    goBackground.SetActive(false);
                    goBackground_NoName.SetActive(true);
                    elementIcon.enabled = false;
                    bossTypeIcon.enabled = false;
                    sizeIcon.enabled = false;
                    labelName.SetActive(false);
                    break;
            }
        }

        public void SetHp(long cur, long max)
        {
            hp.Set(cur, max, maxLine: 1);
        }

        public void TweenHp(long cur, long max)
        {
            hp.Tween(cur, max);
        }

        public void Show(string name, ElementType elementType)
        {
            Show();

            labelName.Text = LocalizeKey._2701.ToText().Replace(ReplaceKey.NAME, name); // [729BFF]{NAME}[-]
            elementIcon.spriteName = elementType.GetIconName();
        }

        public void Show(int? level, string name, ElementType elementType, MonsterType monsterType, int monsterSize, GameObject target, int mvpRareType = 1)
        {
            this.hpHudTarget = target.transform;

            Show();

            if (level.HasValue)
            {
                labelName.Text = LocalizeKey._2700.ToText()
                    .Replace(ReplaceKey.LEVEL, level.Value)
                    .Replace(ReplaceKey.NAME, name); // [000000]Lv.{LEVEL}[-] [729BFF]{NAME}[-]                
            }
            else
            {
                labelName.Text = name;
            }

            elementIcon.spriteName = elementType.GetIconName();
            anchors.SetAnchor(target, 0, 0, 0, 0);

            if (monsterType == MonsterType.MVP)
            {
                if (mvpRareType == 1)
                {
                    bossTypeIcon.spriteName = Constants.CommonAtlas.UI_COMMON_ICON_MVP;
                }
                else if (mvpRareType == 2)
                {
                    bossTypeIcon.spriteName = Constants.CommonAtlas.UI_COMMON_ICON_MVP_2;
                }
                else if (mvpRareType == 3)
                {
                    bossTypeIcon.spriteName = Constants.CommonAtlas.UI_COMMON_ICON_MVP_3;
                }
                else
                {
                    bossTypeIcon.spriteName = Constants.CommonAtlas.UI_COMMON_ICON_MVP;
                }
            }
            else if (monsterType == MonsterType.NormalBoss)
            {
                bossTypeIcon.spriteName = Constants.CommonAtlas.UI_COMMON_ICON_BOSS4;
            }
            else
            {
                bossTypeIcon.enabled = false;
            }

            if (monsterSize == 1)
            {
                sizeIcon.spriteName = Constants.CommonAtlas.UI_COMMON_ICON_SMALL;
            }
            else if (monsterSize == 2)
            {
                sizeIcon.spriteName = Constants.CommonAtlas.UI_COMMON_ICON_MEDIUM;
            }
            else if (monsterSize >= 3)
            {
                sizeIcon.spriteName = Constants.CommonAtlas.UI_COMMON_ICON_LARGE;
            }
            else
            {
                sizeIcon.enabled = false;
            }

            iconGrid.Reposition();
        }

        public void SetOffset(Offset offset = Offset.Default)
        {
            switch (offset)
            {
                case Offset.Default:
                    this.offset.localPosition = new Vector3(0, 200, 0);
                    break;

                case Offset.BossMonster:
                    this.offset.localPosition = Vector3.zero;
                    break;
            }
        }
    }
}