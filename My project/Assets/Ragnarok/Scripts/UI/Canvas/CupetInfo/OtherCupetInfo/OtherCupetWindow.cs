using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class OtherCupetWindow : UIView, IInspectorFinder
    {
        [Header("Cupet")]
        [SerializeField] UICupetProfile cupetProfile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelValue cupetPosition;
        [SerializeField] UIButtonHelper iconElement;

        [Header("Details")]
        [SerializeField] BasicStatusListView basicStatus;
        [SerializeField] BasicStatusListView detailStatus;
        [SerializeField] UILabelHelper labelSkillTitle;
        [SerializeField] UISkillInfo[] skillInfos;
        [SerializeField] UIButton[] btnSkills;

        private CupetEntity entity;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(iconElement.OnClick, OnClickedIconElement);

            for (int i = 0; i < btnSkills.Length; i++)
            {
                EventDelegate.Add(btnSkills[i].onClick, OnClickedBtnSkill);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(iconElement.OnClick, OnClickedIconElement);

            for (int i = 0; i < btnSkills.Length; i++)
            {
                EventDelegate.Remove(btnSkills[i].onClick, OnClickedBtnSkill);
            }
        }

        protected override void OnLocalize()
        {
            cupetPosition.TitleKey = LocalizeKey._19008; // 포지션
            labelSkillTitle.LocalKey = LocalizeKey._19036; // 보유 스킬
        }

        void OnClickedIconElement()
        {
            if (entity == null)
                return;

            UI.Show<UISelectPropertyPopup>().ShowElementView(entity.Cupet.ElementType);
        }

        void OnClickedBtnSkill()
        {
            if (entity == null)
                return;

            int index = 0;
            while (index < btnSkills.Length)
            {
                if (UIButton.current.Equals(btnSkills[index]))
                    break;

                ++index;
            }

            SkillInfo[] skills = entity.Cupet.GetValidSkillList();
            SkillInfo find = index < skills.Length ? skills[index] : null;
            if (find == null)
                return;

            UI.Show<UISkillTooltip>(new UISkillTooltip.Input(find.SkillId, find.SkillLevel));
        }

        public void Initialize(CupetEntity entity)
        {
            this.entity = entity;
            Refresh();
        }

        private void Refresh()
        {
            CupetModel cupetModel;
            int cupetLevel;
            string cupetName;
            CupetType cupetType;
            ElementType cupetElementType;
            IEnumerable<BasicStatusOptionValue> basicStatusOptions;
            IEnumerable<BasicStatusOptionValue> detailStatusOptions;
            SkillInfo[] skills;

            if (entity == null)
            {
                cupetModel = null;
                cupetLevel = 0;
                cupetName = string.Empty;
                cupetType = default;
                cupetElementType = default;
                basicStatusOptions = UnitEntity.GetDefaultBasicOptions();
                detailStatusOptions = UnitEntity.GetDefaultDetailStatusOptions();
                skills = System.Array.Empty<SkillInfo>();
            }
            else
            {
                cupetModel = entity.Cupet;
                cupetLevel = cupetModel.Level;
                cupetName = cupetModel.Name;
                cupetType = cupetModel.CupetType;
                cupetElementType = cupetModel.ElementType;
                basicStatusOptions = entity.GetBasicStatusOptions();
                detailStatusOptions = entity.GetDetailStatusOptions();
                skills = cupetModel.GetValidSkillList();
            }

            cupetProfile.SetData(cupetModel);

#if UNITY_EDITOR

            int cupetId = cupetModel == null ? 0 : cupetModel.CupetID;

            string text = LocalizeKey._19007.ToText() // Lv. {LEVEL} {NAME}
                .Replace(ReplaceKey.LEVEL, cupetLevel)
                .Replace(ReplaceKey.NAME, cupetName);

            labelName.Text = StringBuilderPool.Get()
                .Append(text) // Lv. {LEVEL} {NAME}
                .Append("(").Append(cupetId).Append(")")
                .Release();
#else

            labelName.Text = LocalizeKey._19007.ToText() // Lv. {LEVEL} {NAME}
                .Replace(ReplaceKey.LEVEL, cupetLevel)
                .Replace(ReplaceKey.NAME, cupetName);
#endif

            iconElement.SpriteName = cupetElementType.GetIconName();
            cupetPosition.Value = cupetType.ToText();
            basicStatus.SetData(basicStatusOptions);
            detailStatus.SetData(detailStatusOptions);

            for (int i = 0; i < skillInfos.Length; i++)
            {
                skillInfos[i].Show(i < skills.Length ? skills[i] : null);
            }
        }

        bool IInspectorFinder.Find()
        {
            skillInfos = GetComponentsInChildren<UISkillInfo>();

            if (skillInfos != null)
            {
                btnSkills = new UIButton[skillInfos.Length];
                for (int i = 0; i < btnSkills.Length; i++)
                {
                    btnSkills[i] = skillInfos[i].GetComponent<UIButton>();
                }
            }

            return true;
        }
    }
}