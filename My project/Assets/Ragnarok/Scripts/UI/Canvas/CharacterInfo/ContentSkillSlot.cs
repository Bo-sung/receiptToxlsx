using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="ContentSkill"/>
    /// </summary>
    public class ContentSkillSlot : UIInfo<ContentSkillPresenter, SkillModel.ISkillSimpleValue>, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper iconSkill;

        [SerializeField] UISprite iconSkillType;

        [SerializeField] GameObject goOutline;

        [SerializeField] GameObject goSkillLevel;
        [SerializeField] UILabelHelper labelSkillLevel;

        [SerializeField] UIButtonHelper btnSelf;

        [SerializeField] GameObject goBlock;

        public enum Mode
        {
            ICON = 0,   // 아이콘만
            DETAIL = 1, // 아이콘 + 스킬타입, 스킬레벨
            LOCK = 2,   // 잠김
        }

        private Mode mode;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnSelf.OnClick, OnClickedBtnSelf);
        }

        protected override void OnDestroy()
        {
            EventDelegate.Remove(btnSelf.OnClick, OnClickedBtnSelf);

            base.OnDestroy();
        }

        protected override void Refresh()
        {
            bool isInvalid = (info is null);

            bool showSkillIcon = !isInvalid;
            bool showSkillType = mode == Mode.DETAIL && !isInvalid;
            bool showSkillLevel = mode == Mode.DETAIL && !isInvalid;
            bool showSkillLock = mode == Mode.LOCK;
            bool showSkillOutline = mode == Mode.DETAIL && !isInvalid;

            // 아이콘
            iconSkill.SetActive(showSkillIcon);
            if (showSkillIcon)
                iconSkill.Set(info.GetIconName());

            // 스킬타입
            iconSkillType.gameObject.SetActive(showSkillType);
            if (showSkillType)
                iconSkillType.spriteName = info.SkillType.GetIconName();

            // 스킬레벨
            goSkillLevel.SetActive(showSkillLevel);
            if (showSkillLevel)
                labelSkillLevel.Text = LocalizeKey._4102.ToText().Replace(ReplaceKey.LEVEL, info.SkillLevel); // Lv. {LEVEL}

            goOutline.SetActive(showSkillOutline);
            goBlock.SetActive(showSkillLock);
        }

        public void SetMode(Mode mode)
        {
            this.mode = mode;
        }

        void OnClickedBtnSelf()
        {
            if (info is null || this.mode == Mode.LOCK)
                return;

            var input = new UISkillTooltip.Input(info.SkillId, info.SkillLevel);
            UI.Show<UISkillTooltip>(input);
        }
    }
}